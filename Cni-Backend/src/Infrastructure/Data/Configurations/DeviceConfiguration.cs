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
            .HasColumnType("text")
            .IsRequired();

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

        // Note: SerialNumber uniqueness not enforced at DB level due to existing duplicate data
        // Add unique constraint later after data cleanup if needed
    }
}
