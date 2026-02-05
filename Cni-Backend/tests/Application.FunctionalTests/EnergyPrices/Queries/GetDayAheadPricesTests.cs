using Backend.Application.EnergyPrices.Queries.GetDayAheadPrices;
using Backend.Domain.Entities;

namespace Backend.Application.FunctionalTests.EnergyPrices.Queries;

public class GetDayAheadPricesTests : BaseTestFixture
{
    [Test]
    public async Task ShouldReturnNullForNonExistentFacility()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var query = new GetDayAheadPricesQuery
        {
            FacilityId = 99999,
            StartDate = DateTimeOffset.UtcNow.Date
        };

        // Act
        var result = await Testing.SendAsync(query);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task ShouldReturnNullForFacilityWithoutMarketOperator()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "No MO Tenant", Identifier = "no-mo-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "No MO Firm",
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
            Name = "No MO Facility"
        };
        await Testing.AddAsync(facility);

        var query = new GetDayAheadPricesQuery
        {
            FacilityId = facility.Id,
            StartDate = DateTimeOffset.UtcNow.Date
        };

        // Act
        var result = await Testing.SendAsync(query);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task ShouldReturnNullWhenNoPricesFound()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "No Prices Tenant", Identifier = "no-prices-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "No Prices Firm",
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
            Name = "No Prices Facility"
        };
        await Testing.AddAsync(facility);

        var uniqueMoCode = $"ENT_{Guid.NewGuid():N}";
        var marketOperator = new MarketOperator
        {
            Name = "ENTSO-E",
            Code = uniqueMoCode,
            BaseUrl = "https://web-api.tp.entsoe.eu/api",
            SupportedResolutions = new List<int> { 60 },
            AreaCodes = new List<string> { "10YNL----------L" },
            IsActive = true
        };
        await Testing.AddAsync(marketOperator);

        var firmMarketOperator = new FirmMarketOperator
        {
            FirmId = firm.Id,
            MarketOperatorId = marketOperator.Id,
            AreaCode = "10YNL----------L",
            SamplingPeriodMinutes = 60,
            IsActive = true
        };
        await Testing.AddAsync(firmMarketOperator);

        var query = new GetDayAheadPricesQuery
        {
            FacilityId = facility.Id,
            StartDate = DateTimeOffset.UtcNow.Date.AddYears(-10) // Far in the past, no data
        };

        // Act
        var result = await Testing.SendAsync(query);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task ShouldReturnPricesForValidFacilityWithData()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Prices Tenant", Identifier = "prices-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Prices Firm",
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
            Name = "Prices Facility"
        };
        await Testing.AddAsync(facility);

        var uniqueMoCode = $"ENT_{Guid.NewGuid():N}";
        var marketOperator = new MarketOperator
        {
            Name = "ENTSO-E",
            Code = uniqueMoCode,
            BaseUrl = "https://web-api.tp.entsoe.eu/api",
            SupportedResolutions = new List<int> { 60 },
            AreaCodes = new List<string> { "10YNL----------L" },
            IsActive = true
        };
        await Testing.AddAsync(marketOperator);

        var firmMarketOperator = new FirmMarketOperator
        {
            FirmId = firm.Id,
            MarketOperatorId = marketOperator.Id,
            AreaCode = "10YNL----------L",
            SamplingPeriodMinutes = 60,
            IsActive = true
        };
        await Testing.AddAsync(firmMarketOperator);

        // Add some price data
        var baseDate = DateTimeOffset.UtcNow.Date;
        for (int hour = 0; hour < 24; hour++)
        {
            var priceHistory = new DayAheadEnergyPriceHistory
            {
                MarketOperatorId = marketOperator.Id,
                Country = "Netherlands",
                NetPrice = 50m + hour,
                GrossPrice = 60m + hour,
                PriceLocalTimestamp = baseDate.AddHours(hour)
            };
            await Testing.AddAsync(priceHistory);
        }

        var query = new GetDayAheadPricesQuery
        {
            FacilityId = facility.Id,
            StartDate = baseDate
        };

        // Act
        var result = await Testing.SendAsync(query);

        // Assert
        result.Should().NotBeNull();
        result!.FacilityId.Should().Be(facility.Id);
        result.FacilityName.Should().Be("Prices Facility");
        result.Country.Should().Be("Netherlands");
        result.MarketOperatorId.Should().Be(marketOperator.Id);
        result.MarketOperatorCode.Should().Be(uniqueMoCode);
        result.DataPointCount.Should().Be(24);
        result.Prices.Should().HaveCount(24);
    }

    [Test]
    public async Task ShouldReturnPricesWithDateRange()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Range Tenant", Identifier = "range-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Range Firm",
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
            Name = "Range Facility"
        };
        await Testing.AddAsync(facility);

        var uniqueMoCode = $"ENTDE_{Guid.NewGuid():N}";
        var marketOperator = new MarketOperator
        {
            Name = "ENTSO-E DE",
            Code = uniqueMoCode,
            BaseUrl = "https://web-api.tp.entsoe.eu/api",
            SupportedResolutions = new List<int> { 60 },
            AreaCodes = new List<string> { "10YDE-VE-------2" },
            IsActive = true
        };
        await Testing.AddAsync(marketOperator);

        var firmMarketOperator = new FirmMarketOperator
        {
            FirmId = firm.Id,
            MarketOperatorId = marketOperator.Id,
            AreaCode = "10YDE-VE-------2",
            SamplingPeriodMinutes = 60,
            IsActive = true
        };
        await Testing.AddAsync(firmMarketOperator);

        // Add price data for 3 days
        var baseDate = DateTimeOffset.UtcNow.Date.AddDays(-3);
        for (int day = 0; day < 3; day++)
        {
            for (int hour = 0; hour < 24; hour++)
            {
                var priceHistory = new DayAheadEnergyPriceHistory
                {
                    MarketOperatorId = marketOperator.Id,
                    Country = "Germany",
                    NetPrice = 40m + day * 10 + hour,
                    GrossPrice = 50m + day * 10 + hour,
                    PriceLocalTimestamp = baseDate.AddDays(day).AddHours(hour)
                };
                await Testing.AddAsync(priceHistory);
            }
        }

        var query = new GetDayAheadPricesQuery
        {
            FacilityId = facility.Id,
            StartDate = baseDate,
            EndDate = baseDate.AddDays(2)
        };

        // Act
        var result = await Testing.SendAsync(query);

        // Assert
        result.Should().NotBeNull();
        result!.DataPointCount.Should().Be(72); // 3 days * 24 hours
    }

    [Test]
    public async Task ShouldApplyDiscountFromFacilityTariff()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Discount Tenant", Identifier = "discount-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Discount Firm",
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
            Name = "Discount Facility"
        };
        await Testing.AddAsync(facility);

        var uniqueMoCode = $"ENT_{Guid.NewGuid():N}";
        var marketOperator = new MarketOperator
        {
            Name = "ENTSO-E",
            Code = uniqueMoCode,
            BaseUrl = "https://web-api.tp.entsoe.eu/api",
            SupportedResolutions = new List<int> { 60 },
            AreaCodes = new List<string> { "10YNL----------L" },
            IsActive = true
        };
        await Testing.AddAsync(marketOperator);

        var firmMarketOperator = new FirmMarketOperator
        {
            FirmId = firm.Id,
            MarketOperatorId = marketOperator.Id,
            AreaCode = "10YNL----------L",
            SamplingPeriodMinutes = 60,
            IsActive = true
        };
        await Testing.AddAsync(firmMarketOperator);

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
            DiscountRate = 0.10m // 10% discount
        };
        await Testing.AddAsync(facilityTariff);

        // Add price data
        var baseDate = DateTimeOffset.UtcNow.Date;
        var priceHistory = new DayAheadEnergyPriceHistory
        {
            MarketOperatorId = marketOperator.Id,
            Country = "Netherlands",
            NetPrice = 100m,
            GrossPrice = 125m,
            PriceLocalTimestamp = baseDate
        };
        await Testing.AddAsync(priceHistory);

        var query = new GetDayAheadPricesQuery
        {
            FacilityId = facility.Id,
            StartDate = baseDate
        };

        // Act
        var result = await Testing.SendAsync(query);

        // Assert
        result.Should().NotBeNull();
        result!.DiscountRate.Should().Be(0.10m);
        result.DistributionFee.Should().Be(20m);
        result.TransmissionFee.Should().Be(5m);
        result.TaxRate.Should().Be(0.21m);
        // Discounted price should be 125 * 0.90 = 112.5
        result.Prices.First().DiscountedGrossPrice.Should().Be(112.5m);
    }
}
