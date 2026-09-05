## Purpose

Определяет минимальную authoritative world/entity model, ownership mutable state, entity lifecycle и engine-neutral observation boundary.

## Requirements

### Requirement: Simulation владеет authoritative world state
Mutable authoritative world state SHALL принадлежать simulation instance и MUST NOT предоставляться host как напрямую изменяемая collection/object graph.

#### Scenario: Host observes without mutation
- **WHEN** host запрашивает observable state simulation
- **THEN** host SHALL получить engine-neutral read-only representation без ссылки, позволяющей напрямую мутировать authoritative internals

### Requirement: Entities имеют stable identity внутри simulation instance
Каждая созданная entity SHALL получать non-zero identity, уникальную внутри lifetime конкретного simulation instance, и identity MUST NOT переиспользоваться после удаления entity в этом instance.

#### Scenario: Create two entities
- **WHEN** simulation последовательно создаёт две entities
- **THEN** их identities SHALL быть различными и non-zero

#### Scenario: Removed identity is not reused
- **WHEN** entity удалена и позднее создаётся новая entity
- **THEN** новая entity SHALL NOT получать identity удалённой entity

### Requirement: Entity lifecycle является authoritative transition
Создание и удаление entity SHALL происходить только как simulation-controlled authoritative transition.

#### Scenario: Destroy existing entity
- **WHEN** валидная lifecycle command удаляет существующую entity и simulation продвигается
- **THEN** entity SHALL отсутствовать в последующем observable world state

### Requirement: Observation имеет deterministic ordering
Когда observable representation перечисляет entities, ordering SHALL быть определённым и воспроизводимым для одинакового authoritative state.

#### Scenario: Observe active entities
- **WHEN** host наблюдает world с несколькими active entities
- **THEN** entities SHALL быть возвращены в стабильном ascending identity order
