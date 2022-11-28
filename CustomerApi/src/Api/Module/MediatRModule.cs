using System.Reflection;
using Autofac;
using DepthConsulting.Core.Messaging;
using DepthConsulting.Core.Services.Messaging.EventSource;
using MediatR;
using PRDC2022.CustomerApi.Handlers.DomainEvents;

namespace PRDC2022.CustomerApi.Module;

public class MediatRModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);

        // Mediator itself
        builder
            .RegisterType<Mediator>()
            .As<IMediator>()
            .InstancePerLifetimeScope();

        builder
            .RegisterType<MediatREventPublisher>()
            .As<IEventPublisher>()
            .InstancePerLifetimeScope();

        // request & notification handlers
        builder.Register<ServiceFactory>(context =>
        {
            var c = context.Resolve<IComponentContext>();
            return t => c.Resolve(t);
        });

        builder.RegisterAssemblyTypes(typeof(MediatrCosmosDbEventsHandler).GetTypeInfo().Assembly)
            .Where(t => t.Name.StartsWith("Mediatr"))
            .AsImplementedInterfaces();
    }
}

public class MediatREventPublisher : IEventPublisher
{
    private readonly ILogger _logger;
    private readonly IMediator _mediator;

    public MediatREventPublisher(IMediator mediator, ILogger<IEventPublisher> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task PublishAsync<T>(T @event) where T : EventBase
    {
        _logger.LogDebug("{Component} {Operation} {EventType} to MediatR bus",
            nameof(MediatREventPublisher), nameof(PublishAsync), typeof(T).Name);

        try
        {
            await _mediator.Publish(@event);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}