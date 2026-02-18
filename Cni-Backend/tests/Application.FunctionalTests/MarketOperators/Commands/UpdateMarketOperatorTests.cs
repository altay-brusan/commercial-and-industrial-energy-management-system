using Backend.Application.Common.Exceptions;
using Backend.Application.MarketOperators.Commands.UpdateMarketOperator;
using Backend.Domain.Entities;

namespace Backend.Application.FunctionalTests.MarketOperators.Commands;

public class UpdateMarketOperatorTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireValidMarketOperatorId()
    {
        await Testing.RunAsAdministratorAsync();
        var uniqueCode = $"UPD_{Guid.NewGuid():N}";
        var command = new UpdateMarketOperatorCommand
        {
            Id = Guid.NewGuid(),
            Name = "Updated Name",
            Code = uniqueCode,
            BaseUrl = "https://api.updated.com",
            SupportedResolutions = new List<int> { 60 },
            AreaCodes = new List<string> { "AREA1" }
        };

        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task ShouldUpdateMarketOperator()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();
        var uniqueCode = $"ORI_{Guid.NewGuid():N}";
        var updatedCode = $"UPD_{Guid.NewGuid():N}";
        var marketOperator = new MarketOperator
        {
            Name = "Original Name",
            Code = uniqueCode,
            BaseUrl = "https://api.original.com",
            ApiKey = "old-key",
            SupportedResolutions = new List<int> { 60 },
            AreaCodes = new List<string> { "AREA1" },
            IsActive = true
        };
        await Testing.AddAsync(marketOperator);

        var command = new UpdateMarketOperatorCommand
        {
            Id = marketOperator.Id,
            Name = "Updated Name",
            Code = updatedCode,
            BaseUrl = "https://api.updated.com",
            ApiKey = "new-key",
            SupportedResolutions = new List<int> { 15, 60 },
            AreaCodes = new List<string> { "AREA1", "AREA2" },
            IsActive = true
        };

        // Act
        await Testing.SendAsync(command);

        // Assert
        var updatedOperator = await Testing.FindAsync<MarketOperator>(marketOperator.Id);

        updatedOperator.Should().NotBeNull();
        updatedOperator!.Name.Should().Be("Updated Name");
        updatedOperator.Code.Should().Be(updatedCode);
        updatedOperator.BaseUrl.Should().Be("https://api.updated.com");
        updatedOperator.ApiKey.Should().Be("new-key");
        updatedOperator.SupportedResolutions.Should().BeEquivalentTo(new List<int> { 15, 60 });
        updatedOperator.AreaCodes.Should().BeEquivalentTo(new List<string> { "AREA1", "AREA2" });
    }

    [Test]
    public async Task ShouldDeactivateMarketOperator()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();
        var uniqueCode = $"ACT_{Guid.NewGuid():N}";
        var marketOperator = new MarketOperator
        {
            Name = "Active Operator",
            Code = uniqueCode,
            BaseUrl = "https://api.active.com",
            SupportedResolutions = new List<int> { 60 },
            AreaCodes = new List<string> { "ACTIVE" },
            IsActive = true
        };
        await Testing.AddAsync(marketOperator);

        var command = new UpdateMarketOperatorCommand
        {
            Id = marketOperator.Id,
            Name = marketOperator.Name,
            Code = marketOperator.Code,
            BaseUrl = marketOperator.BaseUrl,
            SupportedResolutions = marketOperator.SupportedResolutions,
            AreaCodes = marketOperator.AreaCodes,
            IsActive = false
        };

        // Act
        await Testing.SendAsync(command);

        // Assert
        var updatedOperator = await Testing.FindAsync<MarketOperator>(marketOperator.Id);
        updatedOperator!.IsActive.Should().BeFalse();
    }

    [Test]
    public async Task ShouldReactivateMarketOperator()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();
        var uniqueCode = $"INA_{Guid.NewGuid():N}";
        var marketOperator = new MarketOperator
        {
            Name = "Inactive Operator",
            Code = uniqueCode,
            BaseUrl = "https://api.inactive.com",
            SupportedResolutions = new List<int> { 60 },
            AreaCodes = new List<string> { "INACTIVE" },
            IsActive = false
        };
        await Testing.AddAsync(marketOperator);

        var command = new UpdateMarketOperatorCommand
        {
            Id = marketOperator.Id,
            Name = marketOperator.Name,
            Code = marketOperator.Code,
            BaseUrl = marketOperator.BaseUrl,
            SupportedResolutions = marketOperator.SupportedResolutions,
            AreaCodes = marketOperator.AreaCodes,
            IsActive = true
        };

        // Act
        await Testing.SendAsync(command);

        // Assert
        var updatedOperator = await Testing.FindAsync<MarketOperator>(marketOperator.Id);
        updatedOperator!.IsActive.Should().BeTrue();
    }

    [Test]
    public async Task ShouldClearApiKey()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();
        var uniqueCode = $"KEY_{Guid.NewGuid():N}";
        var marketOperator = new MarketOperator
        {
            Name = "Operator With Key",
            Code = uniqueCode,
            BaseUrl = "https://api.withkey.com",
            ApiKey = "original-key",
            SupportedResolutions = new List<int> { 60 },
            AreaCodes = new List<string> { "WITHKEY" },
            IsActive = true
        };
        await Testing.AddAsync(marketOperator);

        var command = new UpdateMarketOperatorCommand
        {
            Id = marketOperator.Id,
            Name = marketOperator.Name,
            Code = marketOperator.Code,
            BaseUrl = marketOperator.BaseUrl,
            ApiKey = null,
            SupportedResolutions = marketOperator.SupportedResolutions,
            AreaCodes = marketOperator.AreaCodes,
            IsActive = true
        };

        // Act
        await Testing.SendAsync(command);

        // Assert
        var updatedOperator = await Testing.FindAsync<MarketOperator>(marketOperator.Id);
        updatedOperator!.ApiKey.Should().BeNull();
    }

    [Test]
    public async Task ShouldUpdateAreaCodes()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();
        var uniqueCode = $"ARE_{Guid.NewGuid():N}";
        var marketOperator = new MarketOperator
        {
            Name = "Area Operator",
            Code = uniqueCode,
            BaseUrl = "https://api.area.com",
            SupportedResolutions = new List<int> { 60 },
            AreaCodes = new List<string> { "OLD1", "OLD2" },
            IsActive = true
        };
        await Testing.AddAsync(marketOperator);

        var command = new UpdateMarketOperatorCommand
        {
            Id = marketOperator.Id,
            Name = marketOperator.Name,
            Code = marketOperator.Code,
            BaseUrl = marketOperator.BaseUrl,
            SupportedResolutions = marketOperator.SupportedResolutions,
            AreaCodes = new List<string> { "NEW1", "NEW2", "NEW3" },
            IsActive = true
        };

        // Act
        await Testing.SendAsync(command);

        // Assert
        var updatedOperator = await Testing.FindAsync<MarketOperator>(marketOperator.Id);
        updatedOperator!.AreaCodes.Should().BeEquivalentTo(new List<string> { "NEW1", "NEW2", "NEW3" });
    }

    [Test]
    public async Task ShouldUpdateSupportedResolutions()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();
        var uniqueCode = $"RES_{Guid.NewGuid():N}";
        var marketOperator = new MarketOperator
        {
            Name = "Resolution Operator",
            Code = uniqueCode,
            BaseUrl = "https://api.resolution.com",
            SupportedResolutions = new List<int> { 60 },
            AreaCodes = new List<string> { "RESOLUTION" },
            IsActive = true
        };
        await Testing.AddAsync(marketOperator);

        var command = new UpdateMarketOperatorCommand
        {
            Id = marketOperator.Id,
            Name = marketOperator.Name,
            Code = marketOperator.Code,
            BaseUrl = marketOperator.BaseUrl,
            SupportedResolutions = new List<int> { 5, 15, 30, 60 },
            AreaCodes = marketOperator.AreaCodes,
            IsActive = true
        };

        // Act
        await Testing.SendAsync(command);

        // Assert
        var updatedOperator = await Testing.FindAsync<MarketOperator>(marketOperator.Id);
        updatedOperator!.SupportedResolutions.Should().BeEquivalentTo(new List<int> { 5, 15, 30, 60 });
    }
}
