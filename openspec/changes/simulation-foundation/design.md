# Design: Simulation Foundation Open MOBA

- **Status:** Approved
- **Gate:** Design Gate approved
- **Date:** 2026-09-04
- **Related ADR:** `docs/adr/ADR-004-authoritative-simulation-foundation.md` (Proposed; Design Gate approved, acceptance pending implementation evidence + Merge Gate)

## 1. Purpose

Этот design материализует approved Intent `simulation-foundation` в минимальный authoritative simulation kernel внутри `OpenMoba.Sim`.

Цель — получить устойчивую engine-neutral semantic boundary до networking/gameplay, не проектируя заранее ECS, movement, combat, replication или Mod API.

Design сохраняет `ADR-002`: simulation остаётся plain .NET library без Godot, а `OpenMoba.Server` владеет process lifecycle.

## 2. Решения в одном экране

```text
Host / OpenMoba.Server
        |
        | Submit(command)
        | Advance()
        v
+-----------------------------+
| OpenMoba.Sim                |
|                             |
| SimulationInstance          |
|   Tick: ulong               |
|   CommandQueue: FIFO        |
|   World: internal registry  |
|   RNG: internal PCG32       |
|                             |
+-------------+---------------+
              |
              | StepResult + Snapshot
              v
     engine-neutral observer
```

Принятые design choices:

1. logical time = explicit discrete tick index;
2. один `Advance()` = ровно один logical tick;
3. foundation НЕ задаёт tick rate/Hz или wall-time duration;
4. world store — минимальный custom registry, НЕ third-party ECS;
5. `EntityId` — non-zero monotonic `ulong`, не переиспользуется внутри instance;
6. commands submit синхронно и исполняются FIFO на следующем tick;
7. simulation instance single-threaded; host сериализует доступ;
8. outcomes возвращаются immutable/read-only batch на completed tick, без event bus/subscriptions;
9. observation — explicit read-only snapshot active entity IDs в ascending order;
10. RNG — собственный deterministic PCG32 state внутри simulation instance;
11. determinism guarantee ограничена same simulation build + same .NET runtime major для supported scenario; blanket cross-version/cross-runtime guarantee отсутствует;
12. новые foundation API живут в `OpenMoba.Sim`; `OpenMoba.Contracts` не расширяется этим change;
13. performance benchmark и ECS migration остаются deferred до реального workload evidence.

## 3. Logical clock

### 3.1 Tick model

Foundation использует целочисленный logical tick:

```csharp
public readonly record struct SimulationTick(ulong Value);
```

Новый instance начинается с `Tick = 0`.

`Advance()` синхронно выполняет следующий step и после successful completion переводит simulation на `Tick + 1`.

Conceptual flow:

```text
Tick 0
  |
  | Submit A
  | Submit B
  v
Advance()
  |
  +-- process A on Tick 1
  +-- process B on Tick 1
  +-- produce Tick 1 outcomes
  v
Tick 1
```

### 3.2 Почему нет 30/60 Hz

Foundation пока не имеет movement, cooldowns, network pacing или other time-based gameplay. Выбор 30/60 Hz сейчас был бы production policy без workload evidence.

Поэтому `OpenMoba.Sim` не знает wall-time duration tick. Future gameplay/networking change может определить host pacing и conversion real duration -> logical ticks, не меняя core ordering semantics.

### 3.3 Запрещённые dependencies logical path

Deterministic path не должен опираться на:

- `DateTime.Now` / `UtcNow`;
- `Stopwatch` как источник simulation time;
- `Environment.TickCount`;
- sleep/timers;
- Godot `_Process` / `_PhysicsProcess`;
- background tick thread внутри `OpenMoba.Sim`.

Host может позже использовать wall clock для pacing, но только чтобы решить **когда вызвать** `Advance()`, а не для вычисления authoritative logical state внутри step.

## 4. Simulation instance и lifecycle

Предлагаемый host-facing API находится в `OpenMoba.Sim`:

```csharp
public sealed record SimulationConfig(ulong Seed);

public sealed class SimulationInstance
{
    public SimulationTick Tick { get; }

    public CommandSequence Submit(SimulationCommand command);
    public SimulationStepResult Advance();
    public SimulationSnapshot CaptureSnapshot();
}
```

Названия могут быть механически уточнены implementation agent только если semantics не меняются; изменение boundaries требует Design review.

Instance не запускает background loop и не завершает process. Он не обязан реализовывать `IDisposable`, пока не владеет disposable resources.

Host «останавливает» simulation, прекращая advancement и освобождая ссылку на instance. Отдельная match lifecycle state machine (`Running/Paused/Finished`) сейчас не создаётся.

