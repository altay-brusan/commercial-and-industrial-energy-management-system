using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Data.Configurations;

public class DeviceLinkConfiguration : IEntityTypeConfiguration<DeviceLink>
{
    public void Configure(EntityTypeBuilder<DeviceLink> builder)
    {
        builder.ToTable("DeviceLinks");

        builder.Property(dl => dl.FacilityId)
            .IsRequired();

        builder.Property(dl => dl.LinkId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(dl => dl.LinkType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(dl => dl.Configuration)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(dl => dl.ConnectedPorts)
            .HasColumnType("text")
            .IsRequired();

        // Relationship to Facility
        builder.HasOne(dl => dl.Facility)
            .WithMany(f => f.DeviceLinks)
            .HasForeignKey(dl => dl.FacilityId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique constraint on (FacilityId, LinkId)
        builder.HasIndex(dl => new { dl.FacilityId, dl.LinkId })
            .IsUnique();
    }
}
