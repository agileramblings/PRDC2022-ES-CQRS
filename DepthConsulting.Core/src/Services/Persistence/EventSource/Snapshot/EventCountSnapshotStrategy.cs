using DepthConsulting.Core.DDD;
using Microsoft.Extensions.Options;

namespace DepthConsulting.Core.Services.Persistence.EventSource.Snapshot;

public class EventCountSnapshotStrategy<T> : DefaultSnapshotStrategy<T> where T : AggregateBase, new()
{
    private readonly IOptionsMonitor<EventCountSnapshotStrategyOptions> _maxEventCountOptions;

    public EventCountSnapshotStrategy(IEventStore<T> store, IOptionsMonitor<EventCountSnapshotStrategyOptions> maxEventCount) : base(store)
    {
        _maxEventCountOptions = maxEventCount;
    }

    public override async Task<SnapshotResponse> GetEventsForAggregateAsync(string aggregateId)
    {
        var events = (await _store.GetEventsForAggregateAsync(aggregateId)).ToList();
        var shouldSnapshot = events.Count() > _maxEventCountOptions.CurrentValue.MaxEventCount;

        return new SnapshotResponse
        {
            Events = events,
            ShouldSnapshot = shouldSnapshot
        };
    }
}

public class EventCountSnapshotStrategyOptions
{
    public int MaxEventCount;
}