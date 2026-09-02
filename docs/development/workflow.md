# Development Workflow

Open MOBA is developed docs-first and agent-first. The human owner defines intent, constraints, and acceptance. Agents plan, implement, verify, and review within those boundaries.

## Roles

### Human owner

Owns:

- product intent;
- priorities and scope;
- acceptance of architecture decisions;
- approval of high-impact designs;
- final PR acceptance.

The human owner should not need to manually supervise routine implementation details when those details are already constrained by accepted specs, ADRs, interfaces, and tests.

### Execution agent

Owns:

- reading the relevant source of truth before coding;
- implementation within approved scope;
- tests and other verification required by the change;
- updating task status and implementation notes;
- surfacing conflicts instead of inventing new product intent.

### Review agent

Should be independent from the execution pass when practical. It checks:

- implementation versus the approved spec and design;
- architectural boundary violations;
- missing tests and edge cases;
- scope creep;
- regressions and unsafe assumptions.

The review agent does not replace human merge approval.

## Source-of-truth hierarchy

When guidance conflicts, use this order:

1. accepted product principles and vision;
2. accepted ADRs;
3. current OpenSpec specifications;
4. approved active-change specs/design;
5. code and tests;
6. issues, PR discussion, and agent-session context.

A lower layer must not silently override a higher layer. Resolve the conflict explicitly.

## Standard change flow

```text
idea
  |
  v
explore
  |
  v
proposal
  |
  v
[Intent Gate]
  |
  +------> delta specs
  |
  +------> design
             |
             v
        [Design Gate when required]
             |
             v
            tasks
             |
             v
           apply
             |
             v
           verify
             |
             v
         AI review
             |
             v
           PR
             |
             v
        [Merge Gate]
             |
             v
          archive
```

## Gate policy

### Intent Gate

Mandatory when the change modifies behavior, capability scope, a public contract, architecture, networking, security, persistence, compatibility, or modding boundaries.

Approval means the owner accepts:

- the problem statement;
- scope;
- non-goals;
- affected capabilities;
- expected outcome.

### Design Gate

Mandatory for architecture-impacting changes and any change that creates or modifies a fundamental technical decision.

Approval means the owner accepts:

- boundaries and ownership;
- interfaces and data flow;
- invariants;
- major trade-offs;
- verification strategy;
- required ADRs.

### Merge Gate

Mandatory for non-trivial PRs.

The PR should present enough evidence for an owner to decide without replaying the entire agent session.

## PR evidence

A substantial PR should include:

- linked OpenSpec change;
- concise summary of delivered behavior;
- verification commands/results;
- tests or benchmarks added/changed;
- architecture/ADR impact;
- deviations from the approved design, if any;
- known limitations or follow-up work.

## Fast lane

A full OpenSpec change may be skipped only when the work is clearly low-risk and does not introduce or alter behavior, architecture, or public contracts.

Typical fast-lane work:

- typo and formatting fixes;
- mechanical code cleanup with unchanged behavior;
- documentation corrections that do not redefine requirements;
- a narrow bug fix whose expected behavior is already covered by a current spec and regression test.

If the implementation needs a new requirement, new architectural assumption, new public behavior, or broader scope, leave the fast lane and create an OpenSpec change.

## Agent delegation unit

Tasks should be written so an execution agent can receive one bounded responsibility and independently prove completion.

A good task has:

- one clear outcome;
- explicit files/components or boundaries when known;
- relevant spec/design references;
- acceptance criteria;
- verification command(s) or expected evidence;
- explicit non-goals when scope expansion is tempting.

Avoid vague tasks such as `make networking better` or `finish combat`. Prefer bounded tasks such as `implement fixed-tick SimulationClock with 30 Hz default and tests covering 300 ticks = 10 simulated seconds`.

## Stop conditions for agents

An agent must stop and surface the conflict when:

- a task contradicts an accepted spec or ADR;
- the requested result requires expanding approved scope;
- a new fundamental architecture decision is required;
- verification cannot be made reliable with the current design;
- security or compatibility implications were not covered by the plan;
- implementation would require changing product intent.

Stopping in these cases is correct behavior, not task failure.

## Documentation discipline

Do not create process artifacts that have no durable value. Avoid meeting notes, implementation diaries, agent-thought logs, and status reports as repository documentation.

Durable information belongs in one of:

- product/architecture docs;
- ADRs;
- OpenSpec specs/changes;
- code/tests/tooling.

## Evolution of this workflow

This workflow is deliberately minimal. Add orchestration layers, custom OpenSpec schemas, agent roles, dashboards, or project-management tools only after a concrete coordination problem appears and the benefit can be demonstrated.
