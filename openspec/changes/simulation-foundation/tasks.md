# Tasks

## 1. Simulation foundation types and clock

- [ ] 1.1 Добавить в `OpenMoba.Sim` минимальные host-facing types из approved design: `SimulationConfig`, `SimulationTick`, `EntityId`, `CommandSequence` и foundation command/outcome/result/snapshot types без переноса их в `OpenMoba.Contracts`.
- [ ] 1.2 Реализовать `SimulationInstance` с initial tick `0` и explicit synchronous `Advance()`, где один вызов продвигает ровно один logical tick и не использует wall clock, timers, sleep или background thread.
- [ ] 1.3 Добавить tests: initial tick = 0; N вызовов `Advance()` дают tick N; advancement не требует real-time waiting.

## 2. Minimal authoritative world

- [ ] 2.1 Реализовать internal minimal entity registry без third-party ECS/component framework.
- [ ] 2.2 Реализовать non-zero monotonic `ulong` entity identity: первый ID = 1, IDs уникальны и не переиспользуются после destroy внутри одного simulation instance.
- [ ] 2.3 Реализовать read-only `SimulationSnapshot` с tick completed state и active entity IDs в ascending order без выдачи mutable internal collections.
- [ ] 2.4 Добавить tests на create/destroy lifecycle, unique/non-reused IDs, stable ascending snapshot ordering и невозможность изменить authoritative state через полученный snapshot.

## 3. Command intake and ordered outcomes

- [ ] 3.1 Реализовать только approved foundation commands: `CreateEntityCommand` и `DestroyEntityCommand`; не добавлять movement/combat/player/network/Mod API commands.
- [ ] 3.2 Реализовать synchronous `Submit()` с monotonic `CommandSequence` и FIFO pending queue; submit не должен мутировать world до следующего `Advance()`.
- [ ] 3.3 На `Advance()` обрабатывать snapshot текущей pending queue строго в submission order и возвращать step-scoped read-only ordered outcomes, привязанные к completed tick и originating command sequence.
- [ ] 3.4 Для destroy unknown/dead entity вернуть `CommandRejectedOutcome`/foundation rejection reason без exception и без partial authoritative mutation.
- [ ] 3.5 Добавить tests на no-mutation-before-advance, application-on-next-tick, FIFO нескольких commands, outcome ordering/tick/sequence и invalid destroy rejection без изменения state.

## 4. Deterministic RNG

- [ ] 4.1 Реализовать internal PCG32 (XSH-RR, 64-bit state / 32-bit output) без runtime dependency и без использования `System.Random` в deterministic foundation path.
- [ ] 4.2 Инициализировать RNG explicit seed из `SimulationConfig`; RNG state должен принадлежать конкретному `SimulationInstance`, а не process-global/static state.
- [ ] 4.3 Зафиксировать stream/initialization semantics в implementation и добавить pinned reference-vector test по PCG reference algorithm.
- [ ] 4.4 Добавить tests: одинаковый seed даёт одинаковую sequence, разные test seeds дают различающиеся sequences, независимые instances не разделяют RNG state.
- [ ] 4.5 Не добавлять искусственные random gameplay/entity properties только ради использования RNG в lifecycle commands.

## 5. Canonical determinism scenario

- [ ] 5.1 Создать headless canonical simulation scenario только из foundation operations: config/seed, ordered create/destroy commands, explicit advances, outcomes и snapshots.
- [ ] 5.2 Запустить scenario минимум дважды с одинаковыми input/seed и сравнить structured logical result напрямую без введения stable JSON/binary serialization или network/replay digest contract.
- [ ] 5.3 Добавить regression test, доказывающий equality supported result для одинакового scenario и сохраняющий ограниченную determinism boundary approved design.

## 6. Server composition smoke

- [ ] 6.1 Обновить existing `OpenMoba.Server --smoke`: создать реальный `SimulationInstance` с fixed test seed, submit `CreateEntityCommand`, выполнить один `Advance()` и проверить Tick 1 + create outcome + snapshot.
- [ ] 6.2 Сохранить existing machine-readable success marker и non-zero failure behavior; smoke не должен открывать sockets, ждать wall-clock, запускать background loop или требовать Godot.
- [ ] 6.3 Не превращать `--smoke` в stable production CLI/server API и не добавлять networking/matchmaking/session behavior.

## 7. Boundary and scope verification

- [ ] 7.1 Подтвердить existing architecture tests: `OpenMoba.Sim` не зависит от Godot/client/server adapters, Godot client не получает reference на `OpenMoba.Sim`, server продолжает reference `OpenMoba.Sim`.
- [ ] 7.2 Проверить, что `OpenMoba.Contracts` и `OpenMoba.Client.Godot` не расширены simulation-foundation API без отдельного approved change.
- [ ] 7.3 Проверить dependency set: не добавлены ECS, event-bus, DI/logging, serialization, networking, concurrency/job-system или RNG packages.
- [ ] 7.4 Проверить отсутствие deferred behavior: tick Hz/pacing, gameplay commands/events, components/systems, replication, rollback/replay, Mod API, movement/physics/navigation.

## 8. Documentation and final verification

- [ ] 8.1 Если implementation механически уточняет filenames/type names внутри approved semantics, отразить только durable clarification в relevant design/docs; любое изменение boundary остановить и эскалировать вместо самостоятельного redesign.
- [ ] 8.2 Выполнить OpenSpec validation: `openspec validate --all --strict --no-interactive` и `openspec validate --archived --no-interactive`.
- [ ] 8.3 Выполнить canonical core verification: `dotnet restore OpenMoba.sln`, Release build solution, отдельный Release build `OpenMoba.Sim`, `dotnet test OpenMoba.sln --configuration Release` и server `--smoke`.
- [ ] 8.4 Убедиться, что existing Godot Headless Smoke остаётся green как regression check, хотя client implementation не меняется.
- [ ] 8.5 Отметить checkbox выполненным только после фактической реализации и соответствующего verification evidence; перед передачей на review все completed tasks должны соответствовать реальному diff/test results.
