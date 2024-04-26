using Autofac;
using DepthConsulting.Core.Messaging;
using DepthConsulting.Core.Services.Persistence.CQRS;
using DepthConsulting.Core.Services.Persistence.EventSource;
using DepthConsulting.Core.Services.Persistence.EventSource.Aggregates;
using DepthConsulting.Core.Services.Persistence.EventSource.Snapshot;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using NodaTime;
using PRDC2022.Customer.Domain.Services;
using PRDC2022.CustomerApi.Options;
using PRDC2022.CustomerApi.Persistence;
using PRDC2022.CustomerApi.Persistence.Cosmos;

namespace PRDC2022.CustomerApi.Module;

public class ApplicationModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);

        builder.Register(ctx => SystemClock.Instance).As<IClock>().SingleInstance();

        builder.RegisterType<CorrelationIdResolver>()
            .As<ICorrelationIdResolver>()
            .PropertiesAutowired()
            .InstancePerLifetimeScope();
        builder.RegisterType<Correlation>()
            .OnActivating(e => e.ReplaceInstance(
                new Correlation(e.Context
                    .Resolve<ICorrelationIdResolver>()
                    .Resolve())))
            .InstancePerLifetimeScope();

        builder.RegisterType<CustomerService>()
            .As<ICustomerService>()
            .InstancePerLifetimeScope()
            .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);

        builder.RegisterGeneric(typeof(DefaultSnapshotStrategy<>)).As(typeof(ISnapshotStrategy<>));
        builder.RegisterGeneric(typeof(AggregateRepository<>)).As(typeof(IAggregateRepository<>));
        builder.RegisterGeneric(typeof(EventStore<>)).As(typeof(IEventStore<>));
        builder.RegisterGeneric(typeof(EFEventDescriptorStore<>))
            .As(typeof(IEventDescriptorStorage<>))
            .SingleInstance();

        var cosmosClientOptions = new CosmosClientOptions
        {
            HttpClientFactory = () =>
            {
                HttpMessageHandler httpMessageHandler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };

                return new HttpClient(httpMessageHandler);
            },
            ConnectionMode = ConnectionMode.Gateway
        };

        builder.Register(context =>
        {
            var options = context.Resolve<IOptions<CosmosDbOptions>>().Value;
            return new CosmosClient(options.Account, options.Key, cosmosClientOptions);
        }).As<CosmosClient>().SingleInstance();
        builder.RegisterType<CosmosDbQueryService>()
            .As<IReadModelQuery>()
            .As<IReadModelPersistence>()
            .SingleInstance();
        builder.RegisterType<CustomerService>().As<ICustomerService>();
    }
}

public interface ICorrelationIdResolver
{
    string Resolve();
}

public class CorrelationIdResolver : ICorrelationIdResolver
{
    public string Resolve()
    {
        return Guid.NewGuid().ToString();
    }
}