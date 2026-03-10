using Backend.Domain.Entities.Huawei;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Data.Configurations.Huawei;

public class HuaweiRawDataLogConfiguration : IEntityTypeConfiguration<HuaweiRawDataLog>
{
    public void Configure(EntityTypeBuilder<HuaweiRawDataLog> builder)
    {
        builder.ToTable("RawDataLogs", "huawei");

        builder.Property(r => r.Id)
            .UseIdentityAlwaysColumn();

        builder.Property(r => r.Endpoint)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(r => r.RequestPayload)
            .HasColumnType("jsonb");

        builder.Property(r => r.ResponsePayload)
            .HasColumnType("jsonb");

        builder.HasOne(r => r.HuaweiAccount)
            .WithMany()
            .HasForeignKey(r => r.HuaweiAccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.CollectedAtUtc)
            .IsDescending();
    }
}
