namespace OpenMoba.Sim;

/// <summary>
/// Non-zero monotonic entity identity within one simulation instance.
/// Value 0 is reserved as invalid/default.
/// </summary>
public readonly record struct EntityId(ulong Value)
{
    public bool IsValid => Value != 0;
}
