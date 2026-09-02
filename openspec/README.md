# OpenSpec в Open MOBA

OpenSpec — planning и specification layer для нетривиальных изменений в этом repository. Он хранит product intent, behavioral requirements, design decisions, implementation tasks и поставленные specifications рядом с кодом и под version control.

Repository использует встроенную schema `spec-driven`:

`proposal -> specs + design -> tasks -> apply -> verify -> archive`

## Source of truth

Git является source of truth. История чатов, IDE sessions, agent memory, issue comments и внешние project-management tools могут помогать координации, но долговременные решения должны попадать в repository как одно из следующего:

- product/architecture documentation;
- ADR;
- OpenSpec specification/change;
- code и automated verification.

## Локальная настройка

OpenSpec требует Node.js 20.19.0 или новее.

```bash
npm install -g @fission-ai/openspec@latest
openspec --version
```

Инициализация или обновление интеграции агента из корня repository:

```bash
openspec init
# позже, после обновления OpenSpec:
openspec update
```

Следует выбирать только те AI tools, которые реально используются на машине. Сгенерированные tool-specific skills/commands коммитятся, когда проект фактически принимает соответствующий execution tool.

## Жизненный цикл change

Для значимых behavioral или architecture changes:

1. **Explore** — понять проблему, текущие specs, ADR, ограничения и alternatives.
2. **Propose** — создать OpenSpec change и planning artifacts.
3. **Review** — human owner проверяет intent и design до начала implementation.
4. **Apply** — agent реализует утверждённые tasks.
5. **Verify** — automated checks и независимый review подтверждают соответствие implementation spec.
6. **PR review** — human owner принимает или отклоняет поставленный change.
7. **Archive** — только merged и verified behavior переносится в current specs.

## Human approval gates

Open MOBA намеренно оставляет human approvals редкими, но значимыми.

### Gate 1 — Intent

Требуется до implementation, если change затрагивает product behavior, public APIs, architecture, security, networking, persistence, modding boundaries, compatibility или scope.

Owner подтверждает корректность problem, scope и non-goals.

### Gate 2 — Design

Требуется до implementation для architecture-impacting changes.

Owner подтверждает system boundaries, invariants, interfaces, alternatives и verification strategy. Если design требует нового фундаментального решения, до coding необходимо создать или обновить ADR.

### Gate 3 — Merge

Требуется для каждого non-trivial PR.

Owner проверяет delivered behavior, verification evidence, deviations и architectural impact до merge.

## Standard lane

Полный OpenSpec change обязателен, если выполняется хотя бы одно условие:

- изменяется externally observable behavior;
- изменяется public Mod API или SDK contract;
- меняются engine/platform boundaries;
- затронуты networking, security, persistence, compatibility или sandboxing;
- добавляется новая capability;
- затрагиваются несколько subsystems;
- может потребоваться ADR.

## Fast lane

Полный change необязателен для действительно mechanical работы, которая не меняет behavior или architecture, например:

- typo или documentation-only correction;
- formatting/lint-only change;
- mechanical refactor, покрытый существующими specs/tests;
- узкий bug fix, требуемое поведение которого уже однозначно описано.

При сомнении используется standard lane.

## Правила для агентов

- Нельзя считать implementation convenience разрешением менять product intent.
- Нельзя молча обходить принятый ADR.
- Нельзя расширять scope только потому, что соседняя работа кажется полезной.
- Если implementation противоречит spec, design, invariant или ADR, следует обновить change и получить необходимое approval, а не импровизировать.
- Следует предпочитать requirements, которые можно проверить tests, simulation, benchmarks, validation tools или другим machine-checkable evidence.

## Текущая структура

```text
openspec/
├── config.yaml
├── README.md
├── specs/          # Актуальное поведение системы
└── changes/        # Активные и архивированные изменения
```

Custom OpenSpec schemas намеренно отложены. Проект должен создавать собственную schema только после того, как встроенный `spec-driven` workflow продемонстрирует конкретное ограничение.
