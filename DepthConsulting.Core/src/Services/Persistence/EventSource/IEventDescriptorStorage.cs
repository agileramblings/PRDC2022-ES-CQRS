namespace DepthConsulting.Core.Services.Persistence.EventSource;

public interface IEventDescriptorStorage<T>
{
    Task<IEnumerable<string>> GetAggregateIdsAsync(int page, int count);
    Task<IEnumerable<EventDescriptor>> GetEventDescriptorsAsync(string aggregateId);
    Task<int> GetMostRecentVersion(string aggregateId);
    Task AddDescriptorAsync(string aggregateId, EventDescriptor ed);
    Task AddDescriptorsAsync(string aggregateId, IEnumerable<EventDescriptor> eds);
}