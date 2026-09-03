using System.Reflection;

[assembly: AssemblyMetadata("OpenMoba.Bootstrap", "Contracts")]

namespace OpenMoba.Contracts;

/// <summary>
/// Bootstrap-only marker used to prove shared-contract consumption.
/// This is not a platform Mod API or gameplay contract.
/// </summary>
public static class SharedContractBootstrap
{
    public static string AssemblyName => typeof(SharedContractBootstrap).Assembly.GetName().Name!;
}
