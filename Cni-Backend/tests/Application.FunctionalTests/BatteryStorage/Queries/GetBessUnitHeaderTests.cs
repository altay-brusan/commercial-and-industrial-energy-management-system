using Backend.Application.BatteryStorage.Queries.GetBessUnitHeader;
using Backend.Domain.Constants;
using Backend.Domain.Entities;
using Backend.Domain.Enums;

namespace Backend.Application.FunctionalTests.BatteryStorage.Queries;

public class GetBessUnitHeaderTests : BaseTestFixture
{
    [Test]
    public async Task ShouldReturnHeaderForExistingUnit()
    {
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Header Test Tenant", Identifier = "header-test-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Header Test Firm",
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
            Name = "Header Test Facility",
            EnergyCapacity = 500,
            EnergyUnit = EnergyUnit.KWh
        };
        await Testing.AddAsync(facility);

        var bessType = new DeviceType { Name = DeviceTypeNames.EnergyStorage };
        await Testing.AddAsync(bessType);

        var rtu = new Rtu
        {
            FacilityId = facility.Id,
            RtuSerialNumber = "RTU-HEADER-01"
        };
        await Testing.AddAsync(rtu);

        var device = new Device
        {
            RtuId = rtu.Id,
            DeviceTypeId = bessType.Id,
            Name = "BESS Header Unit",
            Model = "DEMO-BG",
            SerialNumber = "SN-HEADER-01",
            Manufacturer = "CATL"
        };
        await Testing.AddAsync(device);

        var result = await Testing.SendAsync(new GetBessUnitHeaderQuery { UnitId = device.Id });

        result.Should().NotBeNull();
        result!.Id.Should().Be(device.Id);
        result.Name.Should().Be("BESS Header Unit");
        result.Model.Should().Be("DEMO-BG");
        result.Brand.Should().Be("CATL");
        result.FacilityName.Should().Be("Header Test Facility");
        result.AvailableTabs.Should().NotBeEmpty();
    }

    [Test]
    public async Task ShouldThrowNotFoundForNonExistentUnit()
    {
        await Testing.RunAsAdministratorAsync();

        var act = () => Testing.SendAsync(new GetBessUnitHeaderQuery { UnitId = Guid.NewGuid() });

        await act.Should().ThrowAsync<Ardalis.GuardClauses.NotFoundException>();
    }
}
