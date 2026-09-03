using Godot;
using OpenMoba.Contracts;

namespace OpenMoba.Client.Godot;

/// <summary>
/// Headless bootstrap smoke scene. Not gameplay architecture.
/// </summary>
public partial class Bootstrap : Node
{
    public override void _Ready()
    {
        // Prove shared engine-neutral contracts resolve into the Godot client assembly.
        _ = SharedContractBootstrap.AssemblyName;

        GD.Print("""{"component":"OpenMoba.Client.Godot","mode":"smoke","status":"ok"}""");
        GetTree().Quit(0);
    }
}
