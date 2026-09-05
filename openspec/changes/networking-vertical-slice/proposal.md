# Proposal: Networking Vertical Slice / Milestone A

- **Status:** Proposed
- **Intent Gate:** pending
- **Date:** 2026-09-05

## Why

### Problem

`Simulation Foundation` уже задаёт authoritative logical ticks, entity identity/lifecycle, FIFO command processing, ordered outcomes, read-only observation и reproducible RNG. Но multiplayer boundary пока отсутствует полностью: standalone server не принимает network clients, Godot client не умеет подключаться к server, client intent не проходит через untrusted network boundary, а authoritative simulation state не реплицируется обратно нескольким clients.

Если следующий шаг сделать как набор несвязанных transport/client/server hacks, networking начнёт определять simulation semantics задним числом или создаст параллельный state path вне `OpenMoba.Sim`. Это нарушит принятый server-authoritative boundary и усложнит дальнейшие combat/Mod API vertical slices.

### Goal

Доказать первый настоящий server-authoritative multiplayer vertical slice — **Milestone A: Two circles over network**.

Минимальный результат:

```text
Godot Client A ---- intent ----\
                              \
                               > Standalone Server -> OpenMoba.Sim
                              /
Godot Client B ---- intent ---/

Godot Client A <--- authoritative state --- Server
Godot Client B <--- authoritative state --- Server
```

Два client instances должны подключаться к отдельному standalone server process, каждый управлять своей простой authoritative entity через network intent и видеть authoritative положение обеих entities.

Networking обязан оставаться adapter/transport layer вокруг simulation, а не владельцем gameplay state.

### Why now

Roadmap после завершённого `Simulation Foundation` ставит следующим именно `Networking Vertical Slice`:

```text
Simulation Foundation — Done
        |
        v
Networking Vertical Slice — Next
        |
        v
Combat + Public Mod API Vertical
```

Этот milestone должен доказать, что выбранные runtime boundaries работают не только в unit tests, но и как настоящий client/server system.

## What Changes

### Scope

Change должен определить requirements, design и implementation для минимального end-to-end multiplayer path.

#### Real network connection boundary

Standalone `OpenMoba.Server` должен принимать минимум два client connections через реальный network transport/OS socket path.

Intent Gate не выбирает конкретный transport, library, reliability model или wire format. Эти решения принадлежат Design Gate.

Acceptance не должна удовлетворяться in-memory fake transport между objects одного process.

#### Connection/session identity

Server должен различать подключённых clients и иметь минимальную session/control identity, достаточную для связывания конкретного client с конкретной authoritative entity.

Это не account system, authentication platform, lobby или matchmaking.

Design должен определить минимальную handshake/version compatibility boundary и lifecycle connection, необходимую для безопасного deterministic-enough test scenario.

#### Untrusted player intent

Client отправляет **intent**, а не authoritative state mutation.

Для Milestone A intent ограничен минимальным движением controlled entity. Server обязан проверить, что client имеет право воздействовать только на назначенную ему entity, и только после этого перевести intent в approved simulation transition/command path.

Client MUST NOT иметь возможность сообщить server «моя authoritative позиция теперь X/Y» как trusted fact.

Точная форма movement intent и simulation command определяется на Design Gate.

#### Minimal authoritative spatial/movement state

Чтобы увидеть два круга, authoritative simulation должна получить минимальный engine-neutral 2D spatial state и transition, достаточный для перемещения simple entities.

Это не general physics/navigation/gameplay movement framework.

Change должен сохранить `OpenMoba.Sim` владельцем authoritative position/state. Network adapter не должен хранить отдельную canonical world position.

Design должен отдельно решить:

- representation 2D position/movement;
- numeric semantics, необходимые только для этого milestone;
- relation movement advancement к logical ticks;
- нужен ли уже сейчас production/server tick rate и pacing policy.

Intent не выбирает ECS, physics engine или navigation system.

#### Server-authoritative replication

Server должен передавать clients observable authoritative state, достаточный для отображения обеих entities.

Оба clients после server processing должны наблюдать один и тот же authoritative entity identity и положение в рамках определённой convergence/ordering boundary.

Intent не выбирает snapshot-vs-delta replication, message encoding, compression или bandwidth optimization.

#### Godot client adapter

`OpenMoba.Client.Godot` должен:

- подключаться к standalone server;
- отправлять local movement intent через network boundary;
- получать authoritative state;
- отображать две простые entities как circles/primitives;
- не становиться authoritative owner state.

Никакие production assets, UI framework или editor tooling для milestone не требуются.

#### Headless / automated verification

Milestone должен иметь automated integration path, который поднимает реальный standalone server/network endpoint и минимум два test clients, затем доказывает end-to-end flow без ручного editor ritual.

