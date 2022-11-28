namespace DepthConsulting.Core.Services.Messaging;

public interface ICommandSender
{
    Task SendAsync<T>(T command) where T : class;
}