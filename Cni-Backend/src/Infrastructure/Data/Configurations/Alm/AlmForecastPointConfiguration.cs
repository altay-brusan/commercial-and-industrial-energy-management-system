using Backend.Domain.Entities.Alm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Data.Configurations.Alm;

public class AlmForecastPointConfiguration : IEntityTypeConfiguration<AlmForecastPoint>
{
    public void Configure(EntityTypeBuilder<AlmForecastPoint> builder)
    {
        builder.ToTable("ForecastPoints", "alm");

        builder.Property(p => p.Id)
            .UseIdentityAlwaysColumn();

        builder.Property(p => p.Value)
            .IsRequired();

        builder.HasOne(p => p.AlmPredictionJob)
            .WithMany(j => j.ForecastPoints)
            .HasForeignKey(p => p.AlmPredictionJobId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => new { p.AlmPredictionJobId, p.Timestamp });
    }
}
