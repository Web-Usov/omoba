using OpenMoba.Sim;

if (args is ["--smoke"])
{
    try
    {
        const ulong smokeSeed = 42;
        var simulation = new SimulationInstance(new SimulationConfig(Seed: smokeSeed));

        if (simulation.Tick.Value != 0)
        {
            Console.Error.WriteLine("Smoke composition failed: expected initial tick 0.");
            return 1;
        }

        simulation.Submit(new CreateEntityCommand());
        var step = simulation.Advance();
        var snapshot = simulation.CaptureSnapshot();

        if (simulation.Tick.Value != 1 || step.Tick.Value != 1)
        {
            Console.Error.WriteLine("Smoke composition failed: expected tick 1 after one Advance().");
            return 1;
        }

        if (step.Outcomes.Count != 1 || step.Outcomes[0] is not EntityCreatedOutcome created)
        {
            Console.Error.WriteLine("Smoke composition failed: expected a single EntityCreatedOutcome.");
            return 1;
        }

        if (snapshot.ActiveEntities.Count != 1 || snapshot.ActiveEntities[0] != created.EntityId)
        {
            Console.Error.WriteLine("Smoke composition failed: snapshot does not match create outcome.");
            return 1;
        }

        Console.WriteLine("""{"component":"OpenMoba.Server","mode":"smoke","status":"ok"}""");
        return 0;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine(ex);
        return 1;
    }
}

Console.Error.WriteLine("OpenMoba.Server bootstrap host. Pass --smoke for verification.");
return 1;
