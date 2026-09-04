using System.Reflection;

namespace OpenMoba.Sim.Tests;

public sealed class SimAssemblyLoadTests
{
    [Fact]
    public void Sim_Assembly_Loads_With_Bootstrap_Metadata()
    {
        var assembly = Assembly.Load("OpenMoba.Sim");
        var bootstrapMarker = assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .SingleOrDefault(attribute => attribute.Key == "OpenMoba.Bootstrap");

        Assert.Equal("OpenMoba.Sim", assembly.GetName().Name);
        Assert.NotNull(bootstrapMarker);
        Assert.Equal("Sim", bootstrapMarker.Value);
        Assert.Contains(assembly.GetExportedTypes(), type => type == typeof(SimulationInstance));
    }
}
