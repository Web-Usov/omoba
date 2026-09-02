## Purpose

Определяет boundary между presentation client и platform core, чтобы renderer/editor можно было развивать независимо от authoritative gameplay logic.

## ADDED Requirements

### Requirement: Client shell является presentation adapter
Client engine SHALL отвечать за presentation, input capture, UI, animation, VFX/audio и asset/editor integration, но MUST NOT быть authoritative owner gameplay state.

#### Scenario: Client receives authoritative state
- **WHEN** client получает authoritative state update
- **THEN** presentation SHALL отображать state без превращения local scene state в источник authoritative truth

### Requirement: Initial client target является desktop-first
Foundation SHALL поддерживать desktop client workflow для Windows, Linux и macOS; Web export MUST NOT быть acceptance requirement foundation этапа.

#### Scenario: Web limitation does not block foundation
- **WHEN** current Godot C# toolchain не поддерживает Web export
- **THEN** foundation change SHALL remain valid при условии работоспособного desktop client workflow
