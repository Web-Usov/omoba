# Roadmap Open MOBA

Этот roadmap фиксирует последовательность **outcomes**, а не календарный план или полный backlog. Его задача — показывать, что платформа должна доказать на каждом этапе и какие решения ещё рано принимать.

Детальные requirements и implementation tasks принадлежат отдельным OpenSpec changes. Если roadmap конфликтует с Accepted ADR или current specs, нормативным источником остаются ADR/specs.

## 0. Foundation Governance — Done

**Доказано:** проект имеет versioned source of truth и воспроизводимый процесс изменений.

Exit criteria:

- Git является source of truth;
- product vision и project principles зафиксированы;
- `AGENTS.md` определяет provider-neutral contract для AI-агентов;
- OpenSpec задаёт standard change lifecycle и human gates;
- specification integrity проверяется CI.

## 1. Foundation Architecture — Done

**Доказано:** определены базовые runtime boundaries без преждевременного проектирования gameplay systems.

Exit criteria:

- Godot .NET принят как presentation/client/editor shell;
- authoritative simulation является plain C#/.NET и не зависит от Godot;
- dedicated server является standalone .NET process;
- public Mod API проектируется capability-based и interpreter-neutral;
- initial scripting adapter заменяем и sandboxed;
- ECS/world model, transport, replication, tick rate и gameplay architecture остаются deferred до отдельных changes.

## 2. Repository Bootstrap — Next

**Цель:** превратить принятые boundaries в минимальный собираемый repository skeleton.

Должно быть доказано из CLI/CI:

- существуют shared contracts, simulation library, standalone server/CLI и Godot client shell;
- `OpenMoba.Sim` build/test проходит без Godot;
- запрещённая dependency `OpenMoba.Sim -> Godot` обнаруживается автоматически;
- standalone server имеет headless smoke path;
- Godot project загружается headless;
- clean checkout можно проверить без ручных editor rituals.

Не входит в этот этап: выбор ECS, окончательной world model, network transport, replication, tick rate, rollback, gameplay systems или финальной Mod API surface.

## 3. Simulation Foundation — Planned

**Цель:** определить и реализовать минимальную engine-neutral модель authoritative simulation.

Этап должен отдельно решить и проверить необходимые contracts для:

- logical game clock и advancement simulation;
- world/entity representation;
- commands/orders и events;
- ownership состояния и lifecycle;
- reproducible RNG/determinism guarantees в необходимом объёме;
- automated headless simulation tests.

Конкретный ECS library или другой world model выбирается только при наличии отдельного принятого design.

## 4. Networking Vertical Slice — Planned

**Цель:** доказать server-authoritative multiplayer на минимальном сценарии.

Перед implementation отдельный architecture change должен определить transport/replication assumptions и verification strategy.

### Milestone A — Two circles over network

Milestone считается достигнутым, когда два клиента могут подключиться к standalone server, отправлять player intent и видеть authoritative movement двух простых entities, а сценарий воспроизводимо проверяется headless/automated средствами настолько, насколько это практично.

Этот milestone не требует полноценного combat, heroes, items или MOBA rules.

## 5. Combat + Public Mod API Vertical — Planned

**Цель:** доказать, что gameplay capability проходит через public creator boundary, а не через privileged first-party path.

### Milestone B — Hero shoots another hero

Минимальный результат:

- две игровые units/heroes существуют в authoritative simulation;
- одна применяет ability к другой;
- ability определена через public Mod API/content path;
- server authoritative вычисляет cast/result/damage;
- Godot client только отображает результат;
- reference content не использует private gameplay hooks;
- поведение покрыто automated verification.

## 6. Mini-MOBA Reference Slice — Planned

**Цель:** доказать, что из reusable platform capabilities можно собрать небольшую настоящую игру.

Ориентир scope, который уточняется отдельными changes:

- 1 карта;
- 2 команды, быстрый формат около 3v3;
- 3–4 героя;
- примерно 10–15 abilities;
- 6–10 items;
- creeps и structures/towers;
- respawn;
- base/objective и victory condition.

Это не content-production milestone. Приоритет — доказать platform boundaries и public Mod API.

## 7. External Creator Validation — Planned

**Цель:** проверить platform thesis человеком/агентом, который не меняет engine source.

Ключевой acceptance flow:

```text
install SDK
   -> create/extend package
   -> define hero / ability / item / game mode / map
   -> run locally
   -> start dedicated server
   -> friend joins
   -> play
```

Если для прохождения flow требуется private engine modification, public platform contract считается неполным.

## 8. Creator Tooling — Later

**Цель:** улучшить authoring experience после доказательства underlying contracts.

Последовательность остаётся:

```text
declarative data
   -> sandboxed scripting
   -> validation / packaging / CLI
   -> visual authoring on the same underlying representations
```

Visual tools не должны создавать отдельный privileged gameplay runtime.

## 9. Platform Alpha / Ecosystem — Later

**Цель:** перейти от одного reference game к reusable creator ecosystem.

К этому этапу относятся отдельные будущие decisions вокруг:

- package manifest, semantic versions и dependency resolution;
- reusable platform/genre/game packages;
- distribution/Workshop/registry;
- compatibility и update policy;
- discovery/moderation;
- security model для untrusted public content;
- hosting/backend services;
- creator monetization, если продуктовая модель её потребует.

## Как использовать roadmap

- Roadmap описывает **порядок доказательств**, а не список всех задач.
- Каждый существенный этап реализуется маленькими OpenSpec changes и vertical slices.
- Новая работа не должна затягивать deferred решения в более ранний этап «заодно».
- Статусы roadmap обновляются только после фактического merge соответствующих outcomes.
- Главный долгосрочный критерий остаётся неизменным: полноценный multiplayer gameplay должен создаваться и расширяться через публичные platform capabilities без изменения engine source.
