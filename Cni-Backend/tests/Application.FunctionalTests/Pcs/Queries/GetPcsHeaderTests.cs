using Backend.Application.Pcs.Queries.GetPcsHeader;
using Backend.Domain.Constants;
using Backend.Domain.Entities;
using Backend.Domain.Enums;

namespace Backend.Application.FunctionalTests.Pcs.Queries;

public class GetPcsHeaderTests : BaseTestFixture
{
    [Test]
    public async Task ShouldReturnHeaderForExistingUnit()
    {
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "PCS Header Tenant", Identifier = "pcs-header-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "PCS Header Firm",
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
            Name = "PCS Header Facility",
            EnergyCapacity = 500,
            EnergyUnit = EnergyUnit.KWh
        };
        await Testing.AddAsync(facility);

        var pcsType = new DeviceType { Name = DeviceTypeNames.Pcs };
        await Testing.AddAsync(pcsType);

        var rtu = new Rtu
        {
            FacilityId = facility.Id,
            RtuSerialNumber = "RTU-PCS-HEADER-01"
        };
        await Testing.AddAsync(rtu);

        var device = new Device
        {
            RtuId = rtu.Id,
            DeviceTypeId = pcsType.Id,
            Name = "PCS Header Unit",
            Model = "SC500HX",
            SerialNumber = "SN-PCS-HEADER-01",
            Manufacturer = "Sungrow"
        };
        await Testing.AddAsync(device);

        var result = await Testing.SendAsync(new GetPcsHeaderQuery { UnitId = device.Id });

        result.Should().NotBeNull();
        result!.Id.Should().Be(device.Id);
        result.Name.Should().Be("PCS Header Unit");
        result.AvailableTabs.Should().NotBeEmpty();
    }

    [Test]
    public async Task ShouldThrowNotFoundForNonExistentUnit()
    {
        await Testing.RunAsAdministratorAsync();

        var act = () => Testing.SendAsync(new GetPcsHeaderQuery { UnitId = Guid.NewGuid() });

        await act.Should().ThrowAsync<Ardalis.GuardClauses.NotFoundException>();
    }
}
