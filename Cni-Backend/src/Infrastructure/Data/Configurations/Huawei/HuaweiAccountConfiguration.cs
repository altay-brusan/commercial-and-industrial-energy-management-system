using Backend.Domain.Entities.Huawei;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Data.Configurations.Huawei;

public class HuaweiAccountConfiguration : IEntityTypeConfiguration<HuaweiAccount>
{
    public void Configure(EntityTypeBuilder<HuaweiAccount> builder)
    {
        builder.ToTable("Accounts", "huawei");

        builder.Property(a => a.UserName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(a => a.EncryptedSystemCode)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(a => a.BaseUrl)
            .HasMaxLength(500)
            .IsRequired();

        builder.HasOne(a => a.Tenant)
            .WithMany()
            .HasForeignKey(a => a.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => new { a.TenantId, a.UserName })
            .IsUnique();
    }
}
