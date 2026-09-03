# Tasks: bootstrap repository Open MOBA

- **Intent Gate:** approved
- **Design Gate:** approved
- **Execution owner:** one harness-agent / one branch / one PR

Этот task list материализует уже утверждённый `proposal.md` и `design.md`. Он не даёт права расширять scope или принимать deferred architecture decisions.

## 1. Preflight и toolchain

- [ ] Прочитать `AGENTS.md`, `docs/vision/principles.md`, `docs/architecture/overview.md`, ADR-001..003, relevant current specs и весь `openspec/changes/bootstrap-repository/` до изменения кода.
- [ ] Добавить root `global.json` с .NET SDK `10.0.400`, `rollForward: latestPatch`, `allowPrerelease: false`.
- [ ] Добавить `Directory.Build.props`, `Directory.Packages.props` и `.editorconfig` только с нейтральными compiler/package defaults; не добавлять runtime framework/dependencies «на будущее».
- [ ] Создать один root solution `OpenMoba.sln`.

Verification:

```bash
dotnet --version
dotnet restore OpenMoba.sln
```

## 2. Foundation project skeleton

- [ ] Создать `src/OpenMoba.Contracts` с target `net8.0` и без Godot dependencies.
- [ ] Создать `src/OpenMoba.ModApi.Contracts` с target `net8.0`; не добавлять public gameplay/mod types без утверждённого requirement.
- [ ] Создать `src/OpenMoba.Sim` с target `net8.0` и reference только на необходимые engine-neutral shared projects; bootstrap baseline — `OpenMoba.Contracts`.
- [ ] Создать `src/OpenMoba.Server` как standalone `net10.0` executable с reference на `OpenMoba.Sim`.
- [ ] Создать `src/OpenMoba.Cli` как минимальный standalone `net10.0` executable без stable command surface.
- [ ] Добавить все production projects в `OpenMoba.sln`.
- [ ] Не создавать `OpenMoba.ModRuntime` в этом change.

Verification:

```bash
dotnet build src/OpenMoba.Sim/OpenMoba.Sim.csproj --configuration Release
dotnet build OpenMoba.sln --configuration Release
```

Acceptance:
- `OpenMoba.Sim` собирается без установленного Godot binary/runtime;
- shared projects не используют Godot types;
- bootstrap не создаёт `Hero`, `Entity`, `World`, `Tick`, `Ability`, `NetworkMessage` или иной premature public API.

## 3. Standalone server smoke

- [ ] Реализовать bootstrap-only mode `--smoke` в `OpenMoba.Server`.
- [ ] Smoke path должен создать минимальную composition, доказывающую dependency на `OpenMoba.Sim`, но не создавать sockets, match/world/tick loop или gameplay state.
- [ ] При success вывести одну machine-readable record, например `{"component":"OpenMoba.Server","mode":"smoke","status":"ok"}`, и завершиться code `0`.
- [ ] Failure path должен завершаться non-zero code.

Verification:

```bash
dotnet run --project src/OpenMoba.Server --configuration Release -- --smoke
```

## 4. Godot .NET client shell

- [ ] Создать `src/OpenMoba.Client.Godot` на exact `Godot.NET.Sdk/4.7.2`, target `net8.0`.
- [ ] Добавить `ProjectReference` на `OpenMoba.Contracts`.
- [ ] Не добавлять reference на `OpenMoba.Sim`.
- [ ] Создать минимальный `project.godot`, `Scenes/Bootstrap.tscn` и `Scripts/Bootstrap.cs`.
- [ ] Main scene должна только доказать project/C# assembly/shared-contract load, вывести machine-readable bootstrap marker и завершить process через `GetTree().Quit(0)`.
- [ ] Не добавлять gameplay state, camera/game controls, networking или editor tooling.
- [ ] Добавить client project в `OpenMoba.sln`.

Verification:

```bash
dotnet build src/OpenMoba.Client.Godot/OpenMoba.Client.Godot.csproj --configuration Release
godot --headless --path src/OpenMoba.Client.Godot
```

Использовать pinned Godot 4.7.2 .NET/Mono binary.

## 5. Automated architecture boundaries

