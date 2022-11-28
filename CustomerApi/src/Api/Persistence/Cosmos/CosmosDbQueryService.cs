using System.Net;
using DepthConsulting.Core.DDD;
using DepthConsulting.Core.Services.Persistence.CQRS;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Scripts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PRDC2022.CustomerApi.Options;
using Serilog.Context;
using Serilog.Core.Enrichers;

namespace PRDC2022.CustomerApi.Persistence.Cosmos;

public class CosmosDbQueryService : IReadModelQuery, IReadModelPersistence
{
    private const string ContainerName = "CustomerProjections";
    private static readonly object LockObj = new();
    private readonly Dictionary<string, Container> _containers = new();
    private readonly string _databaseName;
    private readonly CosmosClient _dbClient;
    private readonly ILogger _logger;

    public CosmosDbQueryService(
        CosmosClient dbClient,
        IOptions<CosmosDbOptions> options,
        ILogger<CosmosDbQueryService> logger)
    {
        _databaseName = options.Value.DatabaseName;
        _dbClient = dbClient;
        _logger = logger;
        CreateAllContainers();
    }

    public async Task PutAsync<T>(T t, string? partitionKeyValue = null) where T : TypeBasedProjectionBase
    {
        using var prop = LogContext.Push(
            new PropertyEnricher("Component", nameof(CosmosDbQueryService)),
            new PropertyEnricher("Operation", nameof(PutAsync)));

        var id = t.id;
        _logger.LogDebug("Putting Projection {Type}:{AggregateId}", typeof(T).Name, id);

        try
        {
            var container = GetContainer(ContainerName);
            var itemResponse = await container.UpsertItemAsync(t, new PartitionKey(partitionKeyValue ?? id));
            LogRUUsage(itemResponse.RequestCharge, UsageType.Upsert);
        }
        catch (CosmosException ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    public async Task PutAggregateAsync<T>(T t, string? partitionKeyValue = null) where T : ProjectionBase
    {
        using var prop = LogContext.Push(
            new PropertyEnricher("Component", nameof(CosmosDbQueryService)),
            new PropertyEnricher("Operation", nameof(PutAggregateAsync)));

        var id = t.AggregateId;
        var repoType = typeof(T);
        _logger.LogDebug("Putting Projection {Type}:{AggregateId}", repoType.Name, id);

        try
        {
            var container = GetContainer(ContainerName);
            var itemResponse = await container.UpsertItemAsync(t, new PartitionKey(partitionKeyValue ?? id));
            LogRUUsage(itemResponse.RequestCharge, UsageType.Upsert);
        }
        catch (CosmosException ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    public async Task DeleteAsync<T>(T t) where T : TypeBasedProjectionBase
    {
        using var prop = LogContext.Push(
            new PropertyEnricher("Component", nameof(CosmosDbQueryService)),
            new PropertyEnricher("Operation", nameof(DeleteAsync)));

        _logger.LogDebug("Deleting Projection {Type}:{AggregateId}", t, t);

        var response = await GetContainer(ContainerName)
            .DeleteItemAsync<T>(t.id, new PartitionKey(t.id));
        LogRUUsage(response.RequestCharge, UsageType.Delete);
    }

    public async Task DeleteAggregateAsync<T>(T t) where T : ProjectionBase
    {
        using var prop = LogContext.Push(
            new PropertyEnricher("Component", nameof(CosmosDbQueryService)),
            new PropertyEnricher("Operation", nameof(DeleteAggregateAsync)));

        var id = t.AggregateId;
        var repoType = typeof(T);
        _logger.LogDebug("Deleting Projection {Type}:{AggregateId}", repoType, id);

        var response = await GetContainer(ContainerName)
            .DeleteItemAsync<T>(id, new PartitionKey(id));
        LogRUUsage(response.RequestCharge, UsageType.Delete);
    }

    public async Task<T?> GetItemAsync<T>() where T : TypeBasedProjectionBase, new()
    {
        using var prop = LogContext.Push(
            new PropertyEnricher("Component", nameof(CosmosDbQueryService)),
            new PropertyEnricher("Operation", nameof(GetItemAsync)));

        var id = typeof(T).Name;
        _logger.LogDebug("Getting Projection {Type}:{AggregateId}", id, id);
        try
        {
            var response = await GetContainer(ContainerName)
                .ReadItemAsync<T>(id, new PartitionKey(id));
            LogRUUsage(response.RequestCharge);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<T?> GetItemAsync<T>(string aggregateId) where T : ProjectionBase, new()
    {
        using var prop = LogContext.Push(
            new PropertyEnricher("Component", nameof(CosmosDbQueryService)),
            new PropertyEnricher("Operation", nameof(GetItemAsync)));

        var repoType = typeof(T);
        _logger.LogDebug("Getting Projection {Type}:{AggregateId}", repoType, aggregateId);
        try
        {
            double totalRequestUnitsUsed = 0;
            var query = new QueryDefinition($"select * from CustomerProjections c where c.Type = '{repoType.Name}' and c.id = '{aggregateId}'");
            var queryResults = GetContainer(ContainerName)
            .GetItemQueryIterator<T>(query);

            var results = new List<T>();
            while (queryResults.HasMoreResults)
            {
                var response = await queryResults.ReadNextAsync();
                totalRequestUnitsUsed += response.RequestCharge;
                results.AddRange(response.ToList());
            }

            LogRUUsage(totalRequestUnitsUsed);

            return results.FirstOrDefault();

        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<int> GetItemCount<T>(string whereClause = "") where T : ProjectionBase, new()
    {
        using var prop = LogContext.Push(
            new PropertyEnricher("Component", nameof(CosmosDbQueryService)),
            new PropertyEnricher("Operation", nameof(GetItemCount)));

        var repoType = typeof(T);
        _logger.LogDebug("Counting {Type}:{QueryString}", repoType, whereClause);
        var strQuery = $"SELECT VALUE COUNT(1) from {ContainerName} c where c.Type = '{repoType.Name}'" + whereClause;

        double totalRequestUnitsUsed = 0;
        var query = new QueryDefinition(strQuery);
        var queryResults = GetContainer(ContainerName)
            .GetItemQueryIterator<int>(query);

        var count = 0;
        while (queryResults.HasMoreResults)
        {
            var response = await queryResults.ReadNextAsync();
            totalRequestUnitsUsed += response.RequestCharge;
            count = response.Resource.First();
        }

        LogRUUsage(totalRequestUnitsUsed);

        return count;
    }

    public async Task<IEnumerable<T>?> GetItemsAsync<T>(string? queryString) where T : ReadModelBase, new()
    {
        using var prop = LogContext.Push(
            new PropertyEnricher("Component", nameof(CosmosDbQueryService)),
            new PropertyEnricher("Operation", nameof(GetItemsAsync)));

        var repoType = typeof(T);
        _logger.LogDebug("Getting many of {Type}:{QueryString}", repoType, queryString);

        double totalRequestUnitsUsed = 0;
        var query = string.IsNullOrEmpty(queryString) ? new QueryDefinition($"select * from CustomerProjections c where c.Type = '{repoType.Name}'") : new QueryDefinition(queryString);
        var queryResults = GetContainer(ContainerName)
            .GetItemQueryIterator<T>(query);

        var results = new List<T>();
        while (queryResults.HasMoreResults)
        {
            var response = await queryResults.ReadNextAsync();
            totalRequestUnitsUsed += response.RequestCharge;
            results.AddRange(response.ToList());
        }

        LogRUUsage(totalRequestUnitsUsed);

        return results;
    }

    public Task<string?> GetRawJsonAsync<T>() where T : TypeBasedProjectionBase, new()
    {
        throw new NotImplementedException();
    }

    public Task<string?> GetRawJsonAsync<T>(string id) where T : ProjectionBase, new()
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<T>?> GetRawJsonAllAsync<T>(string query) where T : ReadModelBase, new()
    {
        throw new NotImplementedException();
    }

    private void CreateAllContainers()
    {
        using var prop = LogContext.Push(
            new PropertyEnricher("Component", nameof(CosmosDbQueryService)),
            new PropertyEnricher("Operation", nameof(CreateAllContainers)));

        var containersToCreate = new[]
        {
            ContainerName
        };

        foreach (var type in containersToCreate) GetContainer(type);
    }

    private Container GetContainer(string containerName)
    {
        using var prop = LogContext.Push(
            new PropertyEnricher("Operation", nameof(GetContainer)));

        if (_containers.ContainsKey(containerName)) return _containers[containerName];

        lock (LockObj)
        {
            // something may have just created a container again.. check before we create
            if (_containers.ContainsKey(containerName)) return _containers[containerName];

            // check if DB exists
            DatabaseResponse database = null;
            try
            {
                database = _dbClient.CreateDatabaseIfNotExistsAsync(_databaseName).GetAwaiter().GetResult();
                // check if container 
                database.Database.CreateContainerIfNotExistsAsync(containerName, "/id").GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create/open CosmosDb database/container");
                throw;
            }

            var container = _dbClient.GetContainer(_databaseName, containerName);

            CreateUDFs(container);

            _containers.Add(containerName, container);

            _logger.LogDebug("Created CosmosDb Database:{DatabaseName} and Container:{Container}", database.Database.Id, container.Id);

            return container;
        }
    }

    private static void CreateUDFs(Container container)
    {
        using var prop = LogContext.Push(
            new PropertyEnricher("Operation", nameof(CreateUDFs)));

        // create UDFs in container
        // list files in folder
        var currentDir = Directory.GetCurrentDirectory();
        var path = Path.Combine(currentDir, "Persistence", "Cosmos", "UDFs");
        var files = Directory.GetFiles(path)
            .Select(f => new FileInfo(f));

        foreach (var file in files)
        {
            var id = file.Name.Split('.').First();
            var body = File.ReadAllText(file.FullName);
            try
            {
                // if it exists, delete it so we update it
                _ = container.Scripts.ReadUserDefinedFunctionAsync(id).GetAwaiter().GetResult();
                container.Scripts.DeleteUserDefinedFunctionAsync(id).GetAwaiter().GetResult();
            }
            catch
            {
                // noop
            }
            finally
            {
                // recreate it
                container.Scripts.CreateUserDefinedFunctionAsync(new UserDefinedFunctionProperties
                {
                    Id = id,
                    Body = body
                }).GetAwaiter().GetResult();
            }
        }
    }

    private void LogRUUsage(double ruUsed, UsageType type = UsageType.Query)
    {
        _logger.LogInformation("CosmosDbQueryService {UsageType} used {RequestUnits}", type.ToString(), ruUsed);
    }
}

public enum UsageType
{
    Upsert,
    Query,
    Delete
}