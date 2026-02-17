using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Data.Configurations;

public class FeatureConfiguration : IEntityTypeConfiguration<Feature>
{
    public void Configure(EntityTypeBuilder<Feature> builder)
    {
        builder.ToTable("Features");

        builder.Property(f => f.Key)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(f => f.Key)
            .IsUnique();

        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(f => f.Description)
            .HasMaxLength(500);

        builder.Property(f => f.Category)
            .HasMaxLength(100);
    }
}
