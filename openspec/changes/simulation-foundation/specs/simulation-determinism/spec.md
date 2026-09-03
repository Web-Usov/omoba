## Purpose

Определяет ownership deterministic RNG и поддерживаемую reproducibility guarantee authoritative simulation foundation.

## ADDED Requirements

### Requirement: RNG state принадлежит simulation instance
Simulation SHALL инициализировать deterministic pseudo-random state явным seed при создании instance и MUST NOT использовать process-global или wall-clock-seeded randomness внутри deterministic simulation path.

#### Scenario: Same seed initializes same RNG sequence
- **WHEN** два deterministic RNG instances создаются с одинаковым seed и одинаковой algorithm/version configuration
- **THEN** одинаковая последовательность draw operations SHALL возвращать одинаковые значения

#### Scenario: Different seed changes RNG sequence
- **WHEN** deterministic RNG instances создаются с различными seed
- **THEN** verification sequence SHALL различаться для выбранного test vector

### Requirement: Supported simulation scenario воспроизводим
Для одной версии `OpenMoba.Sim` одинаковые initial configuration, seed и ordered command sequence SHALL приводить к одинаковому canonical logical result в пределах одной заявленной runtime compatibility boundary.

#### Scenario: Repeat canonical scenario
- **WHEN** automated test дважды создаёт fresh simulation instance с одинаковыми config/seed, submit одинаковые commands в одинаковом порядке и выполняет одинаковое число ticks
- **THEN** logical tick, ordered entity observation и ordered outcomes SHALL совпадать

### Requirement: Foundation determinism не является blanket cross-version guarantee
Simulation Foundation MUST NOT обещать, что deterministic result сохранится неизменным между различными `OpenMoba.Sim` versions или различными unsupported runtime compatibility boundaries без отдельного compatibility requirement.

#### Scenario: Version upgrade
- **WHEN** implementation RNG/world semantics меняются в будущей version
- **THEN** сохранение старого deterministic trace SHALL требовать отдельного compatibility decision вместо implicit guarantee этого capability
