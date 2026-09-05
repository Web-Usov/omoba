## Purpose

Определяет минимальную server-to-client replication boundary для observable authoritative state Milestone A без фиксации конкретного transport, encoding или snapshot/delta implementation.

## ADDED Requirements

### Requirement: Replicated state происходит из authoritative simulation
Server SHALL формировать client-visible replicated state из authoritative simulation observation и MUST NOT использовать network adapter state как отдельный canonical world source.

#### Scenario: Replicate entity positions
- **WHEN** authoritative simulation завершает movement transition
- **THEN** последующая state update к clients SHALL отражать server-observed authoritative entity identity и position

### Requirement: Несколько clients наблюдают один authoritative world
Все gameplay-active clients SHALL получать достаточную authoritative state information, чтобы наблюдать обе entities Milestone A.

#### Scenario: Both clients observe both entities
- **WHEN** client A и client B активны и authoritative world содержит entity A и entity B
- **THEN** оба clients SHALL иметь возможность получить identity и authoritative position обеих entities

### Requirement: State updates имеют monotonic ordering boundary
Replicated state SHALL содержать server-defined ordering/version information, достаточную для client обнаружения stale state относительно уже принятого более нового state.

#### Scenario: Stale update arrives after newer state
- **WHEN** client уже принял state version `N` и позднее получает update, который protocol определяет как более старый
- **THEN** client SHALL NOT откатывать observable authoritative state к stale version

### Requirement: Clients сходятся к server-produced state
Для bounded Milestone A scenario после прекращения новых intents и доставки поддерживаемых state updates client A и client B SHALL сходиться к одному server-produced authoritative observation.

#### Scenario: Movement convergence
- **WHEN** server обработал movement intent и оба clients получили актуальный authoritative state
- **THEN** identity и position обеих entities, наблюдаемые clients, SHALL соответствовать одному authoritative server state

### Requirement: Replication schema не является mutable authority channel
Client-side replicated representation MUST NOT предоставлять путь, при котором локальная mutation автоматически становится authoritative server state.

#### Scenario: Client mutates presentation state
- **WHEN** client locally изменяет rendered/presentation representation entity
- **THEN** это изменение SHALL NOT изменять authoritative server state без отдельного valid intent path
