using System.Text.Json.Serialization;
using PRDC2022.Customer.Domain.Shared;

namespace PRDC2022.Customer.Domain.Aggregates.Customer.DomainEvents;

public class AddressAdded : AttributedEventBase
{
    public Guid AddressId { get; set; }
    public AddressDetailsRequestParams Details { get; set; }

    [JsonConstructor]
    public AddressAdded() : base(string.Empty, DateTime.MinValue, string.Empty, Guid.Empty, string.Empty)
    {
    }


    public AddressAdded(AddressDetailsRequestParams details, Guid addressId, string aggregateId, DateTime receivedOn, string correlationId, Guid causationId, string updatedBy)
        : base(aggregateId, receivedOn, correlationId, causationId, updatedBy)
    {
        Details = details;
        AddressId = addressId;
    }
}