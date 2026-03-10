using Backend.Domain.Entities.Huawei;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Data.Configurations.Huawei;

public class HuaweiSyncLogConfiguration : IEntityTypeConfiguration<HuaweiSyncLog>
{
    public void Configure(EntityTypeBuilder<HuaweiSyncLog> builder)
    {
        builder.ToTable("SyncLogs", "huawei");

        builder.Property(s => s.Id)
            .UseIdentityAlwaysColumn();

        builder.Property(s => s.SyncType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(s => s.Status)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(s => s.ErrorMessage)
            .HasMaxLength(2000);

        builder.HasOne(s => s.HuaweiAccount)
            .WithMany()
            .HasForeignKey(s => s.HuaweiAccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => new { s.HuaweiAccountId, s.StartedAtUtc })
            .IsDescending(false, true);
    }
}
