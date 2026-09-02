# Design: foundation stack Open MOBA

> Статус: Accepted. Design Gate пройден 2026-09-03.

## 1. Decision summary

Принятый foundation stack:

```text
Godot 4.7.x .NET
  presentation / client / editor shell
          |
          v
OpenMoba.Client.Godot
  engine adapter, input, rendering, UI
          |
          v
OpenMoba.Contracts (net8.0-compatible)

OpenMoba.Server (net10.0)
          |
          +--> OpenMoba.Sim (net8.0)
          +--> OpenMoba.ModRuntime
          +--> OpenMoba.Networking   [transport/replication deferred]
          |
          v
authoritative match process
```

Основные решения:

1. Godot используется как presentation/client/editor shell, но не как owner gameplay simulation.
2. Authoritative simulation реализуется на C# как plain .NET libraries без зависимости от Godot.
3. Shared libraries, которые должны загружаться и Godot client-side adapter, ограничиваются `net8.0`-совместимой API surface.
4. Standalone server и repository CLI используют .NET 10 LTS.
5. Dedicated server является отдельным .NET process и не требует Godot runtime.
6. Initial scripting adapter — MoonSharp hard sandbox поверх capability-based Mod API; конкретный interpreter не является public architecture contract.
7. Network transport, ECS/world model, tick rate и replication остаются отдельными решениями.

## 2. Research snapshot — 2026-09-03

### Godot

- Current stable: Godot 4.7.2 (18 August 2026).
- Есть официальная .NET edition с C# support.
- Godot 4.7 использует .NET 8 для C# integration.
- C# exports поддерживают desktop; Web export для Godot 4 C# сейчас отсутствует.
- Godot поддерживает `--headless`, CLI export и dedicated-server export.
- `.tscn` — text scene format, рассчитанный в том числе на version control.
- Engine распространяется по MIT license.

### .NET

- .NET 10 — active LTS до 14 November 2028.
- .NET 8 заканчивает поддержку 10 November 2026.

Отсюда следует разделение: compatibility-facing shared libraries могут пока target `net8.0`, но standalone hosts должны работать на .NET 10.

### Alternative engines

Unity 6 отменил Runtime Fee, но остаётся proprietary ecosystem с plan/revenue policy. Unreal даёт source access и сильный multiplayer toolchain, но стандартная game license предусматривает royalty после $1M lifetime gross product revenue и повышает стоимость C++/editor-heavy workflow.

## 3. Engine/client choice

### Выбор: Godot .NET

Godot отвечает требованиям Open MOBA лучше альтернатив на foundation этапе:

- permissive MIT license и полный source access;
- небольшой operational footprint;
- CLI/headless workflows;
- text-friendly scenes/resources;
- официальный C# integration;
- достаточно зрелый desktop renderer/editor/UI/animation pipeline;
- отсутствие необходимости строить свой editor/rendering stack.

### Что Godot владеет

- rendering;
- scene/presentation graph;
- camera;
- input capture;
- UI;
- animation;
- VFX/audio;
- asset import;
- map/editor tooling;
- client lifecycle.

### Чем Godot не владеет

- authoritative health/damage/cooldowns;
- game rules;
- authoritative movement validation;
- match state;
- mod execution policy;
- server simulation clock;
- authoritative entity/world model.

Godot objects (`Node`, `Resource`, `Vector*` и т. п.) не должны протекать в `OpenMoba.Sim` public/internal core contracts.

## 4. Simulation runtime

### Выбор: C# / plain .NET

`OpenMoba.Sim` проектируется как обычная .NET class library.

Причины:

- type safety и mature tooling;
- быстрые CLI build/test loops;
- хорошая поддержка AI coding agents;
- общий язык с Godot client integration;
- простой standalone server hosting;
- достаточный performance headroom до появления evidence, требующего native subsystem.

### Target framework strategy

Foundation baseline:

```text
OpenMoba.Contracts       net8.0
OpenMoba.ModApi.Contracts net8.0
OpenMoba.Sim             net8.0
OpenMoba.Server          net10.0
OpenMoba.Cli             net10.0
OpenMoba.Client.Godot    Godot .NET / compatible with shared net8.0 libraries
```

`net8.0` здесь является compatibility target для shared assemblies, а не рекомендацией запускать production server на .NET 8. `OpenMoba.Server` исполняет эти libraries под .NET 10.

Когда Godot перейдёт на новый runtime baseline, shared target может быть поднят отдельным compatibility change.

## 5. Dedicated server boundary

### Выбор: standalone .NET host

Authoritative server не запускается внутри Godot как основной production architecture.

```text
OpenMoba.Server
  owns process lifecycle
  owns match hosting
  owns authoritative clock/tick host
  loads OpenMoba.Sim
  loads ModRuntime
  composes Networking adapters
```

Godot headless остаётся полезным для client/editor integration tests и может использоваться как экспериментальный host, но core server не должен от него зависеть.

Это сохраняет возможность:

- Linux containers без renderer/editor;
- быстрых server tests;
- simulation benchmarks;
- mass bot/simulation runs;
- будущего server scaling без game-engine process overhead.

## 6. Dependency direction

Допустимое направление:

```text
                 +----------------------+
                 | OpenMoba.Contracts   |
                 +----------+-----------+
                            ^
             +--------------+--------------+
             |                             |
+------------+-----------+      +----------+-----------+
| OpenMoba.Sim           |      | Client/Networking    |
| no Godot dependency    |      | adapters             |
+------------+-----------+      +----------+-----------+
             ^                             ^
             |                             |
+------------+-----------+      +----------+-----------+
| OpenMoba.Server        |      | OpenMoba.Client.Godot|
+------------------------+      +----------------------+
```

