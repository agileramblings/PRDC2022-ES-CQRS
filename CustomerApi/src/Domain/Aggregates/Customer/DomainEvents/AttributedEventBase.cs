using DepthConsulting.Core.Messaging;
using MediatR;

namespace PRDC2022.Customer.Domain.Aggregates.Customer.DomainEvents;

public abstract class AttributedEventBase : EventBase, INotification
{
    public AttributedEventBase(string aggregateId, DateTime receivedOn, string correlationId, Guid causationId, string attributedTo) : base(aggregateId, receivedOn, correlationId, causationId)
    {
        AttributedTo = attributedTo;
    }

    public string AttributedTo { get; set; }
}