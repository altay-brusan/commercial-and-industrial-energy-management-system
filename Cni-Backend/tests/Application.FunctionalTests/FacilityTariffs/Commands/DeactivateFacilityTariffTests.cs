using Backend.Application.Common.Exceptions;
using Backend.Application.FacilityTariffs.Commands.DeactivateFacilityTariff;
using Backend.Domain.Entities;

namespace Backend.Application.FunctionalTests.FacilityTariffs.Commands;

public class DeactivateFacilityTariffTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireValidFacilityTariffId()
    {
        await Testing.RunAsAdministratorAsync();

        var command = new DeactivateFacilityTariffCommand
        {
            Id = 99999
        };

        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task ShouldDeactivateFacilityTariff()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Demo Deactivate FT Energy Holding", Identifier = "demo-deactivate-ft-energy-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Deactivate FT Solar BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Noord-Holland",
            City = "Amsterdam",
            Address = "Damrak 100",
            ContactEmail = "info@demo-deactivate-ft-solar.nl",
            ContactPhone = "+31201234567",
            TaxNumber = "NL123456789B01",
            TaxOffice = "Amsterdam"
        };
        await Testing.AddAsync(firm);

        var facility = new Facility
        {
            FirmId = firm.Id,
            Name = "Demo Solar Park Hoofddorp"
        };
        await Testing.AddAsync(facility);

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

        var facilityTariff = new FacilityTariff
        {
            FacilityId = facility.Id,
            TariffId = tariff.Id,
            IsActive = true
        };
        await Testing.AddAsync(facilityTariff);

        var command = new DeactivateFacilityTariffCommand
        {
            Id = facilityTariff.Id
        };

        // Act
        var result = await Testing.SendAsync(command);

        // Assert
        result.Should().BeTrue();

        var deactivatedFt = await Testing.FindAsync<FacilityTariff>(facilityTariff.Id);
        deactivatedFt!.IsActive.Should().BeFalse();
    }

    [Test]
    public async Task ShouldBeIdempotentForAlreadyInactiveFacilityTariff()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Demo Idempotent FT Energy Holding", Identifier = "demo-idempotent-ft-energy-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Idempotent FT Wind BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Zuid-Holland",
            City = "Den Haag",
            Address = "Lange Voorhout 50",
            ContactEmail = "info@demo-idempotent-ft-wind.nl",
            ContactPhone = "+31701234567",
            TaxNumber = "NL987654321B01",
            TaxOffice = "Den Haag"
        };
        await Testing.AddAsync(firm);

        var facility = new Facility
        {
            FirmId = firm.Id,
            Name = "Demo Wind Farm Westland"
        };
        await Testing.AddAsync(facility);

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

        var facilityTariff = new FacilityTariff
        {
            FacilityId = facility.Id,
            TariffId = tariff.Id,
            IsActive = false // Already inactive
        };
        await Testing.AddAsync(facilityTariff);

        var command = new DeactivateFacilityTariffCommand
        {
            Id = facilityTariff.Id
        };

        // Act
        var result = await Testing.SendAsync(command);

        // Assert
        result.Should().BeTrue();

        var stillInactiveFt = await Testing.FindAsync<FacilityTariff>(facilityTariff.Id);
        stillInactiveFt!.IsActive.Should().BeFalse();
    }

    [Test]
    public async Task ShouldPreserveContractDetailsWhenDeactivating()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Demo Preserve Contract Energy Holding", Identifier = "demo-preserve-contract-energy-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Preserve Contract Solar BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Utrecht",
            City = "Utrecht",
            Address = "Oudegracht 100",
            ContactEmail = "info@demo-preserve-contract-solar.nl",
            ContactPhone = "+31301234567",
            TaxNumber = "NL555555555B01",
            TaxOffice = "Utrecht"
        };
        await Testing.AddAsync(firm);

        var facility = new Facility
        {
            FirmId = firm.Id,
            Name = "Demo Solar Farm Nieuwegein"
        };
        await Testing.AddAsync(facility);

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

        var facilityTariff = new FacilityTariff
        {
            FacilityId = facility.Id,
            TariffId = tariff.Id,
            IsActive = true,
            ContractNumber = "CONTRACT-PRESERVE-001",
            DiscountRate = 0.10m,
            Notes = "Important contract details"
        };
        await Testing.AddAsync(facilityTariff);

        var command = new DeactivateFacilityTariffCommand
        {
            Id = facilityTariff.Id
        };

        // Act
        await Testing.SendAsync(command);

        // Assert
        var deactivatedFt = await Testing.FindAsync<FacilityTariff>(facilityTariff.Id);

        deactivatedFt!.IsActive.Should().BeFalse();
        deactivatedFt.ContractNumber.Should().Be("CONTRACT-PRESERVE-001");
        deactivatedFt.DiscountRate.Should().Be(0.10m);
        deactivatedFt.Notes.Should().Be("Important contract details");
    }
}
