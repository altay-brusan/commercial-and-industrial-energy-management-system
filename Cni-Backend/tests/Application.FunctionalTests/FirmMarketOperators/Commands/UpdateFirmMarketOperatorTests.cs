using Backend.Application.Common.Exceptions;
using Backend.Application.FirmMarketOperators.Commands.UpdateFirmMarketOperator;
using Backend.Domain.Entities;

namespace Backend.Application.FunctionalTests.FirmMarketOperators.Commands;

public class UpdateFirmMarketOperatorTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireValidFirmMarketOperatorId()
    {
        await Testing.RunAsAdministratorAsync();

        // Create a valid MarketOperator so validation passes
        var uniqueMoCode = $"ENT_{Guid.NewGuid():N}";
        var marketOperator = new MarketOperator
        {
            Name = "Demo ENTSO-E Validation",
            Code = uniqueMoCode,
            BaseUrl = "https://web-api.tp.entsoe.eu/api",
            SupportedResolutions = new List<int> { 60 },
            AreaCodes = new List<string> { "10YNL----------L" },
            IsActive = true
        };
        await Testing.AddAsync(marketOperator);

        var command = new UpdateFirmMarketOperatorCommand
        {
            Id = 99999,
            MarketOperatorId = marketOperator.Id,
            AreaCode = "10YNL----------L",
            SamplingPeriodMinutes = 60
        };

        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task ShouldUpdateFirmMarketOperator()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Demo Update FMO Holding", Identifier = "demo-update-fmo-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Update FMO Firm BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Noord-Holland",
            City = "Amsterdam",
            Address = "Herengracht 600",
            ContactEmail = "info@demo-update-fmo.nl",
            ContactPhone = "+31201234567",
            TaxNumber = "NL123456789B01",
            TaxOffice = "Amsterdam"
        };
        await Testing.AddAsync(firm);

        var uniqueMoCode1 = $"ENT1_{Guid.NewGuid():N}";
        var marketOperator1 = new MarketOperator
        {
            Name = "Demo ENTSO-E Original",
            Code = uniqueMoCode1,
            BaseUrl = "https://web-api.tp.entsoe.eu/api",
            SupportedResolutions = new List<int> { 60 },
            AreaCodes = new List<string> { "10YNL----------L" },
            IsActive = true
        };
        await Testing.AddAsync(marketOperator1);

        var uniqueMoCode2 = $"ENT2_{Guid.NewGuid():N}";
        var marketOperator2 = new MarketOperator
        {
            Name = "Demo ENTSO-E Updated",
            Code = uniqueMoCode2,
            BaseUrl = "https://web-api.tp.entsoe.eu/api",
            SupportedResolutions = new List<int> { 15, 60 },
            AreaCodes = new List<string> { "10YNL----------L" },
            IsActive = true
        };
        await Testing.AddAsync(marketOperator2);

        var firmMarketOperator = new FirmMarketOperator
        {
            FirmId = firm.Id,
            MarketOperatorId = marketOperator1.Id,
            AreaCode = "10YNL----------L",
            SamplingPeriodMinutes = 60,
            IsActive = true
        };
        await Testing.AddAsync(firmMarketOperator);

        var command = new UpdateFirmMarketOperatorCommand
        {
            Id = firmMarketOperator.Id,
            MarketOperatorId = marketOperator2.Id,
            AreaCode = "10YNL----------L",
            SamplingPeriodMinutes = 15,
            IsActive = true
        };

        // Act
        await Testing.SendAsync(command);

        // Assert
        var updatedFmo = await Testing.FindAsync<FirmMarketOperator>(firmMarketOperator.Id);

        updatedFmo.Should().NotBeNull();
        updatedFmo!.MarketOperatorId.Should().Be(marketOperator2.Id);
        updatedFmo.AreaCode.Should().Be("10YNL----------L");
        updatedFmo.SamplingPeriodMinutes.Should().Be(15);
    }

    [Test]
    public async Task ShouldDeactivateFirmMarketOperator()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Demo Deactivate FMO Holding", Identifier = "demo-deactivate-fmo-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Deactivate FMO Firm BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Zuid-Holland",
            City = "Rotterdam",
            Address = "Wilhelminakade 100",
            ContactEmail = "info@demo-deactivate-fmo.nl",
            ContactPhone = "+31101234567",
            TaxNumber = "NL987654321B01",
            TaxOffice = "Rotterdam"
        };
        await Testing.AddAsync(firm);

        var uniqueMoCode = $"ENT_{Guid.NewGuid():N}";
        var marketOperator = new MarketOperator
        {
            Name = "Demo ENTSO-E Deactivate",
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

        var command = new UpdateFirmMarketOperatorCommand
        {
            Id = firmMarketOperator.Id,
            MarketOperatorId = marketOperator.Id,
            AreaCode = firmMarketOperator.AreaCode,
            SamplingPeriodMinutes = firmMarketOperator.SamplingPeriodMinutes,
            IsActive = false
        };

        // Act
        await Testing.SendAsync(command);

        // Assert
        var updatedFmo = await Testing.FindAsync<FirmMarketOperator>(firmMarketOperator.Id);
        updatedFmo!.IsActive.Should().BeFalse();
    }

    [Test]
    public async Task ShouldReactivateFirmMarketOperator()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Demo Reactivate FMO Holding", Identifier = "demo-reactivate-fmo-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Reactivate FMO Firm BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Noord-Brabant",
            City = "Eindhoven",
            Address = "Stationsplein 10",
            ContactEmail = "info@demo-reactivate-fmo.nl",
            ContactPhone = "+31401234567",
            TaxNumber = "NL444444444B01",
            TaxOffice = "Eindhoven"
        };
        await Testing.AddAsync(firm);

        var uniqueMoCode = $"ENT_{Guid.NewGuid():N}";
        var marketOperator = new MarketOperator
        {
            Name = "Demo ENTSO-E Reactivate",
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
            IsActive = false
        };
        await Testing.AddAsync(firmMarketOperator);

        var command = new UpdateFirmMarketOperatorCommand
        {
            Id = firmMarketOperator.Id,
            MarketOperatorId = marketOperator.Id,
            AreaCode = firmMarketOperator.AreaCode,
            SamplingPeriodMinutes = firmMarketOperator.SamplingPeriodMinutes,
            IsActive = true
        };

        // Act
        await Testing.SendAsync(command);

        // Assert
        var updatedFmo = await Testing.FindAsync<FirmMarketOperator>(firmMarketOperator.Id);
        updatedFmo!.IsActive.Should().BeTrue();
    }

    [Test]
    public async Task ShouldUpdateSamplingPeriod()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Demo Sampling FMO Holding", Identifier = "demo-sampling-fmo-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Sampling FMO Firm BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Utrecht",
            City = "Utrecht",
            Address = "Vredenburg 50",
            ContactEmail = "info@demo-sampling-fmo.nl",
            ContactPhone = "+31301234567",
            TaxNumber = "NL333333333B01",
            TaxOffice = "Utrecht"
        };
        await Testing.AddAsync(firm);

        var uniqueMoCode = $"ENT_{Guid.NewGuid():N}";
        var marketOperator = new MarketOperator
        {
            Name = "Demo ENTSO-E Sampling",
            Code = uniqueMoCode,
            BaseUrl = "https://web-api.tp.entsoe.eu/api",
            SupportedResolutions = new List<int> { 15, 30, 60 },
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

        var command = new UpdateFirmMarketOperatorCommand
        {
            Id = firmMarketOperator.Id,
            MarketOperatorId = marketOperator.Id,
            AreaCode = firmMarketOperator.AreaCode,
            SamplingPeriodMinutes = 15,
            IsActive = true
        };

        // Act
        await Testing.SendAsync(command);

        // Assert
        var updatedFmo = await Testing.FindAsync<FirmMarketOperator>(firmMarketOperator.Id);
        updatedFmo!.SamplingPeriodMinutes.Should().Be(15);
    }
}
