using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backend.Domain.Entities.Topology;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Backend.Domain.Entities;

namespace Backend.Infrastructure.Data.Configurations;

public class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.ToTable("Devices");

        // Temel özellikler
        builder.Property(n => n.TenantId)
            .IsRequired();
        builder.Property(n => n.DeviceTypeId)
            .IsRequired();

        builder.Property(n => n.MachineId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(n => n.DeviceName)
            .HasMaxLength(100);

        builder.Property(n => n.GrpcConnection)
            .IsRequired();

        builder.Property(n => n.GrpcDeviceStatus)
            .IsRequired();

        // Facility ile ilişki
        builder.HasOne(n => n.Facility)
            .WithMany()
            .HasForeignKey(n => n.FacilityId)
            .OnDelete(DeleteBehavior.Restrict);

        // Tenant ile ilişki
        builder.HasOne(n => n.Tenant)
            .WithMany()
            .HasForeignKey(n => n.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(n => n.DeviceType)
            .WithMany()
            .HasForeignKey(n => n.DeviceTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.SourceDevice)
             .WithOne(d => d.ConnectedDevice) // Ters ilişkiyi tanımlıyoruz
             .HasForeignKey<Device>(d => d.SourceDeviceId)
             .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(d => d.TopologyNode)
           .WithOne(t => t.Device)
           .HasForeignKey<TopologyNode>(t => t.DeviceId);
    }
}
