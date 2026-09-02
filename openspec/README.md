# OpenSpec in Open MOBA

OpenSpec is the planning and specification layer for non-trivial changes in this repository. It keeps product intent, behavioral requirements, design decisions, implementation tasks, and shipped specifications versioned next to the code.

The repository uses the built-in `spec-driven` schema:

`proposal -> specs + design -> tasks -> apply -> verify -> archive`

## Source of truth

Git is the source of truth. Chat history, IDE sessions, agent memory, issue comments, and external project-management tools may help coordinate work, but durable decisions must land in the repository as one of:

- product/architecture documentation;
- an ADR;
- an OpenSpec specification/change;
- code and automated verification.

## Local setup

OpenSpec requires Node.js 20.19.0 or newer.

```bash
npm install -g @fission-ai/openspec@latest
openspec --version
```

Initialize or refresh the agent integration from the repository root:

```bash
openspec init
# later, after OpenSpec updates:
openspec update
```

Select only the AI tools actually used on the machine. Generated tool-specific skills/commands should be committed when the project adopts that execution tool.

## Change lifecycle

For meaningful behavior or architecture work:

1. **Explore** — understand the problem, current specs, ADRs, constraints, and alternatives.
2. **Propose** — create the OpenSpec change and planning artifacts.
3. **Review** — the human owner reviews intent and design before implementation.
4. **Apply** — an agent implements the approved tasks.
5. **Verify** — automated checks and an independent review confirm implementation matches the spec.
6. **PR review** — the human owner accepts or rejects the delivered change.
7. **Archive** — only merged, verified behavior is folded into current specs.

## Human approval gates

Open MOBA intentionally keeps human approvals few but meaningful.

### Gate 1 — Intent

Required before implementation when a change affects product behavior, public APIs, architecture, security, networking, persistence, modding boundaries, compatibility, or scope.

The owner confirms that the problem, scope, and non-goals are correct.

### Gate 2 — Design

Required before implementation for architecture-impacting changes.

The owner confirms system boundaries, invariants, interfaces, alternatives, and verification strategy. If the design requires a fundamental new decision, create or update an ADR before coding proceeds.

### Gate 3 — Merge

Required for every non-trivial PR.

The owner reviews delivered behavior, verification evidence, deviations, and architectural impact before merge.

## Standard lane

Use a full OpenSpec change when any of these are true:

- externally observable behavior changes;
- a public Mod API or SDK contract changes;
- engine/platform boundaries change;
- networking, security, persistence, compatibility, or sandboxing is involved;
- a new capability is introduced;
- multiple subsystems are affected;
- an ADR may be required.

## Fast lane

A full change is optional for work that is truly mechanical and does not change behavior or architecture, for example:

- typo or documentation-only correction;
- formatting/lint-only change;
- mechanical refactor covered by existing specs/tests;
- narrow bug fix whose required behavior is already unambiguously specified.

If there is doubt, use the standard lane.

## Rules for agents

- Never treat an implementation convenience as permission to change product intent.
- Never bypass an accepted ADR silently.
- Never expand scope because adjacent work seems useful.
- If implementation contradicts a spec, design, invariant, or ADR, update the change and seek the required approval instead of improvising.
- Prefer requirements that can be verified by tests, simulation, benchmarks, validation tools, or other machine-checkable evidence.

## Current structure

```text
openspec/
├── config.yaml
├── README.md
├── specs/          # Current system behavior
└── changes/        # In-progress and archived changes
```

Custom OpenSpec schemas are intentionally deferred. The project should fork or create a schema only after the built-in `spec-driven` workflow demonstrates a concrete limitation.
