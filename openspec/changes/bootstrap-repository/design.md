# Design: bootstrap repository Open MOBA

- **Status:** Proposed
- **Gate:** Requires Design Gate
- **Date:** 2026-09-03

## 1. Цель design

Этот design материализует уже принятые foundation ADR и current specs в минимальную физическую структуру repository. Он не вводит gameplay architecture и не решает deferred вопросы simulation/networking/modding.

Design должен обеспечить три свойства одновременно:

1. новые agents получают устойчивый и предсказуемый layout;
2. запрещённые dependencies можно обнаружить автоматически;
3. clean checkout проверяется CLI/CI без ручного открытия editor.

Нормативная архитектура остаётся в ADR-001, ADR-002, ADR-003 и current OpenSpec specs. Этот документ выбирает только implementation-level форму bootstrap.

## 2. Принятый repository layout

Использовать один root solution `OpenMoba.sln` и стандартное разделение `src/` / `tests/`:

```text
OpenMoba.sln
global.json
Directory.Build.props
Directory.Packages.props
.editorconfig

src/
├── OpenMoba.Contracts/
├── OpenMoba.ModApi.Contracts/
├── OpenMoba.Sim/
├── OpenMoba.Server/
├── OpenMoba.Cli/
└── OpenMoba.Client.Godot/
    ├── project.godot
    ├── OpenMoba.Client.Godot.csproj
    ├── Scenes/
    │   └── Bootstrap.tscn
    └── Scripts/
        └── Bootstrap.cs

tests/
├── OpenMoba.Sim.Tests/
└── OpenMoba.ArchitectureTests/
```

`OpenMoba.ModRuntime` физически **не создаётся** в bootstrap. Причина: у него пока нет утверждённой public/runtime interface surface, а создание пустого project заставило бы раньше времени выбрать его TFM и dependency graph. Он появится в отдельном mod-runtime implementation change.

`OpenMoba.ModApi.Contracts` создаётся уже сейчас, потому что его existence и `net8.0` compatibility surface прямо зафиксированы ADR-002. В bootstrap assembly может оставаться без gameplay contracts; не следует добавлять фиктивный public API только ради наличия кода.

## 3. Target frameworks и toolchain

### .NET SDK

Root `global.json` фиксирует .NET SDK `10.0.400` с patch-only roll-forward внутри feature band:

```json
{
  "sdk": {
    "version": "10.0.400",
    "rollForward": "latestPatch",
    "allowPrerelease": false
  }
}
```

На дату design .NET 10 является active LTS, а 10.0.400 — current SDK feature band. Shared assemblies по-прежнему target `net8.0` только из-за принятой Godot compatibility boundary; это не означает, что .NET 8 выбирается как долгосрочный server/runtime baseline.

### Project TFMs

```text
OpenMoba.Contracts         net8.0
OpenMoba.ModApi.Contracts  net8.0
OpenMoba.Sim               net8.0
OpenMoba.Server            net10.0
OpenMoba.Cli               net10.0
OpenMoba.Client.Godot      net8.0 / Godot.NET.Sdk 4.7.2
```

Test projects могут target `net10.0`, если они проверяют assemblies с `net8.0` target.

### Godot

Client baseline фиксируется на **Godot 4.7.2 .NET/Mono**.

`OpenMoba.Client.Godot.csproj` использует exact `Godot.NET.Sdk/4.7.2`; floating Godot SDK versions не допускаются.

CI использует официальный release asset:

```text
Godot_v4.7.2-stable_mono_linux_x86_64.zip
```

из `godotengine/godot-builds` и проверяет download по официальному `SHA512-SUMS.txt` перед execution.

Third-party `setup-godot` action в bootstrap не используется: официальный binary + checksum делает CI path явным, легко диагностируемым и не создаёт дополнительный durable dependency на чужой GitHub Action.

## 4. Общие project defaults

`Directory.Build.props` задаёт только нейтральные compiler defaults, например:

- `Nullable=enable`;
- `ImplicitUsings=enable`;
- deterministic build settings, если они не конфликтуют с Godot SDK;
- warnings не должны глобально подавляться.

Package versions test tooling хранятся централизованно в `Directory.Packages.props` и pin'ятся exact versions. Floating package versions запрещены.

