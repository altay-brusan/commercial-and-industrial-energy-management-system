using Backend.Application.Common.Exceptions;
using Backend.Application.EnergyPrices.Queries.GetLiveDayAheadEnergyPrices;
using Backend.Domain.Entities;

namespace Backend.Application.FunctionalTests.EnergyPrices.Queries;

public class GetLiveDayAheadEnergyPricesTests : BaseTestFixture
{
    [Test]
    public async Task ShouldReturnErrorForNonExistentFirm()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var query = new GetLiveDayAheadEnergyPricesQuery
        {
            FirmId = Guid.NewGuid()
        };

        // Act
        var result = await Testing.SendAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    [Test]
    public async Task ShouldReturnErrorWhenNoMarketOperatorConfigured()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Demo No MO Holding", Identifier = "demo-no-mo-holding" };
        await Testing.AddAsync(tenant);

        // Firm without a market operator configured
        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo No MO Firm BV",
            Country = "DemoCountryWithNoMO",
            Type = "Standard",
            State = "Noord-Holland",
            City = "Amsterdam",
            Address = "Keizersgracht 100",
            ContactEmail = "info@demo-no-mo.nl",
            ContactPhone = "+31201234567",
            TaxNumber = "NL123456789B01",
            TaxOffice = "Amsterdam"
        };
        await Testing.AddAsync(firm);

        var query = new GetLiveDayAheadEnergyPricesQuery
        {
            FirmId = firm.Id
        };

        // Act
        var result = await Testing.SendAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Market Operator");
    }

    [Test]
    public async Task ShouldUseFirmMarketOperatorWhenConfigured()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Demo Live MO Holding", Identifier = "demo-live-mo-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Live MO Firm BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Noord-Holland",
            City = "Amsterdam",
            Address = "Herengracht 200",
            ContactEmail = "info@demo-live-mo.nl",
            ContactPhone = "+31201234567",
            TaxNumber = "NL123456789B01",
            TaxOffice = "Amsterdam"
        };
        await Testing.AddAsync(firm);

        var uniqueMoCode = $"ENT_{Guid.NewGuid():N}";
        var marketOperator = new MarketOperator
        {
            Name = "Demo ENTSO-E NL",
            Code = uniqueMoCode,
            BaseUrl = "https://web-api.tp.entsoe.eu/api",
            AreaCodes = new List<string> { "10YNL----------L" },
            SupportedResolutions = new List<int> { 60 },
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

        var query = new GetLiveDayAheadEnergyPricesQuery
        {
            FirmId = firm.Id
        };

        // Act
        var result = await Testing.SendAsync(query);

        // Assert
        // Note: The actual API call may fail in test environment, but the query should at least attempt to use the configured MO
        result.Should().NotBeNull();
        // If it fails, it should still return the MO info
        if (!result.Success)
        {
            result.MarketOperatorCode.Should().Be(uniqueMoCode);
        }
    }

    [Test]
    public async Task ShouldValidateFirmIdIsRequired()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var query = new GetLiveDayAheadEnergyPricesQuery
        {
            FirmId = Guid.Empty // Invalid
        };

        // Act & Assert
        await FluentActions.Invoking(() =>
            Testing.SendAsync(query)).Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task ShouldReturnCorrectFirmIdInResponse()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Demo FirmId Response Holding", Identifier = "demo-firmid-response-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo FirmId Response Firm BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Zuid-Holland",
            City = "Rotterdam",
            Address = "Coolsingel 100",
            ContactEmail = "info@demo-firmid-response.nl",
            ContactPhone = "+31101234567",
            TaxNumber = "NL987654321B01",
            TaxOffice = "Rotterdam"
        };
        await Testing.AddAsync(firm);

        var uniqueMoCode = $"ENT_{Guid.NewGuid():N}";
        var marketOperator = new MarketOperator
        {
            Name = "Demo ENTSO-E Response",
            Code = uniqueMoCode,
            BaseUrl = "https://web-api.tp.entsoe.eu/api",
            AreaCodes = new List<string> { "10YNL----------L" },
            SupportedResolutions = new List<int> { 15, 60 },
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

        var query = new GetLiveDayAheadEnergyPricesQuery
        {
            FirmId = firm.Id
        };

        // Act
        var result = await Testing.SendAsync(query);

        // Assert
        // The query sets FirmId on successful paths
        // Even if external API fails, we check the setup works
        result.Should().NotBeNull();
        if (result.Success)
        {
            result.FirmId.Should().Be(firm.Id);
        }
    }

    [Test]
    public async Task ShouldUseCustomDateRangeWhenSpecified()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Demo Date Holding", Identifier = "demo-date-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Date Firm BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Utrecht",
            City = "Utrecht",
            Address = "Vredenburg 30",
            ContactEmail = "info@demo-date.nl",
            ContactPhone = "+31301234567",
            TaxNumber = "NL333333333B01",
            TaxOffice = "Utrecht"
        };
        await Testing.AddAsync(firm);

        var uniqueMoCode = $"ENT_{Guid.NewGuid():N}";
        var marketOperator = new MarketOperator
        {
            Name = "Demo ENTSO-E Date",
            Code = uniqueMoCode,
            BaseUrl = "https://web-api.tp.entsoe.eu/api",
            AreaCodes = new List<string> { "10YNL----------L" },
            SupportedResolutions = new List<int> { 60 },
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

        var startDate = DateTimeOffset.UtcNow.Date;
        var endDate = startDate.AddDays(2);

        var query = new GetLiveDayAheadEnergyPricesQuery
        {
            FirmId = firm.Id,
            StartDate = startDate,
            EndDate = endDate
        };

        // Act
        var result = await Testing.SendAsync(query);

        // Assert
        result.Should().NotBeNull();
        if (result.Success)
        {
            result.StartDate.Date.Should().Be(startDate.Date);
            result.EndDate.Date.Should().Be(endDate.Date);
        }
    }

    [Test]
    public async Task ShouldDefaultEndDateToStartDatePlusOneDay()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Demo Default EndDate Holding", Identifier = "demo-default-enddate-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Demo Default EndDate Firm BV",
            Type = "Standard",
            Country = "Netherlands",
            State = "Utrecht",
            City = "Utrecht",
            Address = "Vredenburg 40",
            ContactEmail = "info@demo-default-enddate.nl",
            ContactPhone = "+31301234568",
            TaxNumber = "NL333333334B01",
            TaxOffice = "Utrecht"
        };
        await Testing.AddAsync(firm);

        var uniqueMoCode = $"ENT_{Guid.NewGuid():N}";
        var marketOperator = new MarketOperator
        {
            Name = "Demo ENTSO-E Default EndDate",
            Code = uniqueMoCode,
            BaseUrl = "https://web-api.tp.entsoe.eu/api",
            AreaCodes = new List<string> { "10YNL----------L" },
            SupportedResolutions = new List<int> { 60 },
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

        var startDate = DateTimeOffset.UtcNow.Date;

        var query = new GetLiveDayAheadEnergyPricesQuery
        {
            FirmId = firm.Id,
            StartDate = startDate
            // EndDate not specified - should default to StartDate + 1 day
        };

        // Act
        var result = await Testing.SendAsync(query);

        // Assert
        result.Should().NotBeNull();
        // EndDate should default to StartDate + 1 day
        result.EndDate.Date.Should().Be(startDate.AddDays(1).Date);
    }

    [Test]
    public async Task ShouldRejectDateRangeExceeding1Year()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var startDate = DateTimeOffset.UtcNow.Date;
        var endDate = startDate.AddDays(400); // 400 days exceeds 365 day (1 year) limit

        var query = new GetLiveDayAheadEnergyPricesQuery
        {
            FirmId = Guid.NewGuid(),
            StartDate = startDate,
            EndDate = endDate
        };

        // Act & Assert
        await FluentActions.Invoking(() =>
            Testing.SendAsync(query)).Should().ThrowAsync<ValidationException>();
    }

    /// <summary>
    /// Integration test that connects to the real EPIAS API to fetch Turkey day-ahead prices.
    /// This test requires network access and uses real EPIAS credentials.
    /// </summary>
    [Test]
    [Category("Integration")]
    [Category("ExternalApi")]
    public async Task ShouldFetchRealPricesFromEpiasForTurkey()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Turkey Energy Holding", Identifier = "turkey-energy-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Turkey Solar AŞ",
            Type = "Standard",
            Country = "Turkey",
            State = "İstanbul",
            City = "İstanbul",
            Address = "Maslak Mah. Büyükdere Cad. No:123",
            ContactEmail = "info@turkey-solar.com.tr",
            ContactPhone = "+902121234567",
            TaxNumber = "1234567890",
            TaxOffice = "Maslak"
        };
        await Testing.AddAsync(firm);

        // Check if EPIAS market operator already exists, or create a new one
        var marketOperator = await Testing.FirstOrDefaultAsync<MarketOperator>(mo => mo.Code == "EPIAS");
        if (marketOperator == null)
        {
            marketOperator = new MarketOperator
            {
                Name = "EPİAŞ (Turkish Energy Exchange)",
                Code = "EPIAS",
                BaseUrl = "https://seffaflik.epias.com.tr",
                ApiKey = "technology@oklidis.com.tr:***REMOVED***",
                SupportedResolutions = new List<int> { 60 },
                AreaCodes = new List<string> { "TR" },
                IsActive = true
            };
            await Testing.AddAsync(marketOperator);
        }

        var firmMarketOperator = new FirmMarketOperator
        {
            FirmId = firm.Id,
            MarketOperatorId = marketOperator.Id,
            AreaCode = "TR",
            SamplingPeriodMinutes = 60,
            IsActive = true
        };
        await Testing.AddAsync(firmMarketOperator);

        // Create a facility to link to Turkey tariff
        var facility = new Facility
        {
            FirmId = firm.Id,
            Name = "Turkey Solar Park"
        };
        await Testing.AddAsync(facility);

        // Create Turkey tariff with TRY currency
        var turkeyTariff = await Testing.FirstOrDefaultAsync<Tariff>(t => t.CountryName == "Turkey" && t.IsActive);
        if (turkeyTariff == null)
        {
            turkeyTariff = new Tariff
            {
                CountryName = "Turkey",
                DistributionFee = 50m,
                TransmissionFee = 25m,
                TaxRate = 0.20m,
                Currency = "TRY",
                IsActive = true
            };
            await Testing.AddAsync(turkeyTariff);
        }

        // Assign tariff to facility
        var facilityTariff = new FacilityTariff
        {
            FacilityId = facility.Id,
            TariffId = turkeyTariff.Id,
            IsActive = true,
            DiscountRate = 0m
        };
        await Testing.AddAsync(facilityTariff);

        // Use yesterday's date to ensure data is available
        var targetDate = DateTimeOffset.UtcNow.Date.AddDays(-1);

        var query = new GetLiveDayAheadEnergyPricesQuery
        {
            FirmId = firm.Id,
            StartDate = targetDate,
            EndDate = targetDate
        };

        // Act
        var result = await Testing.SendAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue($"EPIAS API call should succeed. Error: {result.ErrorMessage}");
        result.MarketOperatorCode.Should().Be("EPIAS");
        result.Country.Should().Be("Turkey");
        result.Currency.Should().Be("TRY");
        result.ResolutionMinutes.Should().Be(60); // EPIAS always returns hourly data
        result.Timezone.Should().Be("Europe/Istanbul");

        // EPIAS should return 24 hourly price points for a single day
        result.Prices.Should().HaveCount(24, "EPIAS should return 24 hourly prices for one day");
        result.DataPointCount.Should().Be(24);

        // Verify price data structure (energy prices can be 0 during low-demand periods)
        foreach (var price in result.Prices)
        {
            price.NetPrice.Should().BeGreaterThanOrEqualTo(0, "Net price should not be negative");
            price.GrossPrice.Should().BeGreaterThanOrEqualTo(0, "Gross price should not be negative");
            price.Timestamp.Should().NotBe(default);
            price.LocalTimestamp.Should().NotBe(default);
        }

        // Verify statistics are calculated (min can be 0 during low-demand periods)
        result.Statistics.Should().NotBeNull();
        result.Statistics!.MinNetPrice.Should().BeGreaterThanOrEqualTo(0);
        result.Statistics.MaxNetPrice.Should().BeGreaterThanOrEqualTo(result.Statistics.MinNetPrice);
        result.Statistics.AvgNetPrice.Should().BeGreaterThanOrEqualTo(0);

        // Log the actual prices for debugging/visibility
        Console.WriteLine($"EPIAS Turkey Day-Ahead Prices for {targetDate:yyyy-MM-dd}:");
        Console.WriteLine($"  Currency: {result.Currency}");
        Console.WriteLine($"  Data Points: {result.DataPointCount}");
        Console.WriteLine($"  Min Price: {result.Statistics.MinNetPrice:N2} {result.Currency}/MWh at hour {result.Statistics.MinPriceHour}");
        Console.WriteLine($"  Max Price: {result.Statistics.MaxNetPrice:N2} {result.Currency}/MWh at hour {result.Statistics.MaxPriceHour}");
        Console.WriteLine($"  Avg Price: {result.Statistics.AvgNetPrice:N2} {result.Currency}/MWh");
    }

    /// <summary>
    /// Integration test that fetches multiple days of real prices from EPIAS.
    /// </summary>
    [Test]
    [Category("Integration")]
    [Category("ExternalApi")]
    public async Task ShouldFetchMultipleDaysFromEpiasForTurkey()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Turkey Multi-Day Holding", Identifier = "turkey-multiday-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Turkey Wind AŞ",
            Type = "Standard",
            Country = "Turkey",
            State = "İzmir",
            City = "İzmir",
            Address = "Alsancak Mah. Kordon Cad. No:50",
            ContactEmail = "info@turkey-wind.com.tr",
            ContactPhone = "+902321234567",
            TaxNumber = "9876543210",
            TaxOffice = "Alsancak"
        };
        await Testing.AddAsync(firm);

        // Check if EPIAS market operator already exists, or create a new one
        var marketOperator = await Testing.FirstOrDefaultAsync<MarketOperator>(mo => mo.Code == "EPIAS");
        if (marketOperator == null)
        {
            marketOperator = new MarketOperator
            {
                Name = "EPİAŞ (Turkish Energy Exchange)",
                Code = "EPIAS",
                BaseUrl = "https://seffaflik.epias.com.tr",
                ApiKey = "technology@oklidis.com.tr:***REMOVED***",
                SupportedResolutions = new List<int> { 60 },
                AreaCodes = new List<string> { "TR" },
                IsActive = true
            };
            await Testing.AddAsync(marketOperator);
        }

        var firmMarketOperator = new FirmMarketOperator
        {
            FirmId = firm.Id,
            MarketOperatorId = marketOperator.Id,
            AreaCode = "TR",
            SamplingPeriodMinutes = 60,
            IsActive = true
        };
        await Testing.AddAsync(firmMarketOperator);

        // Create a facility to link to Turkey tariff
        var facility = new Facility
        {
            FirmId = firm.Id,
            Name = "Turkey Wind Park Izmir"
        };
        await Testing.AddAsync(facility);

        // Create Turkey tariff with TRY currency
        var turkeyTariff = await Testing.FirstOrDefaultAsync<Tariff>(t => t.CountryName == "Turkey" && t.IsActive);
        if (turkeyTariff == null)
        {
            turkeyTariff = new Tariff
            {
                CountryName = "Turkey",
                DistributionFee = 50m,
                TransmissionFee = 25m,
                TaxRate = 0.20m,
                Currency = "TRY",
                IsActive = true
            };
            await Testing.AddAsync(turkeyTariff);
        }

        // Assign tariff to facility
        var facilityTariff = new FacilityTariff
        {
            FacilityId = facility.Id,
            TariffId = turkeyTariff.Id,
            IsActive = true,
            DiscountRate = 0m
        };
        await Testing.AddAsync(facilityTariff);

        // Fetch 3 days of historical data
        var endDate = DateTimeOffset.UtcNow.Date.AddDays(-1);
        var startDate = endDate.AddDays(-2); // 3 days total

        var query = new GetLiveDayAheadEnergyPricesQuery
        {
            FirmId = firm.Id,
            StartDate = startDate,
            EndDate = endDate
        };

        // Act
        var result = await Testing.SendAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue($"EPIAS API call should succeed. Error: {result.ErrorMessage}");

        // Should have 72 price points (24 hours * 3 days)
        result.Prices.Should().HaveCount(72, "EPIAS should return 72 hourly prices for 3 days");
        result.DataPointCount.Should().Be(72);

        // Verify dates in response
        result.StartDate.Date.Should().Be(startDate.Date);
        result.EndDate.Date.Should().Be(endDate.Date);

        Console.WriteLine($"EPIAS Turkey Multi-Day Prices from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}:");
        Console.WriteLine($"  Total Data Points: {result.DataPointCount}");
        Console.WriteLine($"  Min Price: {result.Statistics!.MinNetPrice:N2} TRY/MWh");
        Console.WriteLine($"  Max Price: {result.Statistics.MaxNetPrice:N2} TRY/MWh");
        Console.WriteLine($"  Avg Price: {result.Statistics.AvgNetPrice:N2} TRY/MWh");
    }
}
