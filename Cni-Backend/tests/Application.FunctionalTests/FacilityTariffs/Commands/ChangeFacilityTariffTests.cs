using Backend.Application.Common.Exceptions;
using Backend.Application.FacilityTariffs.Commands.ChangeFacilityTariff;
using Backend.Domain.Entities;

namespace Backend.Application.FunctionalTests.FacilityTariffs.Commands;

public class ChangeFacilityTariffTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireValidAssignmentId()
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

        var command = new ChangeFacilityTariffCommand
        {
            CurrentAssignmentId = 99999,
            NewTariffId = tariff.Id
        };

        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task ShouldRequireValidNewTariffId()
    {
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Change Tariff Tenant", Identifier = "change-tariff-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Change Tariff Firm",
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
            Name = "Change Tariff Facility"
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

        var command = new ChangeFacilityTariffCommand
        {
            CurrentAssignmentId = facilityTariff.Id,
            NewTariffId = 99999
        };

        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task ShouldChangeFacilityTariff()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Change Success Tenant", Identifier = "change-success-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Change Success Firm",
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
            Name = "Change Success Facility"
        };
        await Testing.AddAsync(facility);

        var oldTariff = new Tariff
        {
            CountryName = "Netherlands",
            DistributionFee = 20m,
            TransmissionFee = 5m,
            TaxRate = 0.21m,
            Currency = "EUR",
            IsActive = true
        };
        await Testing.AddAsync(oldTariff);

        var newTariff = new Tariff
        {
            CountryName = "Germany",
            DistributionFee = 30m,
            TransmissionFee = 8m,
            TaxRate = 0.19m,
            Currency = "EUR",
            IsActive = true
        };
        await Testing.AddAsync(newTariff);

        var currentAssignment = new FacilityTariff
        {
            FacilityId = facility.Id,
            TariffId = oldTariff.Id,
            IsActive = true
        };
        await Testing.AddAsync(currentAssignment);

        var command = new ChangeFacilityTariffCommand
        {
            CurrentAssignmentId = currentAssignment.Id,
            NewTariffId = newTariff.Id
        };

        // Act
        var newAssignmentId = await Testing.SendAsync(command);

        // Assert
        var oldAssignment = await Testing.FindAsync<FacilityTariff>(currentAssignment.Id);
        var newAssignment = await Testing.FindAsync<FacilityTariff>(newAssignmentId);

        oldAssignment!.IsActive.Should().BeFalse();
        newAssignment!.IsActive.Should().BeTrue();
        newAssignment.TariffId.Should().Be(newTariff.Id);
        newAssignment.FacilityId.Should().Be(facility.Id);
    }

    [Test]
    public async Task ShouldChangeFacilityTariffWithContractDetails()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Change Contract Tenant", Identifier = "change-contract-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Change Contract Firm",
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
            Name = "Change Contract Facility"
        };
        await Testing.AddAsync(facility);

        var oldTariff = new Tariff
        {
            CountryName = "Netherlands",
            DistributionFee = 20m,
            TransmissionFee = 5m,
            TaxRate = 0.21m,
            Currency = "EUR",
            IsActive = true
        };
        await Testing.AddAsync(oldTariff);

        var newTariff = new Tariff
        {
            CountryName = "Belgium",
            DistributionFee = 22m,
            TransmissionFee = 6m,
            TaxRate = 0.21m,
            Currency = "EUR",
            IsActive = true
        };
        await Testing.AddAsync(newTariff);

        var currentAssignment = new FacilityTariff
        {
            FacilityId = facility.Id,
            TariffId = oldTariff.Id,
            IsActive = true
        };
        await Testing.AddAsync(currentAssignment);

        var startDate = DateTimeOffset.UtcNow;
        var endDate = DateTimeOffset.UtcNow.AddYears(3);

        var command = new ChangeFacilityTariffCommand
        {
            CurrentAssignmentId = currentAssignment.Id,
            NewTariffId = newTariff.Id,
            ContractNumber = "CONTRACT-CHANGE-001",
            DiscountRate = 0.15m,
            ContractStartDate = startDate,
            ContractEndDate = endDate,
            Notes = "Renegotiated contract"
        };

        // Act
        var newAssignmentId = await Testing.SendAsync(command);

        // Assert
        var newAssignment = await Testing.FindAsync<FacilityTariff>(newAssignmentId);

        newAssignment!.ContractNumber.Should().Be("CONTRACT-CHANGE-001");
        newAssignment.DiscountRate.Should().Be(0.15m);
        newAssignment.ContractStartDate.Should().BeCloseTo(startDate, TimeSpan.FromSeconds(1));
        newAssignment.ContractEndDate.Should().BeCloseTo(endDate, TimeSpan.FromSeconds(1));
        newAssignment.Notes.Should().Be("Renegotiated contract");
    }

    [Test]
    public async Task ShouldRejectChangeFromInactiveAssignment()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Inactive Assignment Tenant", Identifier = "inactive-assignment-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Inactive Assignment Firm",
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
            Name = "Inactive Assignment Facility"
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

        var newTariff = new Tariff
        {
            CountryName = "Germany",
            DistributionFee = 30m,
            TransmissionFee = 8m,
            TaxRate = 0.19m,
            Currency = "EUR",
            IsActive = true
        };
        await Testing.AddAsync(newTariff);

        var inactiveAssignment = new FacilityTariff
        {
            FacilityId = facility.Id,
            TariffId = tariff.Id,
            IsActive = false
        };
        await Testing.AddAsync(inactiveAssignment);

        var command = new ChangeFacilityTariffCommand
        {
            CurrentAssignmentId = inactiveAssignment.Id,
            NewTariffId = newTariff.Id
        };

        // Act & Assert
        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already inactive*");
    }

    [Test]
    public async Task ShouldRejectChangeToInactiveTariff()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Inactive New Tariff Tenant", Identifier = "inactive-new-tariff-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Inactive New Tariff Firm",
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
            Name = "Inactive New Tariff Facility"
        };
        await Testing.AddAsync(facility);

        var activeTariff = new Tariff
        {
            CountryName = "Netherlands",
            DistributionFee = 20m,
            TransmissionFee = 5m,
            TaxRate = 0.21m,
            Currency = "EUR",
            IsActive = true
        };
        await Testing.AddAsync(activeTariff);

        var inactiveTariff = new Tariff
        {
            CountryName = "Germany",
            DistributionFee = 30m,
            TransmissionFee = 8m,
            TaxRate = 0.19m,
            Currency = "EUR",
            IsActive = false
        };
        await Testing.AddAsync(inactiveTariff);

        var currentAssignment = new FacilityTariff
        {
            FacilityId = facility.Id,
            TariffId = activeTariff.Id,
            IsActive = true
        };
        await Testing.AddAsync(currentAssignment);

        var command = new ChangeFacilityTariffCommand
        {
            CurrentAssignmentId = currentAssignment.Id,
            NewTariffId = inactiveTariff.Id
        };

        // Act & Assert
        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not active*");
    }

    [Test]
    public async Task ShouldRejectChangeToSameTariff()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Same Tariff Tenant", Identifier = "same-tariff-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Same Tariff Firm",
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
            Name = "Same Tariff Facility"
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

        var currentAssignment = new FacilityTariff
        {
            FacilityId = facility.Id,
            TariffId = tariff.Id,
            IsActive = true
        };
        await Testing.AddAsync(currentAssignment);

        var command = new ChangeFacilityTariffCommand
        {
            CurrentAssignmentId = currentAssignment.Id,
            NewTariffId = tariff.Id // Same tariff
        };

        // Act & Assert
        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*same as the current tariff*");
    }
}
