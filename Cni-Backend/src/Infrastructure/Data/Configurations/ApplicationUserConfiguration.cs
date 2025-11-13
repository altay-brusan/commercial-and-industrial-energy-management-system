using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Data.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("AppUsers");
        builder.Property(p => p.Name)
            .HasMaxLength(70).IsRequired();
        builder.Property(p => p.Surname)
            .HasMaxLength(70).IsRequired();
        builder.Property(p => p.Title)
            .HasMaxLength(100).IsRequired();
        builder.Property(p => p.Description)
            .HasMaxLength(2000).IsRequired();

    }
}
