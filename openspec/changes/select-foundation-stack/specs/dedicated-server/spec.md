## Purpose

Определяет отдельный authoritative server host, пригодный для Linux/headless execution, automated testing и будущего deployment без game-engine runtime.

## ADDED Requirements

### Requirement: Dedicated server является standalone .NET host
Authoritative dedicated server SHALL запускаться как standalone .NET process и MUST NOT требовать Godot runtime для core server execution.

#### Scenario: Linux headless server start
- **WHEN** server executable запускается на Linux environment без GPU/display server
- **THEN** server SHALL initialize без Godot/editor dependencies

### Requirement: Server композирует simulation и adapters
Server host SHALL владеть composition и process lifecycle, а simulation SHALL оставаться reusable library.

#### Scenario: Server composition
- **WHEN** server process initializes a match
- **THEN** host SHALL compose simulation, mod runtime и future networking adapter через explicit boundaries
