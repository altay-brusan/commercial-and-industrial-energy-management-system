using Backend.Application.Common.Exceptions;
using Backend.Application.Tariffs.Commands.UpdateTariff;
using Backend.Domain.Entities;

namespace Backend.Application.FunctionalTests.Tariffs.Commands;

public class UpdateTariffTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireValidTariffId()
    {
        await Testing.RunAsAdministratorAsync();
        var command = new UpdateTariffCommand
        {
            Id = 99999,
            CountryName = "Netherlands",
            DistributionFee = 20m,
            TransmissionFee = 5m,
            TaxRate = 0.21m
        };

        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task ShouldUpdateTariffByCreatingNewVersion()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var existingTariff = new Tariff
        {
            CountryName = "Netherlands",
            DistributorName = "Liander",
            DistributionFee = 20m,
            TransmissionFee = 5m,
            TaxRate = 0.21m,
            Currency = "EUR",
            IsActive = true
        };
        await Testing.AddAsync(existingTariff);

        var command = new UpdateTariffCommand
        {
            Id = existingTariff.Id,
            CountryName = "Netherlands",
            DistributorName = "Liander",
            DistributionFee = 25m,
            TransmissionFee = 7m,
            TaxRate = 0.21m,
            Currency = "EUR"
        };

        // Act
        var result = await Testing.SendAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.OldTariffId.Should().Be(existingTariff.Id);
        result.NewTariffId.Should().NotBe(existingTariff.Id);

        var oldTariff = await Testing.FindAsync<Tariff>(existingTariff.Id);
        var newTariff = await Testing.FindAsync<Tariff>(result.NewTariffId);

        oldTariff!.IsActive.Should().BeFalse();
        newTariff!.IsActive.Should().BeTrue();
        newTariff.DistributionFee.Should().Be(25m);
        newTariff.TransmissionFee.Should().Be(7m);
    }

    [Test]
    public async Task ShouldRejectUpdateOfInactiveTariff()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var inactiveTariff = new Tariff
        {
            CountryName = "Netherlands",
            DistributionFee = 20m,
            TransmissionFee = 5m,
            TaxRate = 0.21m,
            Currency = "EUR",
            IsActive = false
        };
        await Testing.AddAsync(inactiveTariff);

        var command = new UpdateTariffCommand
        {
            Id = inactiveTariff.Id,
            CountryName = "Netherlands",
            DistributionFee = 25m,
            TransmissionFee = 7m,
            TaxRate = 0.21m,
            Currency = "EUR"
        };

        // Act & Assert
        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already been deactivated*");
    }

    [Test]
    public async Task ShouldMigrateFacilityAssignmentsToNewTariff()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Demo Migration Tariff Energy Holding", Identifier = "demo-migration-tariff-energy-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Migration Tariff Solar BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Noord-Holland",
            City = "Amsterdam",
            Address = "Vijzelstraat 100",
            ContactEmail = "info@demo-migration-tariff-solar.nl",
            ContactPhone = "+31201234567",
            TaxNumber = "NL123456789B01",
            TaxOffice = "Amsterdam"
        };
        await Testing.AddAsync(firm);

        var facility = new Facility
        {
            FirmId = firm.Id,
            Name = "Demo Solar Park Schiphol"
        };
        await Testing.AddAsync(facility);

        var existingTariff = new Tariff
        {
            CountryName = "Netherlands",
            DistributionFee = 20m,
            TransmissionFee = 5m,
            TaxRate = 0.21m,
            Currency = "EUR",
            IsActive = true
        };
        await Testing.AddAsync(existingTariff);

        var facilityTariff = new FacilityTariff
        {
            FacilityId = facility.Id,
            TariffId = existingTariff.Id,
            IsActive = true
        };
        await Testing.AddAsync(facilityTariff);

        var command = new UpdateTariffCommand
        {
            Id = existingTariff.Id,
            CountryName = "Netherlands",
            DistributionFee = 25m,
            TransmissionFee = 7m,
            TaxRate = 0.21m,
            Currency = "EUR"
        };

        // Act
        var result = await Testing.SendAsync(command);

        // Assert
        result.MigratedFacilitiesCount.Should().Be(1);

        var oldAssignment = await Testing.FindAsync<FacilityTariff>(facilityTariff.Id);
        oldAssignment!.IsActive.Should().BeFalse();
    }

    [Test]
    public async Task ShouldReturnZeroMigratedFacilitiesWhenNoneExist()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var existingTariff = new Tariff
        {
            CountryName = "Netherlands",
            DistributorName = "Stedin",
            DistributionFee = 20m,
            TransmissionFee = 5m,
            TaxRate = 0.21m,
            Currency = "EUR",
            IsActive = true
        };
        await Testing.AddAsync(existingTariff);

        var command = new UpdateTariffCommand
        {
            Id = existingTariff.Id,
            CountryName = "Netherlands",
            DistributorName = "Stedin",
            DistributionFee = 25m,
            TransmissionFee = 7m,
            TaxRate = 0.21m,
            Currency = "EUR"
        };

        // Act
        var result = await Testing.SendAsync(command);

        // Assert
        result.MigratedFacilitiesCount.Should().Be(0);
    }

    [Test]
    public async Task ShouldUpdateDistributorName()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var existingTariff = new Tariff
        {
            CountryName = "Netherlands",
            DistributorName = "Liander",
            DistributionFee = 20m,
            TransmissionFee = 5m,
            TaxRate = 0.21m,
            Currency = "EUR",
            IsActive = true
        };
        await Testing.AddAsync(existingTariff);

        var command = new UpdateTariffCommand
        {
            Id = existingTariff.Id,
            CountryName = "Netherlands",
            DistributorName = "Enexis",
            DistributionFee = 20m,
            TransmissionFee = 5m,
            TaxRate = 0.21m,
            Currency = "EUR"
        };

        // Act
        var result = await Testing.SendAsync(command);

        // Assert
        var newTariff = await Testing.FindAsync<Tariff>(result.NewTariffId);
        newTariff!.DistributorName.Should().Be("Enexis");
    }

    [Test]
    public async Task ShouldUpdateCurrency()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var existingTariff = new Tariff
        {
            CountryName = "Türkiye",
            DistributionFee = 100m,
            TransmissionFee = 30m,
            TaxRate = 0.20m,
            Currency = "EUR",
            IsActive = true
        };
        await Testing.AddAsync(existingTariff);

        var command = new UpdateTariffCommand
        {
            Id = existingTariff.Id,
            CountryName = "Türkiye",
            DistributionFee = 150m,
            TransmissionFee = 50m,
            TaxRate = 0.20m,
            Currency = "TRY"
        };

        // Act
        var result = await Testing.SendAsync(command);

        // Assert
        var newTariff = await Testing.FindAsync<Tariff>(result.NewTariffId);
        newTariff!.Currency.Should().Be("TRY");
    }
}
