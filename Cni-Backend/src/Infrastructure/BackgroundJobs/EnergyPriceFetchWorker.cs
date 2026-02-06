using Microsoft.Extensions.Logging;
using Backend.Application.Common.Interfaces;
using Backend.Domain.Entities;
using Backend.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.BackgroundJobs;

/// <summary>
/// Background worker that fetches day-ahead energy prices for all active firm configurations.
/// Fetches data for all unique (MarketOperator, AreaCode) combinations from FirmMarketOperators.
/// Scheduled to run twice daily (early morning and afternoon) to ensure prices are captured.
/// Deduplication ensures re-runs are safe — already stored prices are skipped.
/// </summary>
public class EnergyPriceFetchWorker
{
    private readonly ILogger<EnergyPriceFetchWorker> _logger;
    private readonly IApplicationDbContext _context;
    private readonly IEnergyPriceService _energyPriceService;

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
    /// Fetches today's day-ahead energy prices for all active firm configurations.
    /// Hangfire handles retries automatically — if any source fails, the job throws
    /// and Hangfire reschedules it. Deduplication ensures already-stored prices are skipped.
    /// </summary>
    public async Task FetchDailyPrices()
    {
        var targetDate = DateTimeOffset.UtcNow.Date;
        _logger.LogInformation("Starting daily price fetch for {Date}", targetDate.ToString("yyyy-MM-dd"));

        // Get all unique (MarketOperatorId, AreaCode) combinations from active firm configurations
        var firmConfigs = await _context.FirmMarketOperators
            .Include(fmo => fmo.MarketOperator)
            .Where(fmo => fmo.IsActive && fmo.MarketOperator != null && fmo.MarketOperator.IsActive)
            .ToListAsync();

        var uniqueConfigs = firmConfigs
            .GroupBy(fmo => new { fmo.MarketOperatorId, fmo.AreaCode })
            .Select(g => g.First())
            .ToList();

        _logger.LogInformation("Found {TotalCount} firm configurations, {UniqueCount} unique (MO, AreaCode) combinations",
            firmConfigs.Count, uniqueConfigs.Count);

        var errors = new List<string>();

        foreach (var fmo in uniqueConfigs)
        {
            try
            {
                await FetchPricesForAreaAsync(fmo.MarketOperator!, fmo.AreaCode, targetDate);
            }
            catch (Exception ex)
            {
                var msg = $"{fmo.MarketOperator!.Code}/{fmo.AreaCode}: {ex.Message}";
                _logger.LogError(ex, "Failed to fetch prices for {MoCode}/{AreaCode}", fmo.MarketOperator.Code, fmo.AreaCode);
                errors.Add(msg);
            }
        }

        if (errors.Count > 0)
        {
            throw new AggregateException(
                $"Price fetch failed for {errors.Count} source(s): {string.Join("; ", errors)}",
                errors.Select(e => new InvalidOperationException(e)));
        }

        _logger.LogInformation("Daily price fetch completed successfully for {Date}", targetDate.ToString("yyyy-MM-dd"));
    }

    /// <summary>
    /// Fetches prices for a specific area and stores them in the database.
    /// Throws on failure so Hangfire can handle the retry.
    /// </summary>
    private async Task FetchPricesForAreaAsync(MarketOperator mo, string areaCode, DateTimeOffset targetDate)
    {
        var countryName = AreaCodeMapping.GetCountryNameFromAreaCode(areaCode);

        if (string.IsNullOrEmpty(countryName))
        {
            _logger.LogWarning("Could not map area code {AreaCode} to country name, skipping", areaCode);
            return;
        }

        var result = await _energyPriceService.GetDayAheadPricesAsync(
            mo.Code,
            mo.BaseUrl,
            mo.ApiKey,
            countryName,
            targetDate,
            CancellationToken.None);

        if (!result.Success)
        {
            throw new InvalidOperationException(
                $"Failed to fetch prices from {mo.Code} for {countryName}: {result.ErrorMessage}");
        }

        if (result.Prices.Count == 0)
        {
            _logger.LogWarning("No prices returned from {MoCode} for {Country} on {Date}",
                mo.Code, countryName, targetDate.ToString("yyyy-MM-dd"));
            return;
        }

        var insertedCount = 0;
        var skippedCount = 0;

        // Batch-fetch existing timestamps to avoid N+1 queries
        var incomingTimestamps = result.Prices
            .Select(p => p.Timestamp.ToUniversalTime())
            .ToList();

        var existingTimestampsList = await _context.DayAheadNetEnergyPriceHistories
            .Where(e => e.MarketOperatorId == mo.Id && e.Country == countryName &&
                        incomingTimestamps.Contains(e.TimestampUtc))
            .Select(e => e.TimestampUtc)
            .ToListAsync();
        var existingTimestamps = existingTimestampsList.ToHashSet();

        foreach (var pricePoint in result.Prices)
        {
            var utcTimestamp = pricePoint.Timestamp.ToUniversalTime();

            if (existingTimestamps.Contains(utcTimestamp))
            {
                skippedCount++;
                continue;
            }

            var entity = new DayAheadNetEnergyPriceHistory
            {
                MarketOperatorId = mo.Id,
                Country = countryName,
                NetPrice = pricePoint.PricePerMWh,
                TimestampUtc = utcTimestamp
            };

            _context.DayAheadNetEnergyPriceHistories.Add(entity);
            insertedCount++;
        }

        if (insertedCount > 0)
        {
            await _context.SaveChangesAsync(CancellationToken.None);
        }

        _logger.LogInformation("Fetched prices from {MoCode} for {Country}: {Inserted} inserted, {Skipped} skipped (duplicates)",
            mo.Code, countryName, insertedCount, skippedCount);
    }
}
