using DepthConsulting.Core.Messaging;

namespace DepthConsulting.Core.Services.Persistence.EventSource.Snapshot;

public class SnapshotResponse
{
    public bool ShouldSnapshot { get; set; }
    public IEnumerable<EventBase> Events { get; set; } = new List<EventBase>();
}