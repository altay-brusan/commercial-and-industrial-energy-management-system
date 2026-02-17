using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Data.Configurations;

public class TenantRolePermissionConfiguration : IEntityTypeConfiguration<TenantRolePermission>
{
    public void Configure(EntityTypeBuilder<TenantRolePermission> builder)
    {
        builder.ToTable("TenantRolePermissions");

        builder.HasIndex(trp => new { trp.TenantRoleId, trp.PermissionId })
            .IsUnique();

        builder.HasOne(trp => trp.TenantRole)
            .WithMany(tr => tr.TenantRolePermissions)
            .HasForeignKey(trp => trp.TenantRoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(trp => trp.Permission)
            .WithMany(p => p.TenantRolePermissions)
            .HasForeignKey(trp => trp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
