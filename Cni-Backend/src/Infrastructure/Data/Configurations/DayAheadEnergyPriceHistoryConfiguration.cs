using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Data.Configurations;

public class DayAheadEnergyPriceHistoryConfiguration : IEntityTypeConfiguration<DayAheadEnergyPriceHistory>
{
    public void Configure(EntityTypeBuilder<DayAheadEnergyPriceHistory> builder)
    {
        builder.ToTable("DayAheadEnergyPriceHistories");

        builder.Property(p => p.Country)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.NetPrice)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(p => p.GrossPrice)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(p => p.PriceLocalTimestamp)
            .IsRequired();

        // Foreign key to MarketOperator
        builder.HasOne(p => p.MarketOperator)
            .WithMany(m => m.DayAheadPriceHistories)
            .HasForeignKey(p => p.MarketOperatorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique index on (MarketOperatorId, Country, PriceLocalTimestamp) to prevent duplicates
        builder.HasIndex(p => new { p.MarketOperatorId, p.Country, p.PriceLocalTimestamp })
            .IsUnique();

        // Index for efficient range queries
        builder.HasIndex(p => new { p.MarketOperatorId, p.Country });
    }
}
