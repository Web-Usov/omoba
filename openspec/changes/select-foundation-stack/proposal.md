# Proposal: выбрать foundation stack Open MOBA

## Problem

Open MOBA уже зафиксировала ключевые архитектурные инварианты — platform first, public Mod API для reference game, server-authoritative multiplayer, headless simulation и agent-first development — но пока не выбрала базовый технологический стек, на котором будет строиться первая рабочая вертикаль.

Без этого решения нельзя корректно начать bootstrap codebase, потому что game engine/client shell, simulation runtime, dedicated server boundary и mod runtime определяют структуру repository, dependency direction, test strategy и возможности дальнейшего делегирования AI-агентам.

## Goal

Выбрать минимальный foundation stack и зафиксировать runtime boundaries, достаточные для начала реализации Milestone A без преждевременного проектирования всей платформы.

Результатом change должно стать принятое архитектурное решение, которое однозначно отвечает на вопросы:

1. Какой engine/runtime используется для presentation/client/editor shell?
2. На каком языке и runtime реализуется authoritative simulation core?
3. Как simulation зависит от client/game engine — или не зависит от него?
4. Как выглядит boundary dedicated server на первом этапе?
5. Какой runtime model используется для gameplay mods на первом этапе?
6. Какое направление dependencies между client, simulation, server и Mod API является допустимым?
7. Какие части решения считаются временными и могут быть заменены без нарушения public contracts?

## Why now

Следующий этап проекта — code bootstrap и первая networked vertical slice. До создания solution/project structure необходимо определить foundation boundaries, иначе ранний код случайно превратит предварительные идеи в фактическую архитектуру.

## Scope

В рамках change необходимо:

- сравнить разумные варианты game engine/client shell;
- выбрать основной implementation language/runtime для simulation core;
- определить границу между presentation/client integration и simulation;
- определить стартовую модель dedicated server;
- выбрать начальный подход к mod scripting/runtime с учётом sandboxing и future replaceability;
- определить dependency direction между foundation components;
- определить минимальные CLI/headless требования для agent-first development;
- оформить ADR для фундаментальных решений;
- зафиксировать verification strategy для первого code bootstrap.

## Non-goals

Этот change намеренно НЕ выбирает и НЕ проектирует:

- конкретную ECS library или окончательную world model;
- network transport;
- replication protocol;
- prediction, reconciliation или rollback;
- simulation tick rate;
- serialization format;
- package manifest format;
- visual scripting/editor UX;
- Workshop/backend/persistence;
- production deployment infrastructure;
- окончательную public Mod API surface;
- конкретные gameplay systems reference MOBA.

## Constraints

Выбранный foundation stack должен сохранять действующие project principles:

- reference game не получает privileged gameplay hooks;
- authoritative simulation отделена от presentation;
- simulation запускается headless;
- dedicated-server workflow автоматизируем;
- ключевые проверки доступны из CLI/CI без ручной работы в editor;
- durable project knowledge не зависит от одного AI vendor/tool;
- community mods не требуют выполнения произвольного недоверенного native/.NET-кода;
- решение реалистично для solo + AI-agent development.

## Evaluation criteria

Design сравнивает варианты по следующим критериям:

1. Agent delegability.
2. Simulation isolation.
3. Server viability.
4. Mod safety.
5. Repository friendliness.
6. Performance headroom.
7. Cross-platform support.
8. Licensing/ownership.
9. Replaceability.
10. Solo-development cost.

## Candidate direction to evaluate

Это baseline для design/research, а не автоматически принятое решение:

```text
Godot (.NET) -> presentation / client / editor shell
        |
        v
OpenMoba.Client adapter
        |
        v
shared contracts

OpenMoba.Server
        |
        +---- OpenMoba.Sim (plain .NET / C#)
        +---- OpenMoba.ModRuntime
        +---- OpenMoba.Networking (later decision)
```

## Capabilities

### New Capabilities

- `foundation-runtime` — общие runtime boundaries и допустимое направление dependencies.
- `simulation-hosting` — headless hosting contract authoritative simulation.
- `client-integration` — contract между game engine/presentation и platform core.
- `dedicated-server` — отдельный authoritative server host.
- `mod-runtime` — sandboxed и replaceable scripting runtime boundary.
- `agent-verification` — CLI/headless contract для machine-verifiable development.

### Modified Capabilities

Нет: проект greenfield, current specs для этих capabilities ещё отсутствуют.

## Impact

Решение определит будущую структуру solution/repository, базовые runtimes, dependency boundaries, минимальный toolchain и направление первого code bootstrap. Оно не добавляет gameplay behavior и не реализует vertical slice.

## ADR required

Да. Ожидаются ADR для:

- client/game engine;
- simulation language/runtime и server boundary;
- initial mod runtime model.

## Expected outcome

После design approval следующий implementation change сможет создать repository/code skeleton без повторного обсуждения базовых runtime boundaries.

Первый code bootstrap должен сделать возможной следующую machine-verifiable вертикаль:

```text
headless simulation starts
+
standalone server starts
+
client shell starts/headless-loads
+
core checks run from CLI/CI
```

Само создание этой вертикали в scope текущего proposal не входит.
