# OpenSpec Changes

Этот каталог содержит предлагаемые, активные и архивированные changes проекта.

Для нетривиального изменения используй отдельный каталог с понятным kebab-case именем:

```text
openspec/changes/<change-name>/
```

В стандартном `spec-driven` workflow change проходит через artifacts:

```text
proposal -> specs + design -> tasks -> apply -> verify -> archive
```

Implementation не должна начинаться до прохождения обязательных human gates, описанных в [`docs/development/workflow.md`](../../docs/development/workflow.md).

После merge и verification change архивируется, а поставленное поведение должно быть отражено в `openspec/specs/`.

Этот каталог не является backlog. Идеи, которые ещё не стали конкретным change, не должны храниться здесь как псевдоспецификации.
