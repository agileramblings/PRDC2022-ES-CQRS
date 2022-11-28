using DepthConsulting.Core.DDD;
using DepthConsulting.Core.Messaging;

namespace DepthConsulting.Core.Services.Persistence.EventSource;

public interface IEventStore<T> where T : AggregateBase
{
    Task<IEnumerable<string>> GetAggregateIdsAsync(int page, int count);
    Task RepublishEventsForAggregate(string aggregateId);

    Task PutAsync(string aggregateId, AggregateBase aggregateType, IEnumerable<EventBase> events,
        int expectedVersion, bool failOnConcurrency, bool batchSave = false);

    Task<IEnumerable<EventBase>> GetEventsForAggregateAsync(string aggregateId);
}