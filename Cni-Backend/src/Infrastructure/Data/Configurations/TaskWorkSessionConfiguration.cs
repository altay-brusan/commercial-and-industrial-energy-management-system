using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Data.Configurations;

public class TaskWorkSessionConfiguration : IEntityTypeConfiguration<TaskWorkSession>
{
    public void Configure(EntityTypeBuilder<TaskWorkSession> builder)
    {
        builder.ToTable("TaskWorkSessions");

        builder.HasOne(w => w.MaintenanceTask)
            .WithMany(t => t.WorkSessions)
            .HasForeignKey(w => w.MaintenanceTaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(w => w.User)
            .WithMany()
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(w => new { w.MaintenanceTaskId, w.StartedAt })
            .IsDescending(false, true);
    }
}
