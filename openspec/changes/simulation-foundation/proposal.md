# Proposal: simulation foundation Open MOBA

- **Status:** Proposed
- **Intent Gate:** pending
- **Date:** 2026-09-04

## Why

### Problem

Repository Bootstrap доказал foundation boundaries и дал собираемый `OpenMoba.Sim`, standalone server, Godot client shell и CLI/CI verification. Но authoritative simulation пока фактически не имеет собственной модели состояния и поведения: нет определённого logical time, world/entity representation, command/event flow, lifecycle authoritative state и воспроизводимой RNG/determinism policy.

Если перейти прямо к networking vertical slice, transport и replication начнут одновременно определять semantics самой simulation. Это создаст неправильную зависимость: network implementation будет вынуждена изобрести entity identity, порядок обработки input, temporal model и observable state вместо того, чтобы адаптироваться к уже определённой engine-neutral simulation.

Нужен отдельный foundation step, который создаёт минимальный authoritative simulation kernel и проверяет его headless до появления networking и gameplay.

### Goal

Определить и реализовать минимальную engine-neutral модель authoritative simulation, которой может управлять host process через явный headless API и которая даёт достаточно устойчивые semantics для следующих vertical slices.

Simulation Foundation должен определить только фундаментальные platform mechanics:

- logical simulation time и host-controlled advancement;
- минимальную world/entity representation;
- inbound command/order flow и outbound event flow;
- ownership и lifecycle authoritative state;
- reproducible RNG и явно ограниченные determinism guarantees;
- machine-verifiable headless simulation scenarios.

Этап не должен реализовывать gameplay, networking или creator-facing Mod API.

### Why now

Roadmap ставит `Simulation Foundation` непосредственно после завершённого `Repository Bootstrap` и до `Networking Vertical Slice`.

Это правильная dependency direction:

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

Networking должен переносить authoritative simulation state и player intent, а не определять базовую simulation model задним числом.

## What Changes

### Scope

В рамках change необходимо определить requirements и затем реализовать минимальные contracts/behavior для следующих областей.

#### 1. Logical simulation time

Simulation должна иметь logical time, которым управляет host через engine-neutral API.

Нужно определить:

- что является единицей advancement;
- как host продвигает simulation;
- как simulation time отделяется от wall clock, renderer frame rate и process scheduler;
- какие ordering guarantees существуют внутри одного advancement interval.

Proposal не выбирает fixed/variable timestep, конкретную frequency или public type names. Это Design Gate decision.

#### 2. World и entity representation

Simulation должна владеть минимальным authoritative world state и идентичностью entities.

Нужно определить минимум:

- создание и удаление entities;
- stable entity identity внутри simulation lifecycle;
- ownership mutable state самой simulation;
- безопасный read/observation path для tests и future adapters.

Proposal не выбирает ECS library, archetype/component model, storage layout или конкретный `EntityId` format. Они должны быть исследованы и утверждены в design.

#### 3. Commands / orders

Host должен иметь явный способ передавать simulation намерение изменить state без прямой мутации world извне.

Нужно определить:

- boundary между external host intent и authoritative mutation;
- когда command принимается в logical timeline;
- deterministic ordering нескольких commands;
- что происходит с invalid/unprocessable command на foundation уровне.

Этот change не определяет network messages, player sessions, gameplay orders (`Move`, `Attack`, `Cast`) или creator-facing command API.

#### 4. Events / observable outcomes

Simulation должна иметь engine-neutral способ сообщать о произошедших authoritative transitions без зависимости от Godot или network transport.

Нужно определить:

- event ordering относительно simulation advancement;
- lifecycle event data, необходимую только для проверки foundation behavior;
- способ tests/host получить outcomes без прямого доступа к mutable internals.

Event flow этого change не является final gameplay event bus, Mod API или replication protocol.

#### 5. State ownership и lifecycle

Simulation core должна однозначно владеть authoritative mutable state.

Host отвечает за создание, configuration, advancement и завершение simulation instance, но не должен обходить simulation rules прямой мутацией internal state.

Нужно определить минимальные lifecycle semantics, необходимые для in-process tests и standalone server composition.

#### 6. RNG и determinism guarantees

Simulation должна получить воспроизводимый source of randomness, состояние которого принадлежит simulation и инициализируется явно.

