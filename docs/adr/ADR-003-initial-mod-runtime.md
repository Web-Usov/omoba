# ADR-003: Initial sandboxed mod runtime

- **Status:** Accepted
- **Date:** 2026-09-03

## Context

Open MOBA должна позволять gameplay scripting без выдачи community code произвольного доступа к engine internals, CLR и OS. При этом runtime нельзя превращать в permanent implementation lock-in до того, как сформирован публичный Mod API и threat model Workshop.

## Decision

Фундаментальным contract является capability-based Mod API и replaceable scripting adapter.

Для первой реализации использовать MoonSharp в ограниченном configuration:

- hard-sandbox module set;
- без `io`, system `os` и file APIs;
- без automatic CLR interop;
- custom script loader;
- explicit proxy/capability objects;
- host-controlled execution budget/interruption.

MoonSharp-specific types не входят в public `OpenMoba.ModApi` contracts.

Foundation не обещает стабильную Lua language-version compatibility. Перед публичным Workshop runtime проходит отдельный security/runtime decision.

## Rationale

MoonSharp managed-only и имеет документированные sandbox primitives, что снижает native deployment complexity и позволяет быстро доказать Mod API boundary.

Основная архитектурная ценность — capability boundary, а не конкретный interpreter.

## Alternatives

### Native Lua 5.4 через KeraLua/NLua

Плюс — реальные Lua 5.4 semantics и зрелый native VM. Минусы foundation этапа — native packaging и более сложный sandbox/resource-control surface.

### WASM

Сильная isolation/capability story и multi-language potential, но больше runtime/toolchain/creator complexity для первого vertical slice.

### Arbitrary C# assemblies

Не выбран: недоверенный .NET code получает слишком широкую process capability и делает Workshop security unacceptable.

## Consequences

- initial mod runtime годится для SDK/prototype и controlled content;
- public API проектируется отдельно от interpreter objects;
- adversarial Workshop content НЕ считается безопасным только на основании этого ADR;
- перед публичным distribution mods требуется threat model и повторная runtime evaluation.

## Revisit when

- начинается external creator alpha;
- требуется стабильный Lua version contract;
- performance/resource isolation MoonSharp недостаточны;
- Workshop начинает принимать untrusted third-party packages.
