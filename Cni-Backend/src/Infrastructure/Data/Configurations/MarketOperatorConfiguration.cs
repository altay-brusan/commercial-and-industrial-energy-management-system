using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Data.Configurations;

public class MarketOperatorConfiguration : IEntityTypeConfiguration<MarketOperator>
{
    public void Configure(EntityTypeBuilder<MarketOperator> builder)
    {
        builder.ToTable("MarketOperators");

        builder.Property(p => p.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.BaseUrl)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(p => p.ApiKey)
            .HasMaxLength(500);

        // JSON columns for PostgreSQL
        builder.Property(p => p.SupportedResolutions)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(p => p.AreaCodes)
            .HasColumnType("jsonb")
            .IsRequired();

        // Unique index on Code
        builder.HasIndex(p => p.Code)
            .IsUnique();
    }
}
