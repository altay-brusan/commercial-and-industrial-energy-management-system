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
