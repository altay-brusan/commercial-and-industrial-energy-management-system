using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Data.Configurations;

public class TaskStatusChangeConfiguration : IEntityTypeConfiguration<TaskStatusChange>
{
    public void Configure(EntityTypeBuilder<TaskStatusChange> builder)
    {
        builder.ToTable("TaskStatusChanges");

        builder.Property(s => s.FromStatus)
            .HasMaxLength(50);

        builder.Property(s => s.ToStatus)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasOne(s => s.MaintenanceTask)
            .WithMany(t => t.StatusChanges)
            .HasForeignKey(s => s.MaintenanceTaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.ChangedBy)
            .WithMany()
            .HasForeignKey(s => s.ChangedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => new { s.MaintenanceTaskId, s.ChangedAt })
            .IsDescending(false, true);
    }
}
