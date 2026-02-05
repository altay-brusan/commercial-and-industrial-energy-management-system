using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Data.Configurations;

public class DeviceTypeConfiguration : IEntityTypeConfiguration<DeviceType>
{
    public void Configure(EntityTypeBuilder<DeviceType> builder)
    {
        builder.ToTable("DeviceTypes");

        builder.Property(dt => dt.Name)
            .IsRequired()
            .HasMaxLength(50);

        // Unique constraint on Name
        builder.HasIndex(dt => dt.Name)
            .IsUnique();
    }
}