- [ ] Создать `tests/OpenMoba.ArchitectureTests` с target `net10.0`.
- [ ] Проверять `.csproj` dependency graph напрямую, без third-party architecture framework.
- [ ] Добавить assertions минимум для:
  - [ ] `OpenMoba.Sim` target = `net8.0`;
  - [ ] `OpenMoba.Sim` не reference client/server projects;
  - [ ] transitive graph из `OpenMoba.Sim` не содержит `Godot.NET.Sdk`, Godot package/reference или Godot client project;
  - [ ] `OpenMoba.Client.Godot` reference `OpenMoba.Contracts`;
  - [ ] `OpenMoba.Client.Godot` не reference `OpenMoba.Sim`;
  - [ ] `OpenMoba.Server` reference `OpenMoba.Sim`;
  - [ ] shared TFMs соответствуют Accepted design.
- [ ] Проверить, что test действительно fail'ится при искусственном запрещённом `Sim -> Godot/client` reference, затем вернуть repository в корректное состояние.

## 6. Simulation test boundary

- [ ] Создать `tests/OpenMoba.Sim.Tests` с target `net10.0`.
- [ ] Добавлять только structural/bootstrap test; не изобретать world/entity/tick/gameplay behavior ради наполнения test project.
- [ ] Pin exact versions test-only packages в `Directory.Packages.props`.
- [ ] Добавить оба test projects в `OpenMoba.sln`.

Verification:

```bash
dotnet test OpenMoba.sln --configuration Release
```

Если Godot SDK делает solution-level `dotnet test` нестабильным, выбрать и документировать один явный root-equivalent path через конкретные test projects; не требовать IDE/editor.

## 7. Code CI

- [ ] Сохранить существующий `Spec Integrity` workflow без ослабления.
- [ ] Добавить отдельный `.github/workflows/build.yml` для code verification на PR и push в `main`.
- [ ] Core job должен на clean Ubuntu runner выполнять restore, Release build, отдельный `OpenMoba.Sim` build, automated tests и server smoke.
- [ ] Godot job должен скачать официальный `Godot_v4.7.2-stable_mono_linux_x86_64.zip` из `godotengine/godot-builds`.
- [ ] Проверить archive через официальный `SHA512-SUMS.txt` до extraction/execution.
- [ ] Не использовать third-party setup-godot action в bootstrap.
- [ ] Не коммитить Godot binary/export templates в repository.
- [ ] Export templates не устанавливать: acceptance — headless project load, не export.

## 8. Developer/agent verification documentation

- [ ] Добавить короткую durable documentation с required toolchain и canonical local commands для core checks и Godot smoke.
- [ ] Не создавать implementation diary/status report.
- [ ] Не добавлять `make`, `just`, Docker или custom orchestration только для объединения команд.

Canonical evidence должно включать:

```bash
openspec validate --all --strict --no-interactive
openspec validate --archived --no-interactive

dotnet restore OpenMoba.sln
dotnet build OpenMoba.sln --configuration Release
dotnet build src/OpenMoba.Sim/OpenMoba.Sim.csproj --configuration Release
dotnet test OpenMoba.sln --configuration Release
dotnet run --project src/OpenMoba.Server --configuration Release -- --smoke

godot --headless --path src/OpenMoba.Client.Godot
```

Если exact test command корректируется в рамках разрешённого design exception, обновить documentation и PR evidence так, чтобы существовал один однозначный machine-verifiable path.

## 9. Final verification и PR evidence

- [ ] Запустить все применимые canonical checks локально/в harness environment.
- [ ] Дождаться successful GitHub CI для `Spec Integrity` и нового code workflow.
- [ ] Обновить этот `tasks.md`, отметив реально завершённые tasks.
- [ ] В PR body добавить delivered structure, verification results, tests, known limitations и deviations (если были).
- [ ] Выполнить self-review на scope creep.
- [ ] Передать PR независимому review-agent; не merge'ить самостоятельно.

## Stop conditions

Execution agent обязан остановиться и сообщить blocker, если implementation требует любого из следующего:

- выбор ECS/world/entity model;
- выбор simulation tick/fixed timestep semantics;
- network transport, replication или serialization contracts;
- prediction/reconciliation/rollback;
- public gameplay/Mod API types;
- создание/реализацию `OpenMoba.ModRuntime` или MoonSharp integration;
- изменение `Sim -> presentation` boundary;
- `Client -> Sim` dependency;
- изменение standalone server model;
- изменение Godot/.NET baseline из Accepted design;
- DI/logging/gameplay frameworks как новые архитектурные dependencies;
- production deployment/container architecture.

В этих случаях корректный outcome — остановка и escalation, а не самостоятельное расширение scope.