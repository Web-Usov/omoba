# ADR-005: Networking vertical slice architecture

- **Status:** Proposed
- **Intent Gate:** approved 2026-09-05
- **Design Gate:** pending
- **Date:** 2026-09-05
- **Related change:** `openspec/changes/networking-vertical-slice/`

## Context

После Accepted ADR-004 authoritative simulation уже имеет explicit logical ticks, stable `EntityId`, FIFO command processing, read-only observation и deterministic foundation semantics. Следующий roadmap milestone должен доказать настоящий server-authoritative multiplayer: два clients подключаются к standalone server, отправляют movement intent и оба видят authoritative movement двух simple entities.

Первый networking design впервые требует concrete decisions вокруг transport, protocol, server real-time pacing, session authority, spatial state и replication. Эти decisions нельзя оставить implementation detail, потому что они задают dependency direction и failure/compatibility boundaries для последующих gameplay vertical slices.

Сохраняются invariants ADR-001/002/004:

- Godot не владеет authoritative gameplay state;
- `OpenMoba.Server` является standalone authoritative host;
- `OpenMoba.Sim` не зависит от network transport;
- client input считается untrusted intent;
- authoritative mutation проходит через simulation command boundary.

## Decision

### 1. Initial transport — LiteNetLib 2.1.4 over UDP

Для Milestone A принимается `LiteNetLib` 2.1.4 как replaceable transport implementation.

Transport выбран потому, что current package поддерживает .NET 8/.NET Standard 2.1, используется как managed NuGet dependency, предоставляет connection management и несколько delivery modes, а его `PollEvents()` model позволяет dispatch events под control host loop.

Transport implementation не является public Mod API и может быть заменена отдельным ADR/change.

### 2. Separate `OpenMoba.Networking` boundary

Добавляется plain .NET library:

```text
OpenMoba.Networking -> net8.0
```

Она содержит transport adapter, protocol codec и reusable headless client/server networking primitives.

Dependency rules:

```text
Client.Godot -> Networking
Server       -> Networking
Server       -> Sim
Networking   !-> Sim
Networking   !-> Godot
Sim          !-> Networking
Client.Godot !-> Sim
```

`LiteNetLib` dependency принадлежит `OpenMoba.Networking`.

### 3. Protocol v1 — explicit bounded binary codec

Initial protocol использует explicit version `1` и hand-written bounded binary encoding поверх LiteNetLib reader/writer helpers.

Отдельный serializer framework не добавляется.

Protocol имеет минимальный набор messages:

- connection magic/version;
- `MovementIntent` client -> server;
- `Welcome` server -> client;
- `WorldSnapshot` server -> client.

Unknown/malformed input не превращается в simulation command. Unsupported protocol version отклоняется до gameplay-active session.

Protocol v1 не получает бессрочную backward-compatibility guarantee.

### 4. Session authority is structural

Server создаёт runtime `SessionId` и владеет mapping:

```text
SessionId -> Controlled EntityId
```

`MovementIntent` не содержит authoritative target `EntityId`.

Client сообщает только desired movement axes. Server разрешает target entity из session mapping и сам создаёт approved `MoveEntityCommand`.

Таким образом normal protocol path не позволяет client A выбрать entity B.

### 5. Minimal spatial simulation uses integer coordinates

`OpenMoba.Sim` расширяется optional authoritative 2D spatial state на integer coordinates (`int X`, `int Y`) и minimal commands для positioned creation/movement.

Godot `Vector2`, physics, collision, navigation и ECS не входят в simulation boundary.

Existing non-positioned entities остаются valid.

### 6. Initial server pacing is 30 Hz host policy

Standalone server получает initial real-time pacing default 30 simulation ticks/sec.

Этот rate принадлежит host scheduling policy; `OpenMoba.Sim` по-прежнему знает только explicit logical `Advance()` и не читает wall clock.

30 Hz является Milestone A default, а не permanent platform contract. Изменение rate при появлении combat/prediction evidence требует отдельного design review, если меняются observable contracts.

### 7. Input delivery — reliable ordered for Milestone A

Handshake/control metadata и movement input state передаются reliable ordered.

Movement intent представляет latest desired axis state, а не absolute position или client-selected delta/speed.

Reliable ordered выбран для первого slice, чтобы start/stop input state не терялся и не требовал input-history/reconciliation design.

Future low-latency input strategy может перейти на sequenced/unreliable + redundancy/history отдельным change.

### 8. Replication — full authoritative snapshot via sequenced/latest-state delivery

После каждого completed server tick server формирует full snapshot всех positioned entities из read-only simulation observation.

Snapshot включает logical server tick/state version и entity identity/position в stable order.

Snapshots отправляются latest-state/sequenced delivery: потерянный старый snapshot не должен блокировать более новый full state.

Client принимает только snapshot newer than already accepted state version.

Delta replication, baseline ACK и interest management deferred.

### 9. No client prediction/reconciliation/interpolation in Milestone A

Godot client отображает latest accepted authoritative snapshot напрямую.

Prediction, reconciliation, interpolation и rollback являются отдельным networking/gameplay design после доказательства base authority/replication path.

### 10. Canonical verification uses a separate server process and real sockets

Acceptance test обязан запустить настоящий `OpenMoba.Server` process, bind loopback UDP endpoint и подключить два headless clients через `OpenMoba.Networking`.

Test доказывает:

