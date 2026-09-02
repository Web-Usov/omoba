# ADR-001: Godot как client/editor shell

- **Status:** Accepted
- **Date:** 2026-09-03

## Context

Open MOBA нужен renderer/editor/client shell, но проект не должен тратить solo-development capacity на создание general-purpose rendering/editor engine. При этом authoritative simulation должна оставаться engine-independent, а workflow — пригодным для Git, CLI/CI и AI agents.

Рассматривались Godot, Unity, Unreal Engine и custom rendering stack.

## Decision

Использовать Godot 4.7.x .NET как presentation/client/editor shell.

Godot отвечает за rendering, camera, input capture, UI, animation, VFX/audio, assets и editor/map tooling.

Godot НЕ является owner authoritative gameplay simulation. Engine-specific types не входят в `OpenMoba.Sim` contracts.

## Rationale

- MIT license и source availability;
- официальный C#/.NET support;
- CLI/headless workflows;
- text-friendly `.tscn` scenes;
- desktop cross-platform support;
- существенно меньший operational/editor overhead, чем Unreal;
- меньше platform ownership/licensing risk, чем proprietary engines;
- не требует строить собственный renderer/editor pipeline.

## Alternatives

### Unity

Сильный C# ecosystem и зрелый tooling. Не выбран из-за proprietary engine ownership и vendor coupling. Runtime Fee отменён, но licensing/plan dependence остаётся.

### Unreal Engine

Сильный renderer/network/server tooling и source access. Не выбран из-за тяжёлого C++/Blueprint/editor workflow, royalty model и избыточного complexity для первого platform MVP.

### Custom engine/rendering stack

Даёт максимальный control, но создаёт огромный non-product scope: renderer, scene/editor, UI, import, animation/VFX tooling.

## Consequences

Положительные:

- быстрый старт client/editor;
- text/CLI-friendly workflow;
- нет engine royalties;
- presentation можно менять без переписывания authoritative simulation.

Отрицательные:

- Godot 4 C# currently не экспортируется в Web;
- C# integration следует runtime baseline Godot;
- необходим явный adapter layer вместо прямого размещения gameplay logic в nodes.

## Revisit when

- Web становится обязательной launch platform;
- Godot создаёт измеримый performance/tooling blocker;
- project requirements требуют engine capability, которой Godot системно не предоставляет.
