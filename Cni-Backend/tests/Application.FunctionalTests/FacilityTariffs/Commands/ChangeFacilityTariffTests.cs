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

        var tenant = new Tenant { Name = "Demo Change Tariff Energy Holding", Identifier = "demo-change-tariff-energy-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Change Tariff Solar BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Noord-Holland",
            City = "Amsterdam",
            Address = "Prinsengracht 200",
            ContactEmail = "info@demo-change-tariff-solar.nl",
            ContactPhone = "+31201234567",
            TaxNumber = "NL123456789B01",
            TaxOffice = "Amsterdam"
        };
        await Testing.AddAsync(firm);

        var facility = new Facility
        {
            FirmId = firm.Id,
            Name = "Demo Solar Park Amstelveen"
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

        var tenant = new Tenant { Name = "Demo Change Success Energy Holding", Identifier = "demo-change-success-energy-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Change Success Wind BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Zuid-Holland",
            City = "Rotterdam",
            Address = "Weena 150",
            ContactEmail = "info@demo-change-success-wind.nl",
            ContactPhone = "+31101234567",
            TaxNumber = "NL987654321B01",
            TaxOffice = "Rotterdam"
        };
        await Testing.AddAsync(firm);

        var facility = new Facility
        {
            FirmId = firm.Id,
            Name = "Demo Wind Farm Europoort"
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
            CountryName = "Netherlands",
            DistributorName = "Stedin",
            DistributionFee = 25m,
            TransmissionFee = 7m,
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

        var tenant = new Tenant { Name = "Demo Change Contract Energy Holding", Identifier = "demo-change-contract-energy-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Change Contract Solar BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Utrecht",
            City = "Amersfoort",
            Address = "Stationsplein 30",
            ContactEmail = "info@demo-change-contract-solar.nl",
            ContactPhone = "+31331234567",
            TaxNumber = "NL555555555B01",
            TaxOffice = "Amersfoort"
        };
        await Testing.AddAsync(firm);

        var facility = new Facility
        {
            FirmId = firm.Id,
            Name = "Demo Solar Farm Soestduinen"
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
            CountryName = "Netherlands",
            DistributorName = "Liander",
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

        var tenant = new Tenant { Name = "Demo Inactive Assignment Energy Holding", Identifier = "demo-inactive-assignment-energy-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Inactive Assignment Wind BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Gelderland",
            City = "Nijmegen",
            Address = "Grote Markt 10",
            ContactEmail = "info@demo-inactive-assignment-wind.nl",
            ContactPhone = "+31241234567",
            TaxNumber = "NL666666666B01",
            TaxOffice = "Nijmegen"
        };
        await Testing.AddAsync(firm);

        var facility = new Facility
        {
            FirmId = firm.Id,
            Name = "Demo Wind Park Ooijpolder"
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
            CountryName = "Netherlands",
            DistributorName = "Liander",
            DistributionFee = 25m,
            TransmissionFee = 7m,
            TaxRate = 0.21m,
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

        var tenant = new Tenant { Name = "Demo Inactive New Tariff Energy Holding", Identifier = "demo-inactive-new-tariff-energy-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Inactive New Tariff Solar BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Overijssel",
            City = "Zwolle",
            Address = "Melkmarkt 20",
            ContactEmail = "info@demo-inactive-new-tariff-solar.nl",
            ContactPhone = "+31381234567",
            TaxNumber = "NL777777777B01",
            TaxOffice = "Zwolle"
        };
        await Testing.AddAsync(firm);

        var facility = new Facility
        {
            FirmId = firm.Id,
            Name = "Demo Solar Farm Kampen"
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
            CountryName = "Netherlands",
            DistributionFee = 30m,
            TransmissionFee = 8m,
            TaxRate = 0.21m,
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

        var tenant = new Tenant { Name = "Demo Same Tariff Energy Holding", Identifier = "demo-same-tariff-energy-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Same Tariff Wind BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Limburg",
            City = "Maastricht",
            Address = "Vrijthof 15",
            ContactEmail = "info@demo-same-tariff-wind.nl",
            ContactPhone = "+31431234567",
            TaxNumber = "NL888888888B01",
            TaxOffice = "Maastricht"
        };
        await Testing.AddAsync(firm);

        var facility = new Facility
        {
            FirmId = firm.Id,
            Name = "Demo Wind Park Eijsden"
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
