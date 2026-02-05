using Backend.Application.FacilityTariffs.Queries.GetFacilityTariff;
using Backend.Domain.Entities;

namespace Backend.Application.FunctionalTests.FacilityTariffs.Queries;

public class GetFacilityTariffTests : BaseTestFixture
{
    [Test]
    public async Task ShouldReturnFacilityTariff()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Demo Get FT Energy Holding", Identifier = "demo-get-ft-energy-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Get FT Solar BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Noord-Holland",
            City = "Amsterdam",
            Address = "Singel 200",
            ContactEmail = "info@demo-get-ft-solar.nl",
            ContactPhone = "+31201234567",
            TaxNumber = "NL123456789B01",
            TaxOffice = "Amsterdam"
        };
        await Testing.AddAsync(firm);

        var facility = new Facility
        {
            FirmId = firm.Id,
            Name = "Demo Solar Park Diemen"
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

        // Act
        var result = await Testing.SendAsync(new GetFacilityTariffQuery { Id = facilityTariff.Id });

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(facilityTariff.Id);
        result.FacilityId.Should().Be(facility.Id);
        result.TariffId.Should().Be(tariff.Id);
        result.IsActive.Should().BeTrue();
    }

    [Test]
    public async Task ShouldReturnNullForNonExistentFacilityTariff()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        // Act
        var result = await Testing.SendAsync(new GetFacilityTariffQuery { Id = 99999 });

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task ShouldReturnFacilityTariffWithContractDetails()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Demo Contract Query Energy Holding", Identifier = "demo-contract-query-energy-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Contract Query Wind BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Zuid-Holland",
            City = "Rotterdam",
            Address = "Blaak 100",
            ContactEmail = "info@demo-contract-query-wind.nl",
            ContactPhone = "+31101234567",
            TaxNumber = "NL987654321B01",
            TaxOffice = "Rotterdam"
        };
        await Testing.AddAsync(firm);

        var facility = new Facility
        {
            FirmId = firm.Id,
            Name = "Demo Wind Farm Hoek van Holland"
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

        var facilityTariff = new FacilityTariff
        {
            FacilityId = facility.Id,
            TariffId = tariff.Id,
            IsActive = true,
            ContractNumber = "CONTRACT-QUERY-001",
            DiscountRate = 0.12m,
            ContractStartDate = startDate,
            ContractEndDate = endDate,
            Notes = "Query test notes"
        };
        await Testing.AddAsync(facilityTariff);

        // Act
        var result = await Testing.SendAsync(new GetFacilityTariffQuery { Id = facilityTariff.Id });

        // Assert
        result.Should().NotBeNull();
        result!.ContractNumber.Should().Be("CONTRACT-QUERY-001");
        result.DiscountRate.Should().Be(0.12m);
        result.Notes.Should().Be("Query test notes");
    }

    [Test]
    public async Task ShouldReturnInactiveFacilityTariff()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Demo Inactive FT Query Energy Holding", Identifier = "demo-inactive-ft-query-energy-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Inactive FT Query Solar BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Noord-Brabant",
            City = "Tilburg",
            Address = "Stadhuisplein 10",
            ContactEmail = "info@demo-inactive-ft-query-solar.nl",
            ContactPhone = "+31131234567",
            TaxNumber = "NL444444444B01",
            TaxOffice = "Tilburg"
        };
        await Testing.AddAsync(firm);

        var facility = new Facility
        {
            FirmId = firm.Id,
            Name = "Demo Solar Farm Waalwijk"
        };
        await Testing.AddAsync(facility);

        var tariff = new Tariff
        {
            CountryName = "Netherlands",
            DistributionFee = 25m,
            TransmissionFee = 7m,
            TaxRate = 0.21m,
            Currency = "EUR",
            IsActive = true
        };
        await Testing.AddAsync(tariff);

        var facilityTariff = new FacilityTariff
        {
            FacilityId = facility.Id,
            TariffId = tariff.Id,
            IsActive = false
        };
        await Testing.AddAsync(facilityTariff);

        // Act
        var result = await Testing.SendAsync(new GetFacilityTariffQuery { Id = facilityTariff.Id });

        // Assert
        result.Should().NotBeNull();
        result!.IsActive.Should().BeFalse();
    }
}
