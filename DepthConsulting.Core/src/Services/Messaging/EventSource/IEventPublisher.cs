using DepthConsulting.Core.Messaging;

namespace DepthConsulting.Core.Services.Messaging.EventSource;

public interface IEventPublisher
{
    Task PublishAsync<T>(T @event) where T : EventBase;
}