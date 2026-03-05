using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Data.Configurations;

public class DeviceAlarmConfiguration : IEntityTypeConfiguration<DeviceAlarm>
{
    public void Configure(EntityTypeBuilder<DeviceAlarm> builder)
    {
        builder.ToTable("DeviceAlarms");

        builder.Property(da => da.IsRead)
            .HasDefaultValue(false);

        builder.Property(da => da.IsProcessed)
            .HasDefaultValue(false);

        builder.Property(da => da.Details)
            .HasColumnType("jsonb");

        // Relationship to Device
        builder.HasOne(da => da.Device)
            .WithMany(d => d.Alarms)
            .HasForeignKey(da => da.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship to AlarmType catalog (required — every alarm is typed)
        builder.HasOne(da => da.AlarmType)
            .WithMany()
            .HasForeignKey(da => da.DeviceAlarmTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Index for active alarm queries per device
        builder.HasIndex(da => new { da.DeviceId, da.IsActive });

        // Index for paginated fault history by device
        builder.HasIndex(da => new { da.DeviceId, da.OccurredAt })
            .IsDescending(false, true);

        // Index for dedup during alarm processing (one active alarm per type per device)
        builder.HasIndex(da => new { da.DeviceId, da.DeviceAlarmTypeId, da.IsActive });

        // Acknowledgement / Resolution user FKs
        builder.HasOne(da => da.AcknowledgedBy)
            .WithMany()
            .HasForeignKey(da => da.AcknowledgedById)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(da => da.ResolvedBy)
            .WithMany()
            .HasForeignKey(da => da.ResolvedById)
            .OnDelete(DeleteBehavior.SetNull);

        // MaintenanceTask FK (configured from MaintenanceTaskConfiguration as principal)
    }
}
