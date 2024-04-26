using PRDC2022.Customer.Domain.Aggregates.Customer.DomainEvents;

namespace PRDC2022.Customer.Domain.Aggregates.Timeline.DomainEvents;

public class TimelineCreated : AttributedEventBase
{
    public TimelineCreated(string aggregateId, DateTime receivedOn, string correlationId, Guid causationId, string attributedTo) : base(aggregateId, receivedOn, correlationId, causationId, attributedTo)
    {
    }
}