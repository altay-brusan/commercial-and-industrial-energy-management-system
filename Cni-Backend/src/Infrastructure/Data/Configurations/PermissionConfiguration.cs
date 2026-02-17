using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Data.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");

        builder.Property(p => p.Key)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(p => p.Key)
            .IsUnique();

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.HasOne(p => p.Feature)
            .WithMany(f => f.Permissions)
            .HasForeignKey(p => p.FeatureId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
