using System.Text.Json.Serialization;

namespace PRDC2022.Customer.Domain.Aggregates.Customer.DomainEvents;

public class PhoneRemoved : AttributedEventBase
{
    public Guid PhoneId { get; set; }

    [JsonConstructor]
    public PhoneRemoved() : base(string.Empty, DateTime.MinValue, string.Empty, Guid.Empty, string.Empty)
    {
    }

    public PhoneRemoved(Guid phoneId, string aggregateId, DateTime receivedOn, string correlationId, Guid causationId, string removedBy)
        : base(aggregateId, receivedOn, correlationId, causationId, removedBy)
    {
        PhoneId = phoneId;
    }
}