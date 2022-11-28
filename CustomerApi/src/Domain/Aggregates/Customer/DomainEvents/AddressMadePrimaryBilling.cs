using System.Text.Json.Serialization;

namespace PRDC2022.Customer.Domain.Aggregates.Customer.DomainEvents;

public class AddressMadePrimaryBilling : AttributedEventBase
{
    public Guid AddressId { get; set; }

    [JsonConstructor]
    public AddressMadePrimaryBilling() : base(string.Empty, DateTime.MinValue, string.Empty, Guid.Empty, string.Empty)
    {
    }

    public AddressMadePrimaryBilling(Guid addressId, string aggregateId, DateTime receivedOn, string correlationId, Guid causationId, string updatedBy)
        : base(aggregateId, receivedOn, correlationId, causationId, updatedBy)
    {
        AddressId = addressId;
    }
}