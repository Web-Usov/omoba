# Project Principles

These principles guide product, architecture, and development decisions for Open MOBA. They are intentionally small in number and should change only when there is a strong reason to revisit the project's foundations.

## 1. Platform first, game second

The platform is the primary product. The official MOBA exists to validate the platform and should not bypass it through privileged gameplay hooks.

## 2. The official game uses the public Mod API

If the reference game requires capabilities unavailable to third-party creators, the public API is incomplete. Official gameplay should dogfood the same APIs, package boundaries, and workflows exposed to external creators.

## 3. Server authoritative by default

Clients provide player intent. The authoritative server owns game state and resolves gameplay outcomes unless an explicitly documented design decision says otherwise.

## 4. Simulation is independent from presentation

Gameplay simulation should not depend on rendering, editor state, or a graphical environment. Headless simulation must be possible for dedicated servers, automated tests, benchmarks, and AI-agent verification.

## 5. Moddability is an architectural requirement

Heroes, abilities, items, units, game modes, maps, rules, and reusable gameplay packages should be designed for extension rather than hard-coded around the first game.

## 6. Agent-first development

The project is initially developed by one human orchestrator with AI agents. Prefer text-based, versioned, CLI-accessible, testable workflows that agents can execute and verify without fragile manual editor procedures.

## 7. Git is the source of truth

Durable knowledge belongs in the repository: specifications, ADRs, architecture documentation, code, tests, and review history. Chat history, model memory, IDE state, and vendor-specific agent sessions are not authoritative project knowledge.

## 8. Changes should be machine-verifiable whenever practical

Requirements should produce objective acceptance criteria wherever possible. Agents should be able to prove implementation quality through tests, headless simulations, validation commands, benchmarks, or other reproducible checks.

## 9. Avoid premature creator tooling

Stable underlying APIs come before expensive visual editors. Declarative formats, scripting, validation, and CLI workflows should prove the creator model before node editors, terrain tooling, or workshop UX are built.

## 10. Prefer replaceable development infrastructure

Specifications and project knowledge should remain useful if the team changes coding agents, IDEs, orchestration tools, CI vendors, or hosting providers. Avoid binding core project knowledge to a single AI product.

## 11. Keep the immutable core small

Engine-level primitives should be limited to capabilities that are difficult, unsafe, or inefficient to implement as gameplay content. Game-specific policy belongs in public packages and mods whenever feasible.

## 12. Explicit decisions beat accidental architecture

Costly, cross-cutting, or difficult-to-reverse technical choices should be captured as Architecture Decision Records before they become hidden assumptions in the codebase.
