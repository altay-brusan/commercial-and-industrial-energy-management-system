using Microsoft.Extensions.Logging;
using Backend.Application.Common.Interfaces;
using Backend.Domain.Entities;
using Backend.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.BackgroundJobs;

/// <summary>
/// Background worker that fetches energy prices from all active Market Operators.
/// Runs twice daily:
/// - Morning (0:01 AM UTC): Fetch current day prices
/// - Afternoon (12:01 PM UTC / ~13:01 CET): Fetch tomorrow's prices after ENTSO-E publishes
/// </summary>
public class EnergyPriceFetchWorker
{
    private readonly ILogger<EnergyPriceFetchWorker> _logger;
    private readonly IApplicationDbContext _context;
    private readonly ITsoService _tsoService;

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
    /// Morning job - fetches current day prices for all active Market Operators.
    /// Scheduled to run at 0:01 AM UTC.
    /// </summary>
    public async Task FetchMorningPrices()
    {
        var targetDate = DateTimeOffset.UtcNow.Date;
        _logger.LogInformation("Starting morning price fetch for {Date}", targetDate.ToString("yyyy-MM-dd"));
        await FetchPricesForAllMarketOperatorsAsync(targetDate);
    }

    /// <summary>
    /// Afternoon job - fetches tomorrow's prices after ENTSO-E publishes (~13:00 CET).
    /// Scheduled to run at 12:01 PM UTC (13:01 CET in winter).
    /// </summary>
    public async Task FetchAfternoonPrices()
    {
        var targetDate = DateTimeOffset.UtcNow.Date.AddDays(1);
        _logger.LogInformation("Starting afternoon price fetch for {Date}", targetDate.ToString("yyyy-MM-dd"));
        await FetchPricesForAllMarketOperatorsAsync(targetDate);
    }

    private async Task FetchPricesForAllMarketOperatorsAsync(DateTimeOffset targetDate)
    {
        try
        {
            // Get all active Market Operators
            var marketOperators = await _context.MarketOperators
                .Where(mo => mo.IsActive)
                .ToListAsync();

            _logger.LogInformation("Found {Count} active Market Operators", marketOperators.Count);

            foreach (var mo in marketOperators)
            {
                await FetchPricesForMarketOperatorAsync(mo, targetDate);
            }

            _logger.LogInformation("Price fetch completed for {Date}", targetDate.ToString("yyyy-MM-dd"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during price fetch for {Date}", targetDate.ToString("yyyy-MM-dd"));
            throw;
        }
    }

    private async Task FetchPricesForMarketOperatorAsync(MarketOperator mo, DateTimeOffset targetDate)
    {
        _logger.LogInformation("Fetching prices from {MoName} ({MoCode}) for {Date}",
            mo.Name, mo.Code, targetDate.ToString("yyyy-MM-dd"));

        foreach (var areaCode in mo.AreaCodes)
        {
            try
            {
                await FetchPricesForAreaAsync(mo, areaCode, targetDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching prices from {MoCode} for area {AreaCode}",
                    mo.Code, areaCode);
                // Continue with next area code
            }
        }
    }

    private async Task FetchPricesForAreaAsync(MarketOperator mo, string areaCode, DateTimeOffset targetDate)
    {
        // Get country name from area code for the API call
        var countryName = TsoCountryMapping.GetCountryNameFromAreaCode(areaCode);

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
            _logger.LogWarning("Failed to fetch prices from {MoCode} for area {AreaCode}: {Error}",
                mo.Code, areaCode, result.ErrorMessage);
            return;
        }

        var insertedCount = 0;
        var skippedCount = 0;

        foreach (var pricePoint in result.Prices)
        {
            // Check if this price point already exists (deduplication)
            var exists = await _context.EnergyPriceHistories
                .AnyAsync(e =>
                    e.MarketOperatorId == mo.Id &&
                    e.AreaCode == areaCode &&
                    e.PriceLocalTimestamp == pricePoint.LocalTimestamp);

            if (exists)
            {
                skippedCount++;
                continue;
            }

            // Calculate gross price (for now, store net price as gross too - tariff calculation happens at query time)
            var entity = new EnergyPriceHistory
            {
                MarketOperatorId = mo.Id,
                AreaCode = areaCode,
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

        _logger.LogInformation("Fetched prices from {MoCode} for area {AreaCode}: {Inserted} inserted, {Skipped} skipped (duplicates)",
            mo.Code, areaCode, insertedCount, skippedCount);
    }
}
