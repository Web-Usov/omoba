namespace OpenMoba.Sim;

/// <summary>
/// Authoritative outcome produced during a completed logical tick.
/// </summary>
public abstract record SimulationOutcome(SimulationTick Tick, CommandSequence CommandSequence);

/// <summary>
/// Entity was created as a result of a CreateEntityCommand.
/// </summary>
public sealed record EntityCreatedOutcome(
    SimulationTick Tick,
    CommandSequence CommandSequence,
    EntityId EntityId) : SimulationOutcome(Tick, CommandSequence);

/// <summary>
/// Entity was destroyed as a result of a DestroyEntityCommand.
/// </summary>
public sealed record EntityDestroyedOutcome(
    SimulationTick Tick,
    CommandSequence CommandSequence,
    EntityId EntityId) : SimulationOutcome(Tick, CommandSequence);

/// <summary>
/// Command was rejected without partial authoritative mutation.
/// </summary>
public sealed record CommandRejectedOutcome(
    SimulationTick Tick,
    CommandSequence CommandSequence,
    CommandRejectionReason Reason) : SimulationOutcome(Tick, CommandSequence);
