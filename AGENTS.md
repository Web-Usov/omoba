# AGENTS.md

Этот файл задаёт единый контракт поведения для любых AI-агентов, работающих в репозитории Open MOBA: coding agents, review agents, research agents и orchestration tools.

Файл намеренно tool-neutral. Он не зависит от Kiro, Codex, Claude, Cursor или другого конкретного продукта.

## 1. Главный принцип

Агент реализует и проверяет изменения внутри уже утверждённого intent. Агент не имеет права самостоятельно менять product intent, архитектурные принципы или публичные контракты ради удобства реализации.

Человек-владелец проекта отвечает за:

- product direction;
- scope и priorities;
- принятие архитектурных решений;
- approval на обязательных human gates;
- финальный merge.

## 2. Source of truth

При конфликте источников использовать следующий приоритет:

1. `docs/vision/` — product vision и project principles;
2. принятые ADR в `docs/adr/`;
3. текущие specs в `openspec/specs/`;
4. утверждённые artifacts активного OpenSpec change;
5. code и automated tests;
6. GitHub Issues, PR discussion и текущий agent context.

Нижестоящий источник не должен молча переопределять вышестоящий.

Chat history, память модели, IDE session и внешние project-management tools не являются authoritative source.

## 3. Что прочитать перед работой

Перед изменением кода агент обязан прочитать только релевантный минимум source of truth.

Для любой нетривиальной задачи:

- `docs/vision/principles.md`;
- `docs/development/workflow.md`;
- релевантные current specs;
- релевантные ADR;
- artifacts активного OpenSpec change, если он существует.

Для architecture-impacting work дополнительно прочитать `docs/architecture/overview.md`.

Не требуется загружать всю документацию проекта, если она не относится к задаче.

## 4. Выбор change lane

Использовать правила из `docs/development/workflow.md`.

### Standard lane

Нужен OpenSpec change, если изменение:

- добавляет или меняет observable behavior;
- меняет public API, Mod API или SDK contract;
- затрагивает architecture boundaries;
- касается networking, security, sandboxing, persistence или compatibility;
- вводит новую capability;
- требует нового фундаментального технического решения.

### Fast lane

Допустим только для явно низкорисковых изменений без нового behavior или architecture decision, например:

- typo/formatting;
- механический refactor при неизменном behavior;
- documentation correction без изменения requirements;
- узкий bug fix, если ожидаемое поведение уже однозначно задано существующим spec и regression test.

Если есть сомнение — использовать standard lane.

## 5. Human gates

Агент не должен обходить human approval gates.

### Intent Gate

До implementation должен быть утверждён problem, scope, non-goals и ожидаемый outcome для изменений, где это требуется workflow.

### Design Gate

До implementation architecture-impacting change должен иметь утверждённый design.

Если design требует нового фундаментального решения, должен быть создан ADR со статусом `Proposed`, а coding не должен превращать это решение в фактическую архитектуру до owner approval.

### Merge Gate

Агент может подготовить branch, commits, verification evidence и PR, но не должен самостоятельно считать нетривиальную задачу принятой или обходить финальное human approval.

## 6. Scope discipline

Агент обязан выполнять только approved scope.

Запрещено:

- добавлять adjacent features «заодно»;
- проводить широкий refactor без необходимости для acceptance criteria;
- менять public behavior ради упрощения implementation;
- заменять утверждённое архитектурное решение альтернативой без обновления design/ADR;
- создавать новые platform capabilities, которых нет в approved change.

Если во время работы обнаружена полезная несвязанная задача, зафиксировать её как follow-up, но не включать в текущий implementation без approval.

## 7. Архитектурные инварианты

До появления явно принятых ADR считать действующими следующие project principles:

- platform first, game second;
- official/reference game использует тот же public Mod API, что и внешние creators;
- server authoritative by default;
- simulation отделена от presentation и должна поддерживать headless execution;
- moddability является architectural requirement;
- immutable core должен оставаться минимальным;
- creator tooling строится поверх стабильных public representations/API, а не наоборот;
- Git является source of truth;
- development infrastructure должна оставаться replaceable и vendor-neutral, где это практично.

Агент не должен превращать предварительные идеи из discussion в окончательные technology choices без ADR.

## 8. Implementation rules

При реализации агент должен:

