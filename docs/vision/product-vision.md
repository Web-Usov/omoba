# Product Vision

## What Open MOBA is

Open MOBA is a moddable multiplayer game platform focused on top-down competitive and cooperative games.

The official MOBA is not the platform's privileged core. It is the first reference game built using the same public capabilities that third-party creators receive.

The long-term goal is to make it possible to create games such as:

- classic 5v5 MOBA;
- 3v3 and 3v3v3 variants;
- ARAM and arena modes;
- PvE raids and survival modes;
- tower defense;
- auto-battlers;
- RTS-like custom games;
- other top-down multiplayer game modes.

## Product thesis

Existing games often expose modding after the base game architecture is already fixed. Open MOBA treats moddability as a first-class architectural constraint from the beginning.

The platform should provide reusable low-level systems for simulation, networking, rendering integration, navigation, combat primitives, projectiles, vision, AI hooks, assets, and creator tooling while leaving gameplay policy to packages and mods.

The reference MOBA should prove that the public API is powerful enough to build a complete game without private engine hooks.

## Target users

### Players

Players should be able to discover and join polished official and community-created multiplayer experiences from one ecosystem.

### Creators

Creators should be able to start at different levels of complexity:

1. declarative content for simple heroes, items, abilities, and rules;
2. scripting for custom gameplay logic;
3. visual tooling for maps and gameplay graphs when those workflows become mature enough to justify an editor.

### Developers

Developers should have a stable SDK, documented contracts, local and dedicated-server workflows, automated validation, versioned packages, and compatibility guarantees appropriate to the platform's maturity.

## Creator promise

A creator should eventually be able to:

1. install the SDK;
2. create a game or extend an existing package;
3. define a hero, ability, item, map, or game mode without modifying engine source;
4. run the result locally;
5. start a dedicated server;
6. invite another player;
7. package and publish the result.

This promise is the central acceptance criterion for the platform architecture.

## Reference game

The first official game will intentionally be small. Its purpose is to validate platform capabilities, not to compete with mature MOBAs on content volume.

Early scope should favor a small playable match with a few heroes, abilities, items, creeps, structures, and victory rules over a large content roster.

## Development model

Open MOBA is initially developed by a single human orchestrator working with AI agents.

The repository must therefore be easy for agents to understand, modify, build, test, and review using text-based interfaces and automated verification.

The project should prefer workflows where an agent can independently:

- understand intent from versioned specifications;
- implement a bounded change;
- run tests and headless simulations;
- inspect failures;
- update the implementation;
- produce a reviewable pull request.

The human remains responsible for product direction, architecture approval, scope, and final acceptance.

## What Open MOBA is not

Open MOBA is not:

- a direct Dota 2 clone;
- an attempt to build a custom rendering engine from scratch;
- a promise to support every game genre;
- a visual editor project before the underlying public APIs are proven;
- an AI-generated codebase without explicit architectural ownership and verification.

## Long-term success

Open MOBA succeeds if the platform develops a healthy creator ecosystem where official and community games share the same foundations, creators can safely extend each other through reusable packages, and the platform can evolve without making every game depend on undocumented engine internals.
