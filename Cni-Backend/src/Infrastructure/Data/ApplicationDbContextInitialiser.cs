using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Backend.Domain.Constants;
using Backend.Domain.Entities;
using Backend.Domain.Entities.Topology;
using Backend.Domain.Enums;
using FluentValidation.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backend.Infrastructure.Data;

public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.InitialiseAsync();

        await initialiser.SeedAsync();
    }
}

public class ApplicationDbContextInitialiser
{
    private readonly ILogger<ApplicationDbContextInitialiser> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task InitialiseAsync()
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
            _logger.LogError(ex, "An error occurred while initialising the database.");
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
        var existingTenant = await _context.Tenants.IgnoreQueryFilters().FirstOrDefaultAsync();
        if (existingTenant == null)
        {
            var tenant = new Tenant
            {
                Identifier = "default",
                Name = "Default Tenant",
                Created = DateTime.UtcNow
            };
            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();
            tenantId = tenant.Id;
            _logger.LogInformation("Default tenant created with ID {TenantId}", tenantId);
        }
        else
        {
            tenantId = existingTenant.Id;
            _logger.LogInformation("Using existing tenant with ID {TenantId}", tenantId);
        }

        // Create Firm (required for Facility foreign key)
        // Use IgnoreQueryFilters to check ALL firms regardless of current tenant context
        var existingFirm = await _context.Firms.IgnoreQueryFilters().FirstOrDefaultAsync();
        if (existingFirm == null)
        {
            var firm = new Firm
            {
                TenantId = tenantId,
                Name = "Default Firm",
                Type = "Default",
                Country = "Default",
                State = "",
                City = "",
                District = "",
                PostalCode = "",
                Address = "",
                ContactEmail = "",
                ContactPhone = "",
                Fax = "",
                TaxNumber = "",
                TaxOffice = "",
                Created = DateTime.UtcNow
            };
            _context.Firms.Add(firm);
            await _context.SaveChangesAsync();
            firmId = firm.Id;
            _logger.LogInformation("Default firm created with ID {FirmId}", firmId);
        }
        else
        {
            firmId = existingFirm.Id;
            _logger.LogInformation("Using existing firm with ID {FirmId}", firmId);
        }

