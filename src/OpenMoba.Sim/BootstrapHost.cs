namespace OpenMoba.Sim;

/// <summary>
/// Bootstrap-only composition marker used by the standalone server smoke path.
/// This is not a world, entity, tick, or gameplay model.
/// </summary>
public sealed class BootstrapHost
{
    public static BootstrapHost Create() => new();

    public string ComponentName => "OpenMoba.Sim";
}
