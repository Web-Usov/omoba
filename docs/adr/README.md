# Architecture Decision Records

Architecture Decision Records (ADR) фиксируют существенные технические решения, которые являются дорогими, cross-cutting, security-sensitive или труднообратимыми.

ADR должен объяснять:

- context и проблему;
- принятое решение;
- рассмотренные alternatives;
- consequences и trade-offs;
- статус решения.

ADR — не implementation journal. Небольшие локальные решения должны оставаться в коде, тестах или design-документах конкретного change.

## Статусы

Используются значения:

- `Proposed`
- `Accepted`
- `Superseded`
- `Deprecated`

## Именование

```text
ADR-001-short-decision-title.md
ADR-002-another-decision.md
```

Технологические варианты, обсуждавшиеся до принятия ADR, не считаются финальной архитектурой проекта.