Runtime third-party packages в bootstrap не добавляются, кроме того, что требуется Godot SDK. Test-only framework packages допустимы.

## 5. Dependency graph

Принятый bootstrap graph:

```text
OpenMoba.Contracts                 OpenMoba.ModApi.Contracts
       ^
       |
OpenMoba.Sim
       ^
       |
OpenMoba.Server

OpenMoba.Client.Godot ---> OpenMoba.Contracts

OpenMoba.Cli
```

### Explicit rules

`OpenMoba.Contracts`:
- не зависит от других Open MOBA projects;
- не зависит от Godot.

`OpenMoba.ModApi.Contracts`:
- не зависит от Godot;
- не содержит runtime/interpreter implementation;
- bootstrap не требует от него dependency на `OpenMoba.Contracts`, пока реальный contract не создаёт такую необходимость.

`OpenMoba.Sim`:
- зависит только от engine-neutral shared assemblies, необходимых фактическому bootstrap code;
- на первом bootstrap достаточно reference на `OpenMoba.Contracts`;
- MUST NOT reference Godot, client project, server host или concrete networking/runtime adapters.

`OpenMoba.Server`:
- reference `OpenMoba.Sim`;
- является owner process lifecycle;
- не зависит от Godot;
- в bootstrap не открывает network socket и не создаёт match/tick loop.

`OpenMoba.Client.Godot`:
- reference `OpenMoba.Contracts`;
- MUST NOT reference `OpenMoba.Sim` на bootstrap этапе;
- Godot types остаются внутри client project.

`OpenMoba.Cli`:
- существует как future tooling executable;
- bootstrap не фиксирует public command tree и не добавляет dependencies без необходимости.

## 6. Shared assemblies не получают фиктивный API

Пустой/структурный assembly допустим.

Не следует создавать типы вроде `Hero`, `Entity`, `World`, `GameState`, `Tick`, `Ability`, `NetworkMessage` или fake Mod API только потому, что project уже существует. Такие типы являются будущими product/architecture decisions.

Если compiler/project tooling требует source file, допускается internal implementation marker или assembly metadata, который явно не является public platform contract.

## 7. Server smoke contract

`OpenMoba.Server` получает только один bootstrap-specific verification mode:

```bash
dotnet run --project src/OpenMoba.Server -- --smoke
```

### Поведение smoke

Smoke path:

1. запускает standalone .NET process;
2. создаёт минимальную bootstrap composition, включая ссылку на `OpenMoba.Sim`;
3. не требует Godot/GPU/display server;
4. не открывает listener/socket;
5. не выбирает simulation tick rate;
6. не создаёт gameplay/world model;
7. выводит одну machine-readable success record;
8. завершается кодом `0`.

Рекомендуемый success output:

```json
{"component":"OpenMoba.Server","mode":"smoke","status":"ok"}
```

Failure должен завершаться non-zero exit code. Ошибка может выводиться в stderr; exact error schema не становится public server protocol в bootstrap.

`--smoke` считается internal development verification contract, а не production server CLI API.

## 8. Godot client smoke

Client содержит минимальный main scene `Scenes/Bootstrap.tscn` с C# script `Scripts/Bootstrap.cs`.

Scene не содержит gameplay state. Её задача — доказать, что:

- Godot project валиден;
- C# assembly собирается и загружается;
- project reference на shared `OpenMoba.Contracts` разрешается;
- scene может стартовать в headless mode без display/GPU;
- процесс может сам завершиться success code.

`Bootstrap.cs` может в `_Ready()` вывести machine-readable bootstrap marker и вызвать `GetTree().Quit(0)`. Это smoke behavior, а не gameplay architecture.

Verification выполняется после `dotnet build` client project:

```bash
godot --headless --path src/OpenMoba.Client.Godot
```

Используется именно .NET/Mono Godot binary той же pinned 4.7.2 версии, что и project SDK.

## 9. Architecture verification

Создать `tests/OpenMoba.ArchitectureTests` без third-party architecture framework.

Test читает repository `.csproj` как data и проверяет project graph. Такой подход намеренно выбран вместо reflection-only проверки: он обнаруживает нарушение boundary ещё на уровне declared project references и не требует загружать Godot assemblies.

