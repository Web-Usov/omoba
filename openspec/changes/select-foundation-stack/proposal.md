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

- сравнить разумные варианты game engine/client shell, включая Godot и альтернативы, релевантные текущим требованиям;
- выбрать основной implementation language/runtime для simulation core;
- определить границу между presentation/client integration и simulation;
- определить стартовую модель dedicated server;
- выбрать начальный подход к mod scripting/runtime с учётом sandboxing и будущей replaceability;
- определить dependency direction между foundation components;
- определить минимальные CLI/headless требования, необходимые для agent-first development;
- оформить одно или несколько ADR для принятых фундаментальных решений;
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

Эти решения должны приниматься отдельными changes после появления соответствующего контекста.

## Constraints

Выбранный foundation stack должен сохранять действующие project principles:

- reference game не получает privileged gameplay hooks;
- authoritative simulation должна быть отделена от presentation;
- simulation должна запускаться headless;
- dedicated-server workflow должен быть автоматизируемым;
- ключевые проверки должны быть доступны из CLI/CI без ручной работы в editor;
- durable project knowledge не должно зависеть от одного AI vendor/tool;
- mod runtime не должен требовать выполнения произвольного недоверенного native/.NET-кода от community mods;
- решение должно быть реалистичным для solo + AI-agent development.

## Evaluation criteria

Design должен сравнивать варианты как минимум по следующим критериям:

1. **Agent delegability** — build/test/run/debug доступны через CLI и хорошо автоматизируются.
2. **Simulation isolation** — authoritative core может существовать без renderer/editor.
3. **Server viability** — headless/dedicated execution не является вторичным hack.
4. **Mod safety** — есть реалистичный путь к sandboxed public scripting API.
5. **Repository friendliness** — text-based artifacts и predictable project structure подходят для Git/review/agents.
6. **Performance headroom** — стек не создаёт очевидного потолка для target genre на ранней стадии.
7. **Cross-platform support** — desktop client и Linux server должны быть практичными целями.
8. **Licensing/ownership** — stack не должен создавать неприемлемый platform lock-in или экономический риск.
9. **Replaceability** — временные adapters/transports/tooling можно заменить без переписывания gameplay contracts.
10. **Solo-development cost** — предпочтение меньшему числу runtimes/languages и меньшему operational overhead при сопоставимом результате.

## Candidate direction to evaluate

Это не утверждённое решение, а baseline для design/research:

```text
Godot (.NET)           -> presentation / client / editor shell
        |
        v
OpenMoba.Client adapter
        |
        v
OpenMoba.Sim (plain .NET / C#)
        |
        +------ OpenMoba.Networking
        |
        +------ OpenMoba.ModApi
        |
        v
OpenMoba.Server
        |
        v
sandboxed gameplay scripting (Lua candidate)
```

Design обязан проверить эту схему против существенных альтернатив, а не принимать её только потому, что она обсуждалась ранее.

## Affected capabilities

Первично затрагиваются будущие capabilities:

- `foundation-runtime`;
- `simulation-hosting`;
- `client-integration`;
- `dedicated-server`;
- `mod-runtime`;
- `agent-verification`.

Текущих behavioral specs для этих capabilities ещё нет; они будут созданы после Intent Gate как часть этого change.

## ADR required

Да.

Как минимум ожидаются ADR для:

- выбора client/game engine;
- выбора simulation language/runtime и dependency boundary;
- выбора начального mod runtime model.

Количество ADR может быть скорректировано в design, если решения логически лучше объединить или разделить.

## Expected outcome

После завершения design и approval мы должны иметь достаточно ясную foundation architecture, чтобы следующий implementation change мог создать repository/code skeleton без повторного обсуждения базовых runtime boundaries.

Первый code bootstrap после этого решения должен позволить в перспективе выполнить machine-verifiable вертикаль:

```text
headless simulation starts
        +
server starts
        +
client shell starts
        +
client/server exchange minimal state
        +
all core checks can run from CLI/CI
```

Само создание этой вертикали в scope текущего proposal не входит.
