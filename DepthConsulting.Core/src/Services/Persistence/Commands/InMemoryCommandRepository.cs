using DepthConsulting.Core.Messaging;

namespace DepthConsulting.Core.Services.Persistence.Commands;

public class InMemoryCommandRepository : ICommandRepository
{
    private readonly List<CommandBase> _commands = new();

    public Task SaveAsync<T>(T command) where T : CommandBase
    {
        _commands.Add(command);
        return Task.CompletedTask;
    }

    public Task<CommandBase?> GetAsync(string commandId)
    {
        return Task.FromResult(_commands.FirstOrDefault(c => c.CorrelationId == commandId));
    }

    public Task<IEnumerable<CommandBase>> GetAllAsync(string aggregateId, int skip = 0, int take = int.MaxValue)
    {
        var results = _commands
            .Where(c => c.AggregateId == aggregateId)
            .Skip(skip)
            .Take(take);

        return Task.FromResult(results);
    }
}