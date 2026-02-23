using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Data.Configurations;

public class RtuConfiguration : IEntityTypeConfiguration<Rtu>
{
    public void Configure(EntityTypeBuilder<Rtu> builder)
    {
        builder.ToTable("FacilityRemoteTerminalUnits");

        builder.Property(fc => fc.RtuSerialNumber)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(fc => fc.RtuModel)
            .HasMaxLength(200);

        builder.Property(fc => fc.VpnIpAddress)
            .HasMaxLength(45);

        builder.Property(fc => fc.PublicIpAddress)
            .HasMaxLength(45);

        builder.Property(fc => fc.Protocol)
            .HasMaxLength(50);

        builder.Property(fc => fc.Status)
            .HasMaxLength(50);

        builder.Property(fc => fc.Configuration)
            .HasColumnType("jsonb");

        // Relationship to Facility
        builder.HasOne(fc => fc.Facility)
            .WithMany(f => f.Rtus)
            .HasForeignKey(fc => fc.FacilityId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique index on RtuSerialNumber
        builder.HasIndex(fc => fc.RtuSerialNumber)
            .IsUnique();
    }
}
