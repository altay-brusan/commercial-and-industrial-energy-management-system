using Backend.Application.Common.Exceptions;
using Backend.Application.Facilities.Commands.CreateFacility;
using Backend.Domain.Entities;
using Backend.Domain.Enums;

namespace Backend.Application.FunctionalTests.Facilities.Commands;

public class CreateFacilityTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireMinimumFields()
    {
        await Testing.RunAsAdministratorAsync();
        var command = new CreateFacilityCommand();

        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task ShouldRequireFirmId()
    {
        await Testing.RunAsAdministratorAsync();
        var command = new CreateFacilityCommand
        {
            FirmId = 0,
            Name = "Test Facility"
        };

        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task ShouldRequireName()
    {
        await Testing.RunAsAdministratorAsync();
        // First create a tenant and firm
        var tenant = new Tenant { Name = "Test Tenant", Identifier = "test-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm { TenantId = tenant.Id, Name = "Test Firm", Type = "Standard", Country = "Netherlands", State = "NH", City = "Amsterdam", Address = "Test St", ContactEmail = "test@test.com", ContactPhone = "123", TaxNumber = "123", TaxOffice = "AMS" };
        await Testing.AddAsync(firm);

        var command = new CreateFacilityCommand
        {
            FirmId = firm.Id,
            Name = ""
        };

        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task ShouldCreateFacility()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();
        var tenant = new Tenant { Name = "Test Tenant", Identifier = "test-tenant-create" };
        await Testing.AddAsync(tenant);

        var firm = new Firm { TenantId = tenant.Id, Name = "Test Firm", Type = "Standard", Country = "Netherlands", State = "NH", City = "Amsterdam", Address = "Test St", ContactEmail = "test@test.com", ContactPhone = "123", TaxNumber = "123", TaxOffice = "AMS" };
        await Testing.AddAsync(firm);

        var command = new CreateFacilityCommand
        {
            FirmId = firm.Id,
            Name = "Solar Farm Alpha",
            Address = "123 Solar Street",
            EnergyCapacity = 5000,
            EnergyUnit = EnergyUnit.KWh,
            Longitude = 4.9,
            Latitude = 52.4
        };

        // Act
        var facilityId = await Testing.SendAsync(command);

        // Assert
        var facility = await Testing.FindAsync<Facility>(facilityId);

        facility.Should().NotBeNull();
        facility!.Name.Should().Be(command.Name);
        facility.Address.Should().Be(command.Address);
        facility.EnergyCapacity.Should().Be(command.EnergyCapacity);
        facility.EnergyUnit.Should().Be(EnergyUnit.KWh);
        facility.FirmId.Should().Be(firm.Id);
        facility.Longitude.Should().Be(4.9);
        facility.Latitude.Should().Be(52.4);
    }

    [Test]
    public async Task ShouldCreateFacilityWithDifferentEnergyUnits()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();
        var tenant = new Tenant { Name = "Test Tenant MWh", Identifier = "test-tenant-mwh" };
        await Testing.AddAsync(tenant);

        var firm = new Firm { TenantId = tenant.Id, Name = "Test Firm MWh", Type = "Standard", Country = "Germany", State = "BY", City = "Munich", Address = "Test St", ContactEmail = "test@test.com", ContactPhone = "123", TaxNumber = "123", TaxOffice = "MUC" };
        await Testing.AddAsync(firm);

        var command = new CreateFacilityCommand
        {
            FirmId = firm.Id,
            Name = "Wind Farm Beta",
            EnergyCapacity = 100,
            EnergyUnit = EnergyUnit.MWh
        };

        // Act
        var facilityId = await Testing.SendAsync(command);

        // Assert
        var facility = await Testing.FindAsync<Facility>(facilityId);
        facility.Should().NotBeNull();
        facility!.EnergyUnit.Should().Be(EnergyUnit.MWh);
    }

    [Test]
    public async Task ShouldRejectNegativeEnergyCapacity()
    {
        await Testing.RunAsAdministratorAsync();
        var tenant = new Tenant { Name = "Test Tenant Neg", Identifier = "test-tenant-neg" };
        await Testing.AddAsync(tenant);

        var firm = new Firm { TenantId = tenant.Id, Name = "Test Firm", Type = "Standard", Country = "Netherlands", State = "NH", City = "Amsterdam", Address = "Test St", ContactEmail = "test@test.com", ContactPhone = "123", TaxNumber = "123", TaxOffice = "AMS" };
        await Testing.AddAsync(firm);

        var command = new CreateFacilityCommand
        {
            FirmId = firm.Id,
            Name = "Invalid Facility",
            EnergyCapacity = -100
        };

        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task ShouldRejectNameExceedingMaxLength()
    {
        await Testing.RunAsAdministratorAsync();
        var tenant = new Tenant { Name = "Test Tenant MaxLen", Identifier = "test-tenant-maxlen" };
        await Testing.AddAsync(tenant);

        var firm = new Firm { TenantId = tenant.Id, Name = "Test Firm", Type = "Standard", Country = "Netherlands", State = "NH", City = "Amsterdam", Address = "Test St", ContactEmail = "test@test.com", ContactPhone = "123", TaxNumber = "123", TaxOffice = "AMS" };
        await Testing.AddAsync(firm);

        var command = new CreateFacilityCommand
        {
            FirmId = firm.Id,
            Name = new string('A', 501) // Exceeds 500 char limit
        };

        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<ValidationException>();
    }
}
