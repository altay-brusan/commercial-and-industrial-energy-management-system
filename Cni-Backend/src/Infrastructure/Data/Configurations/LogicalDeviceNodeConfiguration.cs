using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Data.Configurations;

public class LogicalDeviceNodeConfiguration : IEntityTypeConfiguration<LogicalDeviceNode>
{
    public void Configure(EntityTypeBuilder<LogicalDeviceNode> builder)
    {
        builder.ToTable("LogicalDeviceNodes");

        builder.HasKey(ldn => new { ldn.LogicalDeviceId, ldn.LogicalNodeId });

        builder.Property(ldn => ldn.NodeAlias)
            .HasMaxLength(100);

        builder.HasOne(ldn => ldn.LogicalDevice)
            .WithMany(ld => ld.LogicalDeviceNodes)
            .HasForeignKey(ldn => ldn.LogicalDeviceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ldn => ldn.LogicalNode)
            .WithMany(ln => ln.LogicalDeviceNodes)
            .HasForeignKey(ldn => ldn.LogicalNodeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
