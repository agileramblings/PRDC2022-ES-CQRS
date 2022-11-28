using DepthConsulting.Core.DDD;

namespace DepthConsulting.Core.Services.Persistence.CQRS;

public interface IReadModelPersistence
{
    Task PutAsync<T>(T t, string? partitionKeyValue = null) where T : TypeBasedProjectionBase;
    Task PutAggregateAsync<T>(T t, string? partitionKeyValue = null) where T : ProjectionBase;
    Task DeleteAsync<T>(T t) where T : TypeBasedProjectionBase;
    Task DeleteAggregateAsync<T>(T t) where T : ProjectionBase;
}