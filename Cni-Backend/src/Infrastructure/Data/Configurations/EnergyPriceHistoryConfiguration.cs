using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Data.Configurations;

public class EnergyPriceHistoryConfiguration : IEntityTypeConfiguration<EnergyPriceHistory>
{
    public void Configure(EntityTypeBuilder<EnergyPriceHistory> builder)
    {
        builder.ToTable("EnergyPriceHistories");

        builder.Property(p => p.AreaCode)
            .HasMaxLength(50)
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
            .WithMany(m => m.PriceHistories)
            .HasForeignKey(p => p.MarketOperatorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique index on (MarketOperatorId, AreaCode, PriceLocalTimestamp) to prevent duplicates
        builder.HasIndex(p => new { p.MarketOperatorId, p.AreaCode, p.PriceLocalTimestamp })
            .IsUnique();

        // Index for efficient range queries
        builder.HasIndex(p => new { p.MarketOperatorId, p.AreaCode });
    }
}
