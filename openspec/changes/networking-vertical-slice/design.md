# Design: Networking Vertical Slice / Milestone A

- **Status:** Proposed
- **Intent Gate:** approved 2026-09-05
- **Design Gate:** pending
- **Date:** 2026-09-05
- **Related ADR:** `docs/adr/ADR-005-networking-vertical-slice.md` (Proposed)

## 1. Purpose

Этот design материализует approved Intent для первого настоящего multiplayer vertical slice Open MOBA: два clients подключаются к standalone server, каждый управляет своей authoritative entity и оба видят authoritative положение обеих entities.

Design обязан сохранить Accepted ADR-001, ADR-002 и ADR-004:

- Godot остаётся presentation/client shell;
- `OpenMoba.Server` остаётся standalone .NET authoritative host;
- `OpenMoba.Sim` остаётся plain .NET без Godot/network transport dependency;
- authoritative world/state mutation проходит только через simulation command boundary;
- logical simulation time остаётся explicit host-controlled ticks.

## 2. Decision summary

```text
OpenMoba.Client.Godot (net8)
        |
        | uses
        v
OpenMoba.Networking (net8)
        |
        | LiteNetLib 2.1.4 / UDP
        v
================ OS network endpoint ================
        |
        v
OpenMoba.Networking (net8)
        |
        v
OpenMoba.Server (net10)
        |
        | session authority + host pacing
        v
OpenMoba.Sim (net8)
```

Основные решения:

- transport: `LiteNetLib` 2.1.4, UDP;
- networking code: отдельный `OpenMoba.Networking` project targeting `net8.0`;
- `OpenMoba.Networking` MUST NOT reference Godot или `OpenMoba.Sim`;
- protocol: explicit internal binary protocol v1 поверх LiteNetLib `NetDataWriter`/`NetDataReader`;
- connection/control/input messages: reliable ordered delivery;
- authoritative full snapshots: sequenced/latest-state delivery;
- target entity не приходит от client: server разрешает её только из `session -> EntityId` mapping;
- simulation spatial state: integer 2D coordinates, без Godot `Vector2`, physics или ECS;
- server host pacing: initial 30 logical ticks/sec для real-time slice, при этом simulation core не знает wall clock;
- client prediction/reconciliation/interpolation не входят в milestone;
- canonical automated acceptance запускает отдельный server process и два headless .NET test clients через real loopback UDP endpoint.

## 3. Transport choice

### 3.1 LiteNetLib 2.1.4

Выбирается `LiteNetLib` 2.1.4 как initial replaceable transport adapter.

Причины:

- current NuGet package target включает .NET 8 и .NET Standard 2.1;
- pure managed consumer path без отдельного native binary deployment;
- один transport может использоваться standalone .NET server и Godot C# client;
- library явно поддерживает connection management, channels и разные delivery modes;
- `PollEvents()` позволяет держать gameplay-affecting event dispatch под host/client loop control;
- поддерживает reliable ordered и sequenced/unreliable semantics, необходимые для control и latest-state traffic;
- package является implementation dependency `OpenMoba.Networking`, а не public platform contract.

Pinned package version:

```text
LiteNetLib 2.1.4
```

Upgrade transport package требует обычного dependency review и green network integration suite, но не меняет public Mod API.

### 3.2 Event threading policy

`UnsyncedEvents` MUST оставаться disabled.

Server вызывает `NetManager.PollEvents()` из server host loop. Gameplay-affecting callbacks не должны напрямую вызываться library unsynchronized thread и мутировать simulation.

Godot client вызывает networking poll из controlled client processing path; network callback не должен напрямую менять Godot scene tree из background thread.

## 4. Project/dependency boundaries

Добавляется:

```text
src/OpenMoba.Networking/OpenMoba.Networking.csproj  -> net8.0
```

Dependency direction:

```text
OpenMoba.Client.Godot
        |
        +--> OpenMoba.Contracts
        +--> OpenMoba.Networking

OpenMoba.Server
        |
        +--> OpenMoba.Sim
        +--> OpenMoba.Networking

OpenMoba.Networking
        |
        +--> LiteNetLib
        +--> OpenMoba.Contracts only if a concrete existing shared primitive is actually required
```

