using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Data.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");

        builder.Property(p => p.Identifier)
            .HasMaxLength(250);

        builder.Property(p => p.Name)
            .HasMaxLength(500);

        builder.HasOne(t => t.Plan)
            .WithMany(p => p.Tenants)
            .HasForeignKey(t => t.PlanId)
            .OnDelete(DeleteBehavior.SetNull);

        // Navigation to Firms
        builder.HasMany(t => t.Firms)
            .WithOne(f => f.Tenant)
            .HasForeignKey(f => f.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.TenantRoles)
            .WithOne(tr => tr.Tenant)
            .HasForeignKey(tr => tr.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