Минимальные assertions:

1. `OpenMoba.Sim` target = `net8.0`.
2. `OpenMoba.Sim` не имеет `ProjectReference` на client/server projects.
3. transitive project graph, достижимый из `OpenMoba.Sim`, не содержит Godot project SDK/package/reference.
4. никакой достижимый dependency из `OpenMoba.Sim` не является `OpenMoba.Client.Godot`.
5. `OpenMoba.Client.Godot` reference `OpenMoba.Contracts`.
6. `OpenMoba.Client.Godot` не reference `OpenMoba.Sim` в bootstrap graph.
7. `OpenMoba.Server` reference `OpenMoba.Sim`.
8. shared projects сохраняют expected TFMs.

Godot dependency определяется как минимум по:

- project SDK `Godot.NET.Sdk`;
- explicit package/reference на Godot assemblies;
- reference на Godot client project.

Дополнительно CI отдельно собирает `OpenMoba.Sim.csproj` **до установки Godot binary**, что является независимым доказательством engine-neutral build path.

## 10. Simulation tests

`tests/OpenMoba.Sim.Tests` создаётся уже в bootstrap, но не должен изобретать simulation behavior.

Допустимы только structural/bootstrap tests, если появляется минимальный internal object для server composition. Если `OpenMoba.Sim` остаётся пустым assembly, test project может содержать minimal assembly-load/smoke test либо только быть готовой test boundary.

Не создавать fake world/entity/tick tests ради заполнения project.

## 11. CI architecture

Существующий `Spec Integrity` workflow сохраняется.

Добавляется отдельный code workflow, условно `.github/workflows/build.yml`, чтобы specification validation и code verification можно было диагностировать независимо.

### Job 1 — .NET core

На Ubuntu runner:

```text
checkout
setup .NET from global.json
restore
build OpenMoba.sln
build OpenMoba.Sim separately
run dotnet tests
run server --smoke
```

Минимальные canonical commands:

```bash
dotnet restore OpenMoba.sln
dotnet build OpenMoba.sln --no-restore --configuration Release
dotnet test OpenMoba.sln --no-build --configuration Release
dotnet run --project src/OpenMoba.Server --configuration Release --no-build -- --smoke
```

Agent может скорректировать exact `dotnet test` project selection, если Godot SDK делает solution-level test invocation некорректной, но результат должен остаться одним documented reproducible test path без editor actions.

### Job 2 — Godot headless smoke

На Ubuntu runner:

```text
checkout
setup pinned .NET SDK
download official Godot 4.7.2 Mono Linux x86_64
verify SHA512 against official release checksum
extract binary
build OpenMoba.Client.Godot.csproj
run client project headless
```

Export templates не требуются: bootstrap проверяет editor/runtime project load, а не export artifact.

Godot binary не коммитится в repository.

## 12. Local verification contract

Документация bootstrap должна разделять:

### Core checks — без Godot editor binary

```bash
dotnet restore OpenMoba.sln
dotnet build OpenMoba.sln --configuration Release
dotnet test OpenMoba.sln --configuration Release
dotnet run --project src/OpenMoba.Server --configuration Release -- --smoke
```

### Client smoke — требует pinned Godot 4.7.2 .NET binary

```bash
dotnet build src/OpenMoba.Client.Godot/OpenMoba.Client.Godot.csproj --configuration Release
godot --headless --path src/OpenMoba.Client.Godot
```

Не добавлять `make`, `just`, Docker или custom orchestration только ради объединения этих команд в bootstrap. Unified `openmoba` CLI остаётся будущим tooling outcome.

## 13. `OpenMoba.Cli` в bootstrap

CLI project должен только:

- собираться как standalone `net10.0` executable;
- иметь минимальный entry point;
- не объявлять будущий stable public command surface.

Не нужно реализовывать `openmoba build/test/server/client/play/simulate/mod` в этом change.

Если entry point выводит placeholder text, документация должна явно считать его non-contractual bootstrap behavior.

## 14. Что design сознательно не решает

Этот design не выбирает:

