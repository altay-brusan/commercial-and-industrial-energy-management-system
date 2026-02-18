using Backend.Application.Tariffs.Queries.GetTariff;
using Backend.Domain.Entities;

namespace Backend.Application.FunctionalTests.Tariffs.Queries;

public class GetTariffTests : BaseTestFixture
{
    [Test]
    public async Task ShouldReturnTariff()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tariff = new Tariff
        {
            CountryName = "Netherlands",
            DistributorName = "Liander",
            DistributionFee = 25m,
            TransmissionFee = 7m,
            TaxRate = 0.21m,
            Currency = "EUR",
            IsActive = true
        };
        await Testing.AddAsync(tariff);

        // Act
        var result = await Testing.SendAsync(new GetTariffQuery { Id = tariff.Id });

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(tariff.Id);
        result.CountryName.Should().Be("Netherlands");
        result.DistributorName.Should().Be("Liander");
        result.DistributionFee.Should().Be(25m);
        result.TransmissionFee.Should().Be(7m);
        result.TaxRate.Should().Be(0.21m);
        result.Currency.Should().Be("EUR");
        result.IsActive.Should().BeTrue();
    }

    [Test]
    public async Task ShouldReturnNullForNonExistentTariff()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        // Act
        var result = await Testing.SendAsync(new GetTariffQuery { Id = Guid.NewGuid() });

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task ShouldReturnInactiveTariff()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tariff = new Tariff
        {
            CountryName = "Germany",
            DistributionFee = 30m,
            TransmissionFee = 8m,
            TaxRate = 0.19m,
            Currency = "EUR",
            IsActive = false
        };
        await Testing.AddAsync(tariff);

        // Act
        var result = await Testing.SendAsync(new GetTariffQuery { Id = tariff.Id });

        // Assert
        result.Should().NotBeNull();
        result!.IsActive.Should().BeFalse();
    }

    [Test]
    public async Task ShouldReturnTariffWithoutDistributor()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tariff = new Tariff
        {
            CountryName = "Belgium",
            DistributorName = null,
            DistributionFee = 22m,
            TransmissionFee = 6m,
            TaxRate = 0.21m,
            Currency = "EUR",
            IsActive = true
        };
        await Testing.AddAsync(tariff);

        // Act
        var result = await Testing.SendAsync(new GetTariffQuery { Id = tariff.Id });

        // Assert
        result.Should().NotBeNull();
        result!.DistributorName.Should().BeNull();
    }

    [Test]
    public async Task ShouldReturnTariffWithNonEurCurrency()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tariff = new Tariff
        {
            CountryName = "Türkiye",
            DistributorName = "TEDAS",
            DistributionFee = 150m,
            TransmissionFee = 50m,
            TaxRate = 0.20m,
            Currency = "TRY",
            IsActive = true
        };
        await Testing.AddAsync(tariff);

        // Act
        var result = await Testing.SendAsync(new GetTariffQuery { Id = tariff.Id });

        // Assert
        result.Should().NotBeNull();
        result!.Currency.Should().Be("TRY");
    }
}
