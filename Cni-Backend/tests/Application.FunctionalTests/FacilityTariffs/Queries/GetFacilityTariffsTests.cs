using Backend.Application.FacilityTariffs.Queries.GetFacilityTariffs;
using Backend.Domain.Entities;

namespace Backend.Application.FunctionalTests.FacilityTariffs.Queries;

public class GetFacilityTariffsTests : BaseTestFixture
{
    [Test]
    public async Task ShouldReturnFacilityTariffsForFacility()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "List FT Tenant", Identifier = "list-ft-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "List FT Firm",
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
            Name = "List FT Facility"
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

        var facilityTariff1 = new FacilityTariff
        {
            FacilityId = facility.Id,
            TariffId = tariff1.Id,
            IsActive = false // Historical
        };
        await Testing.AddAsync(facilityTariff1);

        var facilityTariff2 = new FacilityTariff
        {
            FacilityId = facility.Id,
            TariffId = tariff2.Id,
            IsActive = true // Current
        };
        await Testing.AddAsync(facilityTariff2);

        // Act
        var result = await Testing.SendAsync(new GetFacilityTariffsQuery { FacilityId = facility.Id });

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
    }

    [Test]
    public async Task ShouldFilterByActiveStatus()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Filter Active Tenant", Identifier = "filter-active-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Filter Active Firm",
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
            Name = "Filter Active Facility"
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

        var activeFt = new FacilityTariff
        {
            FacilityId = facility.Id,
            TariffId = tariff.Id,
            IsActive = true
        };
        await Testing.AddAsync(activeFt);

        var inactiveFt = new FacilityTariff
        {
            FacilityId = facility.Id,
            TariffId = tariff.Id,
            IsActive = false
        };
        await Testing.AddAsync(inactiveFt);

        // Act - Filter by active only
        var result = await Testing.SendAsync(new GetFacilityTariffsQuery
        {
            FacilityId = facility.Id,
            IsActive = true
        });

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().IsActive.Should().BeTrue();
    }

    [Test]
    public async Task ShouldReturnEmptyForFacilityWithNoTariffs()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "No Tariffs Tenant", Identifier = "no-tariffs-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "No Tariffs Firm",
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
            Name = "No Tariffs Facility"
        };
        await Testing.AddAsync(facility);

        // Act
        var result = await Testing.SendAsync(new GetFacilityTariffsQuery { FacilityId = facility.Id });

        // Assert
        result.Items.Should().BeEmpty();
    }

    [Test]
    public async Task ShouldReturnEmptyForNonExistentFacility()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        // Act
        var result = await Testing.SendAsync(new GetFacilityTariffsQuery { FacilityId = 99999 });

        // Assert
        result.Items.Should().BeEmpty();
    }

    [Test]
    public async Task ShouldIncludeContractDetailsInResults()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Contract List Tenant", Identifier = "contract-list-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Contract List Firm",
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
            Name = "Contract List Facility"
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
            ContractNumber = "CONTRACT-LIST-001",
            DiscountRate = 0.15m,
            Notes = "List test notes"
        };
        await Testing.AddAsync(facilityTariff);

        // Act
        var result = await Testing.SendAsync(new GetFacilityTariffsQuery { FacilityId = facility.Id });

        // Assert
        result.Items.Should().HaveCount(1);
        var first = result.Items.First();
        first.ContractNumber.Should().Be("CONTRACT-LIST-001");
        first.DiscountRate.Should().Be(0.15m);
    }

    [Test]
    public async Task ShouldReturnPaginatedResults()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Paginate FT Tenant", Identifier = "paginate-ft-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Paginate FT Firm",
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
            Name = "Paginate FT Facility"
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

        // Add 15 facility tariffs
        for (int i = 0; i < 15; i++)
        {
            var ft = new FacilityTariff
            {
                FacilityId = facility.Id,
                TariffId = tariff.Id,
                IsActive = i % 2 == 0
            };
            await Testing.AddAsync(ft);
        }

        // Act - Page 1, size 10
        var result = await Testing.SendAsync(new GetFacilityTariffsQuery
        {
            FacilityId = facility.Id,
            PageNumber = 1,
            PageSize = 10
        });

        // Assert
        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(15);
        result.TotalPages.Should().Be(2);
    }

    [Test]
    public async Task ShouldFilterByTariffId()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Filter TariffId Tenant", Identifier = "filter-tariffid-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Filter TariffId Firm",
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

        var facility1 = new Facility { FirmId = firm.Id, Name = "Facility 1" };
        var facility2 = new Facility { FirmId = firm.Id, Name = "Facility 2" };
        await Testing.AddAsync(facility1);
        await Testing.AddAsync(facility2);

        var tariff1 = new Tariff
        {
            CountryName = "Netherlands",
            DistributionFee = 20m,
            TransmissionFee = 5m,
            TaxRate = 0.21m,
            Currency = "EUR",
            IsActive = true
        };
        var tariff2 = new Tariff
        {
            CountryName = "Germany",
            DistributionFee = 30m,
            TransmissionFee = 8m,
            TaxRate = 0.19m,
            Currency = "EUR",
            IsActive = true
        };
        await Testing.AddAsync(tariff1);
        await Testing.AddAsync(tariff2);

        var ft1 = new FacilityTariff { FacilityId = facility1.Id, TariffId = tariff1.Id, IsActive = true };
        var ft2 = new FacilityTariff { FacilityId = facility2.Id, TariffId = tariff2.Id, IsActive = true };
        await Testing.AddAsync(ft1);
        await Testing.AddAsync(ft2);

        // Act - Filter by tariff1
        var result = await Testing.SendAsync(new GetFacilityTariffsQuery { TariffId = tariff1.Id });

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().TariffId.Should().Be(tariff1.Id);
    }
}
