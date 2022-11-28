using System.Data;
using Dapper;
using Dapper.Contrib.Extensions;
using DepthConsulting.Core.Services.Persistence.EventSource.Aggregates;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using PRDC2022.Customer.Domain.Projections;
using PRDC2022.CustomerApi.Options;

namespace PRDC2022.CustomerApi.Handlers.DomainEvents;

public class MediatrRelationalEventsHandler : BaseEventHandler
{
    private readonly IDbConnection _conn;
    private readonly ILogger _logger;
    private readonly IAggregateRepository<Customer.Domain.Aggregates.Customer.Customer> _repo;

    public MediatrRelationalEventsHandler(IAggregateRepository<Customer.Domain.Aggregates.Customer.Customer> repo, IOptions<DatabaseOptions> options, ILogger<MediatrRelationalEventsHandler> logger)
    {
        _logger = logger;
        _repo = repo;
        _conn = new SqlConnection(options.Value.ConnectionStrings.StagingContext);
    }

    protected override async Task SaveCustomer(string aggregateId)
    {
        try
        {
            var cd = await GetCustomerDetails(aggregateId);
            await SaveRelationalProjection(cd);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save relational Customer Details object");
            throw;
        }
    }

    private async Task<CustomerDetails> GetCustomerDetails(string aggregateId)
    {
        var customer = await _repo.GetAsync(aggregateId);
        var cd = customer.ToDetails();
        return cd;
    }

    private async Task SaveRelationalProjection(CustomerDetails cd)
    {
        var entity = cd.ToEntity();
        var cdExists = _conn.ExecuteScalar<bool>("select count(1) from CustomerDetails where AggregateId = @AggregateId", new { cd.AggregateId });
        if (cdExists)
            await _conn.UpdateAsync(entity);
        else
            await _conn.InsertAsync(entity);


        foreach (var address in entity.Addresses)
        {
            address.ParentId = entity.AggregateId;
            var addyExists = _conn.ExecuteScalar<bool>("select count(1) from Addresses where AddressId=@AddressId", new { address.AddressId });
            if (addyExists)
                await _conn.UpdateAsync(address);
            else
                await _conn.InsertAsync(address);
        }

        foreach (var pr in entity.PhoneRecords)
        {
            pr.ParentId = entity.AggregateId;
            var prExist = _conn.ExecuteScalar<bool>("select count(1) from Phones where PhoneId=@PhoneId", new { pr.PhoneId });
            if (prExist)
                await _conn.UpdateAsync(pr);
            else
                await _conn.InsertAsync(pr);
        }
    }
}