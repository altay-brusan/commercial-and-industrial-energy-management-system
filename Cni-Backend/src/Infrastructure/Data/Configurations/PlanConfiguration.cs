using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Data.Configurations;

public class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.ToTable("Plans");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Modules).HasMaxLength(500);
        builder.Property(p => p.Price).HasColumnType("decimal(18,2)");

    }
}
