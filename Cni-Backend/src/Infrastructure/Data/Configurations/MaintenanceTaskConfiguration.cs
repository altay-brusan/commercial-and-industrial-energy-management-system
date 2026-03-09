using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Data.Configurations;

public class MaintenanceTaskConfiguration : IEntityTypeConfiguration<MaintenanceTask>
{
    public void Configure(EntityTypeBuilder<MaintenanceTask> builder)
    {
        builder.ToTable("MaintenanceTasks");

        builder.Property(t => t.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasMaxLength(2000);

        builder.Property(t => t.Status)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(t => t.Priority)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(t => t.Resolution)
            .HasMaxLength(2000);

        builder.Property(t => t.RootCause)
            .HasMaxLength(2000);

        builder.Property(t => t.PartsUsed)
            .HasMaxLength(1000);

        builder.Property(t => t.Recommendations)
            .HasMaxLength(2000);

        builder.Property(t => t.CloseReason)
            .HasMaxLength(50);

        builder.Property(t => t.CloseExplanation)
            .HasMaxLength(2000);

        builder.Property(t => t.LaborHours)
            .HasPrecision(8, 2);

        // Relationships
        builder.HasOne(t => t.AssignedTo)
            .WithMany()
            .HasForeignKey(t => t.AssignedToId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.AssignedBy)
            .WithMany()
            .HasForeignKey(t => t.AssignedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.CompletedBy)
            .WithMany()
            .HasForeignKey(t => t.CompletedById)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.ClosedBy)
            .WithMany()
            .HasForeignKey(t => t.ClosedById)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.Facility)
            .WithMany()
            .HasForeignKey(t => t.FacilityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Device)
            .WithMany()
            .HasForeignKey(t => t.DeviceId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.DeviceAlarm)
            .WithOne(a => a.MaintenanceTask)
            .HasForeignKey<MaintenanceTask>(t => t.DeviceAlarmId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => new { t.AssignedToId, t.Status });
        builder.HasIndex(t => t.DeviceId);
        builder.HasIndex(t => t.DueDate);
        builder.HasIndex(t => t.FacilityId);
        builder.HasIndex(t => t.DeviceAlarmId)
            .IsUnique()
            .HasFilter("\"DeviceAlarmId\" IS NOT NULL");
    }
}
