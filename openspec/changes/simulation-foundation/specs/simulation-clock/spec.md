## Purpose

Определяет logical time authoritative simulation и host-controlled advancement независимо от wall clock, renderer frame rate и process scheduler.

## ADDED Requirements

### Requirement: Simulation использует дискретный logical tick
Authoritative simulation SHALL представлять logical time монотонным целочисленным tick index, начинающимся с `0` при создании simulation instance.

#### Scenario: Initial logical time
- **WHEN** host создаёт новый simulation instance
- **THEN** current logical tick SHALL быть `0`

#### Scenario: One explicit advancement
- **WHEN** host выполняет один successful simulation advancement
- **THEN** current logical tick SHALL увеличиться ровно на `1`

### Requirement: Advancement принадлежит host
Simulation SHALL продвигаться только через явный host-controlled API и MUST NOT самостоятельно зависеть от wall-clock timers, sleep, renderer frame loop или background scheduler.

#### Scenario: Headless fast advancement
- **WHEN** automated test последовательно выполняет `N` advancement calls без ожидания wall-clock time
- **THEN** simulation SHALL достичь logical tick `N`

### Requirement: Foundation clock не фиксирует production tick rate
Simulation Foundation SHALL NOT требовать конкретную wall-time duration или Hz для одного logical tick.

#### Scenario: No wall-time mapping required
- **WHEN** host создаёт simulation instance с foundation configuration
- **THEN** host SHALL иметь возможность продвигать logical ticks без настройки production tick frequency
