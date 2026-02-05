using Backend.Application.Common.Exceptions;
using Backend.Application.Tariffs.Commands.DeactivateTariff;
using Backend.Domain.Entities;

namespace Backend.Application.FunctionalTests.Tariffs.Commands;

public class DeactivateTariffTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireValidTariffId()
    {
        await Testing.RunAsAdministratorAsync();
        var command = new DeactivateTariffCommand
        {
            Id = 99999
        };

        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task ShouldDeactivateTariff()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tariff = new Tariff
        {
            CountryName = "Netherlands",
            DistributionFee = 20m,
            TransmissionFee = 5m,
            TaxRate = 0.21m,
            Currency = "EUR",
            IsActive = true
        };
        await Testing.AddAsync(tariff);

        var command = new DeactivateTariffCommand
        {
            Id = tariff.Id
        };

        // Act
        var result = await Testing.SendAsync(command);

        // Assert
        result.Should().BeTrue();

        var deactivatedTariff = await Testing.FindAsync<Tariff>(tariff.Id);
        deactivatedTariff!.IsActive.Should().BeFalse();
    }

    [Test]
    public async Task ShouldBeIdempotentForAlreadyInactiveTariff()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tariff = new Tariff
        {
            CountryName = "Germany",
            DistributionFee = 20m,
            TransmissionFee = 5m,
            TaxRate = 0.19m,
            Currency = "EUR",
            IsActive = false
        };
        await Testing.AddAsync(tariff);

        var command = new DeactivateTariffCommand
        {
            Id = tariff.Id
        };

        // Act
        var result = await Testing.SendAsync(command);

        // Assert
        result.Should().BeTrue();

        var stillInactiveTariff = await Testing.FindAsync<Tariff>(tariff.Id);
        stillInactiveTariff!.IsActive.Should().BeFalse();
    }

    [Test]
    public async Task ShouldDeactivateTariffWithDistributor()
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

        var command = new DeactivateTariffCommand
        {
            Id = tariff.Id
        };

        // Act
        var result = await Testing.SendAsync(command);

        // Assert
        result.Should().BeTrue();

        var deactivatedTariff = await Testing.FindAsync<Tariff>(tariff.Id);
        deactivatedTariff!.IsActive.Should().BeFalse();
        deactivatedTariff.DistributorName.Should().Be("Liander"); // Should preserve other fields
    }

    [Test]
    public async Task ShouldDeactivateTariffWithNonEurCurrency()
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

        var command = new DeactivateTariffCommand
        {
            Id = tariff.Id
        };

        // Act
        var result = await Testing.SendAsync(command);

        // Assert
        result.Should().BeTrue();

        var deactivatedTariff = await Testing.FindAsync<Tariff>(tariff.Id);
        deactivatedTariff!.IsActive.Should().BeFalse();
        deactivatedTariff.Currency.Should().Be("TRY"); // Should preserve currency
    }
}
