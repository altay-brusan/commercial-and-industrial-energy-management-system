using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Data.Configurations;

public class TenantRoleConfiguration : IEntityTypeConfiguration<TenantRole>
{
    public void Configure(EntityTypeBuilder<TenantRole> builder)
    {
        builder.ToTable("TenantRoles");

        builder.Property(tr => tr.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(tr => tr.Description)
            .HasMaxLength(500);

        builder.HasIndex(tr => new { tr.TenantId, tr.Name })
            .IsUnique();

        builder.HasOne(tr => tr.Tenant)
            .WithMany(t => t.TenantRoles)
            .HasForeignKey(tr => tr.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
