using MediatR;
using PRDC2022.Customer.Domain.Aggregates.Customer.DomainEvents;

namespace PRDC2022.CustomerApi.Handlers.DomainEvents;

public abstract class BaseEventHandler :
    INotificationHandler<BasicsUpdated>,
    INotificationHandler<AddressAdded>,
    INotificationHandler<AddressMadePrimaryBilling>,
    INotificationHandler<AddressMadePrimaryShipping>,
    INotificationHandler<AddressRemoved>,
    INotificationHandler<PhoneAdded>,
    INotificationHandler<PhoneRemoved>,
    INotificationHandler<PhoneMadePrimary>
{
    public async Task Handle(AddressAdded notification, CancellationToken cancellationToken)
    {
        await SaveCustomer(notification.AggregateId);
    }

    public async Task Handle(AddressMadePrimaryBilling notification, CancellationToken cancellationToken)
    {
        await SaveCustomer(notification.AggregateId);
    }

    public async Task Handle(AddressMadePrimaryShipping notification, CancellationToken cancellationToken)
    {
        await SaveCustomer(notification.AggregateId);
    }

    public async Task Handle(AddressRemoved notification, CancellationToken cancellationToken)
    {
        await SaveCustomer(notification.AggregateId);
    }

    public async Task Handle(BasicsUpdated notification, CancellationToken cancellationToken)
    {
        await SaveCustomer(notification.AggregateId);
    }

    public async Task Handle(PhoneAdded notification, CancellationToken cancellationToken)
    {
        await SaveCustomer(notification.AggregateId);
    }

    public async Task Handle(PhoneMadePrimary notification, CancellationToken cancellationToken)
    {
        await SaveCustomer(notification.AggregateId);
    }

    public async Task Handle(PhoneRemoved notification, CancellationToken cancellationToken)
    {
        await SaveCustomer(notification.AggregateId);
    }

    protected virtual async Task SaveCustomer(string aggregateId)
    {
        throw new NotImplementedException();
    }
}