using DepthConsulting.Core.Services.Persistence.CQRS;
using DepthConsulting.Core.Services.Persistence.EventSource.Aggregates;

namespace PRDC2022.CustomerApi.Handlers.DomainEvents;

public class MediatrCosmosDbEventsHandler : BaseEventHandler
{
    private readonly ILogger _logger;
    private readonly IReadModelPersistence _readModelPersistence;
    private readonly IAggregateRepository<Customer.Domain.Aggregates.Customer.Customer> _repo;

    public MediatrCosmosDbEventsHandler(IAggregateRepository<Customer.Domain.Aggregates.Customer.Customer> repo,
        IReadModelPersistence readModelPersistence, ILogger<MediatrCosmosDbEventsHandler> logger)
    {
        _logger = logger;
        _repo = repo;
        _readModelPersistence = readModelPersistence;
    }

    protected override async Task SaveCustomer(string aggregateId)
    {
        try
        {
            var customer = await _repo.GetAsync(aggregateId);
            await _readModelPersistence.PutAggregateAsync(customer.ToDetails());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to Save Customer Details object");
            throw;
        }
    }
}