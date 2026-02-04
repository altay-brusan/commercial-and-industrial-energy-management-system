using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Data.Configurations;

public class CountryTariffConfiguration : IEntityTypeConfiguration<CountryTariff>
{
    public void Configure(EntityTypeBuilder<CountryTariff> builder)
    {
        builder.ToTable("CountryTariffs");

        builder.Property(p => p.CountryCode)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(p => p.CountryName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.DistributionFee)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(p => p.TransmissionFee)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(p => p.TaxRate)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(p => p.Currency)
            .HasMaxLength(10)
            .HasDefaultValue("EUR")
            .IsRequired();

        // Foreign key to Tenant
        builder.HasOne(p => p.Tenant)
            .WithMany()
            .HasForeignKey(p => p.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique index on (TenantId, CountryCode)
        builder.HasIndex(p => new { p.TenantId, p.CountryCode })
            .IsUnique();
    }
}
