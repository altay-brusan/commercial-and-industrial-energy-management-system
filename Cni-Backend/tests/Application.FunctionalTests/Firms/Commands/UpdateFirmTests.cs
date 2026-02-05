using Backend.Application.Common.Exceptions;
using Backend.Application.Firms.Commands.UpdateFirm;
using Backend.Domain.Entities;

namespace Backend.Application.FunctionalTests.Firms.Commands;

public class UpdateFirmTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireValidFirmId()
    {
        await Testing.RunAsAdministratorAsync();
        var command = new UpdateFirmCommand
        {
            Id = 99999,
            Name = "Demo Updated Firm",
            Country = "Netherlands",
            State = "Noord-Holland",
            City = "Amsterdam",
            Address = "Rokin 50",
            ContactEmail = "info@demo-updated.nl",
            ContactPhone = "+31201234567",
            TaxNumber = "NL123456789B01",
            TaxOffice = "Amsterdam"
        };

        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task ShouldUpdateFirm()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();
        var tenant = new Tenant { Name = "Demo Update Holding", Identifier = "demo-update-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Original Energy BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Noord-Holland",
            City = "Amsterdam",
            Address = "Keizersgracht 100",
            ContactEmail = "original@demo-energy.nl",
            ContactPhone = "+31201111111",
            TaxNumber = "NL111111111B01",
            TaxOffice = "Amsterdam"
        };
        await Testing.AddAsync(firm);

        var command = new UpdateFirmCommand
        {
            Id = firm.Id,
            Name = "Demo Updated Energy BV",
            Country = "Netherlands",
            State = "Zuid-Holland",
            City = "Rotterdam",
            Address = "Coolsingel 200",
            ContactEmail = "updated@demo-energy.nl",
            ContactPhone = "+31102222222",
            TaxNumber = "NL222222222B01",
            TaxOffice = "Rotterdam"
        };

        // Act
        await Testing.SendAsync(command);

        // Assert
        var updatedFirm = await Testing.FindAsync<Firm>(firm.Id);

        updatedFirm.Should().NotBeNull();
        updatedFirm!.Name.Should().Be("Demo Updated Energy BV");
        updatedFirm.Country.Should().Be("Netherlands");
        updatedFirm.City.Should().Be("Rotterdam");
        updatedFirm.ContactEmail.Should().Be("updated@demo-energy.nl");
    }

    [Test]
    public async Task ShouldUpdatePartialFields()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();
        var tenant = new Tenant { Name = "Demo Partial Update Holding", Identifier = "demo-partial-update-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Partial Solar BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Noord-Holland",
            City = "Amsterdam",
            Address = "Prinsengracht 300",
            ContactEmail = "original@demo-solar.nl",
            ContactPhone = "+31201111111",
            TaxNumber = "NL111111111B01",
            TaxOffice = "Amsterdam",
            LogoUrl = "https://demo.nl/old-logo.png"
        };
        await Testing.AddAsync(firm);

        var command = new UpdateFirmCommand
        {
            Id = firm.Id,
            Name = "Demo Partial Solar Updated BV",
            Country = "Netherlands",
            State = "Noord-Holland",
            City = "Amsterdam",
            Address = "Prinsengracht 300",
            ContactEmail = "original@demo-solar.nl",
            ContactPhone = "+31201111111",
            TaxNumber = "NL111111111B01",
            TaxOffice = "Amsterdam",
            LogoUrl = "https://demo.nl/new-logo.png"
        };

        // Act
        await Testing.SendAsync(command);

        // Assert
        var updatedFirm = await Testing.FindAsync<Firm>(firm.Id);
        updatedFirm!.Name.Should().Be("Demo Partial Solar Updated BV");
        updatedFirm.LogoUrl.Should().Be("https://demo.nl/new-logo.png");
        updatedFirm.Country.Should().Be("Netherlands");
    }

    [Test]
    public async Task ShouldRejectInvalidEmail()
    {
        await Testing.RunAsAdministratorAsync();
        var tenant = new Tenant { Name = "Demo Email Validation Holding", Identifier = "demo-email-validation-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Wind Power BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Noord-Holland",
            City = "Amsterdam",
            Address = "Herengracht 400",
            ContactEmail = "valid@demo-wind.nl",
            ContactPhone = "+31201111111",
            TaxNumber = "NL111111111B01",
            TaxOffice = "Amsterdam"
        };
        await Testing.AddAsync(firm);

        var command = new UpdateFirmCommand
        {
            Id = firm.Id,
            Name = "Demo Wind Power BV",
            Country = "Netherlands",
            State = "Noord-Holland",
            City = "Amsterdam",
            Address = "Herengracht 400",
            ContactEmail = "not-an-email",
            ContactPhone = "+31201111111",
            TaxNumber = "NL111111111B01",
            TaxOffice = "Amsterdam"
        };

        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<ValidationException>();
    }
}
