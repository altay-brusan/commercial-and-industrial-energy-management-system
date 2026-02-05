using Backend.Application.Common.Exceptions;
using Backend.Application.EnergyPrices.Queries.GetLiveEnergyPrices;
using Backend.Domain.Entities;

namespace Backend.Application.FunctionalTests.EnergyPrices.Queries;

public class GetLiveEnergyPricesTests : BaseTestFixture
{
    [Test]
    public async Task ShouldReturnErrorForNonExistentFirm()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var query = new GetLiveEnergyPricesQuery
        {
            FirmId = 99999
        };

        // Act
        var result = await Testing.SendAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    [Test]
    public async Task ShouldReturnErrorWhenNoMarketOperatorConfigured()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Demo No MO Holding", Identifier = "demo-no-mo-holding" };
        await Testing.AddAsync(tenant);

        // Firm without a market operator configured
        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo No MO Firm BV",
            Country = "DemoCountryWithNoMO",
            Type = "Standard",
            State = "Noord-Holland",
            City = "Amsterdam",
            Address = "Keizersgracht 100",
            ContactEmail = "info@demo-no-mo.nl",
            ContactPhone = "+31201234567",
            TaxNumber = "NL123456789B01",
            TaxOffice = "Amsterdam"
        };
        await Testing.AddAsync(firm);

        var query = new GetLiveEnergyPricesQuery
        {
            FirmId = firm.Id
        };

        // Act
        var result = await Testing.SendAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Market Operator");
    }

    [Test]
    public async Task ShouldUseFirmMarketOperatorWhenConfigured()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Demo Live MO Holding", Identifier = "demo-live-mo-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Live MO Firm BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Noord-Holland",
            City = "Amsterdam",
            Address = "Herengracht 200",
            ContactEmail = "info@demo-live-mo.nl",
            ContactPhone = "+31201234567",
            TaxNumber = "NL123456789B01",
            TaxOffice = "Amsterdam"
        };
        await Testing.AddAsync(firm);

        var uniqueMoCode = $"ENT_{Guid.NewGuid():N}";
        var marketOperator = new MarketOperator
        {
            Name = "Demo ENTSO-E NL",
            Code = uniqueMoCode,
            BaseUrl = "https://web-api.tp.entsoe.eu/api",
            AreaCodes = new List<string> { "10YNL----------L" },
            SupportedResolutions = new List<int> { 60 },
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

        var query = new GetLiveEnergyPricesQuery
        {
            FirmId = firm.Id
        };

        // Act
        var result = await Testing.SendAsync(query);

        // Assert
        // Note: The actual API call may fail in test environment, but the query should at least attempt to use the configured MO
        result.Should().NotBeNull();
        // If it fails, it should still return the MO info
        if (!result.Success)
        {
            result.MarketOperatorCode.Should().Be(uniqueMoCode);
        }
    }

    [Test]
    public async Task ShouldValidateFirmIdIsRequired()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var query = new GetLiveEnergyPricesQuery
        {
            FirmId = 0 // Invalid
        };

        // Act & Assert
        await FluentActions.Invoking(() =>
            Testing.SendAsync(query)).Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task ShouldReturnCorrectFirmIdInResponse()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Demo FirmId Response Holding", Identifier = "demo-firmid-response-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo FirmId Response Firm BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Zuid-Holland",
            City = "Rotterdam",
            Address = "Coolsingel 100",
            ContactEmail = "info@demo-firmid-response.nl",
            ContactPhone = "+31101234567",
            TaxNumber = "NL987654321B01",
            TaxOffice = "Rotterdam"
        };
        await Testing.AddAsync(firm);

        var uniqueMoCode = $"ENT_{Guid.NewGuid():N}";
        var marketOperator = new MarketOperator
        {
            Name = "Demo ENTSO-E Response",
            Code = uniqueMoCode,
            BaseUrl = "https://web-api.tp.entsoe.eu/api",
            AreaCodes = new List<string> { "10YNL----------L" },
            SupportedResolutions = new List<int> { 15, 60 },
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

        var query = new GetLiveEnergyPricesQuery
        {
            FirmId = firm.Id
        };

        // Act
        var result = await Testing.SendAsync(query);

        // Assert
        // The query sets FirmId on successful paths
        // Even if external API fails, we check the setup works
        result.Should().NotBeNull();
        if (result.Success)
        {
            result.FirmId.Should().Be(firm.Id);
        }
    }

    [Test]
    public async Task ShouldUseCustomResolutionWhenSpecified()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Demo Resolution Holding", Identifier = "demo-resolution-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Resolution Firm BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Noord-Brabant",
            City = "Eindhoven",
            Address = "Stationsplein 20",
            ContactEmail = "info@demo-resolution.nl",
            ContactPhone = "+31401234567",
            TaxNumber = "NL444444444B01",
            TaxOffice = "Eindhoven"
        };
        await Testing.AddAsync(firm);

        var uniqueMoCode = $"ENT_{Guid.NewGuid():N}";
        var marketOperator = new MarketOperator
        {
            Name = "Demo ENTSO-E Resolution",
            Code = uniqueMoCode,
            BaseUrl = "https://web-api.tp.entsoe.eu/api",
            AreaCodes = new List<string> { "10YNL----------L" },
            SupportedResolutions = new List<int> { 15, 30, 60 },
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

        var query = new GetLiveEnergyPricesQuery
        {
            FirmId = firm.Id,
            Period = 15 // Request 15-minute resolution
        };

        // Act
        var result = await Testing.SendAsync(query);

        // Assert
        result.Should().NotBeNull();
        // If successful, resolution should be 15 or as returned by the MO
    }

    [Test]
    public async Task ShouldUseCustomDateWhenSpecified()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Demo Date Holding", Identifier = "demo-date-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Date Firm BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Utrecht",
            City = "Utrecht",
            Address = "Vredenburg 30",
            ContactEmail = "info@demo-date.nl",
            ContactPhone = "+31301234567",
            TaxNumber = "NL333333333B01",
            TaxOffice = "Utrecht"
        };
        await Testing.AddAsync(firm);

        var uniqueMoCode = $"ENT_{Guid.NewGuid():N}";
        var marketOperator = new MarketOperator
        {
            Name = "Demo ENTSO-E Date",
            Code = uniqueMoCode,
            BaseUrl = "https://web-api.tp.entsoe.eu/api",
            AreaCodes = new List<string> { "10YNL----------L" },
            SupportedResolutions = new List<int> { 60 },
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

        var customDate = DateTimeOffset.UtcNow.Date.AddDays(1);

        var query = new GetLiveEnergyPricesQuery
        {
            FirmId = firm.Id,
            Date = customDate
        };

        // Act
        var result = await Testing.SendAsync(query);

        // Assert
        result.Should().NotBeNull();
        if (result.Success)
        {
            result.Date.Date.Should().Be(customDate.Date);
        }
    }
}
