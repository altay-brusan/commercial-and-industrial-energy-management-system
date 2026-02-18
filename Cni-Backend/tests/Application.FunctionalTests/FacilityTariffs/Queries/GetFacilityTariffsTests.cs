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

        var tenant = new Tenant { Name = "Demo List FT Energy Holding", Identifier = "demo-list-ft-energy-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo List FT Solar BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Noord-Holland",
            City = "Amsterdam",
            Address = "Rokin 150",
            ContactEmail = "info@demo-list-ft-solar.nl",
            ContactPhone = "+31201234567",
            TaxNumber = "NL123456789B01",
            TaxOffice = "Amsterdam"
        };
        await Testing.AddAsync(firm);

        var facility = new Facility
        {
            FirmId = firm.Id,
            Name = "Demo Solar Park Purmerend"
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
            DistributorName = "Liander",
            DistributionFee = 25m,
            TransmissionFee = 7m,
            TaxRate = 0.21m,
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

        var tenant = new Tenant { Name = "Demo Filter Active FT Energy Holding", Identifier = "demo-filter-active-ft-energy-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Filter Active FT Wind BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Zuid-Holland",
            City = "Leiden",
            Address = "Breestraat 100",
            ContactEmail = "info@demo-filter-active-ft-wind.nl",
            ContactPhone = "+31711234567",
            TaxNumber = "NL987654321B01",
            TaxOffice = "Leiden"
        };
        await Testing.AddAsync(firm);

        var facility = new Facility
        {
            FirmId = firm.Id,
            Name = "Demo Wind Farm Katwijk"
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

        var tenant = new Tenant { Name = "Demo No Tariffs FT Energy Holding", Identifier = "demo-no-tariffs-ft-energy-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo No Tariffs FT Solar BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Gelderland",
            City = "Apeldoorn",
            Address = "Hoofdstraat 50",
            ContactEmail = "info@demo-no-tariffs-ft-solar.nl",
            ContactPhone = "+31551234567",
            TaxNumber = "NL555555555B01",
            TaxOffice = "Apeldoorn"
        };
        await Testing.AddAsync(firm);

        var facility = new Facility
        {
            FirmId = firm.Id,
            Name = "Demo Solar Farm Eerbeek"
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
        var result = await Testing.SendAsync(new GetFacilityTariffsQuery { FacilityId = Guid.NewGuid() });

        // Assert
        result.Items.Should().BeEmpty();
    }

    [Test]
    public async Task ShouldIncludeContractDetailsInResults()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Demo Contract List FT Energy Holding", Identifier = "demo-contract-list-ft-energy-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Contract List FT Wind BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Overijssel",
            City = "Enschede",
            Address = "Oude Markt 20",
            ContactEmail = "info@demo-contract-list-ft-wind.nl",
            ContactPhone = "+31531234567",
            TaxNumber = "NL666666666B01",
            TaxOffice = "Enschede"
        };
        await Testing.AddAsync(firm);

        var facility = new Facility
        {
            FirmId = firm.Id,
            Name = "Demo Wind Park Twente"
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

        var tenant = new Tenant { Name = "Demo Paginate FT Energy Holding", Identifier = "demo-paginate-ft-energy-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Paginate FT Solar BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Friesland",
            City = "Leeuwarden",
            Address = "Zaailand 100",
            ContactEmail = "info@demo-paginate-ft-solar.nl",
            ContactPhone = "+31581234567",
            TaxNumber = "NL777777777B01",
            TaxOffice = "Leeuwarden"
        };
        await Testing.AddAsync(firm);

        var facility = new Facility
        {
            FirmId = firm.Id,
            Name = "Demo Solar Farm Harlingen"
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

        var tenant = new Tenant { Name = "Demo Filter TariffId Energy Holding", Identifier = "demo-filter-tariffid-energy-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Filter TariffId Wind BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Groningen",
            City = "Groningen",
            Address = "Grote Markt 30",
            ContactEmail = "info@demo-filter-tariffid-wind.nl",
            ContactPhone = "+31501234567",
            TaxNumber = "NL888888888B01",
            TaxOffice = "Groningen"
        };
        await Testing.AddAsync(firm);

        var facility1 = new Facility { FirmId = firm.Id, Name = "Demo Wind Farm Eemshaven" };
        var facility2 = new Facility { FirmId = firm.Id, Name = "Demo Wind Farm Delfzijl" };
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
            CountryName = "Netherlands",
            DistributorName = "Enexis",
            DistributionFee = 25m,
            TransmissionFee = 7m,
            TaxRate = 0.21m,
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
