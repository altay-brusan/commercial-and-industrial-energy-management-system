using System.Text.Json;
using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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

        builder.Property(p => p.PriceUnit)
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue("MWh");

        // JSON columns for PostgreSQL - need explicit conversion for List<T> types
        builder.Property(p => p.SupportedResolutions)
            .HasColumnType("jsonb")
            .IsRequired()
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<int>>(v, (JsonSerializerOptions?)null) ?? new List<int>())
            .Metadata.SetValueComparer(new ValueComparer<List<int>>(
                (c1, c2) => c1!.SequenceEqual(c2!),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList()));

        builder.Property(p => p.AreaCodes)
            .HasColumnType("jsonb")
            .IsRequired()
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>())
            .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                (c1, c2) => c1!.SequenceEqual(c2!),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList()));

        // Unique index on Code
        builder.HasIndex(p => p.Code)
            .IsUnique();
    }
}
