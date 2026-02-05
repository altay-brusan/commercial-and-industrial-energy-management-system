using Backend.Application.Common.Exceptions;
using Backend.Application.FirmMarketOperators.Commands.CreateFirmMarketOperator;
using Backend.Domain.Entities;

namespace Backend.Application.FunctionalTests.FirmMarketOperators.Commands;

public class CreateFirmMarketOperatorTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireValidFirmId()
    {
        await Testing.RunAsAdministratorAsync();
        var command = new CreateFirmMarketOperatorCommand
        {
            FirmId = 0,
            MarketOperatorId = 1,
            AreaCode = "TR"
        };

        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task ShouldRequireValidMarketOperatorId()
    {
        await Testing.RunAsAdministratorAsync();
        var command = new CreateFirmMarketOperatorCommand
        {
            FirmId = 1,
            MarketOperatorId = 0,
            AreaCode = "TR"
        };

        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task ShouldRequireAreaCode()
    {
        await Testing.RunAsAdministratorAsync();
        var command = new CreateFirmMarketOperatorCommand
        {
            FirmId = 1,
            MarketOperatorId = 1,
            AreaCode = ""
        };

        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task ShouldCreateFirmMarketOperator()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "FMO Tenant", Identifier = "fmo-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "FMO Test Firm",
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
            SupportedResolutions = new List<int> { 60 },
            AreaCodes = new List<string> { "TR" },
            IsActive = true
        };
        await Testing.AddAsync(marketOperator);

        var command = new CreateFirmMarketOperatorCommand
        {
            FirmId = firm.Id,
            MarketOperatorId = marketOperator.Id,
            AreaCode = "TR",
            SamplingPeriodMinutes = 60
        };

        // Act
        var firmMarketOperatorId = await Testing.SendAsync(command);

        // Assert
        var firmMarketOperator = await Testing.FindAsync<FirmMarketOperator>(firmMarketOperatorId);

        firmMarketOperator.Should().NotBeNull();
        firmMarketOperator!.FirmId.Should().Be(firm.Id);
        firmMarketOperator.MarketOperatorId.Should().Be(marketOperator.Id);
        firmMarketOperator.AreaCode.Should().Be("TR");
        firmMarketOperator.SamplingPeriodMinutes.Should().Be(60);
        firmMarketOperator.IsActive.Should().BeTrue();
    }

    [Test]
    public async Task ShouldCreateFirmMarketOperatorWithDefaultSamplingPeriod()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "FMO Default Tenant", Identifier = "fmo-default-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "FMO Default Firm",
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
            SupportedResolutions = new List<int> { 15, 60 },
            AreaCodes = new List<string> { "10YNL----------L" },
            IsActive = true
        };
        await Testing.AddAsync(marketOperator);

        var command = new CreateFirmMarketOperatorCommand
        {
            FirmId = firm.Id,
            MarketOperatorId = marketOperator.Id,
            AreaCode = "10YNL----------L"
        };

        // Act
        var firmMarketOperatorId = await Testing.SendAsync(command);

        // Assert
        var firmMarketOperator = await Testing.FindAsync<FirmMarketOperator>(firmMarketOperatorId);

        firmMarketOperator.Should().NotBeNull();
        firmMarketOperator!.SamplingPeriodMinutes.Should().Be(60); // Default value
    }

    [Test]
    public async Task ShouldCreateFirmMarketOperatorWith15MinSamplingPeriod()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "FMO 15Min Tenant", Identifier = "fmo-15min-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "FMO 15Min Firm",
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
            Name = "ENTSO-E Germany",
            Code = uniqueMoCode,
            BaseUrl = "https://web-api.tp.entsoe.eu/api",
            SupportedResolutions = new List<int> { 15, 60 },
            AreaCodes = new List<string> { "10YDE-VE-------2" },
            IsActive = true
        };
        await Testing.AddAsync(marketOperator);

        var command = new CreateFirmMarketOperatorCommand
        {
            FirmId = firm.Id,
            MarketOperatorId = marketOperator.Id,
            AreaCode = "10YDE-VE-------2",
            SamplingPeriodMinutes = 15
        };

        // Act
        var firmMarketOperatorId = await Testing.SendAsync(command);

        // Assert
        var firmMarketOperator = await Testing.FindAsync<FirmMarketOperator>(firmMarketOperatorId);

        firmMarketOperator.Should().NotBeNull();
        firmMarketOperator!.SamplingPeriodMinutes.Should().Be(15);
    }
}
