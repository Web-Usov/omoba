# Обзор архитектуры

> Статус: evolving. Этот документ описывает текущие высокоуровневые boundaries. Принятые фундаментальные решения перечислены здесь для навигации, а нормативным источником конкретного решения остаётся соответствующий Accepted ADR.

## Архитектурное направление

Open MOBA отделяет platform infrastructure от game-specific policy, чтобы официальную игру можно было реализовать через те же публичные extension mechanisms, что и community games.

Принятый foundation boundary:

```text
Godot 4.7.x .NET
Presentation / Client / Editor Shell
        |
        v
OpenMoba.Client.Godot
        |
        v
Engine-neutral shared contracts
        ^
        |
Standalone OpenMoba.Server (.NET 10)
        |
        +--> OpenMoba.Sim (plain .NET, no Godot dependency)
        +--> OpenMoba.ModRuntime
        +--> OpenMoba.Networking [transport/replication deferred]
        |
        v
Authoritative match process
```

Gameplay packages и reference game должны взаимодействовать с simulation через публичные contracts/capabilities. Конкретный scripting interpreter является adapter detail и не должен протекать в public Mod API.

## Принятые foundation decisions

- `ADR-001-godot-client-shell.md`: Godot 4.7.x .NET используется как presentation/client/editor shell, но не владеет authoritative gameplay simulation.
- `ADR-002-csharp-simulation-and-server.md`: authoritative simulation реализуется как plain C#/.NET libraries без Godot dependency; production dedicated server является standalone .NET process.
- `ADR-003-initial-mod-runtime.md`: Mod API является capability-based и interpreter-neutral; MoonSharp hard sandbox выбран как initial replaceable scripting adapter, а не permanent public runtime contract.

Shared libraries на foundation этапе используют `net8.0` compatibility surface для текущей Godot integration; standalone server и CLI ориентированы на .NET 10 LTS. Compatibility target должен пересматриваться отдельно и не является бессрочным platform contract.

## Желаемые свойства системы

### Headless-first simulation

Core gameplay simulation должна запускаться без graphical presentation layer. Это необходимо для dedicated servers, автоматических тестов, long-running simulations, benchmarks и agent-driven verification.

### Authoritative multiplayer

Server владеет authoritative gameplay state и обрабатывает client commands. Client input рассматривается как недоверенный intent; конкретные transport, replication и prediction strategies пока не определены.

### Публичные gameplay capabilities

Official/reference game не должна иметь privileged gameplay APIs. Если first-party gameplay требует скрытого engine hook, public Mod API считается недостаточным и должен быть расширен через обычный architecture/spec workflow.

### Многоуровневый creator experience

Со временем платформа должна поддерживать несколько уровней создания контента:

1. declarative data;
2. sandboxed scripting;
3. visual authoring tools, построенные поверх стабильных underlying representations.

Visual tooling не должен становиться отдельным gameplay runtime или обходить public contracts.

### Reusable packages

Gameplay capabilities должны быть composable, чтобы games могли переиспользовать platform и genre packages вместо копирования целых implementations.

Возможные будущие package families:

```text
@openmoba/core
@openmoba/combat
@openmoba/navigation
@openmoba/vision
@openmoba/projectiles
@openmoba/moba
@openmoba/rts
```

Названия и точные boundaries остаются иллюстративными, пока они не будут специфицированы и приняты.

## Решения, которые намеренно остаются открытыми

Foundation architecture пока НЕ определяет:

- ECS library или окончательную world/entity model;
- simulation tick rate и scheduling model;
- network transport;
- replication strategy;
- prediction, reconciliation и rollback;
- serialization format;
- package manifest/resolution format;
- окончательную public Mod API surface;
- visual editor technology/UX;
- Workshop/backend/persistence;
- production deployment infrastructure;
- конкретные gameplay systems reference MOBA.

Эти решения должны приниматься отдельными OpenSpec changes и ADR, когда появляется достаточный контекст и проверяемые требования.