`OpenMoba.ModApi.Contracts` является отдельным inward-facing contract. `ModRuntime` реализует binding к нему, а gameplay packages видят только разрешённые capabilities.

Запрещённые зависимости:

```text
OpenMoba.Sim -> Godot
OpenMoba.Sim -> OpenMoba.Server
OpenMoba.Sim -> concrete network transport
Mod scripts -> arbitrary CLR/Godot/OS objects
reference game -> privileged internal engine API
```

## 7. Initial mod runtime

### Public architecture contract

Фундаментальным решением является не конкретная Lua library, а boundary:

```text
mod script
    |
    v
Sandboxed Script Runtime Adapter
    |
    v
OpenMoba.ModApi capability surface
    |
    v
Simulation
```

Mods не получают прямой доступ к CLR objects, filesystem, network, environment variables или game-engine objects.

### Initial adapter: MoonSharp

Для первой реализации выбран MoonSharp, потому что он:

- полностью managed;
- документирует hard sandbox presets;
- позволяет исключать `io`, `os`, `file` и dangerous modules;
- позволяет запретить automatic CLR interop;
- поддерживает custom script loaders;
- предоставляет механизм ограничения выполняемых instructions через preemptive coroutines.

Ограничение: MoonSharp в основном совместим с Lua 5.2, поэтому foundation НЕ обещает стабильный Lua language version как public contract.

До публичного Workshop отдельный security/runtime change обязан повторно сравнить:

- MoonSharp current release;
- native Lua 5.4 через KeraLua/NLua;
- WASM runtime;
- при необходимости process isolation.

Нельзя считать initial in-process sandbox достаточной защитой для произвольного adversarial Workshop content без отдельного threat model.

## 8. Agent-first verification contract

Foundation code bootstrap обязан поддерживать минимум:

```bash
dotnet build
dotnet test
```

и headless checks для Godot project.

Core acceptance не должна зависеть от ручного открытия editor.

Для первого code bootstrap нужны проверки:

1. `OpenMoba.Sim` собирается и тестируется без Godot.
2. Architecture test подтверждает отсутствие Godot references в simulation assemblies.
3. `OpenMoba.Server` запускается headless из CLI и корректно завершается в smoke mode.
4. Godot .NET project загружается через CLI/headless mode.
5. Client adapter может ссылаться на shared contracts.
6. CI может выполнить все перечисленные checks без GUI interaction.

Unified `openmoba` CLI остаётся желаемым последующим tooling layer; foundation не требует писать собственный orchestration framework.

## 9. Alternatives considered

### Unity + C# everywhere

Плюсы: сильный C# ecosystem, зрелый editor, большой asset/tool market.

Не выбран: proprietary ownership/licensing, более высокий vendor coupling и меньше контроля над engine lifecycle. Отмена Runtime Fee снимает важный риск, но не устраняет lock-in.

### Unreal Engine + C++/Blueprint

Плюсы: сильный renderer/networking/dedicated-server ecosystem, source access.

Не выбран: существенно более тяжёлый editor/build workflow, C++/Blueprint split хуже соответствует solo + agent-first iteration, royalty model и избыточность для top-down platform MVP.

### Custom renderer/engine (SDL/MonoGame/raylib-like)

Плюсы: максимальный control, простой dependency graph.

Не выбран: пришлось бы строить renderer, scene/editor, import pipeline, animation/VFX/UI tooling — это противоречит цели создать MOBA platform, а не general game engine.

### Godot + GDScript authoritative simulation

Плюсы: максимальная интеграция с Godot и быстрый prototyping.

Не выбран: связывает simulation с game-engine runtime и ухудшает standalone server/testing/reuse boundaries.

### Native C++/Rust simulation

Плюсы: высокий performance ceiling.

Не выбран сейчас: FFI, memory/toolchain complexity и второй основной systems language без доказанной performance необходимости. Native subsystem допустим позже по benchmark evidence.

### Godot headless как основной server

Плюсы: минимальный initial integration cost.

Не выбран как production boundary: делает authoritative server зависимым от Godot lifecycle и размывает правило simulation independence.

## 10. Known trade-offs

1. Godot C# сейчас исключает Web client как immediate target.
2. Shared `net8.0` target временно ограничивает доступную BCL surface.
3. Standalone server + Godot client требует явных contracts/adapters вместо прямого shared scene logic.
4. MoonSharp не должен становиться незаметным permanent lock-in.
5. Мы сознательно не оптимизируем под ultra-high entity count до benchmarks.

Эти trade-offs приемлемы для desktop-first Milestone A.

## 11. ADRs

Design сопровождается тремя Accepted ADR:

- `ADR-001-godot-client-shell.md`;
- `ADR-002-csharp-simulation-and-server.md`;
- `ADR-003-initial-mod-runtime.md`.

Design Gate пройден; ADR приняты owner и имеют статус `Accepted`.

## 12. References

- https://godotengine.org/download/archive/4.7.2-stable/
- https://godotengine.org/license/
- https://docs.godotengine.org/en/4.7/getting_started/step_by_step/scripting_languages.html
- https://docs.godotengine.org/en/stable/tutorials/export/exporting_for_dedicated_servers.html
- https://docs.godotengine.org/en/stable/engine_details/file_formats/tscn.html
- https://dotnet.microsoft.com/en-us/platform/support/policy
- https://unity.com/releases/unity-6
- https://www.unrealengine.com/license
- https://www.moonsharp.org/sandbox.html
- https://github.com/NLua/KeraLua