Design должен определить точный scope guarantee. Intent этого proposal требует как минимум, чтобы одинаковые поддерживаемые initial conditions, seed и ordered command sequence приводили к воспроизводимому logical result в пределах явно заявленной compatibility boundary.

Intent **не обещает** bit-for-bit determinism между любыми CPU, OS, runtime versions или будущими engine versions. Если такой уровень determinism потребуется позже для rollback/replay/lockstep, он должен быть отдельным requirement с отдельной verification strategy.

#### 7. Headless verification

Нужен automated simulation scenario, который без Godot, network socket и real-time waiting доказывает минимум:

- создание simulation instance;
- deterministic entity lifecycle;
- подачу commands через утверждённую boundary;
- advancement logical time;
- получение ordered outcomes/state observation;
- повторяемость результата при одинаковых input/seed;
- различимость результатов там, где controlled RNG действительно используется.

Scenario должен выполняться из CLI/test runner и быть пригоден для CI и будущих agent workflows.

### Expected outcome

После implementation fresh test host должен быть способен сделать концептуально следующее без Godot и networking:

```text
create simulation(config, seed)
        |
        v
create minimal entity/entities
        |
        v
submit engine-neutral command(s)
        |
        v
advance logical simulation
        |
        v
observe authoritative state/events
        |
        v
repeat same scenario -> same supported deterministic result
```

Точные API names, type layout и storage representation определяются на Design Gate.

### Acceptance outcome

Change считается успешно реализованным, если automated verification доказывает минимум следующее:

1. Simulation instance можно создать и продвигать полностью headless без Godot и graphical environment.
2. Advancement управляется host и не зависит от wall-clock sleep или renderer frame loop.
3. Simulation владеет минимальным world/entity state и entity lifecycle.
4. External host не обязан мутировать authoritative world напрямую для передачи intent; существует явная command boundary.
5. Commands обрабатываются в определённом и воспроизводимом порядке.
6. Authoritative transitions можно наблюдать через engine-neutral state/event boundary, достаточную для automated tests и future adapters.
7. RNG state и seed принадлежат simulation lifecycle, а не global/process random source.
8. Два запуска поддерживаемого headless scenario с одинаковыми initial conditions, seed и command sequence дают одинаковый canonical result.
9. Server host остаётся владельцем process lifecycle и способен композировать новую simulation foundation без Godot.
10. Existing architecture checks и Repository Bootstrap verification остаются green.
11. Result не вводит networking/gameplay/Mod API behavior за пределами этого change.

### Non-goals

Этот change намеренно НЕ реализует и НЕ фиксирует:

- движение units или position gameplay semantics;
- collision, physics, navigation или pathfinding;
- heroes, abilities, items, combat, teams, creeps, towers или game rules;
- network transport;
- connections, sessions, matchmaking или player authentication;
- replication protocol;
- wire serialization format;
- client prediction, reconciliation или rollback;
- replay/save-game/persistence format;
- final snapshot/replication schema;
- Godot presentation behavior;
- public creator-facing Mod API types;
- MoonSharp или `OpenMoba.ModRuntime` implementation;
- package/workshop system;
- gameplay event bus как public contract;
- production tick frequency без Design Gate evidence;
- parallel simulation, jobs system, multithreaded ECS или sharding;
- performance optimization без benchmark evidence;
- cross-platform/cross-runtime bitwise determinism как blanket guarantee.

### Constraints

Implementation и design обязаны сохранять Accepted foundation boundaries:

- `OpenMoba.Sim` остаётся plain .NET и не зависит от Godot;
- simulation не владеет process lifecycle;
- standalone `OpenMoba.Server` остаётся authoritative host process;
- Godot-specific types не входят в simulation contracts;
- network transport/replication остаются external adapters и не определяются этим change;
- reference-game policy не должна протекать в generic simulation foundation;
- creator-facing Mod API не должен появиться случайно из internal simulation convenience API;
- machine-verifiable headless path является обязательной частью architecture, а не optional test utility;
- dependencies добавляются только после design justification;
- public/stable API surface должна оставаться минимальной до появления реального consumer requirement.

## Capabilities

### New Capabilities

#### `simulation-clock`

Определяет logical time и host-controlled advancement authoritative simulation независимо от wall clock и presentation frame rate.

#### `simulation-world`

Определяет минимальную authoritative world/entity representation, entity lifecycle, state ownership и engine-neutral observation boundary.

#### `simulation-command-flow`

Определяет intake/order semantics для commands/orders и ordered observable outcomes/events без привязки к networking или gameplay-specific command types.

