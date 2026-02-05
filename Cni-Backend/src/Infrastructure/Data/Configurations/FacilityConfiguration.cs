using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Data.Configurations;

public class FacilityConfiguration : IEntityTypeConfiguration<Facility>
{
    public void Configure(EntityTypeBuilder<Facility> builder)
    {
        builder.ToTable("Facilities");

        builder.Property(p => p.Name)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(p => p.Address)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(p => p.EnergyCapacity)
            .IsRequired();

        builder.Property(p => p.EnergyUnit)
            .IsRequired();

        builder.Property(p => p.FirmId)
            .IsRequired();

        builder.Property(p => p.Topology)
            .HasColumnType("text");

        // Relationship to Firm (Tenant access is through Firm)
        builder.HasOne(f => f.Firm)
            .WithMany(t => t.Facilities)
            .HasForeignKey(f => f.FirmId)
            .OnDelete(DeleteBehavior.Cascade);

        // Navigation to Devices
        builder.HasMany(f => f.Devices)
            .WithOne(d => d.Facility)
            .HasForeignKey(d => d.FacilityId)
            .OnDelete(DeleteBehavior.Cascade);

        // Navigation to DeviceLinks
        builder.HasMany(f => f.DeviceLinks)
            .WithOne(dl => dl.Facility)
            .HasForeignKey(dl => dl.FacilityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
