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
                Name = "Demo Tenant",
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
                Name = "Demo Energy Solutions",
                Type = "Energy Management",
                Country = "Netherlands",
                State = "North Holland",
                City = "Amsterdam",
                District = "Centrum",
                PostalCode = "1012 JS",
                Address = "Damrak 123",
                ContactEmail = "contact@demo-energy.nl",
                ContactPhone = "+31 20 123 4567",
                Fax = "+31 20 123 4568",
                TaxNumber = "NL123456789B01",
                TaxOffice = "Amsterdam",
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
                Name = "Demo Solar Park Amsterdam",
                Address = "Westpoort Industrial Area 456, 1014 AK Amsterdam, Netherlands",
                EnergyCapacity = 100,
                EnergyUnit = Backend.Domain.Enums.EnergyUnit.KWh,
                Longitude = 4.8945, // Amsterdam coordinates
                Latitude = 52.3676,
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

        // Seed RTU (Remote Terminal Unit) for the facility
        Guid rtuId;
        var existingRtu = await _context.Rtus.IgnoreQueryFilters().OrderBy(x => x.Created).FirstOrDefaultAsync();
        if (existingRtu == null)
        {
            var rtu = new Rtu
            {
                FacilityId = facilitiyId,
                RtuSerialNumber = "RTU-DEMO-001",
                RtuModel = "Siemens SICAM A8000",
                Protocol = "Modbus TCP",
                Status = "online",
                Created = DateTime.UtcNow
            };
            _context.Rtus.Add(rtu);
            await _context.SaveChangesAsync();
            rtuId = rtu.Id;
            _logger.LogInformation("Demo RTU created with ID {RtuId}", rtuId);
        }
        else
        {
            rtuId = existingRtu.Id;
        }

        var devices = new List<Device>
        {
            new Device
            {
                RtuId = rtuId,
                DeviceTypeId = dtLookup[DeviceTypeNames.EnergyStorage],
                Name = "BESS_001",
                Model = "Tesla Megapack",
                SerialNumber = "BESS-SN-001",
                Manufacturer = "Tesla",
                Ports = "[{\"id\":\"DC\",\"type\":\"DC\",\"config\":{\"voltage_max\":1500,\"current_max\":100}}]"
            },
            new Device
            {
                RtuId = rtuId,
                DeviceTypeId = dtLookup[DeviceTypeNames.Solar],
                Name = "PV_001",
                Model = "SunPower Maxeon 6",
                SerialNumber = "PV-SN-001",
                Manufacturer = "SunPower",
                Ports = "[{\"id\":\"DC\",\"type\":\"DC\",\"config\":{\"voltage_max\":600,\"current_max\":15}}]"
            },
            new Device
            {
                RtuId = rtuId,
                DeviceTypeId = dtLookup[DeviceTypeNames.Inverter],
                Name = "INVERTER_001",
                Model = "SMA Sunny Tripower",
                SerialNumber = "INV-SN-001",
                Manufacturer = "SMA",
                Ports = "[{\"id\":\"Grid\",\"type\":\"AC\",\"config\":{\"voltage\":230,\"frequency\":50}},{\"id\":\"MPPT\",\"type\":\"DC\",\"config\":{\"voltage_max\":1000}}]"
            },
            new Device
            {
                RtuId = rtuId,
                DeviceTypeId = dtLookup[DeviceTypeNames.SmartMeter],
                Name = "SM_001",
                Model = "Schneider PM5560",
                SerialNumber = "SM-SN-001",
                Manufacturer = "Schneider Electric",
                Ports = "[{\"id\":\"Main\",\"type\":\"AC\",\"config\":{\"voltage\":230,\"frequency\":50}}]"
            },
            new Device
            {
                RtuId = rtuId,
                DeviceTypeId = dtLookup["Grid"],
                Name = "GRID_001",
                Model = "Grid Connection",
                SerialNumber = "GRID-SN-001",
                Ports = "[{\"id\":\"Main\",\"type\":\"AC\",\"config\":{\"voltage\":230,\"frequency\":50}}]"
            },
            new Device
            {
                RtuId = rtuId,
                DeviceTypeId = dtLookup["Generator"],
                Name = "GEN_001",
                Model = "Caterpillar C18",
                SerialNumber = "GEN-SN-001",
                Manufacturer = "Caterpillar",
                Ports = "[{\"id\":\"Output\",\"type\":\"AC\",\"config\":{\"voltage\":480,\"frequency\":50}}]"
            }
        };

        if (!_context.Devices.Any())
        {
            _context.Devices.AddRange(devices);
            await _context.SaveChangesAsync();
        }

        // Seed DeviceSpecificationTemplate for Tesla Megapack
        if (!_context.DeviceSpecificationTemplates.Any())
        {
            var batteryTypeId = dtLookup.GetValueOrDefault(DeviceTypeNames.EnergyStorage);
            if (batteryTypeId != Guid.Empty)
            {
                var teslaMegapackTemplate = new DeviceSpecificationTemplate
                {
                    DeviceTypeId = batteryTypeId,
                    Model = "Megapack 2XL",
                    Manufacturer = "Tesla",
                    IsDefault = true,
                    SpecificationsTemplate = """
                    {
                      "supportedTabs": [
                        { "key": "summary", "label": "Summary", "isEnabled": true, "order": 0 },
                        { "key": "health", "label": "Health", "isEnabled": true, "order": 1 },
                        { "key": "hierarchy", "label": "Hierarchy", "isEnabled": true, "order": 2 },
                        { "key": "curves", "label": "Curves", "isEnabled": true, "order": 3 },
                        { "key": "faults", "label": "Faults", "isEnabled": true, "order": 4 }
                      ],
                      "tabs": {
                        "summary": {
                          "sections": [
                            {
                              "key": "liveStatus",
                              "title": "Live Status",
                              "order": 0,
                              "fields": [
                                { "key": "soc", "label": "State of Charge", "sourcePath": "stateOfCharge", "unit": "%", "order": 0 },
                                { "key": "soh", "label": "State of Health", "sourcePath": "stateOfHealth", "unit": "%", "order": 1 },
                                { "key": "chargeStatus", "label": "Charge Status", "sourcePath": "chargeStatus", "order": 2 }
                              ]
                            },
                            {
                              "key": "dcPower",
                              "title": "DC Power",
                              "order": 1,
                              "fields": [
                                { "key": "voltage", "label": "Voltage", "sourcePath": "dcPower.voltage", "unit": "V", "order": 0 },
                                { "key": "current", "label": "Current", "sourcePath": "dcPower.current", "unit": "A", "order": 1 },
                                { "key": "power", "label": "Power", "sourcePath": "dcPower.power", "unit": "kW", "order": 2 }
                              ]
                            }
                          ]
                        }
                      },
                      "hierarchy": [
                        { "key": "rack", "label": "Rack", "hasChildren": true },
                        { "key": "pack", "label": "Pack", "hasChildren": true },
                        { "key": "module", "label": "Module", "hasChildren": true },
                        { "key": "cell", "label": "Cell", "hasChildren": false }
                      ],
                      "curveCharts": [
                        {
                          "key": "power",
                          "title": "Power",
                          "chartType": "line",
                          "series": [
                            { "key": "dcPower", "label": "DC Power", "sourcePath": "dcPower.power", "unit": "kW", "color": "#4CAF50" }
                          ]
                        },
                        {
                          "key": "soc",
                          "title": "State of Charge",
                          "chartType": "area",
                          "series": [
                            { "key": "soc", "label": "SOC", "sourcePath": "stateOfCharge", "unit": "%", "color": "#2196F3" }
                          ]
                        }
                      ]
                    }
                    """,
                    ParameterMapTemplate = """
                    {
                      "version": 1,
                      "parameters": {
                        "soc": {
                          "type": "number", "unit": "%",
                          "source": { "kind": "Battery.Energy", "paths": ["socPercent", "stateOfCharge", "soc"] }
                        },
                        "soh": {
                          "type": "number", "unit": "%",
                          "source": { "kind": "Battery.Energy", "paths": ["sohPercent", "stateOfHealth"] }
                        },
                        "voltage": {
                          "type": "number", "unit": "V",
                          "source": { "kind": "Battery.Energy", "paths": ["totalVoltageV"] }
                        },
                        "current": {
                          "type": "number", "unit": "A",
                          "source": { "kind": "Battery.Energy", "paths": ["instantCurrentA"] }
                        },
                        "power": {
                          "type": "number", "unit": "kW",
                          "formula": {
                            "expression": "round(multiply(voltage, current, 0.001), 1)",
                            "inputs": { "voltage": {"$ref":"voltage"}, "current": {"$ref":"current"} }
                          },
                          "fallback": { "kind": "Battery.Energy", "paths": ["dcPower.power", "power"] }
                        },
                        "powerDirection": {
                          "type": "string",
                          "formula": {
                            "expression": "cond(lt(current, 0), 'discharging', cond(gt(current, 0), 'charging', 'idle'))",
                            "inputs": { "current": {"$ref":"current"} }
                          }
                        },
                        "tempMin": {
                          "type": "number", "unit": "C",
                          "source": { "kind": "Battery.Energy", "paths": ["minCellTempC"] }
                        },
                        "tempMax": {
                          "type": "number", "unit": "C",
                          "source": { "kind": "Battery.Energy", "paths": ["maxCellTempC"] }
                        },
                        "tempAvg": {
                          "type": "number", "unit": "C",
                          "formula": {
                            "expression": "round(avg(tempMin, tempMax), 1)",
                            "inputs": { "tempMin": {"$ref":"tempMin"}, "tempMax": {"$ref":"tempMax"} }
                          }
                        },
                        "cellVoltageMax": {
                          "type": "number", "unit": "mV",
                          "source": { "kind": "Battery.Energy", "paths": ["maxCellVoltageMv"] }
                        },
                        "cellVoltageMin": {
                          "type": "number", "unit": "mV",
                          "source": { "kind": "Battery.Energy", "paths": ["minCellVoltageMv"] }
                        },
                        "voltageSpread": {
                          "type": "number", "unit": "mV",
                          "formula": {
                            "expression": "subtract(cellVoltageMax, cellVoltageMin)",
                            "inputs": { "cellVoltageMax": {"$ref":"cellVoltageMax"}, "cellVoltageMin": {"$ref":"cellVoltageMin"} }
                          }
                        },
                        "pcsActivePower": {
                          "type": "number", "unit": "kW",
                          "source": { "kind": "Pcs.PowerDistribution", "paths": ["pcs_outActPwrKw"] }
                        },
                        "pcsDcPower": {
                          "type": "number", "unit": "kW",
                          "source": { "kind": "Pcs.DcMeasurement", "paths": ["pcs_dcPwrKw"] }
                        },
                        "pcsGridFreq": {
                          "type": "number", "unit": "Hz",
                          "source": { "kind": "Pcs.Status", "paths": ["pcs_gridFreqHz"] }
                        },
                        "cycleCount": {
                          "type": "number",
                          "source": { "kind": "Battery.Energy", "paths": ["cycleCount"] }
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
                          "source": { "kind": "Battery.Energy", "paths": ["state"] }
                        },
                        "remainingEnergy": {
                          "type": "number", "unit": "kWh",
                          "source": { "kind": "Battery.Energy", "paths": ["remainingEnergyKwh"] }
                        },
                        "availableEnergy": {
                          "type": "number", "unit": "kWh",
                          "source": { "kind": "Battery.Energy", "paths": ["availableEnergyKwh"] }
                        },
                        "chillerCoolantOut": {
                          "type": "number", "unit": "C",
                          "source": { "kind": "Battery.Thermal", "paths": ["outletCoolantTempC"] }
                        },
                        "chillerCoolantIn": {
                          "type": "number", "unit": "C",
                          "source": { "kind": "Battery.Thermal", "paths": ["inletCoolantTempC"] }
                        },
                        "compressorRunning": {
                          "type": "boolean",
                          "source": { "kind": "Battery.Thermal", "paths": ["compressorStatus"] }
                        },
                        "ambientTemp": {
                          "type": "number", "unit": "C",
                          "source": { "kind": "Battery.Thermal", "paths": ["ambientTempC"] }
                        }
                      }
                    }
                    """,
                    MetadataTemplate = """
                    {
                      "ratedCapacityKwh": 3916,
                      "ratedPowerKw": 1550,
                      "nominalVoltage": 1500,
                      "cellChemistry": "LFP",
                      "cellsPerModule": 14
                    }
                    """,
                    HierarchyTemplate = """
                    {
                      "children": [
                        {
                          "key": "battery",
                          "deviceTypeName": "EnergyStorage.Battery",
                          "count": 1,
                          "namePattern": "Battery",
                          "children": [
                            {
                              "key": "rack",
                              "deviceTypeName": "EnergyStorage.Rack",
                              "count": 4,
                              "namePattern": "Rack {index}",
                              "children": [
                                {
                                  "key": "pack",
                                  "deviceTypeName": "EnergyStorage.Pack",
                                  "count": 4,
                                  "namePattern": "Pack {index}",
                                  "children": [
                                    {
                                      "key": "module",
                                      "deviceTypeName": "EnergyStorage.Module",
                                      "count": 8,
                                      "namePattern": "Module {index}"
                                    }
                                  ]
                                }
                              ]
                            }
                          ]
                        },
                        {
                          "key": "esc",
                          "deviceTypeName": "EnergyStorage.Controller",
                          "count": 1,
                          "namePattern": "ESC-{index}"
                        },
                        {
                          "key": "hvac",
                          "deviceTypeName": "EnergyStorage.TMS",
                          "count": 2,
                          "namePattern": "HVAC-{index}"
                        }
                      ]
                    }
                    """,
                    Created = DateTime.UtcNow
                };

                _context.DeviceSpecificationTemplates.Add(teslaMegapackTemplate);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Seeded Tesla Megapack 2XL device specification template");
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

        // Seed DeviceAlarmTypes for Demo BESS model (global entity — no tenant filter)
        if (!_context.DeviceAlarmTypes.Any())
        {
            var esTypeId = dtLookup.GetValueOrDefault(DeviceTypeNames.EnergyStorage);
            if (esTypeId != Guid.Empty)
            {
                var alarmTypes = BuildDemoBessAlarmTypes(esTypeId);
                _context.DeviceAlarmTypes.AddRange(alarmTypes);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Seeded {Count} DeviceAlarmTypes for DEMO-BG", alarmTypes.Count);
            }
        }

        // Seed DeviceAlarmTypes for Demo PCS model
        var pcsTypeId = dtLookup.GetValueOrDefault(DeviceTypeNames.Pcs);
        if (pcsTypeId != Guid.Empty && !_context.DeviceAlarmTypes.Any(t => t.DeviceTypeId == pcsTypeId))
        {
            var pcsAlarmTypes = BuildDemoPcsAlarmTypes(pcsTypeId);
            _context.DeviceAlarmTypes.AddRange(pcsAlarmTypes);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeded {Count} DeviceAlarmTypes for DEMO-PCS", pcsAlarmTypes.Count);
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
                ContactEmail = "info@pemvilla.com",
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

        #region DemoAlarmsAndMaintenanceTasks

        // Seed demo alarms and maintenance tasks for "Demo Solar Park Amsterdam"
        if (!await _context.DeviceAlarms.IgnoreQueryFilters().AnyAsync(a => a.DeviceName == "BESS_001" || a.DeviceName == "PV_001"))
        {
            _logger.LogInformation("Seeding demo alarms and maintenance tasks for Demo Solar Park Amsterdam...");

            // Look up devices by name (they were created earlier in this method)
            var demoDevices = await _context.Devices.IgnoreQueryFilters()
                .Where(d => d.Rtu != null && d.Rtu.FacilityId == facilitiyId)
                .ToListAsync();
            var deviceByName = demoDevices.ToDictionary(d => d.Name!, d => d);

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
        }

        #endregion
    }

    private static List<DeviceAlarmType> BuildDemoBessAlarmTypes(Guid energyStorageTypeId)
    {
        var now = DateTime.UtcNow;

        DeviceAlarmType T(string code, string faultKey, string kind, string name, string category, string severity, string source, string? desc = null)
            => new()
            {
                DeviceTypeId = energyStorageTypeId,
                Model = "DEMO-BG",
                Manufacturer = "Demo",
                Code = code,
                FaultKey = faultKey,
                Kind = kind,
                Name = name,
                Description = desc,
                Category = category,
                Severity = severity,
                Source = source,
                IsEnabled = true,
                Created = now
            };

        return new List<DeviceAlarmType>
        {
            // ContainerBatteryFaultState — BMS faults
            T("BATT_CELL_OV_WARN", "faultCellOverVoltageWarning", "Battery.Status", "Cell Over Voltage Warning", "battery", "Warning", "BMS"),
            T("BATT_CELL_UV_WARN", "faultCellUnderVoltageWarning", "Battery.Status", "Cell Under Voltage Warning", "battery", "Warning", "BMS"),
            T("BATT_CELL_OV_FAULT", "faultCellOverVoltageFault", "Battery.Status", "Cell Over Voltage Fault", "battery", "Critical", "BMS"),
            T("BATT_CELL_UV_FAULT", "faultCellUnderVoltageFault", "Battery.Status", "Cell Under Voltage Fault", "battery", "Critical", "BMS"),
            T("BATT_PACK_OC", "faultPackOC", "Battery.Status", "Pack Over Current", "battery", "Critical", "BMS"),
            T("BATT_PACK_OV", "faultPackOV", "Battery.Status", "Pack Over Voltage", "battery", "Major", "BMS"),
            T("BATT_PACK_UV", "faultPackUV", "Battery.Status", "Pack Under Voltage", "battery", "Major", "BMS"),
            T("BATT_OT_WARN", "faultOverTempWarning", "Battery.Status", "Over Temperature Warning", "battery", "Warning", "BMS"),
            T("BATT_OT_FAULT", "faultOverTempFault", "Battery.Status", "Over Temperature Fault", "battery", "Critical", "BMS"),
            T("BATT_UT_WARN", "faultUnderTempWarning", "Battery.Status", "Under Temperature Warning", "battery", "Warning", "BMS"),
            T("BATT_UT_FAULT", "faultUnderTempFault", "Battery.Status", "Under Temperature Fault", "battery", "Critical", "BMS"),
            T("BATT_INSUL_WARN", "faultInsulationWarning", "Battery.Status", "Insulation Warning", "battery", "Warning", "BMS"),
            T("BATT_INSUL_FAULT", "faultInsulationFault", "Battery.Status", "Insulation Fault", "battery", "Critical", "BMS"),
            T("BATT_VDIFF_WARN", "faultVoltageDiffWarning", "Battery.Status", "Voltage Difference Warning", "battery", "Warning", "BMS"),
            T("BATT_VDIFF_FAULT", "faultVoltageDiffFault", "Battery.Status", "Voltage Difference Fault", "battery", "Major", "BMS"),
            T("BATT_TDIFF_WARN", "faultTempDiffWarning", "Battery.Status", "Temperature Difference Warning", "battery", "Warning", "BMS"),
            T("BATT_TDIFF_FAULT", "faultTempDiffFault", "Battery.Status", "Temperature Difference Fault", "battery", "Major", "BMS"),
            T("BATT_SOC_LOW_WARN", "faultSocLowWarning", "Battery.Status", "SOC Low Warning", "battery", "Warning", "BMS"),
            T("BATT_SOC_LOW_FAULT", "faultSocLowFault", "Battery.Status", "SOC Low Fault", "battery", "Major", "BMS"),
            T("BATT_SOC_HIGH_WARN", "faultSocHighWarning", "Battery.Status", "SOC High Warning", "battery", "Warning", "BMS"),
            T("BATT_BMS_COMM", "faultBmsCommFailure", "Battery.Status", "BMS Communication Failure", "battery", "Critical", "BMS"),
            T("BATT_CONTACTOR", "faultContactorFailure", "Battery.Status", "Contactor Failure", "battery", "Critical", "BMS"),
            T("BATT_FUSE", "faultFuseBlown", "Battery.Status", "Fuse Blown", "battery", "Critical", "BMS"),
            T("BATT_CHG_OC", "faultChargeOverCurrent", "Battery.Status", "Charge Over Current", "battery", "Major", "BMS"),
            T("BATT_DISCHG_OC", "faultDischargeOverCurrent", "Battery.Status", "Discharge Over Current", "battery", "Major", "BMS"),
            T("BATT_EMRG_STOP", "faultEmergencyStop", "Battery.Status", "Emergency Stop", "battery", "Critical", "BMS"),
            T("BATT_PRECHARGE", "faultPrechargeFailure", "Battery.Status", "Precharge Failure", "battery", "Major", "BMS"),

            // ChillerAlarmState — TMS alarms
            T("TMS_COOLANT_SHORT", "systemCoolantShortage", "ChillerAlarmState", "System Coolant Shortage", "chiller", "Major", "TMS"),
            T("TMS_VFD_OC", "compressorVfdOcAlarm", "ChillerAlarmState", "Compressor VFD Over Current", "chiller", "Major", "TMS"),
            T("TMS_PWR_OV", "powerSupplyOV", "ChillerAlarmState", "Power Supply Over Voltage", "chiller", "Warning", "TMS"),
            T("TMS_PWR_UV", "powerSupplyUV", "ChillerAlarmState", "Power Supply Under Voltage", "chiller", "Warning", "TMS"),
            T("TMS_COMP_OT", "compressorOverTemp", "ChillerAlarmState", "Compressor Over Temperature", "chiller", "Major", "TMS"),
            T("TMS_COMP_HP", "compressorHighPressure", "ChillerAlarmState", "Compressor High Pressure", "chiller", "Major", "TMS"),
            T("TMS_COMP_LP", "compressorLowPressure", "ChillerAlarmState", "Compressor Low Pressure", "chiller", "Warning", "TMS"),
            T("TMS_PUMP_FAULT", "coolantPumpFault", "ChillerAlarmState", "Coolant Pump Fault", "chiller", "Critical", "TMS"),
            T("TMS_FAN_FAULT", "condenserFanFault", "ChillerAlarmState", "Condenser Fan Fault", "chiller", "Major", "TMS"),
            T("TMS_FLOW_LOW", "coolantFlowLow", "ChillerAlarmState", "Coolant Flow Low", "chiller", "Warning", "TMS"),
            T("TMS_OUTLET_OT", "outletOverTemp", "ChillerAlarmState", "Outlet Over Temperature", "chiller", "Warning", "TMS"),
            T("TMS_OUTLET_UT", "outletUnderTemp", "ChillerAlarmState", "Outlet Under Temperature", "chiller", "Warning", "TMS"),
            T("TMS_COMM", "chillerCommFailure", "ChillerAlarmState", "Chiller Communication Failure", "chiller", "Major", "TMS"),
            T("TMS_VFD_COMM", "compressorVfdCommFault", "ChillerAlarmState", "VFD Communication Fault", "chiller", "Major", "TMS"),
            T("TMS_HEATER", "heaterFault", "ChillerAlarmState", "Heater Fault", "chiller", "Warning", "TMS"),

            // PcsFaultGrp1State — PCS hardware & grid faults
            T("PCS_HW_EPO", "pcs_hwEpoFault", "Pcs.Status", "Hardware EPO Fault", "pcs", "Critical", "PCS"),
            T("PCS_GRID_A_OV", "pcs_gridAOvFault", "Pcs.Status", "Grid Phase A Over Voltage", "pcs", "Major", "PCS"),
            T("PCS_GRID_B_OV", "pcs_gridBOvFault", "Pcs.Status", "Grid Phase B Over Voltage", "pcs", "Major", "PCS"),
            T("PCS_GRID_C_OV", "pcs_gridCOvFault", "Pcs.Status", "Grid Phase C Over Voltage", "pcs", "Major", "PCS"),
            T("PCS_GRID_A_UV", "pcs_gridAUvFault", "Pcs.Status", "Grid Phase A Under Voltage", "pcs", "Major", "PCS"),
            T("PCS_GRID_B_UV", "pcs_gridBUvFault", "Pcs.Status", "Grid Phase B Under Voltage", "pcs", "Major", "PCS"),
            T("PCS_GRID_C_UV", "pcs_gridCUvFault", "Pcs.Status", "Grid Phase C Under Voltage", "pcs", "Major", "PCS"),
            T("PCS_GRID_FREQ_HI", "pcs_gridFreqHighFault", "Pcs.Status", "Grid Frequency High", "pcs", "Major", "PCS"),
            T("PCS_GRID_FREQ_LO", "pcs_gridFreqLowFault", "Pcs.Status", "Grid Frequency Low", "pcs", "Major", "PCS"),
            T("PCS_DC_OV", "pcs_dcBusOvFault", "Pcs.Status", "DC Bus Over Voltage", "pcs", "Critical", "PCS"),
            T("PCS_DC_UV", "pcs_dcBusUvFault", "Pcs.Status", "DC Bus Under Voltage", "pcs", "Major", "PCS"),
            T("PCS_IGBT_OT", "pcs_igbtOverTempFault", "Pcs.Status", "IGBT Over Temperature", "pcs", "Critical", "PCS"),
            T("PCS_AC_OC_A", "pcs_acOverCurrentAFault", "Pcs.Status", "AC Over Current Phase A", "pcs", "Major", "PCS"),
            T("PCS_AC_OC_B", "pcs_acOverCurrentBFault", "Pcs.Status", "AC Over Current Phase B", "pcs", "Major", "PCS"),
            T("PCS_AC_OC_C", "pcs_acOverCurrentCFault", "Pcs.Status", "AC Over Current Phase C", "pcs", "Major", "PCS"),
            T("PCS_DC_OC", "pcs_dcOverCurrentFault", "Pcs.Status", "DC Over Current", "pcs", "Critical", "PCS"),
            T("PCS_AMBIENT_OT", "pcs_ambientOverTempFault", "Pcs.Status", "Ambient Over Temperature", "pcs", "Warning", "PCS"),
            T("PCS_INDUCT_OT", "pcs_inductanceOverTempFault", "Pcs.Status", "Inductance Over Temperature", "pcs", "Major", "PCS"),

            // PcsFaultGrp2State — PCS protection & relay faults
            T("PCS_PREREL_CLOSE", "pcs_preRelCloseFail", "Pcs.Status", "Pre-charge Relay Close Failure", "pcs", "Critical", "PCS"),
            T("PCS_PREREL_OPEN", "pcs_preRelOpenFail", "Pcs.Status", "Pre-charge Relay Open Failure", "pcs", "Critical", "PCS"),
            T("PCS_MAINREL_CLOSE", "pcs_mainRelCloseFail", "Pcs.Status", "Main Relay Close Failure", "pcs", "Critical", "PCS"),
            T("PCS_MAINREL_OPEN", "pcs_mainRelOpenFail", "Pcs.Status", "Main Relay Open Failure", "pcs", "Critical", "PCS"),
            T("PCS_ACBKR_CLOSE", "pcs_acBreakerCloseFail", "Pcs.Status", "AC Breaker Close Failure", "pcs", "Critical", "PCS"),
            T("PCS_ACBKR_OPEN", "pcs_acBreakerOpenFail", "Pcs.Status", "AC Breaker Open Failure", "pcs", "Critical", "PCS"),
            T("PCS_GROUND_FAULT", "pcs_groundFault", "Pcs.Status", "Ground Fault", "pcs", "Critical", "PCS"),
            T("PCS_FAN_FAULT", "pcs_coolingFanFault", "Pcs.Status", "Cooling Fan Fault", "pcs", "Major", "PCS"),
            T("PCS_FUSE_FAULT", "pcs_dcFuseFault", "Pcs.Status", "DC Fuse Fault", "pcs", "Critical", "PCS"),
            T("PCS_SPD_FAULT", "pcs_surgeSpdFault", "Pcs.Status", "Surge Protector Fault", "pcs", "Major", "PCS"),
            T("PCS_PHASE_SEQ", "pcs_phaseSeqFault", "Pcs.Status", "Phase Sequence Fault", "pcs", "Major", "PCS"),
            T("PCS_SYNC_FAIL", "pcs_gridSyncFail", "Pcs.Status", "Grid Sync Failure", "pcs", "Major", "PCS"),
            T("PCS_SW_EPO", "pcs_swEpoFault", "Pcs.Status", "Software EPO Fault", "pcs", "Critical", "PCS"),
            T("PCS_COMM_BMS", "pcs_bmsCommFault", "Pcs.Status", "BMS Communication Fault", "pcs", "Major", "PCS"),
            T("PCS_COMM_EMS", "pcs_emsCommFault", "Pcs.Status", "EMS Communication Fault", "pcs", "Warning", "PCS"),

            // PcsTimeoutsState — PCS protection timeouts
            T("PCS_DC_OC_TOUT", "pcs_dcOcProtTout", "PcsTimeoutsState", "DC Over Current Protection Timeout", "pcs", "Critical", "PCS"),
            T("PCS_AC_OC_TOUT", "pcs_acOcProtTout", "PcsTimeoutsState", "AC Over Current Protection Timeout", "pcs", "Critical", "PCS"),
            T("PCS_DC_OV_TOUT", "pcs_dcOvProtTout", "PcsTimeoutsState", "DC Over Voltage Protection Timeout", "pcs", "Critical", "PCS"),
            T("PCS_GRID_OV_TOUT", "pcs_gridOvProtTout", "PcsTimeoutsState", "Grid Over Voltage Protection Timeout", "pcs", "Critical", "PCS"),
            T("PCS_GRID_UV_TOUT", "pcs_gridUvProtTout", "PcsTimeoutsState", "Grid Under Voltage Protection Timeout", "pcs", "Critical", "PCS"),
            T("PCS_GRID_FREQ_TOUT", "pcs_gridFreqProtTout", "PcsTimeoutsState", "Grid Frequency Protection Timeout", "pcs", "Critical", "PCS"),
            T("PCS_OT_PROT_TOUT", "pcs_overTempProtTout", "PcsTimeoutsState", "Over Temperature Protection Timeout", "pcs", "Critical", "PCS"),
            T("PCS_PRECHARGE_TOUT", "pcs_prechargeTout", "PcsTimeoutsState", "Precharge Timeout", "pcs", "Major", "PCS"),
            T("PCS_STARTUP_TOUT", "pcs_startupTout", "PcsTimeoutsState", "Startup Timeout", "pcs", "Major", "PCS"),
            T("PCS_SHUTDOWN_TOUT", "pcs_shutdownTout", "PcsTimeoutsState", "Shutdown Timeout", "pcs", "Warning", "PCS"),
        };
    }

    private static List<DeviceAlarmType> BuildDemoPcsAlarmTypes(Guid pcsTypeId)
    {
        var now = DateTime.UtcNow;

        DeviceAlarmType T(string code, string faultKey, string name, string category, string severity)
            => new()
            {
                DeviceTypeId = pcsTypeId,
                Model = "DEMO-PCS",
                Manufacturer = "Demo",
                Code = code,
                FaultKey = faultKey,
                Kind = "PcsFaultState",
                Name = name,
                Category = category,
                Severity = severity,
                Source = "PCS",
                DetectionStrategy = "BooleanFlag",
                IsEnabled = true,
                Created = now
            };

        return new List<DeviceAlarmType>
        {
            // Hardware faults (20)
            T("PCS_HW_IGBT_OT", "hwIgbtOverTemp", "IGBT Over Temperature", "hardware", "Critical"),
            T("PCS_HW_FAN1", "hwFanFault1", "Fan 1 Fault", "hardware", "Major"),
            T("PCS_HW_IGBT_A", "hwIgbtFaultA", "IGBT Fault Phase A", "hardware", "Critical"),
            T("PCS_HW_IGBT_B", "hwIgbtFaultB", "IGBT Fault Phase B", "hardware", "Critical"),
            T("PCS_HW_IGBT_C", "hwIgbtFaultC", "IGBT Fault Phase C", "hardware", "Critical"),
            T("PCS_HW_FAN2", "hwFanFault2", "Fan 2 Fault", "hardware", "Major"),
            T("PCS_HW_FUSE_A", "hwFuseFaultA", "Fuse Fault Phase A", "hardware", "Critical"),
            T("PCS_HW_FUSE_B", "hwFuseFaultB", "Fuse Fault Phase B", "hardware", "Critical"),
            T("PCS_HW_FUSE_C", "hwFuseFaultC", "Fuse Fault Phase C", "hardware", "Critical"),
            T("PCS_HW_CT", "hwCtFault", "CT Sensor Fault", "hardware", "Major"),
            T("PCS_HW_DSP", "hwDspFault", "DSP Fault", "hardware", "Critical"),
            T("PCS_HW_AUX_PWR", "hwAuxPowerFault", "Auxiliary Power Supply Fault", "hardware", "Major"),
            T("PCS_HW_INDUCT_OT", "hwInductorOverTemp", "Inductor Over Temperature", "hardware", "Major"),
            T("PCS_HW_AMB_OT", "hwAmbientOverTemp", "Ambient Over Temperature", "hardware", "Warning"),
            T("PCS_HW_DC_RELAY", "hwDcRelayFault", "DC Relay Fault", "hardware", "Critical"),
            T("PCS_HW_PRECHARGE", "hwPreChargeRelay", "Pre-charge Relay Fault", "hardware", "Critical"),
            T("PCS_HW_SPD", "hwSurgeProtFault", "Surge Protector Fault", "hardware", "Major"),
            T("PCS_HW_XFMR", "hwTransformerFault", "Transformer Fault", "hardware", "Critical"),
            T("PCS_HW_CAP", "hwCapacitorFault", "DC Link Capacitor Fault", "hardware", "Critical"),
            T("PCS_HW_FILTER", "hwFilterFault", "EMI Filter Fault", "hardware", "Major"),

            // Grid faults (18)
            T("PCS_GRID_OV_A", "gridOverVoltageA", "Grid Over Voltage Phase A", "grid", "Major"),
            T("PCS_GRID_OV_B", "gridOverVoltageB", "Grid Over Voltage Phase B", "grid", "Major"),
            T("PCS_GRID_OV_C", "gridOverVoltageC", "Grid Over Voltage Phase C", "grid", "Major"),
            T("PCS_GRID_UV_A", "gridUnderVoltageA", "Grid Under Voltage Phase A", "grid", "Major"),
            T("PCS_GRID_UV_B", "gridUnderVoltageB", "Grid Under Voltage Phase B", "grid", "Major"),
            T("PCS_GRID_UV_C", "gridUnderVoltageC", "Grid Under Voltage Phase C", "grid", "Major"),
            T("PCS_GRID_OFREQ", "gridOverFreq", "Grid Over Frequency", "grid", "Major"),
            T("PCS_GRID_UFREQ", "gridUnderFreq", "Grid Under Frequency", "grid", "Major"),
            T("PCS_GRID_PHSEQ", "gridPhaseSeqFault", "Grid Phase Sequence Fault", "grid", "Major"),
            T("PCS_GRID_VTHD", "gridVoltageThdHigh", "Grid Voltage THD High", "grid", "Warning"),
            T("PCS_GRID_ITHD", "gridCurrentThdHigh", "Grid Current THD High", "grid", "Warning"),
            T("PCS_GRID_ISLAND", "gridIslandDetect", "Grid Island Detection", "grid", "Critical"),
            T("PCS_GRID_LOSYNC", "gridLossOfSync", "Grid Loss of Sync", "grid", "Major"),
            T("PCS_GRID_RCLOSE", "gridReclosingFault", "Grid Reclosing Fault", "grid", "Major"),
            T("PCS_GRID_VUNBAL", "gridVoltageUnbalance", "Grid Voltage Unbalance", "grid", "Warning"),
            T("PCS_GRID_IUNBAL", "gridCurrentUnbalance", "Grid Current Unbalance", "grid", "Warning"),
            T("PCS_GRID_ROCOF", "gridRocofTrip", "ROCOF Trip", "grid", "Critical"),
            T("PCS_GRID_VSHIFT", "gridVectorShift", "Vector Shift Trip", "grid", "Critical"),

            // Bus / Battery faults (11)
            T("PCS_BUS_OV", "busOverVoltage", "DC Bus Over Voltage", "bus_battery", "Critical"),
            T("PCS_BUS_UV", "busUnderVoltage", "DC Bus Under Voltage", "bus_battery", "Major"),
            T("PCS_BUS_GND", "busGroundFault", "DC Bus Ground Fault", "bus_battery", "Critical"),
            T("PCS_BUS_VUNBAL", "busVoltageUnbalance", "DC Bus Voltage Unbalance", "bus_battery", "Warning"),
            T("PCS_BAT_OV", "batOverVoltage", "Battery Over Voltage", "bus_battery", "Major"),
            T("PCS_BAT_UV", "batUnderVoltage", "Battery Under Voltage", "bus_battery", "Major"),
            T("PCS_BAT_OC", "batOverCurrent", "Battery Over Current", "bus_battery", "Critical"),
            T("PCS_BAT_RPOL", "batReversePolarity", "Battery Reverse Polarity", "bus_battery", "Critical"),
            T("PCS_BAT_INSUL", "batInsulationLow", "Battery Insulation Low", "bus_battery", "Warning"),
            T("PCS_BUS_PRECHG", "busPreChargeFail", "Bus Pre-charge Failure", "bus_battery", "Major"),
            T("PCS_BUS_SSTART", "busSoftStartFail", "Bus Soft Start Failure", "bus_battery", "Major"),

            // AC Protection faults (12)
            T("PCS_AC_OC_A", "acOverCurrentA", "AC Over Current Phase A", "ac_protection", "Major"),
            T("PCS_AC_OC_B", "acOverCurrentB", "AC Over Current Phase B", "ac_protection", "Major"),
            T("PCS_AC_OC_C", "acOverCurrentC", "AC Over Current Phase C", "ac_protection", "Major"),
            T("PCS_AC_SC", "acShortCircuit", "AC Short Circuit", "ac_protection", "Critical"),
            T("PCS_AC_OV", "acOverVoltage", "AC Output Over Voltage", "ac_protection", "Major"),
            T("PCS_AC_UV", "acUnderVoltage", "AC Output Under Voltage", "ac_protection", "Major"),
            T("PCS_AC_OPWR", "acOverPower", "AC Over Power", "ac_protection", "Major"),
            T("PCS_AC_GND", "acGroundFault", "AC Ground Fault", "ac_protection", "Critical"),
            T("PCS_AC_LEAK", "acLeakageCurrent", "AC Leakage Current High", "ac_protection", "Warning"),
            T("PCS_AC_DCI", "acDcInjection", "AC DC Injection High", "ac_protection", "Warning"),
            T("PCS_AC_INDOC", "acInductorOverCurrent", "Inductor Over Current", "ac_protection", "Major"),
            T("PCS_AC_NEUTOC", "acNeutralOverCurrent", "Neutral Over Current", "ac_protection", "Major"),

            // System faults (15)
            T("PCS_SYS_ESTOP", "sysEmergencyStop", "Emergency Stop", "system", "Critical"),
            T("PCS_SYS_SPD", "sysSpdFault", "SPD (Surge Protector) Fault", "system", "Major"),
            T("PCS_SYS_DOOR", "sysDoorOpen", "Cabinet Door Open", "system", "Warning"),
            T("PCS_SYS_FIRE", "sysFireDetect", "Fire Detection", "system", "Critical"),
            T("PCS_SYS_WATER", "sysWaterDetect", "Water Ingress Detection", "system", "Critical"),
            T("PCS_SYS_MODE", "sysModeConflict", "Operating Mode Conflict", "system", "Warning"),
            T("PCS_SYS_CFG", "sysConfigError", "Configuration Error", "system", "Warning"),
            T("PCS_SYS_FW", "sysFirmwareMismatch", "Firmware Version Mismatch", "system", "Warning"),
            T("PCS_SYS_CAL", "sysCalibrationNeeded", "Calibration Needed", "system", "Warning"),
            T("PCS_SYS_EEPROM", "sysEepromFault", "EEPROM Fault", "system", "Major"),
            T("PCS_SYS_WDG", "sysWatchdogReset", "Watchdog Reset", "system", "Major"),
            T("PCS_SYS_PARAM", "sysParamOutOfRange", "Parameter Out Of Range", "system", "Warning"),
            T("PCS_SYS_RTC", "sysRtcFault", "RTC Clock Fault", "system", "Warning"),
            T("PCS_SYS_LOG", "sysLoggingFull", "Logging Storage Full", "system", "Warning"),
            T("PCS_SYS_LIC", "sysLicenseExpired", "License Expired", "system", "Warning"),

            // Relay faults (8)
            T("PCS_REL_AC", "relayAcContactorFault", "AC Contactor Fault", "relay", "Critical"),
            T("PCS_REL_DC", "relayDcContactorFault", "DC Contactor Fault", "relay", "Critical"),
            T("PCS_REL_PRECHG", "relayPreChargeFault", "Pre-charge Relay Fault", "relay", "Critical"),
            T("PCS_REL_PID", "relayAntiPidFault", "Anti-PID Relay Fault", "relay", "Major"),
            T("PCS_REL_GND", "relayGroundFault", "Ground Relay Fault", "relay", "Critical"),
            T("PCS_REL_BYP", "relayBypassFault", "Bypass Relay Fault", "relay", "Major"),
            T("PCS_REL_XFER", "relayTransferFault", "Transfer Switch Fault", "relay", "Major"),
            T("PCS_REL_NEUT", "relayNeutralFault", "Neutral Relay Fault", "relay", "Major"),

            // Inverter faults (14)
            T("PCS_INV_SWTRIP", "invSoftwareTrip", "Inverter Software Trip", "inverter", "Major"),
            T("PCS_INV_HWTRIP", "invHardwareTrip", "Inverter Hardware Trip", "inverter", "Critical"),
            T("PCS_INV_OLOAD", "invOverload", "Inverter Overload", "inverter", "Major"),
            T("PCS_INV_EFF", "invEfficiencyLow", "Inverter Efficiency Low", "inverter", "Warning"),
            T("PCS_INV_PLL", "invPllLockFail", "PLL Lock Failure", "inverter", "Major"),
            T("PCS_INV_PWM", "invPwmFault", "PWM Generation Fault", "inverter", "Critical"),
            T("PCS_INV_DEAD", "invDeadTimeFault", "Dead Time Fault", "inverter", "Critical"),
            T("PCS_INV_MOD", "invModulationError", "Modulation Error", "inverter", "Major"),
            T("PCS_INV_DCOFS", "invDcOffsetHigh", "DC Offset High", "inverter", "Warning"),
            T("PCS_INV_START", "invStartupFail", "Startup Failure", "inverter", "Major"),
            T("PCS_INV_STOP", "invShutdownFail", "Shutdown Failure", "inverter", "Warning"),
            T("PCS_INV_MXFER", "invModeTransitionFail", "Mode Transition Failure", "inverter", "Major"),
            T("PCS_INV_ILOOP", "invCurrentLoop", "Current Loop Fault", "inverter", "Major"),
            T("PCS_INV_VLOOP", "invVoltageLoop", "Voltage Loop Fault", "inverter", "Major"),

            // Timeout faults (12)
            T("PCS_TO_BMS", "timeoutBmsComm", "BMS Communication Timeout", "timeout", "Major"),
            T("PCS_TO_EMS", "timeoutEmsComm", "EMS Communication Timeout", "timeout", "Warning"),
            T("PCS_TO_SCADA", "timeoutScadaComm", "SCADA Communication Timeout", "timeout", "Warning"),
            T("PCS_TO_METER", "timeoutMeterComm", "Meter Communication Timeout", "timeout", "Warning"),
            T("PCS_TO_RELAY", "timeoutGridRelay", "Grid Relay Response Timeout", "timeout", "Major"),
            T("PCS_TO_PRECHG", "timeoutPreCharge", "Pre-charge Timeout", "timeout", "Major"),
            T("PCS_TO_SYNC", "timeoutSyncTimeout", "Grid Sync Timeout", "timeout", "Major"),
            T("PCS_TO_START", "timeoutStartup", "Startup Timeout", "timeout", "Major"),
            T("PCS_TO_STOP", "timeoutShutdown", "Shutdown Timeout", "timeout", "Warning"),
            T("PCS_TO_MODE", "timeoutModeSwitch", "Mode Switch Timeout", "timeout", "Warning"),
            T("PCS_TO_HB", "timeoutHeartbeat", "Heartbeat Timeout", "timeout", "Major"),
            T("PCS_TO_FW", "timeoutFirmwareUpdate", "Firmware Update Timeout", "timeout", "Warning"),
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
