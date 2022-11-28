using DepthConsulting.Core.DDD;

namespace DepthConsulting.Core.Services.Persistence.CQRS;

public interface IReadModelQuery
{
    Task<IEnumerable<T>?> GetItemsAsync<T>(string query) where T : ReadModelBase, new();
    Task<T?> GetItemAsync<T>() where T : TypeBasedProjectionBase, new();
    Task<T?> GetItemAsync<T>(string id) where T : ProjectionBase, new();
    Task<int> GetItemCount<T>(string whereClause) where T : ProjectionBase, new();

    Task<string?> GetRawJsonAsync<T>() where T : TypeBasedProjectionBase, new();
    Task<string?> GetRawJsonAsync<T>(string id) where T : ProjectionBase, new();
    Task<IEnumerable<T>?> GetRawJsonAllAsync<T>(string query) where T : ReadModelBase, new();
}