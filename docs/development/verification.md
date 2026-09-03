# Verification

Этот документ фиксирует required toolchain и canonical CLI commands для проверки repository bootstrap. Он не является implementation diary.

## Required toolchain

- .NET SDK `10.0.400` (`rollForward: latestPatch`) по `global.json`
- .NET 8 targeting pack/runtime для shared assemblies (`net8.0`)
- Godot **4.7.2 .NET/Mono** binary только для client smoke
- OpenSpec CLI `@fission-ai/openspec@1.11.0` для validation specs/changes

Godot binary не коммитится в repository. Export templates для bootstrap не требуются.

## Core checks (без Godot binary)

```bash
openspec validate --all --strict --no-interactive
openspec validate --archived --no-interactive

dotnet restore OpenMoba.sln
dotnet build OpenMoba.sln --configuration Release
dotnet build src/OpenMoba.Sim/OpenMoba.Sim.csproj --configuration Release
dotnet test OpenMoba.sln --configuration Release
dotnet run --project src/OpenMoba.Server --configuration Release -- --smoke
```

`OpenMoba.Cli` собирается вместе с solution; его placeholder output не является stable public command contract.

## Client smoke (требует pinned Godot 4.7.2 .NET)

```bash
dotnet build src/OpenMoba.Client.Godot/OpenMoba.Client.Godot.csproj --configuration Release
godot --headless --path src/OpenMoba.Client.Godot
```

Используйте именно .NET/Mono Godot binary версии 4.7.2. Имя executable может отличаться по OS; важно совпадение версии и Mono/.NET edition.

## CI equivalence

GitHub Actions workflow `Build` выполняет:

1. `.NET Core` job — restore/build/tests/server smoke и отдельную сборку `OpenMoba.Sim` без Godot binary.
2. `Godot Headless Smoke` job — скачивает официальный `Godot_v4.7.2-stable_mono_linux_x86_64.zip`, проверяет `SHA512-SUMS.txt`, собирает client и запускает headless project load.

Workflow `Spec Integrity` сохраняется отдельно и продолжает проверять OpenSpec.
