using System.Data;
using System.Diagnostics;
using Autofac;
using Dapper;
using DepthConsulting.Core.DDD;
using DepthConsulting.Core.DDD.Exceptions;
using DepthConsulting.Core.Services.Persistence.EventSource;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core.Enrichers;

namespace PRDC2022.CustomerApi.Persistence
{
    /// <inheritdoc />
    public class EFEventDescriptorStore<T> : IEventDescriptorStorage<T> where T : AggregateBase
    {
        private readonly ILifetimeScope _container;
        private readonly ITenantContext _ctx;

        public EFEventDescriptorStore(ILifetimeScope container, ITenantContext ctx)
        {
            _container = container;
             _ctx = ctx;
        }

        public async Task<IEnumerable<string>> GetAggregateIdsAsync(int page, int count)
        {
            var repoType = GetType().GetGenericTypeDefinition().ToString();
            await using var db = _container.Resolve<EventDescriptorDbContext>();
            db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            var query = db.EventDescriptors
                .Distinct()
                .Where(ed => ed.TenantId == _ctx.TenantId)
                .Where(ed => ed.AggregateType == repoType)
                .OrderBy(ed => ed.AggregateId)
                .Skip((page - 1) * count)
                .Take(count)
                .Select(ed => ed.AggregateId);

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<EventDescriptor>> GetEventDescriptorsAsync(string aggregateId)
        {
            Serilog.Context.LogContext.Push(new PropertyEnricher("Component", nameof(EFEventDescriptorStore<T>)), new PropertyEnricher("Operation", nameof(GetEventDescriptorsAsync)));
            var logger = _container.Resolve<ILogger<EFEventDescriptorStore<T>>>();
            using var conn = _container.Resolve<IDbConnection>();
            var sw = new Stopwatch();
            sw.Start();
            var bees = (await conn.QueryAsync<BaseEventEntity>(
                @"SELECT * FROM [dbo].[EventDescriptors] ed
                  where AggregateId =  @AggregateId and Version > ISNULL((Select TOP(1) ISNULL(Version, -1) as Version FROM [dbo].[Snapshots] where AggregateId =  @AggregateId order by Version desc), -1)
                  UNION
                  select * from (SELECT TOP (1) * FROM [dbo].[Snapshots]
                  where AggregateId =  @AggregateId
                  order by Version desc) as s", new {AggregateId = aggregateId}))
                .ToList();
            sw.Stop();
            var elapsedMilliseconds = sw.ElapsedMilliseconds;
            sw.Restart();
            var events = bees.OrderBy(s => s.Version).Select(x => x.ToDescriptor()).ToList();
            sw.Stop();
            logger.LogInformation("Queried {EventCount} events for {AggregateId} in {ElapsedMilliseconds} ms. Json Deserialized in {ParsedElapsedMilliseconds} ms",
                events.Count,
                aggregateId,
                elapsedMilliseconds,
                sw.ElapsedMilliseconds);
            

            return events;
        }

        public async Task<int> GetMostRecentVersion(string aggregateId)
        {
            Serilog.Context.LogContext.Push(new PropertyEnricher("Component", nameof(EFEventDescriptorStore<T>)), new PropertyEnricher("Operation", nameof(GetEventDescriptorsAsync)));
            var logger = _container.Resolve<ILogger<EFEventDescriptorStore<T>>>();
            using var conn = _container.Resolve<IDbConnection>();
            var sw = new Stopwatch();
            sw.Start();
            var maxVersion = (await conn.ExecuteScalarAsync(
                @"SELECT TOP(1) a.Version from (SELECT Version from (SELECT TOP(1) Version FROM [dbo].[EventDescriptors]  
                              where AggregateId =  @AggregateId order by Version desc) as eds
                              UNION
                              SELECT Version from (select TOP(1) Version from [dbo].[Snapshots] 
                              where AggregateId =  @AggregateId order by Version desc) as ss) a
                              order by Version desc", new { AggregateId = aggregateId }));
            sw.Stop();
            var elapsedMilliseconds = sw.ElapsedMilliseconds;
            var versionFound = -1;
            if (maxVersion != null)
            {
                _ = int.TryParse(maxVersion.ToString(), out versionFound);
            }
            logger.LogInformation("Queried Max Version({Version}) for {AggregateId} in {ElapsedMilliseconds} ms.",
                versionFound,
                aggregateId,
                sw.ElapsedMilliseconds);
            return versionFound;
        }

        public async Task AddDescriptorAsync(string aggregateId, EventDescriptor ed)
        {
            var eds = new[] { ed };
            await AddDescriptorsAsync(aggregateId, eds);
        }

        public async Task AddDescriptorsAsync(string aggregateId, IEnumerable<EventDescriptor> eds)
        {
            var logger = _container.Resolve<ILogger<EFEventDescriptorStore<T>>>();
            var sw = new Stopwatch();
            sw.Start();

            var counter = 0;
            await using var db = _container.Resolve<EventDescriptorDbContext>();
            foreach (var ed in eds)
            {
                counter++;
                AddEventDescriptorEntityToDb(ed, db);
            }

            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw new ConcurrencyException(aggregateId, -1, -2, e);
            }
            sw.Stop();
            Serilog.Context.LogContext.Push(new PropertyEnricher("Component", nameof(EFEventDescriptorStore<T>)), new PropertyEnricher("Operation", nameof(AddDescriptorsAsync)));
            logger.LogInformation("Saved {Count} events for {AggregateId} in {ElapsedMilliseconds}", counter, aggregateId, sw.ElapsedMilliseconds);
        }

        private static void AddEventDescriptorEntityToDb(EventDescriptor ed, EventDescriptorDbContext db)
        {
            if (ed.EventData.IsSnapshot)
                db.Snapshots.Add(ed.ToSnapshotEntity());
            else
                db.EventDescriptors.Add(ed.ToEventDescriptorEntity());
        }
    }

    public interface ITenantContext
    {
        public Guid TenantId { get; }
    }

    public class TenantContext : ITenantContext
    {
        public Guid TenantId { get; }

        public TenantContext(Guid tenantId)
        {
            TenantId = tenantId;
        }
    }
}