using Backend.Application.Common.Interfaces;
using Backend.Domain.Entities;
using Backend.Infrastructure.BackgroundJobs;

namespace Backend.Application.FunctionalTests.BackgroundJobs;

public class EnergyPriceFetchWorkerTests : BaseTestFixture
{
    /// <summary>
    /// Integration test that verifies the EnergyPriceFetchWorker can fetch prices from EPIAS
    /// and store them in the DayAheadEnergyPriceHistories table.
    /// Uses the EnergyPriceService directly first to verify API connectivity,
    /// then tests the worker.
    /// </summary>
    [Test]
    [Category("Integration")]
    [Category("ExternalApi")]
    public async Task ShouldFetchAndStorePricesFromEpias()
    {
        // Arrange
        var tenant = new Tenant { Name = "Worker Test Holding", Identifier = "worker-test-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Worker Test Firm",
            Type = "Standard",
            Country = "Turkey",
            State = "İstanbul",
            City = "İstanbul",
            Address = "Test Address",
            ContactEmail = "test@worker.com",
            ContactPhone = "+901234567890",
            TaxNumber = "1234567890",
            TaxOffice = "Test"
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

        // First, verify API connectivity by calling service directly
        Console.WriteLine("Testing EPIAS API connectivity...");
        var apiResult = await Testing.ExecuteWithServiceAsync<IEnergyPriceService, EnergyPriceResult>(async service =>
        {
            var yesterday = DateTimeOffset.UtcNow.Date.AddDays(-1);
            return await service.GetDayAheadPricesAsync(
                "EPIAS",
                "https://seffaflik.epias.com.tr",
                "technology@oklidis.com.tr:***REMOVED***",
                "Turkey",
                yesterday);
        });

        Console.WriteLine($"API Result - Success: {apiResult.Success}, Prices: {apiResult.Prices.Count}, Error: {apiResult.ErrorMessage ?? "None"}");
        apiResult.Success.Should().BeTrue($"EPIAS API should be accessible. Error: {apiResult.ErrorMessage}");

        // Get initial count of price history records
        var initialCount = await Testing.CountAsync<DayAheadEnergyPriceHistory>();
        Console.WriteLine($"Initial price history count: {initialCount}");

        // Act - Execute the worker's morning fetch (fetches current day)
        Console.WriteLine("Executing EnergyPriceFetchWorker.FetchMorningPrices()...");
        await Testing.ExecuteWithServiceAsync<EnergyPriceFetchWorker>(async worker =>
        {
            await worker.FetchMorningPrices();
        });

        // Assert - Verify prices were stored
        var finalCount = await Testing.CountAsync<DayAheadEnergyPriceHistory>();
        var newRecords = finalCount - initialCount;
        Console.WriteLine($"Final price history count: {finalCount}, New records: {newRecords}");

        newRecords.Should().BeGreaterThan(0, "Worker should have fetched and stored price data");

        // Debug: Get ALL stored prices to see what countries are stored
        var allStoredPrices = await Testing.GetAllAsync<DayAheadEnergyPriceHistory>();
        var countrySummary = allStoredPrices
            .GroupBy(p => p.Country ?? "<null>")
            .Select(g => $"{g.Key}: {g.Count()}")
            .ToList();
        Console.WriteLine($"All stored prices by country: {string.Join(", ", countrySummary)}");

        // Verify the stored records for Turkey (don't filter by MO ID since it might differ)
        var storedPrices = await Testing.GetAllAsync<DayAheadEnergyPriceHistory>(
            p => p.Country == "Turkey");

        Console.WriteLine($"Stored prices for Turkey: {storedPrices.Count}");

        storedPrices.Should().NotBeEmpty("Worker should have stored prices for Turkey");
        storedPrices.Should().AllSatisfy(price =>
        {
            price.NetPrice.Should().BeGreaterThan(0);
            price.Country.Should().Be("Turkey");
        });

        Console.WriteLine($"EnergyPriceFetchWorker successfully stored {storedPrices.Count} price records for Turkey");
        Console.WriteLine($"Total records in history: {finalCount}");
    }

    /// <summary>
    /// Test that verifies the worker handles deduplication correctly -
    /// running the same fetch twice should not create duplicate records.
    /// </summary>
    [Test]
    [Category("Integration")]
    [Category("ExternalApi")]
    public async Task ShouldNotCreateDuplicateRecords()
    {
        // Arrange
        var tenant = new Tenant { Name = "Dedup Test Holding", Identifier = "dedup-test-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Dedup Test Firm",
            Type = "Standard",
            Country = "Turkey",
            State = "Ankara",
            City = "Ankara",
            Address = "Test Address",
            ContactEmail = "test@dedup.com",
            ContactPhone = "+901234567891",
            TaxNumber = "1234567891",
            TaxOffice = "Test"
        };
        await Testing.AddAsync(firm);

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

        // Act - Run the worker twice
        await Testing.ExecuteWithServiceAsync<EnergyPriceFetchWorker>(async worker =>
        {
            await worker.FetchMorningPrices();
        });

        var countAfterFirstFetch = await Testing.CountAsync<DayAheadEnergyPriceHistory>();

        await Testing.ExecuteWithServiceAsync<EnergyPriceFetchWorker>(async worker =>
        {
            await worker.FetchMorningPrices();
        });

        var countAfterSecondFetch = await Testing.CountAsync<DayAheadEnergyPriceHistory>();

        // Assert - Count should be the same (no duplicates)
        countAfterSecondFetch.Should().Be(countAfterFirstFetch,
            "Running the worker twice should not create duplicate records");

        Console.WriteLine($"Records after first fetch: {countAfterFirstFetch}");
        Console.WriteLine($"Records after second fetch: {countAfterSecondFetch}");
        Console.WriteLine("Deduplication working correctly - no duplicates created");
    }

    /// <summary>
    /// Test that verifies the afternoon fetch gets tomorrow's prices.
    /// </summary>
    [Test]
    [Category("Integration")]
    [Category("ExternalApi")]
    public async Task ShouldFetchTomorrowPricesInAfternoonJob()
    {
        // Arrange
        var tenant = new Tenant { Name = "Afternoon Test Holding", Identifier = "afternoon-test-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Afternoon Test Firm",
            Type = "Standard",
            Country = "Turkey",
            State = "İzmir",
            City = "İzmir",
            Address = "Test Address",
            ContactEmail = "test@afternoon.com",
            ContactPhone = "+901234567892",
            TaxNumber = "1234567892",
            TaxOffice = "Test"
        };
        await Testing.AddAsync(firm);

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

        var initialCount = await Testing.CountAsync<DayAheadEnergyPriceHistory>();

        // Act - Execute the afternoon fetch (fetches tomorrow's prices)
        await Testing.ExecuteWithServiceAsync<EnergyPriceFetchWorker>(async worker =>
        {
            await worker.FetchAfternoonPrices();
        });

        // Assert
        var finalCount = await Testing.CountAsync<DayAheadEnergyPriceHistory>();

        // Note: Tomorrow's prices may not be available yet depending on when the test runs
        // EPIAS publishes around 14:00 Turkey time
        if (finalCount > initialCount)
        {
            Console.WriteLine($"Afternoon job fetched {finalCount - initialCount} price records for tomorrow");

            // Verify the stored records are for tomorrow
            var tomorrow = DateTimeOffset.UtcNow.Date.AddDays(1);
            var tomorrowPrices = await Testing.GetAllAsync<DayAheadEnergyPriceHistory>(
                p => p.MarketOperatorId == marketOperator.Id &&
                     p.Country == "Turkey" &&
                     p.PriceLocalTimestamp.Date == tomorrow);

            if (tomorrowPrices.Count > 0)
            {
                tomorrowPrices.Should().HaveCount(24, "Should have 24 hourly prices for tomorrow");
                Console.WriteLine($"Verified {tomorrowPrices.Count} price records for {tomorrow:yyyy-MM-dd}");
            }
        }
        else
        {
            Console.WriteLine("Tomorrow's prices not yet available (this is expected before ~14:00 Turkey time)");
        }
    }

    /// <summary>
    /// Test that verifies the worker skips inactive firm market operator configurations.
    /// </summary>
    [Test]
    public async Task ShouldSkipInactiveFirmMarketOperators()
    {
        // Arrange
        var tenant = new Tenant { Name = "Inactive Test Holding", Identifier = "inactive-test-holding" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Inactive Test Firm",
            Type = "Standard",
            Country = "Turkey",
            State = "Bursa",
            City = "Bursa",
            Address = "Test Address",
            ContactEmail = "test@inactive.com",
            ContactPhone = "+901234567893",
            TaxNumber = "1234567893",
            TaxOffice = "Test"
        };
        await Testing.AddAsync(firm);

        // Create a unique market operator for this test
        var uniqueCode = $"TEST_{Guid.NewGuid():N}";
        var marketOperator = new MarketOperator
        {
            Name = "Test Inactive MO",
            Code = uniqueCode,
            BaseUrl = "https://example.com",
            ApiKey = "test-key",
            SupportedResolutions = new List<int> { 60 },
            AreaCodes = new List<string> { "TEST" },
            IsActive = true
        };
        await Testing.AddAsync(marketOperator);

        // Create an INACTIVE firm market operator
        var firmMarketOperator = new FirmMarketOperator
        {
            FirmId = firm.Id,
            MarketOperatorId = marketOperator.Id,
            AreaCode = "TEST",
            SamplingPeriodMinutes = 60,
            IsActive = false // Inactive!
        };
        await Testing.AddAsync(firmMarketOperator);

        var initialCount = await Testing.CountAsync<DayAheadEnergyPriceHistory>();

        // Act - Execute the worker
        await Testing.ExecuteWithServiceAsync<EnergyPriceFetchWorker>(async worker =>
        {
            await worker.FetchMorningPrices();
        });

        // Assert - No new records should be created for the inactive configuration
        var pricesForTestMO = await Testing.GetAllAsync<DayAheadEnergyPriceHistory>(
            p => p.MarketOperatorId == marketOperator.Id);

        pricesForTestMO.Should().BeEmpty("Inactive firm market operators should be skipped");

        Console.WriteLine("Verified: Inactive FirmMarketOperator configurations are correctly skipped");
    }
}
