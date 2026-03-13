using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LogiCore.Server.Models.Customers;

namespace LogiCore.Server.Data.Configurations
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.ToTable("Customers");
            builder.HasKey(c => c.CustomerId);

            // Identity
            builder.Property(c => c.FullName).IsRequired().HasMaxLength(150);
            builder.Property(c => c.Email).IsRequired().HasMaxLength(200);
            builder.HasIndex(c => c.Email).IsUnique();                // email must be globally unique
            builder.Property(c => c.Phone).HasMaxLength(50);
            builder.Property(c => c.CompanyName).HasMaxLength(200);
            builder.Property(c => c.TaxId).HasMaxLength(50);

            // Enums stored as strings for portability / readability in SSMS
            builder.Property(c => c.AccountType)
                   .HasConversion<string>()
                   .HasMaxLength(20);
            builder.Property(c => c.Status)
                   .HasConversion<string>()
                   .HasMaxLength(20);

            // Financials
            builder.Property(c => c.CreditLimit).HasColumnType("decimal(12,2)");
            builder.Property(c => c.CurrentBalance).HasColumnType("decimal(12,2)");

            // Address
            builder.Property(c => c.AddressLine1).IsRequired().HasMaxLength(250);
            builder.Property(c => c.AddressLine2).HasMaxLength(250);
            builder.Property(c => c.City).IsRequired().HasMaxLength(100);
            builder.Property(c => c.State).HasMaxLength(50);
            builder.Property(c => c.PostalCode).IsRequired().HasMaxLength(20);
            builder.Property(c => c.Country).HasMaxLength(3).HasDefaultValue("US");

            // Notes
            builder.Property(c => c.Notes).HasMaxLength(2000);

            // Audit defaults
            builder.Property(c => c.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(c => c.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(c => c.IsDeleted).HasDefaultValue(false);

            // Global query filter — soft delete
            builder.HasQueryFilter(c => !c.IsDeleted);

            // Indexes for common filter / sort patterns
            builder.HasIndex(c => c.Status);
            builder.HasIndex(c => c.AccountType);
            builder.HasIndex(c => c.CreatedAt);
            builder.HasIndex(c => new { c.FullName, c.CompanyName });
        }
    }
}
