using DepthConsulting.Core.DDD;

namespace DepthConsulting.Core.Services.Persistence.EventSource.Snapshot;

public interface ISnapshotStrategy<T> where T : AggregateBase, new()
{
    Task<SnapshotResponse> GetEventsForAggregateAsync(string aggregateId);
}