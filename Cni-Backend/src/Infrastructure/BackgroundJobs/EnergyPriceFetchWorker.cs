using Microsoft.Extensions.Logging;
using Backend.Application.Common.Interfaces;
using Backend.Domain.Entities;
using Backend.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.BackgroundJobs;

/// <summary>
/// Background worker that fetches energy prices for all active firm configurations.
/// Fetches data for all unique (MarketOperator, AreaCode) combinations from FirmMarketOperators.
/// Runs twice daily:
/// - Morning (0:01 AM UTC): Fetch current day prices
/// - Afternoon (12:01 PM UTC / ~13:01 CET): Fetch tomorrow's prices after ENTSO-E publishes
/// </summary>
public class EnergyPriceFetchWorker
{
    private readonly ILogger<EnergyPriceFetchWorker> _logger;
    private readonly IApplicationDbContext _context;
    private readonly IEnergyPriceService _energyPriceService;

    private const int MaxRetryAttempts = 3;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromMinutes(5);

    public EnergyPriceFetchWorker(
        ILogger<EnergyPriceFetchWorker> logger,
        IApplicationDbContext context,
        IEnergyPriceService energyPriceService)
    {
        _logger = logger;
        _context = context;
        _energyPriceService = energyPriceService;
    }

    /// <summary>
    /// Morning job - fetches current day prices for all firm configurations.
    /// Scheduled to run at 0:01 AM UTC.
    /// </summary>
    public async Task FetchMorningPrices()
    {
        var targetDate = DateTimeOffset.UtcNow.Date;
        _logger.LogInformation("Starting morning price fetch for {Date}", targetDate.ToString("yyyy-MM-dd"));
        await FetchPricesForAllFirmsAsync(targetDate);
    }

    /// <summary>
    /// Afternoon job - fetches tomorrow's prices after ENTSO-E publishes (~13:00 CET).
    /// Scheduled to run at 12:01 PM UTC (13:01 CET in winter).
    /// </summary>
    public async Task FetchAfternoonPrices()
    {
        var targetDate = DateTimeOffset.UtcNow.Date.AddDays(1);
        _logger.LogInformation("Starting afternoon price fetch for {Date}", targetDate.ToString("yyyy-MM-dd"));
        await FetchPricesForAllFirmsAsync(targetDate);
    }

