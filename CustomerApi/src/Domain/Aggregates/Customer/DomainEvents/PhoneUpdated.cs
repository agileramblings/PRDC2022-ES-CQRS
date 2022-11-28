using PRDC2022.Customer.Domain.Shared;
using System.Text.Json.Serialization;

namespace PRDC2022.Customer.Domain.Aggregates.Customer.DomainEvents;

public class PhoneUpdated : AttributedEventBase
{
    public Guid PhoneId { get; set; }
    public PhoneDetailsRequestParams Details { get; set; }

    [JsonConstructor]
    public PhoneUpdated() : base(string.Empty, DateTime.MinValue, string.Empty, Guid.Empty, string.Empty)
    {
    }

    public PhoneUpdated(PhoneDetailsRequestParams details, Guid phoneId, string aggregateId, DateTime receivedOn, string correlationId, Guid causationId, string createdBy)
        : base(aggregateId, receivedOn, correlationId, causationId, createdBy)
    {
        PhoneId = phoneId;
        Details = details;
    }
}