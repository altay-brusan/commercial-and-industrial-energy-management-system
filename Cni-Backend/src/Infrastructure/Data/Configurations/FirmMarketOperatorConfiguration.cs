using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Data.Configurations;

public class FirmMarketOperatorConfiguration : IEntityTypeConfiguration<FirmMarketOperator>
{
    public void Configure(EntityTypeBuilder<FirmMarketOperator> builder)
    {
        builder.ToTable("FirmMarketOperators");

        builder.Property(p => p.FirmId)
            .IsRequired();

        builder.Property(p => p.MarketOperatorId)
            .IsRequired();

        builder.Property(p => p.AreaCode)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.SamplingPeriodMinutes)
            .IsRequired();

        // Foreign key to Firm
        builder.HasOne(p => p.Firm)
            .WithMany()
            .HasForeignKey(p => p.FirmId)
            .OnDelete(DeleteBehavior.Cascade);

        // Foreign key to MarketOperator
        builder.HasOne(p => p.MarketOperator)
            .WithMany(m => m.FirmMarketOperators)
            .HasForeignKey(p => p.MarketOperatorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique index on (FirmId, MarketOperatorId) - one MO config per firm
        builder.HasIndex(p => new { p.FirmId, p.MarketOperatorId })
            .IsUnique();
    }
}
