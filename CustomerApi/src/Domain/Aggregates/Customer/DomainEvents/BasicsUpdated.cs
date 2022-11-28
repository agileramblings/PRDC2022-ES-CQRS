using PRDC2022.Customer.Domain.Shared;
using System.Text.Json.Serialization;

namespace PRDC2022.Customer.Domain.Aggregates.Customer.DomainEvents;

public class BasicsUpdated : AttributedEventBase
{
    public BasicDetailsRequestParams Details { get; set; }

    [JsonConstructor]
    public BasicsUpdated() : base(string.Empty, DateTime.MinValue, string.Empty, Guid.Empty, string.Empty)
    {
        Details = new BasicDetailsRequestParams();
    }

    public BasicsUpdated(BasicDetailsRequestParams details, string aggregateId, DateTime receivedOn, string correlationId, Guid causationId, string updatedBy)
        : base(aggregateId, receivedOn, correlationId, causationId, updatedBy)
    {
        Details = details;
    }
}