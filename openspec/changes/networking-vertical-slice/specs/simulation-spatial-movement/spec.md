## Purpose

Определяет минимальный engine-neutral authoritative 2D spatial/movement capability для Milestone A без general physics, navigation или ECS requirement.

## ADDED Requirements

### Requirement: Authoritative entity может иметь минимальное 2D spatial state
Simulation SHALL поддерживать минимальное authoritative 2D position state для entities, участвующих в Networking Vertical Slice.

#### Scenario: Observe positioned entity
- **WHEN** authoritative entity имеет spatial state
- **THEN** read-only simulation observation SHALL включать её identity и current authoritative 2D position

### Requirement: Movement является simulation-controlled transition
Изменение authoritative position SHALL происходить через simulation command/transition boundary и MUST NOT выполняться direct mutation network или presentation adapter.

#### Scenario: Apply valid movement command
- **WHEN** host submit valid movement command и выполняет следующий logical advancement
- **THEN** authoritative position target entity SHALL измениться согласно approved movement semantics

### Requirement: Movement следует logical tick semantics
Movement command, принятая на tick `T`, SHALL влиять на authoritative spatial state не раньше processing следующего logical advancement согласно `simulation-command-flow`.

#### Scenario: Movement waits for advancement
- **WHEN** host submit movement command на tick `T` и не вызывает `Advance()`
- **THEN** observable authoritative position SHALL оставаться состоянием tick `T`

### Requirement: Spatial observation имеет deterministic ordering
Когда snapshot содержит несколько positioned entities, ordering SHALL сохранять existing deterministic entity observation guarantees.

#### Scenario: Observe two positioned entities
- **WHEN** snapshot содержит entity A и entity B со spatial state
- **THEN** entities SHALL наблюдаться в стабильном ascending `EntityId` order вместе с их authoritative positions

### Requirement: Foundation movement не требует physics/navigation framework
Milestone A SHALL поддерживать required authoritative movement без обязательного physics engine, collision system, navigation/pathfinding или generic ECS framework.

#### Scenario: Two circles scenario runs
- **WHEN** canonical Milestone A scenario двигает controlled entities
- **THEN** movement SHALL быть проверяем без необходимости создавать collision/navigation/physics subsystems
