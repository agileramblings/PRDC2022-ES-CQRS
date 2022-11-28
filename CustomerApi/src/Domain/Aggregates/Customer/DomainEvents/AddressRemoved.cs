using System.Text.Json.Serialization;

namespace PRDC2022.Customer.Domain.Aggregates.Customer.DomainEvents;

public class AddressRemoved : AttributedEventBase
{
    public Guid AddressId { get; set; }

    [JsonConstructor]
    public AddressRemoved() : base(string.Empty, DateTime.MinValue, string.Empty, Guid.Empty, string.Empty)
    {
    }

    public AddressRemoved(Guid addressId, string aggregateId, DateTime receivedOn, string correlationId, Guid causationId, string updatedBy)
        : base(aggregateId, receivedOn, correlationId, causationId, updatedBy)
    {
        AddressId = addressId;
    }
}