- two distinct sessions/control entities;
- end-to-end movement;
- both clients observe same server-produced state;
- forged direct-target/invalid message не даёт authority bypass;
- server продолжает bounded execution;
- automatic shutdown.

In-memory transport test не заменяет этот acceptance.

## Rationale

### Почему LiteNetLib

Milestone требует reliable connection/control traffic и latest-state traffic, но не должен заставлять проект самостоятельно реализовывать reliable UDP. LiteNetLib предоставляет необходимые delivery semantics и managed desktop path, сохраняя один networking implementation для standalone server и Godot C# client.

References:

- https://www.nuget.org/packages/LiteNetLib/2.1.4
- https://github.com/RevenantX/LiteNetLib

### Почему отдельный Networking project

Transport/protocol должны быть reusable между server и Godot client и одновременно не протекать в simulation. Отдельная net8 library делает dependency boundary machine-verifiable.

### Почему custom binary v1

Сейчас сообщений мало и они fixed/bounded. Serializer framework добавил бы dependency/schema policy раньше, чем появился representative protocol surface. Explicit codec легче проверять на malformed/truncated/oversized input.

### Почему client не передаёт target entity

Validation client-provided target была бы weaker boundary: protocol сначала позволял бы выразить unauthorized target, затем надеялся на validation. Server-owned resolution делает unauthorized target невозможным в approved movement message shape.

### Почему integer position

Для circles не нужны float/physics semantics. Integer arithmetic сохраняет simple deterministic simulation path и не связывает core с Godot numeric types.

### Почему 30 Hz

Simulation Foundation намеренно откладывал real-time pacing до появления movement/network requirement. Этот requirement теперь появился. 30 Hz даёт concrete playable pacing с умеренным packet/tick volume и остаётся host-level configurable assumption, а не simulation time definition.

### Почему full snapshots

При двух entities стоимость full snapshot ничтожна, а correctness/debuggability выше delta protocol. Snapshot содержит всё нужное для восстановления current client view, поэтому latest-state delivery подходит естественно.

## Alternatives

### Godot built-in ENet multiplayer

Rejected для authoritative foundation: создаёт риск связать server/network ownership с Godot runtime, что противоречит standalone server ADR-002.

### ENet-CSharp

Deferred: reliable UDP semantics подходят, но package/native library path требует platform-specific native binary deployment и initialization. Для первого slice это лишний operational risk.

Reference: https://github.com/nxrighthere/ENet-CSharp

### `System.Net.Quic`

Deferred: .NET QUIC API stable в современных .NET, но runtime зависит от MsQuic. Linux требует `libmsquic`, macOS имеет дополнительную installation/runtime setup. Это увеличивает desktop/CI deployment surface раньше необходимости.

Reference: https://learn.microsoft.com/dotnet/fundamentals/networking/quic/quic-overview

### WebSocket/TCP

Rejected как initial game transport: reliable ordered stream прост, но world-state traffic наследует head-of-line behavior и не имеет natural latest-state unreliable/sequenced semantics.

### Raw UDP

Rejected: connection/reliability/sequencing/timeout/fragmentation пришлось бы строить до проверки milestone.

### Generic serializer package

Deferred: current protocol слишком мал, чтобы оправдать extra package/generator/schema lifecycle.

### Float/vector physics model

Deferred/rejected: не нужен для milestone и преждевременно фиксирует numeric/physics architecture.

### Snapshot delta replication

Deferred до measurable state-size/bandwidth pressure.

## Consequences

Положительные:

- реальный multiplayer path проверяется на OS sockets;
- standalone server и Godot client используют один managed networking boundary;
- simulation остаётся transport-independent canonical world;
- client target authority structurally constrained;
- state loss не блокирует newer full snapshot;
- networking decisions доступны CI/agent verification;
- next combat slice получает уже доказанный authoritative transport path.

Trade-offs:

- LiteNetLib становится новой infrastructure dependency;
- protocol codec hand-written;
- reliable ordered input позже может быть слишком conservative для latency-sensitive gameplay;
- full snapshot every tick не масштабируется на большой world;
- 30 Hz может потребовать пересмотра;
- no prediction/interpolation делает первый visual demo менее smooth;
- integer spatial state ещё не является final physics/gameplay coordinate system.

## Verification required before Accepted

ADR может стать `Accepted` только после implementation evidence + Networking Vertical Slice Merge Gate, где automated verification доказывает:

- `OpenMoba.Networking` dependency boundaries;
- protocol parser/compatibility/failure tests;
- simulation positioned movement semantics;
- server 30 Hz host composition без wall-clock dependency внутри Sim;
- real standalone server process + two clients over loopback UDP;
- distinct session/control mapping;
- structural target authority / forged target rejection;
- both clients converge to server-produced authoritative state;
- stale snapshot rejection;
- existing Simulation Foundation and Godot architecture checks remain green.

## Revisit when

Создать отдельный design/ADR review, если:

- prediction/reconciliation/input history вводятся в client/server protocol;
- 30 Hz недостаточно для combat/game feel;
- reliable ordered input показывает unacceptable latency/HOL behavior;
- full snapshots становятся bandwidth bottleneck;
- interest management/relevancy требуется для larger world;
- protocol требует backward compatibility между engine versions;
- encryption/auth/public internet hosting становится product requirement;
- LiteNetLib platform/support constraints становятся blocker;
- mobile/Web targets становятся обязательными;
- spatial model требует float/fixed-point physics/collision semantics.