## 5. Threading model

`SimulationInstance` **не thread-safe** и исполняется single-threaded.

Это часть design, а не временная случайность:

- ordering commands должен быть однозначным;
- internal parallelism сейчас не нужен;
- networking adapter позже обязан сериализовать concurrent external input перед `Submit`;
- jobs/multithreading вводятся только после profiling evidence и отдельного determinism review.

Concurrent calls к одному instance являются unsupported usage, а не source ordering semantics.

## 6. World/entity model

### 6.1 Не выбираем ECS сейчас

Foundation не имеет components, queries или high-volume systems. Поэтому third-party ECS сейчас решал бы отсутствующую проблему и одновременно навязал бы:

- entity lifetime semantics;
- storage/archetype semantics;
- component mutation API;
- iteration/order behavior;
- additional dependency surface.

Рассмотрены как реальные будущие варианты:

- Arch — archetype/chunk C# ECS, .NET 8-compatible;
- Friflo.Engine.ECS — managed C# ECS с command buffers, queries и active development.

Оба подтверждают, что при появлении component-heavy workload у проекта есть зрелые варианты. Но выбирать между ними до movement/combat benchmark преждевременно.

References:

- https://github.com/genaray/Arch
- https://github.com/friflo/Friflo.Engine.ECS

### 6.2 Minimal registry

Foundation world хранит только факт существования entity.

Предпочтительная internal representation:

```text
_nextEntityId: ulong
_alive slots: deterministic index-based storage
```

`EntityId`:

```csharp
public readonly record struct EntityId(ulong Value);
```

Rules:

- `0` reserved как invalid/default;
- первый created ID = `1`;
- IDs растут монотонно;
- удалённый ID никогда не reuse внутри instance;
- overflow считается unreachable invariant failure и не проектируется как normal gameplay error.

Internal store может использовать index-based list/array с tombstone `alive` flag. Public contract не зависит от storage.

### 6.3 Почему no-reuse

Generational/recycled IDs полезны для bounded slot storage, но добавляют stale-reference semantics раньше необходимости.

64-bit monotonic ID:

- проще review/test;
- deterministic;
- исключает ABA/reuse ambiguity;
- удобен будущему networking adapter как stable authoritative identity;
- позволяет позже заменить internal storage без изменения identity semantics.

Если profiling покажет необходимость recycling, это отдельное compatibility decision.

## 7. Commands

### 7.1 Foundation command surface

Для доказательства generic lifecycle вводятся только два concrete foundation commands:

```csharp
public abstract record SimulationCommand;

public sealed record CreateEntityCommand : SimulationCommand;
public sealed record DestroyEntityCommand(EntityId EntityId) : SimulationCommand;
```

Это **не** gameplay command API и не Mod API. `Move`, `Attack`, `Cast`, player/session identifiers и network envelopes отсутствуют.

### 7.2 Submission ordering

`Submit()`:

- валидирует только command object-level preconditions (например, null невозможен/запрещён);
- присваивает monotonic `CommandSequence`;
- добавляет command в FIFO pending queue;
- НЕ мутирует world немедленно.

```csharp
public readonly record struct CommandSequence(ulong Value);
```

На `Advance()` simulation snapshot'ит текущую pending queue, increment tick и обрабатывает commands по `CommandSequence` ascending/FIFO.

Commands, submitted после завершения `Advance()`, относятся к следующему tick. Reentrant submission во время processing не поддерживается.

### 7.3 Invalid command

Expected invalid authoritative intent не бросает exception и не делает partial mutation.

Foundation example:

```text
DestroyEntityCommand(unknown/dead id)
        -> CommandRejectedOutcome(EntityNotFound)
```

Exceptions reserved для programming/invariant failures, а не normal rejected intent.

## 8. Outcomes/events

### 8.1 Step-scoped batch вместо event bus

`Advance()` возвращает результат completed tick:

```csharp
public sealed record SimulationStepResult(
    SimulationTick Tick,
    IReadOnlyList<SimulationOutcome> Outcomes);
```

Foundation outcomes:

```csharp
EntityCreatedOutcome
EntityDestroyedOutcome
CommandRejectedOutcome
```

Каждый outcome содержит минимум:

- completed `SimulationTick`;
- originating `CommandSequence`;
- outcome-specific data (`EntityId`, rejection reason).

Outcomes ordered по command processing order.

Implementation возвращает genuinely read-only collection (например, BCL read-only wrapper), не raw mutable internal array/list.

### 8.2 Почему не callbacks/event bus

Subscriptions сейчас добавили бы lifetime, reentrancy, exception propagation и ordering semantics, которые не нужны foundation.

