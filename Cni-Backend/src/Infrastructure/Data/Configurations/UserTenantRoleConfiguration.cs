using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Data.Configurations;

public class UserTenantRoleConfiguration : IEntityTypeConfiguration<UserTenantRole>
{
    public void Configure(EntityTypeBuilder<UserTenantRole> builder)
    {
        builder.ToTable("UserTenantRoles");

        builder.HasIndex(utr => new { utr.UserId, utr.TenantRoleId })
            .IsUnique();

        builder.Property(utr => utr.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.HasOne(utr => utr.User)
            .WithMany(u => u.UserTenantRoles)
            .HasForeignKey(utr => utr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(utr => utr.TenantRole)
            .WithMany(tr => tr.UserTenantRoles)
            .HasForeignKey(utr => utr.TenantRoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
