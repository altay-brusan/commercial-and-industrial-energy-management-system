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

        var tenant = new Tenant { Name = "Get FMO Tenant", Identifier = "get-fmo-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Get FMO Firm",
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

        var tenant = new Tenant { Name = "No FMO Tenant", Identifier = "no-fmo-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "No FMO Firm",
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
        var result = await Testing.SendAsync(new GetFirmMarketOperatorQuery { FirmId = 99999 });

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task ShouldReturnNullForInactiveFirmMarketOperator()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Inactive FMO Tenant", Identifier = "inactive-fmo-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Inactive FMO Firm",
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
            SupportedResolutions = new List<int> { 15, 60 },
            AreaCodes = new List<string> { "10YDE-VE-------2" },
            IsActive = true
        };
        await Testing.AddAsync(marketOperator);

        var firmMarketOperator = new FirmMarketOperator
        {
            FirmId = firm.Id,
            MarketOperatorId = marketOperator.Id,
            AreaCode = "10YDE-VE-------2",
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

        var tenant = new Tenant { Name = "Sampling Query Tenant", Identifier = "sampling-query-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Sampling Query Firm",
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
            SupportedResolutions = new List<int> { 30, 60 },
            AreaCodes = new List<string> { "10YBE----------2" },
            IsActive = true
        };
        await Testing.AddAsync(marketOperator);

        var firmMarketOperator = new FirmMarketOperator
        {
            FirmId = firm.Id,
            MarketOperatorId = marketOperator.Id,
            AreaCode = "10YBE----------2",
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
        var result = await Testing.SendAsync(new GetFirmMarketOperatorQuery { FirmId = 0 });

        // Assert
        result.Should().BeNull();
    }
}
