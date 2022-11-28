using System.Diagnostics;
using DepthConsulting.Core.DDD;

namespace DepthConsulting.Core.Services.Persistence.EventSource.Snapshot;

public class AcquisitionDurationSnapshotStrategy<T> : DefaultSnapshotStrategy<T> where T : AggregateBase, new()
{
    private readonly int _maxQueryDurationInMilliseconds;

    public AcquisitionDurationSnapshotStrategy(IEventStore<T> store, int maxQueryDurationInMilliseconds) :
        base(store)
    {
        _maxQueryDurationInMilliseconds = maxQueryDurationInMilliseconds;
    }

    public override async Task<SnapshotResponse> GetEventsForAggregateAsync(string aggregateId)
    {
        var sw = new Stopwatch();
        sw.Start();
        var events = await _store.GetEventsForAggregateAsync(aggregateId);
        sw.Stop();
        var shouldSnapshot = sw.ElapsedMilliseconds >= _maxQueryDurationInMilliseconds;

        return new SnapshotResponse
        {
            Events = events,
            ShouldSnapshot = shouldSnapshot
        };
    }
}