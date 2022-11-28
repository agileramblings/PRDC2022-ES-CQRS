using DepthConsulting.Core.DDD;
using DepthConsulting.Core.DDD.Exceptions;
using DepthConsulting.Core.Messaging;
using DepthConsulting.Core.Services.Persistence.EventSource.Snapshot;
using Microsoft.Extensions.Logging;

namespace DepthConsulting.Core.Services.Persistence.EventSource.Aggregates;

public class AggregateRepository<T> : IAggregateRepository<T> where T : AggregateBase, new()
{
    private readonly Correlation _correlation;
    private readonly ILogger _logger;
    private readonly ISnapshotStrategy<T> _snapshotStrategy;
    private readonly IEventStore<T> _store;

    public AggregateRepository(IEventStore<T> store, ISnapshotStrategy<T> snapshotStrategy, Correlation correlation,
        ILogger<AggregateRepository<T>> logger)
    {
        _store = store;
        _snapshotStrategy = snapshotStrategy;
        _correlation = correlation;
        _logger = logger;
    }

    public async Task SaveAsync(T aggregate, int expectedVersion, bool failOnConcurrency = true, bool batchSave = false)
    {
        LogAction(nameof(AggregateRepository<T>), nameof(SaveAsync));
        await _store.PutAsync(aggregate.AggregateId, aggregate, aggregate.GetUncommittedChanges(), expectedVersion,
            failOnConcurrency, batchSave);
    }

    public async Task<T> GetAsync(string aggregateId)
    {
        LogAction(nameof(AggregateRepository<T>), nameof(GetAsync));
        var obj = new T();
        SnapshotResponse ssr;
        try
        {
            ssr = await _snapshotStrategy.GetEventsForAggregateAsync(aggregateId);
        }
        catch (AggregateNotFoundException)
        {
            _logger.LogDebug("Attempt to acquire aggregate failed. {aggregateId} was not found", aggregateId);
            throw;
        }

        obj.LoadsFromHistory(ssr.Events);

        if (ssr.ShouldSnapshot && obj is ISnapshotable snapshotable)
            snapshotable.TakeSnapshot(_correlation.CorrelationId);

        return obj;
    }

    public async Task<IEnumerable<string>> GetAllAggregateIdsAsync(int page = 1, int count = int.MaxValue)
    {
        LogAction(nameof(AggregateRepository<T>), nameof(GetAllAggregateIdsAsync));
        return await _store.GetAggregateIdsAsync(page, count);
    }

    public async Task RepublishEventsForAggregate(string aggregateId)
    {
        LogAction(nameof(AggregateRepository<T>), nameof(RepublishEventsForAggregate));
        await _store.RepublishEventsForAggregate(aggregateId);
    }

    private void LogAction(string component, string operation)
    {
        _logger.LogDebug("{Component} {Operation} executed", component, operation);
    }
}