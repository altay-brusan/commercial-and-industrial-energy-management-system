using Backend.Application.FirmMarketOperators.Queries.GetFirmMarketOperator;
using Backend.Domain.Entities;

namespace Backend.Application.FunctionalTests.FirmMarketOperators.Queries;

public class GetFirmMarketOperatorTests : BaseTestFixture
{
    [Test]
    public async Task ShouldReturnFirmMarketOperator()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Demo Get FMO Energy Holding", Identifier = "demo-get-fmo-energy-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Get FMO Solar BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Noord-Holland",
            City = "Amsterdam",
            Address = "Keizersgracht 250",
            ContactEmail = "info@demo-get-fmo-solar.nl",
            ContactPhone = "+31201234567",
            TaxNumber = "NL123456789B01",
            TaxOffice = "Amsterdam"
        };
        await Testing.AddAsync(firm);

        var uniqueMoCode = $"ENT_{Guid.NewGuid():N}";
        var marketOperator = new MarketOperator
        {
            Name = "Demo ENTSO-E NL Get",
            Code = uniqueMoCode,
            BaseUrl = "https://web-api.tp.entsoe.eu/api",
            SupportedResolutions = new List<int> { 60 },
            AreaCodes = new List<string> { "10YNL----------L" },
            IsActive = true
        };
        await Testing.AddAsync(marketOperator);

        var firmMarketOperator = new FirmMarketOperator
        {
            FirmId = firm.Id,
            MarketOperatorId = marketOperator.Id,
            AreaCode = "10YNL----------L",
            SamplingPeriodMinutes = 60,
            IsActive = true
        };
        await Testing.AddAsync(firmMarketOperator);

        // Act
        var result = await Testing.SendAsync(new GetFirmMarketOperatorQuery { FirmId = firm.Id });

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(firmMarketOperator.Id);
        result.FirmId.Should().Be(firm.Id);
        result.MarketOperatorId.Should().Be(marketOperator.Id);
        result.AreaCode.Should().Be("10YNL----------L");
        result.SamplingPeriodMinutes.Should().Be(60);
    }

    [Test]
    public async Task ShouldReturnNullForFirmWithoutMarketOperator()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Demo No FMO Energy Holding", Identifier = "demo-no-fmo-energy-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo No FMO Wind BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Zuid-Holland",
            City = "Den Haag",
            Address = "Binnenhof 50",
            ContactEmail = "info@demo-no-fmo-wind.nl",
            ContactPhone = "+31701234567",
            TaxNumber = "NL987654321B01",
            TaxOffice = "Den Haag"
        };
        await Testing.AddAsync(firm);

        // Act
        var result = await Testing.SendAsync(new GetFirmMarketOperatorQuery { FirmId = firm.Id });

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task ShouldReturnNullForNonExistentFirm()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        // Act
        var result = await Testing.SendAsync(new GetFirmMarketOperatorQuery { FirmId = Guid.NewGuid() });

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task ShouldReturnNullForInactiveFirmMarketOperator()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Demo Inactive FMO Energy Holding", Identifier = "demo-inactive-fmo-energy-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Inactive FMO Solar BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Noord-Brabant",
            City = "Eindhoven",
            Address = "Stationsplein 15",
            ContactEmail = "info@demo-inactive-fmo-solar.nl",
            ContactPhone = "+31401234567",
            TaxNumber = "NL444444444B01",
            TaxOffice = "Eindhoven"
        };
        await Testing.AddAsync(firm);

        var uniqueMoCode = $"ENTNL_{Guid.NewGuid():N}";
        var marketOperator = new MarketOperator
        {
            Name = "Demo ENTSO-E NL Inactive",
            Code = uniqueMoCode,
            BaseUrl = "https://web-api.tp.entsoe.eu/api",
            SupportedResolutions = new List<int> { 15, 60 },
            AreaCodes = new List<string> { "10YNL----------L" },
            IsActive = true
        };
        await Testing.AddAsync(marketOperator);

        var firmMarketOperator = new FirmMarketOperator
        {
            FirmId = firm.Id,
            MarketOperatorId = marketOperator.Id,
            AreaCode = "10YNL----------L",
            SamplingPeriodMinutes = 15,
            IsActive = false // Inactive
        };
        await Testing.AddAsync(firmMarketOperator);

        // Act
        var result = await Testing.SendAsync(new GetFirmMarketOperatorQuery { FirmId = firm.Id });

        // Assert
        result.Should().BeNull(); // Query only returns active FMOs
    }

    [Test]
    public async Task ShouldReturnCorrectSamplingPeriod()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Demo Sampling Query Energy Holding", Identifier = "demo-sampling-query-energy-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Sampling Query Wind BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Gelderland",
            City = "Arnhem",
            Address = "Koningstraat 100",
            ContactEmail = "info@demo-sampling-query-wind.nl",
            ContactPhone = "+31261234567",
            TaxNumber = "NL555555555B01",
            TaxOffice = "Arnhem"
        };
        await Testing.AddAsync(firm);

        var uniqueMoCode = $"ENTNL_{Guid.NewGuid():N}";
        var marketOperator = new MarketOperator
        {
            Name = "Demo ENTSO-E NL Sampling",
            Code = uniqueMoCode,
            BaseUrl = "https://web-api.tp.entsoe.eu/api",
            SupportedResolutions = new List<int> { 30, 60 },
            AreaCodes = new List<string> { "10YNL----------L" },
            IsActive = true
        };
        await Testing.AddAsync(marketOperator);

        var firmMarketOperator = new FirmMarketOperator
        {
            FirmId = firm.Id,
            MarketOperatorId = marketOperator.Id,
            AreaCode = "10YNL----------L",
            SamplingPeriodMinutes = 30,
            IsActive = true
        };
        await Testing.AddAsync(firmMarketOperator);

        // Act
        var result = await Testing.SendAsync(new GetFirmMarketOperatorQuery { FirmId = firm.Id });

        // Assert
        result.Should().NotBeNull();
        result!.SamplingPeriodMinutes.Should().Be(30);
    }

    [Test]
    public async Task ShouldReturnNullForInvalidFirmId()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        // Act
        var result = await Testing.SendAsync(new GetFirmMarketOperatorQuery { FirmId = Guid.Empty });

        // Assert
        result.Should().BeNull();
    }
}