        // Create Facility (required for Device foreign key)
        // Use IgnoreQueryFilters to check ALL facilities regardless of current tenant context
        var existingFacility = await _context.Facilities.IgnoreQueryFilters().FirstOrDefaultAsync();
        if (existingFacility == null)
        {
            var facility = new Facility
            {
                TenantId = tenantId,
                FirmId = firmId,
                Name = "Default Facility",
                Address = "Default Address",
                Capacity = 100,
                Longitude = 0,
                Latitude = 0,
                GrpcStatus = true,
                GrpcChannel = "",
                Created = DateTime.UtcNow
            };
            _context.Facilities.Add(facility);
            await _context.SaveChangesAsync();
            facilitiyId = facility.Id;
            _logger.LogInformation("Default facility created with ID {FacilityId}", facilitiyId);
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
            Description = "Default system administrator account"
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

                _logger.LogInformation("Administrator user created successfully");
            }
            else
            {
                _logger.LogWarning("Failed to create administrator user: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        #endregion

        #region DeviceConf


        var deviceTypes = new List<DeviceType>
        {
              new DeviceType
            {
                Name = "Bess",
                HasGrpcStream = true,
                IsDataCollectedViaOtherDevice = false,
                Created = DateTime.UtcNow // BaseAuditableEntity için
            },
            new DeviceType
            {
                Name = "SolarPanel",
                HasGrpcStream = true,
                IsDataCollectedViaOtherDevice = false,
                Created = DateTime.UtcNow
            },
            new DeviceType
            {
                Name = "PCS",
                HasGrpcStream = true,
                IsDataCollectedViaOtherDevice = false,
                Created = DateTime.UtcNow
            },
            new DeviceType
            {
                Name = "SmartMeter",
                HasGrpcStream = true,
                IsDataCollectedViaOtherDevice = false,
                Created = DateTime.UtcNow
            },
            new DeviceType
            {
                Name = "PPC",
                HasGrpcStream = true,
                IsDataCollectedViaOtherDevice = false,
                Created = DateTime.UtcNow
            },
            new DeviceType
            {
                Name = "EV Charger",
                HasGrpcStream = false,
                IsDataCollectedViaOtherDevice = true,
                Created = DateTime.UtcNow
            },
            new DeviceType
            {
                Name = "Heat Pump",
                HasGrpcStream = false,
                IsDataCollectedViaOtherDevice = true,
                Created = DateTime.UtcNow
            },
            new DeviceType
            {
                Name = "Generator",
                HasGrpcStream = false,
                IsDataCollectedViaOtherDevice = false,
                Created = DateTime.UtcNow
            }, new DeviceType
            {
                Name = "Grid",
                HasGrpcStream = false,
                IsDataCollectedViaOtherDevice = true,
                Created = DateTime.UtcNow
            }, new DeviceType
            {
                Name = "MPTT",
                HasGrpcStream = false,
                IsDataCollectedViaOtherDevice = true,
                Created = DateTime.UtcNow
            },new DeviceType
            {
                Name = "FacilitySmartMeter",
                HasGrpcStream = true,
                IsDataCollectedViaOtherDevice = false,
                Created = DateTime.UtcNow
            }

        };
        if (!_context.DeviceTypes.Any())
        {
            _context.DeviceTypes.AddRange(deviceTypes);
            await _context.SaveChangesAsync();
        }

        var devies = new List<Device> {
            new Device
            {
                TenantId = 1,
                FacilityId = facilitiyId,
                DeviceTypeId = 1,
                MachineId = "1",
                DeviceName = "BESS-1",
                GrpcConnection = true,
                GrpcDeviceStatus = 0,
                GrpcLastDataReviceDate = null,
            },

            new Device
            {
                TenantId = 1,
                FacilityId = facilitiyId,
                DeviceTypeId = 2,
                MachineId = "1",
                DeviceName = "PV-1",
                GrpcConnection = true,
                GrpcDeviceStatus = 0,
                GrpcLastDataReviceDate = null,
            },


            new Device
            {
                TenantId = 1,
                FacilityId = facilitiyId,
                DeviceTypeId = 3,
                MachineId = "1",
                DeviceName = "PCS",
                GrpcConnection = true,
                GrpcDeviceStatus = 0,
                GrpcLastDataReviceDate = null,
            },
            new Device
            {
                TenantId = 1,
                FacilityId = facilitiyId,
                DeviceTypeId = 4,
                MachineId = "1",
                DeviceName = "SM",
                GrpcConnection = true,
                GrpcDeviceStatus = 0,
                GrpcLastDataReviceDate = null,
            },
            new Device
            {
                TenantId = 1,
                FacilityId = facilitiyId,
                DeviceTypeId = 11,
                MachineId = "2",
                DeviceName = "FacilitySmartMeter",
                GrpcConnection = true,
                GrpcDeviceStatus = 0,
                GrpcLastDataReviceDate = null,
            },   new Device
            {
                TenantId = 1,
                FacilityId = facilitiyId,
                DeviceTypeId = 4,
                MachineId = "3",
                DeviceName = "SM",
                GrpcConnection = false,
                GrpcDeviceStatus = 0,
                GrpcLastDataReviceDate = null,
            },   new Device
            {
                TenantId = 1,
                FacilityId = facilitiyId,
                DeviceTypeId = 4,
                MachineId = "4",
                DeviceName = "SM",
                GrpcConnection = false,
                GrpcDeviceStatus = 0,
                GrpcLastDataReviceDate = null,
            },
            new Device
            {
                TenantId = 1,
                FacilityId = facilitiyId,
                DeviceTypeId = 8,
                MachineId = "1",
                DeviceName = "Generator",
                GrpcConnection = true,
                GrpcDeviceStatus = 0,
                GrpcLastDataReviceDate = null,
            }

    };

        if (!_context.Devices.Any())
        {
            _context.Devices.AddRange(devies);
            await _context.SaveChangesAsync();
        }
        var nonstreamableDevices = new List<Device> {
             new Device
            {
                TenantId = 1,
                FacilityId = facilitiyId,
                DeviceTypeId = 10,
                MachineId = "MPTT",
                DeviceName = "MPTT",
                GrpcConnection = false,
                GrpcDeviceStatus = 0,
                GrpcLastDataReviceDate = null,
                SourceDeviceId=2

            }
            ,new Device
            {
                TenantId = 1,
                FacilityId = facilitiyId,
                DeviceTypeId = 9,
                MachineId = "Grid",
                DeviceName = "Grid",
                GrpcConnection = false,
                GrpcDeviceStatus = 0,
                GrpcLastDataReviceDate = null,
                SourceDeviceId=4


            },//arada facility var
                new Device
            {
                TenantId = 1,
                FacilityId = facilitiyId,
                DeviceTypeId = 6,
                MachineId = "EV Charger",
                DeviceName = "EV Charger",
                GrpcConnection = false,
                GrpcDeviceStatus = 0,
                GrpcLastDataReviceDate = null,
                SourceDeviceId=6
            },
            new Device
            {
                TenantId = 1,
                FacilityId = facilitiyId,
                DeviceTypeId = 7,
                MachineId = "Heat Pump",
                DeviceName = "Heat Pump",
                GrpcConnection = false,
                GrpcDeviceStatus = 0,
                GrpcLastDataReviceDate = null,
                SourceDeviceId=7
            }

        };
        if (!_context.Devices.Include(d => d.DeviceType).Any(d => d.DeviceType!.IsDataCollectedViaOtherDevice))
        {
            _context.Devices.AddRange(nonstreamableDevices);
            await _context.SaveChangesAsync();
        }
        #endregion


        #region topology 

        var parentNodes = new List<TopologyNode>
        {
            // Grup Node'ları
            new TopologyNode
            {
                DeviceId = null,
                TenantId = 1,
                Type = TopologyNodeTypes.labeledGroupNode,
                Extent = null,
                ParentId = null,
                TargetHandleCount = null,
                SourceHandleCount = null,
                Data = "{\"id\": \"bess-group\", \"label\": \"Energy Storage Units\"}",
                Position = "{\"x\": 150, \"y\": -200}",
                Style = "{\"width\": 420, \"height\": 300, \"backgroundColor\": \"transparent\", \"borderStyle\": \"dashed\", \"fontSize\": \"1.2rem\", \"fontWeight\": \"600\"}",
                FacilityId = facilitiyId
            },
            new TopologyNode
            {
                DeviceId = null,
                TenantId = 1,
                Type = TopologyNodeTypes.labeledGroupNode,
                Extent = null,
                ParentId = null,
                TargetHandleCount = null,
                SourceHandleCount = null,
                Data = "{\"id\": \"solar-gess-group\", \"label\": \"PV\"}",
                Position = "{\"x\": 830, \"y\": -200}",
                Style = "{\"width\": 500, \"height\": 300, \"backgroundColor\": \"transparent\", \"borderStyle\": \"dashed\", \"fontSize\": \"1.2rem\", \"fontWeight\": \"600\"}",
                FacilityId = facilitiyId
            },
            new TopologyNode
            {
                DeviceId = null,
                TenantId = 1,
                Type = TopologyNodeTypes.labeledGroupNode,
                Extent = null,
                ParentId = null,
                TargetHandleCount = null,
                SourceHandleCount = null,
                Data = "{\"id\": \"grid-group\", \"label\": \"Grid\"}",
                Position = "{\"x\": -375, \"y\": 800}",
                Style = "{\"width\": 450, \"height\": 400, \"backgroundColor\": \"transparent\", \"borderStyle\": \"dashed\", \"fontSize\": \"1.2rem\", \"fontWeight\": \"600\"}",
                FacilityId = facilitiyId
            },
            new TopologyNode
            {
                DeviceId = null,
                TenantId = 1,
                Type = TopologyNodeTypes.labeledGroupNode,
                Extent = null,
                ParentId = null,
                TargetHandleCount = null,
                SourceHandleCount = null,
                Data = "{\"id\": \"generator-group\", \"label\": \"Generator\"}",
                Position = "{\"x\": 112.5, \"y\": 800}",
                Style = "{\"width\": 380, \"height\": 400, \"backgroundColor\": \"transparent\", \"borderStyle\": \"dashed\", \"fontSize\": \"1.2rem\", \"fontWeight\": \"600\"}",
                FacilityId = facilitiyId
            },
            new TopologyNode
            {
                DeviceId = null,
                TenantId = 1,
                Type = TopologyNodeTypes.labeledGroupNode,
                Extent = null,
                ParentId = null,
                TargetHandleCount = null,
                SourceHandleCount = null,
                Data = "{\"id\": \"demand-group\", \"label\": \"Demand\"}",
                Position = "{\"x\": 528, \"y\": 800}",
                Style = "{\"width\": 1350, \"height\": 400, \"backgroundColor\": \"transparent\", \"borderStyle\": \"dashed\", \"fontSize\": \"1.2rem\", \"fontWeight\": \"600\"}",
                FacilityId = facilitiyId
            },
        
            // Hiçbir gruba bağlı olmayan node'lar
            new TopologyNode
            {
                DeviceId = null,
                TenantId = 1,
                Type = TopologyNodeTypes.lineNode,
                Extent = null,
                ParentId = null,
                TargetHandleCount = 2,
                SourceHandleCount = 4,
                Data = "{\"id\": \"ac-line\", \"value\": 500}",
                Position = "{\"x\": -150, \"y\": 535}",
                Style = "{\"width\": 1800}",
                FacilityId = facilitiyId
            },
            new TopologyNode
            {
                DeviceId = null,
                TenantId = 1,
                Type = TopologyNodeTypes.keyNode,
                Extent = null,
                ParentId = null,
                TargetHandleCount = null,
                SourceHandleCount = null,
                Data = "{\"id\": \"key-1\", \"status\": \"opened\"}",
                Position = "{\"x\": 355, \"y\": 220}",
                Style = null,
                FacilityId = facilitiyId
            },
            new TopologyNode
            {
                DeviceId = null,
                TenantId = 1,
                Type = TopologyNodeTypes.keyNode,
                Extent = null,
                ParentId = null,
                TargetHandleCount = null,
                SourceHandleCount = null,
                Data = "{\"id\": \"key-5\", \"status\": \"closed\"}",
                Position = "{\"x\": 1076, \"y\": 220}",
                Style = null,
                FacilityId = facilitiyId
            },
            new TopologyNode
            {
                DeviceId = null,
                TenantId = 1,
                Type = TopologyNodeTypes.keyNode,
                Extent = null,
                ParentId = null,
                TargetHandleCount = null,
                SourceHandleCount = null,
                Data = "{\"id\": \"key-6\", \"status\": \"opened\"}",
                Position = "{\"x\": 294, \"y\": 620}",
                Style = null,
                FacilityId = facilitiyId
            },
            new TopologyNode
            {
                DeviceId = 9,
                TenantId = 1,
                Type = TopologyNodeTypes.transformerNode,
                Extent = null,
                ParentId = null,
                TargetHandleCount = null,
                SourceHandleCount = null,
                Data = "{\"id\": \"transformer-5\", \"label\": \"E2.5\", \"value\": \"22 kW\"}",
                Position = "{\"x\": 1028, \"y\": 100}",
                Style = null,
                FacilityId = facilitiyId
            },
            new TopologyNode
            {
                DeviceId = null,
                TenantId = 1,
                Type = TopologyNodeTypes.capacitorNode,
                Extent = null,
                ParentId = null,
                TargetHandleCount = null,
                SourceHandleCount = null,
                Data = "{\"id\": \"capacitor-1\", \"label\": \"Grid Transformator\", \"value\": \"Temperature: 60°C\"}",
                Position = "{\"x\": -170, \"y\": 700}",
                Style = null,
                FacilityId = facilitiyId
            },
            new TopologyNode
            {
                DeviceId = null,
                TenantId = 1,
                Type = TopologyNodeTypes.capacitorNode,
                Extent = null,
                ParentId = null,
                TargetHandleCount = null,
                SourceHandleCount = null,
                Data = "{\"id\": \"capacitor-2\"}",
                Position = "{\"x\": 280, \"y\": 715}",
                Style = null,
                FacilityId = facilitiyId
            },
            new TopologyNode
            {
                DeviceId = 7,
                TenantId = 1,
                Type = TopologyNodeTypes.smartMeterNode,
                Extent = null,
                ParentId = null,
                TargetHandleCount = null,
                SourceHandleCount = null,
                Data = "{\"id\": \"smart-meter-3\"}",
                Position = "{\"x\": 1165, \"y\": 620}",
                Style = null,
                FacilityId = facilitiyId
            },
            new TopologyNode
            {
                DeviceId = 6,
                TenantId = 1,
                Type = TopologyNodeTypes.smartMeterNode,
                Extent = null,
                ParentId = null,
                TargetHandleCount = null,
                SourceHandleCount = null,
                Data = "{\"id\": \"smart-meter-4\"}",
                Position = "{\"x\": 1615, \"y\": 620}",
                Style = null,
                FacilityId = facilitiyId
            },
            new TopologyNode
            {
                DeviceId = null,
                TenantId = 1,
                Type = TopologyNodeTypes.smartMeterNode,
                Extent = null,
                ParentId = null,
                TargetHandleCount = null,
                SourceHandleCount = null,
                Data = "{\"id\": \"smart-meter-5\"}",
                Position = "{\"x\": 715, \"y\": 620}",
                Style = null,
                FacilityId = facilitiyId
            },
            new TopologyNode
            {
                DeviceId = 4,
                TenantId = 1,
                Type = TopologyNodeTypes.smartMeterNode,
                Extent = null,
                ParentId = null,
                TargetHandleCount = null,
                SourceHandleCount = null,
                Data = "{\"id\": \"smart-meter-6\"}",
                Position = "{\"x\": -185, \"y\": 580}",
                Style = null,
                FacilityId = facilitiyId
            }
        };

        if (!_context.TopologyNodes.Where(x => x.ParentId == null).Any())
        {
            _context.TopologyNodes.AddRange(parentNodes);
            await _context.SaveChangesAsync();
        }

        // Parent Id'lerini saklamak için bir sözlük
        var parentIds = parentNodes.ToDictionary(p => p.Data!, p => p.Id);

        // Child kayıtlar (parent'lara bağlı node'lar)
        var childNodes = new List<TopologyNode>
        {
            // BESS-1
            new TopologyNode
            {
                DeviceId = 1,
                TenantId = 1,
                Type = TopologyNodeTypes.bessNode,
                Extent = "parent",
                ParentId = parentIds["{\"id\": \"bess-group\", \"label\": \"Energy Storage Units\"}"],
                TargetHandleCount = null,
                SourceHandleCount = null,
                Data = "{\"id\": \"bess-1\", \"label\": \"BESS-1\", \"soc\": 82, \"temperature\": 39, \"state\": \"standby\"}",
                Position = "{\"x\": 25, \"y\": 60}",
                Style = null,
                FacilityId = facilitiyId
            },
            // PV 1
            new TopologyNode
            {
                DeviceId = 2,
                TenantId = 1,
                Type = TopologyNodeTypes.gessNode,
                Extent = "parent",
                ParentId = parentIds["{\"id\": \"solar-gess-group\", \"label\": \"PV\"}"],
                TargetHandleCount = null,
                SourceHandleCount = null,
                Data = "{\"id\": \"gess-3\", \"label\": \"PV 1\", \"performance\": 5, \"temperature\": 68, \"generation\": 100}",
                Position = "{\"x\": 30, \"y\": 40}",
                Style = null,
                FacilityId = facilitiyId
            },
            // PCS
            new TopologyNode
            {
                DeviceId = 3,
                TenantId = 1,
                Type = TopologyNodeTypes.pcsNode,
                Extent = null,
                ParentId = null,
                TargetHandleCount = null,
                SourceHandleCount = null,
                Data = "{\"id\": \"pcs\", \"powerConsumption\": 120}",
                Position = "{\"x\": 672, \"y\": 350}",
                Style = null,
                FacilityId = facilitiyId
            },
            // Grid
            new TopologyNode
            {
                DeviceId = 10,
                TenantId = 1,
                Type = TopologyNodeTypes.gridNode,
                Extent = "parent",
                ParentId = parentIds["{\"id\": \"grid-group\", \"label\": \"Grid\"}"],
                TargetHandleCount = null,
                SourceHandleCount = null,
                Data = "{\"id\": \"grid\", \"temperature\": 60}",
                Position = "{\"x\": 40, \"y\": 40}",
                Style = null,
                FacilityId = facilitiyId
            },
            // Generator
            new TopologyNode
            {
                DeviceId = 8,
                TenantId = 1,
                Type = TopologyNodeTypes.generatorNode,
                Extent = "parent",
                ParentId = parentIds["{\"id\": \"generator-group\", \"label\": \"Generator\"}"],
                TargetHandleCount = null,
                SourceHandleCount = null,
                Data = "{\"id\": \"generator\", \"generation\": 12, \"fuel\": 82}",
                Position = "{\"x\": 40, \"y\": 40}",
                Style = null,
                FacilityId = facilitiyId
            },
            // EV Charger
            new TopologyNode
            {
                DeviceId = 11,
                TenantId = 1,
                Type = TopologyNodeTypes.evChargerNode,
                Extent = "parent",
                ParentId = parentIds["{\"id\": \"demand-group\", \"label\": \"Demand\"}"],
                TargetHandleCount = null,
                SourceHandleCount = null,
                Data = "{\"id\": \"ev-charger\", \"powerConsumption\": 120}",
                Position = "{\"x\": 465, \"y\": 40}",
                Style = null,
                FacilityId = facilitiyId
            },
            // Heat Pump
            new TopologyNode
            {
                DeviceId = 12,
                TenantId = 1,
                Type = TopologyNodeTypes.heatPumpNode,
                Extent = "parent",
                ParentId = parentIds["{\"id\": \"demand-group\", \"label\": \"Demand\"}"],
                TargetHandleCount = null,
                SourceHandleCount = null,
                Data = "{\"id\": \"heat-pump\", \"powerConsumption\": 120}",
                Position = "{\"x\": 916, \"y\": 40}",
                Style = null,
                FacilityId = facilitiyId
            },
            // Facility 1
            new TopologyNode
            {
                DeviceId = 5,
                TenantId = 1,
                Type = TopologyNodeTypes.facilityNode,
                Extent = "parent",
                ParentId = parentIds["{\"id\": \"demand-group\", \"label\": \"Demand\"}"],
                TargetHandleCount = null,
                SourceHandleCount = null,
                Data = "{\"label\": \"Facility 1\", \"powerConsumption\": 120, \"powerFactor\": 0.94}",
                Position = "{\"x\": 15, \"y\": 40}",
                Style = null,
                FacilityId = facilitiyId
            }
        };

        if (!_context.TopologyNodes.Where(x => x.ParentId != null).Any())
        {
            _context.TopologyNodes.AddRange(childNodes);
            await _context.SaveChangesAsync();
        }

        // TopologyNode tablosundaki tüm kayıtları al
        var nodes = _context.TopologyNodes.ToList();

        // Node'ların Data özelliğine göre Id'lerini saklamak için bir sözlük
        var nodeIds = nodes
            .Where(n => n.Data != null)
            .ToDictionary(n => n.Data!, n => n.Id);

        // JSON'daki tüm bağlantıları TopologyEdge listesine dönüştürme
        var edges = new List<TopologyEdge>

                {
            new TopologyEdge
            {
                TenantId = 1,
                SourceId = nodeIds["{\"id\": \"bess-1\", \"label\": \"BESS-1\", \"soc\": 82, \"temperature\": 39, \"state\": \"standby\"}"],
                TargetId = nodeIds["{\"id\": \"key-1\", \"status\": \"opened\"}"],
                Type = TopologyEdgeTypes.smoothstep,
                Data = "{\"id\": \"bess-transformer-1\"}",
                FacilityId = facilitiyId,
                TargetHandle = null,
                SourceHandle = null
            },
            // e1-2
            
            // e1-10
            new TopologyEdge
            {
                TenantId = 1,
                SourceId = nodeIds["{\"id\": \"key-1\", \"status\": \"opened\"}"],
                TargetId = nodeIds["{\"id\": \"pcs\", \"powerConsumption\": 120}"],
                Type = TopologyEdgeTypes.smoothstep,
                Data = "{\"id\": \"e1-10\"}",
                FacilityId = facilitiyId,
                TargetHandle = null,
                SourceHandle = null
            },
            // gess-transformer-3
            new TopologyEdge
            {
                TenantId = 1,
                SourceId = nodeIds["{\"id\": \"gess-3\", \"label\": \"PV 1\", \"performance\": 5, \"temperature\": 68, \"generation\": 100}"],
                TargetId = nodeIds["{\"id\": \"transformer-5\", \"label\": \"E2.5\", \"value\": \"22 kW\"}"],
                Type = TopologyEdgeTypes.smoothstep,
                Data = "{\"id\": \"gess-transformer-3\"}",
                FacilityId = facilitiyId,
                TargetHandle = null,
                SourceHandle = null
            },
            // e5-pcs
            new TopologyEdge
            {
                TenantId = 1,
                SourceId = nodeIds["{\"id\": \"transformer-5\", \"label\": \"E2.5\", \"value\": \"22 kW\"}"],
                TargetId = nodeIds["{\"id\": \"key-5\", \"status\": \"closed\"}"],
                Type = TopologyEdgeTypes.smoothstep,
                Data = "{\"id\": \"e5-pcs\"}",
                FacilityId = facilitiyId,
                TargetHandle = null,
                SourceHandle = null
            },
            // e5-pcs (key to pcs)
            new TopologyEdge
            {
                TenantId = 1,
                SourceId = nodeIds["{\"id\": \"key-5\", \"status\": \"closed\"}"],
                TargetId = nodeIds["{\"id\": \"pcs\", \"powerConsumption\": 120}"],
                Type = TopologyEdgeTypes.smoothstep,
                Data = "{\"id\": \"e5-pcs\"}",
                FacilityId = facilitiyId,
                TargetHandle = null,
                SourceHandle = null
            },
            // e-ppc-grid
            new TopologyEdge
            {
                TenantId = 1,
                SourceId = nodeIds["{\"id\": \"pcs\", \"powerConsumption\": 120}"],
                TargetId = nodeIds["{\"id\": \"ac-line\", \"value\": 500}"],
                Type = TopologyEdgeTypes.smoothstep,
                Data = "{\"id\": \"e-ppc-grid\"}",
                FacilityId = facilitiyId,
                TargetHandle = "target-1",
                SourceHandle = null
            },
            // ac-line-capacitor-1
            new TopologyEdge
            {
                TenantId = 1,
                SourceId = nodeIds["{\"id\": \"ac-line\", \"value\": 500}"],
                TargetId = nodeIds["{\"id\": \"smart-meter-6\"}"],
                Type = TopologyEdgeTypes.smoothstep,
                Data = "{\"id\": \"ac-line-capacitor-1\"}",
                FacilityId = facilitiyId,
                TargetHandle = null,
                SourceHandle = "source-1"
            },
            // ac-line-capacitor-1 (smart-meter to capacitor)
            new TopologyEdge
            {
                TenantId = 1,
                SourceId = nodeIds["{\"id\": \"smart-meter-6\"}"],
                TargetId = nodeIds["{\"id\": \"capacitor-1\", \"label\": \"Grid Transformator\", \"value\": \"Temperature: 60°C\"}"],
                Type = TopologyEdgeTypes.smoothstep,
                Data = "{\"id\": \"ac-line-capacitor-1\"}",
                FacilityId = facilitiyId,
                TargetHandle = null,
                SourceHandle = null
            },
            // e-pcs-grid
            new TopologyEdge
            {
                TenantId = 1,
                SourceId = nodeIds["{\"id\": \"capacitor-1\", \"label\": \"Grid Transformator\", \"value\": \"Temperature: 60°C\"}"],
                TargetId = nodeIds["{\"id\": \"grid\", \"temperature\": 60}"],
                Type = TopologyEdgeTypes.smoothstep,
                Data = "{\"id\": \"e-pcs-grid\"}",
                FacilityId = facilitiyId,
                TargetHandle = null,
                SourceHandle = null
            },
            // e-pcs-generator
            new TopologyEdge
            {
                TenantId = 1,
                SourceId = nodeIds["{\"id\": \"ac-line\", \"value\": 500}"],
                TargetId = nodeIds["{\"id\": \"key-6\", \"status\": \"opened\"}"],
                Type = TopologyEdgeTypes.smoothstep,
                Data = "{\"id\": \"e-pcs-generator\"}",
                FacilityId = facilitiyId,
                TargetHandle = null,
                SourceHandle = "source-2"
            },
            // e-pcs-generator (ac-line to smart-meter-5)
            new TopologyEdge
            {
                TenantId = 1,
                SourceId = nodeIds["{\"id\": \"ac-line\", \"value\": 500}"],
                TargetId = nodeIds["{\"id\": \"smart-meter-5\"}"],
                Type = TopologyEdgeTypes.smoothstep,
                Data = "{\"id\": \"e-pcs-generator\"}",
                FacilityId = facilitiyId,
                TargetHandle = null,
                SourceHandle = "source-3"
            },
            // e-pcs-generator (smart-meter-5 to facility)
            new TopologyEdge
            {
                TenantId = 1,
                SourceId = nodeIds["{\"id\": \"smart-meter-5\"}"],
                TargetId = nodeIds["{\"label\": \"Facility 1\", \"powerConsumption\": 120, \"powerFactor\": 0.94}"],
                Type = TopologyEdgeTypes.smoothstep,
                Data = "{\"id\": \"e-pcs-generator\"}",
                FacilityId = facilitiyId,
                TargetHandle = null,
                SourceHandle = null
            },
            // e-pcs-generator-2
            new TopologyEdge
            {
                TenantId = 1,
                SourceId = nodeIds["{\"id\": \"key-6\", \"status\": \"opened\"}"],
                TargetId = nodeIds["{\"id\": \"capacitor-2\"}"],
                Type = TopologyEdgeTypes.smoothstep,
                Data = "{\"id\": \"e-pcs-generator-2\"}",
                FacilityId = facilitiyId,
                TargetHandle = null,
                SourceHandle = null
            },
            // e-pcs-generator-3
            new TopologyEdge
            {
                TenantId = 1,
                SourceId = nodeIds["{\"id\": \"capacitor-2\"}"],
                TargetId = nodeIds["{\"id\": \"generator\", \"generation\": 12, \"fuel\": 82}"],
                Type = TopologyEdgeTypes.smoothstep,
                Data = "{\"id\": \"e-pcs-generator-3\"}",
                FacilityId = facilitiyId,
                TargetHandle = null,
                SourceHandle = null
            },
            // e-ev-charger
            new TopologyEdge
            {
                TenantId = 1,
                SourceId = nodeIds["{\"id\": \"ac-line\", \"value\": 500}"],
                TargetId = nodeIds["{\"id\": \"smart-meter-3\"}"],
                Type = TopologyEdgeTypes.smoothstep,
                Data = "{\"id\": \"e-ev-charger\"}",
                FacilityId = facilitiyId,
                TargetHandle = null,
                SourceHandle = "source-4"
            },
            // e-ev-charger-2
            new TopologyEdge
            {
                TenantId = 1,
                SourceId = nodeIds["{\"id\": \"smart-meter-3\"}"],
                TargetId = nodeIds["{\"id\": \"ev-charger\", \"powerConsumption\": 120}"],
                Type = TopologyEdgeTypes.smoothstep,
                Data = "{\"id\": \"e-ev-charger-2\"}",
                FacilityId = facilitiyId,
                TargetHandle = null,
                SourceHandle = null
            },
            // e-heat-pump
            new TopologyEdge
            {
                TenantId = 1,
                SourceId = nodeIds["{\"id\": \"ac-line\", \"value\": 500}"],
                TargetId = nodeIds["{\"id\": \"smart-meter-4\"}"],
                Type = TopologyEdgeTypes.smoothstep,
                Data = "{\"id\": \"e-heat-pump\"}",
                FacilityId = facilitiyId,
                TargetHandle = null,
                SourceHandle = "source-5"
            },
            // e-heat-pump-2
            new TopologyEdge
            {
                TenantId = 1,
                SourceId = nodeIds["{\"id\": \"smart-meter-4\"}"],
                TargetId = nodeIds["{\"id\": \"heat-pump\", \"powerConsumption\": 120}"],
                Type = TopologyEdgeTypes.smoothstep,
                Data = "{\"id\": \"e-heat-pump-2\"}",
                FacilityId = facilitiyId,
                TargetHandle = null,
                SourceHandle = null
            }
        };


        if (!_context.TopologyEdges.Any())
        {
            _context.TopologyEdges.AddRange(edges);
            await _context.SaveChangesAsync();
        }
        #endregion

    }
}
