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
            FirmId = Guid.Empty,
            Name = "Demo Solar Facility"
        };

        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task ShouldRequireName()
    {
        await Testing.RunAsAdministratorAsync();
        var tenant = new Tenant { Name = "Demo Facility Validation Holding", Identifier = "demo-facility-validation-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Validation Firm BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Noord-Holland",
            City = "Amsterdam",
            Address = "Keizersgracht 123",
            ContactEmail = "info@demo-validation.nl",
            ContactPhone = "+31201234567",
            TaxNumber = "NL123456789B01",
            TaxOffice = "Amsterdam"
        };
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
        var tenant = new Tenant { Name = "Demo Solar Holding", Identifier = "demo-solar-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Solar Energy BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Noord-Holland",
            City = "Amsterdam",
            Address = "Herengracht 456",
            ContactEmail = "info@demo-solar-energy.nl",
            ContactPhone = "+31201234567",
            TaxNumber = "NL123456789B01",
            TaxOffice = "Amsterdam"
        };
        await Testing.AddAsync(firm);

        var command = new CreateFacilityCommand
        {
            FirmId = firm.Id,
            Name = "Demo Solar Park Zeewolde",
            Address = "Zeewolderweg 100, Zeewolde",
            EnergyCapacity = 5000,
            EnergyUnit = EnergyUnit.KWh,
            Longitude = 5.5422,
            Latitude = 52.3274
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
        facility.Longitude.Should().Be(5.5422);
        facility.Latitude.Should().Be(52.3274);
    }

    [Test]
    public async Task ShouldCreateFacilityWithDifferentEnergyUnits()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();
        var tenant = new Tenant { Name = "Demo Wind Holding", Identifier = "demo-wind-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Wind Power BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Flevoland",
            City = "Lelystad",
            Address = "Stadhuisplein 2",
            ContactEmail = "info@demo-wind-power.nl",
            ContactPhone = "+31320123456",
            TaxNumber = "NL987654321B01",
            TaxOffice = "Lelystad"
        };
        await Testing.AddAsync(firm);

        var command = new CreateFacilityCommand
        {
            FirmId = firm.Id,
            Name = "Demo Wind Park Noordoostpolder",
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
        var tenant = new Tenant { Name = "Demo Negative Validation Holding", Identifier = "demo-negative-validation-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Negative Test BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Noord-Holland",
            City = "Amsterdam",
            Address = "Prinsengracht 789",
            ContactEmail = "info@demo-negative.nl",
            ContactPhone = "+31201234567",
            TaxNumber = "NL123456789B01",
            TaxOffice = "Amsterdam"
        };
        await Testing.AddAsync(firm);

        var command = new CreateFacilityCommand
        {
            FirmId = firm.Id,
            Name = "Demo Invalid Capacity Facility",
            EnergyCapacity = -100
        };

        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task ShouldRejectNameExceedingMaxLength()
    {
        await Testing.RunAsAdministratorAsync();
        var tenant = new Tenant { Name = "Demo Max Length Holding", Identifier = "demo-max-length-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Max Length Test BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Noord-Holland",
            City = "Amsterdam",
            Address = "Singel 500",
            ContactEmail = "info@demo-maxlength.nl",
            ContactPhone = "+31201234567",
            TaxNumber = "NL123456789B01",
            TaxOffice = "Amsterdam"
        };
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
