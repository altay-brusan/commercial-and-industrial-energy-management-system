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
        var tenant = new Tenant { Name = "Get Facility Tenant", Identifier = "get-facility-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm { TenantId = tenant.Id, Name = "Get Facility Firm", Type = "Standard", Country = "Netherlands", State = "NH", City = "Amsterdam", Address = "Test St", ContactEmail = "test@test.com", ContactPhone = "123", TaxNumber = "123", TaxOffice = "AMS" };
        await Testing.AddAsync(firm);

        var facility = new Facility
        {
            FirmId = firm.Id,
            Name = "Test Solar Farm",
            Address = "123 Solar Road",
            EnergyCapacity = 5000,
            EnergyUnit = EnergyUnit.KWh,
            Longitude = 4.9,
            Latitude = 52.4
        };
        await Testing.AddAsync(facility);

        // Act
        var result = await Testing.SendAsync(new GetFacilityQuery { Id = facility.Id });

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(facility.Id);
        result.Name.Should().Be("Test Solar Farm");
        result.Address.Should().Be("123 Solar Road");
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
        var tenant = new Tenant { Name = "Firm Info Tenant", Identifier = "firm-info-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Green Energy Corp",
            Type = "Standard",
            Country = "Germany",
            State = "BY",
            City = "Munich",
            Address = "Test St",
            ContactEmail = "test@test.com",
            ContactPhone = "123",
            TaxNumber = "123",
            TaxOffice = "MUC"
        };
        await Testing.AddAsync(firm);

        var facility = new Facility
        {
            FirmId = firm.Id,
            Name = "Munich Solar Plant"
        };
        await Testing.AddAsync(facility);

        // Act
        var result = await Testing.SendAsync(new GetFacilityQuery { Id = facility.Id });

        // Assert
        result.Should().NotBeNull();
        result!.FirmId.Should().Be(firm.Id);
    }
}
