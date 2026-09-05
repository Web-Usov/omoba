namespace OpenMoba.Sim;

/// <summary>
/// Foundation host-facing command. Not a gameplay or Mod API surface.
/// </summary>
public abstract record SimulationCommand
{
    private protected SimulationCommand() { }

    // Record сохраняет protected copy-конструктор. Недоступный внешним assemblies
    // abstract member закрывает и этот путь создания произвольных commands.
    private protected abstract void RestrictToFoundationCommands();
}

/// <summary>
/// Requests creation of a new entity on the next Advance().
/// </summary>
public sealed record CreateEntityCommand : SimulationCommand
{
    private protected override void RestrictToFoundationCommands() { }
}

/// <summary>
/// Requests destruction of an existing entity on the next Advance().
/// </summary>
public sealed record DestroyEntityCommand(EntityId EntityId) : SimulationCommand
{
    private protected override void RestrictToFoundationCommands() { }
}
