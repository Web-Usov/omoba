# Процесс разработки

Open MOBA разрабатывается docs-first и agent-first. Human owner определяет intent, ограничения и acceptance. Agents планируют, реализуют, проверяют и review'ят работу внутри этих границ.

## Роли

### Human owner

Отвечает за:

- product intent;
- priorities и scope;
- принятие архитектурных решений;
- approval high-impact designs;
- финальное принятие PR.

Human owner не должен вручную контролировать routine implementation details, если они уже ограничены принятыми specs, ADR, interfaces и tests.

### Execution agent

Отвечает за:

- чтение relevant source of truth до начала coding;
- implementation внутри утверждённого scope;
- tests и другую verification, требуемую change;
- обновление task status и implementation notes;
- выявление конфликтов вместо самостоятельного изменения product intent.

### Review agent

По возможности должен быть независим от execution pass. Он проверяет:

- implementation относительно утверждённых spec и design;
- нарушения architecture boundaries;
- отсутствующие tests и edge cases;
- scope creep;
- regressions и unsafe assumptions.

Review agent не заменяет human merge approval.

## Иерархия source of truth

При конфликте инструкций действует следующий порядок:

1. принятые product principles и vision;
2. принятые ADR;
3. актуальные OpenSpec specifications;
4. утверждённые specs/design активного change;
5. code и tests;
6. issues, PR discussion и context agent session.

Нижний уровень не имеет права молча переопределять верхний. Конфликт должен быть разрешён явно.

## Язык проекта

Canonical project documentation ведётся на русском языке.

На русском пишутся:

- product и architecture docs;
- OpenSpec proposals, specs, designs и tasks;
- ADR;
- существенные пояснения в PR, когда они являются частью долговременного контекста проекта.

На английском сохраняются:

- filenames и paths;
- code identifiers, type/class/function names;
- API и protocol identifiers;
- CLI commands;
- schema keys, enums и status values;
- устоявшиеся технические термины, если перевод снижает точность или читаемость.

Публичная англоязычная документация для внешних contributors может быть добавлена позже как отдельный слой. Она не должна становиться вторым независимым source of truth.

## Standard change flow

```text
idea
  |
  v
explore
  |
  v
proposal
  |
  v
[Intent Gate]
  |
  +------> delta specs
  |
  +------> design
             |
             v
        [Design Gate when required]
             |
             v
            tasks
             |
             v
           apply
             |
             v
           verify
             |
             v
         AI review
             |
             v
           PR
             |
             v
        [Merge Gate]
             |
             v
          archive
```

Названия OpenSpec artifacts и операций в diagram сохраняются на английском, поскольку это identifiers workflow.

## Политика gates

### Intent Gate

Обязателен, когда change изменяет behavior, capability scope, public contract, architecture, networking, security, persistence, compatibility или modding boundaries.

Approval означает, что owner согласен с:

- problem statement;
- scope;
- non-goals;
- affected capabilities;
- ожидаемым outcome.

### Design Gate

Обязателен для architecture-impacting changes и любых изменений, создающих или изменяющих фундаментальное техническое решение.

Approval означает согласие owner с:

- boundaries и ownership;
- interfaces и data flow;
- invariants;
- основными trade-offs;
- verification strategy;
- необходимыми ADR.

### Merge Gate

Обязателен для всех non-trivial PR.

PR должен содержать достаточно evidence, чтобы owner мог принять решение без воспроизведения всей agent session.

## Evidence в PR

Существенный PR должен содержать:

- ссылку на OpenSpec change;
- краткое описание delivered behavior;
- verification commands/results;
- добавленные или изменённые tests/benchmarks;
- влияние на architecture/ADR;
- deviations от утверждённого design, если они есть;
- known limitations и follow-up work.

## Fast lane

Полный OpenSpec change можно пропустить только для явно low-risk работы, которая не меняет behavior, architecture или public contracts.

Типичные fast-lane задачи:

- исправление опечаток и formatting;
- mechanical code cleanup без изменения behavior;
- исправления documentation, не переопределяющие requirements;
- узкий bug fix, ожидаемое поведение которого уже покрыто текущим spec и regression test.

Если implementation требует нового requirement, новой архитектурной assumption, нового public behavior или расширения scope, необходимо выйти из fast lane и создать OpenSpec change.

## Единица делегирования агенту

Task должна быть сформулирована так, чтобы execution agent получил одну ограниченную ответственность и мог самостоятельно доказать её завершение.

Хорошая task содержит:

- один ясный outcome;
- явные files/components или boundaries, если они известны;
- ссылки на relevant spec/design;
- acceptance criteria;
- verification command(s) или ожидаемое evidence;
- explicit non-goals, если есть риск scope expansion.

Следует избегать расплывчатых задач вроде `make networking better` или `finish combat`. Предпочтительны ограниченные формулировки вроде `implement fixed-tick SimulationClock with 30 Hz default and tests covering 300 ticks = 10 simulated seconds`.

## Stop conditions для агентов

Agent обязан остановиться и вынести конфликт на обсуждение, если:

- task противоречит принятому spec или ADR;
- требуемый результат расширяет утверждённый scope;
- необходимо новое фундаментальное архитектурное решение;
- текущий design не позволяет сделать verification надёжной;
- security или compatibility implications не были учтены в плане;
- implementation требует изменения product intent.

Остановка в таких случаях — корректное поведение, а не failure задачи.

## Дисциплина документации

Не следует создавать process artifacts без долговременной ценности. В repository не нужны meeting notes, implementation diaries, agent-thought logs и status reports.

Долговременная информация должна находиться в одном из мест:

- product/architecture docs;
- ADR;
- OpenSpec specs/changes;
- code/tests/tooling.

## Развитие workflow

Этот workflow намеренно минимален. Orchestration layers, custom OpenSpec schemas, дополнительные agent roles, dashboards и project-management tools следует добавлять только после появления конкретной coordination problem и доказанной пользы от её решения.
