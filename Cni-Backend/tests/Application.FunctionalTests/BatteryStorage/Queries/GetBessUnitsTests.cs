using Backend.Application.BatteryStorage.Queries.GetBessUnits;
using Backend.Domain.Constants;
using Backend.Domain.Entities;
using Backend.Domain.Enums;

namespace Backend.Application.FunctionalTests.BatteryStorage.Queries;

public class GetBessUnitsTests : BaseTestFixture
{
    private Tenant _tenant = null!;
    private Facility _facility = null!;
    private DeviceType _bessType = null!;
    private Rtu _rtu = null!;

    private async Task SeedFacilityWithBessUnit(string unitName = "BESS-01", string serial = "SN-BESS-01")
    {
        await Testing.RunAsAdministratorAsync();

        _tenant = new Tenant { Name = "Test Tenant", Identifier = "test-tenant" };
        await Testing.AddAsync(_tenant);

        var firm = new Firm
        {
            TenantId = _tenant.Id,
            Name = "Test Firm",
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
            Name = "Test Facility",
            EnergyCapacity = 1000,
            EnergyUnit = EnergyUnit.KWh
        };
        await Testing.AddAsync(_facility);

        _bessType = new DeviceType { Name = DeviceTypeNames.EnergyStorage };
        await Testing.AddAsync(_bessType);

        _rtu = new Rtu
        {
            FacilityId = _facility.Id,
            RtuSerialNumber = "RTU-001"
        };
        await Testing.AddAsync(_rtu);

        var device = new Device
        {
            RtuId = _rtu.Id,
            DeviceTypeId = _bessType.Id,
            Name = unitName,
            Model = "TestModel",
            SerialNumber = serial,
            Manufacturer = "TestBrand"
        };
        await Testing.AddAsync(device);
    }

    [Test]
    public async Task ShouldReturnUnitsForFacility()
    {
        await SeedFacilityWithBessUnit();

        var result = await Testing.SendAsync(new GetBessUnitsQuery
        {
            FacilityId = _facility.Id
        });

        result.Should().NotBeNull();
        result.Units.Should().HaveCount(1);
        result.Units[0].Name.Should().Be("BESS-01");
        result.Units[0].FacilityId.Should().Be(_facility.Id);
        result.Aggregates.TotalUnits.Should().Be(1);
    }

    [Test]
    public async Task ShouldReturnEmptyForFacilityWithNoUnits()
    {
        await Testing.RunAsAdministratorAsync();

        var result = await Testing.SendAsync(new GetBessUnitsQuery
        {
            FacilityId = Guid.NewGuid()
        });

        result.Should().NotBeNull();
        result.Units.Should().BeEmpty();
        result.Aggregates.TotalUnits.Should().Be(0);
    }

    [Test]
    public async Task ShouldFilterBySearch()
    {
        await SeedFacilityWithBessUnit("Alpha Unit", "SN-ALPHA-01");

        // Add a second unit
        var device2 = new Device
        {
            RtuId = _rtu.Id,
            DeviceTypeId = _bessType.Id,
            Name = "Beta Unit",
            Model = "TestModel",
            SerialNumber = "SN-BETA-01",
            Manufacturer = "TestBrand"
        };
        await Testing.AddAsync(device2);

        var result = await Testing.SendAsync(new GetBessUnitsQuery
        {
            FacilityId = _facility.Id,
            Search = "Alpha"
        });

        result.Units.Should().HaveCount(1);
        result.Units[0].Name.Should().Be("Alpha Unit");
    }

    [Test]
    public async Task ShouldSortByName()
    {
        await SeedFacilityWithBessUnit("Zebra Unit", "SN-Z-01");

        var device2 = new Device
        {
            RtuId = _rtu.Id,
            DeviceTypeId = _bessType.Id,
            Name = "Alpha Unit",
            Model = "TestModel",
            SerialNumber = "SN-A-01",
            Manufacturer = "TestBrand"
        };
        await Testing.AddAsync(device2);

        var resultAsc = await Testing.SendAsync(new GetBessUnitsQuery
        {
            FacilityId = _facility.Id,
            SortBy = "name",
            SortDirection = "asc"
        });

        resultAsc.Units.Should().HaveCount(2);
        resultAsc.Units[0].Name.Should().Be("Alpha Unit");
        resultAsc.Units[1].Name.Should().Be("Zebra Unit");
    }
}
