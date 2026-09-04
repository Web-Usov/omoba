namespace OpenMoba.Sim.Tests;

public sealed class SimulationCommandFlowTests
{
    [Fact]
    public void Submit_Does_Not_Mutate_World_Before_Advance()
    {
        var simulation = new SimulationInstance(new SimulationConfig(Seed: 11));
        simulation.Submit(new CreateEntityCommand());

        Assert.Equal(0UL, simulation.Tick.Value);
        Assert.Empty(simulation.CaptureSnapshot().ActiveEntities);
    }

    [Fact]
    public void Command_Applies_On_Next_Tick()
    {
        var simulation = new SimulationInstance(new SimulationConfig(Seed: 11));
        var sequence = simulation.Submit(new CreateEntityCommand());

        Assert.Equal(0UL, simulation.Tick.Value);

        var step = simulation.Advance();

        Assert.Equal(1UL, simulation.Tick.Value);
        Assert.Equal(1UL, step.Tick.Value);
        var created = Assert.IsType<EntityCreatedOutcome>(Assert.Single(step.Outcomes));
        Assert.Equal(sequence, created.CommandSequence);
        Assert.Equal(1UL, created.Tick.Value);
        Assert.Equal(new[] { created.EntityId }, simulation.CaptureSnapshot().ActiveEntities);
    }

    [Fact]
    public void Multiple_Commands_Process_Fifo_In_One_Tick()
    {
        var simulation = new SimulationInstance(new SimulationConfig(Seed: 11));
        var a = simulation.Submit(new CreateEntityCommand());
        var b = simulation.Submit(new CreateEntityCommand());
        var c = simulation.Submit(new CreateEntityCommand());

        var step = simulation.Advance();

        Assert.Equal(new[] { a, b, c }, step.Outcomes.Select(o => o.CommandSequence));
        Assert.All(step.Outcomes, o => Assert.Equal(1UL, o.Tick.Value));
        Assert.Collection(
            step.Outcomes,
            o => Assert.IsType<EntityCreatedOutcome>(o),
            o => Assert.IsType<EntityCreatedOutcome>(o),
            o => Assert.IsType<EntityCreatedOutcome>(o));
    }

    [Fact]
    public void Invalid_Destroy_Produces_Rejection_Without_Partial_Mutation()
    {
        var simulation = new SimulationInstance(new SimulationConfig(Seed: 11));
        simulation.Submit(new CreateEntityCommand());
        var createStep = simulation.Advance();
        var alive = Assert.IsType<EntityCreatedOutcome>(Assert.Single(createStep.Outcomes)).EntityId;

        var before = simulation.CaptureSnapshot();
        simulation.Submit(new DestroyEntityCommand(new EntityId(999)));
        simulation.Submit(new DestroyEntityCommand(alive));
        simulation.Submit(new DestroyEntityCommand(alive));
        var step = simulation.Advance();

        Assert.Collection(
            step.Outcomes,
            o =>
            {
                var rejected = Assert.IsType<CommandRejectedOutcome>(o);
                Assert.Equal(CommandRejectionReason.EntityNotFound, rejected.Reason);
            },
            o => Assert.IsType<EntityDestroyedOutcome>(o),
            o =>
            {
                var rejected = Assert.IsType<CommandRejectedOutcome>(o);
                Assert.Equal(CommandRejectionReason.EntityNotFound, rejected.Reason);
            });

        Assert.Empty(simulation.CaptureSnapshot().ActiveEntities);
        Assert.Single(before.ActiveEntities);
    }

    [Fact]
    public void Step_Outcomes_Are_ReadOnly()
    {
        var simulation = new SimulationInstance(new SimulationConfig(Seed: 11));
        simulation.Submit(new CreateEntityCommand());
        var step = simulation.Advance();

        var mutable = Assert.IsAssignableFrom<IList<SimulationOutcome>>(step.Outcomes);
        Assert.Throws<NotSupportedException>(() => mutable.Clear());
    }
}
