using System.Data;
using Autofac;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using PRDC2022.CustomerApi.Options;
using PRDC2022.CustomerApi.Persistence;

namespace PRDC2022.CustomerApi.Module;

public class DatabaseModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);
        builder.Register(c =>
        {
            var config = c.Resolve<IOptions<DatabaseOptions>>();
            return new SqlConnection(config.Value.ConnectionStrings.EventsContext);
        }).As<IDbConnection>();

        builder.Register(c =>
        {
            var config = c.Resolve<IOptions<DatabaseOptions>>();
            return new SqlServerEventDbContext(config);
        }).As<EventDescriptorDbContext>();

        builder.Register(c =>
        {
            var config = c.Resolve<IOptions<DatabaseOptions>>();
            return new StagingDbContext(config);
        }).As<StagingDbContext>();//.InstancePerDependency();
    }
}