## Purpose

Расширяет observable authoritative world минимальным spatial state, сохраняя ownership, stable identity и deterministic observation guarantees.

## ADDED Requirements

### Requirement: Positioned entities хранят authoritative spatial state внутри simulation
Authoritative 2D position entity SHALL принадлежать mutable world state `OpenMoba.Sim` и MUST NOT храниться как отдельная canonical position в network или Godot adapter.

#### Scenario: Server observes positioned entity
- **WHEN** simulation world содержит positioned entity
- **THEN** host SHALL получить её authoritative position через read-only observation boundary

### Requirement: Spatial state связан со stable `EntityId`
Observable spatial state SHALL использовать existing `EntityId` identity semantics ADR-004 и MUST NOT вводить отдельную client-owned canonical entity identity.

#### Scenario: Replicate entity identity and position
- **WHEN** host наблюдает positioned entity для replication
- **THEN** observation SHALL связывать position с тем же stable `EntityId`, которым владеет simulation lifecycle

### Requirement: Spatial observation остаётся read-only
Host/client-facing observation of position MUST NOT предоставлять mutable reference, позволяющую обойти authoritative command processing.

#### Scenario: Consumer receives spatial snapshot
- **WHEN** host или adapter получает spatial observation
- **THEN** consumer SHALL не иметь direct mutable access к authoritative stored position через эту observation

### Requirement: Entity without spatial state остаётся допустимой foundation entity
Добавление Milestone A spatial capability MUST NOT требовать, чтобы каждая simulation entity обязательно имела position.

#### Scenario: Non-positioned entity exists
- **WHEN** simulation создаёт foundation entity без spatial state
- **THEN** entity SHALL оставаться valid active entity и existing lifecycle/identity semantics SHALL сохраняться
