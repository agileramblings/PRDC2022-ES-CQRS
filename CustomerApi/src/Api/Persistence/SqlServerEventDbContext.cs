using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PRDC2022.CustomerApi.Options;

namespace PRDC2022.CustomerApi.Persistence
{
    public class SqlServerEventDbContext : EventDescriptorDbContext
    {
        private readonly DatabaseOptions? _options;

        public SqlServerEventDbContext()
        {
            _options = null;
        }

        public SqlServerEventDbContext(IOptions<DatabaseOptions> options)
        {
            _options = options.Value;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlServer(_options == null
                //? "Server=(localdb)\\mssqllocaldb;Database=CustomerApiEvents;Trusted_Connection=True;"
                ? "Server=127.0.0.1;Database=CustomerApiStaging;User=sa;Password=P@ssw0rd;"
                : _options.ConnectionStrings.EventsContext);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<EventDescriptorEntity>()
                .HasKey(table => new
                {
                    table.AggregateId,
                    table.AggregateType,
                    table.Version
                })
                .HasName("PKC_EventDescriptors_AggregateId"); //  PKC_<schema name>_<table name>_<column name> 

            builder.Entity<SnapshotEntity>()
                .HasKey(table => new
                {
                    table.AggregateId,
                    table.AggregateType,
                    table.Version
                })
                .HasName("PKC_Snapshots_AggregateId");
        }
    }
}