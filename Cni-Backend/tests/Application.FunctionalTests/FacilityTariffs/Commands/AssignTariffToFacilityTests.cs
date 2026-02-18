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
            FacilityId = Guid.NewGuid(),
            TariffId = tariff.Id
        };

        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task ShouldRequireValidTariffId()
    {
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Demo Assign Tariff Energy Holding", Identifier = "demo-assign-tariff-energy-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Assign Tariff Solar BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Noord-Holland",
            City = "Amsterdam",
            Address = "Herengracht 150",
            ContactEmail = "info@demo-assign-tariff-solar.nl",
            ContactPhone = "+31201234567",
            TaxNumber = "NL123456789B01",
            TaxOffice = "Amsterdam"
        };
        await Testing.AddAsync(firm);

        var facility = new Facility
        {
            FirmId = firm.Id,
            Name = "Demo Solar Park Zaandam"
        };
        await Testing.AddAsync(facility);

        var command = new AssignTariffToFacilityCommand
        {
            FacilityId = facility.Id,
            TariffId = Guid.NewGuid()
        };

        await FluentActions.Invoking(() =>
            Testing.SendAsync(command)).Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task ShouldAssignTariffToFacility()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Demo Assign Success Energy Holding", Identifier = "demo-assign-success-energy-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Assign Success Wind BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Zuid-Holland",
            City = "Rotterdam",
            Address = "Coolsingel 100",
            ContactEmail = "info@demo-assign-success-wind.nl",
            ContactPhone = "+31101234567",
            TaxNumber = "NL987654321B01",
            TaxOffice = "Rotterdam"
        };
        await Testing.AddAsync(firm);

        var facility = new Facility
        {
            FirmId = firm.Id,
            Name = "Demo Wind Farm Maasvlakte"
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

        var tenant = new Tenant { Name = "Demo Contract Details Energy Holding", Identifier = "demo-contract-details-energy-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Contract Details Solar BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Utrecht",
            City = "Utrecht",
            Address = "Vredenburg 20",
            ContactEmail = "info@demo-contract-details-solar.nl",
            ContactPhone = "+31301234567",
            TaxNumber = "NL555555555B01",
            TaxOffice = "Utrecht"
        };
        await Testing.AddAsync(firm);

        var facility = new Facility
        {
            FirmId = firm.Id,
            Name = "Demo Solar Farm Leidsche Rijn"
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

        var tenant = new Tenant { Name = "Demo Deactivate Existing Energy Holding", Identifier = "demo-deactivate-existing-energy-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Deactivate Existing Wind BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Noord-Brabant",
            City = "Eindhoven",
            Address = "Stationsplein 25",
            ContactEmail = "info@demo-deactivate-existing-wind.nl",
            ContactPhone = "+31401234567",
            TaxNumber = "NL444444444B01",
            TaxOffice = "Eindhoven"
        };
        await Testing.AddAsync(firm);

        var facility = new Facility
        {
            FirmId = firm.Id,
            Name = "Demo Wind Park Helmond"
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
            CountryName = "Netherlands",
            DistributorName = "Enexis",
            DistributionFee = 25m,
            TransmissionFee = 6m,
            TaxRate = 0.21m,
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

        var tenant = new Tenant { Name = "Demo Inactive Tariff Energy Holding", Identifier = "demo-inactive-tariff-energy-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Inactive Tariff Solar BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Gelderland",
            City = "Arnhem",
            Address = "Koningstraat 50",
            ContactEmail = "info@demo-inactive-tariff-solar.nl",
            ContactPhone = "+31261234567",
            TaxNumber = "NL666666666B01",
            TaxOffice = "Arnhem"
        };
        await Testing.AddAsync(firm);

        var facility = new Facility
        {
            FirmId = firm.Id,
            Name = "Demo Solar Farm Veluwe"
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

        var tenant = new Tenant { Name = "Demo Zero Discount Energy Holding", Identifier = "demo-zero-discount-energy-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Zero Discount Wind BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Flevoland",
            City = "Lelystad",
            Address = "Stationsplein 5",
            ContactEmail = "info@demo-zero-discount-wind.nl",
            ContactPhone = "+31320123456",
            TaxNumber = "NL777777777B01",
            TaxOffice = "Lelystad"
        };
        await Testing.AddAsync(firm);

        var facility = new Facility
        {
            FirmId = firm.Id,
            Name = "Demo Wind Park Flevopolder"
        };
        await Testing.AddAsync(facility);

        var tariff = new Tariff
        {
            CountryName = "Netherlands",
            DistributionFee = 22m,
            TransmissionFee = 6m,
            TaxRate = 0.21m,
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
