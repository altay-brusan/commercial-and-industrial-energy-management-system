using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Data.Configurations;

public class UserFirmFacilityConfiguration : IEntityTypeConfiguration<UserFirmFacility>
{
    public void Configure(EntityTypeBuilder<UserFirmFacility> builder)
    {
        builder.ToTable("UserFirmFacilities");
        // Composite Key
        builder.HasIndex(uf => new { uf.UserId, uf.FirmId, uf.FacilityId }).IsUnique();

        // User ile ilişki (Kullanıcı silindiğinde alt kayıtları da sil)
        builder.HasOne(uf => uf.User)
            .WithMany(u => u.UserFirmFacilities)
            .HasForeignKey(uf => uf.UserId)
            .OnDelete(DeleteBehavior.Cascade);


    }
}
