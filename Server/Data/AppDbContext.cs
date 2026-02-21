using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LogiCore.Server.Models.Shipments;

namespace LogiCore.Server.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Shipment Module
        public DbSet<Shipment> Shipments => Set<Shipment>();
        public DbSet<Package> Packages => Set<Package>();
        public DbSet<ShipmentEvent> ShipmentEvents => Set<ShipmentEvent>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Auto-apply all IEntityTypeConfiguration<T> classes in this assembly
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        /// <summary>
        /// Automatically sets UpdatedAt on every modified entity that has that property.
        /// </summary>
        public override int SaveChanges()
        {
            SetTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void SetTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                var prop = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "UpdatedAt");
                if (prop != null)
                    prop.CurrentValue = DateTime.UtcNow;
            }
        }
    }
}
