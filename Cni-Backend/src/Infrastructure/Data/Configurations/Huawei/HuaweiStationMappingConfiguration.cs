using Backend.Domain.Entities.Huawei;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Data.Configurations.Huawei;

public class HuaweiStationMappingConfiguration : IEntityTypeConfiguration<HuaweiStationMapping>
{
    public void Configure(EntityTypeBuilder<HuaweiStationMapping> builder)
    {
        builder.ToTable("StationMappings", "huawei");

        builder.Property(s => s.StationCode)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(s => s.StationName)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasOne(s => s.HuaweiAccount)
            .WithMany(a => a.StationMappings)
            .HasForeignKey(s => s.HuaweiAccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Facility)
            .WithMany()
            .HasForeignKey(s => s.FacilityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => new { s.HuaweiAccountId, s.StationCode })
            .IsUnique();
    }
}
