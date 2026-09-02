## Purpose

Определяет базовые runtime boundaries Open MOBA и правила зависимостей, чтобы platform core оставался независимым от presentation engine и заменяемых adapters.

## ADDED Requirements

### Requirement: Simulation runtime не зависит от presentation engine
Authoritative simulation core SHALL собираться и исполняться без Godot или другого graphical/editor runtime.

#### Scenario: Headless simulation dependency check
- **WHEN** simulation project собирается отдельно от client project
- **THEN** build SHALL succeed без Godot assemblies и graphical environment

### Requirement: Engine-specific dependencies остаются во внешних adapters
Platform core MUST NOT принимать Godot-specific types как часть simulation contracts.

#### Scenario: Client integration type boundary
- **WHEN** Godot client передаёт данные platform core
- **THEN** boundary SHALL использовать engine-neutral contracts вместо `Node`, `Resource` или Godot vector/object types

### Requirement: Shared contracts совместимы с client и server hosts
Shared foundation assemblies SHALL иметь target/API surface, которую могут потреблять current Godot .NET client adapter и standalone .NET server host.

#### Scenario: Shared assembly consumption
- **WHEN** client adapter и server host ссылаются на один shared contract assembly
- **THEN** оба hosts SHALL build без forked copies этого contract
