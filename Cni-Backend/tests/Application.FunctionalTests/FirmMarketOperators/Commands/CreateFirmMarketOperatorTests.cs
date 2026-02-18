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
            FirmId = Guid.Empty,
            MarketOperatorId = Guid.NewGuid(),
            AreaCode = "10YNL----------L"
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
            FirmId = Guid.NewGuid(),
            MarketOperatorId = Guid.Empty,
            AreaCode = "10YNL----------L"
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
            FirmId = Guid.NewGuid(),
            MarketOperatorId = Guid.NewGuid(),
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

        var tenant = new Tenant { Name = "Demo FMO Holding", Identifier = "demo-fmo-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo FMO Energy BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Noord-Holland",
            City = "Amsterdam",
            Address = "Keizersgracht 500",
            ContactEmail = "info@demo-fmo-energy.nl",
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
            SupportedResolutions = new List<int> { 60 },
            AreaCodes = new List<string> { "10YNL----------L" },
            IsActive = true
        };
        await Testing.AddAsync(marketOperator);

        var command = new CreateFirmMarketOperatorCommand
        {
            FirmId = firm.Id,
            MarketOperatorId = marketOperator.Id,
            AreaCode = "10YNL----------L",
            SamplingPeriodMinutes = 60
        };

        // Act
        var firmMarketOperatorId = await Testing.SendAsync(command);

        // Assert
        var firmMarketOperator = await Testing.FindAsync<FirmMarketOperator>(firmMarketOperatorId);

        firmMarketOperator.Should().NotBeNull();
        firmMarketOperator!.FirmId.Should().Be(firm.Id);
        firmMarketOperator.MarketOperatorId.Should().Be(marketOperator.Id);
        firmMarketOperator.AreaCode.Should().Be("10YNL----------L");
        firmMarketOperator.SamplingPeriodMinutes.Should().Be(60);
        firmMarketOperator.IsActive.Should().BeTrue();
    }

    [Test]
    public async Task ShouldCreateFirmMarketOperatorWithDefaultSamplingPeriod()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Demo Default FMO Holding", Identifier = "demo-default-fmo-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Default FMO Firm BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Zuid-Holland",
            City = "Rotterdam",
            Address = "Coolsingel 200",
            ContactEmail = "info@demo-default-fmo.nl",
            ContactPhone = "+31101234567",
            TaxNumber = "NL987654321B01",
            TaxOffice = "Rotterdam"
        };
        await Testing.AddAsync(firm);

        var uniqueMoCode = $"ENT_{Guid.NewGuid():N}";
        var marketOperator = new MarketOperator
        {
            Name = "Demo ENTSO-E Default",
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

        var tenant = new Tenant { Name = "Demo 15Min FMO Holding", Identifier = "demo-15min-fmo-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo 15Min FMO Firm BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Noord-Holland",
            City = "Haarlem",
            Address = "Grote Markt 15",
            ContactEmail = "info@demo-15min-fmo.nl",
            ContactPhone = "+31235678901",
            TaxNumber = "NL555555555B01",
            TaxOffice = "Haarlem"
        };
        await Testing.AddAsync(firm);

        var uniqueMoCode = $"ENT15_{Guid.NewGuid():N}";
        var marketOperator = new MarketOperator
        {
            Name = "Demo ENTSO-E 15Min",
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
            AreaCode = "10YNL----------L",
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
