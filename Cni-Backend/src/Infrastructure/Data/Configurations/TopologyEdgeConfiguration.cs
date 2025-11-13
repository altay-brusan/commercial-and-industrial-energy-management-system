using Backend.Domain.Entities.Topology;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Data.Configurations;

public class TopologyEdgeConfiguration : IEntityTypeConfiguration<TopologyEdge>
{
    public void Configure(EntityTypeBuilder<TopologyEdge> builder)
    {
        builder.ToTable("TopologyEdges"); // Yeni tablo ismi

        // Temel özellikler
        builder.Property(e => e.TenantId)
            .IsRequired();

        builder.Property(n => n.Data).HasMaxLength(1000);

        builder.Property(e => e.TargetHandle)
            .HasMaxLength(50);

        builder.Property(e => e.SourceHandle)
            .HasMaxLength(50);

        // SourceNode ile ilişki
        builder.HasOne(e => e.SourceNode)
            .WithMany(n => n.SourceEdges)
            .HasForeignKey(e => e.SourceId)
            .OnDelete(DeleteBehavior.Restrict);

        // TargetNode ile ilişki
        builder.HasOne(e => e.TargetNode)
            .WithMany(n => n.TargetEdges)
            .HasForeignKey(e => e.TargetId)
            .OnDelete(DeleteBehavior.Restrict);

        // Tenant ile ilişki
        builder.HasOne(e => e.Tenant)
            .WithMany()
            .HasForeignKey(e => e.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Facility ile ilişki
        builder.HasOne(n => n.Facility)
            .WithMany()
            .HasForeignKey(n => n.FacilityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
