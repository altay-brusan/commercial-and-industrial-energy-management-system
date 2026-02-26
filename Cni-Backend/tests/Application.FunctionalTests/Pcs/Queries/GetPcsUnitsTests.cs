using Backend.Application.Pcs.Queries.GetPcsUnits;
using Backend.Domain.Constants;
using Backend.Domain.Entities;
using Backend.Domain.Enums;

namespace Backend.Application.FunctionalTests.Pcs.Queries;

public class GetPcsUnitsTests : BaseTestFixture
{
    private Facility _facility = null!;
    private DeviceType _pcsType = null!;
    private Rtu _rtu = null!;

    private async Task SeedFacilityWithPcsUnit(string unitName = "PCS-01", string serial = "SN-PCS-01")
    {
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "PCS Test Tenant", Identifier = "pcs-test-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "PCS Test Firm",
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

        _facility = new Facility
        {
            FirmId = firm.Id,
            Name = "PCS Test Facility",
            EnergyCapacity = 1000,
            EnergyUnit = EnergyUnit.KWh
        };
        await Testing.AddAsync(_facility);

        _pcsType = new DeviceType { Name = DeviceTypeNames.Pcs };
        await Testing.AddAsync(_pcsType);

        _rtu = new Rtu
        {
            FacilityId = _facility.Id,
            RtuSerialNumber = "RTU-PCS-01"
        };
        await Testing.AddAsync(_rtu);

        var device = new Device
        {
            RtuId = _rtu.Id,
            DeviceTypeId = _pcsType.Id,
            Name = unitName,
            Model = "PCS-Model",
            SerialNumber = serial,
            Manufacturer = "Sungrow"
        };
        await Testing.AddAsync(device);
    }

    [Test]
    public async Task ShouldReturnUnitsForFacility()
    {
        await SeedFacilityWithPcsUnit();

        var result = await Testing.SendAsync(new GetPcsUnitsQuery
        {
            FacilityId = _facility.Id
        });

        result.Should().NotBeNull();
        result.Units.Should().HaveCount(1);
        result.Units[0].Name.Should().Be("PCS-01");
        result.Units[0].FacilityId.Should().Be(_facility.Id);
        result.Aggregates.TotalUnits.Should().Be(1);
    }

    [Test]
    public async Task ShouldReturnEmptyForFacilityWithNoUnits()
    {
        await Testing.RunAsAdministratorAsync();

        var result = await Testing.SendAsync(new GetPcsUnitsQuery
        {
            FacilityId = Guid.NewGuid()
        });

        result.Should().NotBeNull();
        result.Units.Should().BeEmpty();
        result.Aggregates.TotalUnits.Should().Be(0);
    }

    [Test]
    public async Task ShouldNotReturnNonPcsDevices()
    {
        await SeedFacilityWithPcsUnit();

        // Add a BESS device to the same facility — should not appear in PCS results
        var bessType = new DeviceType { Name = DeviceTypeNames.EnergyStorage };
        await Testing.AddAsync(bessType);

        var bessDevice = new Device
        {
            RtuId = _rtu.Id,
            DeviceTypeId = bessType.Id,
            Name = "BESS-01",
            Model = "BessModel",
            SerialNumber = "SN-BESS-01"
        };
        await Testing.AddAsync(bessDevice);

        var result = await Testing.SendAsync(new GetPcsUnitsQuery
        {
            FacilityId = _facility.Id
        });

        result.Units.Should().HaveCount(1);
        result.Units[0].Name.Should().Be("PCS-01");
    }

    [Test]
    public async Task ShouldIncludeLinkedBessUnitWhenParentIsEnergyStorage()
    {
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "PCS Link Tenant", Identifier = "pcs-link-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "PCS Link Firm",
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
            Name = "PCS Link Facility",
            EnergyCapacity = 1000,
            EnergyUnit = EnergyUnit.KWh
        };
        await Testing.AddAsync(facility);

        var bessType = new DeviceType { Name = DeviceTypeNames.EnergyStorage };
        await Testing.AddAsync(bessType);

        var pcsType = new DeviceType { Name = DeviceTypeNames.Pcs };
        await Testing.AddAsync(pcsType);

        var rtu = new Rtu
        {
            FacilityId = facility.Id,
            RtuSerialNumber = "RTU-LINK-01"
        };
        await Testing.AddAsync(rtu);

        // Create BESS parent first
        var bessDevice = new Device
        {
            RtuId = rtu.Id,
            DeviceTypeId = bessType.Id,
            Name = "BESS Container",
            Model = "BessModel",
            SerialNumber = "SN-BESS-PARENT"
        };
        await Testing.AddAsync(bessDevice);

        // Create PCS as child of BESS
        var pcsDevice = new Device
        {
            RtuId = rtu.Id,
            DeviceTypeId = pcsType.Id,
            ParentDeviceId = bessDevice.Id,
            Name = "PCS-01",
            Model = "PCS-Model",
            SerialNumber = "SN-PCS-LINKED",
            Manufacturer = "Sungrow"
        };
        await Testing.AddAsync(pcsDevice);

        var result = await Testing.SendAsync(new GetPcsUnitsQuery
        {
            FacilityId = facility.Id
        });

        result.Units.Should().HaveCount(1);
        result.Units[0].LinkedBessUnit.Should().NotBeNull();
        result.Units[0].LinkedBessUnit!.Name.Should().Be("BESS Container");
    }
}
