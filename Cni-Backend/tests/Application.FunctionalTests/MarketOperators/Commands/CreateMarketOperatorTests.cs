using Backend.Application.Common.Exceptions;
using Backend.Application.MarketOperators.Commands.CreateMarketOperator;
using Backend.Domain.Entities;

namespace Backend.Application.FunctionalTests.MarketOperators.Commands;

public class CreateMarketOperatorTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireMinimumFields()
    {
        await Testing.RunAsAdministratorAsync();
        var command = new CreateMarketOperatorCommand();

        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task ShouldRequireName()
    {
        await Testing.RunAsAdministratorAsync();
        var command = new CreateMarketOperatorCommand
        {
            Name = "",
            Code = "TEST",
            BaseUrl = "https://api.test.com"
        };

        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task ShouldRequireCode()
    {
        await Testing.RunAsAdministratorAsync();
        var command = new CreateMarketOperatorCommand
        {
            Name = "Test Operator",
            Code = "",
            BaseUrl = "https://api.test.com"
        };

        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task ShouldRequireBaseUrl()
    {
        await Testing.RunAsAdministratorAsync();
        var command = new CreateMarketOperatorCommand
        {
            Name = "Test Operator",
            Code = "TEST",
            BaseUrl = ""
        };

        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task ShouldCreateMarketOperator()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();
        var uniqueCode = $"ENT_{Guid.NewGuid():N}";
        var command = new CreateMarketOperatorCommand
        {
            Name = "ENTSO-E",
            Code = uniqueCode,
            BaseUrl = "https://web-api.tp.entsoe.eu/api",
            ApiKey = "test-api-key",
            SupportedResolutions = new List<int> { 15, 60 },
            AreaCodes = new List<string> { "10YNL----------L", "10YDE-VE-------2" }
        };

        // Act
        var marketOperatorId = await Testing.SendAsync(command);

        // Assert
        var marketOperator = await Testing.FindAsync<MarketOperator>(marketOperatorId);

        marketOperator.Should().NotBeNull();
        marketOperator!.Name.Should().Be("ENTSO-E");
        marketOperator.Code.Should().Be(uniqueCode);
        marketOperator.BaseUrl.Should().Be("https://web-api.tp.entsoe.eu/api");
        marketOperator.ApiKey.Should().Be("test-api-key");
        marketOperator.SupportedResolutions.Should().BeEquivalentTo(new List<int> { 15, 60 });
        marketOperator.AreaCodes.Should().BeEquivalentTo(new List<string> { "10YNL----------L", "10YDE-VE-------2" });
        marketOperator.IsActive.Should().BeTrue();
    }

    [Test]
    public async Task ShouldCreateMarketOperatorWithDefaultResolutions()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();
        var uniqueCode = $"EPI_{Guid.NewGuid():N}";
        var command = new CreateMarketOperatorCommand
        {
            Name = "EPIAS",
            Code = uniqueCode,
            BaseUrl = "https://seffaflik.epias.com.tr/transparency",
            AreaCodes = new List<string> { "TR" }
        };

        // Act
        var marketOperatorId = await Testing.SendAsync(command);

        // Assert
        var marketOperator = await Testing.FindAsync<MarketOperator>(marketOperatorId);

        marketOperator.Should().NotBeNull();
        marketOperator!.SupportedResolutions.Should().BeEquivalentTo(new List<int> { 60 });
    }

    [Test]
    public async Task ShouldCreateMarketOperatorWithoutApiKey()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();
        var uniqueCode = $"PUB_{Guid.NewGuid():N}";
        var command = new CreateMarketOperatorCommand
        {
            Name = "Public Operator",
            Code = uniqueCode,
            BaseUrl = "https://api.public.com",
            ApiKey = null,
            AreaCodes = new List<string> { "PUBLIC" }
        };

        // Act
        var marketOperatorId = await Testing.SendAsync(command);

        // Assert
        var marketOperator = await Testing.FindAsync<MarketOperator>(marketOperatorId);

        marketOperator.Should().NotBeNull();
        marketOperator!.ApiKey.Should().BeNull();
    }

    [Test]
    public async Task ShouldRejectNameExceedingMaxLength()
    {
        await Testing.RunAsAdministratorAsync();
        var command = new CreateMarketOperatorCommand
        {
            Name = new string('A', 201),
            Code = "TEST",
            BaseUrl = "https://api.test.com"
        };

        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task ShouldRejectCodeExceedingMaxLength()
    {
        await Testing.RunAsAdministratorAsync();
        var command = new CreateMarketOperatorCommand
        {
            Name = "Test Operator",
            Code = new string('A', 51),
            BaseUrl = "https://api.test.com"
        };

        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<ValidationException>();
    }
}
