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

        var tenant = new Tenant { Name = "Deactivate FT Tenant", Identifier = "deactivate-ft-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Deactivate FT Firm",
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

        var facility = new Facility
        {
            FirmId = firm.Id,
            Name = "Deactivate FT Facility"
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

        var tenant = new Tenant { Name = "Idempotent FT Tenant", Identifier = "idempotent-ft-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Idempotent FT Firm",
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

        var facility = new Facility
        {
            FirmId = firm.Id,
            Name = "Idempotent FT Facility"
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

        var tenant = new Tenant { Name = "Preserve Contract Tenant", Identifier = "preserve-contract-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Preserve Contract Firm",
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

        var facility = new Facility
        {
            FirmId = firm.Id,
            Name = "Preserve Contract Facility"
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
