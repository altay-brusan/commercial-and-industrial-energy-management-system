using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Data.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");
        builder.Property(p => p.Identifier)
            .HasMaxLength(250);
        builder.Property(p => p.Name)
            .HasMaxLength(500);
        builder.Property(p => p.ConnectionString)
            .HasMaxLength(1000);

    }
}
