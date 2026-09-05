## Purpose

Определяет boundary untrusted player intent между client session и authoritative server, включая ownership validation и запрет direct authoritative mutation.

## ADDED Requirements

### Requirement: Client передаёт intent, а не authoritative state
Network protocol SHALL представлять player action как intent, который server обязан интерпретировать и валидировать до authoritative transition.

#### Scenario: Client requests movement
- **WHEN** client отправляет supported movement intent
- **THEN** server SHALL обработать его как request на изменение controlled entity, а не как trusted authoritative position

### Requirement: Server валидирует control authority
Gameplay-affecting intent SHALL применяться только если active session имеет server-owned authority над target entity.

#### Scenario: Session controls own entity
- **WHEN** session отправляет valid intent для назначенной ей entity
- **THEN** server MAY передать соответствующий approved simulation command в authoritative simulation

#### Scenario: Session targets another entity
- **WHEN** session A отправляет intent, пытающийся воздействовать на entity, назначенную session B
- **THEN** server SHALL отклонить intent без authoritative mutation target entity

### Requirement: Network client не получает arbitrary simulation command surface
Network protocol MUST NOT позволять client напрямую создавать произвольные internal `OpenMoba.Sim` commands или обходить server validation boundary.

#### Scenario: Unsupported command kind
- **WHEN** client отправляет unsupported или неизвестный gameplay message kind
- **THEN** server SHALL не преобразовывать его в arbitrary simulation command и SHALL сохранить authoritative consistency

### Requirement: Invalid intent обрабатывается bounded и безопасно
Malformed, unsupported или semantically invalid intent SHALL обрабатываться без crash server и без partial authoritative mutation в покрытом protocol path.

#### Scenario: Malformed movement payload
- **WHEN** active session отправляет malformed movement payload
- **THEN** server SHALL reject/ignore сообщение согласно protocol policy и authoritative world SHALL не измениться вследствие этого payload
