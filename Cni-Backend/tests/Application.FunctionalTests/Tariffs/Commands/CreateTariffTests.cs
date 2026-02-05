using Backend.Application.Common.Exceptions;
using Backend.Application.Tariffs.Commands.CreateTariff;
using Backend.Domain.Entities;

namespace Backend.Application.FunctionalTests.Tariffs.Commands;

public class CreateTariffTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireCountryName()
    {
        await Testing.RunAsAdministratorAsync();
        var command = new CreateTariffCommand
        {
            CountryName = "",
            DistributionFee = 20m,
            TransmissionFee = 5m,
            TaxRate = 0.21m
        };

        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task ShouldCreateTariff()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();
        var command = new CreateTariffCommand
        {
            CountryName = "Netherlands",
            DistributorName = "Liander",
            DistributionFee = 25.5m,
            TransmissionFee = 6.0m,
            TaxRate = 0.21m,
            Currency = "EUR"
        };

        // Act
        var tariffId = await Testing.SendAsync(command);

        // Assert
        var tariff = await Testing.FindAsync<Tariff>(tariffId);

        tariff.Should().NotBeNull();
        tariff!.CountryName.Should().Be("Netherlands");
        tariff.DistributorName.Should().Be("Liander");
        tariff.DistributionFee.Should().Be(25.5m);
        tariff.TransmissionFee.Should().Be(6.0m);
        tariff.TaxRate.Should().Be(0.21m);
        tariff.Currency.Should().Be("EUR");
        tariff.IsActive.Should().BeTrue();
    }

    [Test]
    public async Task ShouldCreateTariffWithDefaultCurrency()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();
        var command = new CreateTariffCommand
        {
            CountryName = "Germany",
            DistributionFee = 30m,
            TransmissionFee = 8m,
            TaxRate = 0.19m
        };

        // Act
        var tariffId = await Testing.SendAsync(command);

        // Assert
        var tariff = await Testing.FindAsync<Tariff>(tariffId);
        tariff!.Currency.Should().Be("EUR");
    }

    [Test]
    public async Task ShouldCreateTariffWithDifferentCurrency()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();
        var command = new CreateTariffCommand
        {
            CountryName = "Türkiye",
            DistributorName = "TEDAS",
            DistributionFee = 150m,
            TransmissionFee = 50m,
            TaxRate = 0.20m,
            Currency = "TRY"
        };

        // Act
        var tariffId = await Testing.SendAsync(command);

        // Assert
        var tariff = await Testing.FindAsync<Tariff>(tariffId);
        tariff!.Currency.Should().Be("TRY");
    }

    [Test]
    public async Task ShouldDeactivateExistingTariffsForSameCountry()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        // Create first tariff
        var existingTariff = new Tariff
        {
            CountryName = "France",
            DistributionFee = 20m,
            TransmissionFee = 5m,
            TaxRate = 0.20m,
            Currency = "EUR",
            IsActive = true
        };
        await Testing.AddAsync(existingTariff);

        // Create second tariff for same country
        var command = new CreateTariffCommand
        {
            CountryName = "France",
            DistributionFee = 22m,
            TransmissionFee = 6m,
            TaxRate = 0.20m,
            Currency = "EUR"
        };

        // Act
        var newTariffId = await Testing.SendAsync(command);

        // Assert
        var oldTariff = await Testing.FindAsync<Tariff>(existingTariff.Id);
        var newTariff = await Testing.FindAsync<Tariff>(newTariffId);

        oldTariff!.IsActive.Should().BeFalse();
        newTariff!.IsActive.Should().BeTrue();
    }

    [Test]
    public async Task ShouldCreateTariffWithoutDistributorName()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();
        var command = new CreateTariffCommand
        {
            CountryName = "Spain",
            DistributorName = null,
            DistributionFee = 18m,
            TransmissionFee = 4m,
            TaxRate = 0.21m,
            Currency = "EUR"
        };

        // Act
        var tariffId = await Testing.SendAsync(command);

        // Assert
        var tariff = await Testing.FindAsync<Tariff>(tariffId);
        tariff!.DistributorName.Should().BeNull();
    }

    [Test]
    public async Task ShouldRejectNegativeDistributionFee()
    {
        await Testing.RunAsAdministratorAsync();
        var command = new CreateTariffCommand
        {
            CountryName = "Italy",
            DistributionFee = -10m,
            TransmissionFee = 5m,
            TaxRate = 0.22m
        };

        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task ShouldRejectNegativeTransmissionFee()
    {
        await Testing.RunAsAdministratorAsync();
        var command = new CreateTariffCommand
        {
            CountryName = "Italy",
            DistributionFee = 20m,
            TransmissionFee = -5m,
            TaxRate = 0.22m
        };

        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task ShouldRejectNegativeTaxRate()
    {
        await Testing.RunAsAdministratorAsync();
        var command = new CreateTariffCommand
        {
            CountryName = "Italy",
            DistributionFee = 20m,
            TransmissionFee = 5m,
            TaxRate = -0.10m
        };

        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task ShouldRejectTaxRateGreaterThanOne()
    {
        await Testing.RunAsAdministratorAsync();
        var command = new CreateTariffCommand
        {
            CountryName = "Italy",
            DistributionFee = 20m,
            TransmissionFee = 5m,
            TaxRate = 1.5m
        };

        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task ShouldHandleCaseInsensitiveCountryDeactivation()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        // Create first tariff with lowercase
        var existingTariff = new Tariff
        {
            CountryName = "netherlands",
            DistributionFee = 20m,
            TransmissionFee = 5m,
            TaxRate = 0.21m,
            Currency = "EUR",
            IsActive = true
        };
        await Testing.AddAsync(existingTariff);

        // Create second tariff with different case
        var command = new CreateTariffCommand
        {
            CountryName = "Netherlands",
            DistributionFee = 22m,
            TransmissionFee = 6m,
            TaxRate = 0.21m,
            Currency = "EUR"
        };

        // Act
        var newTariffId = await Testing.SendAsync(command);

        // Assert
        var oldTariff = await Testing.FindAsync<Tariff>(existingTariff.Id);
        oldTariff!.IsActive.Should().BeFalse();
    }
}
