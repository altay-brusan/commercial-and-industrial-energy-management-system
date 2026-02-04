using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Data.Configurations;

public class TenantMarketOperatorConfiguration : IEntityTypeConfiguration<TenantMarketOperator>
{
    public void Configure(EntityTypeBuilder<TenantMarketOperator> builder)
    {
        builder.ToTable("TenantMarketOperators");

        builder.Property(p => p.AreaCode)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.SamplingFrequencyMinutes)
            .IsRequired();

        // Foreign key to Tenant
        builder.HasOne(p => p.Tenant)
            .WithMany()
            .HasForeignKey(p => p.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Foreign key to MarketOperator
        builder.HasOne(p => p.MarketOperator)
            .WithMany(m => m.TenantMarketOperators)
            .HasForeignKey(p => p.MarketOperatorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique index on (TenantId, MarketOperatorId) - one MO config per tenant
        builder.HasIndex(p => new { p.TenantId, p.MarketOperatorId })
            .IsUnique();
    }
}
