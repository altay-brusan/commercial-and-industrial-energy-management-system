using Backend.Domain.Entities.DeviceData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Data.Configurations;

/// <summary>
/// Base configuration for all device telemetry data tables.
/// Optimized for time-series data with proper indexing.
/// </summary>
public abstract class DeviceDataBaseConfiguration<T> : IEntityTypeConfiguration<T> where T : DeviceDataBase
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        // BIGINT primary key for high-volume data
        builder.Property(e => e.Id)
            .UseIdentityAlwaysColumn();

        // DeviceId FK
        builder.Property(e => e.DeviceId)
            .IsRequired();

        // Device local timestamp
        builder.Property(e => e.DeviceLocalTime)
            .IsRequired();

        // Message type discriminator
        builder.Property(e => e.Kind)
            .HasMaxLength(100);

        // JSON payload - use PostgreSQL JSONB type for efficient querying
        builder.Property(e => e.Payload)
            .HasColumnType("jsonb")
            .IsRequired()
            .HasDefaultValue("{}");

        // Audit fields
        builder.Property(e => e.Created).IsRequired();
        builder.Property(e => e.CreatedBy).HasMaxLength(450);
        builder.Property(e => e.LastModified).IsRequired();
        builder.Property(e => e.LastModifiedBy).HasMaxLength(450);

        // Primary index for time-series queries: DeviceId + DeviceLocalTime DESC
        builder.HasIndex(e => new { e.DeviceId, e.DeviceLocalTime })
            .IsDescending(false, true)
            .HasDatabaseName($"IX_{typeof(T).Name}_DeviceId_DeviceLocalTime");

        // Index for kind-filtered queries
        builder.HasIndex(e => new { e.DeviceId, e.Kind, e.DeviceLocalTime })
            .IsDescending(false, false, true)
            .HasDatabaseName($"IX_{typeof(T).Name}_DeviceId_Kind_DeviceLocalTime");

        // Foreign keys
        builder.HasOne(e => e.Device)
            .WithMany()
            .HasForeignKey(e => e.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class DeviceBatteryDataConfiguration : DeviceDataBaseConfiguration<DeviceBatteryData>
{
    public override void Configure(EntityTypeBuilder<DeviceBatteryData> builder)
    {
        builder.ToTable("DeviceBatteryData");
        base.Configure(builder);
    }
}

public class DeviceSolarDataConfiguration : DeviceDataBaseConfiguration<DeviceSolarData>
{
    public override void Configure(EntityTypeBuilder<DeviceSolarData> builder)
    {
        builder.ToTable("DeviceSolarData");
        base.Configure(builder);
    }
}

public class DevicePcsDataConfiguration : DeviceDataBaseConfiguration<DevicePcsData>
{
    public override void Configure(EntityTypeBuilder<DevicePcsData> builder)
    {
        builder.ToTable("DevicePcsData");
        base.Configure(builder);
    }
}

public class DeviceSmartMeterDataConfiguration : DeviceDataBaseConfiguration<DeviceSmartMeterData>
{
    public override void Configure(EntityTypeBuilder<DeviceSmartMeterData> builder)
    {
        builder.ToTable("DeviceSmartMeterData");
        base.Configure(builder);
    }
}
