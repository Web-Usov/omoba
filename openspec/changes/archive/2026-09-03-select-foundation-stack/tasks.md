# Tasks: foundation stack

Этот change фиксирует архитектурное решение. Реализация code bootstrap намеренно вынесена в отдельный follow-up OpenSpec change.

## Architecture decision

- [x] Утвердить Godot 4.7.x .NET как presentation/client/editor shell.
- [x] Утвердить plain C#/.NET как runtime authoritative simulation без Godot dependency.
- [x] Утвердить standalone `OpenMoba.Server` как отдельный .NET process.
- [x] Утвердить `net8.0` compatibility target для shared libraries и .NET 10 LTS для standalone server/CLI.
- [x] Утвердить capability-based Mod API и replaceable scripting adapter.
- [x] Утвердить MoonSharp hard sandbox как initial scripting adapter, не как permanent public runtime contract.

## Specification

- [x] Описать `foundation-runtime` requirements.
- [x] Описать `simulation-hosting` requirements.
- [x] Описать `client-integration` requirements.
- [x] Описать `dedicated-server` requirements.
- [x] Описать `mod-runtime` requirements.
- [x] Описать `agent-verification` requirements.

## ADR

- [x] Принять `ADR-001-godot-client-shell.md`.
- [x] Принять `ADR-002-csharp-simulation-and-server.md`.
- [x] Принять `ADR-003-initial-mod-runtime.md`.

## Verification

- [x] Проверить, что design сохраняет server-authoritative и headless-first principles.
- [x] Проверить, что `OpenMoba.Sim` не получает dependency на Godot.
- [x] Проверить, что public Mod API не зависит от MoonSharp-specific types.
- [x] Проверить, что ECS, network transport, replication, tick rate и gameplay systems остаются вне scope.
- [x] Проверить, что architecture допускает CLI/headless verification для будущих implementation changes.

## Follow-up после merge

Следующий отдельный OpenSpec change: `bootstrap-repository`.

Follow-up не является delivery scope этого architecture change и не блокирует его archive.
