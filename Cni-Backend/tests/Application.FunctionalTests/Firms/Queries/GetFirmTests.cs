using Backend.Application.Firms.Queries.GetFirm;
using Backend.Domain.Entities;

namespace Backend.Application.FunctionalTests.Firms.Queries;

public class GetFirmTests : BaseTestFixture
{
    [Test]
    public async Task ShouldReturnFirm()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();
        var tenant = new Tenant { Name = "Get Firm Tenant", Identifier = "get-firm-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Energy Solutions BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Zuid-Holland",
            City = "Rotterdam",
            Address = "Harbor Street 1",
            ContactEmail = "contact@energysolutions.nl",
            ContactPhone = "+31101234567",
            TaxNumber = "NL987654321B01",
            TaxOffice = "Rotterdam"
        };
        await Testing.AddAsync(firm);

        // Act
        var result = await Testing.SendAsync(new GetFirmQuery { Id = firm.Id });

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(firm.Id);
        result.Name.Should().Be("Energy Solutions BV");
        result.Country.Should().Be("Netherlands");
        result.City.Should().Be("Rotterdam");
    }

    [Test]
    public async Task ShouldReturnNullForNonExistentFirm()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        // Act
        var result = await Testing.SendAsync(new GetFirmQuery { Id = 99999 });

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task ShouldReturnAllFirmProperties()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();
        var tenant = new Tenant { Name = "Full Properties Tenant", Identifier = "full-props-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Complete Firm",
            LogoUrl = "https://logo.com/complete.png",
            Type = "Solar Provider",
            Country = "Germany",
            State = "Bavaria",
            City = "Munich",
            District = "Schwabing",
            PostalCode = "80802",
            Address = "Leopoldstrasse 1",
            ContactEmail = "info@complete.de",
            ContactPhone = "+49891234567",
            Fax = "+49891234568",
            TaxNumber = "DE123456789",
            TaxOffice = "Munich Central"
        };
        await Testing.AddAsync(firm);

        // Act
        var result = await Testing.SendAsync(new GetFirmQuery { Id = firm.Id });

        // Assert
        result.Should().NotBeNull();
        result!.LogoUrl.Should().Be("https://logo.com/complete.png");
        result.Type.Should().Be("Solar Provider");
        result.District.Should().Be("Schwabing");
        result.PostalCode.Should().Be("80802");
        result.Fax.Should().Be("+49891234568");
    }
}
