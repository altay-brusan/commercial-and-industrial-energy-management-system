using Backend.Domain.Entities.Alm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Data.Configurations.Alm;

public class AlmPredictionJobConfiguration : IEntityTypeConfiguration<AlmPredictionJob>
{
    public void Configure(EntityTypeBuilder<AlmPredictionJob> builder)
    {
        builder.ToTable("PredictionJobs", "alm");

        builder.Property(j => j.ExternalJobId)
            .HasMaxLength(200);

        builder.Property(j => j.ResultMessage)
            .HasMaxLength(1000);

        builder.Property(j => j.ErrorDetails)
            .HasMaxLength(4000);

        builder.HasOne(j => j.Facility)
            .WithMany()
            .HasForeignKey(j => j.FacilityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(j => new { j.FacilityId, j.JobType, j.Status });
        builder.HasIndex(j => j.ExternalJobId);
    }
}
