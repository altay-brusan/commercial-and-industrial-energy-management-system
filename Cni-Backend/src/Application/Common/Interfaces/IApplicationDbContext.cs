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
    DbSet<DeviceBatteryData> DeviceBatteryData { get; }
    DbSet<DeviceSolarData> DeviceSolarData { get; }
    DbSet<DevicePcsData> DevicePcsData { get; }
    DbSet<DeviceSmartMeterData> DeviceSmartMeterData { get; }

    // Authorization entities
    DbSet<Feature> Features { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<PlanFeature> PlanFeatures { get; }
    DbSet<TenantRole> TenantRoles { get; }
    DbSet<TenantRolePermission> TenantRolePermissions { get; }
    DbSet<UserTenantRole> UserTenantRoles { get; }

    // Device alarms, connectivity
    DbSet<DeviceAlarm> DeviceAlarms { get; }
    DbSet<DeviceAlarmType> DeviceAlarmTypes { get; }
    DbSet<Rtu> Rtus { get; }

    // Device specification templates
    DbSet<DeviceSpecificationTemplate> DeviceSpecificationTemplates { get; }

    // Maintenance tasks
    DbSet<MaintenanceTask> MaintenanceTasks { get; }
    DbSet<TaskNote> TaskNotes { get; }
    DbSet<TaskChecklistItem> TaskChecklistItems { get; }
    DbSet<TaskStatusChange> TaskStatusChanges { get; }
    DbSet<TaskWorkSession> TaskWorkSessions { get; }

    // Notifications
    DbSet<Notification> Notifications { get; }

    // Market and tariff entities
    DbSet<MarketOperator> MarketOperators { get; }
    DbSet<FirmMarketOperator> FirmMarketOperators { get; }
    DbSet<DayAheadNetEnergyPriceHistory> DayAheadNetEnergyPriceHistories { get; }
    DbSet<Tariff> Tariffs { get; }
    DbSet<FacilityTariff> FacilityTariffs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
}
