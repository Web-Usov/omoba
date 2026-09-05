## Purpose

Определяет минимальный connection/session lifecycle и control assignment между standalone authoritative server и multiplayer clients без account, lobby или matchmaking semantics.

## ADDED Requirements

### Requirement: Server принимает несколько concurrent client sessions
Standalone server SHALL поддерживать как минимум две одновременно активные client sessions через real network endpoint.

#### Scenario: Two clients connect
- **WHEN** два clients подключаются к одному запущенному server instance через поддерживаемый protocol
- **THEN** server SHALL установить две distinct active sessions

### Requirement: Session identity является server-assigned runtime identity
Каждая активная network session SHALL иметь distinct server-controlled runtime identity, пригодную для authority mapping внутри конкретного server instance.

#### Scenario: Distinct session identities
- **WHEN** client A и client B завершают успешный handshake
- **THEN** их active session identities SHALL различаться

### Requirement: Protocol compatibility проверяется до gameplay intent
Server SHALL определить поддерживаемую protocol compatibility до принятия gameplay-affecting intent от session.

#### Scenario: Unsupported protocol version
- **WHEN** client предлагает unsupported protocol version или несовместимую handshake configuration
- **THEN** server SHALL отклонить или завершить session без передачи gameplay intent в authoritative simulation

### Requirement: Control assignment принадлежит server
Server SHALL связывать active session с authority над конкретной authoritative entity и MUST NOT принимать client-provided control ownership как trusted fact.

#### Scenario: Server assigns controlled entity
- **WHEN** новая поддерживаемая session становится gameplay-active
- **THEN** server SHALL определить entity, которой эта session имеет право управлять, через server-owned mapping

### Requirement: Session termination не передаёт authority другой session неявно
Завершение network session MUST NOT автоматически предоставлять другой существующей session control authority над ранее назначенной entity без явного server decision.

#### Scenario: Client disconnects
- **WHEN** session A завершается
- **THEN** session B SHALL NOT получить control authority entity A только вследствие disconnect A
