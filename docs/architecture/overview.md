# Architecture Overview

> Status: evolving. This document intentionally describes only high-level boundaries that are consistent with the current product vision. Concrete technology choices belong in ADRs and have not yet been finalized here.

## Architectural direction

Open MOBA should separate platform infrastructure from game-specific policy so that the official game can be implemented using the same public extension mechanisms as community games.

A preliminary boundary model is:

```text
Presentation / Client
        |
        v
Public Client Integration
        |
        v
Simulation <----> Networking
        |
        v
Authoritative Server
        |
        v
Public Mod Runtime / SDK
        |
        v
Games and reusable gameplay packages
```

The exact technologies, process model, runtime boundaries, and APIs behind these blocks require explicit architectural decisions before implementation.

## Desired system properties

### Headless-first simulation

Core gameplay simulation must be executable without graphical presentation. This enables dedicated servers, automated tests, long-running simulations, benchmarks, and agent-driven verification.

### Authoritative multiplayer

The server owns authoritative gameplay state and evaluates client commands. Network architecture should make trust boundaries explicit from the beginning.

### Public gameplay capabilities

The official game should depend on public extension contracts rather than privileged internal APIs wherever practical.

### Layered creator experience

The platform is expected to support multiple levels of content creation over time:

1. declarative data;
2. sandboxed scripting;
3. visual authoring tools built on top of stable underlying representations.

### Reusable packages

Gameplay capabilities should be composable so games can share platform and genre packages instead of copying entire implementations.

Possible future package families include:

```text
@openmoba/core
@openmoba/combat
@openmoba/navigation
@openmoba/vision
@openmoba/projectiles
@openmoba/moba
@openmoba/rts
```

Names and boundaries are illustrative until specified and accepted.

## Decisions intentionally not made here

This overview does not yet decide:

- game engine;
- primary implementation language;
- ECS versus another world model;
- mod scripting language or runtime;
- network transport;
- replication strategy;
- simulation tick rate;
- package manifest format;
- editor technology;
- persistence and backend services.

Each consequential choice should be evaluated through the project's spec-driven workflow and recorded in an ADR where appropriate.
