# Proposed and In-Progress Changes

This directory contains bounded changes to Open MOBA.

A substantial change should move through a documented lifecycle rather than going directly from an idea to implementation.

The initial workflow is expected to use these artifacts:

```text
openspec/changes/<change-name>/
  proposal.md
  requirements.md
  design.md
  tasks.md
```

The intended flow is:

```text
idea
  -> proposal
  -> human intent approval
  -> requirements
  -> design
  -> human design approval
  -> tasks
  -> implementation
  -> automated verification
  -> review
  -> human PR approval
  -> merge
  -> archive/update current specs
```

Not every small bug requires the full workflow. The amount of ceremony should be proportional to architectural impact, risk, and reversibility.

The concrete OpenSpec schema and agent workflow will be defined in a later dedicated change.
