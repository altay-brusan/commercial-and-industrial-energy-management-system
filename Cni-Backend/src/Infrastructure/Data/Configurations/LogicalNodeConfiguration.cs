using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Data.Configurations;

public class LogicalNodeConfiguration : IEntityTypeConfiguration<LogicalNode>
{
    public void Configure(EntityTypeBuilder<LogicalNode> builder)
    {
        builder.ToTable("LogicalNodes");

        builder.Property(n => n.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(n => n.Name)
            .IsUnique();

        builder.Property(n => n.DisplayName)
            .HasMaxLength(200);

        builder.Property(n => n.Description)
            .HasMaxLength(500);

        builder.Property(n => n.Parameters)
            .HasColumnType("jsonb")
            .IsRequired();
    }
}
