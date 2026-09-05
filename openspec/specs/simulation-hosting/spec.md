## Purpose

Определяет способ hosting authoritative simulation независимо от renderer, process UI и конкретного network transport.

## Requirements

### Requirement: Simulation поддерживает headless hosting
Simulation SHALL запускаться и продвигаться host process без graphical presentation layer.

#### Scenario: Headless host start
- **WHEN** simulation запускается из automated test или server process без display server
- **THEN** simulation SHALL initialize and run без graphical dependencies

### Requirement: Process lifecycle принадлежит host
Simulation core MUST NOT владеть process lifecycle или требовать конкретный executable host.

#### Scenario: Test host composes simulation
- **WHEN** unit/integration test создаёт simulation host in-process
- **THEN** test SHALL управлять start/advance/stop lifecycle через engine-neutral API

### Requirement: Host явно создаёт и продвигает simulation instance
Engine-neutral host SHALL иметь возможность создать simulation instance с явной configuration, submit commands, выполнять logical advancement и завершить владение instance без background loop внутри simulation core.

#### Scenario: In-process test host controls lifecycle
- **WHEN** automated test создаёт simulation instance, submit command и выполняет advancement
- **THEN** весь lifecycle SHALL выполняться синхронно под control test host без graphical runtime, socket или wall-clock waiting

### Requirement: Server может композировать simulation foundation
Standalone server host SHALL быть способен создать и проверить simulation foundation через existing headless smoke path без Godot dependency и без открытия network listener.

#### Scenario: Server foundation smoke
- **WHEN** `OpenMoba.Server` запускается в bounded smoke mode
- **THEN** server SHALL создать simulation instance, выполнить минимальный authoritative advancement и завершиться с machine-readable success/failure без network transport