Forbidden:

```text
OpenMoba.Networking -> OpenMoba.Sim
OpenMoba.Networking -> Godot
OpenMoba.Sim        -> OpenMoba.Networking
OpenMoba.Sim        -> LiteNetLib
OpenMoba.Client.Godot -> OpenMoba.Sim
```

Для Milestone A предпочтительно не расширять `OpenMoba.Contracts`: protocol DTOs принадлежат `OpenMoba.Networking`, а simulation `EntityId` преобразуется server adapter в wire `ulong` и обратно только внутри authoritative host boundary.

Public C# types в `OpenMoba.Networking` являются internal-engine shared consumer surface между client/server projects, но НЕ считаются creator-facing Mod API и НЕ получают бессрочную wire compatibility guarantee.

## 5. Protocol v1

### 5.1 Compatibility boundary

Initial protocol имеет explicit version:

```text
ProtocolVersion = 1
```

Connection request data содержит короткий magic/version envelope. Server проверяет его до gameplay-active session.

Version mismatch -> connection rejection. Milestone не поддерживает negotiation нескольких protocol versions одновременно.

Protocol v1 является engine-internal milestone contract. Backward compatibility между будущими engine releases не обещается без отдельного requirement/ADR.

### 5.2 Encoding

Не добавляется отдельный MessagePack/MemoryPack/Protobuf dependency.

Codec использует bounded explicit binary layout поверх `NetDataWriter` / `NetDataReader`:

```text
connection data:
  magic
  protocolVersion

client -> server:
  MovementIntent

server -> client:
  Welcome
  WorldSnapshot
```

Каждый payload имеет explicit message kind и fixed/bounded fields. Parser обязан проверять доступную длину, enum/range values и bounded counts до mutation/large allocation.

Unknown/malformed gameplay message от client является protocol violation: session может быть disconnected, но server process MUST продолжить работу, а simulation MUST NOT быть мутирована этим message.

### 5.3 Bounded protocol rules

Milestone design задаёт маленькие hard limits:

- connection data имеет fixed small maximum;
- client gameplay payloads fixed-size;
- snapshot entity count имеет conservative hard maximum, существенно выше двух canonical entities, но bounded;
- unexpected trailing/short bytes считаются malformed;
- arbitrary strings/blobs из gameplay packet не аллоцируются без bounded length.

Exact constants становятся implementation constants и покрываются parser tests; изменение этих limits не должно превращать milestone protocol в public internet service contract.

## 6. Session and authority model

### 6.1 Session identity

Server создаёт monotonic non-zero runtime `SessionId` для accepted peer.

`SessionId`:

- существует только внутри текущего server process;
- не является account/player identity;
- не приходит от client как trusted value;
- не должен переиспользоваться в рамках bounded server instance, пока это не потребуется отдельным design.

### 6.2 Control mapping

Server владеет:

```text
SessionId -> Controlled EntityId
```

Client protocol **не содержит target `EntityId` в MovementIntent**.

Flow:

```text
MovementIntent(axisX, axisY)
        |
        v
resolve peer -> SessionId
        |
        v
resolve SessionId -> Controlled EntityId
        |
        v
server computes authoritative MoveEntityCommand
```

Это structural authority boundary: normal client protocol физически не даёт выбрать entity B из session A.

Forged direct-target/unknown payload не декодируется как approved movement intent.

### 6.3 Session activation

После transport connection:

1. server создаёт pending session;
2. на controlled simulation path создаётся positioned entity;
3. server получает resulting authoritative `EntityId`;
4. server записывает mapping;
5. client получает reliable `Welcome` с `SessionId`, `ControlledEntityId`, protocol/tick metadata;
6. session становится gameplay-active.

До шага 5 gameplay movement intent не применяется.

### 6.4 Disconnect

При disconnect server:

- удаляет latest input state session;
- удаляет session/control mapping;
- submit `DestroyEntityCommand` для controlled entity через normal simulation command path;
- не передаёт entity другой session.