- ECS или world/entity representation;
- fixed/variable timestep или tick frequency;
- deterministic lockstep/rollback model;
- transport library;
- replication serialization;
- network message contracts;
- DI framework;
- logging framework;
- gameplay event bus;
- command/order model;
- public Mod API types;
- MoonSharp integration;
- runtime sandbox implementation;
- package format;
- production container/deployment model.

Не добавлять эти dependencies/abstractions «для будущего».

## 15. Alternatives considered

### Несколько solutions

Например отдельные `.sln` для server/client. Отклонено на bootstrap: усложняет cold-start agent workflow и позволяет boundaries незаметно расходиться. Один solution остаётся простым index всех assemblies.

### Godot project вне `src/`

Отдельный `client/` top-level каталог возможен, но создаёт вторую организационную модель. `src/OpenMoba.Client.Godot` сохраняет единый component naming/layout и явно показывает, что Godot — один adapter среди platform components.

### Client reference на `OpenMoba.Sim`

Отклонено на bootstrap. Это может понадобиться для prediction/replay/local simulation в будущем, но пока такого решения нет. Client получает только shared contracts.

### Создать `OpenMoba.ModRuntime` сейчас

Отклонено. Это вынудило бы выбрать runtime TFM/interfaces до соответствующего implementation change и стимулировало бы MoonSharp integration раньше времени.

### Third-party architecture testing library

Отклонено на bootstrap. Project-reference graph можно проверить небольшим deterministic test без дополнительной dependency и без загрузки Godot runtime.

### Godot setup GitHub Action

Отклонено для foundation smoke: официальный pinned release asset + checksum прозрачнее и снижает supply-chain/dependency surface. В будущем action может быть принят, если manual setup станет measurable maintenance burden.

### Docker как canonical bootstrap environment

Отклонено. Для текущей задачи .NET/Godot CLI достаточно; Docker добавил бы ещё один environment layer до появления deployment need.

## 16. Risks и mitigations

### `net8.0` nearing end of support

Это известное следствие current Godot compatibility baseline, а не новый выбор bootstrap. Shared TFM должен быть пересмотрен отдельным compatibility change после обновления Godot baseline. Server/CLI уже остаются на .NET 10 LTS.

### Godot binary download делает CI тяжелее

Mono Linux archive порядка сотни MB. Для bootstrap это приемлемая цена за real headless integration proof. Cache можно добавить позже только после измерения CI pain.

### Architecture test может проверять только declared project graph

Для foundation этого достаточно вместе с отдельным build `OpenMoba.Sim` без Godot binary. Более глубокие assembly architecture rules добавляются, когда появится реальный code graph.

### Empty contract assemblies выглядят искусственно

Это намеренно. Лучше физически зафиксировать accepted boundary без fake public API, чем создать premature types, которые agents начнут воспринимать как продуктовый contract.

## 17. Verification strategy

Implementation считается соответствующей design только если PR предоставляет evidence минимум для:

```bash
openspec validate --all --strict --no-interactive
openspec validate --archived --no-interactive

dotnet restore OpenMoba.sln
dotnet build OpenMoba.sln --configuration Release
dotnet test OpenMoba.sln --configuration Release
dotnet run --project src/OpenMoba.Server --configuration Release -- --smoke

# pinned Godot 4.7.2 .NET binary
godot --headless --path src/OpenMoba.Client.Godot
```

CI должен выполнять эквивалентные checks на clean runner.

Review agent дополнительно проверяет:

- отсутствие gameplay/networking/ECS scope creep;
- отсутствие `Sim -> Godot` dependency;
- отсутствие `Client -> Sim` dependency на bootstrap;
- отсутствие MoonSharp/mod runtime implementation;
- отсутствие floating tool/package versions;
- отсутствие manual editor-only step в acceptance path.

## 18. Delegation после Design Gate

После approval создаётся `tasks.md`. Затем один execution harness получает branch/change и должен реализовать tasks без истории этого чата.

Harness не получает дополнительные архитектурные объяснения, кроме короткого запускающего prompt с указанием прочитать repository source of truth. Это намеренно проверяет cold-start качество `AGENTS.md` + ADR + specs + active change.

Если harness обнаруживает необходимость выйти за этот design, корректное поведение — остановиться и вынести blocker, а не расширить scope самостоятельно.
