using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using PRDC2022.CustomerApi.Module;
using PRDC2022.CustomerApi.Options;
using PRDC2022.CustomerApi.Persistence;
using Serilog;
using Serilog.Events;

namespace PRDC2022.CustomerApi;

public class Program
{
    public static readonly string AppName = "PRDC2022.CustomerAPI";

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Configuration
            .AddJsonFile("appsettings.json", false, true)
            .AddEnvironmentVariables();
        builder.Host
            .UseSerilog(ConfigureLogging)
            .UseServiceProviderFactory(new AutofacServiceProviderFactory());
        builder.Host.ConfigureContainer<ContainerBuilder>(b => b.RegisterModule(new DatabaseModule()));
        builder.Host.ConfigureContainer<ContainerBuilder>(b => b.RegisterModule(new MediatRModule()));
        builder.Host.ConfigureContainer<ContainerBuilder>(b => b.RegisterModule(new ApplicationModule()));

        builder.Services.Configure<DatabaseOptions>(builder.Configuration.GetSection(DatabaseOptions.Position));
        builder.Services.Configure<CosmosDbOptions>(builder.Configuration.GetSection(CosmosDbOptions.Position));

        // Add services to the container.
        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        app.StartEFMigrations();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        try
        {
            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host {Application} terminated unexpectedly", AppName);
        }
        finally
        {
            Log.Information("{Application} is closing", AppName);
        }
    }

    private static void ConfigureLogging(HostBuilderContext ctx, LoggerConfiguration lc)
    {
        string[] lines = { "DefaultBuildNumber", "DefaultBuildId", "DefaultGitHsh" };
        try
        {
            // read the .buildinfo.json file to get build variables
            lines = File.ReadAllLinesAsync(".buildinfo.json")
                .GetAwaiter()
                .GetResult().First().Split(":");
        }
        catch
        {
            // don't fail if missing
        }

        lc.MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.HostFiltering.HostFilteringMiddleware", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Server.Kestrel", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Routing.EndpointMiddleware", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Routing.EndpointRoutingMiddleware", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Routing.Matching.DfaMatcher", LogEventLevel.Warning)
            .MinimumLevel.Verbose()
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithProperty("Environment", "PrDC2022-Dev")
            .Enrich.WithProperty("Application", "CustomerAPI")
            .Enrich.WithProperty("BuildNumber", lines[0])
            .WriteTo.Console(LogEventLevel.Debug, "{NewLine}{@Timestamp:HH:mm:ss} [{Level}] {Message}{Exception}")
            .WriteTo.Seq(ctx.Configuration.GetSection("Seq:Url").Value);
    }
}

internal static class WebBuilderExtensions
{
    public static void StartEFMigrations(this WebApplication app)
    {
        app.Lifetime.ApplicationStarted.Register(() =>
        {
            var hostEnv = app.Services.GetAutofacRoot().Resolve<IHostEnvironment>();
            var stagingDbContext = app.Services.GetAutofacRoot().Resolve<StagingDbContext>();
            var eventsDbContext = app.Services.GetAutofacRoot().Resolve<EventDescriptorDbContext>();
            ProvisionDatabase(hostEnv, eventsDbContext, stagingDbContext);
        });
    }

    private static void ProvisionDatabase(IHostEnvironment env, params DbContext[] dbContexts)
    {
        if (env.IsEnvironment("LocalDevelopment"))
            ReplaceDatabase(dbContexts);
        else
            UpdateExistingDatabase(dbContexts);
    }

    private static void ReplaceDatabase(params DbContext[] dbContexts)
    {
        foreach (var dbContext in dbContexts)
        {
            dbContext.Database.EnsureDeleted();
            dbContext.Database.Migrate();
        }
    }

    private static void UpdateExistingDatabase(params DbContext[] dbContexts)//, DbContext stagingDb, IEventDescriptorStorage<Customer.Domain.Aggregates.Customer.Customer> eds)
    {
        foreach (var dbContext in dbContexts)
        {
            dbContext.Database.Migrate();
        }
    }
}