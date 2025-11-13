using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Data.Configurations;
public class DeviceTypeConfiguration : IEntityTypeConfiguration<DeviceType>
{
    public void Configure(EntityTypeBuilder<DeviceType> builder)
    {
        builder.ToTable("DeviceTypes");

        // Property Konfigürasyonları
        builder.Property(dt => dt.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(dt => dt.HasGrpcStream)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(dt => dt.IsDataCollectedViaOtherDevice)
            .IsRequired()
            .HasDefaultValue(false);

        //Seed Data(Enum karşılıkları)
        //builder.HasData(
        //    new DeviceType
        //    {
        //        Id = 1,
        //        Name = "Bess",
        //        HasGrpcStream = true,
        //        IsDataCollectedViaOtherDevice = false,
        //        Created = DateTime.UtcNow // BaseAuditableEntity için
        //    },
        //    new DeviceType
        //    {
        //        Id = 2,
        //        Name = "SolarPanel",
        //        HasGrpcStream = true,
        //        IsDataCollectedViaOtherDevice = false,
        //        Created = DateTime.UtcNow
        //    },
        //    new DeviceType
        //    {
        //        Id = 3,
        //        Name = "SmartMeter",
        //        HasGrpcStream = true,
        //        IsDataCollectedViaOtherDevice = false,
        //        Created = DateTime.UtcNow
        //    },
        //    new DeviceType
        //    {
        //        Id = 4,
        //        Name = "PPC",
        //        HasGrpcStream = true,
        //        IsDataCollectedViaOtherDevice = false,
        //        Created = DateTime.UtcNow
        //    },
        //     Diğer tipleri aynı formatta ekleyin...
        //    new DeviceType
        //    {
        //        Id = 5,
        //        Name = "EV Charger",
        //        HasGrpcStream = false,
        //        IsDataCollectedViaOtherDevice = true,
        //        Created = DateTime.UtcNow
        //    },
        //    new DeviceType
        //    {
        //        Id = 6,
        //        Name = "Heat Pump",
        //        HasGrpcStream = false,
        //        IsDataCollectedViaOtherDevice = true,
        //        Created = DateTime.UtcNow
        //    },
        //    new DeviceType
        //    {
        //        Id = 7,
        //        Name = "Generator",
        //        HasGrpcStream = false,
        //        IsDataCollectedViaOtherDevice = true,
        //        Created = DateTime.UtcNow
        //    }, new DeviceType
        //    {
        //        Id = 8,
        //        Name = "Grid",
        //        HasGrpcStream = false,
        //        IsDataCollectedViaOtherDevice = true,
        //        Created = DateTime.UtcNow
        //    }
        //);
    }
}
