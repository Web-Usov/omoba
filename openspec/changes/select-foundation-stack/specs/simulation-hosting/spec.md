## Purpose

Определяет способ hosting authoritative simulation независимо от renderer, process UI и конкретного network transport.

## ADDED Requirements

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
