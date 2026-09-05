## Purpose

Расширяет Godot presentation adapter network connection/input/replicated-state path, сохраняя client non-authoritative.

## ADDED Requirements

### Requirement: Godot client подключается к standalone server через engine-neutral networking boundary
Desktop Godot client SHALL иметь возможность установить supported network session со standalone `OpenMoba.Server` без reference на `OpenMoba.Sim`.

#### Scenario: Client connects to server
- **WHEN** Godot client запускается с supported local server endpoint
- **THEN** client SHALL установить session через networking/shared contracts и не SHALL загружать simulation library для authority

### Requirement: Client input преобразуется в network intent
Local input capture SHALL формировать supported movement intent и MUST NOT напрямую изменять authoritative server state.

#### Scenario: User requests movement
- **WHEN** local player вводит supported movement input
- **THEN** client SHALL отправить movement intent server и MAY обновлять только presentation-local transient state согласно approved design

### Requirement: Client отображает replicated authoritative entities
Godot presentation SHALL отображать минимум две simple entities на основе полученного authoritative replicated state.

#### Scenario: Two authoritative entities received
- **WHEN** client получает current authoritative observation entity A и entity B
- **THEN** presentation SHALL отобразить обе entities с server-produced identities/positions

### Requirement: Stale replicated state не становится presentation truth
Client SHALL соблюдать protocol ordering/version boundary и MUST NOT заменять уже принятую более новую authoritative observation заведомо stale update.

#### Scenario: Older state arrives late
- **WHEN** client после state version `N` получает update, определённый protocol как older than `N`
- **THEN** rendered authoritative model SHALL не откатываться к stale state
