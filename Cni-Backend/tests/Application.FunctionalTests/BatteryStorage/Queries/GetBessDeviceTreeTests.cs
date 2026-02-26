using Backend.Application.BatteryStorage.Queries.GetBessDeviceTree;
using Backend.Domain.Constants;
using Backend.Domain.Entities;
using Backend.Domain.Enums;

namespace Backend.Application.FunctionalTests.BatteryStorage.Queries;

public class GetBessDeviceTreeTests : BaseTestFixture
{
    [Test]
    public async Task ShouldReturnTreeForFacility()
    {
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Tree Test Tenant", Identifier = "tree-test-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Tree Test Firm",
            Type = "Standard",
            Country = "TR",
            State = "Istanbul",
            City = "Istanbul",
            Address = "Test Address",
            ContactEmail = "test@test.com",
            ContactPhone = "+901234567890",
            TaxNumber = "1234567890",
            TaxOffice = "Test"
        };
        await Testing.AddAsync(firm);

        var facility = new Facility
        {
            FirmId = firm.Id,
            Name = "Tree Test Facility",
            EnergyCapacity = 1000,
            EnergyUnit = EnergyUnit.KWh
        };
        await Testing.AddAsync(facility);

        var bessType = new DeviceType { Name = DeviceTypeNames.EnergyStorage };
        await Testing.AddAsync(bessType);

        var rackType = new DeviceType { Name = DeviceTypeNames.EnergyStorageRack };
        await Testing.AddAsync(rackType);

        var rtu = new Rtu
        {
            FacilityId = facility.Id,
            RtuSerialNumber = "RTU-TREE-01"
        };
        await Testing.AddAsync(rtu);

        var parentDevice = new Device
        {
            RtuId = rtu.Id,
            DeviceTypeId = bessType.Id,
            Name = "BESS Container",
            Model = "TestModel",
            SerialNumber = "SN-TREE-01",
            Manufacturer = "TestBrand"
        };
        await Testing.AddAsync(parentDevice);

        var childDevice = new Device
        {
            RtuId = rtu.Id,
            DeviceTypeId = rackType.Id,
            ParentDeviceId = parentDevice.Id,
            Name = "Rack 1",
            Model = "RackModel",
            SerialNumber = "SN-RACK-01",
            HierarchyIndex = 0
        };
        await Testing.AddAsync(childDevice);

        var result = await Testing.SendAsync(new GetBessDeviceTreeQuery { FacilityId = facility.Id });

        result.Should().NotBeNull();
        result.Tree.Should().NotBeEmpty();
        result.Tree[0].Name.Should().Be("BESS Container");
        result.Tree[0].Children.Should().NotBeEmpty();
        result.Tree[0].Children[0].Name.Should().Be("Rack 1");
    }

    [Test]
    public async Task ShouldReturnEmptyTreeForFacilityWithNoDevices()
    {
        await Testing.RunAsAdministratorAsync();

        var result = await Testing.SendAsync(new GetBessDeviceTreeQuery { FacilityId = Guid.NewGuid() });

        result.Should().NotBeNull();
        result.Tree.Should().BeEmpty();
    }
}
