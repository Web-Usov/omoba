# Open MOBA

Open MOBA — модифицируемая платформа для многопользовательских игр, в которой официальная игра является reference implementation и создаётся на тех же публичных API, которые доступны community creators.

Проект строится вокруг трёх ключевых идей:

- **Сначала платформа, потом игра** — базовая MOBA должна подтверждать состоятельность платформы, а не обходить её ограничения.
- **Modding по умолчанию** — правила игры, герои, способности, предметы, карты и game modes должны расширяться без изменения исходного кода engine.
- **Agent-first разработка** — архитектура, документация, тесты и workflow устроены так, чтобы разработку можно было безопасно делегировать AI-агентам, при этом человек сохраняет ответственность за product intent и финальное принятие результата.

## Статус проекта

Open MOBA находится на этапе foundation. Сейчас основной фокус — зафиксировать product vision, архитектурные принципы и spec-driven workflow до начала реализации.

## Документация

- [`docs/vision/product-vision.md`](docs/vision/product-vision.md) — product vision и долгосрочное направление.
- [`docs/vision/principles.md`](docs/vision/principles.md) — фундаментальные принципы проекта.
- [`docs/architecture/overview.md`](docs/architecture/overview.md) — развивающийся обзор архитектуры.
- [`docs/adr/`](docs/adr/) — Architecture Decision Records.
- [`openspec/specs/`](openspec/specs/) — актуальные спецификации системы.
- [`openspec/changes/`](openspec/changes/) — предлагаемые и находящиеся в работе изменения.

## Модель разработки

Git является source of truth. Долговременные знания проекта должны находиться в versioned documentation, specs, ADR, коде и тестах, а не в истории чатов или состоянии конкретного AI-инструмента.