Reconnect/resume не входит в milestone.

## 7. Minimal spatial simulation

### 7.1 Numeric model

Simulation получает engine-neutral integer 2D types, conceptual shape:

```csharp
Position2I(int X, int Y)
PositionDelta2I(int X, int Y)
```

Coordinate unit намеренно является generic `simulation unit`, а не meter/pixel contract.

Причины integer representation:

- не вводит Godot type dependency;
- deterministic arithmetic для текущего movement path;
- не требует float cross-platform numeric policy раньше combat/physics;
- достаточно для simple circles.

Godot adapter преобразует coordinates в presentation `Vector2` только на client side.

### 7.2 World storage

Текущий custom `EntityRegistry` остаётся; ECS не добавляется.

Minimal internal slot/world storage расширяется optional spatial state.

Foundation entity без position остаётся valid.

Read-only snapshot сохраняет existing `ActiveEntities` semantics и добавляет deterministic ordered positioned observation, например:

```text
PositionedEntitySnapshot(EntityId, Position2I)
```

Ordering остаётся ascending `EntityId`.

### 7.3 Commands

Добавляются только необходимые host-facing commands:

```text
CreatePositionedEntityCommand(initialPosition)
MoveEntityCommand(entityId, delta)
```

Они остаются внутри закрытой `SimulationCommand` hierarchy, как требует существующая API-surface protection.

`MoveEntityCommand`:

- применяется только на next `Advance()`;
- target должен существовать и иметь spatial state;
- integer overflow должен давать rejection без partial mutation;
- successful transition может вернуть `EntityMovedOutcome` для test/host evidence.

Не добавляются velocity/acceleration/collision/pathfinding/components/systems.

## 8. Movement intent semantics

`MovementIntent` описывает latest desired cardinal/diagonal input state, не world position и не delta magnitude:

```text
AxisX: -1 | 0 | 1
AxisY: -1 | 0 | 1
```

Другие values invalid.

Server хранит latest validated input state per active session. Client отправляет intent при изменении local axis state.

Для Milestone A intent передаётся `ReliableOrdered`:

- stop/start state не теряется;
- low message volume;
- simplest correctness boundary.

Переход на sequenced/unreliable input может быть сделан позже после prediction/reconciliation/input-history design.

Перед каждым simulation tick server:

1. polls network events;
2. обновляет latest validated intent states;
3. обходит active sessions в stable `SessionId` order;
4. вычисляет server-owned integer movement delta из axis state и configured step-per-tick;
5. submit `MoveEntityCommand` для controlled entity;
6. вызывает один `SimulationInstance.Advance()`.

Client не задаёт speed, delta или authoritative target.

Movement step magnitude является host/demo configuration, а не wire-authoritative client field.

## 9. Server pacing

Simulation core остаётся wall-clock independent.

Standalone server получает initial host pacing policy:

```text
30 simulation ticks / second
```

Это **initial Milestone A default**, а не бессрочный platform-wide tick-rate guarantee.

Один host pacing event -> максимум один intended simulation `Advance()` для этого period. Wall clock используется только server host для scheduling, никогда внутри `OpenMoba.Sim` semantics.

Unit simulation tests продолжают вызывать `Advance()` без sleep/wall clock.

Canonical real-network integration scenario использует production-like paced server loop, но bounded timeout относится только к test harness.

Почему 30 Hz сейчас:

- достаточно для визуально понятного simple movement;
- ограничивает packet/tick volume первого slice;
- даёт concrete real-time requirement, которого раньше foundation намеренно не имел;
- может быть изменён отдельным evidence-driven design, если combat/prediction покажут необходимость.

## 10. Replication model

### 10.1 Full snapshot first

Для двух entities используется full authoritative snapshot после каждого completed server tick.

Snapshot conceptual fields:

```text
Protocol message kind
SimulationTick / StateVersion
EntityCount
[ EntityId, X, Y ] * N positioned entities
```

Entities сериализуются ascending `EntityId`.

