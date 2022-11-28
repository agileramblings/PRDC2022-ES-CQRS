namespace DepthConsulting.Core.Messaging;

public abstract class CommandBase : Message
{
    protected CommandBase(string aggregateId, int originalVersion, DateTime receivedOn, string correlationId,
        Guid causationId)
    {
        MessageId = Guid.NewGuid();
        CorrelationId = correlationId;
        CausationId = causationId;

        OriginalVersion = originalVersion;
        ReceivedOn = receivedOn;
        AggregateId = aggregateId;
    }

    public string AggregateId { get; set; } // Commands always happen to something

    public int OriginalVersion { get; set; } // The current version of the aggregate in the database. New aggregates are -1

    public DateTime ReceivedOn { get; set; }
}