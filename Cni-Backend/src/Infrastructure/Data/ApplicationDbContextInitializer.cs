using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Backend.Domain.Constants;
using Backend.Domain.Entities;
using Backend.Domain.Enums;
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
                          "source": { "kind": "ContainerBatteryState", "paths": ["socPercent", "stateOfCharge", "soc"] }
                        },
                        "soh": {
                          "type": "number", "unit": "%",
                          "source": { "kind": "ContainerBatteryState", "paths": ["sohPercent", "stateOfHealth"] }
                        },
                        "voltage": {
                          "type": "number", "unit": "V",
                          "source": { "kind": "ContainerBatteryState", "paths": ["totalVoltageV"] }
                        },
                        "current": {
                          "type": "number", "unit": "A",
                          "source": { "kind": "ContainerBatteryState", "paths": ["instantCurrentA"] }
                        },
                        "power": {
                          "type": "number", "unit": "kW",
                          "formula": {
                            "expression": "round(multiply(voltage, current, 0.001), 1)",
                            "inputs": { "voltage": {"$ref":"voltage"}, "current": {"$ref":"current"} }
                          },
                          "fallback": { "kind": "ContainerBatteryState", "paths": ["dcPower.power", "power"] }
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
                          "source": { "kind": "ContainerBatteryState", "paths": ["minCellTempC"] }
                        },
                        "tempMax": {
                          "type": "number", "unit": "C",
                          "source": { "kind": "ContainerBatteryState", "paths": ["maxCellTempC"] }
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
                          "source": { "kind": "ContainerBatteryState", "paths": ["maxCellVoltageMv"] }
                        },
                        "cellVoltageMin": {
                          "type": "number", "unit": "mV",
                          "source": { "kind": "ContainerBatteryState", "paths": ["minCellVoltageMv"] }
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
                          "source": { "kind": "PcsPowerState", "paths": ["pcs_outActPwrKw"] }
                        },
                        "pcsDcPower": {
                          "type": "number", "unit": "kW",
                          "source": { "kind": "PcsDcMeasState", "paths": ["pcs_dcPwrKw"] }
                        },
                        "pcsGridFreq": {
                          "type": "number", "unit": "Hz",
                          "source": { "kind": "PcsStatusState", "paths": ["pcs_gridFreqHz"] }
                        },
                        "cycleCount": {
                          "type": "number",
                          "source": { "kind": "ContainerBatteryState", "paths": ["cycleCount"] }
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
                          "source": { "kind": "ContainerBatteryState", "paths": ["state"] }
                        },
                        "remainingEnergy": {
                          "type": "number", "unit": "kWh",
                          "source": { "kind": "ContainerBatteryState", "paths": ["remainingEnergyKwh"] }
                        },
                        "availableEnergy": {
                          "type": "number", "unit": "kWh",
                          "source": { "kind": "ContainerBatteryState", "paths": ["availableEnergyKwh"] }
                        },
                        "chillerCoolantOut": {
                          "type": "number", "unit": "C",
                          "source": { "kind": "ChillerState", "paths": ["outletCoolantTempC"] }
                        },
                        "chillerCoolantIn": {
                          "type": "number", "unit": "C",
                          "source": { "kind": "ChillerState", "paths": ["inletCoolantTempC"] }
                        },
                        "compressorRunning": {
                          "type": "boolean",
                          "source": { "kind": "ChillerState", "paths": ["compressorStatus"] }
                        },
                        "ambientTemp": {
                          "type": "number", "unit": "C",
                          "source": { "kind": "ChillerState", "paths": ["ambientTempC"] }
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
            T("BATT_CELL_OV_WARN", "faultCellOverVoltageWarning", "ContainerBatteryFaultState", "Cell Over Voltage Warning", "battery", "Warning", "BMS"),
            T("BATT_CELL_UV_WARN", "faultCellUnderVoltageWarning", "ContainerBatteryFaultState", "Cell Under Voltage Warning", "battery", "Warning", "BMS"),
            T("BATT_CELL_OV_FAULT", "faultCellOverVoltageFault", "ContainerBatteryFaultState", "Cell Over Voltage Fault", "battery", "Critical", "BMS"),
            T("BATT_CELL_UV_FAULT", "faultCellUnderVoltageFault", "ContainerBatteryFaultState", "Cell Under Voltage Fault", "battery", "Critical", "BMS"),
            T("BATT_PACK_OC", "faultPackOC", "ContainerBatteryFaultState", "Pack Over Current", "battery", "Critical", "BMS"),
            T("BATT_PACK_OV", "faultPackOV", "ContainerBatteryFaultState", "Pack Over Voltage", "battery", "Major", "BMS"),
            T("BATT_PACK_UV", "faultPackUV", "ContainerBatteryFaultState", "Pack Under Voltage", "battery", "Major", "BMS"),
            T("BATT_OT_WARN", "faultOverTempWarning", "ContainerBatteryFaultState", "Over Temperature Warning", "battery", "Warning", "BMS"),
            T("BATT_OT_FAULT", "faultOverTempFault", "ContainerBatteryFaultState", "Over Temperature Fault", "battery", "Critical", "BMS"),
            T("BATT_UT_WARN", "faultUnderTempWarning", "ContainerBatteryFaultState", "Under Temperature Warning", "battery", "Warning", "BMS"),
            T("BATT_UT_FAULT", "faultUnderTempFault", "ContainerBatteryFaultState", "Under Temperature Fault", "battery", "Critical", "BMS"),
            T("BATT_INSUL_WARN", "faultInsulationWarning", "ContainerBatteryFaultState", "Insulation Warning", "battery", "Warning", "BMS"),
            T("BATT_INSUL_FAULT", "faultInsulationFault", "ContainerBatteryFaultState", "Insulation Fault", "battery", "Critical", "BMS"),
            T("BATT_VDIFF_WARN", "faultVoltageDiffWarning", "ContainerBatteryFaultState", "Voltage Difference Warning", "battery", "Warning", "BMS"),
            T("BATT_VDIFF_FAULT", "faultVoltageDiffFault", "ContainerBatteryFaultState", "Voltage Difference Fault", "battery", "Major", "BMS"),
            T("BATT_TDIFF_WARN", "faultTempDiffWarning", "ContainerBatteryFaultState", "Temperature Difference Warning", "battery", "Warning", "BMS"),
            T("BATT_TDIFF_FAULT", "faultTempDiffFault", "ContainerBatteryFaultState", "Temperature Difference Fault", "battery", "Major", "BMS"),
            T("BATT_SOC_LOW_WARN", "faultSocLowWarning", "ContainerBatteryFaultState", "SOC Low Warning", "battery", "Warning", "BMS"),
            T("BATT_SOC_LOW_FAULT", "faultSocLowFault", "ContainerBatteryFaultState", "SOC Low Fault", "battery", "Major", "BMS"),
            T("BATT_SOC_HIGH_WARN", "faultSocHighWarning", "ContainerBatteryFaultState", "SOC High Warning", "battery", "Warning", "BMS"),
            T("BATT_BMS_COMM", "faultBmsCommFailure", "ContainerBatteryFaultState", "BMS Communication Failure", "battery", "Critical", "BMS"),
            T("BATT_CONTACTOR", "faultContactorFailure", "ContainerBatteryFaultState", "Contactor Failure", "battery", "Critical", "BMS"),
            T("BATT_FUSE", "faultFuseBlown", "ContainerBatteryFaultState", "Fuse Blown", "battery", "Critical", "BMS"),
            T("BATT_CHG_OC", "faultChargeOverCurrent", "ContainerBatteryFaultState", "Charge Over Current", "battery", "Major", "BMS"),
            T("BATT_DISCHG_OC", "faultDischargeOverCurrent", "ContainerBatteryFaultState", "Discharge Over Current", "battery", "Major", "BMS"),
            T("BATT_EMRG_STOP", "faultEmergencyStop", "ContainerBatteryFaultState", "Emergency Stop", "battery", "Critical", "BMS"),
            T("BATT_PRECHARGE", "faultPrechargeFailure", "ContainerBatteryFaultState", "Precharge Failure", "battery", "Major", "BMS"),

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
            T("PCS_HW_EPO", "pcs_hwEpoFault", "PcsFaultGrp1State", "Hardware EPO Fault", "pcs", "Critical", "PCS"),
            T("PCS_GRID_A_OV", "pcs_gridAOvFault", "PcsFaultGrp1State", "Grid Phase A Over Voltage", "pcs", "Major", "PCS"),
            T("PCS_GRID_B_OV", "pcs_gridBOvFault", "PcsFaultGrp1State", "Grid Phase B Over Voltage", "pcs", "Major", "PCS"),
            T("PCS_GRID_C_OV", "pcs_gridCOvFault", "PcsFaultGrp1State", "Grid Phase C Over Voltage", "pcs", "Major", "PCS"),
            T("PCS_GRID_A_UV", "pcs_gridAUvFault", "PcsFaultGrp1State", "Grid Phase A Under Voltage", "pcs", "Major", "PCS"),
            T("PCS_GRID_B_UV", "pcs_gridBUvFault", "PcsFaultGrp1State", "Grid Phase B Under Voltage", "pcs", "Major", "PCS"),
            T("PCS_GRID_C_UV", "pcs_gridCUvFault", "PcsFaultGrp1State", "Grid Phase C Under Voltage", "pcs", "Major", "PCS"),
            T("PCS_GRID_FREQ_HI", "pcs_gridFreqHighFault", "PcsFaultGrp1State", "Grid Frequency High", "pcs", "Major", "PCS"),
            T("PCS_GRID_FREQ_LO", "pcs_gridFreqLowFault", "PcsFaultGrp1State", "Grid Frequency Low", "pcs", "Major", "PCS"),
            T("PCS_DC_OV", "pcs_dcBusOvFault", "PcsFaultGrp1State", "DC Bus Over Voltage", "pcs", "Critical", "PCS"),
            T("PCS_DC_UV", "pcs_dcBusUvFault", "PcsFaultGrp1State", "DC Bus Under Voltage", "pcs", "Major", "PCS"),
            T("PCS_IGBT_OT", "pcs_igbtOverTempFault", "PcsFaultGrp1State", "IGBT Over Temperature", "pcs", "Critical", "PCS"),
            T("PCS_AC_OC_A", "pcs_acOverCurrentAFault", "PcsFaultGrp1State", "AC Over Current Phase A", "pcs", "Major", "PCS"),
            T("PCS_AC_OC_B", "pcs_acOverCurrentBFault", "PcsFaultGrp1State", "AC Over Current Phase B", "pcs", "Major", "PCS"),
            T("PCS_AC_OC_C", "pcs_acOverCurrentCFault", "PcsFaultGrp1State", "AC Over Current Phase C", "pcs", "Major", "PCS"),
            T("PCS_DC_OC", "pcs_dcOverCurrentFault", "PcsFaultGrp1State", "DC Over Current", "pcs", "Critical", "PCS"),
            T("PCS_AMBIENT_OT", "pcs_ambientOverTempFault", "PcsFaultGrp1State", "Ambient Over Temperature", "pcs", "Warning", "PCS"),
            T("PCS_INDUCT_OT", "pcs_inductanceOverTempFault", "PcsFaultGrp1State", "Inductance Over Temperature", "pcs", "Major", "PCS"),

            // PcsFaultGrp2State — PCS protection & relay faults
            T("PCS_PREREL_CLOSE", "pcs_preRelCloseFail", "PcsFaultGrp2State", "Pre-charge Relay Close Failure", "pcs", "Critical", "PCS"),
            T("PCS_PREREL_OPEN", "pcs_preRelOpenFail", "PcsFaultGrp2State", "Pre-charge Relay Open Failure", "pcs", "Critical", "PCS"),
            T("PCS_MAINREL_CLOSE", "pcs_mainRelCloseFail", "PcsFaultGrp2State", "Main Relay Close Failure", "pcs", "Critical", "PCS"),
            T("PCS_MAINREL_OPEN", "pcs_mainRelOpenFail", "PcsFaultGrp2State", "Main Relay Open Failure", "pcs", "Critical", "PCS"),
            T("PCS_ACBKR_CLOSE", "pcs_acBreakerCloseFail", "PcsFaultGrp2State", "AC Breaker Close Failure", "pcs", "Critical", "PCS"),
            T("PCS_ACBKR_OPEN", "pcs_acBreakerOpenFail", "PcsFaultGrp2State", "AC Breaker Open Failure", "pcs", "Critical", "PCS"),
            T("PCS_GROUND_FAULT", "pcs_groundFault", "PcsFaultGrp2State", "Ground Fault", "pcs", "Critical", "PCS"),
            T("PCS_FAN_FAULT", "pcs_coolingFanFault", "PcsFaultGrp2State", "Cooling Fan Fault", "pcs", "Major", "PCS"),
            T("PCS_FUSE_FAULT", "pcs_dcFuseFault", "PcsFaultGrp2State", "DC Fuse Fault", "pcs", "Critical", "PCS"),
            T("PCS_SPD_FAULT", "pcs_surgeSpdFault", "PcsFaultGrp2State", "Surge Protector Fault", "pcs", "Major", "PCS"),
            T("PCS_PHASE_SEQ", "pcs_phaseSeqFault", "PcsFaultGrp2State", "Phase Sequence Fault", "pcs", "Major", "PCS"),
            T("PCS_SYNC_FAIL", "pcs_gridSyncFail", "PcsFaultGrp2State", "Grid Sync Failure", "pcs", "Major", "PCS"),
            T("PCS_SW_EPO", "pcs_swEpoFault", "PcsFaultGrp2State", "Software EPO Fault", "pcs", "Critical", "PCS"),
            T("PCS_COMM_BMS", "pcs_bmsCommFault", "PcsFaultGrp2State", "BMS Communication Fault", "pcs", "Major", "PCS"),
            T("PCS_COMM_EMS", "pcs_emsCommFault", "PcsFaultGrp2State", "EMS Communication Fault", "pcs", "Warning", "PCS"),

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
}