- предпочитать минимальное изменение, достаточное для acceptance criteria;
- сохранять существующие boundaries и public contracts;
- использовать явные interfaces и contracts вместо скрытой связанности;
- не смешивать platform policy и reference-game policy без необходимости;
- избегать vendor-specific coupling в durable project knowledge;
- писать код так, чтобы его можно было собирать и проверять через CLI/headless workflow, когда это возможно;
- не добавлять dependency без объяснимой необходимости;
- не скрывать ошибки или failing checks отключением validation/tests.

## 9. Verification first

Работа не считается выполненной только потому, что код написан.

Каждая implementation task должна иметь воспроизводимое evidence корректности, подходящее для её типа:

- unit tests;
- integration tests;
- headless simulation;
- protocol/contract tests;
- validation commands;
- benchmarks;
- static analysis/lint/build;
- regression test для bug fix.

Агент обязан запустить strongest available verification, относящийся к изменению.

Если acceptance criterion нельзя надёжно проверить автоматически, агент должен:

1. явно указать это;
2. предоставить лучший воспроизводимый manual/inspection procedure;
3. не выдавать непроверенный результат за доказанный.

Не исправлять failing test удалением, ослаблением assertion или исключением test case, если change не меняет соответствующее требование.

## 10. Stop conditions

Агент обязан остановить implementation и сообщить о блокере, если:

- task противоречит accepted spec или ADR;
- необходим выход за approved scope;
- требуется новое фундаментальное architecture decision;
- обнаружены неучтённые security/compatibility implications;
- verification невозможно сделать надёжной в рамках утверждённого design;
- реализация требует изменения product intent;
- два authoritative sources противоречат друг другу.

Остановка в этих случаях считается корректным поведением агента, а не failure.

## 11. Documentation discipline

Canonical project documentation ведётся на русском языке.

Английскими остаются:

- filenames и paths;
- code identifiers;
- API/protocol identifiers;
- CLI commands;
- schema keys и enum values;
- общепринятые технические термины, если перевод ухудшает точность.

Не создавать repository-документы вида:

- agent thoughts;
- chain-of-thought logs;
- meeting diary;
- daily status report;
- implementation journal.

Durable knowledge должно попадать в один из существующих источников: vision/architecture docs, ADR, OpenSpec, code, tests или tooling.

## 12. Git и branch policy

Если задача выполняется через Git:

- не работать напрямую в `main` для нетривиальных изменений;
- использовать отдельную branch с узким назначением;
- commits должны описывать фактическое изменение;
- не force-push чужую работу;
- не переписывать unrelated files;
- не merge нетривиальный PR без human Merge Gate;
- не использовать history rewrite как способ скрыть неудачные решения или verification failures.

## 13. PR contract

Нетривиальный PR должен позволять owner принять решение без чтения всей agent session.

PR должен содержать:

- кратко: что изменено и зачем;
- ссылку/указание на OpenSpec change, если применимо;
- scope и существенные non-goals;
- verification commands и результаты;
- какие tests/benchmarks добавлены или изменены;
- влияние на architecture/ADR;
- deviations от approved design;
- known limitations и follow-up work.

Не скрывать известные ограничения за формулировкой «done».

## 14. Review-agent contract

Review agent должен проверять результат независимо от implementation narrative.

Приоритет проверки:

1. соответствие approved requirements/spec;
2. нарушение архитектурных invariants и boundaries;
3. correctness и edge cases;
4. security/trust boundaries;
5. test quality и regression risk;
6. scope creep;
7. maintainability.

Review agent не должен одобрять изменение только потому, что tests зелёные, если implementation нарушает spec или ADR.

## 15. Tool-specific instructions

Инструкции конкретного execution tool могут существовать дополнительно, но они не должны противоречить этому файлу.

Если tool-specific instruction конфликтует с `AGENTS.md`, project specs или accepted ADR, агент должен следовать project source of truth и явно сообщить о конфликте.

## 16. Definition of done для агента

Перед передачей результата на review агент должен убедиться, что:

- approved scope реализован;
- acceptance criteria покрыты;
- relevant verification выполнена;
- tests/build не сломаны изменением;
- docs/spec/ADR обновлены там, где это требуется;
- нет скрытого scope creep;
- известные deviations и limitations задокументированы;
- PR содержит достаточное evidence для Merge Gate.
