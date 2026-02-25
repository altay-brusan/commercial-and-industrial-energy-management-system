using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Data.Configurations;

public class DeviceAlarmTypeConfiguration : IEntityTypeConfiguration<DeviceAlarmType>
{
    public void Configure(EntityTypeBuilder<DeviceAlarmType> builder)
    {
        builder.ToTable("DeviceAlarmTypes");

        builder.Property(t => t.Code)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(t => t.FaultKey)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Kind)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(t => t.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Model)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Manufacturer)
            .HasMaxLength(200);

        builder.Property(t => t.Category)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(t => t.Severity)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.Source)
            .HasMaxLength(200);

        builder.Property(t => t.DetectionStrategy)
            .HasMaxLength(50)
            .HasDefaultValue("BooleanFlag")
            .IsRequired();

        builder.Property(t => t.MatchPattern)
            .HasMaxLength(500);

        builder.HasOne(t => t.DeviceType)
            .WithMany()
            .HasForeignKey(t => t.DeviceTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique: one fault key per device model+manufacturer
        builder.HasIndex(t => new { t.DeviceTypeId, t.Model, t.Manufacturer, t.FaultKey })
            .IsUnique();

        // Fast lookup during ingestion by Kind
        builder.HasIndex(t => t.Kind);

        // Bulk loading by device model
        builder.HasIndex(t => new { t.DeviceTypeId, t.Model, t.Manufacturer });
    }
}
