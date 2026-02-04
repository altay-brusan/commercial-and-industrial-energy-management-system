using Microsoft.Extensions.Logging;
using Backend.Application.Common.Interfaces;
using Backend.Domain.Entities;
using Backend.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.BackgroundJobs;

/// <summary>
/// Background worker that fetches energy prices for all active tenant configurations.
/// Fetches data for all unique (MarketOperator, AreaCode) combinations from TenantMarketOperators.
/// Runs twice daily:
/// - Morning (0:01 AM UTC): Fetch current day prices
/// - Afternoon (12:01 PM UTC / ~13:01 CET): Fetch tomorrow's prices after ENTSO-E publishes
/// </summary>
public class EnergyPriceFetchWorker
{
    private readonly ILogger<EnergyPriceFetchWorker> _logger;
    private readonly IApplicationDbContext _context;
    private readonly ITsoService _tsoService;

    private const int MaxRetryAttempts = 3;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromMinutes(5);

    public EnergyPriceFetchWorker(
        ILogger<EnergyPriceFetchWorker> logger,
        IApplicationDbContext context,
        ITsoService tsoService)
    {
        _logger = logger;
        _context = context;
        _tsoService = tsoService;
    }

    /// <summary>
    /// Morning job - fetches current day prices for all tenant configurations.
    /// Scheduled to run at 0:01 AM UTC.
    /// </summary>
    public async Task FetchMorningPrices()
    {
        var targetDate = DateTimeOffset.UtcNow.Date;
        _logger.LogInformation("Starting morning price fetch for {Date}", targetDate.ToString("yyyy-MM-dd"));
        await FetchPricesForAllTenantsAsync(targetDate);
    }

    /// <summary>
    /// Afternoon job - fetches tomorrow's prices after ENTSO-E publishes (~13:00 CET).
    /// Scheduled to run at 12:01 PM UTC (13:01 CET in winter).
    /// </summary>
    public async Task FetchAfternoonPrices()
    {
        var targetDate = DateTimeOffset.UtcNow.Date.AddDays(1);
        _logger.LogInformation("Starting afternoon price fetch for {Date}", targetDate.ToString("yyyy-MM-dd"));
        await FetchPricesForAllTenantsAsync(targetDate);
    }

    /// <summary>
    /// Fetches prices for all unique (MarketOperator, AreaCode) combinations from active tenant configurations.
    /// </summary>
    private async Task FetchPricesForAllTenantsAsync(DateTimeOffset targetDate)
    {
        try
        {
            // Get all unique (MarketOperatorId, AreaCode) combinations from active tenant configurations
            var tenantConfigs = await _context.TenantMarketOperators
                .Include(tmo => tmo.MarketOperator)
                .Where(tmo => tmo.IsActive && tmo.MarketOperator != null && tmo.MarketOperator.IsActive)
                .ToListAsync();

            // Group by (MarketOperatorId, AreaCode) to avoid duplicate fetches
            var uniqueConfigs = tenantConfigs
                .GroupBy(tmo => new { tmo.MarketOperatorId, tmo.AreaCode })
                .Select(g => g.First())
                .ToList();

            _logger.LogInformation("Found {TotalCount} tenant configurations, {UniqueCount} unique (MO, AreaCode) combinations",
                tenantConfigs.Count, uniqueConfigs.Count);

            var failedConfigs = new List<(TenantMarketOperator Config, string Error)>();

            foreach (var tmo in uniqueConfigs)
            {
                var success = await FetchPricesForConfigWithRetryAsync(tmo, targetDate);
                if (!success)
                {
                    failedConfigs.Add((tmo, "Failed after retries"));
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
    /// Fetches prices for a tenant configuration with retry logic.
    /// </summary>
    private async Task<bool> FetchPricesForConfigWithRetryAsync(TenantMarketOperator tmo, DateTimeOffset targetDate)
    {
        var mo = tmo.MarketOperator!;

        for (int attempt = 1; attempt <= MaxRetryAttempts; attempt++)
        {
            try
            {
                var success = await FetchPricesForAreaAsync(mo, tmo.AreaCode, targetDate);
                if (success)
                {
                    return true;
                }

                if (attempt < MaxRetryAttempts)
                {
                    _logger.LogWarning("Fetch attempt {Attempt}/{MaxAttempts} failed for {MoCode}/{AreaCode}. Retrying in {Delay}...",
                        attempt, MaxRetryAttempts, mo.Code, tmo.AreaCode, RetryDelay);
                    await Task.Delay(RetryDelay);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on attempt {Attempt}/{MaxAttempts} fetching prices from {MoCode} for area {AreaCode}",
                    attempt, MaxRetryAttempts, mo.Code, tmo.AreaCode);

                if (attempt < MaxRetryAttempts)
                {
                    await Task.Delay(RetryDelay);
                }
            }
        }

        _logger.LogError("Failed to fetch prices from {MoCode} for area {AreaCode} after {MaxAttempts} attempts",
            mo.Code, tmo.AreaCode, MaxRetryAttempts);
        return false;
    }

    /// <summary>
    /// Fetches prices for a specific area and stores them in the database.
    /// </summary>
    /// <returns>True if prices were successfully fetched (even if all were duplicates), false otherwise.</returns>
    private async Task<bool> FetchPricesForAreaAsync(MarketOperator mo, string areaCode, DateTimeOffset targetDate)
    {
        // Get country name from area code - this is what we'll store in the database
        var countryName = TsoCountryMapping.GetCountryNameFromAreaCode(areaCode);

        if (string.IsNullOrEmpty(countryName))
        {
            _logger.LogWarning("Could not map area code {AreaCode} to country name, skipping", areaCode);
            return false;
        }

        // Use the first supported resolution (typically 60 for hourly data)
        var resolution = mo.SupportedResolutions.FirstOrDefault(r => r == 60) != 0
            ? 60
            : mo.SupportedResolutions.FirstOrDefault();

        var result = await _tsoService.GetDayAheadPricesAsync(
            mo.Code,
            mo.BaseUrl,
            mo.ApiKey,
            countryName,
            targetDate,
            resolution,
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
            var exists = await _context.EnergyPriceHistories
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
            var entity = new EnergyPriceHistory
            {
                MarketOperatorId = mo.Id,
                Country = countryName,
                NetPrice = pricePoint.PricePerMWh,
                GrossPrice = pricePoint.PricePerMWh, // Gross price calculated at query time with tenant tariffs
                PriceLocalTimestamp = pricePoint.LocalTimestamp
            };

            _context.EnergyPriceHistories.Add(entity);
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
