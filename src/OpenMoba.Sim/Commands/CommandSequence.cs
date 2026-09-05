namespace OpenMoba.Sim;

/// <summary>
/// Monotonic submission order of a command within one simulation instance.
/// </summary>
public readonly record struct CommandSequence(ulong Value);
