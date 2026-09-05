## Purpose

Определяет boundary untrusted player intent между client session и authoritative server, включая ownership validation и запрет direct authoritative mutation.

## ADDED Requirements

### Requirement: Client передаёт intent, а не authoritative state
Network protocol SHALL представлять player action как intent, который server обязан интерпретировать и валидировать до authoritative transition.

#### Scenario: Client requests movement
- **WHEN** client отправляет supported movement intent
- **THEN** server SHALL обработать его как request на изменение controlled entity, а не как trusted authoritative position

### Requirement: Target authoritative entity разрешается server из session ownership
Movement intent SHALL NOT требовать client-provided authoritative target `EntityId`; server SHALL разрешать target из server-owned mapping active session -> controlled entity.

#### Scenario: Session moves controlled entity
- **WHEN** active session отправляет valid movement intent
- **THEN** server SHALL применить intent только к entity, назначенной этой session server-owned control mapping

#### Scenario: Client forges direct target payload
- **WHEN** client пытается отправить unsupported payload/message, содержащий arbitrary target entity для обхода session mapping
- **THEN** server SHALL отклонить такой protocol path без authoritative mutation чужой entity

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
