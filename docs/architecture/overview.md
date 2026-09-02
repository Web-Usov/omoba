# Обзор архитектуры

> Статус: evolving. Этот документ намеренно описывает только высокоуровневые boundaries, совместимые с текущим product vision. Конкретные технологические решения должны фиксироваться в ADR и пока здесь не считаются принятыми.

## Архитектурное направление

Open MOBA должна отделять platform infrastructure от game-specific policy, чтобы официальную игру можно было реализовать через те же публичные extension mechanisms, что и community games.

Предварительная модель boundaries:

```text
Presentation / Client
        |
        v
Public Client Integration
        |
        v
Simulation <----> Networking
        |
        v
Authoritative Server
        |
        v
Public Mod Runtime / SDK
        |
        v
Games and reusable gameplay packages
```

Точные технологии, process model, runtime boundaries и APIs внутри этих блоков должны быть определены отдельными архитектурными решениями до начала реализации.

## Желаемые свойства системы

### Headless-first simulation

Core gameplay simulation должна запускаться без графического presentation layer. Это необходимо для dedicated servers, автоматических тестов, long-running simulations, benchmarks и agent-driven verification.

### Authoritative multiplayer

Server владеет authoritative gameplay state и обрабатывает client commands. Network architecture с самого начала должна явно определять trust boundaries.

### Публичные gameplay capabilities

Официальная игра по возможности должна зависеть от публичных extension contracts, а не от privileged internal APIs.

### Многоуровневый creator experience

Со временем платформа должна поддерживать несколько уровней создания контента:

1. declarative data;
2. sandboxed scripting;
3. visual authoring tools, построенные поверх стабильных underlying representations.

### Reusable packages

Gameplay capabilities должны быть composable, чтобы игры могли переиспользовать platform и genre packages вместо копирования целых implementations.

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

Названия и boundaries являются иллюстративными, пока они не будут специфицированы и приняты.

## Решения, которые намеренно не принимаются этим документом

Этот overview пока не определяет:

- game engine;
- основной implementation language;
- ECS или альтернативную world model;
- язык или runtime для mod scripting;
- network transport;
- replication strategy;
- simulation tick rate;
- package manifest format;
- editor technology;
- persistence и backend services.

Каждое существенное решение должно проходить через spec-driven workflow проекта и фиксироваться в ADR, когда это уместно.
