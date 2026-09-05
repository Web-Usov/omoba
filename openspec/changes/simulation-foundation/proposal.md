# Proposal: simulation foundation Open MOBA

- **Status:** Accepted
- **Intent Gate:** approved
- **Date:** 2026-09-04

## Why

### Problem

Repository Bootstrap материализовал foundation boundaries и дал собираемый `OpenMoba.Sim`, standalone server, Godot client shell и CLI/CI verification. Но authoritative simulation пока не имеет собственной модели состояния и поведения: отсутствуют определённый logical time, world/entity representation, command/event flow, ownership authoritative state и воспроизводимая RNG/determinism policy.

Если перейти сразу к networking vertical slice, transport и replication будут вынуждены одновременно определять semantics simulation: entity identity, порядок обработки input, temporal model и observable state. Это создаст неправильное направление зависимости.

### Goal

Определить и реализовать минимальную engine-neutral модель authoritative simulation, которой host process управляет через явный headless API и которая даёт устойчивые semantics для следующих vertical slices.

Simulation Foundation должен определить только фундаментальные platform mechanics:

- logical simulation time и host-controlled advancement;
- минимальную world/entity representation;
- inbound command/order flow и outbound ordered outcomes/events;
- ownership и lifecycle authoritative state;
- reproducible RNG и явно ограниченную determinism guarantee;
- machine-verifiable headless simulation scenarios.

### Why now

Roadmap ставит `Simulation Foundation` после завершённого `Repository Bootstrap` и до `Networking Vertical Slice`:

```text
Repository Bootstrap
        |
        v
Simulation Foundation
        |
        v
Networking Vertical Slice
        |
        v
Combat + Public Mod API Vertical
```

Networking должен быть consumer authoritative simulation semantics, а не источником этих semantics.

## What Changes

### Scope

Change должен определить requirements, design и implementation для следующих областей.

#### Logical simulation time

Simulation должна иметь logical time, которым управляет host через engine-neutral API. Logical advancement не зависит от wall clock, renderer frame rate или process scheduler.

Design определяет конкретную advancement model. Production tick frequency не выбирается на Intent Gate.

#### World и entity representation

Simulation должна владеть минимальным authoritative world state и идентичностью entities. Требуются создание/удаление entities, stable identity внутри lifecycle instance, ownership mutable state и engine-neutral observation path.

Intent не выбирает ECS library, component model или storage layout.

#### Commands / orders

Host должен передавать intent через явную command boundary вместо прямой мутации world. Design должен определить timing, ordering и foundation-level handling invalid commands.

Change не определяет network messages или gameplay-specific orders (`Move`, `Attack`, `Cast`).

#### Events / observable outcomes

Simulation должна сообщать об authoritative transitions через engine-neutral ordered outcomes/events, пригодные для tests и future adapters, без Godot и network transport.

Это не final gameplay event bus, Mod API или replication protocol.

#### State ownership и lifecycle

Simulation core однозначно владеет authoritative mutable state. Host отвечает за создание/configuration, advancement и lifetime simulation instance, но не обходит simulation rules прямой мутацией internals.

#### RNG и determinism

RNG state принадлежит simulation instance и инициализируется явно. Для одинаковых поддерживаемых initial conditions, seed и ordered command sequence должен существовать воспроизводимый logical result в пределах явно заявленной compatibility boundary.

Intent не обещает blanket bit-for-bit determinism между любыми CPU, OS, runtime или будущими engine versions. Более сильные требования для rollback/replay/lockstep требуют отдельного change.

#### Headless verification

Automated scenario без Godot, network socket и real-time waiting должен доказать:

- создание simulation instance;
- entity lifecycle;
- command submission через утверждённую boundary;
- advancement logical time;
- ordered outcomes/state observation;
- повторяемость результата при одинаковых inputs;
- воспроизводимость controlled RNG sequence.

### Expected outcome

После implementation test/server host должен быть способен концептуально выполнить:

```text
create simulation(config, seed)
        |
        v
submit foundation command(s)
        |
        v
advance logical simulation
        |
        v
observe authoritative state/events
        |
        v
repeat supported scenario -> same result
```

Точные API names и representation утверждаются на Design Gate.

### Acceptance outcome

Change считается успешно реализованным, когда automated verification доказывает:

1. Simulation создаётся и продвигается headless без Godot/graphical environment.
2. Advancement управляется host и не зависит от wall-clock sleep или renderer frame loop.
3. Simulation владеет минимальным world/entity state и entity lifecycle.
4. Host передаёт intent через command boundary, а не прямую mutation authoritative world.
5. Commands имеют определённый воспроизводимый ordering.
6. Authoritative transitions наблюдаемы через engine-neutral ordered state/event boundary.
7. RNG state/seed принадлежат simulation lifecycle и не используют process-global randomness.
8. Одинаковые supported initial conditions, seed и command sequence дают одинаковый canonical logical result в заявленной compatibility boundary.
9. Server остаётся владельцем process lifecycle и композирует simulation без Godot.
10. Existing architecture checks и Repository Bootstrap verification остаются green.
11. Change не вводит networking, gameplay или creator-facing Mod API behavior.

