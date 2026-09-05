## Purpose

Определяет machine-verifiable end-to-end acceptance path Networking Vertical Slice через real network endpoint, standalone server и минимум два clients.

## ADDED Requirements

### Requirement: Verification использует real network endpoint
Canonical networking integration test SHALL использовать OS network endpoint между server и clients и MUST NOT считаться выполненным только через in-memory transport adapter.

#### Scenario: Canonical test starts server listener
- **WHEN** networking integration scenario запускается из terminal/CI
- **THEN** standalone server SHALL bind поддерживаемый local network endpoint, а test clients SHALL подключиться через этот endpoint

### Requirement: Canonical scenario включает два concurrent clients
Automated verification SHALL запускать минимум два client participants против одного authoritative server instance.

#### Scenario: Two-client session established
- **WHEN** canonical scenario завершает connection phase
- **THEN** server SHALL иметь две distinct gameplay-active sessions и две distinct controlled entities

### Requirement: Canonical scenario доказывает end-to-end movement
Automated verification SHALL доказать путь client intent -> server validation -> simulation transition -> replicated state для движения хотя бы одной entity.

#### Scenario: Client A moves
- **WHEN** client A отправляет valid movement intent
- **THEN** authoritative simulation SHALL изменить position entity A и оба clients SHALL получить resulting authoritative observation

### Requirement: Canonical scenario покрывает authority violation
Automated verification SHALL доказать, что client не может выбрать arbitrary authoritative target вместо server-owned session mapping.

#### Scenario: Client A forges direct target payload
- **WHEN** client A отправляет unsupported/malformed direct-target payload с identity entity B
- **THEN** server SHALL отклонить path, authoritative state entity B SHALL остаться неизменным и normal session A control mapping SHALL не измениться

### Requirement: Canonical scenario покрывает invalid protocol input
Automated verification SHALL включать bounded malformed или unsupported protocol input case.

#### Scenario: Invalid message is received
- **WHEN** test client отправляет покрытый malformed/unsupported message
- **THEN** server SHALL продолжить bounded execution без crash и без direct authoritative mutation от этого message

### Requirement: Verification завершается автоматически
Canonical networking scenario SHALL иметь bounded startup, execution и shutdown и MUST NOT требовать ручного Godot editor interaction.

#### Scenario: CI execution completes
- **WHEN** scenario запускается в supported CI environment
- **THEN** test SHALL завершиться success/failure result в bounded time и освободить server/client resources
