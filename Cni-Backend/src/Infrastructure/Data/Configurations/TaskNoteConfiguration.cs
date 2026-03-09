using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Data.Configurations;

public class TaskNoteConfiguration : IEntityTypeConfiguration<TaskNote>
{
    public void Configure(EntityTypeBuilder<TaskNote> builder)
    {
        builder.ToTable("TaskNotes");

        builder.Property(n => n.Content)
            .HasMaxLength(4000)
            .IsRequired();

        builder.HasOne(n => n.MaintenanceTask)
            .WithMany(t => t.Notes)
            .HasForeignKey(n => n.MaintenanceTaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(n => n.Author)
            .WithMany()
            .HasForeignKey(n => n.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(n => new { n.MaintenanceTaskId, n.Created })
            .IsDescending(false, true);
    }
}
