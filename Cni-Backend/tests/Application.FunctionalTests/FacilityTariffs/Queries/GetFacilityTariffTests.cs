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

        var tenant = new Tenant { Name = "Get FT Tenant", Identifier = "get-ft-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Get FT Firm",
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
            Name = "Get FT Facility"
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

        var tenant = new Tenant { Name = "Contract Details Tenant", Identifier = "contract-details-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Contract Details Firm",
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
            Name = "Contract Details Facility"
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

        var tenant = new Tenant { Name = "Inactive FT Query Tenant", Identifier = "inactive-ft-query-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Inactive FT Query Firm",
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
            Name = "Inactive FT Query Facility"
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
