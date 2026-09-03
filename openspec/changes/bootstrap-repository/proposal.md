# Proposal: bootstrap repository Open MOBA

## Problem

Open MOBA уже имеет принятые product principles, foundation ADR и current OpenSpec specs, которые определяют основные runtime boundaries: Godot .NET как presentation/client/editor shell, plain C#/.NET authoritative simulation без Godot dependency, standalone dedicated server и machine-verifiable headless workflow.

При этом repository пока почти полностью состоит из документации. Принятые boundaries ещё не материализованы в собираемую структуру проектов, поэтому невозможно проверить две важные гипотезы одновременно:

1. что foundation architecture действительно реализуема без скрытой связности между client, simulation и server;
2. что новый execution agent, не имеющий истории чата, способен по repository source of truth самостоятельно собрать, проверить и подготовить ограниченный implementation PR.

Без минимального code skeleton следующий этап `Simulation Foundation` начнёт одновременно решать repository layout, toolchain, project references и собственно simulation architecture, что увеличит scope и риск случайных решений.

## Goal

Создать минимальный собираемый repository skeleton, который материализует уже принятые foundation boundaries и предоставляет воспроизводимый CLI/CI verification contract для последующих OpenSpec changes и execution agents.

Bootstrap должен доказать структуру и зависимости, но не реализовывать gameplay/platform subsystems раньше их собственных design changes.

## Why now

Roadmap определяет `Repository Bootstrap` как следующий outcome после завершённых `Foundation Governance` и `Foundation Architecture`.

Этот change нужен до `Simulation Foundation`, потому что следующие agents должны работать уже внутри устойчивой solution/project structure и получать объективный feedback от build/tests/headless checks, а не проектировать эти основы заново в каждой задаче.

## Scope

В рамках change необходимо:

- создать минимальную .NET solution/workspace structure согласно Accepted foundation ADR;
- создать engine-neutral shared contract projects, достаточные для проверки client/server consumption boundaries;
- создать `OpenMoba.Sim` как plain .NET library без Godot dependency;
- создать standalone `OpenMoba.Server` host с ограниченным headless smoke path, который может стартовать, скомпозировать bootstrap dependencies и завершиться без GUI/Godot;
- создать минимальный `OpenMoba.Cli` executable как будущую точку repository tooling, не реализуя весь planned `openmoba` command surface;
- создать Godot 4.7.x .NET client shell и adapter boundary, использующий shared engine-neutral contracts;
- при необходимости создать structural projects `OpenMoba.ModApi.Contracts` и `OpenMoba.ModRuntime`, но без реализации scripting interpreter/runtime behavior;
- создать automated tests/architecture checks, которые доказывают отсутствие запрещённой dependency `OpenMoba.Sim -> Godot`;
- расширить CI так, чтобы clean checkout мог выполнить .NET build/tests, standalone server smoke и Godot headless project load без interactive editor actions;
- зафиксировать минимальный reproducible local toolchain и verification commands в repository documentation.

Точный layout каталогов, project references, architecture-test mechanism, smoke command contract и Godot CI setup должны быть определены в `design.md` после Intent Gate.

## Expected repository boundaries

Bootstrap должен материализовать уже принятое направление, не расширяя его:

```text
OpenMoba.Contracts
OpenMoba.ModApi.Contracts
        ^
        |
+-------+-------------------------------+
|                                       |
OpenMoba.Sim                       OpenMoba.Client.Godot
(no Godot dependency)             (Godot adapter / presentation)
        ^
        |
OpenMoba.Server
        |
        +-- OpenMoba.ModRuntime boundary

OpenMoba.Cli
(repository tooling entry point)
```

Это conceptual boundary для Intent Gate. Конкретные project references утверждаются на Design Gate.

## Acceptance outcome

После implementation fresh execution agent или CI runner должен иметь возможность из clean checkout доказать минимум следующее:

1. Shared .NET solution/projects собираются documented CLI command из корня repository.
2. Automated tests запускаются documented CLI command без IDE/editor.
3. `OpenMoba.Sim` собирается без Godot assemblies/runtime.
4. Automated architecture check падает, если в simulation появляется Godot dependency.
5. `OpenMoba.Server` имеет bounded headless smoke path: процесс создаёт bootstrap composition, не требует graphical environment/Godot и завершается с machine-readable success/failure.
6. Godot .NET client project загружается через documented headless CLI path без ручного открытия editor.
7. Client adapter и standalone server способны потреблять общие engine-neutral contracts без forked contract copies.
8. CI выполняет соответствующие checks автоматически и сохраняет существующую OpenSpec validation.
9. Локальные verification commands и required toolchain описаны достаточно, чтобы новый agent мог воспроизвести CI contract без истории чата.

## Non-goals

