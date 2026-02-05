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
        var tenant = new Tenant { Name = "Demo Energy Holding", Identifier = "demo-energy-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Energy Solutions BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Zuid-Holland",
            City = "Rotterdam",
            Address = "Wilhelminakade 123",
            ContactEmail = "contact@demo-energy-solutions.nl",
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
        result.Name.Should().Be("Demo Energy Solutions BV");
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
        var tenant = new Tenant { Name = "Demo Full Properties Holding", Identifier = "demo-full-properties-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Complete Solar BV",
            LogoUrl = "https://demo.nl/complete-logo.png",
            Type = "Solar Provider",
            Country = "Netherlands",
            State = "Noord-Holland",
            City = "Amsterdam",
            District = "Centrum",
            PostalCode = "1017BM",
            Address = "Leidseplein 26",
            ContactEmail = "info@demo-complete-solar.nl",
            ContactPhone = "+31201234567",
            Fax = "+31201234568",
            TaxNumber = "NL123456789B01",
            TaxOffice = "Amsterdam Centrum"
        };
        await Testing.AddAsync(firm);

        // Act
        var result = await Testing.SendAsync(new GetFirmQuery { Id = firm.Id });

        // Assert
        result.Should().NotBeNull();
        result!.LogoUrl.Should().Be("https://demo.nl/complete-logo.png");
        result.Type.Should().Be("Solar Provider");
        result.District.Should().Be("Centrum");
        result.PostalCode.Should().Be("1017BM");
        result.Fax.Should().Be("+31201234568");
    }
}
