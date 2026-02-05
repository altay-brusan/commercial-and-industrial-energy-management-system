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

        var tenant = new Tenant { Name = "Demo No MO Energy Holding", Identifier = "demo-no-mo-energy-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo No MO Solar BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Noord-Holland",
            City = "Amsterdam",
            Address = "Herengracht 100",
            ContactEmail = "info@demo-no-mo-solar.nl",
            ContactPhone = "+31201234567",
            TaxNumber = "NL123456789B01",
            TaxOffice = "Amsterdam"
        };
        await Testing.AddAsync(firm);

        var facility = new Facility
        {
            FirmId = firm.Id,
            Name = "Demo Solar Park Almere"
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

        var tenant = new Tenant { Name = "Demo No Prices Energy Holding", Identifier = "demo-no-prices-energy-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo No Prices Wind BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Noord-Holland",
            City = "Haarlem",
            Address = "Grote Markt 50",
            ContactEmail = "info@demo-no-prices-wind.nl",
            ContactPhone = "+31235678901",
            TaxNumber = "NL987654321B01",
            TaxOffice = "Haarlem"
        };
        await Testing.AddAsync(firm);

        var facility = new Facility
        {
            FirmId = firm.Id,
            Name = "Demo Wind Park Wieringermeer"
        };
        await Testing.AddAsync(facility);

        var uniqueMoCode = $"ENT_{Guid.NewGuid():N}";
        var marketOperator = new MarketOperator
        {
            Name = "Demo ENTSO-E NL",
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

        var tenant = new Tenant { Name = "Demo Prices Energy Holding", Identifier = "demo-prices-energy-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Prices Solar BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Zuid-Holland",
            City = "Rotterdam",
            Address = "Coolsingel 200",
            ContactEmail = "info@demo-prices-solar.nl",
            ContactPhone = "+31101234567",
            TaxNumber = "NL555555555B01",
            TaxOffice = "Rotterdam"
        };
        await Testing.AddAsync(firm);

        var facility = new Facility
        {
            FirmId = firm.Id,
            Name = "Demo Solar Farm Delfland"
        };
        await Testing.AddAsync(facility);

        var uniqueMoCode = $"ENT_{Guid.NewGuid():N}";
        var marketOperator = new MarketOperator
        {
            Name = "Demo ENTSO-E NL Prices",
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
        result.FacilityName.Should().Be("Demo Solar Farm Delfland");
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

        var tenant = new Tenant { Name = "Demo Date Range Energy Holding", Identifier = "demo-date-range-energy-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Date Range Wind BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Flevoland",
            City = "Lelystad",
            Address = "Stationsplein 10",
            ContactEmail = "info@demo-date-range-wind.nl",
            ContactPhone = "+31320123456",
            TaxNumber = "NL666666666B01",
            TaxOffice = "Lelystad"
        };
        await Testing.AddAsync(firm);

        var facility = new Facility
        {
            FirmId = firm.Id,
            Name = "Demo Wind Park Flevoland"
        };
        await Testing.AddAsync(facility);

        var uniqueMoCode = $"ENTNL_{Guid.NewGuid():N}";
        var marketOperator = new MarketOperator
        {
            Name = "Demo ENTSO-E NL Range",
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

        // Add price data for 3 days
        var baseDate = DateTimeOffset.UtcNow.Date.AddDays(-3);
        for (int day = 0; day < 3; day++)
        {
            for (int hour = 0; hour < 24; hour++)
            {
                var priceHistory = new DayAheadEnergyPriceHistory
                {
                    MarketOperatorId = marketOperator.Id,
                    Country = "Netherlands",
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

        var tenant = new Tenant { Name = "Demo Discount Energy Holding", Identifier = "demo-discount-energy-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Discount Solar BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Utrecht",
            City = "Utrecht",
            Address = "Vredenburg 40",
            ContactEmail = "info@demo-discount-solar.nl",
            ContactPhone = "+31301234567",
            TaxNumber = "NL777777777B01",
            TaxOffice = "Utrecht"
        };
        await Testing.AddAsync(firm);

        var facility = new Facility
        {
            FirmId = firm.Id,
            Name = "Demo Solar Park Utrecht"
        };
        await Testing.AddAsync(facility);

        var uniqueMoCode = $"ENT_{Guid.NewGuid():N}";
        var marketOperator = new MarketOperator
        {
            Name = "Demo ENTSO-E NL Discount",
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
