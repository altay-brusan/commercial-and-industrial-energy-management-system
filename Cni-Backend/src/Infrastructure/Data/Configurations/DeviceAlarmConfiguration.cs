using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Data.Configurations;

public class DeviceAlarmConfiguration : IEntityTypeConfiguration<DeviceAlarm>
{
    public void Configure(EntityTypeBuilder<DeviceAlarm> builder)
    {
        builder.ToTable("DeviceAlarms");

        builder.Property(da => da.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(da => da.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(da => da.Severity)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(da => da.Category)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(da => da.Source)
            .HasMaxLength(500);

        builder.Property(da => da.FaultKey)
            .HasMaxLength(200);

        builder.Property(da => da.Details)
            .HasColumnType("jsonb");

        // Relationship to Device
        builder.HasOne(da => da.Device)
            .WithMany(d => d.Alarms)
            .HasForeignKey(da => da.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship to Facility (denormalized)
        builder.HasOne(da => da.Facility)
            .WithMany()
            .HasForeignKey(da => da.FacilityId)
            .OnDelete(DeleteBehavior.Restrict);

        // Index for active alarm queries per device
        builder.HasIndex(da => new { da.DeviceId, da.IsActive });

        // Index for paginated fault history by facility
        builder.HasIndex(da => new { da.FacilityId, da.OccurredAt })
            .IsDescending(false, true);
    }
}
