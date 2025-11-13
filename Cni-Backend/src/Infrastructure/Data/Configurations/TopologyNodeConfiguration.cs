using Backend.Domain.Entities.Topology;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Data.Configurations;

public class TopologyNodeConfiguration : IEntityTypeConfiguration<TopologyNode>
{
    public void Configure(EntityTypeBuilder<TopologyNode> builder)
    {
        builder.ToTable("TopologyNodes");

        // Temel özellikler
        builder.Property(n => n.TenantId)
            .IsRequired();

        builder.Property(n => n.Extent)
            .HasMaxLength(100);

        builder.Property(n => n.Data).HasMaxLength(1000);

        builder.Property(n => n.Position).HasMaxLength(1000);

        builder.Property(n => n.Style).HasMaxLength(1000);

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
        builder.HasOne(n => n.Device)
       .WithOne(d => d.TopologyNode) // Device'daki navigation property
       .HasForeignKey<TopologyNode>(n => n.DeviceId)
       .OnDelete(DeleteBehavior.Restrict);

        // Edge ile ilişkiler (Source ve Target)
        builder.HasMany(n => n.SourceEdges)
            .WithOne(e => e.SourceNode)
            .HasForeignKey(e => e.SourceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(n => n.TargetEdges)
            .WithOne(e => e.TargetNode)
            .HasForeignKey(e => e.TargetId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