Snapshot создаётся только из `SimulationInstance.CaptureSnapshot()` / equivalent read-only authoritative observation.

Network layer не хранит canonical positions.

### 10.2 Delivery

Snapshots отправляются через LiteNetLib latest-state/sequenced delivery.

Причина: потерянный старый world snapshot не должен блокировать более новый; следующий full snapshot самодостаточен для Milestone A.

`Welcome`, handshake/control metadata и MovementIntent используют reliable ordered delivery.

### 10.3 Client ordering

Client хранит `LastAcceptedServerTick`.

- snapshot tick > last -> accept;
- snapshot tick <= last -> ignore as stale/duplicate.

Rollback/reconciliation отсутствуют.

### 10.4 No deltas yet

Delta compression, baseline ACK, interest management и bandwidth optimization deferred. Для двух circles full snapshot проще, диагностируемее и легче проверяется.

## 11. Godot client adapter

Godot client получает thin presentation controller поверх reusable `OpenMoba.Networking` client facade.

Responsibilities:

- connect/disconnect;
- poll network events;
- translate WASD/arrow state to `MovementIntent`;
- keep latest read-only replicated world model;
- render positioned entities primitives/circles;
- visually distinguish controlled entity от remote entity минимальным presentation-only способом;
- never reference `OpenMoba.Sim`.

Milestone не требует prediction или interpolation: client отображает latest accepted authoritative snapshot directly.

Manual demo path может запускать два Godot instances против одного local server. Automated acceptance не зависит от ручного окна/editor.

## 12. Automated verification strategy

### 12.1 Simulation tests

Добавить tests для:

- positioned create;
- move only on `Advance()`;
- non-positioned/missing target rejection;
- integer overflow rejection без partial mutation;
- deterministic position observation/order;
- existing entity identity/no-reuse tests remain green.

### 12.2 Protocol/networking unit tests

Добавить tests для:

- protocol v1 handshake encode/decode;
- exact movement payload/range validation;
- Welcome/Snapshot roundtrip;
- stale snapshot rejection;
- truncated/extra/unknown messages;
- bounded entity count;
- no target `EntityId` in movement intent contract.

### 12.3 Architecture tests

Enforce:

```text
OpenMoba.Networking target = net8.0
Networking !-> Sim
Networking !-> Godot
Sim !-> Networking
Client.Godot -> Networking
Client.Godot !-> Sim
Server -> Networking
Server -> Sim
LiteNetLib dependency owned only by Networking project
```

### 12.4 Canonical process-level integration test

Новый integration test должен:

1. launch **real `OpenMoba.Server` process**;
2. попросить server bind loopback endpoint, предпочтительно port `0`/ephemeral;
3. получить machine-readable readiness/selected port через stdout/test control output;
4. создать два headless `OpenMoba.Networking` clients;
5. connect оба через real UDP loopback sockets;
6. дождаться distinct Welcome/control entities;
7. дождаться snapshot, где оба видят две entities;
8. отправить movement intent client A;
9. доказать server-produced movement entity A на обоих clients;
10. доказать entity B не изменилась от A movement;
11. отправить forged direct-target/unknown malformed packet и доказать отсутствие authority bypass/server crash;
12. дождаться последующего valid snapshot/health evidence;
13. завершить clients/server в bounded timeout.

In-process fake transport MAY использоваться в unit tests, но не закрывает canonical integration requirement.

### 12.5 Godot smoke

Existing Godot headless smoke остаётся обязательным и расширяется настолько, чтобы доказать:

- client project с `OpenMoba.Networking` build/load проходит;
- networking presentation node может initialize headless без connection;
- Godot не получает `OpenMoba.Sim` dependency.

Два graphical Godot windows не являются CI acceptance requirement.

## 13. Failure/security boundaries

Milestone не является public-internet security system, но untrusted input boundary уже обязателен.

Rules:

- unsupported protocol version rejected before gameplay-active state;
- malformed/unknown gameplay packet не вызывает simulation command;
- target entity structurally server-resolved;
- client axes range validated;
- parser counts/sizes bounded;
- protocol violation MAY disconnect offending peer;
- disconnect одного peer не crash server и не передаёт его authority;
- library callbacks не mutate simulation from unsynchronized thread;
- exceptions одного malformed packet не должны завершать host process normal covered path.

