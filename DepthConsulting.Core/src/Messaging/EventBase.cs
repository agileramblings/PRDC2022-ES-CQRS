namespace DepthConsulting.Core.Messaging;

public abstract class EventBase : Message
{
    protected EventBase()
    {
        // DO NOT USE - Serializers only
    }

    protected EventBase(string aggregateId, DateTime receivedOn, string correlationId, Guid causationId)
    {
        AggregateId = aggregateId;
        CorrelationId = correlationId;
        CausationId = causationId;
        ReceivedOn = receivedOn;
        AggregateId = aggregateId;
    }

    public string AggregateId { get; set; } = null!;
    public int Version { get; set; }
    public bool IsSnapshot { get; set; }
    public DateTime ReceivedOn { get; set; }
}