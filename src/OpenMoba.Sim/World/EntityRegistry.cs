namespace OpenMoba.Sim;

/// <summary>
/// Minimal internal entity liveness registry. Not an ECS.
/// </summary>
internal sealed class EntityRegistry
{
    private ulong _nextEntityId = 1;
    private readonly Dictionary<ulong, int> _idToSlot = new();
    private readonly List<Slot> _slots = new();

    public EntityId Create()
    {
        if (_nextEntityId == 0)
        {
            throw new InvalidOperationException("EntityId space exhausted.");
        }

        var idValue = _nextEntityId++;
        var id = new EntityId(idValue);
        var slotIndex = _slots.Count;
        _slots.Add(new Slot(id, Alive: true));
        _idToSlot[idValue] = slotIndex;
        return id;
    }

    public bool TryDestroy(EntityId entityId)
    {
        if (!entityId.IsValid)
        {
            return false;
        }

        if (!_idToSlot.TryGetValue(entityId.Value, out var slotIndex))
        {
            return false;
        }

        var slot = _slots[slotIndex];
        if (!slot.Alive)
        {
            return false;
        }

        _slots[slotIndex] = slot with { Alive = false };
        return true;
    }

    public bool IsAlive(EntityId entityId)
    {
        if (!entityId.IsValid)
        {
            return false;
        }

        return _idToSlot.TryGetValue(entityId.Value, out var slotIndex) && _slots[slotIndex].Alive;
    }

    public SimulationSnapshot CaptureSnapshot(SimulationTick tick)
    {
        var alive = new List<EntityId>();
        for (var i = 0; i < _slots.Count; i++)
        {
            var slot = _slots[i];
            if (slot.Alive)
            {
                alive.Add(slot.Id);
            }
        }

        // Slots are appended in creation order and IDs are monotonic, so the list
        // is already ascending. Sort defensively to keep observation deterministic.
        alive.Sort(static (left, right) => left.Value.CompareTo(right.Value));
        return new SimulationSnapshot(tick, alive.AsReadOnly());
    }

    private readonly record struct Slot(EntityId Id, bool Alive);
}
