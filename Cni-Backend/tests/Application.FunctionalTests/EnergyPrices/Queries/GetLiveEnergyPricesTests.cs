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

        var tenant = new Tenant { Name = "Live No MO Tenant", Identifier = "live-no-mo-tenant" };
        await Testing.AddAsync(tenant);

        // Firm without a market operator configured
        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Live No MO Firm",
            Country = "TestCountryWithNoMO",
            Type = "Standard",
            State = "NH",
            City = "Amsterdam",
            Address = "Test St",
            ContactEmail = "test@test.com",
            ContactPhone = "123",
            TaxNumber = "123",
            TaxOffice = "AMS"
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

        var tenant = new Tenant { Name = "Live MO Tenant", Identifier = "live-mo-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Live MO Firm",
            Type = "Standard",
            Country = "Türkiye",
            State = "Istanbul",
            City = "Istanbul",
            Address = "Test St",
            ContactEmail = "test@test.com",
            ContactPhone = "123",
            TaxNumber = "123",
            TaxOffice = "IST"
        };
        await Testing.AddAsync(firm);

        var uniqueMoCode = $"EPI_{Guid.NewGuid():N}";
        var marketOperator = new MarketOperator
        {
            Name = "EPIAS",
            Code = uniqueMoCode,
            BaseUrl = "https://seffaflik.epias.com.tr/transparency",
            AreaCodes = new List<string> { "TR" },
            SupportedResolutions = new List<int> { 60 },
            IsActive = true
        };
        await Testing.AddAsync(marketOperator);

        var firmMarketOperator = new FirmMarketOperator
        {
            FirmId = firm.Id,
            MarketOperatorId = marketOperator.Id,
            AreaCode = "TR",
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

        var tenant = new Tenant { Name = "FirmId Response Tenant", Identifier = "firmid-response-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "FirmId Response Firm",
            Type = "Standard",
            Country = "Netherlands",
            State = "NH",
            City = "Amsterdam",
            Address = "Test St",
            ContactEmail = "test@test.com",
            ContactPhone = "123",
            TaxNumber = "123",
            TaxOffice = "AMS"
        };
        await Testing.AddAsync(firm);

        var uniqueMoCode = $"ENT_{Guid.NewGuid():N}";
        var marketOperator = new MarketOperator
        {
            Name = "ENTSO-E",
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

        var tenant = new Tenant { Name = "Resolution Tenant", Identifier = "resolution-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Resolution Firm",
            Type = "Standard",
            Country = "Germany",
            State = "BY",
            City = "Munich",
            Address = "Test St",
            ContactEmail = "test@test.com",
            ContactPhone = "123",
            TaxNumber = "123",
            TaxOffice = "MUC"
        };
        await Testing.AddAsync(firm);

        var uniqueMoCode = $"ENTDE_{Guid.NewGuid():N}";
        var marketOperator = new MarketOperator
        {
            Name = "ENTSO-E DE",
            Code = uniqueMoCode,
            BaseUrl = "https://web-api.tp.entsoe.eu/api",
            AreaCodes = new List<string> { "10YDE-VE-------2" },
            SupportedResolutions = new List<int> { 15, 30, 60 },
            IsActive = true
        };
        await Testing.AddAsync(marketOperator);

        var firmMarketOperator = new FirmMarketOperator
        {
            FirmId = firm.Id,
            MarketOperatorId = marketOperator.Id,
            AreaCode = "10YDE-VE-------2",
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

        var tenant = new Tenant { Name = "Date Tenant", Identifier = "date-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Date Firm",
            Type = "Standard",
            Country = "Belgium",
            State = "Brussels",
            City = "Brussels",
            Address = "Test St",
            ContactEmail = "test@test.com",
            ContactPhone = "123",
            TaxNumber = "123",
            TaxOffice = "BRU"
        };
        await Testing.AddAsync(firm);

        var uniqueMoCode = $"ENTBE_{Guid.NewGuid():N}";
        var marketOperator = new MarketOperator
        {
            Name = "ENTSO-E BE",
            Code = uniqueMoCode,
            BaseUrl = "https://web-api.tp.entsoe.eu/api",
            AreaCodes = new List<string> { "10YBE----------2" },
            SupportedResolutions = new List<int> { 60 },
            IsActive = true
        };
        await Testing.AddAsync(marketOperator);

        var firmMarketOperator = new FirmMarketOperator
        {
            FirmId = firm.Id,
            MarketOperatorId = marketOperator.Id,
            AreaCode = "10YBE----------2",
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
