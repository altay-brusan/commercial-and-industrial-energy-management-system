using Backend.Application.Common.Exceptions;
using Backend.Application.Facilities.Commands.UpdateFacility;
using Backend.Domain.Entities;
using Backend.Domain.Enums;

namespace Backend.Application.FunctionalTests.Facilities.Commands;

public class UpdateFacilityTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireValidFacilityId()
    {
        await Testing.RunAsAdministratorAsync();
        var command = new UpdateFacilityCommand
        {
            Id = Guid.NewGuid(),
            Name = "Demo Updated Facility"
        };

        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task ShouldUpdateFacility()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();
        var tenant = new Tenant { Name = "Demo Update Facility Holding", Identifier = "demo-update-facility-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Update Facility Firm BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Noord-Holland",
            City = "Amsterdam",
            Address = "Keizersgracht 200",
            ContactEmail = "info@demo-update-facility.nl",
            ContactPhone = "+31201234567",
            TaxNumber = "NL123456789B01",
            TaxOffice = "Amsterdam"
        };
        await Testing.AddAsync(firm);

        var facility = new Facility
        {
            FirmId = firm.Id,
            Name = "Demo Original Solar Park",
            Address = "Oude Weg 1, Almere",
            EnergyCapacity = 1000,
            EnergyUnit = EnergyUnit.KWh
        };
        await Testing.AddAsync(facility);

        var command = new UpdateFacilityCommand
        {
            Id = facility.Id,
            FirmId = firm.Id,
            Name = "Demo Updated Solar Park",
            Address = "Nieuwe Weg 50, Almere",
            EnergyCapacity = 2000,
            EnergyUnit = EnergyUnit.MWh,
            Longitude = 5.2647,
            Latitude = 52.3508
        };

        // Act
        await Testing.SendAsync(command);

        // Assert
        var updatedFacility = await Testing.FindAsync<Facility>(facility.Id);

        updatedFacility.Should().NotBeNull();
        updatedFacility!.Name.Should().Be("Demo Updated Solar Park");
        updatedFacility.Address.Should().Be("Nieuwe Weg 50, Almere");
        updatedFacility.EnergyCapacity.Should().Be(2000);
        updatedFacility.EnergyUnit.Should().Be(EnergyUnit.MWh);
        updatedFacility.Longitude.Should().Be(5.2647);
        updatedFacility.Latitude.Should().Be(52.3508);
    }

    [Test]
    public async Task ShouldUpdateFacilityTopology()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();
        var tenant = new Tenant { Name = "Demo Topology Holding", Identifier = "demo-topology-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Topology Firm BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Zuid-Holland",
            City = "Rotterdam",
            Address = "Coolsingel 100",
            ContactEmail = "info@demo-topology.nl",
            ContactPhone = "+31101234567",
            TaxNumber = "NL987654321B01",
            TaxOffice = "Rotterdam"
        };
        await Testing.AddAsync(firm);

        var facility = new Facility
        {
            FirmId = firm.Id,
            Name = "Demo Topology Solar Facility",
            EnergyCapacity = 500
        };
        await Testing.AddAsync(facility);

        var topologyJson = "{\"devices\":[{\"id\":\"INV001\",\"type\":\"Inverter\"}]}";

        var command = new UpdateFacilityCommand
        {
            Id = facility.Id,
            FirmId = firm.Id,
            Name = facility.Name,
            EnergyCapacity = facility.EnergyCapacity,
            EnergyUnit = facility.EnergyUnit,
            Topology = topologyJson
        };

        // Act
        await Testing.SendAsync(command);

        // Assert
        var updatedFacility = await Testing.FindAsync<Facility>(facility.Id);
        updatedFacility.Should().NotBeNull();
        updatedFacility!.Topology.Should().Be(topologyJson);
    }

    [Test]
    public async Task ShouldRejectEmptyName()
    {
        await Testing.RunAsAdministratorAsync();
        var tenant = new Tenant { Name = "Demo Empty Name Holding", Identifier = "demo-empty-name-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Empty Name Firm BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Noord-Holland",
            City = "Amsterdam",
            Address = "Herengracht 300",
            ContactEmail = "info@demo-empty-name.nl",
            ContactPhone = "+31201234567",
            TaxNumber = "NL123456789B01",
            TaxOffice = "Amsterdam"
        };
        await Testing.AddAsync(firm);

        var facility = new Facility { FirmId = firm.Id, Name = "Demo Test Facility", EnergyCapacity = 100 };
        await Testing.AddAsync(facility);

        var command = new UpdateFacilityCommand
        {
            Id = facility.Id,
            FirmId = firm.Id,
            Name = "",
            EnergyCapacity = 100
        };

        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<ValidationException>();
    }
}
