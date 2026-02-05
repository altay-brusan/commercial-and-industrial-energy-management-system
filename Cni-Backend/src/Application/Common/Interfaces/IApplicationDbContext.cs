using Backend.Domain.Entities;
using Backend.Domain.Entities.DeviceData;
using Microsoft.EntityFrameworkCore.Storage;

namespace Backend.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    // Core entities
    DbSet<Plan> Plans { get; }
    DbSet<Tenant> Tenants { get; }
    DbSet<Firm> Firms { get; }
    DbSet<Facility> Facilities { get; }
    DbSet<Device> Devices { get; }
    DbSet<DeviceType> DeviceTypes { get; }
    DbSet<DeviceLink> DeviceLinks { get; }
    DbSet<UserFirmFacility> UserFirmFacilities { get; }

    // Device telemetry data (time-series with JSON payload)
    DbSet<DeviceBessData> DeviceBessData { get; }
    DbSet<DeviceSolarData> DeviceSolarData { get; }
    DbSet<DevicePcsData> DevicePcsData { get; }
    DbSet<DeviceSmartMeterData> DeviceSmartMeterData { get; }

    // Market and tariff entities
    DbSet<MarketOperator> MarketOperators { get; }
    DbSet<FirmMarketOperator> FirmMarketOperators { get; }
    DbSet<EnergyPriceHistory> EnergyPriceHistories { get; }
    DbSet<Tariff> Tariffs { get; }
    DbSet<FacilityTariff> FacilityTariffs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
}
