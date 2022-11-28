using Microsoft.EntityFrameworkCore;

namespace PRDC2022.CustomerApi.Persistence
{
    public class EventDescriptorDbContext : DbContext
    {
        public DbSet<EventDescriptorEntity> EventDescriptors { get; set; }
        public DbSet<SnapshotEntity> Snapshots { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            base.OnConfiguring(options);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EventDescriptorEntity>()
                .HasKey(c => new {c.AggregateId, c.Version, c.AggregateType});
            modelBuilder.Entity<EventDescriptorEntity>()
                .HasIndex(c => new {c.AggregateId});

            modelBuilder.Entity<SnapshotEntity>()
                .HasKey(c => new {c.AggregateId, c.Version, c.AggregateType});
            modelBuilder.Entity<SnapshotEntity>()
                .HasIndex(c => new {c.AggregateId});
        }
    }
}