using Backend.Application.Common.Exceptions;
using Backend.Application.Firms.Commands.CreateFirm;
using Backend.Domain.Entities;

namespace Backend.Application.FunctionalTests.Firms.Commands;

public class CreateFirmTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireMinimumFields()
    {
        await Testing.RunAsAdministratorAsync();
        var command = new CreateFirmCommand();

        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task ShouldRequireName()
    {
        await Testing.RunAsAdministratorAsync();
        var tenant = new Tenant { Name = "Demo Validation Tenant", Identifier = "demo-validation-tenant" };
        await Testing.AddAsync(tenant);

        var command = new CreateFirmCommand
        {
            TenantId = tenant.Id,
            Name = "",
            Country = "Netherlands",
            State = "Noord-Holland",
            City = "Amsterdam",
            Address = "Keizersgracht 123",
            ContactEmail = "info@demo.nl",
            ContactPhone = "+31201234567",
            TaxNumber = "NL123456789B01",
            TaxOffice = "Amsterdam"
        };

        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task ShouldRequireValidEmail()
    {
        await Testing.RunAsAdministratorAsync();
        var tenant = new Tenant { Name = "Demo Email Validation Tenant", Identifier = "demo-email-validation-tenant" };
        await Testing.AddAsync(tenant);

        var command = new CreateFirmCommand
        {
            TenantId = tenant.Id,
            Name = "Demo Energy Firm",
            Country = "Netherlands",
            State = "Noord-Holland",
            City = "Amsterdam",
            Address = "Prinsengracht 456",
            ContactEmail = "invalid-email",
            ContactPhone = "+31201234567",
            TaxNumber = "NL123456789B01",
            TaxOffice = "Amsterdam"
        };

        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task ShouldCreateFirm()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();
        var tenant = new Tenant { Name = "Demo Energy Holding", Identifier = "demo-energy-holding" };
        await Testing.AddAsync(tenant);

        var command = new CreateFirmCommand
        {
            TenantId = tenant.Id,
            Name = "Demo Green Energy BV",
            LogoUrl = "https://demo.nl/logo.png",
            Type = "Energy Provider",
            Country = "Netherlands",
            State = "Noord-Holland",
            City = "Amsterdam",
            District = "Centrum",
            PostalCode = "1016EA",
            Address = "Herengracht 182",
            ContactEmail = "contact@demo-green-energy.nl",
            ContactPhone = "+31201234567",
            Fax = "+31201234568",
            TaxNumber = "NL123456789B01",
            TaxOffice = "Amsterdam"
        };

        // Act
        var firmId = await Testing.SendAsync(command);

        // Assert
        var firm = await Testing.FindAsync<Firm>(firmId);

        firm.Should().NotBeNull();
        firm!.Name.Should().Be("Demo Green Energy BV");
        firm.TenantId.Should().Be(tenant.Id);
        firm.Country.Should().Be("Netherlands");
        firm.City.Should().Be("Amsterdam");
        firm.ContactEmail.Should().Be("contact@demo-green-energy.nl");
        firm.TaxNumber.Should().Be("NL123456789B01");
    }

    [Test]
    public async Task ShouldCreateFirmWithMinimalFields()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();
        var tenant = new Tenant { Name = "Demo Minimal Holding", Identifier = "demo-minimal-holding" };
        await Testing.AddAsync(tenant);

        var command = new CreateFirmCommand
        {
            TenantId = tenant.Id,
            Name = "Demo Solar Solutions BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Noord-Holland",
            City = "Haarlem",
            Address = "Grote Markt 10",
            ContactEmail = "info@demo-solar.nl",
            ContactPhone = "+31235678901",
            TaxNumber = "NL987654321B01",
            TaxOffice = "Haarlem"
        };

        // Act
        var firmId = await Testing.SendAsync(command);

        // Assert
        var firm = await Testing.FindAsync<Firm>(firmId);
        firm.Should().NotBeNull();
        firm!.Name.Should().Be("Demo Solar Solutions BV");
    }

    [Test]
    public async Task ShouldRejectNameExceedingMaxLength()
    {
        await Testing.RunAsAdministratorAsync();
        var tenant = new Tenant { Name = "Demo Max Length Tenant", Identifier = "demo-max-length-tenant" };
        await Testing.AddAsync(tenant);

        var command = new CreateFirmCommand
        {
            TenantId = tenant.Id,
            Name = new string('A', 501),
            Country = "Netherlands",
            State = "Noord-Holland",
            City = "Amsterdam",
            Address = "Singel 100",
            ContactEmail = "info@demo.nl",
            ContactPhone = "+31201234567",
            TaxNumber = "NL123456789B01",
            TaxOffice = "Amsterdam"
        };

        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task ShouldRequireCountry()
    {
        await Testing.RunAsAdministratorAsync();
        var tenant = new Tenant { Name = "Demo Country Validation Tenant", Identifier = "demo-country-validation-tenant" };
        await Testing.AddAsync(tenant);

        var command = new CreateFirmCommand
        {
            TenantId = tenant.Id,
            Name = "Demo Wind Power BV",
            Country = "",
            State = "Noord-Holland",
            City = "Amsterdam",
            Address = "Amstel 200",
            ContactEmail = "info@demo-wind.nl",
            ContactPhone = "+31201234567",
            TaxNumber = "NL123456789B01",
            TaxOffice = "Amsterdam"
        };

        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<ValidationException>();
    }
}