Automated scenario должен проверять минимум:

1. server успешно начинает listen;
2. два clients подключаются и получают distinct control/session identity;
3. для каждого client существует/назначена authoritative entity;
4. client A отправляет movement intent;
5. server применяет authoritative transition через simulation;
6. client A и client B получают resulting authoritative state;
7. оба observers согласны по identity/position entities;
8. client не может легитимно мутировать entity другого client;
9. malformed/unsupported protocol input не приводит к direct authoritative mutation или crash server в покрытом test case;
10. scenario bounded и завершается автоматически.

Godot headless smoke должен продолжать доказывать client integration boundary. Полностью visual pixel-perfect assertion не является обязательным acceptance этого change.

### Expected outcome

После implementation должен существовать conceptual flow:

```text
start standalone server
        |
        +--> listen
        |
        +--> create authoritative simulation
        |
        v
connect client A ----> assign entity A
connect client B ----> assign entity B
        |
        v
client A input
        |
        v
network intent
        |
        v
server validates ownership/protocol
        |
        v
simulation command / authoritative tick
        |
        v
replicated authoritative state
        |
        +--> client A renders A + B
        +--> client B renders A + B
```

### Acceptance outcome

Change считается успешно поставленным, когда machine-verifiable evidence доказывает:

1. `OpenMoba.Server` остаётся standalone .NET authoritative host и реально принимает network connections без Godot runtime.
2. Минимум два clients могут одновременно подключиться к одному server instance.
3. Server различает clients и связывает control authority с отдельными authoritative entities.
4. Client input передаётся как untrusted intent и не является direct world mutation.
5. Authoritative movement state принадлежит `OpenMoba.Sim`, а не network/client adapter.
6. Client не может через normal protocol path управлять entity другого client.
7. Server authoritative state реплицируется обоим clients.
8. После movement одного client оба clients наблюдают server-produced position обеих entities.
9. Godot presentation использует replicated state и не становится authoritative source of truth.
10. Automated integration scenario использует реальный network endpoint, два clients и bounded execution.
11. Covered invalid/malformed/unsupported input обрабатывается без bypass authority и без server crash.
12. Existing Simulation Foundation determinism/architecture tests, server smoke и Godot headless smoke остаются green.
13. Change не вводит combat, heroes, abilities, items или creator-facing Mod API behavior.

### Non-goals

Change намеренно НЕ обязан реализовывать или фиксировать:

- matchmaking, lobby browser или party system;
- accounts, login, identity provider или persistent player identity;
- NAT traversal, relay, STUN/TURN, public Internet discovery;
- production deployment/cloud orchestration;
- TLS/DTLS certificate infrastructure или end-user encryption product policy;
- reconnect/resume, host migration или seamless session recovery;
- spectator mode;
- chat/voice;
- anti-cheat system сверх базового server-authority invariant;
- client prediction, reconciliation или rollback как acceptance requirement;
- lag compensation;
- replay/save/persistence;
- large-world interest management;
- relevancy/spatial culling;
- production bandwidth optimization/compression;
- general ECS/component framework;
- physics engine, collision response или rigid-body simulation;
- navigation/pathfinding;
- animation system;
- camera/gameplay UX polish;
- heroes, combat, abilities, projectiles, items, teams или MOBA rules;
- public Mod API exposure movement/networking capabilities;
- Workshop/backend/package distribution;
- Web client/export support;
- mobile networking;
- horizontal server scaling/sharding;
- production load target на сотни/тысячи concurrent players.

### Constraints

Design и implementation обязаны сохранять принятые boundaries:

- server является единственным authoritative owner multiplayer state transitions;
- `OpenMoba.Sim` не зависит от concrete network transport;
- network/client code не получает direct mutable access к simulation internals;
- Godot client не reference `OpenMoba.Sim` как способ bypass network/shared contracts;
- client input считается недоверенным;
- authoritative entity identity сохраняет semantics ADR-004;
- simulation logical advancement остаётся host-controlled;
- networking adapter не создаёт параллельный canonical world state;
- shared/wire contracts должны быть минимальны и появляться только при реальном client/server consumer requirement;
- reference/content-specific gameplay policy не должна маскироваться под generic networking infrastructure;
- automated terminal-driven verification является обязательной частью milestone;
- новое fundamental architecture decision требует Design Gate и ADR, а не implementation improvisation.

## Capabilities

### New Capabilities

#### `network-session`

Минимальный connection/session lifecycle, client identity/control assignment и protocol compatibility boundary между standalone server и clients.

#### `network-intent`

Передача untrusted player intent к authoritative server с ownership validation и запретом direct authoritative client mutation.

#### `network-replication`

Передача observable authoritative state от server к нескольким clients с определёнными ordering/convergence semantics для Milestone A.

