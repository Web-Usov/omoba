using OpenMoba.Sim;

namespace OpenMoba.Sim.Tests;

public sealed class BootstrapHostTests
{
    [Fact]
    public void Create_Returns_Composition_Marker()
    {
        var host = BootstrapHost.Create();

        Assert.NotNull(host);
        Assert.Equal("OpenMoba.Sim", host.ComponentName);
    }
}
