using DepthConsulting.Core.Messaging;

namespace DepthConsulting.Core.Services.Persistence.Commands;

public interface ICommandRepository
{
    Task SaveAsync<T>(T command) where T : CommandBase;
    Task<CommandBase?> GetAsync(string commandId);
    Task<IEnumerable<CommandBase>> GetAllAsync(string aggregateId, int skip, int take);
}