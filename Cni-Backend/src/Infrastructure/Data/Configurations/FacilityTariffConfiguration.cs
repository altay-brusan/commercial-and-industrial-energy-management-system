using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Data.Configurations;

/// <summary>
/// Configuration for FacilityTariff junction table.
/// Tracks history of tariff assignments to facilities.
/// </summary>
public class FacilityTariffConfiguration : IEntityTypeConfiguration<FacilityTariff>
{
    public void Configure(EntityTypeBuilder<FacilityTariff> builder)
    {
        builder.ToTable("FacilityTariffs");

        builder.Property(ft => ft.FacilityId)
            .IsRequired();

        builder.Property(ft => ft.TariffId)
            .IsRequired();

        builder.Property(ft => ft.IsActive)
            .HasDefaultValue(true);

        // Contract details
        builder.Property(ft => ft.ContractNumber)
            .HasMaxLength(100);

        builder.Property(ft => ft.DiscountRate)
            .HasPrecision(5, 4)
            .HasDefaultValue(0m);

        builder.Property(ft => ft.Notes)
            .HasMaxLength(1000);

        // Relationship to Facility
        builder.HasOne(ft => ft.Facility)
            .WithMany(f => f.FacilityTariffs)
            .HasForeignKey(ft => ft.FacilityId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship to Tariff
        builder.HasOne(ft => ft.Tariff)
            .WithMany(t => t.FacilityTariffs)
            .HasForeignKey(ft => ft.TariffId)
            .OnDelete(DeleteBehavior.Restrict); // Don't delete tariffs when facility is deleted

        // Index for efficient lookups (only one active tariff per facility)
        builder.HasIndex(ft => new { ft.FacilityId, ft.IsActive })
            .HasFilter("\"IsActive\" = true");

        // Index for finding all facilities using a tariff
        builder.HasIndex(ft => ft.TariffId);
    }
}
