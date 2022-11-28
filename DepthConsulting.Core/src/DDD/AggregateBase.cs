using DepthConsulting.Core.Messaging;
using ReflectionMagic;

namespace DepthConsulting.Core.DDD;

public abstract class AggregateBase
{
    private readonly List<EventBase> _changes = new();

    protected AggregateBase(string aggregateId)
    {
        AggregateId = aggregateId;
    }

    public string AggregateId { get; set; }
    public DateTime Created { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime Modified { get; set; }
    public string? ModifiedBy { get; set; }
    public int Version { get; protected set; } = -1;

    public int EventCount => _changes.Count;

    public IEnumerable<EventBase> GetUncommittedChanges()
    {
        return _changes;
    }

    public void MarkChangesAsCommitted()
    {
        _changes.Clear();
    }

    public void LoadsFromHistory(IEnumerable<EventBase> history)
    {
        var version = -1;
        foreach (var e in history)
        {
            ApplyChange(e, false);
            version++;
        }

        Version = version;
    }

    protected void ApplyChange(EventBase @event)
    {
        ApplyChange(@event, true);
    }

    // push atomic aggregate changes to local history for further processing (EventStore.SaveEvents)
    private void ApplyChange(EventBase @event, bool isNew)
    {
        this.AsDynamic().Apply(@event);
        if (isNew) _changes.Add(@event);
    }
}