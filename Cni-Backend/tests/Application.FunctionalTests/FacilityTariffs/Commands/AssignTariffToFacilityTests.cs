using Backend.Application.Common.Exceptions;
using Backend.Application.FacilityTariffs.Commands.AssignTariffToFacility;
using Backend.Domain.Entities;

namespace Backend.Application.FunctionalTests.FacilityTariffs.Commands;

public class AssignTariffToFacilityTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireValidFacilityId()
    {
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

        var command = new AssignTariffToFacilityCommand
        {
            FacilityId = 99999,
            TariffId = tariff.Id
        };

        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task ShouldRequireValidTariffId()
    {
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Assign Tenant", Identifier = "assign-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Assign Firm",
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
            Name = "Assign Facility"
        };
        await Testing.AddAsync(facility);

        var command = new AssignTariffToFacilityCommand
        {
            FacilityId = facility.Id,
            TariffId = 99999
        };

        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task ShouldAssignTariffToFacility()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Assign Tariff Tenant", Identifier = "assign-tariff-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Assign Tariff Firm",
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
            Name = "Assign Tariff Facility"
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

        var command = new AssignTariffToFacilityCommand
        {
            FacilityId = facility.Id,
            TariffId = tariff.Id
        };

        // Act
        var facilityTariffId = await Testing.SendAsync(command);

        // Assert
        var facilityTariff = await Testing.FindAsync<FacilityTariff>(facilityTariffId);

        facilityTariff.Should().NotBeNull();
        facilityTariff!.FacilityId.Should().Be(facility.Id);
        facilityTariff.TariffId.Should().Be(tariff.Id);
        facilityTariff.IsActive.Should().BeTrue();
    }

    [Test]
    public async Task ShouldAssignTariffWithContractDetails()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Contract Tenant", Identifier = "contract-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Contract Firm",
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
            Name = "Contract Facility"
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

        var startDate = DateTimeOffset.UtcNow;
        var endDate = DateTimeOffset.UtcNow.AddYears(2);

        var command = new AssignTariffToFacilityCommand
        {
            FacilityId = facility.Id,
            TariffId = tariff.Id,
            ContractNumber = "CONTRACT-2024-001",
            DiscountRate = 0.10m,
            ContractStartDate = startDate,
            ContractEndDate = endDate,
            Notes = "Special enterprise agreement"
        };

        // Act
        var facilityTariffId = await Testing.SendAsync(command);

        // Assert
        var facilityTariff = await Testing.FindAsync<FacilityTariff>(facilityTariffId);

        facilityTariff.Should().NotBeNull();
        facilityTariff!.ContractNumber.Should().Be("CONTRACT-2024-001");
        facilityTariff.DiscountRate.Should().Be(0.10m);
        facilityTariff.ContractStartDate.Should().BeCloseTo(startDate, TimeSpan.FromSeconds(1));
        facilityTariff.ContractEndDate.Should().BeCloseTo(endDate, TimeSpan.FromSeconds(1));
        facilityTariff.Notes.Should().Be("Special enterprise agreement");
    }

    [Test]
    public async Task ShouldDeactivateExistingAssignmentWhenAssigningNew()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Deactivate Tenant", Identifier = "deactivate-assign-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Deactivate Firm",
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
            Name = "Deactivate Facility"
        };
        await Testing.AddAsync(facility);

        var tariff1 = new Tariff
        {
            CountryName = "Netherlands",
            DistributionFee = 20m,
            TransmissionFee = 5m,
            TaxRate = 0.21m,
            Currency = "EUR",
            IsActive = true
        };
        await Testing.AddAsync(tariff1);

        var tariff2 = new Tariff
        {
            CountryName = "Germany",
            DistributionFee = 30m,
            TransmissionFee = 8m,
            TaxRate = 0.19m,
            Currency = "EUR",
            IsActive = true
        };
        await Testing.AddAsync(tariff2);

        // Create first assignment
        var existingAssignment = new FacilityTariff
        {
            FacilityId = facility.Id,
            TariffId = tariff1.Id,
            IsActive = true
        };
        await Testing.AddAsync(existingAssignment);

        // Assign new tariff
        var command = new AssignTariffToFacilityCommand
        {
            FacilityId = facility.Id,
            TariffId = tariff2.Id
        };

        // Act
        var newAssignmentId = await Testing.SendAsync(command);

        // Assert
        var oldAssignment = await Testing.FindAsync<FacilityTariff>(existingAssignment.Id);
        var newAssignment = await Testing.FindAsync<FacilityTariff>(newAssignmentId);

        oldAssignment!.IsActive.Should().BeFalse();
        newAssignment!.IsActive.Should().BeTrue();
    }

    [Test]
    public async Task ShouldRejectInactiveTariff()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Inactive Tariff Tenant", Identifier = "inactive-tariff-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Inactive Tariff Firm",
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
            Name = "Inactive Tariff Facility"
        };
        await Testing.AddAsync(facility);

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

        var command = new AssignTariffToFacilityCommand
        {
            FacilityId = facility.Id,
            TariffId = inactiveTariff.Id
        };

        // Act & Assert
        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not active*");
    }

    [Test]
    public async Task ShouldAssignTariffWithZeroDiscount()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Zero Discount Tenant", Identifier = "zero-discount-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Zero Discount Firm",
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

        var facility = new Facility
        {
            FirmId = firm.Id,
            Name = "Zero Discount Facility"
        };
        await Testing.AddAsync(facility);

        var tariff = new Tariff
        {
            CountryName = "Germany",
            DistributionFee = 30m,
            TransmissionFee = 8m,
            TaxRate = 0.19m,
            Currency = "EUR",
            IsActive = true
        };
        await Testing.AddAsync(tariff);

        var command = new AssignTariffToFacilityCommand
        {
            FacilityId = facility.Id,
            TariffId = tariff.Id,
            DiscountRate = 0m
        };

        // Act
        var facilityTariffId = await Testing.SendAsync(command);

        // Assert
        var facilityTariff = await Testing.FindAsync<FacilityTariff>(facilityTariffId);
        facilityTariff!.DiscountRate.Should().Be(0m);
    }
}
