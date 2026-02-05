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
        var uniqueMoCode = $"EPI_{Guid.NewGuid():N}";
        var marketOperator = new MarketOperator
        {
            Name = "EPIAS Test",
            Code = uniqueMoCode,
            BaseUrl = "https://seffaflik.epias.com.tr/transparency",
            SupportedResolutions = new List<int> { 60 },
            AreaCodes = new List<string> { "TR" },
            IsActive = true
        };
        await Testing.AddAsync(marketOperator);

        var command = new UpdateFirmMarketOperatorCommand
        {
            Id = 99999,
            MarketOperatorId = marketOperator.Id,
            AreaCode = "TR",
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

        var tenant = new Tenant { Name = "Update FMO Tenant", Identifier = "update-fmo-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Update FMO Firm",
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

        var uniqueMoCode1 = $"EPI_{Guid.NewGuid():N}";
        var marketOperator1 = new MarketOperator
        {
            Name = "EPIAS",
            Code = uniqueMoCode1,
            BaseUrl = "https://seffaflik.epias.com.tr/transparency",
            SupportedResolutions = new List<int> { 60 },
            AreaCodes = new List<string> { "TR" },
            IsActive = true
        };
        await Testing.AddAsync(marketOperator1);

        var uniqueMoCode2 = $"ENT_{Guid.NewGuid():N}";
        var marketOperator2 = new MarketOperator
        {
            Name = "ENTSO-E",
            Code = uniqueMoCode2,
            BaseUrl = "https://web-api.tp.entsoe.eu/api",
            SupportedResolutions = new List<int> { 15, 60 },
            AreaCodes = new List<string> { "10YTR-TEIAS----W" },
            IsActive = true
        };
        await Testing.AddAsync(marketOperator2);

        var firmMarketOperator = new FirmMarketOperator
        {
            FirmId = firm.Id,
            MarketOperatorId = marketOperator1.Id,
            AreaCode = "TR",
            SamplingPeriodMinutes = 60,
            IsActive = true
        };
        await Testing.AddAsync(firmMarketOperator);

        var command = new UpdateFirmMarketOperatorCommand
        {
            Id = firmMarketOperator.Id,
            MarketOperatorId = marketOperator2.Id,
            AreaCode = "10YTR-TEIAS----W",
            SamplingPeriodMinutes = 15,
            IsActive = true
        };

        // Act
        await Testing.SendAsync(command);

        // Assert
        var updatedFmo = await Testing.FindAsync<FirmMarketOperator>(firmMarketOperator.Id);

        updatedFmo.Should().NotBeNull();
        updatedFmo!.MarketOperatorId.Should().Be(marketOperator2.Id);
        updatedFmo.AreaCode.Should().Be("10YTR-TEIAS----W");
        updatedFmo.SamplingPeriodMinutes.Should().Be(15);
    }

    [Test]
    public async Task ShouldDeactivateFirmMarketOperator()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Deactivate FMO Tenant", Identifier = "deactivate-fmo-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Deactivate FMO Firm",
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

        var tenant = new Tenant { Name = "Reactivate FMO Tenant", Identifier = "reactivate-fmo-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Reactivate FMO Firm",
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
            SupportedResolutions = new List<int> { 60 },
            AreaCodes = new List<string> { "10YDE-VE-------2" },
            IsActive = true
        };
        await Testing.AddAsync(marketOperator);

        var firmMarketOperator = new FirmMarketOperator
        {
            FirmId = firm.Id,
            MarketOperatorId = marketOperator.Id,
            AreaCode = "10YDE-VE-------2",
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

        var tenant = new Tenant { Name = "Sampling FMO Tenant", Identifier = "sampling-fmo-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Sampling FMO Firm",
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
            SupportedResolutions = new List<int> { 15, 30, 60 },
            AreaCodes = new List<string> { "10YBE----------2" },
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
