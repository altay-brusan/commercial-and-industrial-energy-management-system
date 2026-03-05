using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Data.Configurations;

public class TaskChecklistItemConfiguration : IEntityTypeConfiguration<TaskChecklistItem>
{
    public void Configure(EntityTypeBuilder<TaskChecklistItem> builder)
    {
        builder.ToTable("TaskChecklistItems");

        builder.Property(c => c.Label)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(c => c.IsCompleted)
            .HasDefaultValue(false);

        builder.HasOne(c => c.MaintenanceTask)
            .WithMany(t => t.ChecklistItems)
            .HasForeignKey(c => c.MaintenanceTaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.CompletedBy)
            .WithMany()
            .HasForeignKey(c => c.CompletedById)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(c => new { c.MaintenanceTaskId, c.SortOrder });
    }
}
