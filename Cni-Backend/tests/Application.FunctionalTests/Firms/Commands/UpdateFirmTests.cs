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
            Name = "Updated Firm",
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
            Testing.SendAsync(command)).Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task ShouldUpdateFirm()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();
        var tenant = new Tenant { Name = "Update Firm Tenant", Identifier = "update-firm-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Original Firm",
            Type = "Standard",
            Country = "Netherlands",
            State = "NH",
            City = "Amsterdam",
            Address = "Original St",
            ContactEmail = "original@firm.nl",
            ContactPhone = "111111",
            TaxNumber = "NL111111111",
            TaxOffice = "Amsterdam"
        };
        await Testing.AddAsync(firm);

        var command = new UpdateFirmCommand
        {
            Id = firm.Id,
            Name = "Updated Firm Name",
            Country = "Germany",
            State = "Bavaria",
            City = "Munich",
            Address = "Updated Street 1",
            ContactEmail = "updated@firm.de",
            ContactPhone = "222222",
            TaxNumber = "DE222222222",
            TaxOffice = "Munich"
        };

        // Act
        await Testing.SendAsync(command);

        // Assert
        var updatedFirm = await Testing.FindAsync<Firm>(firm.Id);

        updatedFirm.Should().NotBeNull();
        updatedFirm!.Name.Should().Be("Updated Firm Name");
        updatedFirm.Country.Should().Be("Germany");
        updatedFirm.City.Should().Be("Munich");
        updatedFirm.ContactEmail.Should().Be("updated@firm.de");
    }

    [Test]
    public async Task ShouldUpdatePartialFields()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();
        var tenant = new Tenant { Name = "Partial Update Tenant", Identifier = "partial-update-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Partial Firm",
            Type = "Standard",
            Country = "Netherlands",
            State = "NH",
            City = "Amsterdam",
            Address = "Original St",
            ContactEmail = "original@firm.nl",
            ContactPhone = "111111",
            TaxNumber = "NL111111111",
            TaxOffice = "Amsterdam",
            LogoUrl = "https://old-logo.com"
        };
        await Testing.AddAsync(firm);

        var command = new UpdateFirmCommand
        {
            Id = firm.Id,
            Name = "Updated Name Only",
            Country = "Netherlands",
            State = "NH",
            City = "Amsterdam",
            Address = "Original St",
            ContactEmail = "original@firm.nl",
            ContactPhone = "111111",
            TaxNumber = "NL111111111",
            TaxOffice = "Amsterdam",
            LogoUrl = "https://new-logo.com"
        };

        // Act
        await Testing.SendAsync(command);

        // Assert
        var updatedFirm = await Testing.FindAsync<Firm>(firm.Id);
        updatedFirm!.Name.Should().Be("Updated Name Only");
        updatedFirm.LogoUrl.Should().Be("https://new-logo.com");
        updatedFirm.Country.Should().Be("Netherlands");
    }

    [Test]
    public async Task ShouldRejectInvalidEmail()
    {
        await Testing.RunAsAdministratorAsync();
        var tenant = new Tenant { Name = "Invalid Email Tenant", Identifier = "invalid-email-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Test Firm",
            Type = "Standard",
            Country = "Netherlands",
            State = "NH",
            City = "Amsterdam",
            Address = "Test St",
            ContactEmail = "valid@firm.nl",
            ContactPhone = "111111",
            TaxNumber = "NL111111111",
            TaxOffice = "Amsterdam"
        };
        await Testing.AddAsync(firm);

        var command = new UpdateFirmCommand
        {
            Id = firm.Id,
            Name = "Test Firm",
            Country = "Netherlands",
            State = "NH",
            City = "Amsterdam",
            Address = "Test St",
            ContactEmail = "not-an-email",
            ContactPhone = "111111",
            TaxNumber = "NL111111111",
            TaxOffice = "Amsterdam"
        };

        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<ValidationException>();
    }
}
