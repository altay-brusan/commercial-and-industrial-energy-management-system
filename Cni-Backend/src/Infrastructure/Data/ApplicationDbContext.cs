using System.Reflection;
using Backend.Application.Common.Interfaces;
using Backend.Domain.Entities;
using Backend.Domain.Entities.Proto;
using Backend.Domain.Entities.Topology;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Backend.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>, IApplicationDbContext
{
    private readonly int? _tenantId;
    private readonly bool _isSuperAdmin;
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IMultiTenantService multiTenantService) : base(options)
    {

        _tenantId = multiTenantService.GetCurrentTenantId();
        _isSuperAdmin = multiTenantService.IsSuperAdmin();
    }
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Firm> Firms => Set<Firm>();
    public DbSet<Facility> Facilities => Set<Facility>();
    public DbSet<UserFirmFacility> UserFirmFacilities => Set<UserFirmFacility>();
    public DbSet<TopologyNode> TopologyNodes => Set<TopologyNode>();
    public DbSet<TopologyEdge> TopologyEdges => Set<TopologyEdge>();

    public DbSet<Device> Devices => Set<Device>();
    public DbSet<DeviceType> DeviceTypes => Set<DeviceType>();
    public DbSet<AppSolarData> SolarDatas => Set<AppSolarData>();
    public DbSet<AppBESSData> BESSDatas => Set<AppBESSData>();
    public DbSet<AppGrpcSystemData> GrpcFacilityDatas => Set<AppGrpcSystemData>();
    public DbSet<AppPCSData> PCSDatas => Set<AppPCSData>();
    public DbSet<AppSmartMeterData> SmartMeterDatas => Set<AppSmartMeterData>();

    public DbSet<MarketOperator> MarketOperators => Set<MarketOperator>();
    public DbSet<TenantMarketOperator> TenantMarketOperators => Set<TenantMarketOperator>();
    public DbSet<EnergyPriceHistory> EnergyPriceHistories => Set<EnergyPriceHistory>();
    public DbSet<CountryTariff> CountryTariffs => Set<CountryTariff>();

    public DbSet<TodoList> TodoLists => Set<TodoList>();

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();


    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Tenant>().HasQueryFilter(x => _isSuperAdmin || x.Id == _tenantId);
        builder.Entity<Firm>().HasQueryFilter(x => _isSuperAdmin || x.TenantId == _tenantId);
        builder.Entity<Facility>().HasQueryFilter(x => _isSuperAdmin || x.TenantId == _tenantId);
        builder.Entity<TodoList>().HasQueryFilter(x => _isSuperAdmin || x.TenantId == _tenantId);
        builder.Entity<TenantMarketOperator>().HasQueryFilter(x => _isSuperAdmin || x.TenantId == _tenantId);
        builder.Entity<CountryTariff>().HasQueryFilter(x => _isSuperAdmin || x.TenantId == _tenantId);
        // Note: MarketOperators and EnergyPriceHistories are global - no tenant query filter
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
