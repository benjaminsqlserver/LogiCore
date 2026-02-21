using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LogiCore.Server.Models.Shipments;

namespace LogiCore.Server.Data.Configurations
{
    public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
    {
        public void Configure(EntityTypeBuilder<Shipment> builder)
        {
            builder.ToTable("Shipments");
            builder.HasKey(s => s.ShipmentId);

            builder.Property(s => s.TrackingNumber)
                .IsRequired()
                .HasMaxLength(30);
            builder.HasIndex(s => s.TrackingNumber).IsUnique();

            // Sender
            builder.Property(s => s.SenderName).IsRequired().HasMaxLength(150);
            builder.Property(s => s.SenderEmail).HasMaxLength(200);
            builder.Property(s => s.SenderPhone).HasMaxLength(50);

            // Recipient
            builder.Property(s => s.RecipientName).IsRequired().HasMaxLength(150);
            builder.Property(s => s.RecipientEmail).HasMaxLength(200);
            builder.Property(s => s.RecipientPhone).HasMaxLength(50);

            // Origin address
            builder.Property(s => s.OriginAddress).IsRequired().HasMaxLength(250);
            builder.Property(s => s.OriginCity).IsRequired().HasMaxLength(100);
            builder.Property(s => s.OriginState).HasMaxLength(50);
            builder.Property(s => s.OriginPostalCode).IsRequired().HasMaxLength(20);
            builder.Property(s => s.OriginCountry).HasMaxLength(3).HasDefaultValue("US");

            // Destination address
            builder.Property(s => s.DestinationAddress).IsRequired().HasMaxLength(250);
            builder.Property(s => s.DestinationCity).IsRequired().HasMaxLength(100);
            builder.Property(s => s.DestinationState).HasMaxLength(50);
            builder.Property(s => s.DestinationPostalCode).IsRequired().HasMaxLength(20);
            builder.Property(s => s.DestinationCountry).HasMaxLength(3).HasDefaultValue("US");

            // Enums stored as strings for readability/portability
            builder.Property(s => s.Status)
                .HasConversion<string>()
                .HasMaxLength(30);
            builder.Property(s => s.ServiceType)
                .HasConversion<string>()
                .HasMaxLength(30);

            // Decimal precision
            builder.Property(s => s.TotalWeight).HasColumnType("decimal(10,3)");
            builder.Property(s => s.DeclaredValue).HasColumnType("decimal(12,2)");
            builder.Property(s => s.Length).HasColumnType("decimal(8,2)");
            builder.Property(s => s.Width).HasColumnType("decimal(8,2)");
            builder.Property(s => s.Height).HasColumnType("decimal(8,2)");
            builder.Property(s => s.ShippingCost).HasColumnType("decimal(12,2)");

            builder.Property(s => s.SignedBy).HasMaxLength(150);
            builder.Property(s => s.ProofOfDeliveryUrl).HasMaxLength(500);
            builder.Property(s => s.Notes).HasMaxLength(2000);

            // Audit defaults
            builder.Property(s => s.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(s => s.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(s => s.IsDeleted).HasDefaultValue(false);

            // Global query filter — soft delete
            builder.HasQueryFilter(s => !s.IsDeleted);

            // Relationships
            builder.HasMany(s => s.Events)
                .WithOne()
                .HasForeignKey(e => e.ShipmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(s => s.Packages)
                .WithOne()
                .HasForeignKey(p => p.ShipmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for common filter/sort patterns
            builder.HasIndex(s => s.Status);
            builder.HasIndex(s => s.ServiceType);
            builder.HasIndex(s => s.PickupDate);
            builder.HasIndex(s => s.CreatedAt);
            builder.HasIndex(s => new { s.SenderName, s.RecipientName });
        }
    }

    public class PackageConfiguration : IEntityTypeConfiguration<Package>
    {
        public void Configure(EntityTypeBuilder<Package> builder)
        {
            builder.ToTable("Packages");
            builder.HasKey(p => p.PackageId);

            builder.Property(p => p.Weight).HasColumnType("decimal(10,3)");
            builder.Property(p => p.Length).HasColumnType("decimal(8,2)");
            builder.Property(p => p.Width).HasColumnType("decimal(8,2)");
            builder.Property(p => p.Height).HasColumnType("decimal(8,2)");
            builder.Property(p => p.DeclaredValue).HasColumnType("decimal(12,2)");
            builder.Property(p => p.Description).HasMaxLength(500);
        }
    }

    public class ShipmentEventConfiguration : IEntityTypeConfiguration<ShipmentEvent>
    {
        public void Configure(EntityTypeBuilder<ShipmentEvent> builder)
        {
            builder.ToTable("ShipmentEvents");
            builder.HasKey(e => e.EventId);

            builder.Property(e => e.TrackingNumber).IsRequired().HasMaxLength(30);
            builder.Property(e => e.EventType)
                .HasConversion<string>()
                .HasMaxLength(40);
            builder.Property(e => e.Location).HasMaxLength(200);
            builder.Property(e => e.Notes).HasMaxLength(1000);
            builder.Property(e => e.OperatorName).HasMaxLength(150);

            builder.Property(e => e.Timestamp).HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(e => e.ShipmentId);
            builder.HasIndex(e => e.TrackingNumber);
            builder.HasIndex(e => e.Timestamp);
        }
    }
}
