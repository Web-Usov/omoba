# Принципы проекта

Эти принципы направляют продуктовые, архитектурные и инженерные решения Open MOBA. Их намеренно немного; изменять их следует только при наличии веской причины пересмотреть фундамент проекта.

## 1. Сначала платформа, потом игра

Платформа — основной продукт. Официальная MOBA существует для проверки платформы и не должна обходить её через privileged gameplay hooks.

## 2. Официальная игра использует публичный Mod API

Если reference game требует возможностей, недоступных сторонним creators, публичный API неполон. Официальный gameplay должен использовать те же APIs, package boundaries и workflows, которые доступны внешним разработчикам.

## 3. Server-authoritative по умолчанию

Клиент передаёт намерение игрока. Authoritative server владеет игровым состоянием и определяет gameplay outcomes, если отдельное принятое архитектурное решение явно не устанавливает иное.

## 4. Simulation не зависит от presentation

Gameplay simulation не должна зависеть от rendering, состояния редактора или графической среды. Headless simulation должна быть возможна для dedicated servers, автоматических тестов, benchmarks и verification AI-агентами.

## 5. Moddability — архитектурное требование

Герои, способности, предметы, units, game modes, карты, правила и reusable gameplay packages должны проектироваться с расчётом на расширение, а не быть hard-coded под первую игру.

## 6. Agent-first разработка

Изначально проект разрабатывается одним человеком-оркестратором с AI-агентами. Предпочтение отдаётся text-based, versioned, CLI-accessible и testable workflows, которые агенты способны выполнять и проверять без хрупких ручных процедур в редакторе.

## 7. Git — source of truth

Долговременные знания должны находиться в repository: specs, ADR, архитектурной документации, коде, тестах и review history. История чатов, память модели, состояние IDE и vendor-specific agent sessions не являются authoritative knowledge проекта.

## 8. Изменения должны быть machine-verifiable, когда это практично

Requirements должны приводить к объективным acceptance criteria везде, где это возможно. Агент должен иметь возможность доказать качество реализации тестами, headless simulations, validation commands, benchmarks или другими воспроизводимыми проверками.

## 9. Не строить creator tooling преждевременно

Стабильные underlying APIs важнее дорогих визуальных редакторов. Declarative formats, scripting, validation и CLI workflows должны сначала доказать creator model; node editors, terrain tooling и Workshop UX появляются позже.

## 10. Предпочитать заменяемую инфраструктуру разработки

Specs и знания проекта должны оставаться полезными при смене coding agents, IDE, orchestration tools, CI vendors или hosting providers. Нельзя привязывать ключевые знания проекта к одному AI-продукту.

## 11. Immutable core должен быть небольшим

На уровне engine должны находиться только возможности, которые сложно, небезопасно или неэффективно реализовывать как gameplay content. Game-specific policy по возможности принадлежит публичным packages и mods.

## 12. Явные решения лучше случайной архитектуры

Дорогие, cross-cutting или труднообратимые технические решения должны фиксироваться в Architecture Decision Records до того, как превратятся в скрытые assumptions внутри codebase.
