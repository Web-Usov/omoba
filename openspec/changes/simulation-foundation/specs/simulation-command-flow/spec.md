## Purpose

Определяет engine-neutral intake/order semantics commands и ordered authoritative outcomes без networking или gameplay-specific coupling.

## ADDED Requirements

### Requirement: Host передаёт intent через command boundary
Host SHALL передавать simulation intent через явный command submission API и MUST NOT требовать прямой mutation authoritative world.

#### Scenario: Submit foundation lifecycle command
- **WHEN** host отправляет valid foundation command до следующего advancement
- **THEN** command SHALL быть принят в pending command sequence без немедленной прямой mutation world host-кодом

### Requirement: Commands обрабатываются на следующем logical tick
Commands, успешно принятые после предыдущего advancement и до следующего, SHALL обрабатываться во время следующего logical tick.

#### Scenario: Command waits for advancement
- **WHEN** host submit command на tick `T` и не выполняет advancement
- **THEN** observable authoritative state SHALL оставаться состоянием tick `T`

#### Scenario: Command applies on next tick
- **WHEN** после submit command на tick `T` host выполняет один advancement
- **THEN** command SHALL быть обработан как часть tick `T + 1`

### Requirement: Ordering commands является deterministic
В рамках одного simulation instance commands SHALL получать монотонный submission order и SHALL обрабатываться в этом порядке.

#### Scenario: Multiple commands before one tick
- **WHEN** host последовательно submit commands `A`, `B`, `C` до одного advancement
- **THEN** simulation SHALL process `A`, затем `B`, затем `C`

### Requirement: Expected invalid command не создаёт partial mutation
Если foundation command не может быть применена из-за ожидаемого invalid state, simulation SHALL сохранить authoritative consistency и SHALL вернуть ordered rejection outcome вместо partial mutation.

#### Scenario: Destroy missing entity
- **WHEN** command пытается удалить identity, которая не существует или уже удалена
- **THEN** world SHALL остаться неизменным для этой command и step outcomes SHALL содержать rejection после всех предыдущих outcomes согласно command order

### Requirement: Outcomes принадлежат completed advancement
Каждый successful advancement SHALL возвращать ordered read-only batch outcomes, возникших именно во время этого tick.

#### Scenario: Observe step outcomes
- **WHEN** advancement создаёт несколько authoritative transitions
- **THEN** host SHALL получить outcomes в том же deterministic order и batch SHALL быть связан с completed logical tick
