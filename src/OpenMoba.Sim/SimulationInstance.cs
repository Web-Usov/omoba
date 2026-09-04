namespace OpenMoba.Sim;

/// <summary>
/// Single-threaded authoritative simulation instance.
/// Host owns advancement; this type does not run timers or background loops.
/// </summary>
public sealed class SimulationInstance
{
    private readonly EntityRegistry _world = new();
    private readonly List<PendingCommand> _pending = new();
    private ulong _nextSequence;
    private Pcg32 _rng;

    public SimulationTick Tick { get; private set; }

    public SimulationInstance(SimulationConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        _rng = Pcg32.Create(config.Seed);
        Tick = new SimulationTick(0);
    }

    /// <summary>
    /// Enqueues a command for the next Advance(). Does not mutate world state.
    /// </summary>
    public CommandSequence Submit(SimulationCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var sequence = new CommandSequence(_nextSequence);
        _nextSequence++;
        _pending.Add(new PendingCommand(sequence, command));
        return sequence;
    }

    /// <summary>
    /// Advances exactly one logical tick and returns step-scoped outcomes.
    /// </summary>
    public SimulationStepResult Advance()
    {
        var batch = _pending.ToArray();
        _pending.Clear();

        var completedTick = new SimulationTick(checked(Tick.Value + 1));
        Tick = completedTick;

        var outcomes = new List<SimulationOutcome>(batch.Length);
        foreach (var pending in batch)
        {
            outcomes.Add(ProcessCommand(completedTick, pending));
        }

        return new SimulationStepResult(completedTick, outcomes.AsReadOnly());
    }

    /// <summary>
    /// Captures a read-only snapshot of the current completed-tick authoritative state.
    /// </summary>
    public SimulationSnapshot CaptureSnapshot()
    {
        return _world.CaptureSnapshot(Tick);
    }

    /// <summary>
    /// Exposes instance-owned RNG for internal verification only.
    /// </summary>
    internal ref Pcg32 Rng => ref _rng;

    private SimulationOutcome ProcessCommand(SimulationTick tick, PendingCommand pending)
    {
        switch (pending.Command)
        {
            case CreateEntityCommand:
            {
                var entityId = _world.Create();
                return new EntityCreatedOutcome(tick, pending.Sequence, entityId);
            }

            case DestroyEntityCommand destroy:
            {
                if (_world.TryDestroy(destroy.EntityId))
                {
                    return new EntityDestroyedOutcome(tick, pending.Sequence, destroy.EntityId);
                }

                return new CommandRejectedOutcome(
                    tick,
                    pending.Sequence,
                    CommandRejectionReason.EntityNotFound);
            }

            default:
                throw new InvalidOperationException(
                    $"Unsupported foundation command type: {pending.Command.GetType().FullName}");
        }
    }

    private readonly record struct PendingCommand(CommandSequence Sequence, SimulationCommand Command);
}
