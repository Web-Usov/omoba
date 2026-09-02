## Purpose

Определяет machine-verifiable development contract, позволяющий coding/review agents собирать, запускать и проверять foundation без ручных editor rituals.

## ADDED Requirements

### Requirement: Core build и tests доступны из CLI
Foundation projects SHALL предоставлять reproducible CLI commands для build и automated tests.

#### Scenario: Clean checkout verification
- **WHEN** agent или CI получает clean repository checkout с documented toolchain
- **THEN** core build и tests SHALL запускаться без interactive editor actions

### Requirement: Client shell имеет headless smoke check
Godot client project SHALL иметь CLI/headless verification, подтверждающую загрузку project и integration assemblies.

#### Scenario: CI loads client project
- **WHEN** CI запускает Godot в headless mode для client project
- **THEN** project SHALL загрузиться или завершиться с machine-readable failure без GUI interaction

### Requirement: Architecture boundaries проверяемы автоматически
Foundation SHALL позволять automated check, который обнаруживает запрещённую dependency `OpenMoba.Sim -> Godot`.

#### Scenario: Forbidden reference introduced
- **WHEN** simulation assembly получает Godot dependency
- **THEN** architecture verification SHALL fail
