using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Data.Configurations;

public class TariffConfiguration : IEntityTypeConfiguration<Tariff>
{
    public void Configure(EntityTypeBuilder<Tariff> builder)
    {
        builder.ToTable("Tariffs");

        builder.Property(t => t.CountryName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(t => t.DistributorName)
            .HasMaxLength(200);

        builder.Property(t => t.DistributionFee)
            .HasPrecision(18, 4);

        builder.Property(t => t.TransmissionFee)
            .HasPrecision(18, 4);

        builder.Property(t => t.TaxRate)
            .HasPrecision(5, 4);

        builder.Property(t => t.Currency)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(t => t.IsActive)
            .HasDefaultValue(true);

        // Index for efficient lookups
        builder.HasIndex(t => t.CountryName);
        builder.HasIndex(t => t.IsActive);
    }
}
