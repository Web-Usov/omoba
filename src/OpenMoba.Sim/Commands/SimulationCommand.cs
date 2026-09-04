namespace OpenMoba.Sim;

/// <summary>
/// Foundation host-facing command. Not a gameplay or Mod API surface.
/// </summary>
public abstract record SimulationCommand;

/// <summary>
/// Requests creation of a new entity on the next Advance().
/// </summary>
public sealed record CreateEntityCommand : SimulationCommand;

/// <summary>
/// Requests destruction of an existing entity on the next Advance().
/// </summary>
public sealed record DestroyEntityCommand(EntityId EntityId) : SimulationCommand;