#### `network-verification`

Machine-verifiable end-to-end scenario с real network endpoint, standalone server и минимум двумя clients.

#### `simulation-spatial-movement`

Минимальный engine-neutral authoritative 2D spatial/movement capability, необходимый для Two Circles milestone, без general physics/navigation framework.

### Modified Capabilities

#### `dedicated-server`

Server host расширяется real networking composition/listener lifecycle и связывает network adapter с authoritative simulation.

#### `client-integration`

Godot presentation adapter расширяется connection/input/replicated-state path для двух simple entities, оставаясь non-authoritative.

#### `simulation-command-flow`

Command/intent boundary расширяется минимальным authoritative movement transition, при этом network client не получает право создавать arbitrary simulation commands или напрямую мутировать world.

#### `simulation-world`

Observable authoritative state расширяется минимальным spatial state entities, не меняя identity ownership/lifecycle guarantees ADR-004.

### Unchanged but constrained capabilities

- `simulation-clock` — existing explicit logical tick semantics сохраняются; real-time pacing может быть добавлен только как host policy, не как wall-clock source authoritative time;
- `simulation-determinism` — stronger rollback/lockstep guarantee не требуется;
- `mod-runtime` — не затрагивается;
- `foundation-runtime` — `OpenMoba.Sim -> Godot` остаётся запрещённым.

## Design questions after Intent Gate

Design Gate должен явно решить как минимум:

1. network transport/library и почему он подходит standalone .NET server + Godot .NET client;
2. reliable/unreliable delivery policy для connection, intent и state updates;
3. wire serialization/encoding и protocol versioning/handshake;
4. project/dependency boundaries для server/client/shared networking code;
5. session/client/control identity и mapping к `EntityId`;
6. lifecycle connect/disconnect и entity ownership policy;
7. minimal 2D position/movement representation;
8. movement intent semantics и validation;
9. server simulation pacing/tick frequency, если real-time movement требует его сейчас;
10. queueing/order semantics network input относительно `SimulationInstance.Advance()`;
11. replication model: full snapshots, deltas или другой минимальный approach;
12. state update ordering, stale/out-of-order handling и convergence boundary;
13. нужен ли interpolation только для presentation и входит ли он в milestone;
14. malformed/oversized/unsupported message handling и bounded resource rules;
15. behavior protocol/version mismatch;
16. disconnect/failure semantics, достаточные для slice;
17. automated two-client harness и способ доказать, что используется real network transport;
18. Godot integration verification без manual-only acceptance;
19. какие contracts действительно принадлежат `OpenMoba.Contracts`, а какие остаются transport-internal;
20. нужен ли отдельный `OpenMoba.Networking` project и его dependency direction;
21. какой ADR или набор ADR фиксирует transport/replication/pacing decisions.

## ADR impact

**ADR required.**

Networking Vertical Slice впервые фиксирует concrete transport/replication/protocol boundaries и, вероятно, real-time server pacing policy. Эти decisions не покрываются ADR-001..004 и не должны быть скрыты внутри implementation.

Ожидается как минимум новый ADR для networking architecture. Design Gate определит, достаточно ли одного ADR или transport/protocol и simulation pacing требуют отдельных decisions.

Accepted ADR-001, ADR-002 и ADR-004 не пересматриваются без отдельной эскалации.

## Risks to evaluate in Design

- выбранный transport хорошо работает в Godot client, но создаёт неудобный/нестабильный standalone .NET server path;
- слишком ранняя wire schema превращается в permanent public compatibility promise;
- replication начинает владеть canonical state вместо simulation;
- network receive threads нарушают single-thread FIFO semantics simulation;
- movement вынуждает преждевременно принять ECS/physics architecture;
- fixed tick rate выбирается без связи с actual network/movement requirements;
- unreliable delivery усложняет correctness раньше необходимости;
- fully reliable transport создаёт head-of-line behavior, который позже плохо подходит action gameplay;
- test harness проверяет in-memory adapter и создаёт ложное ощущение working multiplayer;
- Godot-only networking implementation нарушает standalone server architecture;
- client-facing types случайно становятся gameplay/public Mod API contracts;
- malformed client traffic вызывает unbounded allocation, crash или authority bypass.

## Intent Gate decision requested

Owner должен подтвердить именно **problem, outcome, scope и non-goals** этого proposal.

Intent approval НЕ означает выбор:

- UDP/TCP/QUIC/WebSocket/ENet или конкретной networking library;
- protobuf/MessagePack/custom binary/JSON или другой serialization;
- snapshot/delta replication;
- tick rate;
- interpolation/prediction;
- exact movement numeric representation;
- ECS;
- exact project/API names.

Эти решения разрешены только после отдельного design research и Design Gate.
