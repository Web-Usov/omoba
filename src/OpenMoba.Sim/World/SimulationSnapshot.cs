namespace OpenMoba.Sim;

/// <summary>
/// Read-only observation of authoritative state at a completed tick.
/// Not a replication or serialization schema.
/// </summary>
public sealed record SimulationSnapshot(
    SimulationTick Tick,
    IReadOnlyList<EntityId> ActiveEntities);
