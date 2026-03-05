using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Data.Configurations;

public class LogicalDeviceConfiguration : IEntityTypeConfiguration<LogicalDevice>
{
    public void Configure(EntityTypeBuilder<LogicalDevice> builder)
    {
        builder.ToTable("LogicalDevices");

        builder.Property(d => d.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(d => d.Name)
            .IsUnique();

        builder.Property(d => d.DisplayName)
            .HasMaxLength(200);

        builder.Property(d => d.Description)
            .HasMaxLength(500);

        builder.Property(d => d.Category)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(d => d.Category);

        builder.Property(d => d.Ports)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(d => d.HierarchyTemplate)
            .HasColumnType("jsonb");

        builder.Property(d => d.UiTemplate)
            .HasColumnType("jsonb");
    }
}
