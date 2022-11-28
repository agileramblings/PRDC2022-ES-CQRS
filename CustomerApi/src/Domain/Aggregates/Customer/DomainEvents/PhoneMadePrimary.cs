using System.Text.Json.Serialization;

namespace PRDC2022.Customer.Domain.Aggregates.Customer.DomainEvents;

public class PhoneMadePrimary : AttributedEventBase
{
    public Guid PhoneId { get; set; }

    [JsonConstructor]
    public PhoneMadePrimary() : base(string.Empty, DateTime.MinValue, string.Empty, Guid.Empty, string.Empty)
    {
    }

    public PhoneMadePrimary(Guid phoneId, string aggregateId, DateTime receivedOn, string correlationId, Guid causationId, string modifiedBy)
        : base(aggregateId, receivedOn, correlationId, causationId, modifiedBy)
    {
        PhoneId = phoneId;
    }
}