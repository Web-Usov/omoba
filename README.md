# Open MOBA

Open MOBA is a moddable multiplayer game platform where the official game is a reference implementation built on the same public APIs available to community creators.

The project is designed around three core ideas:

- **Platform first, game second** — the base MOBA validates the platform rather than bypassing it.
- **Modding by default** — gameplay rules, heroes, abilities, items, maps, and game modes should be extensible without changing engine source.
- **Agent-first development** — architecture, documentation, tests, and workflows are structured so development can be delegated safely to AI agents while a human remains responsible for product intent and final approval.

## Project status

Open MOBA is in the foundation phase. The current focus is establishing product vision, architectural principles, and a spec-driven development workflow before implementation begins.

## Documentation

- [`docs/vision/product-vision.md`](docs/vision/product-vision.md) — product vision and long-term direction.
- [`docs/vision/principles.md`](docs/vision/principles.md) — foundational project principles.
- [`docs/architecture/overview.md`](docs/architecture/overview.md) — evolving architecture overview.
- [`docs/adr/`](docs/adr/) — architecture decision records.
- [`openspec/specs/`](openspec/specs/) — current system specifications.
- [`openspec/changes/`](openspec/changes/) — proposed and in-progress changes.

## Development model

Git is the source of truth. Durable project knowledge belongs in versioned documentation, specifications, ADRs, code, and tests rather than in chat history or a specific AI tool.