Step result batch:

- легко тестировать;
- естественно подходит server loop;
- не создаёт privileged gameplay bus;
- позже может быть input для replication/event adapters.

## 9. Observation snapshot

Host получает explicit observation:

```csharp
public sealed record SimulationSnapshot(
    SimulationTick Tick,
    IReadOnlyList<EntityId> ActiveEntities);
```

Rules:

- snapshot не предоставляет mutable internals;
- entity list ascending по `EntityId`;
- snapshot отражает state completed tick;
- snapshot не является network serialization/replication schema;
- никакой JSON/binary format этим change не фиксируется.

## 10. RNG

### 10.1 Не использовать `System.Random`

Microsoft прямо указывает, что implementation `System.Random` не гарантируется неизменной между major .NET versions, поэтому одинаковый seed не является достаточным long-term algorithm contract.

Reference:
https://learn.microsoft.com/en-us/dotnet/api/system.random

### 10.2 PCG32

Внутри `OpenMoba.Sim` реализуется небольшой deterministic PCG32 (XSH-RR, 64-bit state / 32-bit output) по reference algorithm.

Причины:

- маленькая implementation surface;
- явное integer state;
- repeatable sequence для одинакового seed/state;
- официальный reference и testable vectors;
- не требует third-party runtime package.

Reference:
https://www.pcg-random.org/using-pcg-c-basic.html

RNG остается `internal` foundation service. Host не получает API «вытащить случайное число», а future gameplay/Mod API не наследует этот internal interface автоматически.

`SimulationConfig.Seed` инициализирует instance RNG; stream constant фиксируется implementation/version contract как `Pcg32.FoundationStream = 54` (совпадает с PCG basic demo `initseq`), поэтому `Seed = 42` pins public reference vector. Unit tests pin reference vectors.

### 10.3 RNG usage сейчас

Foundation lifecycle commands не обязаны искусственно потреблять randomness. RNG verification делается отдельно на internal deterministic service + ownership/configuration tests.

Мы не добавляем fake random entity property только ради демонстрации seed.

## 11. Determinism boundary

### 11.1 Что гарантируется

Для **одного build/version `OpenMoba.Sim` и одного major .NET runtime boundary** одинаковые:

- `SimulationConfig`;
- seed;
- ordered command sequence;
- count/order `Advance()`;

должны давать одинаковые foundation-level:

- tick values;
- entity IDs/liveness;
- ordered outcomes;
- snapshot ordering;
- PCG32 sequence.

Canonical verification сравнивает structured values напрямую. Stable network/persistence serialization или hash format не вводится.

### 11.2 Что не гарантируется

Этот design НЕ обещает:

- compatibility deterministic traces между разными Open MOBA versions;
- rollback/lockstep suitability;
- bitwise float determinism будущих movement/physics systems;
- deterministic multithreading;
- replay/save compatibility;
- blanket same bits на любых runtime/CPU/OS без отдельного CI contract.

Server-authoritative networking не требует lockstep клиента и сервера, поэтому более сильная гарантия сейчас не оправдана.

## 12. Dependency / project impact

### `OpenMoba.Sim`

Получает всю новую foundation API/implementation.

Expected logical groups (точные filenames implementation detail):

```text
Clock/
World/
Commands/
Outcomes/
Random/
SimulationInstance.cs
SimulationConfig.cs
```

### `OpenMoba.Contracts`

**Не меняется.**

Сейчас нет consumer, которому эти simulation-host contracts нужны одновременно на client и server. Godot client по-прежнему не должен reference `OpenMoba.Sim`.

Networking change позже определит минимальные shared/replication contracts вместо преждевременного переноса simulation internals в shared assembly.

### `OpenMoba.Server`

Existing `--smoke` должен перестать ограничиваться assembly metadata proof и реально:

1. создать `SimulationInstance` с fixed test seed;
2. submit `CreateEntityCommand`;
3. выполнить один `Advance()`;
4. проверить Tick 1 + create outcome/snapshot;
5. вывести existing machine-readable success marker;
6. завершиться без socket/Godot/background timer.

`--smoke` остаётся internal development verification, не production CLI API.

### Godot client

Не меняется.

## 13. Verification strategy

### Unit / behavior tests

`OpenMoba.Sim.Tests` должен покрыть минимум:

1. new instance starts at tick 0;
2. N explicit advances -> tick N без sleep;
3. submit command does not mutate before advance;
4. command applies on next tick;
5. FIFO processing for several commands in one tick;
6. IDs are non-zero, unique and not reused after destroy;
7. invalid destroy produces rejection and no partial mutation;
8. step outcomes ordered and attached to correct tick;
9. snapshot is read-only and sorted ascending;
10. same canonical scenario twice -> equal structured result;
11. PCG32 reference vector;
12. same seed -> same RNG sequence;
13. different seed -> different test sequence.

