# ADR-002: C#/.NET simulation и standalone server

- **Status:** Accepted
- **Date:** 2026-09-03

## Context

Open MOBA требует authoritative simulation, которую можно тестировать headless, запускать в dedicated server и развивать независимо от presentation engine. Solo + agent-first workflow требует predictable CLI tooling и минимального числа основных implementation languages.

## Decision

Authoritative simulation реализуется на C# как plain .NET libraries без Godot dependency.

Foundation targets:

```text
OpenMoba.Contracts        net8.0
OpenMoba.ModApi.Contracts net8.0
OpenMoba.Sim              net8.0
OpenMoba.Server           net10.0
OpenMoba.Cli              net10.0
```

`net8.0` для shared libraries — compatibility target с текущим Godot .NET runtime. Standalone server/CLI исполняются под .NET 10 LTS.

`OpenMoba.Server` является отдельным authoritative process и композирует simulation, mod runtime и networking adapters. Production server не требует Godot.

## Rationale

- один основной compiled language для core и client adapter;
- mature compiler/test/profiling ecosystem;
- эффективное agent delegation через CLI;
- достаточный performance headroom для MVP;
- standalone Linux server без renderer;
- возможность позже вынести конкретный hotspot в native subsystem по benchmark evidence.

## Alternatives

### GDScript simulation

Быстро для Godot prototyping, но связывает authoritative core с engine runtime.

### C++ simulation

Высокий performance ceiling, но увеличивает memory-safety/toolchain cost и agent review burden без текущего evidence необходимости.

### Rust simulation

Хорошая safety/performance модель, но добавляет второй systems ecosystem и FFI boundary слишком рано.

### Godot headless как основной server

Работоспособен технически, но делает server lifecycle зависимым от presentation engine и ослабляет architectural isolation.

## Consequences

- simulation должна использовать engine-neutral primitives/contracts;
- standalone server получает простой container/headless path;
- shared libraries временно ограничены `net8.0` API surface;
- runtime target shared libraries следует пересмотреть после обновления Godot baseline;
- network transport и replication не определяются этим ADR.

## Revisit when

- profiling показывает, что managed simulation не удовлетворяет target workload;
- Godot runtime baseline позволяет безопасно поднять shared TFM;
- deployment constraints требуют иной server process model.
