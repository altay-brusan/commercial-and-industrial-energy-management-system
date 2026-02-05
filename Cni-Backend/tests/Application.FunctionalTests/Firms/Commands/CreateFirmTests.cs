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
        var tenant = new Tenant { Name = "Firm Name Test Tenant", Identifier = "firm-name-test" };
        await Testing.AddAsync(tenant);

        var command = new CreateFirmCommand
        {
            TenantId = tenant.Id,
            Name = "",
            Country = "Netherlands",
            State = "NH",
            City = "Amsterdam",
            Address = "Test St",
            ContactEmail = "test@test.com",
            ContactPhone = "123456",
            TaxNumber = "NL123456789",
            TaxOffice = "Amsterdam"
        };

        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task ShouldRequireValidEmail()
    {
        await Testing.RunAsAdministratorAsync();
        var tenant = new Tenant { Name = "Firm Email Test Tenant", Identifier = "firm-email-test" };
        await Testing.AddAsync(tenant);

        var command = new CreateFirmCommand
        {
            TenantId = tenant.Id,
            Name = "Test Firm",
            Country = "Netherlands",
            State = "NH",
            City = "Amsterdam",
            Address = "Test St",
            ContactEmail = "invalid-email",
            ContactPhone = "123456",
            TaxNumber = "NL123456789",
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
        var tenant = new Tenant { Name = "Create Firm Tenant", Identifier = "create-firm-tenant" };
        await Testing.AddAsync(tenant);

        var command = new CreateFirmCommand
        {
            TenantId = tenant.Id,
            Name = "Green Energy BV",
            LogoUrl = "https://example.com/logo.png",
            Type = "Energy Provider",
            Country = "Netherlands",
            State = "Noord-Holland",
            City = "Amsterdam",
            District = "Centrum",
            PostalCode = "1012AB",
            Address = "Damrak 1",
            ContactEmail = "contact@greenenergy.nl",
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
        firm!.Name.Should().Be("Green Energy BV");
        firm.TenantId.Should().Be(tenant.Id);
        firm.Country.Should().Be("Netherlands");
        firm.City.Should().Be("Amsterdam");
        firm.ContactEmail.Should().Be("contact@greenenergy.nl");
        firm.TaxNumber.Should().Be("NL123456789B01");
    }

    [Test]
    public async Task ShouldCreateFirmWithMinimalFields()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();
        var tenant = new Tenant { Name = "Minimal Firm Tenant", Identifier = "minimal-firm-tenant" };
        await Testing.AddAsync(tenant);

        var command = new CreateFirmCommand
        {
            TenantId = tenant.Id,
            Name = "Minimal Firm",
            Type = "Standard",
            Country = "Germany",
            State = "Bavaria",
            City = "Munich",
            Address = "Test Street 1",
            ContactEmail = "min@firm.de",
            ContactPhone = "+49123456",
            TaxNumber = "DE123456789",
            TaxOffice = "Munich"
        };

        // Act
        var firmId = await Testing.SendAsync(command);

        // Assert
        var firm = await Testing.FindAsync<Firm>(firmId);
        firm.Should().NotBeNull();
        firm!.Name.Should().Be("Minimal Firm");
    }

    [Test]
    public async Task ShouldRejectNameExceedingMaxLength()
    {
        await Testing.RunAsAdministratorAsync();
        var tenant = new Tenant { Name = "Long Name Tenant", Identifier = "long-name-tenant" };
        await Testing.AddAsync(tenant);

        var command = new CreateFirmCommand
        {
            TenantId = tenant.Id,
            Name = new string('A', 501),
            Country = "Netherlands",
            State = "NH",
            City = "Amsterdam",
            Address = "Test St",
            ContactEmail = "test@test.com",
            ContactPhone = "123456",
            TaxNumber = "NL123456789",
            TaxOffice = "Amsterdam"
        };

        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task ShouldRequireCountry()
    {
        await Testing.RunAsAdministratorAsync();
        var tenant = new Tenant { Name = "No Country Tenant", Identifier = "no-country-tenant" };
        await Testing.AddAsync(tenant);

        var command = new CreateFirmCommand
        {
            TenantId = tenant.Id,
            Name = "Test Firm",
            Country = "",
            State = "NH",
            City = "Amsterdam",
            Address = "Test St",
            ContactEmail = "test@test.com",
            ContactPhone = "123456",
            TaxNumber = "NL123456789",
            TaxOffice = "Amsterdam"
        };

        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<ValidationException>();
    }
}
