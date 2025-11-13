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

        builder.Property(p => p.Capacity)
            .IsRequired();

        builder.Property(p => p.TenantId)
            .IsRequired();

        builder.Property(p => p.FirmId)
            .IsRequired();

        builder.HasOne(p => p.Tenant)
            .WithMany()
            .HasForeignKey(p => p.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.Firm)
               .WithMany(t => t.Facilities) 
               .HasForeignKey(f => f.FirmId) 
               .OnDelete(DeleteBehavior.Cascade);
    }
}
