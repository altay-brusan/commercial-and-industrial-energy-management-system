using Backend.Application.Facilities.Queries.GetFacility;
using Backend.Domain.Entities;
using Backend.Domain.Enums;

namespace Backend.Application.FunctionalTests.Facilities.Queries;

public class GetFacilityTests : BaseTestFixture
{
    [Test]
    public async Task ShouldReturnFacility()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();
        var tenant = new Tenant { Name = "Demo Get Facility Holding", Identifier = "demo-get-facility-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Get Facility Firm BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Noord-Holland",
            City = "Amsterdam",
            Address = "Prinsengracht 500",
            ContactEmail = "info@demo-get-facility.nl",
            ContactPhone = "+31201234567",
            TaxNumber = "NL123456789B01",
            TaxOffice = "Amsterdam"
        };
        await Testing.AddAsync(firm);

        var facility = new Facility
        {
            FirmId = firm.Id,
            Name = "Demo Solar Farm Flevoland",
            Address = "Flevopolder 123, Dronten",
            EnergyCapacity = 5000,
            EnergyUnit = EnergyUnit.KWh,
            Longitude = 5.7167,
            Latitude = 52.5167
        };
        await Testing.AddAsync(facility);

        // Act
        var result = await Testing.SendAsync(new GetFacilityQuery { Id = facility.Id });

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(facility.Id);
        result.Name.Should().Be("Demo Solar Farm Flevoland");
        result.Address.Should().Be("Flevopolder 123, Dronten");
        result.EnergyCapacity.Should().Be(5000);
    }

    [Test]
    public async Task ShouldReturnNullForNonExistentFacility()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        // Act
        var result = await Testing.SendAsync(new GetFacilityQuery { Id = 99999 });

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task ShouldIncludeFirmInformation()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();
        var tenant = new Tenant { Name = "Demo Firm Info Holding", Identifier = "demo-firm-info-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Green Energy Corp BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Zuid-Holland",
            City = "Den Haag",
            Address = "Binnenhof 1",
            ContactEmail = "info@demo-green-energy-corp.nl",
            ContactPhone = "+31701234567",
            TaxNumber = "NL555555555B01",
            TaxOffice = "Den Haag"
        };
        await Testing.AddAsync(firm);

        var facility = new Facility
        {
            FirmId = firm.Id,
            Name = "Demo Wind Park Westland"
        };
        await Testing.AddAsync(facility);

        // Act
        var result = await Testing.SendAsync(new GetFacilityQuery { Id = facility.Id });

        // Assert
        result.Should().NotBeNull();
        result!.FirmId.Should().Be(firm.Id);
    }
}