### Architecture verification

Existing architecture tests remain mandatory:

- Sim has no Godot dependency;
- Client has no Sim dependency;
- Server references Sim.

No new architecture-test framework dependency.

### Server integration smoke

Existing CI `.NET Core` server smoke становится real composition proof новой foundation.

### CI

Новый workflow не нужен. Existing commands должны продолжить быть canonical:

```bash
openspec validate --all --strict --no-interactive
openspec validate --archived --no-interactive

dotnet restore OpenMoba.sln
dotnet build OpenMoba.sln --configuration Release
dotnet build src/OpenMoba.Sim/OpenMoba.Sim.csproj --configuration Release
dotnet test OpenMoba.sln --configuration Release
dotnet run --project src/OpenMoba.Server --configuration Release -- --smoke
```

Godot Headless Smoke остаётся regression check, хотя client code change не ожидается.

## 14. Performance strategy

Benchmark в этом change **не вводится**.

Причина: world пока хранит только liveness, нет representative queries/movement/combat workload. Benchmark entity creation alone может подтолкнуть architecture к оптимизации нерепрезентативного случая.

Trigger для ECS/performance design:

- movement/combat vertical создаёт component/query workload;
- profiler показывает cost world storage/query;
- custom store начинает требовать generic component/archetype machinery;
- target headless simulation workload не выполняется.

Тогда отдельный change сравнивает custom store, Arch, Friflo или другой вариант на реальном benchmark scenario.

## 15. Alternatives considered

### Variable delta-time simulation

Rejected: связывает authoritative behavior с host timing и усложняет deterministic headless tests.

### Fixed 30/60 Hz сейчас

Deferred: tick ordering нужен сейчас, real-time mapping — нет.

### Adopt Arch ECS сейчас

Deferred: хорошая C# archetype ECS option, но отсутствует component/query workload, который оправдывает dependency и semantics.

### Adopt Friflo.Engine.ECS сейчас

Deferred по той же причине. Active managed implementation остаётся кандидатом для будущего evidence-based comparison.

### Generational/recycled entity IDs

Rejected for foundation: сложнее stale-reference rules без bounded-ID requirement. Monotonic `ulong` проще и стабильнее.

### `System.Random`

Rejected: algorithm не гарантирован stable между major .NET versions.

### Event bus / callbacks

Rejected: ненужные subscription/reentrancy/lifetime semantics. Step result batch достаточно.

### Stable serialization/digest

Deferred: это бы преждевременно определило replication/persistence schema.

### Cross-platform lockstep determinism

Deferred: server-authoritative milestone этого не требует; stronger guarantee должна иметь отдельную numeric/runtime verification strategy.

## 16. Risks

### Public `OpenMoba.Sim` API может стать слишком ранним contract

Mitigation: surface минимальна, host-oriented и остаётся вне `OpenMoba.Contracts`/Mod API. Любое расширение gameplay semantics требует отдельного spec.

### Custom world registry позже придётся заменить ECS

Это ожидаемая возможность, а не failure. Entity identity и command/tick semantics отделены от storage, поэтому migration должна быть внутренней при сохранении observable contracts.

### Single-thread model может ограничить performance

До profiling это преимущество для correctness/determinism. Parallelism добавляется только с measurable need.

### Determinism формулировка может быть воспринята слишком широко

Specs/ADR явно ограничивают compatibility boundary и исключают rollback/replay/cross-version promises.

## 17. Deferred decisions

Этот design не решает:

- ECS/component architecture;
- systems scheduler;
- 30/60 Hz production pacing;
- numeric/fixed-point strategy для movement;
- spatial index/collision/navigation;
- networking transport/replication;
- network entity mapping;
- gameplay commands/events;
- Mod API exposure;
- snapshots/serialization для wire/replay;
- rollback/prediction;
- multithreading/jobs;
- persistence/replay compatibility.

Если execution требует один из этих decisions, agent обязан остановиться и эскалировать.

## 18. Design Gate acceptance

Design Gate **approved 2026-09-04**. Можно создавать `tasks.md` и передавать change одному execution harness.

Design approval означает согласие owner с ключевыми boundaries:

```text
explicit ticks, no Hz
custom minimal registry, no ECS yet
monotonic no-reuse EntityId
single-thread FIFO commands
step-scoped outcomes + read-only snapshot
internal PCG32
limited deterministic compatibility boundary
no new OpenMoba.Contracts/client/network/gameplay API
```
