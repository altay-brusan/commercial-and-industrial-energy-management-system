using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Data.Configurations;

public class DeviceModelConfiguration : IEntityTypeConfiguration<DeviceModel>
{
    public void Configure(EntityTypeBuilder<DeviceModel> builder)
    {
        builder.ToTable("DeviceModels");

        builder.Property(dm => dm.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(dm => dm.Manufacturer)
            .HasMaxLength(200);

        builder.Property(dm => dm.PortTemplate)
            .HasColumnType("jsonb");

        builder.Property(dm => dm.Specifications)
            .HasColumnType("jsonb");

        // Relationship to DeviceType
        builder.HasOne(dm => dm.DeviceType)
            .WithMany(dt => dt.DeviceModels)
            .HasForeignKey(dm => dm.DeviceTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique index on (DeviceTypeId, Name, Manufacturer)
        builder.HasIndex(dm => new { dm.DeviceTypeId, dm.Name, dm.Manufacturer })
            .IsUnique();
    }
}
