using System.Diagnostics;

namespace OpenMoba.Sim.Tests;

public sealed class SimulationClockTests
{
    [Fact]
    public void New_Instance_Starts_At_Tick_Zero()
    {
        var simulation = new SimulationInstance(new SimulationConfig(Seed: 1));

        Assert.Equal(0UL, simulation.Tick.Value);
    }

    [Fact]
    public void N_Advances_Reach_Tick_N_Without_Wall_Clock_Waiting()
    {
        var simulation = new SimulationInstance(new SimulationConfig(Seed: 1));
        const int advances = 50;
        var stopwatch = Stopwatch.StartNew();

        for (var i = 0; i < advances; i++)
        {
            simulation.Advance();
        }

        stopwatch.Stop();

        Assert.Equal((ulong)advances, simulation.Tick.Value);
        Assert.True(
            stopwatch.Elapsed < TimeSpan.FromMilliseconds(500),
            $"Advancement unexpectedly waited on wall clock: {stopwatch.Elapsed}");
    }
}