#### `simulation-determinism`

Определяет ownership RNG state/seed и точный поддерживаемый scope reproducibility/determinism guarantees.

### Modified Capabilities

#### `simulation-hosting`

Current capability уже требует headless hosting и host-owned lifecycle, но её requirements необходимо уточнить так, чтобы host мог явно initialize/advance/stop новую simulation model через engine-neutral API.

### Unchanged but constrained capabilities

- `foundation-runtime` — сохраняет запрет `Simulation -> Godot` и engine-specific types в core contracts;
- `dedicated-server` — server остаётся standalone host, но networking behavior не добавляется;
- `client-integration` — behavior client не меняется;
- `mod-runtime` — не реализуется этим change.

## Design questions after Intent Gate

После approval Intent Gate design должен исследовать и явно решить минимум следующие вопросы:

1. Logical advancement model: fixed-step, explicit discrete step или другой вариант; связь logical time с duration.
2. Нужен ли foundation default tick rate или frequency должна оставаться host/config concern.
3. World representation: minimal custom store, ECS architecture или конкретная ECS library.
4. Если рассматривается third-party ECS — её maturity, allocation model, query ergonomics, determinism implications, maintenance risk и agent-readability.
5. Entity identity/lifecycle semantics, включая stale references и reuse policy.
6. Где проходит mutable state boundary и какие read-only observation/snapshot primitives действительно нужны сейчас.
7. Command scheduling и deterministic ordering, включая commands, поступившие на один logical step.
8. Minimal invalid-command/failure semantics без создания gameplay validation framework.
9. Event/outcome semantics и lifetime полученных events.
10. RNG algorithm/API, seed/state ownership и способ избежать process-global randomness.
11. Exact determinism compatibility boundary: same process/runtime, same platform family или более сильная гарантия.
12. Canonical result/digest для deterministic tests без преждевременного выбора network serialization.
13. Lifecycle API simulation instance и composition с `OpenMoba.Server`.
14. Нужны ли performance baseline/benchmark уже на этом этапе или correctness verification достаточно до появления movement/network workload.
15. Какие types являются internal foundation API, какие shared host contracts действительно должны попасть в `OpenMoba.Contracts`, а какие должны остаться внутри `OpenMoba.Sim`.

## ADR impact

Новый ADR **required** до implementation.

Simulation clock semantics, world/entity representation, command ordering и determinism policy являются фундаментальными решениями, на которые будут опираться networking, gameplay и Mod API. Они не должны закрепляться только кодом или `design.md`.

Ожидается как минимум один новый Proposed ADR уровня `ADR-004` для authoritative simulation foundation. На Design phase нужно решить, достаточно ли одного ADR или world model/ECS choice следует вынести в отдельный ADR.

Accepted ADR-002 не пересматривается: C#/.NET, `OpenMoba.Sim` engine-neutral boundary и standalone server остаются действующими.

## Delegation model

До Intent Gate implementation отсутствует.

После Intent approval последовательность должна быть:

```text
proposal approved
        |
        v
delta specs for new/modified capabilities
        |
        v
design + Proposed ADR(s)
        |
        v
[Design Gate]
        |
        v
tasks
        |
        v
one execution harness / bounded branch task
        |
        v
verification + independent review
        |
        v
[Merge Gate]
```

Execution agent не получает права самостоятельно выбрать ECS, tick model, determinism level или public simulation API, если эти решения не утверждены в design/ADR.

## Impact

### Repository

Основная будущая implementation ожидается внутри:

- `src/OpenMoba.Sim/`;
- `tests/OpenMoba.Sim.Tests/`;
- возможно `src/OpenMoba.Contracts/` только для действительно shared host contracts;
- `src/OpenMoba.Server/` только для composition/smoke adaptation;
- architecture/verification docs и CI — только если новые checks требуют durable integration.

Точный file/project impact определяется design.

### Product/architecture

Это первый change, который создаёт реальное authoritative platform state behavior. Поэтому его contracts должны оставаться минимальными и generic: следующий Networking Vertical Slice должен использовать simulation foundation как consumer, а не заставлять simulation зависеть от network assumptions.

### Specs

В отличие от `bootstrap-repository`, этот change **требует behavioral delta specs**. `skip_specs: true` использовать нельзя.

После Intent Gate необходимо создать delta specs для всех новых/modified capabilities до Design Gate.