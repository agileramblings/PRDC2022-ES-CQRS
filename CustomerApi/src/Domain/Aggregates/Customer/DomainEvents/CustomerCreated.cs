using System.Text.Json.Serialization;

namespace PRDC2022.Customer.Domain.Aggregates.Customer.DomainEvents;

public class CustomerCreated : AttributedEventBase
{
    [JsonConstructor]
    public CustomerCreated() : base(string.Empty, DateTime.MinValue, string.Empty, Guid.Empty, string.Empty)
    {
    }

    public CustomerCreated(string aggregateId, DateTime receivedOn, string correlationId, Guid causationId, string createdBy)
        : base(aggregateId, receivedOn, correlationId, causationId, createdBy)
    {
    }
}