TLS/DTLS/authentication/DoS-hardening/public exposure policy остаются separate future work.

## 14. Alternatives considered

### Godot built-in ENet multiplayer API

Rejected for this slice: слишком легко сделать Godot runtime владельцем transport/server semantics и нарушить standalone .NET server boundary. Возможна будущая adapter работа, но не как authoritative hosting foundation.

### ENet-CSharp

Deferred/rejected сейчас: функционально подходит reliable UDP, но требует native ENet binaries/runtime initialization и platform-specific binary deployment. Это увеличивает CI/export friction, тогда как Milestone A может получить нужные delivery semantics pure managed package path.

### `System.Net.Quic`

Deferred: современный transport и stable API в current .NET, но зависит от MsQuic; Linux требует `libmsquic`, macOS support имеет дополнительные installation/runtime условия. Для первого desktop Godot + standalone server slice это лишняя deployment surface.

### WebSocket / TCP

Rejected as initial game transport: очень прост для reliable messages, но единый reliable ordered byte stream создаёт head-of-line coupling для recurring world state. Он не даёт естественный latest-state unreliable/sequenced channel, который нужен даже этому slice.

### Raw UDP

Rejected: пришлось бы самостоятельно реализовывать connection lifecycle, reliability, sequencing, fragmentation/MTU policy и timeout semantics до проверки product milestone.

### MessagePack / MemoryPack / Protobuf

Deferred: protocol v1 имеет несколько маленьких fixed/bounded messages. Дополнительный serializer/schema/generator dependency пока не окупается. Explicit codec легче fuzz/boundary test и не создаёт ложный compatibility promise.

### Snapshot delta replication

Deferred: две entities делают full snapshot дешевле и проще. Delta/baseline ACK нужен только после measurable bandwidth/state-size pressure.

### Client prediction/reconciliation

Deferred: Milestone A доказывает server authority/replication, а не production input feel. Prediction потребует input history, correction semantics и отдельный design.

### Float `Vector2` inside simulation

Rejected: Godot type нарушает engine-neutral boundary; generic float pair преждевременно открывает cross-platform numeric/determinism policy. Integer coordinates достаточны для slice.

### ECS сейчас

Deferred: spatial state двух entities не создаёт representative component/query workload, оправдывающий ECS decision.

## 15. Consequences

Положительные:

- появляется настоящий real-socket multiplayer proof;
- same networking library работает server/client без Godot authority coupling;
- simulation остаётся canonical world owner;
- structural authority уменьшает attack/error surface;
- latest-state snapshots не блокируются потерянными старыми state packets;
- protocol маленький и полностью machine-testable;
- deterministic integer movement не меняет foundation RNG/numeric guarantees;
- architecture остаётся пригодной для дальнейшего combat vertical slice.

Trade-offs:

- protocol v1 hand-written и не предназначен для large schema evolution;
- reliable ordered input может позже потребовать replacement для low-latency gameplay;
- full snapshot every tick плохо масштабируется на большой world;
- no interpolation/prediction означает заметный network jitter/latency в demo;
- 30 Hz пока только initial host pacing assumption;
- integer position semantics позже могут потребовать richer fixed-point/physics model;
- LiteNetLib становится infrastructure dependency, которую придётся сопровождать или заменить при необходимости.

## 16. Design Gate acceptance

Перед implementation owner должен отдельно подтвердить:

- LiteNetLib 2.1.4 / UDP;
- `OpenMoba.Networking` project boundary;
- protocol v1 + explicit binary codec;
- structural session-owned target resolution;
- integer spatial model;
- initial 30 Hz host pacing;
- reliable ordered control/input + sequenced full snapshots;
- no prediction/interpolation/delta replication;
- real separate server process + two headless clients as canonical integration acceptance.

После Design Gate создаётся `tasks.md`. До этого production code не изменяется.
