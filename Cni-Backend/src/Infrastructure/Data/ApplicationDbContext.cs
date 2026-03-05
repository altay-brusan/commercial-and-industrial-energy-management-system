using System.Reflection;
using Backend.Application.Common.Interfaces;
using Backend.Domain.Entities;
using Backend.Domain.Entities.DeviceData;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Backend.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>, IApplicationDbContext
{
    private readonly Guid? _effectiveTenantId;
    private readonly bool _isTenantScoped;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IMultiTenantService multiTenantService) : base(options)
    {
        _effectiveTenantId = multiTenantService.GetEffectiveTenantId();
        _isTenantScoped = multiTenantService.IsTenantScoped();
    }

    // Core entities
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Firm> Firms => Set<Firm>();
    public DbSet<Facility> Facilities => Set<Facility>();
    public DbSet<UserFirmFacility> UserFirmFacilities => Set<UserFirmFacility>();

    public DbSet<Device> Devices => Set<Device>();
    public DbSet<DeviceType> DeviceTypes => Set<DeviceType>();
    public DbSet<DeviceLink> DeviceLinks => Set<DeviceLink>();
    public DbSet<Rtu> Rtus => Set<Rtu>();
    public DbSet<DeviceAlarm> DeviceAlarms => Set<DeviceAlarm>();
    public DbSet<DeviceAlarmType> DeviceAlarmTypes => Set<DeviceAlarmType>();
    public DbSet<DeviceSpecificationTemplate> DeviceSpecificationTemplates => Set<DeviceSpecificationTemplate>();

    // Logical device catalog (global — no tenant filter)
    public DbSet<LogicalNode> LogicalNodes => Set<LogicalNode>();
    public DbSet<LogicalDevice> LogicalDevices => Set<LogicalDevice>();
    public DbSet<LogicalDeviceNode> LogicalDeviceNodes => Set<LogicalDeviceNode>();

    // Device telemetry data (time-series with JSON payload)
    public DbSet<DeviceBatteryData> DeviceBatteryData => Set<DeviceBatteryData>();
    public DbSet<DeviceSolarData> DeviceSolarData => Set<DeviceSolarData>();
    public DbSet<DevicePcsData> DevicePcsData => Set<DevicePcsData>();
    public DbSet<DeviceSmartMeterData> DeviceSmartMeterData => Set<DeviceSmartMeterData>();

    // Authorization entities
    public DbSet<Feature> Features => Set<Feature>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<PlanFeature> PlanFeatures => Set<PlanFeature>();
    public DbSet<TenantRole> TenantRoles => Set<TenantRole>();
    public DbSet<TenantRolePermission> TenantRolePermissions => Set<TenantRolePermission>();
    public DbSet<UserTenantRole> UserTenantRoles => Set<UserTenantRole>();

    // Auth entities
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // Market and tariff entities
    public DbSet<MarketOperator> MarketOperators => Set<MarketOperator>();
    public DbSet<FirmMarketOperator> FirmMarketOperators => Set<FirmMarketOperator>();
    public DbSet<DayAheadNetEnergyPriceHistory> DayAheadNetEnergyPriceHistories => Set<DayAheadNetEnergyPriceHistory>();
    public DbSet<Tariff> Tariffs => Set<Tariff>();
    public DbSet<FacilityTariff> FacilityTariffs => Set<FacilityTariff>();


    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Query filters use effective tenant ID when scoped
        // !_isTenantScoped = admin without selection -> shows ALL data
        // _isTenantScoped = regular user OR admin with selection -> scoped to effective tenant
        builder.Entity<Tenant>().HasQueryFilter(x => !_isTenantScoped || x.Id == _effectiveTenantId);
        builder.Entity<Firm>().HasQueryFilter(x => !_isTenantScoped || x.TenantId == _effectiveTenantId);
        // Entities that access tenant through Firm relationship need matching filters
        builder.Entity<Facility>().HasQueryFilter(x => !_isTenantScoped || x.Firm!.TenantId == _effectiveTenantId);
        builder.Entity<FirmMarketOperator>().HasQueryFilter(x => !_isTenantScoped || x.Firm!.TenantId == _effectiveTenantId);
        // Entities that access tenant through Facility → Firm need matching filters
        builder.Entity<Device>().HasQueryFilter(x => !_isTenantScoped || x.Rtu!.Facility!.Firm!.TenantId == _effectiveTenantId);
        builder.Entity<DeviceLink>().HasQueryFilter(x => !_isTenantScoped || x.Facility!.Firm!.TenantId == _effectiveTenantId);
        builder.Entity<Rtu>().HasQueryFilter(x => !_isTenantScoped || x.Facility!.Firm!.TenantId == _effectiveTenantId);
        builder.Entity<DeviceAlarm>().HasQueryFilter(x => !_isTenantScoped || x.Device!.Rtu!.Facility!.Firm!.TenantId == _effectiveTenantId);
        builder.Entity<FacilityTariff>().HasQueryFilter(x => !_isTenantScoped || x.Facility!.Firm!.TenantId == _effectiveTenantId);
        // Device telemetry data - filter through Device → Rtu → Facility → Firm → Tenant
        builder.Entity<DeviceBatteryData>().HasQueryFilter(x => !_isTenantScoped || x.Device!.Rtu!.Facility!.Firm!.TenantId == _effectiveTenantId);
        builder.Entity<DeviceSolarData>().HasQueryFilter(x => !_isTenantScoped || x.Device!.Rtu!.Facility!.Firm!.TenantId == _effectiveTenantId);
        builder.Entity<DevicePcsData>().HasQueryFilter(x => !_isTenantScoped || x.Device!.Rtu!.Facility!.Firm!.TenantId == _effectiveTenantId);
        builder.Entity<DeviceSmartMeterData>().HasQueryFilter(x => !_isTenantScoped || x.Device!.Rtu!.Facility!.Firm!.TenantId == _effectiveTenantId);
        // Authorization entities with tenant scoping
        builder.Entity<TenantRole>().HasQueryFilter(x => !_isTenantScoped || x.TenantId == _effectiveTenantId);
        builder.Entity<TenantRolePermission>().HasQueryFilter(x => !_isTenantScoped || x.TenantRole!.TenantId == _effectiveTenantId);
        builder.Entity<UserTenantRole>().HasQueryFilter(x => !_isTenantScoped || x.TenantRole!.TenantId == _effectiveTenantId);
        // Note: Tariffs, MarketOperators, DayAheadNetEnergyPriceHistories, DeviceAlarmTypes are global - no tenant query filter
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // RefreshToken configuration
        builder.Entity<RefreshToken>(entity =>
        {
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.Property(e => e.Token).HasMaxLength(256);
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        builder.Entity<ApplicationUser>().ToTable("AppUsers");
        builder.Entity<IdentityRole>().ToTable("AppRoles");

        builder.Entity<IdentityUserClaim<string>>().ToTable("AppUserClaims");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("AppRoleClaims");

        builder.Entity<IdentityUserLogin<string>>().ToTable("AppUserLogins");
        builder.Entity<IdentityUserToken<string>>().ToTable("AppUserTokens");
        builder.Entity<IdentityUserRole<string>>().ToTable("AppUserRoles");

        // SQLite doesn't support DateTimeOffset in ORDER BY / WHERE — convert to sortable strings
        if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
        {
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTimeOffset))
                    {
                        property.SetValueConverter(
                            new ValueConverter<DateTimeOffset, string>(
                                v => v.ToUniversalTime().ToString("o"),
                                v => DateTimeOffset.Parse(v)));
                    }
                    else if (property.ClrType == typeof(DateTimeOffset?))
                    {
                        property.SetValueConverter(
                            new ValueConverter<DateTimeOffset?, string?>(
                                v => v.HasValue ? v.Value.ToUniversalTime().ToString("o") : null,
                                v => v != null ? DateTimeOffset.Parse(v) : null));
                    }
                }
            }
        }
    }
    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        return await Database.BeginTransactionAsync(cancellationToken);
    }
}
