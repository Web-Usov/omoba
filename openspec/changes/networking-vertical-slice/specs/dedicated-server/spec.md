## Purpose

Расширяет standalone authoritative server hosting реальным network listener lifecycle и composition networking adapter вокруг `OpenMoba.Sim`.

## ADDED Requirements

### Requirement: Dedicated server владеет network listener lifecycle
Standalone server SHALL запускать и завершать real network listener как часть host process lifecycle без Godot runtime.

#### Scenario: Server starts multiplayer listener
- **WHEN** server запускается в Networking Vertical Slice mode
- **THEN** host SHALL создать authoritative simulation, bind network endpoint и принимать supported client connections

### Requirement: Server является authority boundary между network и simulation
Network adapter SHALL передавать validated host intent в `OpenMoba.Sim`, а server MUST NOT предоставлять clients direct mutable simulation access.

#### Scenario: Client message reaches simulation
- **WHEN** server получает supported gameplay intent от active session
- **THEN** server SHALL проверить protocol/session authority до submit соответствующей simulation command

### Requirement: Network shutdown является bounded
Server SHALL иметь bounded shutdown path, который завершает listener/client resources и authoritative host lifecycle без manual intervention.

#### Scenario: Integration test stops server
- **WHEN** canonical networking test запрашивает завершение server
- **THEN** listener и active connection resources SHALL быть освобождены, а process/test host SHALL завершиться автоматически
