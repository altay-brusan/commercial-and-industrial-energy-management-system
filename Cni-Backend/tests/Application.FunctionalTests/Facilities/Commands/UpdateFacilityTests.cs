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
            Id = 99999,
            Name = "Updated Name"
        };

        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task ShouldUpdateFacility()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();
        var tenant = new Tenant { Name = "Update Test Tenant", Identifier = "update-test-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm { TenantId = tenant.Id, Name = "Update Test Firm", Type = "Standard", Country = "Netherlands", State = "NH", City = "Amsterdam", Address = "Test St", ContactEmail = "test@test.com", ContactPhone = "123", TaxNumber = "123", TaxOffice = "AMS" };
        await Testing.AddAsync(firm);

        var facility = new Facility
        {
            FirmId = firm.Id,
            Name = "Original Name",
            Address = "Original Address",
            EnergyCapacity = 1000,
            EnergyUnit = EnergyUnit.KWh
        };
        await Testing.AddAsync(facility);

        var command = new UpdateFacilityCommand
        {
            Id = facility.Id,
            FirmId = firm.Id,
            Name = "Updated Name",
            Address = "Updated Address",
            EnergyCapacity = 2000,
            EnergyUnit = EnergyUnit.MWh,
            Longitude = 5.0,
            Latitude = 53.0
        };

        // Act
        await Testing.SendAsync(command);

        // Assert
        var updatedFacility = await Testing.FindAsync<Facility>(facility.Id);

        updatedFacility.Should().NotBeNull();
        updatedFacility!.Name.Should().Be("Updated Name");
        updatedFacility.Address.Should().Be("Updated Address");
        updatedFacility.EnergyCapacity.Should().Be(2000);
        updatedFacility.EnergyUnit.Should().Be(EnergyUnit.MWh);
        updatedFacility.Longitude.Should().Be(5.0);
        updatedFacility.Latitude.Should().Be(53.0);
    }

    [Test]
    public async Task ShouldUpdateFacilityTopology()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();
        var tenant = new Tenant { Name = "Topology Test Tenant", Identifier = "topology-test-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm { TenantId = tenant.Id, Name = "Topology Test Firm", Type = "Standard", Country = "Germany", State = "BY", City = "Munich", Address = "Test St", ContactEmail = "test@test.com", ContactPhone = "123", TaxNumber = "123", TaxOffice = "MUC" };
        await Testing.AddAsync(firm);

        var facility = new Facility
        {
            FirmId = firm.Id,
            Name = "Facility With Topology",
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
        var tenant = new Tenant { Name = "Empty Name Tenant", Identifier = "empty-name-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm { TenantId = tenant.Id, Name = "Test Firm", Type = "Standard", Country = "Netherlands", State = "NH", City = "Amsterdam", Address = "Test St", ContactEmail = "test@test.com", ContactPhone = "123", TaxNumber = "123", TaxOffice = "AMS" };
        await Testing.AddAsync(firm);

        var facility = new Facility { FirmId = firm.Id, Name = "Test Facility", EnergyCapacity = 100 };
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