Этот change намеренно НЕ выбирает и НЕ реализует:

- ECS library или окончательную world/entity model;
- simulation clock/tick rate/scheduling semantics;
- entities, components, movement, collision, navigation или orders;
- network transport;
- replication, serialization, prediction, reconciliation или rollback;
- matchmaking/session/backend infrastructure;
- heroes, abilities, items, combat или любой reference-game gameplay;
- окончательную public Mod API surface;
- MoonSharp integration или исполнение mod scripts;
- sandbox security model beyond сохранения уже принятой runtime boundary;
- package manifest/dependency resolver;
- Workshop/registry;
- visual editor/creator UX;
- production deployment/container orchestration;
- полный unified `openmoba` CLI command set.

Если для bootstrap оказывается необходимо принять одно из этих решений, implementation должна остановиться и change должен быть перепланирован через соответствующий Design/ADR process.

## Constraints

Implementation обязана сохранять current specs и Accepted ADR:

- `OpenMoba.Sim` не зависит от Godot и presentation runtime;
- Godot-specific types не протекают в simulation/shared core contracts;
- authoritative server остаётся standalone .NET process;
- server/process lifecycle не принадлежит simulation library;
- shared contracts могут использоваться и current Godot .NET client, и standalone server;
- core build/tests/headless checks доступны из CLI/CI;
- bootstrap не создаёт privileged gameplay path для reference content;
- dependencies добавляются только когда они необходимы acceptance criteria;
- durable verification не зависит от конкретного AI coding vendor.

Accepted foundation baseline для design:

- Godot 4.7.x .NET — client/presentation/editor shell;
- shared compatibility-facing assemblies — `net8.0`-compatible surface;
- standalone server/CLI — .NET 10 LTS;
- Mod API/runtime boundary — interpreter-neutral и replaceable.

Если текущая toolchain compatibility требует отступления от Accepted baseline, это считается blocker и требует явного design/ADR review, а не скрытой корректировки implementation.

## Affected current capabilities

Этот change в первую очередь реализует уже существующие requirements и не должен придумывать им новый смысл.

Затрагиваются current capabilities:

- `foundation-runtime`;
- `simulation-hosting`;
- `client-integration`;
- `dedicated-server`;
- `agent-verification`.

`mod-runtime` остаётся отдельной будущей implementation capability: bootstrap может создать structural boundary/project, но не считается реализацией scripting sandbox, execution budgeting или interpreter integration.

## Spec delta expectation

На Intent Gate **новый behavioral spec delta не предполагается**: требования bootstrap уже выражены current foundation specs и roadmap outcome.

После Intent Gate design обязан проверить это предположение. Если для корректной реализации обнаружится отсутствующий durable requirement, до Design Gate необходимо добавить минимальный delta spec и явно показать owner, какое новое требование предлагается. Нельзя превращать implementation detail в новый implicit contract.

## Design questions after Intent Gate

После approval необходимо определить только implementation-level choices, достаточные для bootstrap:

1. Конкретный repository/solution/project layout.
2. Project reference graph, соответствующий Accepted dependency direction.
3. Способ pin/reproducibility .NET SDK и Godot toolchain.
4. Механизм architecture check для запрещённых references.
5. Minimal server smoke lifecycle и machine-readable exit semantics.
6. Minimal Godot scene/project, достаточный для headless load verification.
7. Test project organization и команды root-level verification.
8. Расширение GitHub Actions для .NET/Godot checks без избыточной CI инфраструктуры.
9. Нужны ли `OpenMoba.ModRuntime` и `OpenMoba.ModApi.Contracts` как физические projects уже в bootstrap или достаточно зафиксировать один из boundaries иначе — без реализации scripting behavior.

Эти choices не должны принимать ECS/network/gameplay decisions.

## ADR impact

Новый ADR **не ожидается**, если design только материализует Accepted ADR-001..003.

Если design требует изменить runtime baseline, dependency boundary, server process model, Godot ownership или mod-runtime architecture, необходимо остановиться и создать/обновить ADR до implementation.

## Delegation model

После Intent и Design Gate implementation должна быть передана одному execution harness/agent как одна ограниченная branch/worktree задача.

Agent получает repository как единственный durable context и должен:

```text
read AGENTS.md + relevant vision/ADR/spec/change
        -> implement approved tasks
        -> run documented verification
        -> fix failures inside approved scope
        -> prepare PR with evidence
        -> stop before Merge Gate
```

Первый bootstrap PR одновременно является проверкой качества architecture и проверкой agent-first development model.

## Expected outcome

После merge `Repository Bootstrap` считается завершённым, когда repository имеет минимальную физическую структуру и CI contract, достаточные для следующего отдельного change `Simulation Foundation` без повторного выбора foundation stack или repository architecture.
