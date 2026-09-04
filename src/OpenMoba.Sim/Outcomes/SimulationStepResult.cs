namespace OpenMoba.Sim;

/// <summary>
/// Read-only batch of outcomes for one completed Advance().
/// </summary>
public sealed record SimulationStepResult(
    SimulationTick Tick,
    IReadOnlyList<SimulationOutcome> Outcomes);
