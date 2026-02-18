using Backend.Application.MarketOperators.Queries.GetMarketOperator;
using Backend.Domain.Entities;

namespace Backend.Application.FunctionalTests.MarketOperators.Queries;

public class GetMarketOperatorTests : BaseTestFixture
{
    [Test]
    public async Task ShouldReturnMarketOperator()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();
        var uniqueCode = $"ENT_{Guid.NewGuid():N}";
        var marketOperator = new MarketOperator
        {
            Name = "ENTSO-E",
            Code = uniqueCode,
            BaseUrl = "https://web-api.tp.entsoe.eu/api",
            ApiKey = "test-key",
            SupportedResolutions = new List<int> { 15, 60 },
            AreaCodes = new List<string> { "10YNL----------L" },
            IsActive = true
        };
        await Testing.AddAsync(marketOperator);

        // Act
        var result = await Testing.SendAsync(new GetMarketOperatorQuery { Id = marketOperator.Id });

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(marketOperator.Id);
        result.Name.Should().Be("ENTSO-E");
        result.Code.Should().Be(uniqueCode);
        result.BaseUrl.Should().Be("https://web-api.tp.entsoe.eu/api");
    }

    [Test]
    public async Task ShouldReturnNullForNonExistentMarketOperator()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        // Act
        var result = await Testing.SendAsync(new GetMarketOperatorQuery { Id = Guid.NewGuid() });

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task ShouldReturnInactiveMarketOperator()
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

        // Act
        var result = await Testing.SendAsync(new GetMarketOperatorQuery { Id = marketOperator.Id });

        // Assert
        result.Should().NotBeNull();
        result!.IsActive.Should().BeFalse();
    }

    [Test]
    public async Task ShouldReturnAllProperties()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();
        var uniqueCode = $"CMP_{Guid.NewGuid():N}";
        var marketOperator = new MarketOperator
        {
            Name = "Complete Operator",
            Code = uniqueCode,
            BaseUrl = "https://api.complete.com",
            ApiKey = "secret-key",
            SupportedResolutions = new List<int> { 5, 15, 30, 60 },
            AreaCodes = new List<string> { "AREA1", "AREA2", "AREA3" },
            IsActive = true
        };
        await Testing.AddAsync(marketOperator);

        // Act
        var result = await Testing.SendAsync(new GetMarketOperatorQuery { Id = marketOperator.Id });

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Complete Operator");
        result.Code.Should().Be(uniqueCode);
        result.BaseUrl.Should().Be("https://api.complete.com");
        result.IsActive.Should().BeTrue();
    }
}
