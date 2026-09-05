## Purpose

Расширяет authoritative command flow минимальной movement transition, сохраняя host-owned submission boundary и запрет arbitrary network commands.

## ADDED Requirements

### Requirement: Simulation принимает минимальную movement command через existing command boundary
`OpenMoba.Sim` SHALL поддерживать approved engine-neutral movement command для positioned entity через тот же host submission flow, что и другие authoritative commands.

#### Scenario: Host submits movement command
- **WHEN** server host после network/session validation submit supported movement command
- **THEN** command SHALL войти в pending FIFO sequence без немедленной authoritative mutation

### Requirement: Movement command подчиняется deterministic FIFO ordering
Movement commands SHALL обрабатываться в existing submission order относительно других commands одного simulation instance.

#### Scenario: Create then move in one pending sequence
- **WHEN** host submit create command, затем movement command для resulting entity в поддерживаемой последовательности до advancement
- **THEN** simulation SHALL обрабатывать transitions в submission order согласно defined command semantics

### Requirement: Network boundary не расширяет command hierarchy произвольно
Добавление network adapter MUST NOT превращать `SimulationCommand` в открытый creator/network extension point для arbitrary external concrete commands.

#### Scenario: Network receives unknown intent kind
- **WHEN** server получает unknown/unsupported intent kind
- **THEN** adapter SHALL не создавать новый arbitrary subtype simulation command для обхода approved command surface

### Requirement: Invalid movement command не создаёт partial mutation
Expected invalid movement command SHALL сохранять authoritative consistency и возвращать rejection/outcome согласно simulation command policy.

#### Scenario: Move missing entity
- **WHEN** movement command target entity отсутствует на processing tick
- **THEN** authoritative world SHALL не получить partial spatial mutation и step result SHALL отражать rejection согласно approved outcome model
