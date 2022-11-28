using System.Data;
using Dapper;
using Dapper.Contrib.Extensions;
using DepthConsulting.Core.Services.Persistence.EventSource.Aggregates;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using PRDC2022.Customer.Domain.Aggregates.Customer.DomainEvents;
using PRDC2022.Customer.Domain.Projections;
using PRDC2022.CustomerApi.Options;
using PRDC2022.CustomerApi.Persistence;

namespace PRDC2022.CustomerApi.Handlers.DomainEvents;

public class MediatrAnalyticsEventsHandler :
    INotificationHandler<AddressAdded>,
    INotificationHandler<AddressRemoved>,
    INotificationHandler<PhoneAdded>,
    INotificationHandler<PhoneRemoved>
{
    private readonly IDbConnection _conn;
    private readonly ILogger _logger;
    private readonly IAggregateRepository<Customer.Domain.Aggregates.Customer.Customer> _repo;

    public MediatrAnalyticsEventsHandler(IAggregateRepository<Customer.Domain.Aggregates.Customer.Customer> repo, IOptions<DatabaseOptions> options, ILogger<MediatrAnalyticsEventsHandler> logger)
    {
        _logger = logger;
        _repo = repo;
        _conn = new SqlConnection(options.Value.ConnectionStrings.StagingContext);
    }

    public async Task Handle(AddressAdded notification, CancellationToken cancellationToken)
    {
        await SaveAnalyticRecord(notification.AggregateId);
    }

    public async Task Handle(AddressRemoved notification, CancellationToken cancellationToken)
    {
        await SaveAnalyticRecord(notification.AggregateId);
    }

    public async Task Handle(PhoneAdded notification, CancellationToken cancellationToken)
    {
        await SaveAnalyticRecord(notification.AggregateId);
    }

    public async Task Handle(PhoneRemoved notification, CancellationToken cancellationToken)
    {
        await SaveAnalyticRecord(notification.AggregateId);
    }

    private async Task SaveAnalyticRecord(string aggregateId)
    {
        try
        {
            var cd = await GetCustomerDetails(aggregateId);
            await SaveAnalyticsProjection(cd);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save an analytic projection object");
            throw;
        }
    }

    private async Task<CustomerDetails> GetCustomerDetails(string aggregateId)
    {
        var customer = await _repo.GetAsync(aggregateId);
        var cd = customer.ToDetails();
        return cd;
    }

    private async Task SaveAnalyticsProjection(CustomerDetails cd)
    {
        var analyticEntity = new CustomerAddressAnalyticsEntity()
        {
            CustomerId = cd.AggregateId,
            FirstName = cd.FirstName,
            LastName = cd.LastName,
            MiddleName = cd.MiddleName,
            NumberOfAddresses = cd.Addresses.Count,
            NumberOfPhoneNumbers = cd.PhoneRecords.Count
        };
        var recordExists = _conn.ExecuteScalar<bool>("select count(1) from CustomerAddressAnalytics where CustomerId = @CustomerId", new { analyticEntity.CustomerId });
        if (recordExists)
            await _conn.UpdateAsync(analyticEntity);
        else
            await _conn.InsertAsync(analyticEntity);
    }
}