    /// <summary>
    /// Fetches prices for all unique (MarketOperator, AreaCode) combinations from active firm configurations.
    /// </summary>
    private async Task FetchPricesForAllFirmsAsync(DateTimeOffset targetDate)
    {
        try
        {
            // Get all unique (MarketOperatorId, AreaCode) combinations from active firm configurations
            var firmConfigs = await _context.FirmMarketOperators
                .Include(fmo => fmo.MarketOperator)
                .Where(fmo => fmo.IsActive && fmo.MarketOperator != null && fmo.MarketOperator.IsActive)
                .ToListAsync();

            // Group by (MarketOperatorId, AreaCode) to avoid duplicate fetches
            var uniqueConfigs = firmConfigs
                .GroupBy(fmo => new { fmo.MarketOperatorId, fmo.AreaCode })
                .Select(g => g.First())
                .ToList();

            _logger.LogInformation("Found {TotalCount} firm configurations, {UniqueCount} unique (MO, AreaCode) combinations",
                firmConfigs.Count, uniqueConfigs.Count);

            var failedConfigs = new List<(FirmMarketOperator Config, string Error)>();

            foreach (var fmo in uniqueConfigs)
            {
                var success = await FetchPricesForConfigWithRetryAsync(fmo, targetDate);
                if (!success)
                {
                    failedConfigs.Add((fmo, "Failed after retries"));
                }
            }

            if (failedConfigs.Count > 0)
            {
                _logger.LogWarning("Price fetch completed with {FailedCount} failures for {Date}",
                    failedConfigs.Count, targetDate.ToString("yyyy-MM-dd"));
            }
            else
            {
                _logger.LogInformation("Price fetch completed successfully for {Date}", targetDate.ToString("yyyy-MM-dd"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during price fetch for {Date}", targetDate.ToString("yyyy-MM-dd"));
            throw;
        }
    }

    /// <summary>
    /// Fetches prices for a firm configuration with retry logic.
    /// </summary>
    private async Task<bool> FetchPricesForConfigWithRetryAsync(FirmMarketOperator fmo, DateTimeOffset targetDate)
    {
        var mo = fmo.MarketOperator!;

        for (int attempt = 1; attempt <= MaxRetryAttempts; attempt++)
        {
            try
            {
                var success = await FetchPricesForAreaAsync(mo, fmo.AreaCode, targetDate);
                if (success)
                {
                    return true;
                }

                if (attempt < MaxRetryAttempts)
                {
                    _logger.LogWarning("Fetch attempt {Attempt}/{MaxAttempts} failed for {MoCode}/{AreaCode}. Retrying in {Delay}...",
                        attempt, MaxRetryAttempts, mo.Code, fmo.AreaCode, RetryDelay);
                    await Task.Delay(RetryDelay);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on attempt {Attempt}/{MaxAttempts} fetching prices from {MoCode} for area {AreaCode}",
                    attempt, MaxRetryAttempts, mo.Code, fmo.AreaCode);

                if (attempt < MaxRetryAttempts)
                {
                    await Task.Delay(RetryDelay);
                }
            }
        }

        _logger.LogError("Failed to fetch prices from {MoCode} for area {AreaCode} after {MaxAttempts} attempts",
            mo.Code, fmo.AreaCode, MaxRetryAttempts);
        return false;
    }

    /// <summary>
    /// Fetches prices for a specific area and stores them in the database.
    /// </summary>
    /// <returns>True if prices were successfully fetched (even if all were duplicates), false otherwise.</returns>
    private async Task<bool> FetchPricesForAreaAsync(MarketOperator mo, string areaCode, DateTimeOffset targetDate)
    {
        // Get country name from area code - this is what we'll store in the database
        var countryName = AreaCodeMapping.GetCountryNameFromAreaCode(areaCode);

        if (string.IsNullOrEmpty(countryName))
        {
            _logger.LogWarning("Could not map area code {AreaCode} to country name, skipping", areaCode);
            return false;
        }

        // Resolution is determined by the Market Operator API response
        var result = await _energyPriceService.GetDayAheadPricesAsync(
            mo.Code,
            mo.BaseUrl,
            mo.ApiKey,
            countryName,
            targetDate,
            CancellationToken.None);

        if (!result.Success)
        {
            _logger.LogWarning("Failed to fetch prices from {MoCode} for {Country}: {Error}",
                mo.Code, countryName, result.ErrorMessage);
            return false;
        }

        if (result.Prices.Count == 0)
        {
            _logger.LogWarning("No prices returned from {MoCode} for {Country} on {Date}",
                mo.Code, countryName, targetDate.ToString("yyyy-MM-dd"));
            return false;
        }

        var insertedCount = 0;
        var skippedCount = 0;

        foreach (var pricePoint in result.Prices)
        {
            // Check if this price point already exists (deduplication by country name)
            var exists = await _context.DayAheadEnergyPriceHistories
                .AnyAsync(e =>
                    e.MarketOperatorId == mo.Id &&
                    e.Country == countryName &&
                    e.PriceLocalTimestamp == pricePoint.LocalTimestamp);

            if (exists)
            {
                skippedCount++;
                continue;
            }

            // Store with country name instead of area code
            var entity = new DayAheadEnergyPriceHistory
            {
                MarketOperatorId = mo.Id,
                Country = countryName,
                NetPrice = pricePoint.PricePerMWh,
                GrossPrice = pricePoint.PricePerMWh, // Gross price calculated at query time with tenant tariffs
                PriceLocalTimestamp = pricePoint.LocalTimestamp
            };

            _context.DayAheadEnergyPriceHistories.Add(entity);
            insertedCount++;
        }

        if (insertedCount > 0)
        {
            await _context.SaveChangesAsync(CancellationToken.None);
        }

        _logger.LogInformation("Fetched prices from {MoCode} for {Country}: {Inserted} inserted, {Skipped} skipped (duplicates)",
            mo.Code, countryName, insertedCount, skippedCount);

        return true;
    }
}
