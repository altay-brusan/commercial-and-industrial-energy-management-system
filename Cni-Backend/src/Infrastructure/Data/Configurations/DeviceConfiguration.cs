using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Data.Configurations;

public class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.ToTable("Devices");

        builder.Property(d => d.FacilityId)
            .IsRequired();

        builder.Property(d => d.DeviceTypeId)
            .IsRequired();

        builder.Property(d => d.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(d => d.Model)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(d => d.SerialNumber)
            .HasMaxLength(100);

        builder.Property(d => d.Manufacturer)
            .HasMaxLength(200);

        builder.Property(d => d.Ports)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(d => d.TopologyId)
            .HasMaxLength(100);

        builder.Property(d => d.Metadata)
            .HasColumnType("jsonb");

        builder.Property(d => d.Status)
            .HasMaxLength(50);

        // Relationship to Facility
        builder.HasOne(d => d.Facility)
            .WithMany(f => f.Devices)
            .HasForeignKey(d => d.FacilityId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship to DeviceType
        builder.HasOne(d => d.DeviceType)
            .WithMany()
            .HasForeignKey(d => d.DeviceTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship to DeviceModel (nullable for legacy devices)
        builder.HasOne(d => d.DeviceModel)
            .WithMany(dm => dm.Devices)
            .HasForeignKey(d => d.DeviceModelId)
            .OnDelete(DeleteBehavior.Restrict);

        // Self-referencing relationship for composite devices
        builder.HasOne(d => d.ParentDevice)
            .WithMany(d => d.ChildDevices)
            .HasForeignKey(d => d.ParentDeviceId)
            .OnDelete(DeleteBehavior.Restrict);

        // Index for unit listing with status filters
        builder.HasIndex(d => new { d.FacilityId, d.Status });
    }
}
