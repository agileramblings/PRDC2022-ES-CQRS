using PRDC2022.Customer.Domain.Shared;
using System.Text.Json.Serialization;

namespace PRDC2022.Customer.Domain.Aggregates.Customer.DomainEvents;

public class PhoneAdded : AttributedEventBase
{
    public Guid PhoneId { get; set; }
    public PhoneDetailsRequestParams Details { get; set; }

    [JsonConstructor]
    public PhoneAdded() : base(string.Empty, DateTime.MinValue, string.Empty, Guid.Empty, string.Empty)
    {
    }

    public PhoneAdded(PhoneDetailsRequestParams details, Guid phoneId, string aggregateId, DateTime receivedOn, string correlationId, Guid causationId, string createdBy)
        : base(aggregateId, receivedOn, correlationId, causationId, createdBy)
    {
        PhoneId = phoneId;
        Details = details;
    }
}