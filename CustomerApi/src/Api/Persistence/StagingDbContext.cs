using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PRDC2022.Customer.Domain.Projections;
using PRDC2022.Customer.Domain.Shared;
using PRDC2022.CustomerApi.Options;

namespace PRDC2022.CustomerApi.Persistence
{
    public class StagingDbContext : DbContext
    {
        public DbSet<CustomerAddressAnalyticsEntity> CustomersAnalytics { get; set; }
        public DbSet<CustomerDetailsEntity> CustomerDetails { get; set; }
        public DbSet<Phone> Phones { get; set; }
        public DbSet<Address> Addresses { get; set; }

        private readonly DatabaseOptions _options;

        public StagingDbContext()
        {
        }

        public StagingDbContext(IOptions<DatabaseOptions> options)
        {
            _options = options.Value;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlServer(_options == null
                //? "Server=(localdb)\\mssqllocaldb;Database=CustomerApiStaging;Trusted_Connection=True;"
                ? "Server=127.0.0.1;Database=CustomerApiStaging;User=sa;Password=P@ssw0rd;"
                : _options.ConnectionStrings.StagingContext);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CustomerAddressAnalyticsEntity>()
                .HasKey(c => new { c.CustomerId });
            //modelBuilder.Entity<Phone>()
            //    .HasKey(c => new { c.PhoneId, c.Customerdetailsid });
        }
    }
}