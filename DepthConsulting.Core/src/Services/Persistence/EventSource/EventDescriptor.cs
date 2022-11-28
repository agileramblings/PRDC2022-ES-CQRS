using DepthConsulting.Core.Messaging;

namespace DepthConsulting.Core.Services.Persistence.EventSource;

public struct EventDescriptor
{
    public readonly Guid MessageId;
    public readonly string CorrelationId;
    public readonly Guid CausationId;

    public readonly string AggregateId;
    public readonly EventBase EventData;
    public readonly int Version;
    public readonly DateTime ReceivedOn;
    public readonly string AggregateType;

    public EventDescriptor(string aggregateId, string aggregateType, EventBase eventData, int version,
        DateTime receivedOn,
        Guid messageId, string correlationId, Guid causationId)
    {
        MessageId = messageId;
        CorrelationId = correlationId;
        CausationId = causationId;
        AggregateId = aggregateId;
        EventData = eventData;
        Version = version;
        ReceivedOn = receivedOn;
        AggregateType = aggregateType;
    }

    public bool Equals(EventDescriptor other)
    {
        return AggregateId == other.AggregateId && Version == other.Version;
    }

    public override bool Equals(object? obj)
    {
        return obj is EventDescriptor other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return ((AggregateId != null ? AggregateId.GetHashCode() : 0) * 397) ^ Version;
        }
    }

    public static bool operator ==(EventDescriptor ed1, EventDescriptor ed2)
    {
        return ed1.Equals(ed2);
    }

    public static bool operator !=(EventDescriptor ed1, EventDescriptor ed2)
    {
        return !ed1.Equals(ed2);
    }
}