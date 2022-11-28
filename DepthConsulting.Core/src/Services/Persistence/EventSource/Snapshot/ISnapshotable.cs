namespace DepthConsulting.Core.Services.Persistence.EventSource.Snapshot;

public interface ISnapshotable
{
    void TakeSnapshot(string? correlationId);
}