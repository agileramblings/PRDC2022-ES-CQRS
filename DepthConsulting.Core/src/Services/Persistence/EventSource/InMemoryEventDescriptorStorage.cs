using DepthConsulting.Core.DDD;

namespace DepthConsulting.Core.Services.Persistence.EventSource;

public class InMemoryEventDescriptorStorage<T> : IEventDescriptorStorage<T> where T : AggregateBase
{
    private readonly Dictionary<string, AggregateEventStorage> _eventStore = new();

    public Task<IEnumerable<string>> GetAggregateIdsAsync(int page, int count)
    {
        var pagesToSkip = page <= 1 ? 1 : page - 1;
        var skip = pagesToSkip * count;
        return Task.FromResult(_eventStore.Keys.Distinct().Skip(skip).Take(count));
    }

    public Task<IEnumerable<EventDescriptor>> GetEventDescriptorsAsync(string aggregateId)
    {
        IEnumerable<EventDescriptor> eventDescriptors;
        if (!_eventStore.ContainsKey(aggregateId))
        {
            eventDescriptors = new List<EventDescriptor>();
        }
        else
        {
            var allEvents = new List<EventDescriptor>();
            var latestSnapshot = _eventStore[aggregateId].Snapshots
                .OrderBy(ede => ede.Key)
                .LastOrDefault().Value;

            if (!string.IsNullOrEmpty(latestSnapshot.AggregateId))
            {
                allEvents.Add(latestSnapshot);
                allEvents.AddRange(_eventStore[aggregateId].Events
                    .Where(ed => ed.Key > latestSnapshot.Version)
                    .Select(ede => ede.Value));
            }
            else
            {
                allEvents.AddRange(_eventStore[aggregateId].Events
                    .Select(ede => ede.Value));
            }

            eventDescriptors = allEvents;
        }

        return Task.FromResult(eventDescriptors);
    }

    public Task<int> GetMostRecentVersion(string aggregateId)
    {
        if (!_eventStore.ContainsKey(aggregateId))
        {
            return Task.FromResult(-1);
        }

        var allEvents = new List<EventDescriptor>();

        var latestSnapshot = _eventStore[aggregateId].Snapshots
            .OrderBy(ede => ede.Key)
            .LastOrDefault().Value;

        if (!string.IsNullOrEmpty(latestSnapshot.AggregateId))
        {
            allEvents.Add(latestSnapshot);
            allEvents.AddRange(_eventStore[aggregateId].Events
                .Where(ed => ed.Key > latestSnapshot.Version)
                .Select(ede => ede.Value));
        }
        else
        {
            allEvents.AddRange(_eventStore[aggregateId].Events
                .Select(ede => ede.Value));
        }

        return Task.FromResult(allEvents.OrderBy(e => e.Version).Max(e => e.Version));
    }

    public Task AddDescriptorAsync(string aggregateId, EventDescriptor ed)
    {
        if (!_eventStore.ContainsKey(aggregateId))
        {
            _eventStore.Add(aggregateId, new AggregateEventStorage());
            AddEventToProperCollection(ed);
        }
        else
        {
            AddEventToProperCollection(ed);
        }

        return Task.CompletedTask;
    }

    public Task AddDescriptorsAsync(string aggregateId, IEnumerable<EventDescriptor> eds)
    {
        foreach (var ed in eds) AddDescriptorAsync(aggregateId, ed);

        return Task.CompletedTask;
    }

    private void AddEventToProperCollection(EventDescriptor ed)
    {
        if (ed.EventData.IsSnapshot)
            _eventStore[ed.AggregateId].Snapshots.Add(ed.Version, ed);
        else
            _eventStore[ed.AggregateId].Events.Add(ed.Version, ed);
    }

    private class AggregateEventStorage
    {
        public readonly Dictionary<int, EventDescriptor> Events = new();
        public readonly Dictionary<int, EventDescriptor> Snapshots = new();
    }
}