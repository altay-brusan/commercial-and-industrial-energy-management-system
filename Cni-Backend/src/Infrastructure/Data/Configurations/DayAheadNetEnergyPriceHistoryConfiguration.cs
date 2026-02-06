using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Data.Configurations;

public class DayAheadNetEnergyPriceHistoryConfiguration : IEntityTypeConfiguration<DayAheadNetEnergyPriceHistory>
{
    public void Configure(EntityTypeBuilder<DayAheadNetEnergyPriceHistory> builder)
    {
        builder.ToTable("DayAheadNetEnergyPriceHistories");

        builder.Property(p => p.Country)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.NetPrice)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(p => p.TimestampUtc)
            .IsRequired();

        // Foreign key to MarketOperator
        builder.HasOne(p => p.MarketOperator)
            .WithMany(m => m.DayAheadPriceHistories)
            .HasForeignKey(p => p.MarketOperatorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique index on (MarketOperatorId, Country, TimestampUtc) to prevent duplicates
        builder.HasIndex(p => new { p.MarketOperatorId, p.Country, p.TimestampUtc })
            .IsUnique();

        // Index for efficient range queries
        builder.HasIndex(p => new { p.MarketOperatorId, p.Country });
    }
}
