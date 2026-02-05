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
            // Check if using SQLite (typically for tests) - use EnsureCreated instead of Migrate
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

        int tenantId = 1;
        int firmId = 1;
        int facilitiyId = 1;

        // Create Tenant first (required for multi-tenancy)
        // Use IgnoreQueryFilters to check ALL tenants regardless of current tenant context
        var existingTenant = await _context.Tenants.IgnoreQueryFilters().OrderBy(x => x.Id).FirstOrDefaultAsync();
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
        var existingFirm = await _context.Firms.IgnoreQueryFilters().OrderBy(x => x.Id).FirstOrDefaultAsync();
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
        var existingFacility = await _context.Facilities.IgnoreQueryFilters().OrderBy(x => x.Id).FirstOrDefaultAsync();
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

        #endregion

        #region DeviceConf

        var deviceTypes = new List<DeviceType>
        {
            new DeviceType { Name = "Solar", Created = DateTime.UtcNow },
            new DeviceType { Name = "Generator", Created = DateTime.UtcNow },
            new DeviceType { Name = "Wind Turbine", Created = DateTime.UtcNow },
            new DeviceType { Name = "Battery", Created = DateTime.UtcNow },
            new DeviceType { Name = "Inverter", Created = DateTime.UtcNow },
            new DeviceType { Name = "PCS", Created = DateTime.UtcNow },
            new DeviceType { Name = "Smart Meter", Created = DateTime.UtcNow },
            new DeviceType { Name = "Grid", Created = DateTime.UtcNow },
            new DeviceType { Name = "DC Bus", Created = DateTime.UtcNow },
            new DeviceType { Name = "Transformer", Created = DateTime.UtcNow },
            new DeviceType { Name = "Load", Created = DateTime.UtcNow },
            new DeviceType { Name = "EV Charger", Created = DateTime.UtcNow },
            new DeviceType { Name = "Heat Pump", Created = DateTime.UtcNow }
        };
        if (!_context.DeviceTypes.Any())
        {
            _context.DeviceTypes.AddRange(deviceTypes);
            await _context.SaveChangesAsync();
        }

        var devices = new List<Device>
        {
            new Device
            {
                FacilityId = facilitiyId,
                DeviceTypeId = 4, // Battery
                Name = "BESS_001",
                Model = "Tesla Megapack",
                SerialNumber = "BESS-SN-001",
                Manufacturer = "Tesla",
                Ports = "[{\"id\":\"DC\",\"type\":\"DC\",\"config\":{\"voltage_max\":1500,\"current_max\":100}}]"
            },
            new Device
            {
                FacilityId = facilitiyId,
                DeviceTypeId = 1, // Solar
                Name = "PV_001",
                Model = "SunPower Maxeon 6",
                SerialNumber = "PV-SN-001",
                Manufacturer = "SunPower",
                Ports = "[{\"id\":\"DC\",\"type\":\"DC\",\"config\":{\"voltage_max\":600,\"current_max\":15}}]"
            },
            new Device
            {
                FacilityId = facilitiyId,
                DeviceTypeId = 5, // Inverter
                Name = "INVERTER_001",
                Model = "SMA Sunny Tripower",
                SerialNumber = "INV-SN-001",
                Manufacturer = "SMA",
                Ports = "[{\"id\":\"Grid\",\"type\":\"AC\",\"config\":{\"voltage\":230,\"frequency\":50}},{\"id\":\"MPPT\",\"type\":\"DC\",\"config\":{\"voltage_max\":1000}}]"
            },
            new Device
            {
                FacilityId = facilitiyId,
                DeviceTypeId = 7, // Smart Meter
                Name = "SM_001",
                Model = "Schneider PM5560",
                SerialNumber = "SM-SN-001",
                Manufacturer = "Schneider Electric",
                Ports = "[{\"id\":\"Main\",\"type\":\"AC\",\"config\":{\"voltage\":230,\"frequency\":50}}]"
            },
            new Device
            {
                FacilityId = facilitiyId,
                DeviceTypeId = 8, // Grid
                Name = "GRID_001",
                Model = "Grid Connection",
                SerialNumber = "GRID-SN-001",
                Ports = "[{\"id\":\"Main\",\"type\":\"AC\",\"config\":{\"voltage\":230,\"frequency\":50}}]"
            },
            new Device
            {
                FacilityId = facilitiyId,
                DeviceTypeId = 2, // Generator
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
}
