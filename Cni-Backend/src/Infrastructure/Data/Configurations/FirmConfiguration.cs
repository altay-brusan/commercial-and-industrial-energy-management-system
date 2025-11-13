using Backend.Domain.Common;
using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Data.Configurations;

public class FirmConfiguration : IEntityTypeConfiguration<Firm>
{
    public void Configure(EntityTypeBuilder<Firm> builder)
    {
        builder.ToTable("Firms");

        builder.Property(p => p.Name)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(p => p.LogoUrl)
            .HasMaxLength(1000);

        builder.Property(p => p.Type)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.Country)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.State)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.City)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.District)
            .HasMaxLength(100);

        builder.Property(p => p.PostalCode)
            .HasMaxLength(20);

        builder.Property(p => p.Address)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(p => p.ContactEmail)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(p => p.ContactPhone)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Fax)
            .HasMaxLength(50);

        builder.Property(p => p.TaxNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.TaxOffice)
            .HasMaxLength(100)
            .IsRequired();

        // TenantId alanını zorunlu (required) olarak işaretleyin
        builder.Property(p => p.TenantId)
               .IsRequired();

        // Tenant ile ilişkiyi tanımlayın
        builder.HasOne(f => f.Tenant)
               .WithMany(t => t.Firms) // Tenant'ın birden çok Firm'i olabilir
               .HasForeignKey(f => f.TenantId) // ForeignKey olarak TenantId'yi kullan
               .OnDelete(DeleteBehavior.Restrict); // Silme davranışını belirle

    }
}
