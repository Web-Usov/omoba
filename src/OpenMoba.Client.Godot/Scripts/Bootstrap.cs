using System.Reflection;
using Godot;

namespace OpenMoba.Client.Godot;

/// <summary>
/// Headless bootstrap smoke scene. Not gameplay architecture.
/// </summary>
public partial class Bootstrap : Node
{
    public override void _Ready()
    {
        // Prove shared engine-neutral contracts resolve into the Godot client assembly
        // without consuming a public bootstrap type from OpenMoba.Contracts.
        var contracts = Assembly.Load("OpenMoba.Contracts");
        var bootstrapMarker = contracts
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(attribute => attribute.Key == "OpenMoba.Bootstrap");

        if (contracts.GetName().Name != "OpenMoba.Contracts" || bootstrapMarker?.Value != "Contracts")
        {
            GD.PrintErr("Smoke composition failed: OpenMoba.Contracts assembly metadata was not loaded.");
            GetTree().Quit(1);
            return;
        }

        GD.Print("""{"component":"OpenMoba.Client.Godot","mode":"smoke","status":"ok"}""");
        CallDeferred(nameof(ExitSmoke));
    }

    private void ExitSmoke()
    {
        GetTree().Quit(0);
    }
}
