# ADR-004: Authoritative simulation foundation

- **Status:** Proposed
- **Design Gate:** approved 2026-09-04
- **Acceptance:** pending implementation evidence + Merge Gate
- **Date:** 2026-09-04
- **Related change:** `openspec/changes/simulation-foundation/`

## Context

После Repository Bootstrap `OpenMoba.Sim` физически существует как plain .NET library, но authoritative simulation semantics ещё не определены.

Перед Networking Vertical Slice необходимо зафиксировать минимальную модель logical time, entity identity/lifecycle, command ordering, observation и reproducible RNG так, чтобы networking был adapter/consumer simulation, а не определял core semantics задним числом.

Решение должно сохранять Accepted ADR-002:

- simulation остаётся plain C#/.NET;
- Godot не участвует в authoritative state;
- process lifecycle принадлежит standalone server/host;
- networking transport остаётся deferred.

## Decision

### 1. Logical time — explicit discrete ticks

Authoritative simulation использует monotonic integer logical tick index.

- новый instance начинается с tick `0`;
- один explicit host `Advance()` продвигает ровно один tick;
- simulation не запускает background timer/thread;
- wall clock определяет только pacing host, но не authoritative time внутри step;
- foundation не фиксирует 30/60 Hz или другую real-time duration tick.

### 2. World model — minimal custom registry, без ECS dependency

Simulation Foundation не принимает Arch, Friflo или другую ECS library.

На этом этапе world обязан поддерживать только entity identity/liveness. Components, queries и systems ещё отсутствуют, поэтому ECS dependency была бы преждевременной и закрепила бы storage/mutation semantics без representative workload.

Internal storage остаётся replaceable за public boundary.

### 3. Entity identity — monotonic non-reused `ulong`

`EntityId` использует non-zero 64-bit monotonic value.

- `0` invalid/default;
- IDs уникальны внутри simulation instance;
- удалённые IDs не переиспользуются в lifetime instance;
- generational/recycled IDs deferred до доказанной необходимости.

Это устраняет reuse/ABA ambiguity и отделяет stable identity от будущей ECS/storage implementation.

### 4. Command execution — synchronous single-thread FIFO

Simulation instance выполняется single-threaded и не является thread-safe.

Host submit commands в FIFO boundary. Commands, принятые между ticks, обрабатываются на следующем `Advance()` в submission order.

Expected invalid command возвращает rejection outcome без partial authoritative mutation. Concurrent external input будущий network adapter обязан сначала сериализовать.

### 5. Outcomes — step-scoped ordered batches

`Advance()` возвращает read-only ordered batch authoritative outcomes completed tick.

Foundation не вводит global event bus, callbacks/subscriptions или public gameplay event system.

Host может отдельно запросить read-only snapshot authoritative state; snapshot не является replication/serialization schema.

### 6. RNG — simulation-owned PCG32

Deterministic RNG является state simulation instance и инициализируется explicit seed.

`System.Random` не используется как deterministic contract, потому что Microsoft не гарантирует одинаковую algorithm implementation между major .NET versions.

Foundation использует небольшой internal PCG32 implementation (XSH-RR, 64-bit state / 32-bit output) с pinned reference vectors. RNG API не становится Mod API или client contract.

References:

- Microsoft `System.Random`: https://learn.microsoft.com/en-us/dotnet/api/system.random
- PCG reference: https://www.pcg-random.org/using-pcg-c-basic.html

### 7. Determinism guarantee — deliberately limited

Для одного build/version `OpenMoba.Sim` и одной major .NET runtime compatibility boundary одинаковые config, seed, ordered commands и advancement sequence должны воспроизводить одинаковый foundation-level logical result.

Этот ADR не обещает:

- deterministic trace compatibility между engine versions;
- rollback/lockstep suitability;
- bitwise float determinism будущих gameplay systems;
- deterministic multithreading;
- replay/save compatibility;
- blanket cross-platform/cross-runtime equivalence без отдельной verification policy.

### 8. API ownership

Новые host-facing simulation foundation types остаются в `OpenMoba.Sim`.

`OpenMoba.Contracts` не расширяется, потому что client/shared consumer requirement пока отсутствует. Godot client не получает reference на `OpenMoba.Sim`.

Future networking change определит только реально необходимые shared/replication contracts.

