namespace OpenMoba.Sim.Tests;

public sealed class CanonicalDeterminismTests
{
    [Fact]
    public void Canonical_Scenario_Is_Reproducible_For_Same_Seed_And_Inputs()
    {
        var first = RunCanonicalScenario();
        var second = RunCanonicalScenario();

        Assert.Equal(first.FinalTick, second.FinalTick);
        Assert.Equal(first.Outcomes, second.Outcomes);
        Assert.Equal(first.ActiveEntities, second.ActiveEntities);
    }

    private static CanonicalResult RunCanonicalScenario()
    {
        var simulation = new SimulationInstance(new SimulationConfig(Seed: 42));
        var outcomes = new List<SimulationOutcome>();

        simulation.Submit(new CreateEntityCommand());
        simulation.Submit(new CreateEntityCommand());
        outcomes.AddRange(simulation.Advance().Outcomes);

        var firstCreated = outcomes.OfType<EntityCreatedOutcome>().First().EntityId;
        simulation.Submit(new DestroyEntityCommand(firstCreated));
        simulation.Submit(new CreateEntityCommand());
        simulation.Submit(new DestroyEntityCommand(new EntityId(999)));
        outcomes.AddRange(simulation.Advance().Outcomes);

        simulation.Submit(new CreateEntityCommand());
        outcomes.AddRange(simulation.Advance().Outcomes);

        var snapshot = simulation.CaptureSnapshot();
        return new CanonicalResult(
            snapshot.Tick.Value,
            outcomes.ToArray(),
            snapshot.ActiveEntities.ToArray());
    }

    private sealed record CanonicalResult(
        ulong FinalTick,
        IReadOnlyList<SimulationOutcome> Outcomes,
        IReadOnlyList<EntityId> ActiveEntities);
}
