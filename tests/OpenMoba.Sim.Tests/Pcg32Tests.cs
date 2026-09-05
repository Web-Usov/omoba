namespace OpenMoba.Sim.Tests;

public sealed class Pcg32Tests
{
    // Pinned against pcg-c-basic demo: pcg32_srandom_r(&rng, 42u, 54u)
    // https://www.pcg-random.org/using-pcg-c-basic.html
    private static readonly uint[] ReferenceRound1 =
    [
        0xa15c02b7u,
        0x7b47f409u,
        0xba1d3330u,
        0x83d2f293u,
        0xbfa4784bu,
        0xcbed606eu,
    ];

    [Fact]
    public void Pinned_Reference_Vector_Matches_Pcg_Basic_Demo()
    {
        var rng = Pcg32.Create(seed: 42, stream: Pcg32.FoundationStream);

        var actual = new uint[ReferenceRound1.Length];
        for (var i = 0; i < actual.Length; i++)
        {
            actual[i] = rng.NextUInt32();
        }

        Assert.Equal(ReferenceRound1, actual);
    }

    [Fact]
    public void Same_Seed_Produces_Same_Sequence()
    {
        var left = Pcg32.Create(123);
        var right = Pcg32.Create(123);

        for (var i = 0; i < 32; i++)
        {
            Assert.Equal(left.NextUInt32(), right.NextUInt32());
        }
    }

    [Fact]
    public void Different_Seeds_Produce_Different_Sequences()
    {
        var left = Pcg32.Create(1);
        var right = Pcg32.Create(2);

        var leftValues = Enumerable.Range(0, 8).Select(_ => left.NextUInt32()).ToArray();
        var rightValues = Enumerable.Range(0, 8).Select(_ => right.NextUInt32()).ToArray();

        Assert.NotEqual(leftValues, rightValues);
    }

    [Fact]
    public void Independent_Simulation_Instances_Do_Not_Share_Rng_State()
    {
        var first = new SimulationInstance(new SimulationConfig(Seed: 99));
        var second = new SimulationInstance(new SimulationConfig(Seed: 99));

        var firstDraw = first.Rng.NextUInt32();
        var secondDraw = second.Rng.NextUInt32();

        Assert.Equal(firstDraw, secondDraw);

        _ = first.Rng.NextUInt32();
        Assert.NotEqual(first.Rng.NextUInt32(), second.Rng.NextUInt32());
    }
}