## Rationale

### Почему explicit ticks

Headless tests и server-authoritative logic требуют ordering, независимого от renderer/wall-clock jitter. Integer ticks дают простой reproducible timeline и не заставляют сейчас выбирать production frequency.

### Почему custom registry вместо ECS

Текущий workload — entity lifecycle, а не component iteration. Современные C# ECS варианты существуют и совместимы с выбранным runtime direction, но их преимущества можно оценить только на representative gameplay workload.

На Design date рассмотрены:

- Arch: https://github.com/genaray/Arch
- Friflo.Engine.ECS: https://github.com/friflo/Friflo.Engine.ECS

Откладывание ECS не запрещает ECS architecture позже; наоборот, будущий выбор должен быть benchmark-driven и не ломать EntityId/tick/command contracts.

### Почему monotonic IDs

64-bit address space достаточен для match lifetime, а no-reuse устраняет отдельную категорию stale-reference bugs и упрощает будущую replication mapping.

### Почему single-thread

Ordering и verification важнее hypothetical parallel performance. Internal parallelism создаёт scheduling/determinism cost и не нужен до profiling evidence.

### Почему step batches вместо event bus

Server loop естественно получает результат каждого advancement. Это проще проверять, не создаёт reentrancy/subscription semantics и не превращается случайно в privileged gameplay API.

### Почему PCG32 вместо `System.Random`

Нужна pinned reproducible sequence, не зависящая от undocumented runtime algorithm changes. PCG32 имеет маленькую explicit integer-state implementation и опубликованный reference behavior.

## Alternatives

### Variable delta-time simulation

Rejected: authoritative result начинает зависеть от host timing/delta policy и усложняет deterministic headless verification.

### Fixed 30/60 Hz foundation

Deferred: production tick frequency не требуется до movement/network pacing requirements.

### Arch ECS сейчас

Deferred: хорошая high-performance archetype ECS option, но foundation пока не имеет queries/components, которые оправдывают adoption.

### Friflo.Engine.ECS сейчас

Deferred: active managed ECS option, но по той же причине преждевременно фиксировать ECS semantics.

### Generational entity IDs

Deferred: полезны при slot reuse, но сейчас добавляют generation/stale semantics без bounded storage need.

### Global/callback event bus

Rejected: добавляет ordering/lifetime/reentrancy surface без текущего consumer requirement.

### `System.Random`

Rejected для deterministic path: algorithm stability между major .NET versions не гарантируется.

### Strong cross-platform lockstep determinism

Deferred: server-authoritative roadmap этого не требует, а stronger guarantee потребует numeric policy, platform matrix и отдельного compatibility contract.

## Consequences

Положительные:

- networking получает заранее определённый authoritative timeline/entity/command boundary;
- simulation остаётся полностью headless и легко тестируется;
- минимальная dependency surface;
- ECS/storage migration остаётся возможной;
- deterministic tests не зависят от wall clock;
- reference game не получает privileged gameplay semantics.

Trade-offs:

- custom registry не оптимизирован под будущие high-volume component queries;
- single-thread execution ограничивает theoretical throughput;
- no-reuse IDs могут накапливать tombstones в простом storage;
- determinism guarantee намеренно слабее rollback/lockstep engines;
- future gameplay change всё равно должен определить systems/component architecture и numeric policy.

## Verification required before Accepted

ADR может стать `Accepted` только вместе с implementation evidence и Merge Gate, где tests доказывают:

- explicit tick advancement без wall-clock wait;
- FIFO command processing;
- unique non-reused entity IDs;
- read-only deterministic observation;
- expected invalid-command rejection без partial mutation;
- PCG32 pinned reference vector и same-seed reproducibility;
- repeated canonical simulation scenario equality;
- standalone server composition smoke;
- existing `Sim -> Godot` architecture prohibition.

## Revisit when

Создать отдельный design/ADR review, если:

- movement/combat требует generic component/query architecture;
- profiling показывает необходимость ECS или parallel execution;
- entity lifecycle volume делает no-reuse storage непрактичным;
- networking требует shared identity/serialization contract;
- rollback/replay/lockstep требует stronger determinism;
- production pacing требует фиксированного tick rate;
- Mod API должен получить controlled RNG/entity capabilities.
