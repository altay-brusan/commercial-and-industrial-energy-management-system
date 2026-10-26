using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Backend.Domain.Constants;
using Backend.Domain.Entities;
using Backend.Domain.Entities.Huawei;
using Backend.Domain.Enums;
using Backend.Infrastructure.Huawei;
using FluentValidation.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backend.Infrastructure.Data;

public static class InitializerExtensions
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initializer = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitializer>();

        await initializer.InitializeAsync();

        await initializer.SeedAsync();
    }
}

public class ApplicationDbContextInitializer
{
    private readonly ILogger<ApplicationDbContextInitializer> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public ApplicationDbContextInitializer(ILogger<ApplicationDbContextInitializer> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task InitializeAsync()
    {
        try
        {
            var isSqlite = _context.Database.ProviderName?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) ?? false;
            if (isSqlite)
            {
                await _context.Database.EnsureCreatedAsync();
            }
            else
            {
                await _context.Database.MigrateAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        // NOTE: Use IgnoreQueryFilters() to bypass multi-tenant query filters during seeding.
        // Without this, the Any() checks may return false even when records exist,
        // because the query filters require a tenant context that isn't set during startup.

        Guid tenantId = Guid.Empty;
        Guid firmId = Guid.Empty;
        Guid facilitiyId = Guid.Empty;

        // Create Tenant first (required for multi-tenancy)
        // Use IgnoreQueryFilters to check ALL tenants regardless of current tenant context
        var existingTenant = await _context.Tenants.IgnoreQueryFilters().OrderBy(x => x.Created).FirstOrDefaultAsync();
        if (existingTenant == null)
        {
            var tenant = new Tenant
            {
                Identifier = "demo",
                Name = "Ottomotive BESS",
                Created = DateTime.UtcNow
            };
            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();
            tenantId = tenant.Id;
            _logger.LogInformation("Demo tenant created with ID {TenantId}", tenantId);
        }
        else
        {
            tenantId = existingTenant.Id;
            if (existingTenant.Name != "Ottomotive BESS")
            {
                existingTenant.Name = "Ottomotive BESS";
                await _context.SaveChangesAsync();
            }
            _logger.LogInformation("Using existing tenant with ID {TenantId}", tenantId);
        }

        // Create Firm (required for Facility foreign key)
        // Use IgnoreQueryFilters to check ALL firms regardless of current tenant context
        var existingFirm = await _context.Firms.IgnoreQueryFilters().OrderBy(x => x.Created).FirstOrDefaultAsync();
        if (existingFirm == null)
        {
            var firm = new Firm
            {
                TenantId = tenantId,
                Name = "Ottomotive Energy",
                Type = "Energy Storage",
                Country = "Türkiye",
                State = "İstanbul",
                City = "İstanbul",
                District = "Kadıköy",
                PostalCode = "34734",
                Address = "Ottomotive BESS Facility",
                ContactEmail = "contact@example.com",
                ContactPhone = "+90 216 123 4567",
                Fax = "+90 216 123 4568",
                TaxNumber = "TR1234567890",
                TaxOffice = "İstanbul",
                Created = DateTime.UtcNow
            };
            _context.Firms.Add(firm);
            await _context.SaveChangesAsync();
            firmId = firm.Id;
            _logger.LogInformation("Demo firm created with ID {FirmId}", firmId);
        }
        else
        {
            firmId = existingFirm.Id;
            if (existingFirm.Name != "Ottomotive Energy")
            {
                existingFirm.Name = "Ottomotive Energy";
                await _context.SaveChangesAsync();
            }
            _logger.LogInformation("Using existing firm with ID {FirmId}", firmId);
        }

        // Create Facility (required for Device foreign key)
        // Use IgnoreQueryFilters to check ALL facilities regardless of current tenant context
        var existingFacility = await _context.Facilities.IgnoreQueryFilters().OrderBy(x => x.Created).FirstOrDefaultAsync();
        if (existingFacility == null)
        {
            var facility = new Facility
            {
                FirmId = firmId,
                Name = "Ottomotive BESS",
                Address = "Ottomotive Energy Storage Facility, Kadıköy, İstanbul, Türkiye",
                EnergyCapacity = 3000,
                EnergyUnit = Backend.Domain.Enums.EnergyUnit.KWh,
                Longitude = 29.0343, // Istanbul coordinates
                Latitude = 40.9815,
                Created = DateTime.UtcNow
            };
            _context.Facilities.Add(facility);
            await _context.SaveChangesAsync();
            facilitiyId = facility.Id;
            _logger.LogInformation("Demo facility created with ID {FacilityId}", facilitiyId);
        }
        else
        {
            facilitiyId = existingFacility.Id;
            if (existingFacility.Name != "Ottomotive BESS")
            {
                existingFacility.Name = "Ottomotive BESS";
                await _context.SaveChangesAsync();
            }
            _logger.LogInformation("Using existing facility with ID {FacilityId}", facilitiyId);
        }

        #region RolesAndUsers

        // Create Administrator role (super admin with full system access)
        var administratorRole = new IdentityRole(Roles.Administrator);
        if (_roleManager.Roles.All(r => r.Name != administratorRole.Name))
        {
            await _roleManager.CreateAsync(administratorRole);
            _logger.LogInformation("Created role: {Role}", Roles.Administrator);
        }

        // Create TenantAdmin role (tenant administrator)
        var tenantAdminRole = new IdentityRole(Roles.TenantAdmin);
        if (_roleManager.Roles.All(r => r.Name != tenantAdminRole.Name))
        {
            await _roleManager.CreateAsync(tenantAdminRole);
            _logger.LogInformation("Created role: {Role}", Roles.TenantAdmin);
        }

        // Create User role (regular user with limited access)
        var userRole = new IdentityRole(Roles.User);
        if (_roleManager.Roles.All(r => r.Name != userRole.Name))
        {
            await _roleManager.CreateAsync(userRole);
            _logger.LogInformation("Created role: {Role}", Roles.User);
        }

        // Create Operator role (view + settings control)
        var operatorRole = new IdentityRole(Roles.Operator);
        if (_roleManager.Roles.All(r => r.Name != operatorRole.Name))
        {
            await _roleManager.CreateAsync(operatorRole);
            _logger.LogInformation("Created role: {Role}", Roles.Operator);
        }

        // Create Viewer role (read-only access)
        var viewerRole = new IdentityRole(Roles.Viewer);
        if (_roleManager.Roles.All(r => r.Name != viewerRole.Name))
        {
            await _roleManager.CreateAsync(viewerRole);
            _logger.LogInformation("Created role: {Role}", Roles.Viewer);
        }

        // Create Administrator user
        var administrator = new ApplicationUser
        {
            UserName = "administrator@localhost",
            Email = "administrator@localhost",
            TenantId = tenantId,
            Name = "Admin",
            Surname = "User",
            Title = "System Administrator",
            Description = "Demo system administrator account"
        };

        if (_userManager.Users.All(u => u.UserName != administrator.UserName))
        {
            var result = await _userManager.CreateAsync(administrator, "***REMOVED***");
            if (result.Succeeded)
            {
                await _userManager.AddToRolesAsync(administrator, new[] { administratorRole.Name! });

                // Create UserFirmFacility association
                var userFirmFacility = new UserFirmFacility
                {
                    TenantId = tenantId,
                    UserId = administrator.Id,
                    FirmId = firmId,
                    FacilityId = facilitiyId,
                    Created = DateTime.UtcNow
                };
                _context.UserFirmFacilities.Add(userFirmFacility);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Demo administrator user created successfully");
            }
            else
            {
                _logger.LogWarning("Failed to create demo administrator user: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        // Create Oklidis Admin user (for testing)
        var oklidisAdmin = new ApplicationUser
        {
            UserName = "admin@oklidis.com.tr",
            Email = "admin@oklidis.com.tr",
            EmailConfirmed = true,
            TenantId = tenantId,
            Name = "Oklidis",
            Surname = "Admin",
            Title = "System Administrator",
            Description = "Oklidis test administrator account"
        };

        if (_userManager.Users.All(u => u.UserName != oklidisAdmin.UserName))
        {
            var result = await _userManager.CreateAsync(oklidisAdmin, "Admin@123");
            if (result.Succeeded)
            {
                await _userManager.AddToRolesAsync(oklidisAdmin, new[] { Roles.Administrator });

                var userFirmFacility = new UserFirmFacility
                {
                    TenantId = tenantId,
                    UserId = oklidisAdmin.Id,
                    FirmId = firmId,
                    FacilityId = facilitiyId,
                    Created = DateTime.UtcNow
                };
                _context.UserFirmFacilities.Add(userFirmFacility);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Oklidis admin user created successfully (admin@oklidis.com.tr)");
            }
            else
            {
                _logger.LogWarning("Failed to create Oklidis admin user: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        #endregion

        #region FeaturesAndPermissions

        // Seed Features and Permissions
        if (!_context.Set<Feature>().IgnoreQueryFilters().Any())
        {
            var features = new Dictionary<string, Feature>
            {
                [Features.Portfolio] = new Feature { Key = Features.Portfolio, Name = "Portfolio", Category = "Core", Created = DateTime.UtcNow },
                [Features.Dashboard] = new Feature { Key = Features.Dashboard, Name = "Dashboard", Category = "Core", Created = DateTime.UtcNow },
                [Features.Monitoring] = new Feature { Key = Features.Monitoring, Name = "Monitoring", Category = "Core", Created = DateTime.UtcNow },
                [Features.Solar] = new Feature { Key = Features.Solar, Name = "Solar", Category = "Energy", Created = DateTime.UtcNow },
                [Features.Bess] = new Feature { Key = Features.Bess, Name = "BESS", Category = "Energy", Created = DateTime.UtcNow },
                [Features.GridAndLoads] = new Feature { Key = Features.GridAndLoads, Name = "Grid & Loads", Category = "Energy", Created = DateTime.UtcNow },
                [Features.Scada] = new Feature { Key = Features.Scada, Name = "SCADA", Category = "Energy", Created = DateTime.UtcNow },
                [Features.Analytics] = new Feature { Key = Features.Analytics, Name = "Analytics", Category = "Intelligence", Created = DateTime.UtcNow },
                [Features.Financials] = new Feature { Key = Features.Financials, Name = "Financials", Category = "Intelligence", Created = DateTime.UtcNow },
                [Features.Reporting] = new Feature { Key = Features.Reporting, Name = "Reporting", Category = "Intelligence", Created = DateTime.UtcNow },
                [Features.Operations] = new Feature { Key = Features.Operations, Name = "Operations", Category = "Management", Created = DateTime.UtcNow },
                [Features.Settings] = new Feature { Key = Features.Settings, Name = "Settings", Category = "Management", Created = DateTime.UtcNow },
                [Features.UserManagement] = new Feature { Key = Features.UserManagement, Name = "User Management", Category = "Management", Created = DateTime.UtcNow },
                [Features.Audit] = new Feature { Key = Features.Audit, Name = "Audit", Category = "Management", Created = DateTime.UtcNow },
                [Features.FacilityMap] = new Feature { Key = Features.FacilityMap, Name = "Facility Map", Category = "Core", Created = DateTime.UtcNow },
                [Features.Pcs] = new Feature { Key = Features.Pcs, Name = "PCS Monitoring", Category = "Energy", Created = DateTime.UtcNow },
                [Features.Alarm] = new Feature { Key = Features.Alarm, Name = "Alarms", Category = "Management", Created = DateTime.UtcNow },
                [Features.Maintenance] = new Feature { Key = Features.Maintenance, Name = "Maintenance", Category = "Management", Created = DateTime.UtcNow },
            };

            _context.Set<Feature>().AddRange(features.Values);
            await _context.SaveChangesAsync();

            // Seed Permissions mapped to features
            var permissions = new List<Permission>
            {
                new() { Key = Permissions.PortfolioView, Name = "View Portfolio", FeatureId = features[Features.Portfolio].Id, Created = DateTime.UtcNow },
                new() { Key = Permissions.DashboardView, Name = "View Dashboard", FeatureId = features[Features.Dashboard].Id, Created = DateTime.UtcNow },
                new() { Key = Permissions.MonitoringView, Name = "View Monitoring", FeatureId = features[Features.Monitoring].Id, Created = DateTime.UtcNow },
                new() { Key = Permissions.SolarView, Name = "View Solar", FeatureId = features[Features.Solar].Id, Created = DateTime.UtcNow },
                new() { Key = Permissions.BessView, Name = "View BESS", FeatureId = features[Features.Bess].Id, Created = DateTime.UtcNow },
                new() { Key = Permissions.GridAndLoadsView, Name = "View Grid & Loads", FeatureId = features[Features.GridAndLoads].Id, Created = DateTime.UtcNow },
                new() { Key = Permissions.ScadaView, Name = "View SCADA", FeatureId = features[Features.Scada].Id, Created = DateTime.UtcNow },
                new() { Key = Permissions.ScadaControl, Name = "Control SCADA", FeatureId = features[Features.Scada].Id, Created = DateTime.UtcNow },
                new() { Key = Permissions.AnalyticsView, Name = "View Analytics", FeatureId = features[Features.Analytics].Id, Created = DateTime.UtcNow },
                new() { Key = Permissions.FinancialsView, Name = "View Financials", FeatureId = features[Features.Financials].Id, Created = DateTime.UtcNow },
                new() { Key = Permissions.ReportingView, Name = "View Reporting", FeatureId = features[Features.Reporting].Id, Created = DateTime.UtcNow },
                new() { Key = Permissions.ReportingExport, Name = "Export Reports", FeatureId = features[Features.Reporting].Id, Created = DateTime.UtcNow },
                new() { Key = Permissions.OperationsView, Name = "View Operations", FeatureId = features[Features.Operations].Id, Created = DateTime.UtcNow },
                new() { Key = Permissions.OperationsManage, Name = "Manage Operations", FeatureId = features[Features.Operations].Id, Created = DateTime.UtcNow },
                new() { Key = Permissions.FirmView, Name = "View Firms", FeatureId = features[Features.Settings].Id, Created = DateTime.UtcNow },
                new() { Key = Permissions.FirmCreate, Name = "Create Firms", FeatureId = features[Features.Settings].Id, Created = DateTime.UtcNow },
                new() { Key = Permissions.FirmEdit, Name = "Edit Firms", FeatureId = features[Features.Settings].Id, Created = DateTime.UtcNow },
                new() { Key = Permissions.FirmDelete, Name = "Delete Firms", FeatureId = features[Features.Settings].Id, Created = DateTime.UtcNow },
                new() { Key = Permissions.FacilityView, Name = "View Facilities", FeatureId = features[Features.Settings].Id, Created = DateTime.UtcNow },
                new() { Key = Permissions.FacilityCreate, Name = "Create Facilities", FeatureId = features[Features.Settings].Id, Created = DateTime.UtcNow },
                new() { Key = Permissions.FacilityEdit, Name = "Edit Facilities", FeatureId = features[Features.Settings].Id, Created = DateTime.UtcNow },
                new() { Key = Permissions.FacilityDelete, Name = "Delete Facilities", FeatureId = features[Features.Settings].Id, Created = DateTime.UtcNow },
                new() { Key = Permissions.DeviceView, Name = "View Devices", FeatureId = features[Features.Settings].Id, Created = DateTime.UtcNow },
                new() { Key = Permissions.DeviceCreate, Name = "Create Devices", FeatureId = features[Features.Settings].Id, Created = DateTime.UtcNow },
                new() { Key = Permissions.UserManagementView, Name = "View Users", FeatureId = features[Features.UserManagement].Id, Created = DateTime.UtcNow },
                new() { Key = Permissions.UserManagementCreate, Name = "Create Users", FeatureId = features[Features.UserManagement].Id, Created = DateTime.UtcNow },
                new() { Key = Permissions.UserManagementEdit, Name = "Edit Users", FeatureId = features[Features.UserManagement].Id, Created = DateTime.UtcNow },
                new() { Key = Permissions.UserManagementDelete, Name = "Delete Users", FeatureId = features[Features.UserManagement].Id, Created = DateTime.UtcNow },
                new() { Key = Permissions.AuditView, Name = "View Audit Logs", FeatureId = features[Features.Audit].Id, Created = DateTime.UtcNow },
                new() { Key = Permissions.FacilityMapView, Name = "View Facility Map", FeatureId = features[Features.FacilityMap].Id, Created = DateTime.UtcNow },
                new() { Key = Permissions.PcsView, Name = "View PCS Data", FeatureId = features[Features.Pcs].Id, Created = DateTime.UtcNow },
                new() { Key = Permissions.AlarmView, Name = "View Alarms", FeatureId = features[Features.Alarm].Id, Created = DateTime.UtcNow },
                new() { Key = Permissions.AlarmManage, Name = "Manage Alarms", FeatureId = features[Features.Alarm].Id, Created = DateTime.UtcNow },
                new() { Key = Permissions.MaintenanceView, Name = "View Maintenance Tasks", FeatureId = features[Features.Maintenance].Id, Created = DateTime.UtcNow },
                new() { Key = Permissions.MaintenanceCreate, Name = "Create Maintenance Tasks", FeatureId = features[Features.Maintenance].Id, Created = DateTime.UtcNow },
                new() { Key = Permissions.MaintenanceManage, Name = "Manage Maintenance Tasks", FeatureId = features[Features.Maintenance].Id, Created = DateTime.UtcNow },
            };

            _context.Set<Permission>().AddRange(permissions);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeded {FeatureCount} features and {PermissionCount} permissions", features.Count, permissions.Count);

            // Seed Plans with feature assignments
            var starterFeatureKeys = new[] { Features.Portfolio, Features.Dashboard, Features.Monitoring, Features.FacilityMap, Features.Settings };
            var professionalFeatureKeys = starterFeatureKeys.Concat(new[] { Features.Solar, Features.Bess, Features.Pcs, Features.GridAndLoads, Features.Analytics, Features.Financials, Features.Reporting }).ToArray();
            var enterpriseFeatureKeys = features.Keys.ToArray();

            if (!_context.Plans.IgnoreQueryFilters().Any(p => p.Name == "Starter"))
            {
                var starterPlan = new Plan { Name = "Starter", Modules = "Portfolio,Dashboard,Monitoring,FacilityMap,Settings", ApiAccess = false, MaxFacilities = 5, MaxUsers = 10, Price = 0 };
                var professionalPlan = new Plan { Name = "Professional", Modules = "All Starter + Solar,Bess,GridAndLoads,Analytics,Financials,Reporting", ApiAccess = true, MaxFacilities = 50, MaxUsers = 100, Price = 499 };
                var enterprisePlan = new Plan { Name = "Enterprise", Modules = "All Features", ApiAccess = true, MaxFacilities = 999, MaxUsers = 999, Price = 1999 };

                _context.Plans.AddRange(starterPlan, professionalPlan, enterprisePlan);
                await _context.SaveChangesAsync();

                // Create PlanFeature associations
                var planFeatures = new List<PlanFeature>();

                foreach (var key in starterFeatureKeys)
                    planFeatures.Add(new PlanFeature { PlanId = starterPlan.Id, FeatureId = features[key].Id, Created = DateTime.UtcNow });

                foreach (var key in professionalFeatureKeys)
                    planFeatures.Add(new PlanFeature { PlanId = professionalPlan.Id, FeatureId = features[key].Id, Created = DateTime.UtcNow });

                foreach (var key in enterpriseFeatureKeys)
                    planFeatures.Add(new PlanFeature { PlanId = enterprisePlan.Id, FeatureId = features[key].Id, Created = DateTime.UtcNow });

                _context.Set<PlanFeature>().AddRange(planFeatures);
                await _context.SaveChangesAsync();

                // Assign demo tenant to Enterprise plan
                var demoTenant = await _context.Tenants.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == tenantId);
                if (demoTenant != null)
                {
                    demoTenant.PlanId = enterprisePlan.Id;
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation("Seeded 3 plans (Starter, Professional, Enterprise) with feature assignments");
            }
        }

        #endregion

        #region DeviceConf

        var deviceTypes = new List<DeviceType>
        {
            new DeviceType { Name = DeviceTypeNames.Solar, Created = DateTime.UtcNow },
            new DeviceType { Name = "Generator", Created = DateTime.UtcNow },
            new DeviceType { Name = "Wind Turbine", Created = DateTime.UtcNow },
            new DeviceType { Name = DeviceTypeNames.EnergyStorage, Created = DateTime.UtcNow },
            new DeviceType { Name = DeviceTypeNames.Inverter, Created = DateTime.UtcNow },
            new DeviceType { Name = DeviceTypeNames.Pcs, Created = DateTime.UtcNow },
            new DeviceType { Name = DeviceTypeNames.SmartMeter, Created = DateTime.UtcNow },
            new DeviceType { Name = "Grid", Created = DateTime.UtcNow },
            new DeviceType { Name = "DC Bus", Created = DateTime.UtcNow },
            new DeviceType { Name = "Transformer", Created = DateTime.UtcNow },
            new DeviceType { Name = "Load", Created = DateTime.UtcNow },
            new DeviceType { Name = "EV Charger", Created = DateTime.UtcNow },
            new DeviceType { Name = "Heat Pump", Created = DateTime.UtcNow },
            // EnergyStorage hierarchy sub-types
            new DeviceType { Name = DeviceTypeNames.EnergyStorageBattery, Created = DateTime.UtcNow },
            new DeviceType { Name = DeviceTypeNames.EnergyStorageRack, Created = DateTime.UtcNow },
            new DeviceType { Name = DeviceTypeNames.EnergyStoragePack, Created = DateTime.UtcNow },
            new DeviceType { Name = DeviceTypeNames.EnergyStorageModule, Created = DateTime.UtcNow },
            new DeviceType { Name = DeviceTypeNames.EnergyStorageController, Created = DateTime.UtcNow },
            new DeviceType { Name = DeviceTypeNames.EnergyStorageTMS, Created = DateTime.UtcNow }
        };
        if (!_context.DeviceTypes.Any())
        {
            _context.DeviceTypes.AddRange(deviceTypes);
            await _context.SaveChangesAsync();
        }
        else
        {
            // Seed new battery hierarchy sub-types if they don't exist yet
            var existingNames = await _context.DeviceTypes.Select(dt => dt.Name).ToListAsync();
            var newTypes = deviceTypes.Where(dt => !existingNames.Contains(dt.Name)).ToList();
            if (newTypes.Count != 0)
            {
                _context.DeviceTypes.AddRange(newTypes);
                await _context.SaveChangesAsync();
            }
        }

        // Reload device types from DB to get their Guid IDs
        var savedDeviceTypes = await _context.DeviceTypes.ToListAsync();
        var dtLookup = savedDeviceTypes.ToDictionary(dt => dt.Name!, dt => dt.Id);

        // Seed RTU (Remote Terminal Unit) for the demo facility
        // Look for the known demo RTU first, then fall back to any RTU on this facility, then create one
        const string demoRtuSerial = "ec-100-38100938B6076C91";
        Guid rtuId;
        var existingRtu = await _context.Rtus.IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.RtuSerialNumber == demoRtuSerial);
        if (existingRtu != null)
        {
            rtuId = existingRtu.Id;
            // Ensure the RTU is attached to the demo facility
            if (existingRtu.FacilityId != facilitiyId)
            {
                existingRtu.FacilityId = facilitiyId;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Reassigned RTU {RtuSerial} to demo facility {FacilityId}", demoRtuSerial, facilitiyId);
            }
            if (existingRtu.RtuModel != "Ottomotive RTU")
            {
                existingRtu.RtuModel = "Ottomotive RTU";
                await _context.SaveChangesAsync();
            }
            _logger.LogInformation("Using existing demo RTU {RtuSerial} with ID {RtuId}", demoRtuSerial, rtuId);
        }
        else
        {
            // Fall back to any RTU already on this facility
            var facilityRtu = await _context.Rtus.IgnoreQueryFilters()
                .FirstOrDefaultAsync(r => r.FacilityId == facilitiyId);
            if (facilityRtu != null)
            {
                rtuId = facilityRtu.Id;
                _logger.LogInformation("Using existing facility RTU with ID {RtuId}", rtuId);
            }
            else
            {
                var rtu = new Rtu
                {
                    FacilityId = facilitiyId,
                    RtuSerialNumber = demoRtuSerial,
                    RtuModel = "Ottomotive RTU",
                    Protocol = "Modbus TCP",
                    Status = "online",
                    Created = DateTime.UtcNow
                };
                _context.Rtus.Add(rtu);
                await _context.SaveChangesAsync();
                rtuId = rtu.Id;
                _logger.LogInformation("Demo RTU created with serial {RtuSerial}, ID {RtuId}", demoRtuSerial, rtuId);
            }
        }

        // Seed Ottomotive BESS devices
        var existingDeviceNames = await _context.Devices.IgnoreQueryFilters()
            .Where(d => d.RtuId == rtuId)
            .Select(d => d.Name)
            .ToListAsync();

        // Root container device
        Guid bessContainerId;
        if (!existingDeviceNames.Contains("BESS_001"))
        {
            var bessContainer = new Device
            {
                RtuId = rtuId,
                DeviceTypeId = dtLookup[DeviceTypeNames.EnergyStorage],
                Name = "BESS_001",
                Model = "Ottomotive BESS Container",
                SerialNumber = "DEMO-BG0RS3P12S0001",
                Manufacturer = "Ottomotive"
            };
            _context.Devices.Add(bessContainer);
            await _context.SaveChangesAsync();
            bessContainerId = bessContainer.Id;
            _logger.LogInformation("Seeded BESS container device BESS_001");
        }
        else
        {
            var existingBess = await _context.Devices.IgnoreQueryFilters()
                .FirstAsync(d => d.RtuId == rtuId && d.Name == "BESS_001");
            bessContainerId = existingBess.Id;
            if (existingBess.Model != "Ottomotive BESS Container" || existingBess.Manufacturer != "Ottomotive")
            {
                existingBess.Model = "Ottomotive BESS Container";
                existingBess.Manufacturer = "Ottomotive";
                existingBess.SerialNumber = "DEMO-BG0RS3P12S0001";
                await _context.SaveChangesAsync();
            }
        }

        // 12 Racks (children of BESS_001)
        var rackDevices = new List<Device>();
        for (var i = 1; i <= 12; i++)
        {
            var rackName = $"Rack {i}";
            if (!existingDeviceNames.Contains(rackName))
            {
                rackDevices.Add(new Device
                {
                    RtuId = rtuId,
                    DeviceTypeId = dtLookup[DeviceTypeNames.EnergyStorageRack],
                    ParentDeviceId = bessContainerId,
                    HierarchyIndex = i - 1,
                    Name = rackName,
                    Model = "Ottomotive Battery Rack",
                    SerialNumber = $"DEMO-BG0RS3P12S0001-Rack-{i}",
                    Manufacturer = "Ottomotive"
                });
            }
        }
        if (rackDevices.Count > 0)
        {
            _context.Devices.AddRange(rackDevices);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeded {Count} rack devices", rackDevices.Count);
        }

        // PCS device (child of BESS_001)
        if (!existingDeviceNames.Contains("PCS_001"))
        {
            var pcs = new Device
            {
                RtuId = rtuId,
                DeviceTypeId = dtLookup[DeviceTypeNames.Pcs],
                ParentDeviceId = bessContainerId,
                Name = "PCS_001",
                Model = "Ottomotive PCS",
                SerialNumber = "DEMO-PCS-001",
                Manufacturer = "Ottomotive"
            };
            _context.Devices.Add(pcs);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeded PCS device PCS_001");
        }

        // HVAC/Chiller device (child of BESS_001)
        if (!existingDeviceNames.Contains("HVAC_001"))
        {
            var hvac = new Device
            {
                RtuId = rtuId,
                DeviceTypeId = dtLookup[DeviceTypeNames.EnergyStorageTMS],
                ParentDeviceId = bessContainerId,
                Name = "HVAC_001",
                Model = "Ottomotive TMS",
                SerialNumber = "DEMO-TMS-001",
                Manufacturer = "Ottomotive"
            };
            _context.Devices.Add(hvac);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeded HVAC device HVAC_001");
        }

        // Seed or update DeviceSpecificationTemplate for Ottomotive BESS
        {
            var batteryTypeId = dtLookup.GetValueOrDefault(DeviceTypeNames.EnergyStorage);
            if (batteryTypeId != Guid.Empty)
            {
                // Remove any old templates (e.g., Tesla Megapack) and replace with Ottomotive
                var existingTemplates = await _context.DeviceSpecificationTemplates
                    .Where(t => t.DeviceTypeId == batteryTypeId)
                    .ToListAsync();
                if (existingTemplates.Any(t => t.Model != "Ottomotive BESS Container"))
                {
                    _context.DeviceSpecificationTemplates.RemoveRange(
                        existingTemplates.Where(t => t.Model != "Ottomotive BESS Container"));
                    await _context.SaveChangesAsync();
                }

                var existingOttomotive = existingTemplates
                    .FirstOrDefault(t => t.Model == "Ottomotive BESS Container");
                var isNewTemplate = existingOttomotive == null;

                var ottomotiveBessTemplate = existingOttomotive ?? new DeviceSpecificationTemplate
                {
                    DeviceTypeId = batteryTypeId,
                    Created = DateTime.UtcNow
                };
                ottomotiveBessTemplate.Model = "Ottomotive BESS Container";
                ottomotiveBessTemplate.Manufacturer = "Ottomotive";
                ottomotiveBessTemplate.IsDefault = true;
                ottomotiveBessTemplate.SpecificationsTemplate = """
                    {
                      "supportedTabs": [
                        { "key": "summary", "label": "Summary", "isEnabled": true, "order": 0 },
                        { "key": "health", "label": "Health", "isEnabled": true, "order": 1 },
                        { "key": "hierarchy", "label": "Hierarchy", "isEnabled": true, "order": 2 },
                        { "key": "curves", "label": "Curves", "isEnabled": true, "order": 3 },
                        { "key": "cells", "label": "Cells", "isEnabled": true, "order": 4 },
                        { "key": "faults", "label": "Faults", "isEnabled": true, "order": 5 },
                        { "key": "settings", "label": "Settings", "isEnabled": true, "order": 6 }
                      ],
                      "tabs": {
                        "summary": {
                          "sections": [
                            {
                              "key": "liveStatus",
                              "title": "Live Status",
                              "order": 0,
                              "fields": [
                                { "key": "soc", "label": "State of Charge", "unit": "%", "order": 0 },
                                { "key": "soh", "label": "State of Health", "unit": "%", "order": 1 },
                                { "key": "operatingMode", "label": "Operating Mode", "order": 2 },
                                { "key": "powerDirection", "label": "Power Direction", "order": 3 }
                              ]
                            },
                            {
                              "key": "electrical",
                              "title": "Electrical",
                              "order": 1,
                              "fields": [
                                { "key": "voltage", "label": "Voltage", "unit": "V", "order": 0 },
                                { "key": "current", "label": "Current", "unit": "A", "order": 1 },
                                { "key": "power", "label": "Power", "unit": "kW", "order": 2 }
                              ]
                            },
                            {
                              "key": "energy",
                              "title": "Energy",
                              "order": 2,
                              "fields": [
                                { "key": "remainingEnergy", "label": "Remaining Energy", "unit": "kWh", "order": 0 },
                                { "key": "availableEnergy", "label": "Available Energy", "unit": "kWh", "order": 1 },
                                { "key": "cycleCount", "label": "Cycle Count", "order": 2 }
                              ]
                            },
                            {
                              "key": "temperature",
                              "title": "Temperature",
                              "order": 3,
                              "fields": [
                                { "key": "tempMin", "label": "Min Cell Temp", "unit": "\u00b0C", "order": 0 },
                                { "key": "tempMax", "label": "Max Cell Temp", "unit": "\u00b0C", "order": 1 },
                                { "key": "ambientTemp", "label": "Ambient Temp", "unit": "\u00b0C", "order": 2 }
                              ]
                            },
                            {
                              "key": "cellVoltage",
                              "title": "Cell Voltage",
                              "order": 4,
                              "fields": [
                                { "key": "cellVoltageMax", "label": "Max Cell Voltage", "unit": "mV", "order": 0 },
                                { "key": "cellVoltageMin", "label": "Min Cell Voltage", "unit": "mV", "order": 1 },
                                { "key": "cellVoltageAvg", "label": "Avg Cell Voltage", "unit": "mV", "order": 2 },
                                { "key": "voltageSpread", "label": "Voltage Spread", "unit": "mV", "order": 3 }
                              ]
                            }
                          ]
                        }
                      },
                      "hierarchy": [
                        { "key": "rack", "label": "Rack", "hasChildren": false }
                      ],
                      "curveCharts": [
                        {
                          "key": "socTrend",
                          "title": "State of Charge",
                          "chartType": "area",
                          "series": [
                            { "key": "soc", "label": "SoC", "unit": "%", "color": "#2196F3" }
                          ]
                        },
                        {
                          "key": "powerTrend",
                          "title": "Power",
                          "chartType": "line",
                          "series": [
                            { "key": "power", "label": "Battery Power", "unit": "kW", "color": "#4CAF50" },
                            { "key": "pcsActivePower", "label": "PCS Active Power", "unit": "kW", "color": "#FF9800" }
                          ]
                        },
                        {
                          "key": "voltageTrend",
                          "title": "Voltage & Current",
                          "chartType": "line",
                          "series": [
                            { "key": "voltage", "label": "Voltage", "unit": "V", "color": "#9C27B0" },
                            { "key": "current", "label": "Current", "unit": "A", "color": "#F44336" }
                          ]
                        },
                        {
                          "key": "temperatureTrend",
                          "title": "Temperature",
                          "chartType": "area",
                          "series": [
                            { "key": "tempMax", "label": "Max Temp", "unit": "\u00b0C", "color": "#F44336" },
                            { "key": "tempMin", "label": "Min Temp", "unit": "\u00b0C", "color": "#2196F3" },
                            { "key": "ambientTemp", "label": "Ambient", "unit": "\u00b0C", "color": "#4CAF50" }
                          ]
                        }
                      ]
                    }
                    """;
                ottomotiveBessTemplate.ParameterMapTemplate = """
                    {
                      "version": 1,
                      "parameters": {
                        "soc": {
                          "type": "number", "unit": "%",
                          "source": { "kind": "Battery.Energy", "paths": ["soc"] }
                        },
                        "soh": {
                          "type": "number", "unit": "%",
                          "source": { "kind": "Battery.Soh", "paths": ["soh"] }
                        },
                        "voltage": {
                          "type": "number", "unit": "V",
                          "source": { "kind": "Battery.Energy", "paths": ["voltage"] }
                        },
                        "current": {
                          "type": "number", "unit": "A",
                          "source": { "kind": "Battery.Energy", "paths": ["current"] }
                        },
                        "power": {
                          "type": "number", "unit": "kW",
                          "formula": {
                            "expression": "round(multiply(voltage, current, 0.001), 1)",
                            "inputs": { "voltage": {"$ref":"voltage"}, "current": {"$ref":"current"} }
                          },
                          "fallback": { "kind": "Battery.Energy", "paths": ["power"] }
                        },
                        "powerDirection": {
                          "type": "string",
                          "formula": {
                            "expression": "cond(lt(current, 0), 'discharging', cond(gt(current, 0), 'charging', 'idle'))",
                            "inputs": { "current": {"$ref":"current"} }
                          },
                          "fallback": { "kind": "Battery.Energy", "paths": ["powerDirection"] }
                        },
                        "remainingEnergy": {
                          "type": "number", "unit": "kWh",
                          "source": { "kind": "Battery.Energy", "paths": ["remainingEnergy"] }
                        },
                        "availableEnergy": {
                          "type": "number", "unit": "kWh",
                          "source": { "kind": "Battery.Energy", "paths": ["availableEnergy"] }
                        },
                        "tempMin": {
                          "type": "number", "unit": "C",
                          "source": { "kind": "Battery.Energy", "paths": ["tempMin", "cellTempMin"] }
                        },
                        "tempMax": {
                          "type": "number", "unit": "C",
                          "source": { "kind": "Battery.Energy", "paths": ["tempMax", "cellTempMax"] }
                        },
                        "tempAvg": {
                          "type": "number", "unit": "C",
                          "formula": {
                            "expression": "round(avg(tempMin, tempMax), 1)",
                            "inputs": { "tempMin": {"$ref":"tempMin"}, "tempMax": {"$ref":"tempMax"} }
                          }
                        },
                        "ambientTemp": {
                          "type": "number", "unit": "C",
                          "source": { "kind": "Battery.Energy", "paths": ["ambientTemp"] }
                        },
                        "cellVoltageMax": {
                          "type": "number", "unit": "mV",
                          "source": { "kind": "Battery.Energy", "paths": ["cellVoltageMax"] }
                        },
                        "cellVoltageMin": {
                          "type": "number", "unit": "mV",
                          "source": { "kind": "Battery.Energy", "paths": ["cellVoltageMin"] }
                        },
                        "cellVoltageAvg": {
                          "type": "number", "unit": "mV",
                          "source": { "kind": "Battery.Energy", "paths": ["cellVoltageAvg"] }
                        },
                        "voltageSpread": {
                          "type": "number", "unit": "mV",
                          "formula": {
                            "expression": "subtract(cellVoltageMax, cellVoltageMin)",
                            "inputs": { "cellVoltageMax": {"$ref":"cellVoltageMax"}, "cellVoltageMin": {"$ref":"cellVoltageMin"} }
                          }
                        },
                        "cycleCount": {
                          "type": "number",
                          "source": { "kind": "Battery.Health", "paths": ["cycleCount"] }
                        },
                        "cycleLifePct": {
                          "type": "number", "unit": "%",
                          "formula": {
                            "expression": "round(multiply(divide(cycles, designed), 100), 1)",
                            "inputs": {
                              "cycles": {"$ref":"cycleCount"},
                              "designed": {"source": {"origin":"metadata", "paths":["designedCycleLife"], "default":6000}}
                            }
                          }
                        },
                        "operatingMode": {
                          "type": "string",
                          "source": { "kind": "Battery.Status", "paths": ["operatingMode"] }
                        },
                        "pcsActivePower": {
                          "type": "number", "unit": "kW",
                          "source": { "kind": "Pcs.PowerDistribution", "paths": ["pcsActivePower"] }
                        },
                        "pcsReactivePower": {
                          "type": "number", "unit": "kVAR",
                          "source": { "kind": "Pcs.PowerDistribution", "paths": ["pcsReactivePower"] }
                        },
                        "pcsDcPower": {
                          "type": "number", "unit": "kW",
                          "source": { "kind": "Pcs.DcMeasurement", "paths": ["pcsDcPower"] }
                        },
                        "pcsGridFreq": {
                          "type": "number", "unit": "Hz",
                          "source": { "kind": "Pcs.AcMeasurement", "paths": ["pcsGridFreq"] }
                        },
                        "pcsWorkingCondition": {
                          "type": "string",
                          "source": { "kind": "Pcs.Status", "paths": ["pcsWorkingCondition"] }
                        },
                        "pcsEfficiency": {
                          "type": "number", "unit": "%",
                          "source": { "kind": "Pcs.Status", "paths": ["pcsEfficiency"] }
                        },
                        "pcsIgbtTemp": {
                          "type": "number", "unit": "C",
                          "source": { "kind": "Pcs.Status", "paths": ["pcsIgbtTemp"] }
                        },
                        "pcsAmbientTemp": {
                          "type": "number", "unit": "C",
                          "source": { "kind": "Pcs.Status", "paths": ["pcsAmbientTemp"] }
                        },
                        "pcsInductanceTemp": {
                          "type": "number", "unit": "C",
                          "source": { "kind": "Pcs.Status", "paths": ["pcsInductanceTemp"] }
                        },
                        "pcsTotalDcBusVoltage": {
                          "type": "number", "unit": "V",
                          "source": { "kind": "Pcs.DcMeasurement", "paths": ["pcsTotalDcBusVoltage"] }
                        },
                        "pcsLineVoltageAB": {
                          "type": "number", "unit": "V",
                          "source": { "kind": "Pcs.AcMeasurement", "paths": ["pcsLineVoltageAB"] }
                        },
                        "pcsPhaseCurrentA": {
                          "type": "number", "unit": "A",
                          "source": { "kind": "Pcs.AcMeasurement", "paths": ["pcsPhaseCurrentA"] }
                        },
                        "pcsPhaseCurrentB": {
                          "type": "number", "unit": "A",
                          "source": { "kind": "Pcs.AcMeasurement", "paths": ["pcsPhaseCurrentB"] }
                        },
                        "pcsPhaseCurrentC": {
                          "type": "number", "unit": "A",
                          "source": { "kind": "Pcs.AcMeasurement", "paths": ["pcsPhaseCurrentC"] }
                        },
                        "chillerCoolantOut": {
                          "type": "number", "unit": "C",
                          "source": { "kind": "Battery.Thermal", "paths": ["chillerCoolantOut"] }
                        },
                        "chillerCoolantIn": {
                          "type": "number", "unit": "C",
                          "source": { "kind": "Battery.Thermal", "paths": ["chillerCoolantIn"] }
                        },
                        "compressorRunning": {
                          "type": "boolean",
                          "source": { "kind": "Battery.Thermal", "paths": ["compressorRunning"] }
                        },
                        "chillerAmbientTemp": {
                          "type": "number", "unit": "C",
                          "source": { "kind": "Battery.Thermal", "paths": ["ambientTemp"] }
                        }
                      }
                    }
                    """;
                ottomotiveBessTemplate.MetadataTemplate = """
                    {
                      "ratedCapacityKwh": 3000,
                      "ratedPowerKw": 1500,
                      "nominalVoltage": 800,
                      "cellChemistry": "LFP",
                      "rackCount": 12,
                      "cellsTotal": 244,
                      "cellsPerRack": 20,
                      "designedCycleLife": 6000,
                      "manufacturer": "Ottomotive",
                      "model": "Ottomotive BESS Container"
                    }
                    """;
                ottomotiveBessTemplate.HierarchyTemplate = """
                    {
                      "children": [
                        {
                          "key": "rack",
                          "deviceTypeName": "EnergyStorage.Rack",
                          "count": 12,
                          "namePattern": "Rack {index}"
                        },
                        {
                          "key": "pcs",
                          "deviceTypeName": "Pcs",
                          "count": 1,
                          "namePattern": "PCS_001"
                        },
                        {
                          "key": "hvac",
                          "deviceTypeName": "EnergyStorage.TMS",
                          "count": 1,
                          "namePattern": "HVAC_001"
                        }
                      ]
                    }
                    """;

                if (isNewTemplate)
                    _context.DeviceSpecificationTemplates.Add(ottomotiveBessTemplate);
                await _context.SaveChangesAsync();
                _logger.LogInformation("{Action} Ottomotive BESS Container device specification template",
                    isNewTemplate ? "Seeded" : "Updated");
            }
        }
        #endregion

        #region MarketOperators

        // Seed Market Operators (global entities - no tenant filter)
        if (!_context.MarketOperators.Any())
        {
            var marketOperators = new List<MarketOperator>
            {
                new MarketOperator
                {
                    Name = "ENTSO-E Transparency Platform",
                    Code = "ENTSOE",
                    BaseUrl = "https://web-api.tp.entsoe.eu/api",
                    ApiKey = "***REMOVED***",
                    SupportedResolutions = new List<int> { 15, 60 },
                    AreaCodes = new List<string>
                    {
                        "10YNL----------L", // Netherlands
                        "10YDE-VE-------2", // Germany
                        "10YFR-RTE------C", // France
                        "10YBE----------2", // Belgium
                        "10YAT-APG------L"  // Austria
                    },
                    IsActive = true,
                    Created = DateTime.UtcNow
                },
                new MarketOperator
                {
                    Name = "EPİAŞ (Turkish Energy Exchange)",
                    Code = "EPIAS",
                    BaseUrl = "https://seffaflik.epias.com.tr",
                    ApiKey = "technology@oklidis.com.tr:***REMOVED***",
                    SupportedResolutions = new List<int> { 60 },
                    AreaCodes = new List<string> { "TR" },
                    IsActive = true,
                    Created = DateTime.UtcNow
                }
            };
            _context.MarketOperators.AddRange(marketOperators);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Demo market operators created (ENTSO-E, EPIAS)");

            // Assign ENTSO-E to demo firm with Netherlands area code
            var entsoe = marketOperators.First(mo => mo.Code == "ENTSOE");
            var firmMarketOperator = new FirmMarketOperator
            {
                FirmId = firmId,
                MarketOperatorId = entsoe.Id,
                AreaCode = "10YNL----------L", // Netherlands
                SamplingPeriodMinutes = 60,
                IsActive = true,
                Created = DateTime.UtcNow
            };
            _context.FirmMarketOperators.Add(firmMarketOperator);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Demo firm assigned to ENTSO-E market operator (Netherlands area)");
        }

        #endregion

        #region DeviceAlarmTypes

        // Seed or replace DeviceAlarmTypes for Ottomotive BESS (global entity — no tenant filter)
        {
            var esTypeId = dtLookup.GetValueOrDefault(DeviceTypeNames.EnergyStorage);
            if (esTypeId != Guid.Empty)
            {
                var alarmTypes = BuildDemoBessAlarmTypes(esTypeId);
                var existingCodes = await _context.DeviceAlarmTypes
                    .Where(t => t.DeviceTypeId == esTypeId)
                    .Select(t => t.Code)
                    .ToListAsync();
                var newAlarmTypes = alarmTypes.Where(a => !existingCodes.Contains(a.Code)).ToList();
                if (newAlarmTypes.Count > 0)
                {
                    _context.DeviceAlarmTypes.AddRange(newAlarmTypes);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Seeded {Count} new DeviceAlarmTypes for Ottomotive BESS", newAlarmTypes.Count);
                }
            }
        }

        // Seed or replace DeviceAlarmTypes for Ottomotive PCS
        {
            var pcsTypeId = dtLookup.GetValueOrDefault(DeviceTypeNames.Pcs);
            if (pcsTypeId != Guid.Empty)
            {
                var pcsAlarmTypes = BuildDemoPcsAlarmTypes(pcsTypeId);
                var existingCodes = await _context.DeviceAlarmTypes
                    .Where(t => t.DeviceTypeId == pcsTypeId)
                    .Select(t => t.Code)
                    .ToListAsync();
                var newAlarmTypes = pcsAlarmTypes.Where(a => !existingCodes.Contains(a.Code)).ToList();
                if (newAlarmTypes.Count > 0)
                {
                    _context.DeviceAlarmTypes.AddRange(newAlarmTypes);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Seeded {Count} new DeviceAlarmTypes for Ottomotive PCS", newAlarmTypes.Count);
                }
            }
        }

        #endregion

        #region LogicalNodes and LogicalDevices

        if (!_context.LogicalNodes.Any())
        {
            var (logicalNodes, logicalDevices) = BuildLogicalDeviceCatalog();
            _context.LogicalNodes.AddRange(logicalNodes);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeded {Count} LogicalNodes", logicalNodes.Count);

            // Build lookup for node assignment
            var nodeLookup = logicalNodes.ToDictionary(n => n.Name, n => n.Id);

            foreach (var (ld, nodeSpecs) in logicalDevices)
            {
                foreach (var spec in nodeSpecs)
                {
                    if (nodeLookup.TryGetValue(spec.NodeName, out var nodeId))
                    {
                        ld.LogicalDeviceNodes.Add(new LogicalDeviceNode
                        {
                            LogicalNodeId = nodeId,
                            NodeAlias = spec.Alias,
                            InstanceCount = spec.InstanceCount,
                            SortOrder = spec.SortOrder
                        });
                    }
                }
            }

            _context.LogicalDevices.AddRange(logicalDevices.Select(x => x.Device));
            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeded {Count} LogicalDevices", logicalDevices.Count);
        }

        #endregion

        #region Tariffs

        // Seed default tariffs
        if (!_context.Tariffs.Any())
        {
            var tariffs = new List<Tariff>
            {
                new Tariff
                {
                    CountryName = "Netherlands",
                    DistributorName = "Liander",
                    DistributionFee = 25m,
                    TransmissionFee = 5m,
                    TaxRate = 0.21m,
                    Currency = "EUR",
                    IsActive = true,
                    Created = DateTime.UtcNow
                },
                new Tariff
                {
                    CountryName = "Germany",
                    DistributorName = "E.ON",
                    DistributionFee = 30m,
                    TransmissionFee = 6m,
                    TaxRate = 0.19m,
                    Currency = "EUR",
                    IsActive = true,
                    Created = DateTime.UtcNow
                },
                new Tariff
                {
                    CountryName = "Türkiye",
                    DistributorName = "TEDAŞ",
                    DistributionFee = 150m,
                    TransmissionFee = 30m,
                    TaxRate = 0.20m,
                    Currency = "TRY",
                    IsActive = true,
                    Created = DateTime.UtcNow
                }
            };
            _context.Tariffs.AddRange(tariffs);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Demo tariffs created");

            // Assign Netherlands tariff to the demo facility
            var nlTariff = tariffs.First(t => t.CountryName == "Netherlands");
            var facilityTariff = new FacilityTariff
            {
                FacilityId = facilitiyId,
                TariffId = nlTariff.Id,
                IsActive = true,
                Created = DateTime.UtcNow
            };
            _context.FacilityTariffs.Add(facilityTariff);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Demo facility tariff assignment created");
        }

        #endregion

        #region HuaweiPemVilla

        // Seed Huawei Pem Villa EMMA facility with virtual RTU + devices
        var existingHuaweiAccount = await _context.HuaweiAccounts.IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.UserName == "Pemapi");

        if (existingHuaweiAccount == null)
        {
            _logger.LogInformation("Seeding Huawei Pem Villa EMMA integration...");

            // Use the existing demo tenant for Pem Villa
            // Create Pem Villa firm
            var pemFirm = new Firm
            {
                TenantId = tenantId,
                Name = "Pem Villa Energy",
                Type = "Residential Energy",
                Country = "Turkey",
                State = "Antalya",
                City = "Antalya",
                District = "Konyaalti",
                PostalCode = "07070",
                Address = "Pem Villa, Antalya",
                ContactEmail = "info@example.org",
                ContactPhone = "+90 242 000 0000",
                TaxNumber = "N/A",
                TaxOffice = "N/A",
                Created = DateTime.UtcNow
            };
            _context.Firms.Add(pemFirm);
            await _context.SaveChangesAsync();

            // Create Pem Villa EMMA facility
            var pemFacility = new Facility
            {
                FirmId = pemFirm.Id,
                Name = "Pem Villa EMMA",
                Address = "Pem Villa, Antalya, Turkey",
                EnergyCapacity = 20,
                EnergyUnit = EnergyUnit.KWh,
                Longitude = 30.7133,
                Latitude = 36.8969,
                DataSource = FacilityDataSource.Huawei,
                Created = DateTime.UtcNow
            };
            _context.Facilities.Add(pemFacility);
            await _context.SaveChangesAsync();

            // Assign Pem Villa firm/facility to admin users
            var pemAdminUsers = await _userManager.GetUsersInRoleAsync(Roles.Administrator);
            foreach (var adminUser in pemAdminUsers)
            {
                var exists = await _context.UserFirmFacilities.IgnoreQueryFilters()
                    .AnyAsync(uff => uff.UserId == adminUser.Id && uff.FirmId == pemFirm.Id && uff.FacilityId == pemFacility.Id);

                if (!exists)
                {
                    _context.UserFirmFacilities.Add(new UserFirmFacility
                    {
                        TenantId = tenantId,
                        UserId = adminUser.Id,
                        FirmId = pemFirm.Id,
                        FacilityId = pemFacility.Id,
                        Created = DateTime.UtcNow
                    });
                }
            }
            await _context.SaveChangesAsync();

            // Create virtual RTU for Huawei station
            var pemRtu = new Rtu
            {
                FacilityId = pemFacility.Id,
                RtuSerialNumber = "HUAWEI-NE55911180",
                RtuModel = "Huawei FusionSolar Virtual RTU",
                Protocol = "HTTPS",
                Status = "online",
                Created = DateTime.UtcNow
            };
            _context.Rtus.Add(pemRtu);
            await _context.SaveChangesAsync();

            // Create Huawei specification templates with ParameterMapTemplate
            var huaweiBatteryTemplate = new DeviceSpecificationTemplate
            {
                DeviceTypeId = dtLookup[DeviceTypeNames.EnergyStorage],
                Model = "LUNA2000",
                Manufacturer = "Huawei",
                IsDefault = false,
                SpecificationsTemplate = """
                {
                  "supportedTabs": [
                    { "key": "summary", "label": "Summary", "isEnabled": true, "order": 0 },
                    { "key": "health", "label": "Health", "isEnabled": true, "order": 1 },
                    { "key": "curves", "label": "Curves", "isEnabled": true, "order": 2 },
                    { "key": "faults", "label": "Faults", "isEnabled": true, "order": 3 }
                  ],
                  "batteryDataKind": "Battery.Energy",
                  "curveCharts": [
                    {
                      "key": "powerSoc",
                      "title": "Power & State of Charge",
                      "type": "area",
                      "series": [
                        { "key": "power", "label": "Power", "sourcePath": "chDischargePower", "unit": "W", "color": "#8b5cf6", "yAxis": "left", "type": "area" },
                        { "key": "soc", "label": "SoC", "sourcePath": "soc", "unit": "%", "color": "#22c55e", "yAxis": "right", "type": "line" }
                      ]
                    },
                    {
                      "key": "voltageEnergy",
                      "title": "Voltage & Energy",
                      "type": "line",
                      "series": [
                        { "key": "voltage", "label": "Busbar Voltage", "sourcePath": "busbarVoltage", "unit": "V", "color": "#3b82f6", "yAxis": "left", "type": "line" },
                        { "key": "chargeEnergy", "label": "Charge Energy", "sourcePath": "chargeEnergy", "unit": "kWh", "color": "#22c55e", "yAxis": "right", "type": "line" }
                      ]
                    }
                  ]
                }
                """,
                MetadataTemplate = """{"cellChemistry": "LFP"}""",
                ParameterMapTemplate = """
                {
                  "version": 1,
                  "parameters": {
                    "soc": {
                      "type": "number", "unit": "%",
                      "source": { "kind": "Battery.Energy", "paths": ["soc"] }
                    },
                    "soh": {
                      "type": "number", "unit": "%",
                      "source": { "kind": "Battery.Energy", "paths": ["soh"] }
                    },
                    "power": {
                      "type": "number", "unit": "W",
                      "source": { "kind": "Battery.Energy", "paths": ["chDischargePower"] }
                    },
                    "powerDirection": {
                      "type": "string",
                      "formula": {
                        "expression": "cond(gt(p, 0), 'charging', cond(lt(p, 0), 'discharging', 'idle'))",
                        "inputs": { "p": {"$ref":"power"} }
                      }
                    },
                    "voltage": {
                      "type": "number", "unit": "V",
                      "source": { "kind": "Battery.Energy", "paths": ["busbarVoltage"] }
                    },
                    "availableEnergy": {
                      "type": "number", "unit": "kWh",
                      "source": { "kind": "Battery.Energy", "paths": ["ratedCapacity"] }
                    },
                    "operatingMode": {
                      "type": "string",
                      "formula": {
                        "expression": "cond(eq(s, 0), 'offline', cond(eq(s, 1), 'standby', cond(eq(s, 2), 'running', cond(eq(s, 3), 'fault', 'unknown'))))",
                        "inputs": { "s": {"source": {"kind": "Battery.Energy", "paths": ["batteryStatus"]}} }
                      }
                    },
                    "ambientTemp": {
                      "type": "number", "unit": "°C",
                      "source": { "kind": "Battery.Energy", "paths": ["batteryTemperature"] }
                    }
                  }
                }
                """,
                Created = DateTime.UtcNow
            };

            var huaweiPcsTemplate = new DeviceSpecificationTemplate
            {
                DeviceTypeId = dtLookup[DeviceTypeNames.Pcs],
                Model = "SUN2000-Inverter",
                Manufacturer = "Huawei",
                IsDefault = false,
                SpecificationsTemplate = """
                {
                  "supportedTabs": [
                    { "key": "general", "label": "General", "isEnabled": true, "order": 0 },
                    { "key": "curve", "label": "Curve", "isEnabled": true, "order": 1 },
                    { "key": "fault", "label": "Fault", "isEnabled": true, "order": 2 }
                  ]
                }
                """,
                MetadataTemplate = "{}",
                ParameterMapTemplate = """
                {
                  "version": 1,
                  "parameters": {
                    "pcsActivePower": {
                      "type": "number", "unit": "kW",
                      "source": { "kind": "Pcs.AcMeasurement", "paths": ["activePower"] }
                    },
                    "pcsReactivePower": {
                      "type": "number", "unit": "kVar",
                      "source": { "kind": "Pcs.AcMeasurement", "paths": ["reactivePower"] }
                    },
                    "pcsPowerFactor": {
                      "type": "number",
                      "source": { "kind": "Pcs.AcMeasurement", "paths": ["powerFactor"] }
                    },
                    "pcsGridFreq": {
                      "type": "number", "unit": "Hz",
                      "source": { "kind": "Pcs.AcMeasurement", "paths": ["gridFrequency"] }
                    },
                    "pcsIgbtTemp": {
                      "type": "number", "unit": "°C",
                      "source": { "kind": "Pcs.AcMeasurement", "paths": ["temperature"] }
                    },
                    "pcsEfficiency": {
                      "type": "number", "unit": "%",
                      "source": { "kind": "Pcs.AcMeasurement", "paths": ["efficiency"] }
                    },
                    "pcsPhaseVoltageA": {
                      "type": "number", "unit": "V",
                      "source": { "kind": "Pcs.AcMeasurement", "paths": ["phaseAVoltage"] }
                    },
                    "pcsPhaseVoltageB": {
                      "type": "number", "unit": "V",
                      "source": { "kind": "Pcs.AcMeasurement", "paths": ["phaseBVoltage"] }
                    },
                    "pcsPhaseVoltageC": {
                      "type": "number", "unit": "V",
                      "source": { "kind": "Pcs.AcMeasurement", "paths": ["phaseCVoltage"] }
                    },
                    "pcsPhaseCurrentA": {
                      "type": "number", "unit": "A",
                      "source": { "kind": "Pcs.AcMeasurement", "paths": ["phaseACurrent"] }
                    },
                    "pcsPhaseCurrentB": {
                      "type": "number", "unit": "A",
                      "source": { "kind": "Pcs.AcMeasurement", "paths": ["phaseBCurrent"] }
                    },
                    "pcsPhaseCurrentC": {
                      "type": "number", "unit": "A",
                      "source": { "kind": "Pcs.AcMeasurement", "paths": ["phaseCCurrent"] }
                    },
                    "pcsLineVoltageAB": {
                      "type": "number", "unit": "V",
                      "source": { "kind": "Pcs.AcMeasurement", "paths": ["lineABVoltage"] }
                    },
                    "pcsLineVoltageBC": {
                      "type": "number", "unit": "V",
                      "source": { "kind": "Pcs.AcMeasurement", "paths": ["lineBCVoltage"] }
                    },
                    "pcsLineVoltageCA": {
                      "type": "number", "unit": "V",
                      "source": { "kind": "Pcs.AcMeasurement", "paths": ["lineCAVoltage"] }
                    },
                    "pcsAmbientTemp": {
                      "type": "number", "unit": "°C",
                      "source": { "kind": "Pcs.AcMeasurement", "paths": ["temperature"] }
                    },
                    "pcsGridMode": {
                      "type": "string",
                      "source": { "kind": "Pcs.AcMeasurement", "paths": ["inverterState"], "default": "N/A" }
                    }
                  }
                }
                """,
                Created = DateTime.UtcNow
            };

            var huaweiSolarTemplate = new DeviceSpecificationTemplate
            {
                DeviceTypeId = dtLookup[DeviceTypeNames.Solar],
                Model = "SUN2000-Solar",
                Manufacturer = "Huawei",
                IsDefault = false,
                SpecificationsTemplate = "{}",
                MetadataTemplate = "{}",
                ParameterMapTemplate = """
                {
                  "version": 1,
                  "parameters": {
                    "activePower": {
                      "type": "number", "unit": "kW",
                      "source": { "kind": "Solar.Power", "paths": ["activePower"] }
                    },
                    "dailyYield": {
                      "type": "number", "unit": "kWh",
                      "source": { "kind": "Solar.Power", "paths": ["dailyYield"] }
                    },
                    "totalYield": {
                      "type": "number", "unit": "kWh",
                      "source": { "kind": "Solar.Power", "paths": ["totalYield"] }
                    },
                    "mpptPower": {
                      "type": "number", "unit": "kW",
                      "source": { "kind": "Solar.Power", "paths": ["mpptPower"] }
                    }
                  }
                }
                """,
                Created = DateTime.UtcNow
            };

            _context.DeviceSpecificationTemplates.AddRange(huaweiBatteryTemplate, huaweiPcsTemplate, huaweiSolarTemplate);
            await _context.SaveChangesAsync();

            // Create battery devices (3x LUNA2000)
            var battery12 = new Device
            {
                RtuId = pemRtu.Id,
                DeviceTypeId = dtLookup[DeviceTypeNames.EnergyStorage],
                DeviceSpecificationTemplateId = huaweiBatteryTemplate.Id,
                Name = "Battery-12",
                Model = "LUNA2000-10KW-C1",
                Manufacturer = "Huawei",
                Created = DateTime.UtcNow
            };
            var battery13 = new Device
            {
                RtuId = pemRtu.Id,
                DeviceTypeId = dtLookup[DeviceTypeNames.EnergyStorage],
                DeviceSpecificationTemplateId = huaweiBatteryTemplate.Id,
                Name = "Battery-13",
                Model = "LUNA2000-5KW-C0",
                Manufacturer = "Huawei",
                Created = DateTime.UtcNow
            };
            var battery14 = new Device
            {
                RtuId = pemRtu.Id,
                DeviceTypeId = dtLookup[DeviceTypeNames.EnergyStorage],
                DeviceSpecificationTemplateId = huaweiBatteryTemplate.Id,
                Name = "Battery-14",
                Model = "LUNA2000-5KW-C0",
                Manufacturer = "Huawei",
                Created = DateTime.UtcNow
            };
            _context.Devices.AddRange(battery12, battery13, battery14);
            await _context.SaveChangesAsync();

            // Create PCS devices (inverters as PCS, children of batteries)
            var pcs12 = new Device
            {
                RtuId = pemRtu.Id,
                DeviceTypeId = dtLookup[DeviceTypeNames.Pcs],
                DeviceSpecificationTemplateId = huaweiPcsTemplate.Id,
                ParentDeviceId = battery12.Id,
                Name = "Inverter-12-PCS",
                Model = "SUN2000-10K-MAP0",
                SerialNumber = "BT2460093355",
                Manufacturer = "Huawei",
                HierarchyIndex = 0,
                Created = DateTime.UtcNow
            };
            var pcs13 = new Device
            {
                RtuId = pemRtu.Id,
                DeviceTypeId = dtLookup[DeviceTypeNames.Pcs],
                DeviceSpecificationTemplateId = huaweiPcsTemplate.Id,
                ParentDeviceId = battery13.Id,
                Name = "Inverter-13-PCS",
                Model = "SUN2000-10K-MAP0",
                SerialNumber = "BT2511002783",
                Manufacturer = "Huawei",
                HierarchyIndex = 0,
                Created = DateTime.UtcNow
            };
            var pcs14 = new Device
            {
                RtuId = pemRtu.Id,
                DeviceTypeId = dtLookup[DeviceTypeNames.Pcs],
                DeviceSpecificationTemplateId = huaweiPcsTemplate.Id,
                ParentDeviceId = battery14.Id,
                Name = "Inverter-14-PCS",
                Model = "SUN2000-10K-MAP0",
                SerialNumber = "BT2511002884",
                Manufacturer = "Huawei",
                HierarchyIndex = 0,
                Created = DateTime.UtcNow
            };

            // Create Solar devices (inverters as solar producers)
            var solar12 = new Device
            {
                RtuId = pemRtu.Id,
                DeviceTypeId = dtLookup[DeviceTypeNames.Solar],
                DeviceSpecificationTemplateId = huaweiSolarTemplate.Id,
                Name = "Inverter-12-Solar",
                Model = "SUN2000-10K-MAP0",
                SerialNumber = "BT2460093355-SOL",
                Manufacturer = "Huawei",
                Created = DateTime.UtcNow
            };
            var solar13 = new Device
            {
                RtuId = pemRtu.Id,
                DeviceTypeId = dtLookup[DeviceTypeNames.Solar],
                DeviceSpecificationTemplateId = huaweiSolarTemplate.Id,
                Name = "Inverter-13-Solar",
                Model = "SUN2000-10K-MAP0",
                SerialNumber = "BT2511002783-SOL",
                Manufacturer = "Huawei",
                Created = DateTime.UtcNow
            };
            var solar14 = new Device
            {
                RtuId = pemRtu.Id,
                DeviceTypeId = dtLookup[DeviceTypeNames.Solar],
                DeviceSpecificationTemplateId = huaweiSolarTemplate.Id,
                Name = "Inverter-14-Solar",
                Model = "SUN2000-10K-MAP0",
                SerialNumber = "BT2511002884-SOL",
                Manufacturer = "Huawei",
                Created = DateTime.UtcNow
            };

            _context.Devices.AddRange(pcs12, pcs13, pcs14, solar12, solar13, solar14);
            await _context.SaveChangesAsync();

            // Create HuaweiAccount with encrypted credentials
            var encryptor = new HuaweiCredentialEncryptor("change-me-in-production-32-bytes!");
            var huaweiAccount = new HuaweiAccount
            {
                TenantId = tenantId,
                UserName = "Pemapi",
                EncryptedSystemCode = encryptor.Encrypt("Pem12345"),
                BaseUrl = "https://intl.fusionsolar.huawei.com/thirdData",
                IsActive = true,
                Created = DateTime.UtcNow
            };
            _context.HuaweiAccounts.Add(huaweiAccount);
            await _context.SaveChangesAsync();

            // Create station mapping
            var stationMapping = new HuaweiStationMapping
            {
                HuaweiAccountId = huaweiAccount.Id,
                StationCode = "NE=55911180",
                StationName = "Pem Villa EMMA",
                FacilityId = pemFacility.Id,
                SyncEnabled = true,
                Created = DateTime.UtcNow
            };
            _context.HuaweiStationMappings.Add(stationMapping);
            await _context.SaveChangesAsync();

            // Create device mappings
            // Each Huawei inverter (devTypeId=1) maps to TWO CNI devices (PCS + Solar)
            // Each Huawei battery (devTypeId=39) maps to ONE CNI device (EnergyStorage)
            var deviceMappings = new List<HuaweiDeviceMapping>
            {
                // Battery mappings (devTypeId=39)
                new HuaweiDeviceMapping
                {
                    StationMappingId = stationMapping.Id,
                    HuaweiDeviceId = 1000000055833018,
                    HuaweiDevTypeId = 39,
                    DeviceId = battery12.Id,
                    DeviceName = "Battery-1 (Inverter-12)",
                    Model = "LUNA2000-10KW-C1",
                    Created = DateTime.UtcNow
                },
                new HuaweiDeviceMapping
                {
                    StationMappingId = stationMapping.Id,
                    HuaweiDeviceId = 1000000060185748,
                    HuaweiDevTypeId = 39,
                    DeviceId = battery13.Id,
                    DeviceName = "Battery-1 (Inverter-13)",
                    Model = "LUNA2000-5KW-C0",
                    Created = DateTime.UtcNow
                },
                new HuaweiDeviceMapping
                {
                    StationMappingId = stationMapping.Id,
                    HuaweiDeviceId = 1000000060185750,
                    HuaweiDevTypeId = 39,
                    DeviceId = battery14.Id,
                    DeviceName = "Battery-1 (Inverter-14)",
                    Model = "LUNA2000-5KW-C0",
                    Created = DateTime.UtcNow
                },
                // Inverter → PCS mappings (devTypeId=1)
                new HuaweiDeviceMapping
                {
                    StationMappingId = stationMapping.Id,
                    HuaweiDeviceId = 1000000055833015,
                    HuaweiDevTypeId = 1,
                    DeviceId = pcs12.Id,
                    DeviceName = "Inverter-12 (PCS)",
                    SerialNumber = "BT2460093355",
                    Model = "SUN2000-10K-MAP0",
                    Created = DateTime.UtcNow
                },
                new HuaweiDeviceMapping
                {
                    StationMappingId = stationMapping.Id,
                    HuaweiDeviceId = 1000000060185746,
                    HuaweiDevTypeId = 1,
                    DeviceId = pcs13.Id,
                    DeviceName = "Inverter-13 (PCS)",
                    SerialNumber = "BT2511002783",
                    Model = "SUN2000-10K-MAP0",
                    Created = DateTime.UtcNow
                },
                new HuaweiDeviceMapping
                {
                    StationMappingId = stationMapping.Id,
                    HuaweiDeviceId = 1000000060185749,
                    HuaweiDevTypeId = 1,
                    DeviceId = pcs14.Id,
                    DeviceName = "Inverter-14 (PCS)",
                    SerialNumber = "BT2511002884",
                    Model = "SUN2000-10K-MAP0",
                    Created = DateTime.UtcNow
                },
                // Inverter → Solar mappings (devTypeId=1, same Huawei device, different CNI device)
                new HuaweiDeviceMapping
                {
                    StationMappingId = stationMapping.Id,
                    HuaweiDeviceId = 1000000055833015,
                    HuaweiDevTypeId = 1,
                    DeviceId = solar12.Id,
                    DeviceName = "Inverter-12 (Solar)",
                    SerialNumber = "BT2460093355",
                    Model = "SUN2000-10K-MAP0",
                    Created = DateTime.UtcNow
                },
                new HuaweiDeviceMapping
                {
                    StationMappingId = stationMapping.Id,
                    HuaweiDeviceId = 1000000060185746,
                    HuaweiDevTypeId = 1,
                    DeviceId = solar13.Id,
                    DeviceName = "Inverter-13 (Solar)",
                    SerialNumber = "BT2511002783",
                    Model = "SUN2000-10K-MAP0",
                    Created = DateTime.UtcNow
                },
                new HuaweiDeviceMapping
                {
                    StationMappingId = stationMapping.Id,
                    HuaweiDeviceId = 1000000060185749,
                    HuaweiDevTypeId = 1,
                    DeviceId = solar14.Id,
                    DeviceName = "Inverter-14 (Solar)",
                    SerialNumber = "BT2511002884",
                    Model = "SUN2000-10K-MAP0",
                    Created = DateTime.UtcNow
                }
            };

            _context.HuaweiDeviceMappings.AddRange(deviceMappings);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Huawei Pem Villa EMMA seeded: Account={AccountId}, Station={StationCode}, Facility={FacilityId}, {DeviceCount} device mappings",
                huaweiAccount.Id, stationMapping.StationCode, pemFacility.Id, deviceMappings.Count);
        }

        #endregion

        #region DemoTenantUser

        // Seed a standalone tenant user with a single facility (no devices)
        var demoUserEmail = "user@demo.com";
        if (_userManager.Users.All(u => u.UserName != demoUserEmail))
        {
            _logger.LogInformation("Seeding demo tenant user with standalone facility...");

            // Create a separate tenant
            var demoTenant = new Tenant
            {
                Identifier = "greenfield",
                Name = "Greenfield Solar",
                Created = DateTime.UtcNow
            };
            _context.Tenants.Add(demoTenant);
            await _context.SaveChangesAsync();

            // Assign Enterprise plan (same as demo tenant)
            var enterprisePlan = await _context.Set<Plan>().IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Name == "Enterprise");
            if (enterprisePlan != null)
            {
                demoTenant.PlanId = enterprisePlan.Id;
                await _context.SaveChangesAsync();
            }

            // Create firm
            var demoFirm = new Firm
            {
                TenantId = demoTenant.Id,
                Name = "Greenfield Renewables",
                Type = "Solar Energy",
                Country = "Türkiye",
                State = "Ankara",
                City = "Ankara",
                District = "Çankaya",
                PostalCode = "06690",
                Address = "Greenfield Solar Park, Çankaya",
                ContactEmail = "info@example.net",
                ContactPhone = "+90 312 000 0000",
                TaxNumber = "TR9876543210",
                TaxOffice = "Ankara",
                Created = DateTime.UtcNow
            };
            _context.Firms.Add(demoFirm);
            await _context.SaveChangesAsync();

            // Create facility
            var demoFacility = new Facility
            {
                FirmId = demoFirm.Id,
                Name = "Greenfield Solar Park",
                Country = "Türkiye",
                Province = "06",
                City = "Çankaya",
                Address = "Greenfield Solar Park, Çankaya, Ankara, Türkiye",
                EnergyCapacity = 500,
                EnergyUnit = EnergyUnit.KWh,
                Longitude = 32.8597,
                Latitude = 39.9334,
                Created = DateTime.UtcNow
            };
            _context.Facilities.Add(demoFacility);
            await _context.SaveChangesAsync();

            // Create TenantAdmin user
            var demoUser = new ApplicationUser
            {
                UserName = demoUserEmail,
                Email = demoUserEmail,
                EmailConfirmed = true,
                TenantId = demoTenant.Id,
                Name = "Demo",
                Surname = "User",
                Title = "Tenant Administrator",
                Description = "Demo tenant admin with a single facility"
            };

            var createResult = await _userManager.CreateAsync(demoUser, "Demo@123");
            if (createResult.Succeeded)
            {
                await _userManager.AddToRolesAsync(demoUser, new[] { Roles.TenantAdmin });

                _context.UserFirmFacilities.Add(new UserFirmFacility
                {
                    TenantId = demoTenant.Id,
                    UserId = demoUser.Id,
                    FirmId = demoFirm.Id,
                    FacilityId = demoFacility.Id,
                    Created = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                _logger.LogInformation("Demo tenant user created: {Email} / Demo@123 (TenantAdmin, Greenfield Solar)", demoUserEmail);
            }
            else
            {
                _logger.LogWarning("Failed to create demo tenant user: {Errors}",
                    string.Join(", ", createResult.Errors.Select(e => e.Description)));
            }
        }

        #endregion

        #region DemoAlarmsAndMaintenanceTasks

        // Seed demo alarms and maintenance tasks for "Demo Solar Park Amsterdam"
        if (!await _context.DeviceAlarms.IgnoreQueryFilters().AnyAsync(a => a.DeviceName == "BESS_001" || a.DeviceName == "PV_001"))
        {
            _logger.LogInformation("Seeding demo alarms and maintenance tasks for Demo Solar Park Amsterdam...");

            // Look up devices by name (they were created earlier in this method)
            var demoDevices = await _context.Devices.IgnoreQueryFilters()
                .Where(d => d.Rtu != null && d.Rtu.FacilityId == facilitiyId)
                .ToListAsync();
            var deviceByName = demoDevices
                .GroupBy(d => d.Name!)
                .ToDictionary(g => g.Key, g => g.First());

            // Look up alarm types
            var allAlarmTypes = await _context.DeviceAlarmTypes.ToListAsync();
            var alarmTypeByCode = allAlarmTypes
                .GroupBy(at => at.Code)
                .ToDictionary(g => g.Key, g => g.First());

            // Find admin user for assignments
            var adminUser = await _userManager.FindByEmailAsync("administrator@localhost");
            var adminUserId = adminUser?.Id ?? "";

            var now = DateTimeOffset.UtcNow;

            // ── Active Alarms (currently firing) ──

            var activeAlarms = new List<DeviceAlarm>();

            if (deviceByName.TryGetValue("BESS_001", out var bessDevice))
            {
                // Critical: Battery cell over-voltage fault — active, unacknowledged
                if (alarmTypeByCode.TryGetValue("BATT_CELL_OV_FAULT", out var cellOvFault))
                {
                    activeAlarms.Add(new DeviceAlarm
                    {
                        DeviceId = bessDevice.Id,
                        DeviceAlarmTypeId = cellOvFault.Id,
                        Path = "Rack/1/Pack/3",
                        OccurredAt = now.AddHours(-2),
                        IsActive = true,
                        IsRead = false,
                        IsProcessed = false,
                        Details = "{\"message\":\"Cell voltage reached 4.35V (threshold: 4.20V). Pack 3 in Rack 1 affected.\",\"value\":4.35,\"threshold\":4.20}",
                        RtuSerialNumber = "RTU-DEMO-001",
                        DeviceName = "BESS_001",
                        Created = now.AddHours(-2)
                    });
                }

                // Major: Battery over-temperature — active, acknowledged
                if (alarmTypeByCode.TryGetValue("BATT_OT_FAULT", out var otFault))
                {
                    activeAlarms.Add(new DeviceAlarm
                    {
                        DeviceId = bessDevice.Id,
                        DeviceAlarmTypeId = otFault.Id,
                        Path = "Rack/2",
                        OccurredAt = now.AddHours(-5),
                        IsActive = true,
                        IsRead = true,
                        IsProcessed = false,
                        Details = "{\"message\":\"Rack 2 temperature at 48°C (threshold: 45°C). Cooling system under investigation.\",\"value\":48,\"threshold\":45}",
                        RtuSerialNumber = "RTU-DEMO-001",
                        DeviceName = "BESS_001",
                        AcknowledgedById = adminUserId,
                        AcknowledgedAt = now.AddHours(-4),
                        Created = now.AddHours(-5)
                    });
                }

                // Warning: SOC low — active
                if (alarmTypeByCode.TryGetValue("BATT_SOC_LOW_FAULT", out var socLow))
                {
                    activeAlarms.Add(new DeviceAlarm
                    {
                        DeviceId = bessDevice.Id,
                        DeviceAlarmTypeId = socLow.Id,
                        OccurredAt = now.AddHours(-1),
                        IsActive = true,
                        IsRead = false,
                        IsProcessed = false,
                        Details = "{\"message\":\"State of charge dropped to 8%. Minimum threshold is 10%.\",\"value\":8,\"threshold\":10}",
                        RtuSerialNumber = "RTU-DEMO-001",
                        DeviceName = "BESS_001",
                        Created = now.AddHours(-1)
                    });
                }

                // Warning: Coolant shortage — active
                if (alarmTypeByCode.TryGetValue("TMS_COOLANT_SHORT", out var coolant))
                {
                    activeAlarms.Add(new DeviceAlarm
                    {
                        DeviceId = bessDevice.Id,
                        DeviceAlarmTypeId = coolant.Id,
                        OccurredAt = now.AddDays(-1),
                        IsActive = true,
                        IsRead = true,
                        IsProcessed = true,
                        Details = "{\"message\":\"Coolant level below minimum. Maintenance scheduled.\"}",
                        RtuSerialNumber = "RTU-DEMO-001",
                        DeviceName = "BESS_001",
                        AcknowledgedById = adminUserId,
                        AcknowledgedAt = now.AddDays(-1).AddHours(1),
                        Created = now.AddDays(-1)
                    });
                }

                // Critical: BMS communication failure — active
                if (alarmTypeByCode.TryGetValue("BATT_BMS_COMM", out var bmsComm))
                {
                    activeAlarms.Add(new DeviceAlarm
                    {
                        DeviceId = bessDevice.Id,
                        DeviceAlarmTypeId = bmsComm.Id,
                        Path = "Rack/3",
                        OccurredAt = now.AddMinutes(-30),
                        IsActive = true,
                        IsRead = false,
                        IsProcessed = false,
                        Details = "{\"message\":\"Lost communication with BMS on Rack 3. Last heartbeat 30 minutes ago.\"}",
                        RtuSerialNumber = "RTU-DEMO-001",
                        DeviceName = "BESS_001",
                        Created = now.AddMinutes(-30)
                    });
                }
            }

            // ── Recovered Alarms (historical, resolved) ──

            var recoveredAlarms = new List<DeviceAlarm>();

            if (deviceByName.TryGetValue("BESS_001", out var bessForRecovered))
            {
                if (alarmTypeByCode.TryGetValue("BATT_CELL_OV_WARN", out var cellOvWarn))
                {
                    recoveredAlarms.Add(new DeviceAlarm
                    {
                        DeviceId = bessForRecovered.Id,
                        DeviceAlarmTypeId = cellOvWarn.Id,
                        Path = "Rack/1/Pack/1",
                        OccurredAt = now.AddDays(-3),
                        RecoveredAt = now.AddDays(-3).AddHours(2),
                        IsActive = false,
                        IsRead = true,
                        IsProcessed = true,
                        Details = "{\"message\":\"Cell voltage warning on Pack 1, self-recovered after load balancing.\"}",
                        RtuSerialNumber = "RTU-DEMO-001",
                        DeviceName = "BESS_001",
                        AcknowledgedById = adminUserId,
                        AcknowledgedAt = now.AddDays(-3).AddMinutes(15),
                        ResolvedById = adminUserId,
                        ResolvedAt = now.AddDays(-3).AddHours(2),
                        Created = now.AddDays(-3)
                    });
                }

                if (alarmTypeByCode.TryGetValue("BATT_UT_FAULT", out var utFault))
                {
                    recoveredAlarms.Add(new DeviceAlarm
                    {
                        DeviceId = bessForRecovered.Id,
                        DeviceAlarmTypeId = utFault.Id,
                        OccurredAt = now.AddDays(-7),
                        RecoveredAt = now.AddDays(-7).AddHours(4),
                        IsActive = false,
                        IsRead = true,
                        IsProcessed = true,
                        Details = "{\"message\":\"Under-temperature fault during cold snap. Heaters activated and recovered.\"}",
                        RtuSerialNumber = "RTU-DEMO-001",
                        DeviceName = "BESS_001",
                        AcknowledgedById = adminUserId,
                        AcknowledgedAt = now.AddDays(-7).AddMinutes(10),
                        ResolvedById = adminUserId,
                        ResolvedAt = now.AddDays(-7).AddHours(4),
                        Created = now.AddDays(-7)
                    });
                }

                if (alarmTypeByCode.TryGetValue("TMS_PUMP_FAULT", out var pumpFault))
                {
                    recoveredAlarms.Add(new DeviceAlarm
                    {
                        DeviceId = bessForRecovered.Id,
                        DeviceAlarmTypeId = pumpFault.Id,
                        OccurredAt = now.AddDays(-14),
                        RecoveredAt = now.AddDays(-12),
                        IsActive = false,
                        IsRead = true,
                        IsProcessed = true,
                        Details = "{\"message\":\"Coolant pump failure. Replacement pump installed on-site.\"}",
                        RtuSerialNumber = "RTU-DEMO-001",
                        DeviceName = "BESS_001",
                        AcknowledgedById = adminUserId,
                        AcknowledgedAt = now.AddDays(-14).AddMinutes(5),
                        ResolvedById = adminUserId,
                        ResolvedAt = now.AddDays(-12),
                        Created = now.AddDays(-14)
                    });
                }

                if (alarmTypeByCode.TryGetValue("BATT_CONTACTOR", out var contactor))
                {
                    recoveredAlarms.Add(new DeviceAlarm
                    {
                        DeviceId = bessForRecovered.Id,
                        DeviceAlarmTypeId = contactor.Id,
                        Path = "Rack/2",
                        OccurredAt = now.AddDays(-5),
                        RecoveredAt = now.AddDays(-5).AddHours(1),
                        IsActive = false,
                        IsRead = true,
                        IsProcessed = true,
                        Details = "{\"message\":\"Contactor on Rack 2 failed to close. Manual reset resolved the issue.\"}",
                        RtuSerialNumber = "RTU-DEMO-001",
                        DeviceName = "BESS_001",
                        AcknowledgedById = adminUserId,
                        AcknowledgedAt = now.AddDays(-5).AddMinutes(20),
                        ResolvedById = adminUserId,
                        ResolvedAt = now.AddDays(-5).AddHours(1),
                        Created = now.AddDays(-5)
                    });
                }
            }

            // PCS/Inverter alarms
            if (deviceByName.TryGetValue("INVERTER_001", out var inverterDevice))
            {
                // Active: Grid over-voltage on phase A
                if (alarmTypeByCode.TryGetValue("PCS_GRID_OV_A", out var gridOvA))
                {
                    activeAlarms.Add(new DeviceAlarm
                    {
                        DeviceId = inverterDevice.Id,
                        DeviceAlarmTypeId = gridOvA.Id,
                        OccurredAt = now.AddHours(-3),
                        IsActive = true,
                        IsRead = true,
                        IsProcessed = false,
                        Details = "{\"message\":\"Grid voltage on Phase A at 253V (threshold: 250V). Inverter derated to 80%.\",\"value\":253,\"threshold\":250}",
                        RtuSerialNumber = "RTU-DEMO-001",
                        DeviceName = "INVERTER_001",
                        AcknowledgedById = adminUserId,
                        AcknowledgedAt = now.AddHours(-2),
                        Created = now.AddHours(-3)
                    });
                }

                // Recovered: Emergency stop
                if (alarmTypeByCode.TryGetValue("PCS_SYS_ESTOP", out var estop))
                {
                    recoveredAlarms.Add(new DeviceAlarm
                    {
                        DeviceId = inverterDevice.Id,
                        DeviceAlarmTypeId = estop.Id,
                        OccurredAt = now.AddDays(-10),
                        RecoveredAt = now.AddDays(-10).AddHours(6),
                        IsActive = false,
                        IsRead = true,
                        IsProcessed = true,
                        Details = "{\"message\":\"Emergency stop triggered during routine maintenance. System restarted after inspection.\"}",
                        RtuSerialNumber = "RTU-DEMO-001",
                        DeviceName = "INVERTER_001",
                        AcknowledgedById = adminUserId,
                        AcknowledgedAt = now.AddDays(-10).AddMinutes(5),
                        ResolvedById = adminUserId,
                        ResolvedAt = now.AddDays(-10).AddHours(6),
                        Created = now.AddDays(-10)
                    });
                }

                // Recovered: Grid frequency fault
                if (alarmTypeByCode.TryGetValue("PCS_GRID_FREQ", out var gridFreq))
                {
                    recoveredAlarms.Add(new DeviceAlarm
                    {
                        DeviceId = inverterDevice.Id,
                        DeviceAlarmTypeId = gridFreq.Id,
                        OccurredAt = now.AddDays(-2),
                        RecoveredAt = now.AddDays(-2).AddMinutes(45),
                        IsActive = false,
                        IsRead = true,
                        IsProcessed = true,
                        Details = "{\"message\":\"Grid frequency dropped to 49.2Hz. Inverter disconnected and reconnected after frequency stabilized.\",\"value\":49.2,\"threshold\":49.5}",
                        RtuSerialNumber = "RTU-DEMO-001",
                        DeviceName = "INVERTER_001",
                        AcknowledgedById = adminUserId,
                        AcknowledgedAt = now.AddDays(-2).AddMinutes(10),
                        ResolvedById = adminUserId,
                        ResolvedAt = now.AddDays(-2).AddMinutes(45),
                        Created = now.AddDays(-2)
                    });
                }
            }

            var allAlarms = activeAlarms.Concat(recoveredAlarms).ToList();
            _context.DeviceAlarms.AddRange(allAlarms);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeded {Count} demo alarms ({Active} active, {Recovered} recovered)",
                allAlarms.Count, activeAlarms.Count, recoveredAlarms.Count);

            // ── Maintenance Tasks ──

            var maintenanceTasks = new List<MaintenanceTask>();

            // 1. Critical — linked to the BMS communication alarm
            var bmsAlarm = activeAlarms.FirstOrDefault(a => a.DeviceName == "BESS_001"
                && a.Details != null && a.Details.Contains("BMS", StringComparison.OrdinalIgnoreCase));
            maintenanceTasks.Add(new MaintenanceTask
            {
                Title = "Investigate BMS communication failure on Rack 3",
                Description = "BMS on Rack 3 stopped responding. Check CAN bus wiring, BMS controller board, and communication module. Restart BMS if wiring is intact.",
                Status = TaskStatuses.InProgress,
                Priority = TaskPriorities.Critical,
                AssignedToId = adminUserId,
                AssignedById = adminUserId,
                DueDate = now.AddHours(4),
                FacilityId = facilitiyId,
                DeviceId = bessDevice?.Id,
                DeviceAlarmId = bmsAlarm?.Id,
                Created = now.AddMinutes(-25)
            });

            // 2. High — coolant system maintenance (linked to coolant alarm)
            var coolantAlarm = activeAlarms.FirstOrDefault(a => a.Details != null && a.Details.Contains("Coolant"));
            maintenanceTasks.Add(new MaintenanceTask
            {
                Title = "Refill coolant and inspect TMS loop",
                Description = "Coolant level is below minimum. Refill with approved glycol mixture. Inspect all hose connections, pump seals, and radiator for leaks.",
                Status = TaskStatuses.Scheduled,
                Priority = TaskPriorities.High,
                AssignedToId = adminUserId,
                AssignedById = adminUserId,
                DueDate = now.AddDays(1),
                FacilityId = facilitiyId,
                DeviceId = bessDevice?.Id,
                DeviceAlarmId = coolantAlarm?.Id,
                Created = now.AddDays(-1).AddHours(2)
            });

            // 3. Medium — routine inverter inspection
            maintenanceTasks.Add(new MaintenanceTask
            {
                Title = "Quarterly inverter inspection — SMA Sunny Tripower",
                Description = "Perform quarterly maintenance: check AC/DC connections torque, clean dust filters, verify fan operation, inspect surge protection devices, and review error logs.",
                Status = TaskStatuses.Pending,
                Priority = TaskPriorities.Medium,
                AssignedToId = adminUserId,
                AssignedById = adminUserId,
                DueDate = now.AddDays(7),
                FacilityId = facilitiyId,
                DeviceId = inverterDevice?.Id,
                Created = now.AddDays(-2)
            });

            // 4. Low — solar panel cleaning
            maintenanceTasks.Add(new MaintenanceTask
            {
                Title = "Solar panel array cleaning — PV_001",
                Description = "Scheduled cleaning of solar panel array. Use deionized water and soft brushes. Check for micro-cracks and hotspots with thermal camera.",
                Status = TaskStatuses.Scheduled,
                Priority = TaskPriorities.Low,
                AssignedToId = adminUserId,
                AssignedById = adminUserId,
                DueDate = now.AddDays(14),
                FacilityId = facilitiyId,
                DeviceId = deviceByName.GetValueOrDefault("PV_001")?.Id,
                Created = now.AddDays(-5)
            });

            // 5. Completed — pump replacement (linked to recovered pump alarm)
            var pumpAlarm = recoveredAlarms.FirstOrDefault(a => a.Details != null && a.Details.Contains("pump"));
            maintenanceTasks.Add(new MaintenanceTask
            {
                Title = "Replace failed coolant pump",
                Description = "Primary coolant pump seized. Replace with spare unit from warehouse.",
                Status = TaskStatuses.Completed,
                Priority = TaskPriorities.Critical,
                AssignedToId = adminUserId,
                AssignedById = adminUserId,
                DueDate = now.AddDays(-13),
                FacilityId = facilitiyId,
                DeviceId = bessDevice?.Id,
                DeviceAlarmId = pumpAlarm?.Id,
                Resolution = "Replaced pump with Grundfos MAGNA3 unit. System tested and running normally.",
                RootCause = "Bearing failure due to coolant contamination. Coolant filter was clogged.",
                LaborHours = 6.5m,
                PartsUsed = "Grundfos MAGNA3 40-120F (1x), coolant filter (1x), glycol coolant 20L",
                Recommendations = "Add coolant filter inspection to monthly checklist. Consider upgrading to stainless steel impeller pump.",
                CompletedAt = now.AddDays(-12),
                CompletedById = adminUserId,
                Created = now.AddDays(-14)
            });

            // 6. Closed — false alarm investigation
            maintenanceTasks.Add(new MaintenanceTask
            {
                Title = "Investigate false contactor alarm — Rack 2",
                Description = "Contactor alarm triggered but no physical fault found. Investigate sensor and wiring.",
                Status = TaskStatuses.Closed,
                Priority = TaskPriorities.Medium,
                AssignedToId = adminUserId,
                AssignedById = adminUserId,
                DueDate = now.AddDays(-4),
                FacilityId = facilitiyId,
                DeviceId = bessDevice?.Id,
                Resolution = "Loose connector on contactor feedback sensor. Re-crimped and tested.",
                RootCause = "Vibration from adjacent HVAC unit loosened the feedback connector over time.",
                LaborHours = 2.0m,
                PartsUsed = "Crimp connectors (2x)",
                CompletedAt = now.AddDays(-4).AddHours(3),
                CompletedById = adminUserId,
                CloseReason = "Resolved",
                CloseExplanation = "Root cause identified and fixed. Added vibration dampening pad between HVAC mount and cable tray.",
                ClosedAt = now.AddDays(-4).AddHours(4),
                ClosedById = adminUserId,
                Created = now.AddDays(-5)
            });

            // 7. Overdue — smart meter calibration
            maintenanceTasks.Add(new MaintenanceTask
            {
                Title = "Annual smart meter calibration — SM_001",
                Description = "Perform annual calibration of Schneider PM5560. Compare readings against reference meter and adjust CT ratios if needed.",
                Status = TaskStatuses.Overdue,
                Priority = TaskPriorities.Medium,
                AssignedToId = adminUserId,
                AssignedById = adminUserId,
                DueDate = now.AddDays(-3),
                FacilityId = facilitiyId,
                DeviceId = deviceByName.GetValueOrDefault("SM_001")?.Id,
                Created = now.AddDays(-10)
            });

            // 8. High — grid voltage investigation
            maintenanceTasks.Add(new MaintenanceTask
            {
                Title = "Investigate persistent grid over-voltage on Phase A",
                Description = "Grid voltage on Phase A has been consistently above 250V for the past 3 hours. Contact grid operator (Liander) if issue persists. Check transformer tap settings.",
                Status = TaskStatuses.InProgress,
                Priority = TaskPriorities.High,
                AssignedToId = adminUserId,
                AssignedById = adminUserId,
                DueDate = now.AddDays(1),
                FacilityId = facilitiyId,
                DeviceId = inverterDevice?.Id,
                Created = now.AddHours(-2)
            });

            _context.MaintenanceTasks.AddRange(maintenanceTasks);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeded {Count} demo maintenance tasks", maintenanceTasks.Count);

            // Link alarms to their maintenance tasks where applicable
            if (bmsAlarm != null) bmsAlarm.MaintenanceTaskId = maintenanceTasks[0].Id;
            if (coolantAlarm != null) coolantAlarm.MaintenanceTaskId = maintenanceTasks[1].Id;
            if (pumpAlarm != null) pumpAlarm.MaintenanceTaskId = maintenanceTasks[4].Id;
            await _context.SaveChangesAsync();

            // ── Notifications ──
            if (!await _context.Notifications.AnyAsync())
            {
                var notifications = new List<Notification>
                {
                    // Unread alarm notifications
                    new Notification
                    {
                        UserId = adminUserId,
                        Type = "alarm",
                        Title = "Critical: BMS Communication Failure",
                        Message = "Lost communication with BMS on Rack 3 of BESS_001. Immediate investigation required.",
                        IsRead = false,
                        Timestamp = now.AddMinutes(-30),
                        RelatedAlarmId = bmsAlarm?.Id
                    },
                    new Notification
                    {
                        UserId = adminUserId,
                        Type = "alarm",
                        Title = "Critical: Cell Over-Voltage Detected",
                        Message = "Cell voltage reached 4.35V on BESS_001 Rack/1/Pack/3. Threshold is 4.20V.",
                        IsRead = false,
                        Timestamp = now.AddHours(-2),
                        RelatedAlarmId = activeAlarms.FirstOrDefault(a => a.Details != null && a.Details.Contains("4.35"))?.Id
                    },
                    new Notification
                    {
                        UserId = adminUserId,
                        Type = "alarm",
                        Title = "Warning: SOC Below Minimum",
                        Message = "BESS_001 state of charge dropped to 8%. Minimum threshold is 10%.",
                        IsRead = false,
                        Timestamp = now.AddHours(-1)
                    },
                    // Read alarm notifications
                    new Notification
                    {
                        UserId = adminUserId,
                        Type = "alarm",
                        Title = "Over-Temperature on Rack 2",
                        Message = "Rack 2 temperature at 48°C (threshold: 45°C). Cooling system under investigation.",
                        IsRead = true,
                        Timestamp = now.AddHours(-5)
                    },
                    new Notification
                    {
                        UserId = adminUserId,
                        Type = "alarm",
                        Title = "Grid Over-Voltage Phase A",
                        Message = "Grid voltage on Phase A at 253V. Inverter derated to 80%.",
                        IsRead = true,
                        Timestamp = now.AddHours(-3)
                    },
                    // Maintenance task notifications
                    new Notification
                    {
                        UserId = adminUserId,
                        Type = "maintenance",
                        Title = "Task Assigned: BMS Communication Failure",
                        Message = "You have been assigned a critical maintenance task: Investigate BMS communication failure on Rack 3.",
                        IsRead = false,
                        Timestamp = now.AddMinutes(-25),
                        RelatedTaskId = maintenanceTasks[0].Id
                    },
                    new Notification
                    {
                        UserId = adminUserId,
                        Type = "maintenance",
                        Title = "Task Overdue: Smart Meter Calibration",
                        Message = "Annual smart meter calibration for SM_001 is 3 days overdue.",
                        IsRead = false,
                        Timestamp = now.AddDays(-3),
                        RelatedTaskId = maintenanceTasks[6].Id
                    },
                    new Notification
                    {
                        UserId = adminUserId,
                        Type = "maintenance",
                        Title = "Task Completed: Coolant Pump Replacement",
                        Message = "Coolant pump replacement on BESS_001 has been completed successfully.",
                        IsRead = true,
                        Timestamp = now.AddDays(-12),
                        RelatedTaskId = maintenanceTasks[4].Id
                    },
                    // System notifications
                    new Notification
                    {
                        UserId = adminUserId,
                        Type = "info",
                        Title = "Scheduled Maintenance Window",
                        Message = "Planned maintenance window for Demo Solar Park Amsterdam: Saturday 02:00-06:00 UTC.",
                        IsRead = true,
                        Timestamp = now.AddDays(-1)
                    },
                    new Notification
                    {
                        UserId = adminUserId,
                        Type = "info",
                        Title = "System Update Available",
                        Message = "DERMS platform v2.4.1 is available. Contact admin to schedule the update.",
                        IsRead = false,
                        Timestamp = now.AddDays(-2)
                    }
                };

                _context.Notifications.AddRange(notifications);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Seeded {Count} demo notifications", notifications.Count);
            }
        }

        #endregion
    }

    private static List<DeviceAlarmType> BuildDemoBessAlarmTypes(Guid energyStorageTypeId)
    {
        var now = DateTime.UtcNow;

        DeviceAlarmType T(string code, string faultKey, string kind, string name, string category, string severity, string source)
            => new()
            {
                DeviceTypeId = energyStorageTypeId,
                Model = "Ottomotive",
                Manufacturer = "Ottomotive",
                Code = code,
                FaultKey = faultKey,
                Kind = kind,
                Name = name,
                Category = category,
                Severity = severity,
                Source = source,
                IsEnabled = true,
                Created = now
            };

        return new List<DeviceAlarmType>
        {
            // Battery.FaultState — BMS faults (50 flags)
            T("BATT_PACK_WARN", "faultPackWarning", "Battery.FaultState", "Pack Warning", "battery", "Warning", "BMS"),
            T("BATT_PACK_DANGER", "faultPackDanger", "Battery.FaultState", "Pack Danger", "battery", "Critical", "BMS"),
            T("BATT_CELL_OV_WARN", "faultCellOverVoltageWarning", "Battery.FaultState", "Cell Over Voltage Warning", "battery", "Warning", "BMS"),
            T("BATT_CELL_OV_DANGER", "faultCellOverVoltageDanger", "Battery.FaultState", "Cell Over Voltage Danger", "battery", "Critical", "BMS"),
            T("BATT_CELL_UV_WARN", "faultCellUnderVoltageWarning", "Battery.FaultState", "Cell Under Voltage Warning", "battery", "Warning", "BMS"),
            T("BATT_CELL_UV_DANGER", "faultCellUnderVoltageDanger", "Battery.FaultState", "Cell Under Voltage Danger", "battery", "Critical", "BMS"),
            T("BATT_CELL_OT_WARN", "faultCellOverTempWarning", "Battery.FaultState", "Cell Over Temperature Warning", "battery", "Warning", "BMS"),
            T("BATT_CELL_OT_DANGER", "faultCellOverTempDanger", "Battery.FaultState", "Cell Over Temperature Danger", "battery", "Critical", "BMS"),
            T("BATT_CELL_UT_WARN", "faultCellUnderTempWarning", "Battery.FaultState", "Cell Under Temperature Warning", "battery", "Warning", "BMS"),
            T("BATT_CELL_UT_DANGER", "faultCellUnderTempDanger", "Battery.FaultState", "Cell Under Temperature Danger", "battery", "Critical", "BMS"),
            T("BATT_PACK_OC_WARN", "warningPackOC", "Battery.FaultState", "Pack Over Current Warning", "battery", "Warning", "BMS"),
            T("BATT_PACK_OC", "faultPackOC", "Battery.FaultState", "Pack Over Current Fault", "battery", "Critical", "BMS"),
            T("BATT_CURR_SENSOR", "faultPackCurrentSensor", "Battery.FaultState", "Pack Current Sensor Fault", "battery", "Critical", "BMS"),
            T("BATT_DISCHG_CONTACTOR", "faultPackDischargeContactor", "Battery.FaultState", "Pack Discharge Contactor Fault", "battery", "Critical", "BMS"),
            T("BATT_CHG_CONTACTOR", "faultPackChargeContactor", "Battery.FaultState", "Pack Charge Contactor Fault", "battery", "Critical", "BMS"),
            T("BATT_FUSE", "faultFuse", "Battery.FaultState", "Fuse Fault", "battery", "Critical", "BMS"),
            T("BATT_HVIL2", "faultPackHVIL2", "Battery.FaultState", "Pack HVIL2 Fault", "battery", "Critical", "BMS"),
            T("BATT_PACK_FUSE", "faultPackFuse", "Battery.FaultState", "Pack Fuse Fault", "battery", "Critical", "BMS"),
            T("BATT_FIRE_WARN", "faultPackFireWarningSensor", "Battery.FaultState", "Pack Fire Warning Sensor", "battery", "Critical", "BMS"),
            T("BATT_HIGH_HUMIDITY", "faultPackHighHumidity", "Battery.FaultState", "Pack High Humidity", "battery", "Major", "BMS"),
            T("BATT_BMU", "faultPackBMU", "Battery.FaultState", "Pack BMU Fault", "battery", "Critical", "BMS"),
            T("BATT_NEG_CONTACTOR", "faultPackNegContactor", "Battery.FaultState", "Pack Negative Contactor Fault", "battery", "Critical", "BMS"),
            T("BATT_PRECHARGE", "faultPackPrecharge", "Battery.FaultState", "Pack Precharge Fault", "battery", "Critical", "BMS"),
            T("BATT_ISOLATION", "faultIsolation", "Battery.FaultState", "Isolation Fault", "battery", "Critical", "BMS"),
            T("BATT_EMERGENCY", "faultPackEmergency", "Battery.FaultState", "Pack Emergency", "battery", "Critical", "BMS"),
            T("BATT_TBAL_WARN", "warningModuleTempBalance", "Battery.FaultState", "Module Temp Balance Warning", "battery", "Warning", "BMS"),
            T("BATT_TBAL_FAULT", "faultModuleTempBalance", "Battery.FaultState", "Module Temp Balance Fault", "battery", "Critical", "BMS"),
            T("BATT_DEAD_CELL", "faultDeadCell", "Battery.FaultState", "Dead Cell", "battery", "Critical", "BMS"),
            T("BATT_CAN_COMM", "faultCANCommunication", "Battery.FaultState", "CAN Communication Fault", "battery", "Major", "BMS"),
            T("BATT_INT_COMM", "faultPackIntCommunication", "Battery.FaultState", "Pack Internal Communication Fault", "battery", "Major", "BMS"),
            T("BATT_INT_MEAS", "faultInternalMeasurement", "Battery.FaultState", "Internal Measurement Fault", "battery", "Major", "BMS"),
            T("BATT_VOLT_REDUND", "faultVoltageRedundancy", "Battery.FaultState", "Voltage Redundancy Fault", "battery", "Major", "BMS"),
            T("BATT_OPEN_WIRE", "faultOpenWire", "Battery.FaultState", "Open Wire Fault", "battery", "Major", "BMS"),
            T("BATT_BRD_BMU_NORESP", "faultBoardBMUNoResponse", "Battery.FaultState", "Board BMU No Response", "battery", "Critical", "BMS"),
            T("BATT_BRD_STK_INIT", "faultBoardStackInitialize", "Battery.FaultState", "Board Stack Initialize Fault", "battery", "Critical", "BMS"),
            T("BATT_BRD_MEAS", "faultBoardMeasurement", "Battery.FaultState", "Board Measurement Fault", "battery", "Major", "BMS"),
            T("BATT_BRD_STK_CFG", "faultBoardStackConfiguration", "Battery.FaultState", "Board Stack Configuration Fault", "battery", "Major", "BMS"),
            T("BATT_BRD_BAL", "faultBoardBalance", "Battery.FaultState", "Board Balance Fault", "battery", "Major", "BMS"),
            T("BATT_BRD_OS", "faultBoardOperationSystem", "Battery.FaultState", "Board Operation System Fault", "battery", "Critical", "BMS"),
            T("BATT_BRD_GPI", "faultBoardGPI", "Battery.FaultState", "Board GPI Fault", "battery", "Major", "BMS"),
            T("BATT_BRD_CHIP", "faultBoardChip", "Battery.FaultState", "Board Chip Fault", "battery", "Critical", "BMS"),
            T("BATT_BRD_SYS", "faultBoardSys", "Battery.FaultState", "Board System Fault", "battery", "Critical", "BMS"),
            T("BATT_BRD_COMM", "faultBoardCommunication", "Battery.FaultState", "Board Communication Fault", "battery", "Major", "BMS"),
            T("BATT_BRD_COMP_OV", "faultBoardCompOverVoltage", "Battery.FaultState", "Board Comp Over Voltage Fault", "battery", "Major", "BMS"),
            T("BATT_BRD_COMP_UV", "faultBoardCompUnderVoltage", "Battery.FaultState", "Board Comp Under Voltage Fault", "battery", "Major", "BMS"),
            T("BATT_BRD_AUX_OV", "faultBoardAuxOverVoltage", "Battery.FaultState", "Board Aux Over Voltage Fault", "battery", "Major", "BMS"),
            T("BATT_BRD_AUX_UV", "faultBoardAuxUnderVoltage", "Battery.FaultState", "Board Aux Under Voltage Fault", "battery", "Major", "BMS"),
            T("BATT_BRD_OV", "faultBoardOverVoltage", "Battery.FaultState", "Board Over Voltage Fault", "battery", "Major", "BMS"),
            T("BATT_BRD_UV", "faultBoardUnderVoltage", "Battery.FaultState", "Board Under Voltage Fault", "battery", "Major", "BMS"),
            T("BATT_BRD_STK_ERR", "faultBoardStackError", "Battery.FaultState", "Board Stack Error", "battery", "Critical", "BMS"),
            T("BATT_BRD_OW_DET", "faultBoardOpenWireDetect", "Battery.FaultState", "Board Open Wire Detect", "battery", "Major", "BMS"),

            // Battery.ThermalAlarm — Chiller/TMS alarms (34 flags)
            T("TMS_O_COOL_PRESS", "oICoolantPressSensorFault", "Battery.ThermalAlarm", "Outlet Coolant Pressure Sensor Fault", "chiller", "Warning", "TMS"),
            T("TMS_PWR_OV", "powerSupplyOV", "Battery.ThermalAlarm", "Power Supply Over Voltage", "chiller", "Warning", "TMS"),
            T("TMS_PWR_UV", "powerSupplyUV", "Battery.ThermalAlarm", "Power Supply Under Voltage", "chiller", "Warning", "TMS"),
            T("TMS_COOLANT_SHORT", "systemCoolantShortage", "Battery.ThermalAlarm", "System Coolant Shortage", "chiller", "Major", "TMS"),
            T("TMS_PUMP_FAULT", "pumpFaultSummary", "Battery.ThermalAlarm", "Pump Fault Summary", "chiller", "Major", "TMS"),
            T("TMS_PUMP_LOCK", "pumpFaultLockSummary", "Battery.ThermalAlarm", "Pump Fault Lock Summary", "chiller", "Critical", "TMS"),
            T("TMS_O_COOL_PRESS_F", "oCoolantPressSensorFault", "Battery.ThermalAlarm", "Outlet Coolant Pressure Sensor Fault", "chiller", "Warning", "TMS"),
            T("TMS_I_COOL_PRESS", "iCoolantPressureSensorFault", "Battery.ThermalAlarm", "Inlet Coolant Pressure Sensor Fault", "chiller", "Warning", "TMS"),
            T("TMS_LP_SENSOR", "lowPressureSensorFault", "Battery.ThermalAlarm", "Low Pressure Sensor Fault", "chiller", "Warning", "TMS"),
            T("TMS_O_COOL_TEMP", "oCoolantTempSensorFault", "Battery.ThermalAlarm", "Outlet Coolant Temp Sensor Fault", "chiller", "Warning", "TMS"),
            T("TMS_SYS_LP_ALARM", "systemLowPressureAlarm", "Battery.ThermalAlarm", "System Low Pressure Alarm", "chiller", "Warning", "TMS"),
            T("TMS_SYS_HP_ALARM", "systemHighPressureAlarm", "Battery.ThermalAlarm", "System High Pressure Alarm", "chiller", "Warning", "TMS"),
            T("TMS_DISCHG_TEMP_HI", "dischargeTempTooHighAlarm", "Battery.ThermalAlarm", "Discharge Temp Too High Alarm", "chiller", "Warning", "TMS"),
            T("TMS_CAN_COMM", "canCommunicationFaultAlarm", "Battery.ThermalAlarm", "CAN Communication Fault Alarm", "chiller", "Major", "TMS"),
            T("TMS_SUCTION_TEMP", "suctionTempSensorFault", "Battery.ThermalAlarm", "Suction Temp Sensor Fault", "chiller", "Warning", "TMS"),
            T("TMS_COOL_SYS_ABN", "coolingSystemAbnormal", "Battery.ThermalAlarm", "Cooling System Abnormal", "chiller", "Major", "TMS"),
            T("TMS_LOW_SUCT_SH", "lowSuctionSuperheatDegreeA", "Battery.ThermalAlarm", "Low Suction Superheat Degree", "chiller", "Warning", "TMS"),
            T("TMS_VFD_OV", "compressorVfdOvAlarm", "Battery.ThermalAlarm", "Compressor VFD Over Voltage", "chiller", "Major", "TMS"),
            T("TMS_VFD_UV", "compressorVfdUvAlarm", "Battery.ThermalAlarm", "Compressor VFD Under Voltage", "chiller", "Major", "TMS"),
            T("TMS_VFD_OC", "compressorVfdOcAlarm", "Battery.ThermalAlarm", "Compressor VFD Over Current", "chiller", "Major", "TMS"),
            T("TMS_VFD_OT", "compressorVfdOtAlarm", "Battery.ThermalAlarm", "Compressor VFD Over Temp", "chiller", "Major", "TMS"),
            T("TMS_VFD_COMM", "compressorVfdComFaultA", "Battery.ThermalAlarm", "Compressor VFD Comm Fault", "chiller", "Major", "TMS"),
            T("TMS_VFD_PH_LOSS", "compressorVfdPhaseLossA", "Battery.ThermalAlarm", "Compressor VFD Phase Loss", "chiller", "Major", "TMS"),
            T("TMS_VFD_OTHER", "otherCompVfdFaultAlarm", "Battery.ThermalAlarm", "Other Compressor VFD Fault", "chiller", "Major", "TMS"),
            T("TMS_PUMP_DIFF_P", "pumpLowDiffPressAlarm", "Battery.ThermalAlarm", "Pump Low Differential Pressure Alarm", "chiller", "Major", "TMS"),
            T("TMS_DISCHG_TEMP_LOCK", "dischargeTempTooHighLock", "Battery.ThermalAlarm", "Discharge Temp Too High Lock", "chiller", "Critical", "TMS"),
            T("TMS_SYS_HP_LOCK", "systemHighPressLock", "Battery.ThermalAlarm", "System High Pressure Lock", "chiller", "Critical", "TMS"),
            T("TMS_SYS_LP_LOCK", "systemLowPressLock", "Battery.ThermalAlarm", "System Low Pressure Lock", "chiller", "Critical", "TMS"),
            T("TMS_VFD_OC_LOCK", "compressorVfdOcLock", "Battery.ThermalAlarm", "Compressor VFD Over Current Lock", "chiller", "Critical", "TMS"),
            T("TMS_VFD_OV_LOCK", "compressorVfdOvLock", "Battery.ThermalAlarm", "Compressor VFD Over Voltage Lock", "chiller", "Critical", "TMS"),
            T("TMS_VFD_UV_LOCK", "compressorVfdUvLock", "Battery.ThermalAlarm", "Compressor VFD Under Voltage Lock", "chiller", "Critical", "TMS"),
            T("TMS_VFD_OT_LOCK", "compressorVfdOtLock", "Battery.ThermalAlarm", "Compressor VFD Over Temp Lock", "chiller", "Critical", "TMS"),
            T("TMS_VFD_PH_LOCK", "compressorVfdPhsLossLock", "Battery.ThermalAlarm", "Compressor VFD Phase Loss Lock", "chiller", "Critical", "TMS"),
            T("TMS_VFD_OTHER_LOCK", "otherCompVfdFaultLocks", "Battery.ThermalAlarm", "Other Compressor VFD Fault Lock", "chiller", "Critical", "TMS"),
        };
    }

    private static List<DeviceAlarmType> BuildDemoPcsAlarmTypes(Guid pcsTypeId)
    {
        var now = DateTime.UtcNow;

        DeviceAlarmType T(string code, string faultKey, string kind, string name, string category, string severity)
            => new()
            {
                DeviceTypeId = pcsTypeId,
                Model = "Ottomotive",
                Manufacturer = "Ottomotive",
                Code = code,
                FaultKey = faultKey,
                Kind = kind,
                Name = name,
                Category = category,
                Severity = severity,
                Source = "PCS",
                IsEnabled = true,
                Created = now
            };

        return new List<DeviceAlarmType>
        {
            // Pcs.FaultGroup1 — Hardware faults
            T("PCS_HW_EPO", "pcs_hwEpoFault", "Pcs.FaultGroup1", "Hardware EPO Fault", "pcs_hardware", "Critical"),
            T("PCS_HW_IGBT_OCP", "pcs_hwIgbtOcpFault", "Pcs.FaultGroup1", "IGBT Over Current Protection Fault", "pcs_hardware", "Critical"),
            T("PCS_HW_BUS_OV", "pcs_hwBusOvFault", "Pcs.FaultGroup1", "Bus Over Voltage Fault", "pcs_hardware", "Major"),
            T("PCS_HW_WAVE_LIM", "pcs_hwWaveLimitFault", "Pcs.FaultGroup1", "Wave Limit Fault", "pcs_hardware", "Major"),
            T("PCS_HW_BAL_WAVE", "pcs_hwBalWaveLimitFault", "Pcs.FaultGroup1", "Balance Wave Limit Fault", "pcs_hardware", "Major"),
            T("PCS_HW_24V_PWR", "pcs_hw24vPwrFault", "Pcs.FaultGroup1", "24V Power Supply Fault", "pcs_hardware", "Major"),
            T("PCS_HW_FAN", "pcs_hwFanFault", "Pcs.FaultGroup1", "Fan Fault", "pcs_hardware", "Major"),
            T("PCS_HW_CONN", "pcs_hwConnFault", "Pcs.FaultGroup1", "Connection Fault", "pcs_hardware", "Major"),
            T("PCS_HW_EXT_DI3", "pcs_hwExtDi3Fault", "Pcs.FaultGroup1", "External DI3 Fault", "pcs_hardware", "Major"),
            T("PCS_HW_IND_OT", "pcs_hwIndOvertFault", "Pcs.FaultGroup1", "Inductance Over Temperature Fault", "pcs_hardware", "Major"),
            T("PCS_HW_MOD_OT", "pcs_hwPwrModOvert", "Pcs.FaultGroup1", "Power Module Over Temperature", "pcs_hardware", "Major"),
            T("PCS_HW_BAL_OT", "pcs_hwBalOvert", "Pcs.FaultGroup1", "Balance Over Temperature", "pcs_hardware", "Major"),
            T("PCS_HW_15V_PWR", "pcs_hw15vPwrFault", "Pcs.FaultGroup1", "15V Power Supply Fault", "pcs_hardware", "Major"),
            T("PCS_HW_FIRE", "pcs_hwFireAlarmFault", "Pcs.FaultGroup1", "Fire Alarm Fault", "pcs_hardware", "Critical"),
            T("PCS_HW_EXT_DI1", "pcs_hwExtDi1Fault", "Pcs.FaultGroup1", "External DI1 Fault", "pcs_hardware", "Major"),
            T("PCS_HW_EXT_DI2", "pcs_hwExtDi2Fault", "Pcs.FaultGroup1", "External DI2 Fault", "pcs_hardware", "Major"),

            // Pcs.FaultGroup1 — Grid faults
            T("PCS_GRID_A_OV", "pcs_gridAOvFault", "Pcs.FaultGroup1", "Grid Phase A Over Voltage", "pcs_grid", "Major"),
            T("PCS_GRID_B_OV", "pcs_gridBOvFault", "Pcs.FaultGroup1", "Grid Phase B Over Voltage", "pcs_grid", "Major"),
            T("PCS_GRID_C_OV", "pcs_gridCOvFault", "Pcs.FaultGroup1", "Grid Phase C Over Voltage", "pcs_grid", "Major"),
            T("PCS_GRID_A_UV", "pcs_gridAUvFault", "Pcs.FaultGroup1", "Grid Phase A Under Voltage", "pcs_grid", "Major"),
            T("PCS_GRID_B_UV", "pcs_gridBUvFault", "Pcs.FaultGroup1", "Grid Phase B Under Voltage", "pcs_grid", "Major"),
            T("PCS_GRID_C_UV", "pcs_gridCUvFault", "Pcs.FaultGroup1", "Grid Phase C Under Voltage", "pcs_grid", "Major"),
            T("PCS_GRID_OVERF", "pcs_gridOverfFault", "Pcs.FaultGroup1", "Grid Over Frequency", "pcs_grid", "Major"),
            T("PCS_GRID_UNDERF", "pcs_gridUnderfFault", "Pcs.FaultGroup1", "Grid Under Frequency", "pcs_grid", "Major"),
            T("PCS_GRID_PHSEQ", "pcs_gridPhSeqFault", "Pcs.FaultGroup1", "Grid Phase Sequence Fault", "pcs_grid", "Major"),
            T("PCS_GRID_A_OC", "pcs_gridAOcFault", "Pcs.FaultGroup1", "Grid Phase A Over Current", "pcs_grid", "Major"),
            T("PCS_GRID_B_OC", "pcs_gridBOcFault", "Pcs.FaultGroup1", "Grid Phase B Over Current", "pcs_grid", "Major"),
            T("PCS_GRID_C_OC", "pcs_gridCOcFault", "Pcs.FaultGroup1", "Grid Phase C Over Current", "pcs_grid", "Major"),
            T("PCS_GRID_V_UNBAL", "pcs_gridVUnbalFault", "Pcs.FaultGroup1", "Grid Voltage Unbalance", "pcs_grid", "Major"),
            T("PCS_GRID_I_UNBAL", "pcs_gridIUnbalFault", "Pcs.FaultGroup1", "Grid Current Unbalance", "pcs_grid", "Major"),
            T("PCS_GRID_PH_LOSS", "pcs_gridPhLossFault", "Pcs.FaultGroup1", "Grid Phase Loss", "pcs_grid", "Major"),
            T("PCS_GRID_N_OC", "pcs_gridNOcFault", "Pcs.FaultGroup1", "Grid Neutral Over Current", "pcs_grid", "Major"),

            // Pcs.FaultGroup1 — Bus/Battery faults
            T("PCS_BUS_PRE_OV", "pcs_busPreOvFault", "Pcs.FaultGroup1", "Bus Pre-charge Over Voltage", "pcs_bus", "Critical"),
            T("PCS_BUS_PRE_UV", "pcs_busPreUvFault", "Pcs.FaultGroup1", "Bus Pre-charge Under Voltage", "pcs_bus", "Major"),
            T("PCS_BUS_RUN_OV", "pcs_busRunOvFault", "Pcs.FaultGroup1", "Bus Running Over Voltage", "pcs_bus", "Critical"),
            T("PCS_BUS_RUN_UV", "pcs_busRunUvFault", "Pcs.FaultGroup1", "Bus Running Under Voltage", "pcs_bus", "Major"),
            T("PCS_BUS_UNBAL", "pcs_busUnbalFault", "Pcs.FaultGroup1", "Bus Unbalance Fault", "pcs_bus", "Major"),
            T("PCS_BAT_UV", "pcs_batUvFault", "Pcs.FaultGroup1", "Battery Under Voltage", "pcs_bus", "Major"),
            T("PCS_BAT_OV", "pcs_batOvFault", "Pcs.FaultGroup1", "Battery Over Voltage", "pcs_bus", "Critical"),
            T("PCS_DC_PRE_OC", "pcs_dcPreOcFault", "Pcs.FaultGroup1", "DC Pre-charge Over Current", "pcs_bus", "Critical"),
            T("PCS_DC_OC", "pcs_dcOcFault", "Pcs.FaultGroup1", "DC Over Current", "pcs_bus", "Critical"),
            T("PCS_BAL_SW_OC", "pcs_balSwOcFault", "Pcs.FaultGroup1", "Balance Switch Over Current", "pcs_bus", "Critical"),
            T("PCS_BAT_REV", "pcs_batRevFault", "Pcs.FaultGroup1", "Battery Reverse Fault", "pcs_bus", "Critical"),

            // Pcs.FaultGroup1 — AC faults
            T("PCS_AC_PRE_TO", "pcs_acPreToFault", "Pcs.FaultGroup1", "AC Pre-charge Timeout", "pcs_ac", "Major"),
            T("PCS_AC_PRE_A_OC", "pcs_acPreAOcFault", "Pcs.FaultGroup1", "AC Pre-charge Phase A Over Current", "pcs_ac", "Major"),
            T("PCS_AC_PRE_B_OC", "pcs_acPreBOcFault", "Pcs.FaultGroup1", "AC Pre-charge Phase B Over Current", "pcs_ac", "Major"),
            T("PCS_AC_PRE_C_OC", "pcs_acPreCOcFault", "Pcs.FaultGroup1", "AC Pre-charge Phase C Over Current", "pcs_ac", "Major"),
            T("PCS_AC_LEAK_OC", "pcs_acLeakOcFault", "Pcs.FaultGroup1", "AC Leakage Over Current", "pcs_ac", "Critical"),
            T("PCS_AC_DC_COMP", "pcs_acDcCompFault", "Pcs.FaultGroup1", "AC DC Component Fault", "pcs_ac", "Critical"),
            T("PCS_AC_PWR_MIS", "pcs_acPwrMisFault", "Pcs.FaultGroup1", "AC Power Mismatch", "pcs_ac", "Critical"),

            // Pcs.FaultGroup1 — System faults
            T("PCS_SYS_RAM", "pcs_sysRamFault", "Pcs.FaultGroup1", "System RAM Fault", "pcs_system", "Major"),
            T("PCS_SYS_EEPROM", "pcs_sysEepromFault", "Pcs.FaultGroup1", "System EEPROM Fault", "pcs_system", "Major"),
            T("PCS_SYS_AD_DRIFT", "pcs_sysAdDriftFault", "Pcs.FaultGroup1", "System AD Drift Fault", "pcs_system", "Major"),
            T("PCS_SYS_BACK_COM", "pcs_sysBackComFault", "Pcs.FaultGroup1", "System Backend Communication Fault", "pcs_system", "Major"),
            T("PCS_SYS_CAN_PROT", "pcs_sysCanProtFault", "Pcs.FaultGroup1", "System CAN Protocol Fault", "pcs_system", "Major"),
            T("PCS_SYS_CPLD_COM", "pcs_sysCpldComFault", "Pcs.FaultGroup1", "System CPLD Communication Fault", "pcs_system", "Major"),
            T("PCS_SYS_DATALOG", "pcs_sysDatalogFault", "Pcs.FaultGroup1", "System Datalog Fault", "pcs_system", "Warning"),
            T("PCS_SYS_INSUL", "pcs_sysInsulFault", "Pcs.FaultGroup1", "System Insulation Fault", "pcs_system", "Major"),
            T("PCS_SYS_FW_MIS", "pcs_sysFwMisFault", "Pcs.FaultGroup1", "System Firmware Mismatch", "pcs_system", "Warning"),
            T("PCS_SYS_BMS_STAT", "pcs_sysBmsStatFault", "Pcs.FaultGroup1", "System BMS Status Fault", "pcs_system", "Major"),
            T("PCS_SYS_STS_COM", "pcs_sysStsComFault", "Pcs.FaultGroup1", "System STS Communication Fault", "pcs_system", "Major"),
            T("PCS_SYS_BMS_COM", "pcs_sysBmsComFault", "Pcs.FaultGroup1", "System BMS Communication Fault", "pcs_system", "Major"),
            T("PCS_SYS_PAR_CAN", "pcs_sysParCanFault", "Pcs.FaultGroup1", "System Parallel CAN Fault", "pcs_system", "Major"),
            T("PCS_SYS_EMS_COM", "pcs_sysEmsComFault", "Pcs.FaultGroup1", "System EMS Communication Fault", "pcs_system", "Major"),

            // Pcs.FaultGroup2 — Relay faults
            T("PCS_PREREL_CLOSE", "pcs_preRelCloseFail", "Pcs.FaultGroup2", "Pre-charge Relay Close Failure", "pcs_relay", "Critical"),
            T("PCS_PREREL_OPEN", "pcs_preRelOpenFail", "Pcs.FaultGroup2", "Pre-charge Relay Open Failure", "pcs_relay", "Critical"),
            T("PCS_PREREL_ST_CLOSE", "pcs_preRelStClose", "Pcs.FaultGroup2", "Pre-charge Relay Status Close", "pcs_relay", "Major"),
            T("PCS_PREREL_ST_OPEN", "pcs_preRelStOpen", "Pcs.FaultGroup2", "Pre-charge Relay Status Open", "pcs_relay", "Major"),
            T("PCS_MAINREL_CLOSE", "pcs_mainRelCloseFail", "Pcs.FaultGroup2", "Main Relay Close Failure", "pcs_relay", "Critical"),
            T("PCS_MAINREL_OPEN", "pcs_mainRelOpenFail", "Pcs.FaultGroup2", "Main Relay Open Failure", "pcs_relay", "Critical"),
            T("PCS_MAINREL_ST_CLOSE", "pcs_mainRelStClose", "Pcs.FaultGroup2", "Main Relay Status Close", "pcs_relay", "Major"),
            T("PCS_MAINREL_NUM", "pcs_mainRelNum", "Pcs.FaultGroup2", "Main Relay Number Fault", "pcs_relay", "Major"),
            T("PCS_AC_MAIN_ADH", "pcs_acMainAdhFault", "Pcs.FaultGroup2", "AC Main Adhesion Fault", "pcs_relay", "Critical"),
            T("PCS_DC_REL_OPEN", "pcs_dcRelOpenFault", "Pcs.FaultGroup2", "DC Relay Open Fault", "pcs_relay", "Critical"),
            T("PCS_AC_MAIN_OPEN", "pcs_acMainOpenFault", "Pcs.FaultGroup2", "AC Main Open Fault", "pcs_relay", "Critical"),

            // Pcs.FaultGroup2 — Inverter faults
            T("PCS_INV_A_OV", "pcs_invAOvFault", "Pcs.FaultGroup2", "Inverter Phase A Over Voltage", "pcs_inverter", "Major"),
            T("PCS_INV_B_OV", "pcs_invBOvFault", "Pcs.FaultGroup2", "Inverter Phase B Over Voltage", "pcs_inverter", "Major"),
            T("PCS_INV_C_OV", "pcs_invCOvFault", "Pcs.FaultGroup2", "Inverter Phase C Over Voltage", "pcs_inverter", "Major"),
            T("PCS_GRID_ISLAND", "pcs_gridIslandFault", "Pcs.FaultGroup2", "Grid Island Fault", "pcs_inverter", "Critical"),
            T("PCS_SYS_RESON", "pcs_sysResonFault", "Pcs.FaultGroup2", "System Resonance Fault", "pcs_inverter", "Critical"),
            T("PCS_SW_OV_OC", "pcs_swOvOcFault", "Pcs.FaultGroup2", "Software OV/OC Fault", "pcs_inverter", "Critical"),
            T("PCS_HV_CROSS_TO", "pcs_hvCrossToFault", "Pcs.FaultGroup2", "HV Cross Timeout Fault", "pcs_inverter", "Major"),
            T("PCS_INV_A_UV", "pcs_invAUvFault", "Pcs.FaultGroup2", "Inverter Phase A Under Voltage", "pcs_inverter", "Major"),
            T("PCS_INV_B_UV", "pcs_invBUvFault", "Pcs.FaultGroup2", "Inverter Phase B Under Voltage", "pcs_inverter", "Major"),
            T("PCS_INV_C_UV", "pcs_invCUvFault", "Pcs.FaultGroup2", "Inverter Phase C Under Voltage", "pcs_inverter", "Major"),
            T("PCS_OFF_NO_SYNC", "pcs_offNoSyncFault", "Pcs.FaultGroup2", "Offline No Sync Fault", "pcs_inverter", "Major"),
            T("PCS_OFF_SHORT", "pcs_offShortFault", "Pcs.FaultGroup2", "Offline Short Fault", "pcs_inverter", "Critical"),
            T("PCS_LV_CROSS_TO", "pcs_lvCrossToFault", "Pcs.FaultGroup2", "LV Cross Timeout Fault", "pcs_inverter", "Major"),

            // Pcs.FaultGroup2 — Timeout faults
            T("PCS_MSG1_TO", "pcs_msg1Timeout", "Pcs.FaultGroup2", "Message 1 Timeout", "pcs_timeout", "Major"),
            T("PCS_MSG2_TO", "pcs_msg2Timeout", "Pcs.FaultGroup2", "Message 2 Timeout", "pcs_timeout", "Major"),
            T("PCS_PWR_TO", "pcs_powerTimeout", "Pcs.FaultGroup2", "Power Timeout", "pcs_timeout", "Major"),
            T("PCS_MOD_OPMODE_TO", "pcs_moduleOpModeTout", "Pcs.FaultGroup2", "Module Op Mode Timeout", "pcs_timeout", "Warning"),
            T("PCS_SET_ONGRID_TO", "pcs_setOngridModeTout", "Pcs.FaultGroup2", "Set Ongrid Mode Timeout", "pcs_timeout", "Warning"),
            T("PCS_SET_OFFGRID_F_TO", "pcs_setOffgridFreqTout", "Pcs.FaultGroup2", "Set Offgrid Frequency Timeout", "pcs_timeout", "Warning"),
            T("PCS_SET_OFFGRID_V_TO", "pcs_setOffgridVoltTout", "Pcs.FaultGroup2", "Set Offgrid Voltage Timeout", "pcs_timeout", "Warning"),
            T("PCS_SET_PCS_TO", "pcs_setPcsTout", "Pcs.FaultGroup2", "Set PCS Timeout", "pcs_timeout", "Warning"),
            T("PCS_SET_PWR_INDEP_TO", "pcs_setPwrIndepTout", "Pcs.FaultGroup2", "Set Power Independent Timeout", "pcs_timeout", "Warning"),
            T("PCS_SET_MOD_HOST_TO", "pcs_setModHostTout", "Pcs.FaultGroup2", "Set Module Host Timeout", "pcs_timeout", "Warning"),
            T("PCS_MOD_CURR_SRC_TO", "pcs_modCurrSrcParTout", "Pcs.FaultGroup2", "Module Current Source Par Timeout", "pcs_timeout", "Warning"),
            T("PCS_SET_DEHUMID_TO", "pcs_setDehumidTout", "Pcs.FaultGroup2", "Set Dehumidifier Timeout", "pcs_timeout", "Warning"),
            T("PCS_SET_NUM_PAR_TO", "pcs_setNumParMdiTout", "Pcs.FaultGroup2", "Set Number Parallel MDI Timeout", "pcs_timeout", "Warning"),
            T("PCS_SET_APH_ACT_TO", "pcs_setAPhActPwrTout", "Pcs.FaultGroup2", "Set A Phase Active Power Timeout", "pcs_timeout", "Warning"),
            T("PCS_SET_APH_REA_TO", "pcs_setAPhReaPwrTout", "Pcs.FaultGroup2", "Set A Phase Reactive Power Timeout", "pcs_timeout", "Warning"),
            T("PCS_SET_BPH_ACT_TO", "pcs_setBPhActPwrTout", "Pcs.FaultGroup2", "Set B Phase Active Power Timeout", "pcs_timeout", "Warning"),
            T("PCS_SET_BPH_REA_TO", "pcs_setBPhReaPwrTout", "Pcs.FaultGroup2", "Set B Phase Reactive Power Timeout", "pcs_timeout", "Warning"),
            T("PCS_SET_CPH_ACT_TO", "pcs_setCPhActPwrTout", "Pcs.FaultGroup2", "Set C Phase Active Power Timeout", "pcs_timeout", "Warning"),
            T("PCS_SET_CPH_REA_TO", "pcs_setCPhReaPwrTout", "Pcs.FaultGroup2", "Set C Phase Reactive Power Timeout", "pcs_timeout", "Warning"),
            T("PCS_YEAR_TO", "pcs_yearTout", "Pcs.FaultGroup2", "Year Setting Timeout", "pcs_timeout", "Warning"),
            T("PCS_MONTH_TO", "pcs_monthTout", "Pcs.FaultGroup2", "Month Setting Timeout", "pcs_timeout", "Warning"),
            T("PCS_DAY_TO", "pcs_dayTout", "Pcs.FaultGroup2", "Day Setting Timeout", "pcs_timeout", "Warning"),
            T("PCS_HOUR_TO", "pcs_hourTout", "Pcs.FaultGroup2", "Hour Setting Timeout", "pcs_timeout", "Warning"),
            T("PCS_MINUTE_TO", "pcs_minuteTout", "Pcs.FaultGroup2", "Minute Setting Timeout", "pcs_timeout", "Warning"),
            T("PCS_SECOND_TO", "pcs_secondTout", "Pcs.FaultGroup2", "Second Setting Timeout", "pcs_timeout", "Warning"),
            T("PCS_SET_MAX_CHG_P_TO", "pcs_setMaxChgPwrTout", "Pcs.FaultGroup2", "Set Max Charge Power Timeout", "pcs_timeout", "Warning"),
            T("PCS_SET_MAX_DCH_P_TO", "pcs_setMaxDchPwrTout", "Pcs.FaultGroup2", "Set Max Discharge Power Timeout", "pcs_timeout", "Warning"),
            T("PCS_SET_MAX_CHG_I_TO", "pcs_setMaxChgCurrTout", "Pcs.FaultGroup2", "Set Max Charge Current Timeout", "pcs_timeout", "Warning"),
            T("PCS_SET_MAX_DCH_I_TO", "pcs_setMaxDchCurrTout", "Pcs.FaultGroup2", "Set Max Discharge Current Timeout", "pcs_timeout", "Warning"),
            T("PCS_SET_DC_BUS_V_TO", "pcs_setDcBusVTout", "Pcs.FaultGroup2", "Set DC Bus Voltage Timeout", "pcs_timeout", "Warning"),
            T("PCS_SET_DC_SRC_V_TO", "pcs_setDcSrcBusVTout", "Pcs.FaultGroup2", "Set DC Source Bus Voltage Timeout", "pcs_timeout", "Warning"),
            T("PCS_SET_INSUL_EN_TO", "pcs_setInsulEnTout", "Pcs.FaultGroup2", "Set Insulation Enable Timeout", "pcs_timeout", "Warning"),
            T("PCS_GRID_OVERF_PROT_TO", "pcs_gridOverfProtTout", "Pcs.FaultGroup2", "Grid Over Frequency Protection Timeout", "pcs_timeout", "Major"),
            T("PCS_GRID_UNDERF_PROT_TO", "pcs_gridUnderfProtTout", "Pcs.FaultGroup2", "Grid Under Frequency Protection Timeout", "pcs_timeout", "Major"),
            T("PCS_ISL_OVERF_PROT_TO", "pcs_islOverfProtTout", "Pcs.FaultGroup2", "Island Over Frequency Protection Timeout", "pcs_timeout", "Major"),
            T("PCS_ISL_UNDERF_PROT_TO", "pcs_islUnderfProtTout", "Pcs.FaultGroup2", "Island Under Frequency Protection Timeout", "pcs_timeout", "Major"),
            T("PCS_BAT_OV_PROT_TO", "pcs_batOvProtTout", "Pcs.FaultGroup2", "Battery OV Protection Timeout", "pcs_timeout", "Major"),
            T("PCS_BAT_UV_PROT_TO", "pcs_batUvProtTout", "Pcs.FaultGroup2", "Battery UV Protection Timeout", "pcs_timeout", "Major"),
            T("PCS_BUS_OV_PROT_TO", "pcs_busOvProtTout", "Pcs.FaultGroup2", "Bus OV Protection Timeout", "pcs_timeout", "Major"),
            T("PCS_BUS_UV_PROT_TO", "pcs_busUvProtTout", "Pcs.FaultGroup2", "Bus UV Protection Timeout", "pcs_timeout", "Major"),

            // Pcs.Timeout — Protection & command timeouts
            T("PCS_DC_OC_PROT_TO", "pcs_dcOcProtTout", "Pcs.Timeout", "DC Over Current Protection Timeout", "pcs_timeout", "Critical"),
            T("PCS_AC_OV_PROT_TO", "pcs_acOvProtTout", "Pcs.Timeout", "AC Over Voltage Protection Timeout", "pcs_timeout", "Critical"),
            T("PCS_AC_UV_PROT_TO", "pcs_acUvProtTout", "Pcs.Timeout", "AC Under Voltage Protection Timeout", "pcs_timeout", "Critical"),
            T("PCS_AC_OC_PROT_TO", "pcs_acOcProtTout", "Pcs.Timeout", "AC Over Current Protection Timeout", "pcs_timeout", "Critical"),
            T("PCS_MOD_OT_PROT_TO", "pcs_modOvertProtTout", "Pcs.Timeout", "Module Over Temperature Protection Timeout", "pcs_timeout", "Critical"),
            T("PCS_CCCV_RECHG_V_TO", "pcs_ccCvRechgVTout", "Pcs.Timeout", "CC/CV Recharge Voltage Timeout", "pcs_timeout", "Major"),
            T("PCS_BAT_CV_CHG_V_TO", "pcs_batCvChgVTout", "Pcs.Timeout", "Battery CV Charge Voltage Timeout", "pcs_timeout", "Major"),
            T("PCS_CV_CHG_MAX_T_TO", "pcs_cvChgMaxTimeTout", "Pcs.Timeout", "CV Charge Max Time Timeout", "pcs_timeout", "Major"),
            T("PCS_CV_FLOAT_CURR_TO", "pcs_cvToFloatCurrTout", "Pcs.Timeout", "CV to Float Current Timeout", "pcs_timeout", "Major"),
            T("PCS_FLOAT_CHG_V_TO", "pcs_floatChgVTout", "Pcs.Timeout", "Float Charge Voltage Timeout", "pcs_timeout", "Major"),
            T("PCS_FLOAT_CC_CURR_TO", "pcs_floatToCcCurrTout", "Pcs.Timeout", "Float to CC Current Timeout", "pcs_timeout", "Major"),
            T("PCS_PWR_ON_OFF_TO", "pcs_pwrOnOffCmdTout", "Pcs.Timeout", "Power On/Off Command Timeout", "pcs_timeout", "Major"),
            T("PCS_FAULT_RST_TO", "pcs_faultResetCmdTout", "Pcs.Timeout", "Fault Reset Command Timeout", "pcs_timeout", "Major"),
            T("PCS_CURE_PARAM_TO", "pcs_cureParamCmdTout", "Pcs.Timeout", "Cure Parameter Command Timeout", "pcs_timeout", "Major"),
            T("PCS_CHG_RST_TO", "pcs_chgResetCmdTout", "Pcs.Timeout", "Charge Reset Command Timeout", "pcs_timeout", "Major"),
            T("PCS_SAFETY_KEY_TO", "pcs_safetyOneKeyTout", "Pcs.Timeout", "Safety One Key Timeout", "pcs_timeout", "Major"),
            T("PCS_RE_MB_ADDR_TO", "pcs_reMbAddrTout", "Pcs.Timeout", "Remote Modbus Address Timeout", "pcs_timeout", "Warning"),
            T("PCS_RE_MB_BAUD_TO", "pcs_reMbBaudTout", "Pcs.Timeout", "Remote Modbus Baud Timeout", "pcs_timeout", "Warning"),
            T("PCS_HMI_MB_ADDR_TO", "pcs_hmiMbAddrTout", "Pcs.Timeout", "HMI Modbus Address Timeout", "pcs_timeout", "Warning"),
            T("PCS_HMI_MB_BAUD_TO", "pcs_hmiMbBaudTout", "Pcs.Timeout", "HMI Modbus Baud Timeout", "pcs_timeout", "Warning"),
            T("PCS_EMS_COMM_FAIL_TO", "pcs_emsCommFailTout", "Pcs.Timeout", "EMS Communication Fail Timeout", "pcs_timeout", "Warning"),
            T("PCS_PC_CMD_TIME_TO", "pcs_setPcsPcCmdTimeTout", "Pcs.Timeout", "Set PCS PC Command Time Timeout", "pcs_timeout", "Warning"),
            T("PCS_FAULT", "pcs_fault", "Pcs.Timeout", "PCS General Fault", "pcs_timeout", "Critical"),
            T("PCS_RS485_FAULT", "pcs_rs485Fault", "Pcs.Timeout", "RS485 Communication Fault", "pcs_timeout", "Warning"),
            T("PCS_VAR_PWR_TO", "pcs_varPowerTimeout", "Pcs.Timeout", "Variable Power Timeout", "pcs_timeout", "Major"),
            T("PCS_MSG1_2_TO", "pcs_msg1_2Timeout", "Pcs.Timeout", "Message 1/2 Timeout", "pcs_timeout", "Major"),
        };
    }

    private record NodeSpec(string NodeName, int SortOrder, int InstanceCount = 1, string? Alias = null);

    private static (List<LogicalNode> Nodes, List<(LogicalDevice Device, NodeSpec[] Nodes)> Devices) BuildLogicalDeviceCatalog()
    {
        var now = DateTime.UtcNow;

        // Helper to build parameter JSON
        static string P(params (string key, string label, string unit, string dataType, string sourceType)[] parameters)
        {
            var items = parameters.Select(p => $@"{{""key"":""{p.key}"",""label"":""{p.label}"",""unit"":""{p.unit}"",""dataType"":""{p.dataType}"",""sourceType"":""{p.sourceType}""}}");
            return "[" + string.Join(",", items) + "]";
        }

        // ── LogicalNodes ──
        var nodes = new List<LogicalNode>
        {
            // BESS Nodes
            new()
            {
                Name = LogicalNodeNames.BatteryNameplateData,
                DisplayName = "Battery Nameplate Data",
                Description = "Static registration data for a battery container",
                Parameters = P(
                    ("ratedCapacityAh", "Rated Capacity", "Ah", "float", "Registration"),
                    ("ratedCapacityKwh", "Rated Energy Capacity", "kWh", "float", "Registration"),
                    ("ratedVoltage", "Rated Voltage", "V", "float", "Registration"),
                    ("maxChargeCurrent", "Max Charge Current", "A", "float", "Registration"),
                    ("maxDischargeCurrent", "Max Discharge Current", "A", "float", "Registration"),
                    ("nominalVoltage", "Nominal Voltage", "V", "float", "Registration"),
                    ("chemistry", "Chemistry Type", "", "string", "Registration"),
                    ("cycleLifeRating", "Cycle Life Rating", "cycles", "int", "Registration"),
                    ("calendarLifeYears", "Calendar Life", "years", "int", "Registration"),
                    ("maxChargeVoltage", "Max Charge Voltage", "V", "float", "Registration")
                ),
                Created = now
            },
            new()
            {
                Name = LogicalNodeNames.BatteryStatusData,
                DisplayName = "Battery Status Data",
                Description = "Real-time operational status of a battery container",
                Parameters = P(
                    ("operatingState", "Operating State", "", "string", "Realtime"),
                    ("contactorState", "Contactor State", "", "string", "Realtime"),
                    ("alarmState", "Alarm State", "", "string", "Realtime"),
                    ("warningState", "Warning State", "", "string", "Realtime"),
                    ("protectionState", "Protection State", "", "string", "Realtime"),
                    ("communicationState", "Communication State", "", "string", "Realtime")
                ),
                Created = now
            },
            new()
            {
                Name = LogicalNodeNames.BatteryEnergyData,
                DisplayName = "Battery Energy Data",
                Description = "Real-time energy measurements for a battery container",
                Parameters = P(
                    ("soc", "State of Charge", "%", "float", "Realtime"),
                    ("soh", "State of Health", "%", "float", "Realtime"),
                    ("voltage", "Voltage", "V", "float", "Realtime"),
                    ("current", "Current", "A", "float", "Realtime"),
                    ("power", "Power", "kW", "float", "Realtime"),
                    ("powerDirection", "Power Direction", "", "string", "Realtime"),
                    ("temperature", "Temperature", "°C", "float", "Realtime"),
                    ("maxCellVoltage", "Max Cell Voltage", "V", "float", "Realtime"),
                    ("minCellVoltage", "Min Cell Voltage", "V", "float", "Realtime"),
                    ("maxCellTemp", "Max Cell Temperature", "°C", "float", "Realtime"),
                    ("minCellTemp", "Min Cell Temperature", "°C", "float", "Realtime"),
                    ("chargedEnergy", "Charged Energy", "kWh", "float", "Realtime")
                ),
                Created = now
            },
            new()
            {
                Name = LogicalNodeNames.BatteryHealthData,
                DisplayName = "Battery Health Data",
                Description = "Health and degradation metrics for a battery container",
                Parameters = P(
                    ("soh", "State of Health", "%", "float", "Realtime"),
                    ("cycleCount", "Cycle Count", "", "int", "Realtime"),
                    ("capacityFade", "Capacity Fade", "%", "float", "Calculated"),
                    ("resistanceIncrease", "Resistance Increase", "%", "float", "Calculated"),
                    ("avgCellVoltageDeviation", "Avg Cell Voltage Deviation", "mV", "float", "Calculated"),
                    ("avgCellTempDeviation", "Avg Cell Temp Deviation", "°C", "float", "Calculated"),
                    ("energyThroughput", "Energy Throughput", "MWh", "float", "Calculated"),
                    ("estimatedRemainingLife", "Estimated Remaining Life", "%", "float", "Calculated"),
                    ("lastMaintenanceDate", "Last Maintenance Date", "", "datetime", "Installation"),
                    ("nextMaintenanceDate", "Next Maintenance Date", "", "datetime", "Calculated")
                ),
                Created = now
            },
            new()
            {
                Name = LogicalNodeNames.BatteryPackEnergyData,
                DisplayName = "Battery Pack Energy Data",
                Description = "Real-time measurements for individual battery racks",
                Parameters = P(
                    ("soc", "State of Charge", "%", "float", "Realtime"),
                    ("voltage", "Voltage", "V", "float", "Realtime"),
                    ("current", "Current", "A", "float", "Realtime"),
                    ("power", "Power", "kW", "float", "Realtime"),
                    ("maxCellVoltage", "Max Cell Voltage", "V", "float", "Realtime"),
                    ("minCellVoltage", "Min Cell Voltage", "V", "float", "Realtime"),
                    ("maxCellTemp", "Max Cell Temperature", "°C", "float", "Realtime"),
                    ("minCellTemp", "Min Cell Temperature", "°C", "float", "Realtime")
                ),
                Created = now
            },
            new()
            {
                Name = LogicalNodeNames.BatteryModuleEnergyData,
                DisplayName = "Battery Module Energy Data",
                Description = "Real-time measurements for individual battery packs/modules",
                Parameters = P(
                    ("voltage", "Voltage", "V", "float", "Realtime"),
                    ("current", "Current", "A", "float", "Realtime"),
                    ("temperature", "Temperature", "°C", "float", "Realtime"),
                    ("maxCellVoltage", "Max Cell Voltage", "V", "float", "Realtime"),
                    ("minCellVoltage", "Min Cell Voltage", "V", "float", "Realtime"),
                    ("cellVoltageSpread", "Cell Voltage Spread", "mV", "float", "Calculated")
                ),
                Created = now
            },
            new()
            {
                Name = LogicalNodeNames.HvacStatusData,
                DisplayName = "HVAC Status Data",
                Description = "Real-time HVAC/TMS status for container thermal management",
                Parameters = P(
                    ("operatingMode", "Operating Mode", "", "string", "Realtime"),
                    ("supplyAirTemp", "Supply Air Temperature", "°C", "float", "Realtime"),
                    ("returnAirTemp", "Return Air Temperature", "°C", "float", "Realtime"),
                    ("setpointTemp", "Setpoint Temperature", "°C", "float", "Realtime"),
                    ("compressorState", "Compressor State", "", "string", "Realtime"),
                    ("fanSpeed", "Fan Speed", "RPM", "int", "Realtime"),
                    ("powerConsumption", "Power Consumption", "kW", "float", "Realtime"),
                    ("alarmState", "Alarm State", "", "string", "Realtime")
                ),
                Created = now
            },

            // PCS Nodes
            new()
            {
                Name = LogicalNodeNames.PcsStatusData,
                DisplayName = "PCS Status Data",
                Description = "Real-time status of a power conversion system",
                Parameters = P(
                    ("operatingState", "Operating State", "", "string", "Realtime"),
                    ("operatingMode", "Operating Mode", "", "string", "Realtime"),
                    ("alarmState", "Alarm State", "", "string", "Realtime"),
                    ("warningState", "Warning State", "", "string", "Realtime"),
                    ("gridContactorState", "Grid Contactor State", "", "string", "Realtime"),
                    ("dcContactorState", "DC Contactor State", "", "string", "Realtime")
                ),
                Created = now
            },
            new()
            {
                Name = LogicalNodeNames.PcsAcMeasurementData,
                DisplayName = "PCS AC Measurement Data",
                Description = "AC-side electrical measurements of a PCS",
                Parameters = P(
                    ("acVoltageA", "AC Voltage Phase A", "V", "float", "Realtime"),
                    ("acVoltageB", "AC Voltage Phase B", "V", "float", "Realtime"),
                    ("acVoltageC", "AC Voltage Phase C", "V", "float", "Realtime"),
                    ("acCurrentA", "AC Current Phase A", "A", "float", "Realtime"),
                    ("acCurrentB", "AC Current Phase B", "A", "float", "Realtime"),
                    ("acCurrentC", "AC Current Phase C", "A", "float", "Realtime"),
                    ("acActivePower", "AC Active Power", "kW", "float", "Realtime"),
                    ("acFrequency", "AC Frequency", "Hz", "float", "Realtime")
                ),
                Created = now
            },
            new()
            {
                Name = LogicalNodeNames.PcsDcMeasurementData,
                DisplayName = "PCS DC Measurement Data",
                Description = "DC-side electrical measurements of a PCS",
                Parameters = P(
                    ("dcVoltage", "DC Voltage", "V", "float", "Realtime"),
                    ("dcCurrent", "DC Current", "A", "float", "Realtime"),
                    ("dcPower", "DC Power", "kW", "float", "Realtime"),
                    ("dcBusVoltage", "DC Bus Voltage", "V", "float", "Realtime"),
                    ("insulationResistance", "Insulation Resistance", "MΩ", "float", "Realtime"),
                    ("igbtTemperature", "IGBT Temperature", "°C", "float", "Realtime")
                ),
                Created = now
            },
            new()
            {
                Name = LogicalNodeNames.PcsChargeAlgorithmData,
                DisplayName = "PCS Charge Algorithm Data",
                Description = "Charge/discharge algorithm parameters of a PCS",
                Parameters = P(
                    ("chargeMode", "Charge Mode", "", "string", "Realtime"),
                    ("targetPower", "Target Power", "kW", "float", "Realtime"),
                    ("currentLimit", "Current Limit", "A", "float", "Realtime"),
                    ("voltageLimit", "Voltage Limit", "V", "float", "Realtime"),
                    ("powerFactor", "Power Factor", "", "float", "Realtime"),
                    ("reactivePower", "Reactive Power", "kVAR", "float", "Realtime")
                ),
                Created = now
            },

            // Solar Nodes
            new()
            {
                Name = LogicalNodeNames.SolarStatusData,
                DisplayName = "Solar Status Data",
                Description = "Real-time status and output of a solar inverter/string",
                Parameters = P(
                    ("operatingState", "Operating State", "", "string", "Realtime"),
                    ("totalActivePower", "Total Active Power", "kW", "float", "Realtime"),
                    ("totalEnergy", "Total Energy", "kWh", "float", "Realtime"),
                    ("dailyEnergy", "Daily Energy", "kWh", "float", "Realtime"),
                    ("efficiency", "Efficiency", "%", "float", "Realtime"),
                    ("irradiance", "Irradiance", "W/m²", "float", "Realtime"),
                    ("panelTemperature", "Panel Temperature", "°C", "float", "Realtime"),
                    ("ambientTemperature", "Ambient Temperature", "°C", "float", "Realtime"),
                    ("alarmState", "Alarm State", "", "string", "Realtime"),
                    ("peakPower", "Peak Power Today", "kW", "float", "Calculated")
                ),
                Created = now
            },
            new()
            {
                Name = LogicalNodeNames.SolarInverterData,
                DisplayName = "Solar Inverter Data",
                Description = "Electrical measurements from a solar inverter",
                Parameters = P(
                    ("dcVoltage", "DC Voltage", "V", "float", "Realtime"),
                    ("dcCurrent", "DC Current", "A", "float", "Realtime"),
                    ("acVoltage", "AC Voltage", "V", "float", "Realtime"),
                    ("acCurrent", "AC Current", "A", "float", "Realtime"),
                    ("acFrequency", "AC Frequency", "Hz", "float", "Realtime"),
                    ("acPower", "AC Power", "kW", "float", "Realtime")
                ),
                Created = now
            },

            // Grid Nodes
            new()
            {
                Name = LogicalNodeNames.GridMeterPowerData,
                DisplayName = "Grid Meter Power Data",
                Description = "Real-time power measurements from a grid meter",
                Parameters = P(
                    ("voltageA", "Voltage Phase A", "V", "float", "Realtime"),
                    ("voltageB", "Voltage Phase B", "V", "float", "Realtime"),
                    ("voltageC", "Voltage Phase C", "V", "float", "Realtime"),
                    ("currentA", "Current Phase A", "A", "float", "Realtime"),
                    ("currentB", "Current Phase B", "A", "float", "Realtime"),
                    ("currentC", "Current Phase C", "A", "float", "Realtime"),
                    ("activePower", "Active Power", "kW", "float", "Realtime"),
                    ("reactivePower", "Reactive Power", "kVAR", "float", "Realtime"),
                    ("apparentPower", "Apparent Power", "kVA", "float", "Realtime"),
                    ("powerFactor", "Power Factor", "", "float", "Realtime"),
                    ("frequency", "Frequency", "Hz", "float", "Realtime"),
                    ("totalEnergy", "Total Energy", "kWh", "float", "Realtime")
                ),
                Created = now
            },
            new()
            {
                Name = LogicalNodeNames.GridBreakerLifecycleData,
                DisplayName = "Grid Breaker Lifecycle Data",
                Description = "Calculated lifecycle metrics for grid breakers",
                Parameters = P(
                    ("operationCount", "Operation Count", "", "int", "Calculated"),
                    ("lastTripDate", "Last Trip Date", "", "datetime", "Calculated"),
                    ("avgTripCurrent", "Avg Trip Current", "A", "float", "Calculated"),
                    ("estimatedRemainingOps", "Estimated Remaining Operations", "", "int", "Calculated"),
                    ("maintenanceRequired", "Maintenance Required", "", "bool", "Calculated"),
                    ("healthScore", "Health Score", "%", "float", "Calculated")
                ),
                Created = now
            },

            // Wind Nodes
            new()
            {
                Name = LogicalNodeNames.WindPowerData,
                DisplayName = "Wind Power Data",
                Description = "Real-time power output from a wind turbine",
                Parameters = P(
                    ("activePower", "Active Power", "kW", "float", "Realtime"),
                    ("reactivePower", "Reactive Power", "kVAR", "float", "Realtime"),
                    ("dailyEnergy", "Daily Energy", "kWh", "float", "Realtime"),
                    ("totalEnergy", "Total Energy", "MWh", "float", "Realtime")
                ),
                Created = now
            },
            new()
            {
                Name = LogicalNodeNames.WindConditionData,
                DisplayName = "Wind Condition Data",
                Description = "Environmental conditions at a wind turbine site",
                Parameters = P(
                    ("windSpeed", "Wind Speed", "m/s", "float", "Realtime"),
                    ("windDirection", "Wind Direction", "°", "float", "Realtime"),
                    ("ambientTemp", "Ambient Temperature", "°C", "float", "Realtime"),
                    ("humidity", "Humidity", "%", "float", "Realtime"),
                    ("airPressure", "Air Pressure", "hPa", "float", "Realtime")
                ),
                Created = now
            },
            new()
            {
                Name = LogicalNodeNames.WindElectricalData,
                DisplayName = "Wind Electrical Data",
                Description = "Electrical measurements from a wind turbine generator",
                Parameters = P(
                    ("generatorSpeed", "Generator Speed", "RPM", "float", "Realtime"),
                    ("rotorSpeed", "Rotor Speed", "RPM", "float", "Realtime"),
                    ("pitchAngle", "Pitch Angle", "°", "float", "Realtime"),
                    ("yawAngle", "Yaw Angle", "°", "float", "Realtime"),
                    ("voltageA", "Voltage Phase A", "V", "float", "Realtime"),
                    ("voltageB", "Voltage Phase B", "V", "float", "Realtime"),
                    ("voltageC", "Voltage Phase C", "V", "float", "Realtime"),
                    ("currentA", "Current Phase A", "A", "float", "Realtime"),
                    ("currentB", "Current Phase B", "A", "float", "Realtime"),
                    ("frequency", "Frequency", "Hz", "float", "Realtime")
                ),
                Created = now
            },

            // Common Nodes
            new()
            {
                Name = LogicalNodeNames.DeviceMaintenanceData,
                DisplayName = "Device Maintenance Data",
                Description = "Calculated maintenance metrics applicable to any device",
                Parameters = P(
                    ("lastMaintenanceDate", "Last Maintenance Date", "", "datetime", "Calculated"),
                    ("nextMaintenanceDate", "Next Maintenance Date", "", "datetime", "Calculated"),
                    ("maintenanceScore", "Maintenance Score", "%", "float", "Calculated")
                ),
                Created = now
            },
            new()
            {
                Name = LogicalNodeNames.DeviceNameplateData,
                DisplayName = "Device Nameplate Data",
                Description = "Common registration data for any device",
                Parameters = P(
                    ("serialNumber", "Serial Number", "", "string", "Registration"),
                    ("manufacturer", "Manufacturer", "", "string", "Registration"),
                    ("model", "Model", "", "string", "Registration"),
                    ("firmwareVersion", "Firmware Version", "", "string", "Registration"),
                    ("installationDate", "Installation Date", "", "datetime", "Installation"),
                    ("warrantyExpiry", "Warranty Expiry Date", "", "datetime", "Registration")
                ),
                Created = now
            }
        };

        // ── LogicalDevices ──
        var devices = new List<(LogicalDevice Device, NodeSpec[] Nodes)>
        {
            (new LogicalDevice
            {
                Name = LogicalDeviceNames.BatteryContainer,
                DisplayName = "Battery Container",
                Description = "A BESS container with racks, packs, PCS and HVAC",
                Category = "BESS",
                HierarchyTemplate = @"{""children"":[{""logicalDeviceName"":""BatteryRack"",""count"":4,""namePattern"":""Rack {index}"",""deviceTypeName"":""EnergyStorage.Rack""}]}",
                Created = now
            }, new[] { new NodeSpec(LogicalNodeNames.BatteryNameplateData, 0), new NodeSpec(LogicalNodeNames.BatteryStatusData, 1), new NodeSpec(LogicalNodeNames.BatteryEnergyData, 2), new NodeSpec(LogicalNodeNames.BatteryHealthData, 3), new NodeSpec(LogicalNodeNames.HvacStatusData, 4) }),

            (new LogicalDevice
            {
                Name = LogicalDeviceNames.BatteryRack,
                DisplayName = "Battery Rack",
                Description = "A rack within a battery container, containing packs",
                Category = "BESS",
                HierarchyTemplate = @"{""children"":[{""logicalDeviceName"":""BatteryPack"",""count"":8,""namePattern"":""Pack {index}"",""deviceTypeName"":""EnergyStorage.Pack""}]}",
                Created = now
            }, new[] { new NodeSpec(LogicalNodeNames.BatteryPackEnergyData, 0) }),

            (new LogicalDevice
            {
                Name = LogicalDeviceNames.BatteryPack,
                DisplayName = "Battery Pack",
                Description = "A pack/module within a battery rack",
                Category = "BESS",
                Created = now
            }, new[] { new NodeSpec(LogicalNodeNames.BatteryModuleEnergyData, 0) }),

            (new LogicalDevice
            {
                Name = LogicalDeviceNames.PcsUnit,
                DisplayName = "PCS Unit",
                Description = "A power conversion system (bidirectional inverter)",
                Category = "PCS",
                Created = now
            }, new[] { new NodeSpec(LogicalNodeNames.PcsStatusData, 0), new NodeSpec(LogicalNodeNames.PcsAcMeasurementData, 1), new NodeSpec(LogicalNodeNames.PcsDcMeasurementData, 2), new NodeSpec(LogicalNodeNames.PcsChargeAlgorithmData, 3) }),

            (new LogicalDevice
            {
                Name = LogicalDeviceNames.SolarInverter,
                DisplayName = "Solar Inverter",
                Description = "A solar inverter with MPPT strings",
                Category = "Solar",
                Created = now
            }, new[] { new NodeSpec(LogicalNodeNames.SolarStatusData, 0), new NodeSpec(LogicalNodeNames.SolarInverterData, 1, InstanceCount: 2, Alias: "mppt") }),

            (new LogicalDevice
            {
                Name = LogicalDeviceNames.GridMeter,
                DisplayName = "Grid Meter",
                Description = "A grid-connection smart meter with breaker monitoring",
                Category = "Grid",
                Created = now
            }, new[] { new NodeSpec(LogicalNodeNames.GridMeterPowerData, 0), new NodeSpec(LogicalNodeNames.GridBreakerLifecycleData, 1) }),

            (new LogicalDevice
            {
                Name = LogicalDeviceNames.WindTurbine,
                DisplayName = "Wind Turbine",
                Description = "A wind turbine generator",
                Category = "Wind",
                Created = now
            }, new[] { new NodeSpec(LogicalNodeNames.WindPowerData, 0), new NodeSpec(LogicalNodeNames.WindConditionData, 1), new NodeSpec(LogicalNodeNames.WindElectricalData, 2) })
        };

        return (nodes, devices);
    }
}
