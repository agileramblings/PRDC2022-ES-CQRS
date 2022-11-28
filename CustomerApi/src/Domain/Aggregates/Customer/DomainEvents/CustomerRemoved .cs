using System.Text.Json.Serialization;

namespace PRDC2022.Customer.Domain.Aggregates.Customer.DomainEvents;

public class CustomerRemoved : AttributedEventBase
{
    [JsonConstructor]
    public CustomerRemoved() : base(string.Empty, DateTime.MinValue, string.Empty, Guid.Empty, string.Empty)
    {
    }

    public CustomerRemoved(string aggregateId, DateTime receivedOn, string correlationId, Guid causationId, string removedBy)
        : base(aggregateId, receivedOn, correlationId, causationId, removedBy)
    {
    }
}