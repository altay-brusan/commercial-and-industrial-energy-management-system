using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Data.Configurations;

public class DeviceSpecificationTemplateConfiguration : IEntityTypeConfiguration<DeviceSpecificationTemplate>
{
    public void Configure(EntityTypeBuilder<DeviceSpecificationTemplate> builder)
    {
        builder.ToTable("DeviceSpecificationTemplates");

        builder.Property(t => t.Model)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Manufacturer)
            .HasMaxLength(200);

        builder.Property(t => t.SpecificationsTemplate)
            .HasColumnType("jsonb")
            .HasDefaultValue("{}");

        builder.Property(t => t.MetadataTemplate)
            .HasColumnType("jsonb")
            .HasDefaultValue("{}");

        builder.Property(t => t.HierarchyTemplate)
            .HasColumnType("jsonb");

        builder.Property(t => t.ParameterMapTemplate)
            .HasColumnType("jsonb");

        builder.HasOne(t => t.DeviceType)
            .WithMany()
            .HasForeignKey(t => t.DeviceTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => new { t.DeviceTypeId, t.Model, t.Manufacturer })
            .IsUnique();
    }
}
