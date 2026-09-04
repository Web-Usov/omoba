namespace OpenMoba.Sim.Tests;

public sealed class SimulationWorldTests
{
    [Fact]
    public void Create_And_Destroy_Lifecycle_Updates_Snapshot()
    {
        var simulation = new SimulationInstance(new SimulationConfig(Seed: 7));
        simulation.Submit(new CreateEntityCommand());
        simulation.Submit(new CreateEntityCommand());
        var createStep = simulation.Advance();

        var created = createStep.Outcomes.OfType<EntityCreatedOutcome>().Select(o => o.EntityId).ToArray();
        Assert.Equal(2, created.Length);

        var afterCreate = simulation.CaptureSnapshot();
        Assert.Equal(new[] { created[0], created[1] }, afterCreate.ActiveEntities);

        simulation.Submit(new DestroyEntityCommand(created[0]));
        simulation.Advance();

        var afterDestroy = simulation.CaptureSnapshot();
        Assert.Equal(new[] { created[1] }, afterDestroy.ActiveEntities);
    }

    [Fact]
    public void Entity_Ids_Are_NonZero_Unique_And_Not_Reused()
    {
        var simulation = new SimulationInstance(new SimulationConfig(Seed: 7));
        simulation.Submit(new CreateEntityCommand());
        simulation.Submit(new CreateEntityCommand());
        var firstStep = simulation.Advance();
        var firstIds = firstStep.Outcomes.OfType<EntityCreatedOutcome>().Select(o => o.EntityId).ToArray();

        Assert.All(firstIds, id => Assert.True(id.IsValid));
        Assert.Equal(2, firstIds.Distinct().Count());
        Assert.Equal(1UL, firstIds[0].Value);
        Assert.Equal(2UL, firstIds[1].Value);

        simulation.Submit(new DestroyEntityCommand(firstIds[0]));
        simulation.Advance();

        simulation.Submit(new CreateEntityCommand());
        var secondStep = simulation.Advance();
        var recreated = Assert.IsType<EntityCreatedOutcome>(Assert.Single(secondStep.Outcomes));

        Assert.Equal(3UL, recreated.EntityId.Value);
        Assert.DoesNotContain(recreated.EntityId, firstIds);
    }

    [Fact]
    public void Snapshot_Is_Ascending_And_ReadOnly()
    {
        var simulation = new SimulationInstance(new SimulationConfig(Seed: 7));
        simulation.Submit(new CreateEntityCommand());
        simulation.Submit(new CreateEntityCommand());
        simulation.Submit(new CreateEntityCommand());
        simulation.Advance();

        var snapshot = simulation.CaptureSnapshot();
        Assert.Equal(3, snapshot.ActiveEntities.Count);
        Assert.True(snapshot.ActiveEntities[0].Value < snapshot.ActiveEntities[1].Value);
        Assert.True(snapshot.ActiveEntities[1].Value < snapshot.ActiveEntities[2].Value);

        var mutable = Assert.IsAssignableFrom<IList<EntityId>>(snapshot.ActiveEntities);
        Assert.Throws<NotSupportedException>(() => mutable.Add(new EntityId(99)));
        Assert.Throws<NotSupportedException>(() => mutable.Clear());

        Assert.Equal(3, simulation.CaptureSnapshot().ActiveEntities.Count);
    }
}