### Non-goals

Change намеренно НЕ реализует и НЕ фиксирует:

- movement/position gameplay semantics;
- collision, physics, navigation/pathfinding;
- heroes, abilities, items, combat, teams, creeps, towers или game rules;
- network transport, connections, sessions или matchmaking;
- replication/wire serialization;
- client prediction, reconciliation или rollback;
- replay/save-game/persistence format;
- final snapshot/replication schema;
- Godot presentation behavior;
- public creator-facing Mod API types;
- MoonSharp или `OpenMoba.ModRuntime` implementation;
- package/workshop system;
- final gameplay event bus;
- production tick frequency без Design Gate evidence;
- parallel simulation, jobs system, multithreaded ECS или sharding;
- performance optimization без benchmark evidence;
- blanket cross-platform/cross-runtime bitwise determinism.

### Constraints

Design и implementation обязаны сохранять Accepted foundation boundaries:

- `OpenMoba.Sim` остаётся plain .NET без Godot dependency;
- simulation не владеет process lifecycle;
- `OpenMoba.Server` остаётся standalone authoritative host;
- Godot-specific types не входят в simulation contracts;
- networking/replication остаются external adapters;
- reference-game policy не протекает в generic simulation foundation;
- creator-facing Mod API не возникает из internal convenience API;
- headless machine-verifiable path обязателен;
- dependencies добавляются только после design justification;
- stable/public surface остаётся минимальной до появления реального consumer requirement.

## Capabilities

### New Capabilities

#### `simulation-clock`

Logical time и host-controlled advancement authoritative simulation независимо от wall clock и presentation frame rate.

#### `simulation-world`

Минимальная authoritative world/entity representation, entity lifecycle, state ownership и engine-neutral observation boundary.

#### `simulation-command-flow`

Intake/order semantics commands/orders и ordered observable outcomes/events без networking/gameplay coupling.

#### `simulation-determinism`

Ownership RNG state/seed и поддерживаемый scope reproducibility/determinism guarantees.

### Modified Capabilities

#### `simulation-hosting`

Existing headless/lifecycle requirements расширяются явной host-controlled composition и advancement новой simulation model.

### Unchanged but constrained capabilities

- `foundation-runtime` — сохраняет запрет `Simulation -> Godot`;
- `dedicated-server` — server остаётся standalone host, networking behavior не добавляется;
- `client-integration` — client behavior не меняется;
- `mod-runtime` — не реализуется этим change.

## Design questions after Intent Gate

Design должен явно решить:

1. logical advancement model и представление logical time;
2. нужен ли foundation tick rate или он остаётся deferred;
3. custom world store vs ECS architecture/library;
4. entity identity, stale references и reuse policy;
5. mutable state boundary и read-only observation model;
6. command scheduling/order semantics;
7. invalid-command semantics;
8. event/outcome ordering и lifetime;
9. RNG algorithm/API и seed/state ownership;
10. exact determinism compatibility boundary;
11. canonical deterministic test result без выбора network serialization;
12. composition с `OpenMoba.Server`;
13. необходимость benchmark на этом этапе;
14. какие contracts остаются в `OpenMoba.Sim`, а какие действительно нужны в `OpenMoba.Contracts`.

## ADR impact

Новый ADR **required** до implementation.

Clock semantics, world/entity representation, command ordering и determinism policy являются фундаментальными решениями для будущих networking/gameplay/Mod API layers. Ожидается `ADR-004` для authoritative simulation foundation. Если Design потребует отдельного долгосрочного ECS/world-model choice, он должен получить отдельный ADR.

`ADR-002` не пересматривается: C#/.NET, engine-neutral `OpenMoba.Sim` и standalone server остаются действующими.

## Delegation model

После Intent approval flow:

```text
delta specs
    |
    v
design + Proposed ADR(s)
    |
    v
[Design Gate]
    |
    v
tasks -> execution harness -> verification -> independent review
    |
    v
[Merge Gate]
```

Execution agent не выбирает ECS, clock model, determinism level или public simulation API за пределами approved design/ADR.

## Impact

### Repository

Ожидаемая implementation область:

- `src/OpenMoba.Sim/`;
- `tests/OpenMoba.Sim.Tests/`;
- `src/OpenMoba.Contracts/` только если Design докажет необходимость shared host contract;
- `src/OpenMoba.Server/` только для composition/smoke adaptation;
- architecture/verification docs и CI только при необходимости durable verification.

### Product/architecture

Это первый change с реальным authoritative platform state behavior. Следующий Networking Vertical Slice должен использовать эту foundation как consumer.

### Specs

Этот change требует behavioral delta specs. `skip_specs: true` запрещён.