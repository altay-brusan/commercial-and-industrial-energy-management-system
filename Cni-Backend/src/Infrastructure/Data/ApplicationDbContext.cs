using System.Reflection;
using Backend.Application.Common.Interfaces;
using Backend.Domain.Entities;
using Backend.Domain.Entities.DeviceData;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Backend.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>, IApplicationDbContext
{
    private readonly int? _effectiveTenantId;
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

    // Device telemetry data (time-series with JSON payload)
    public DbSet<DeviceBessData> DeviceBessData => Set<DeviceBessData>();
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
        builder.Entity<Device>().HasQueryFilter(x => !_isTenantScoped || x.Facility!.Firm!.TenantId == _effectiveTenantId);
        builder.Entity<DeviceLink>().HasQueryFilter(x => !_isTenantScoped || x.Facility!.Firm!.TenantId == _effectiveTenantId);
        builder.Entity<FacilityTariff>().HasQueryFilter(x => !_isTenantScoped || x.Facility!.Firm!.TenantId == _effectiveTenantId);
        // Device telemetry data - filter by denormalized FirmId
        builder.Entity<DeviceBessData>().HasQueryFilter(x => !_isTenantScoped || x.Firm!.TenantId == _effectiveTenantId);
        builder.Entity<DeviceSolarData>().HasQueryFilter(x => !_isTenantScoped || x.Firm!.TenantId == _effectiveTenantId);
        builder.Entity<DevicePcsData>().HasQueryFilter(x => !_isTenantScoped || x.Firm!.TenantId == _effectiveTenantId);
        builder.Entity<DeviceSmartMeterData>().HasQueryFilter(x => !_isTenantScoped || x.Firm!.TenantId == _effectiveTenantId);
        // Authorization entities with tenant scoping
        builder.Entity<TenantRole>().HasQueryFilter(x => !_isTenantScoped || x.TenantId == _effectiveTenantId);
        builder.Entity<TenantRolePermission>().HasQueryFilter(x => !_isTenantScoped || x.TenantRole!.TenantId == _effectiveTenantId);
        builder.Entity<UserTenantRole>().HasQueryFilter(x => !_isTenantScoped || x.TenantRole!.TenantId == _effectiveTenantId);
        // Note: Tariffs, MarketOperators, DayAheadNetEnergyPriceHistories are global - no tenant query filter
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        builder.Entity<ApplicationUser>().ToTable("AppUsers");
        builder.Entity<IdentityRole>().ToTable("AppRoles");

        builder.Entity<IdentityUserClaim<string>>().ToTable("AppUserClaims");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("AppRoleClaims");

        builder.Entity<IdentityUserLogin<string>>().ToTable("AppUserLogins");
        builder.Entity<IdentityUserToken<string>>().ToTable("AppUserTokens");
        builder.Entity<IdentityUserRole<string>>().ToTable("AppUserRoles");

    }
    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        return await Database.BeginTransactionAsync(cancellationToken);
    }
}
