using Backend.Domain.Entities.Huawei;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Data.Configurations.Huawei;

public class HuaweiDeviceMappingConfiguration : IEntityTypeConfiguration<HuaweiDeviceMapping>
{
    public void Configure(EntityTypeBuilder<HuaweiDeviceMapping> builder)
    {
        builder.ToTable("DeviceMappings", "huawei");

        builder.Property(d => d.DeviceName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(d => d.SerialNumber)
            .HasMaxLength(100);

        builder.Property(d => d.Model)
            .HasMaxLength(200);

        builder.HasOne(d => d.StationMapping)
            .WithMany(s => s.DeviceMappings)
            .HasForeignKey(d => d.StationMappingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Device)
            .WithMany()
            .HasForeignKey(d => d.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(d => new { d.StationMappingId, d.HuaweiDeviceId, d.DeviceId })
            .IsUnique();

        builder.HasIndex(d => d.DeviceId)
            .IsUnique();
    }
}
