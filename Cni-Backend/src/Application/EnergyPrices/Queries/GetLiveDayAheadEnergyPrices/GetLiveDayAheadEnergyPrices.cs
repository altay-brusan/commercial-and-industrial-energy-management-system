using Backend.Application.Common;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.EnergyPrices.Queries.GetLiveDayAheadEnergyPrices;

/// <summary>
/// Fetches live/current day-ahead prices directly from a Market Operator API.
/// Uses the firm's configured Market Operator or falls back to default based on Firm's country.
/// This does NOT store the data - it's for real-time display.
/// Resolution is determined by the Market Operator (e.g., ENTSO-E returns PT15M or PT60M based on market).
/// </summary>
[Authorize(Roles = "Administrator,Tenant.Admin")]
public record GetLiveDayAheadEnergyPricesQuery : IRequest<LiveDayAheadEnergyPricesDto>
{
    /// <summary>
    /// Firm ID to fetch prices for. Required.
    /// </summary>
    public int FirmId { get; set; }

    /// <summary>
    /// Start date (inclusive). If not specified, uses today or tomorrow based on MO publication rules.
    /// </summary>
    public DateTimeOffset? StartDate { get; set; }

    /// <summary>
    /// End date (inclusive). If not specified, defaults to StartDate + 1 day.
    /// </summary>
    public DateTimeOffset? EndDate { get; set; }
}

public class GetLiveDayAheadEnergyPricesQueryHandler : IRequestHandler<GetLiveDayAheadEnergyPricesQuery, LiveDayAheadEnergyPricesDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IEnergyPriceService _energyPriceService;
    private readonly ICountryMappingService _countryMappingService;

    public GetLiveDayAheadEnergyPricesQueryHandler(
        IApplicationDbContext context,
        IEnergyPriceService energyPriceService,
        ICountryMappingService countryMappingService)
    {
        _context = context;
        _energyPriceService = energyPriceService;
        _countryMappingService = countryMappingService;
    }

    public async Task<LiveDayAheadEnergyPricesDto> Handle(GetLiveDayAheadEnergyPricesQuery request, CancellationToken cancellationToken)
    {
        // Verify the firm exists
        var firm = await _context.Firms
            .Where(f => f.Id == request.FirmId)
            .FirstOrDefaultAsync(cancellationToken);

        if (firm == null)
        {
            return new LiveDayAheadEnergyPricesDto
            {
                Success = false,
                ErrorMessage = $"Firm with ID {request.FirmId} not found"
            };
        }

        // Get firm's Market Operator configuration
        var firmMo = await _context.FirmMarketOperators
            .Include(x => x.MarketOperator)
            .Where(x => x.FirmId == request.FirmId && x.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        // If no firm MO configured, try to determine default based on Firm's country
        if (firmMo?.MarketOperator == null)
        {
            var fallbackResult = await GetFallbackMarketOperatorAsync(firm, cancellationToken);
            if (!fallbackResult.Result.Success)
            {
                return fallbackResult.Result;
            }
            // If fallback found, use that MO
            firmMo = fallbackResult.FirmMarketOperator;
        }

        var mo = firmMo!.MarketOperator!;
        var areaCode = firmMo.AreaCode;

        // Determine the date range to fetch
        var startDate = request.StartDate ?? DetermineTargetDate(mo.Code);
        // If EndDate not specified, default to StartDate + 1 day
        var endDate = request.EndDate ?? startDate.AddDays(1);

        // Get country name from area code for tariff lookup
        var countryName = _countryMappingService.GetCountryNameFromAreaCode(areaCode);

        // Fetch country tariff from database (or use defaults)
        var (distributionFee, transmissionFee, taxRate, currency, usingDefaultTariff) =
            await GetTariffForCountryAsync(request.FirmId, countryName, cancellationToken);

        // Fetch prices for each day in the range
        var allPrices = new List<LiveDayAheadPricePointDto>();
        var errors = new List<string>();
        int resolutionMinutes = 60; // Will be updated from API response
        string? timezone = null;

        var currentDate = startDate.Date;
        var endDateValue = endDate.Date;

        while (currentDate <= endDateValue)
        {
            // Resolution is determined by the Market Operator, not requested by us
            var result = await _energyPriceService.GetDayAheadPricesAsync(
                mo.Code,
                mo.BaseUrl,
                mo.ApiKey,
                countryName,
                currentDate,
                cancellationToken: cancellationToken);

            if (result.Success && result.Prices.Count > 0)
            {
                resolutionMinutes = result.ResolutionMinutes;
                timezone = result.Timezone;

                // Calculate prices with formula: (NetPrice + DistributionFee + TransmissionFee) × (1 + TaxRate)
                var dayPrices = result.Prices.Select(p =>
                {
                    var grossPrice = (p.PricePerMWh + distributionFee + transmissionFee) * (1 + taxRate);
                    return new LiveDayAheadPricePointDto
                    {
                        Timestamp = p.Timestamp,
                        LocalTimestamp = p.LocalTimestamp,
                        NetPrice = p.PricePerMWh,
                        GrossPrice = grossPrice
                    };
                });

                allPrices.AddRange(dayPrices);
            }
            else if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                errors.Add($"{currentDate:yyyy-MM-dd}: {result.ErrorMessage}");
            }

            currentDate = currentDate.AddDays(1);
        }

        // If no prices were fetched at all, return error
        if (allPrices.Count == 0)
        {
            return new LiveDayAheadEnergyPricesDto
            {
                Success = false,
                ErrorMessage = errors.Count > 0 ? string.Join("; ", errors) : "No price data found for the specified date range",
                MarketOperatorId = mo.Id,
                MarketOperatorName = mo.Name,
                MarketOperatorCode = mo.Code,
                AreaCode = areaCode,
                StartDate = startDate,
                EndDate = endDate,
                ResolutionMinutes = resolutionMinutes,
                Timezone = timezone
            };
        }

        // Sort all prices by timestamp
        allPrices = allPrices.OrderBy(p => p.Timestamp).ToList();

        // Calculate statistics across all days
        LiveDayAheadPriceStatisticsDto? statistics = null;
        if (allPrices.Count > 0)
        {
            var minNetPrice = allPrices.Min(p => p.NetPrice);
            var maxNetPrice = allPrices.Max(p => p.NetPrice);

            statistics = new LiveDayAheadPriceStatisticsDto
            {
                MinNetPrice = minNetPrice,
                MaxNetPrice = maxNetPrice,
                AvgNetPrice = allPrices.Average(p => p.NetPrice),
                MinGrossPrice = allPrices.Min(p => p.GrossPrice),
                MaxGrossPrice = allPrices.Max(p => p.GrossPrice),
                AvgGrossPrice = allPrices.Average(p => p.GrossPrice),
                MinPriceHour = allPrices.FirstOrDefault(p => p.NetPrice == minNetPrice)?.LocalTimestamp.Hour,
                MaxPriceHour = allPrices.FirstOrDefault(p => p.NetPrice == maxNetPrice)?.LocalTimestamp.Hour
            };
        }

        return new LiveDayAheadEnergyPricesDto
        {
            Success = true,
            FirmId = request.FirmId,
            MarketOperatorId = mo.Id,
            MarketOperatorName = mo.Name,
            MarketOperatorCode = mo.Code,
            AreaCode = areaCode,
            Country = countryName ?? string.Empty,
            Currency = currency,
            StartDate = startDate,
            EndDate = endDate,
            DistributionFee = distributionFee,
            TransmissionFee = transmissionFee,
            TaxRate = taxRate,
            UsingDefaultTariff = usingDefaultTariff,
            Prices = allPrices,
            Statistics = statistics,
            ResolutionMinutes = resolutionMinutes,
            Timezone = timezone,
            DataPointCount = allPrices.Count
        };
    }

    private async Task<(LiveDayAheadEnergyPricesDto Result, Domain.Entities.FirmMarketOperator? FirmMarketOperator)> GetFallbackMarketOperatorAsync(Domain.Entities.Firm firm, CancellationToken cancellationToken)
    {
        if (firm.Country == null)
        {
            return (new LiveDayAheadEnergyPricesDto
            {
                Success = false,
                ErrorMessage = "No Market Operator configured for this firm and no firm country found for fallback"
            }, null);
        }

        // Determine MO based on country
        var moCode = firm.Country.ToUpperInvariant() switch
        {
            "TURKEY" or "TÜRKİYE" or "TURKIYE" or "TR" => "EPIAS",
            _ => "ENTSOE" // Default to ENTSO-E for EU countries
        };

        var mo = await _context.MarketOperators
            .Where(m => m.Code == moCode && m.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        if (mo == null)
        {
            return (new LiveDayAheadEnergyPricesDto
            {
                Success = false,
                ErrorMessage = $"Default Market Operator '{moCode}' not found"
            }, null);
        }

        // Get the first area code from the MO or map from country
        var areaCode = _countryMappingService.GetAreaCode(moCode, firm.Country) ?? mo.AreaCodes.FirstOrDefault() ?? "";

        // Create a virtual FirmMarketOperator for the fallback
        var virtualFmo = new Domain.Entities.FirmMarketOperator
        {
            FirmId = firm.Id,
            MarketOperatorId = mo.Id,
            MarketOperator = mo,
            AreaCode = areaCode,
            SamplingPeriodMinutes = mo.SupportedResolutions.FirstOrDefault(r => r == 60) != 0
                ? 60
                : mo.SupportedResolutions.FirstOrDefault(),
            IsActive = true
        };

        return (new LiveDayAheadEnergyPricesDto { Success = true }, virtualFmo);
    }

    private async Task<(decimal distributionFee, decimal transmissionFee, decimal taxRate, string currency, bool usingDefault)> GetTariffForCountryAsync(
        int firmId,
        string? countryName,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(countryName))
        {
            return (TariffDefaults.DistributionFee, TariffDefaults.TransmissionFee, TariffDefaults.TaxRate, "EUR", true);
        }

        // Get facilities for this firm and find matching tariff through junction table
        var facilityIds = await _context.Facilities
            .Where(f => f.FirmId == firmId)
            .Select(f => f.Id)
            .ToListAsync(cancellationToken);

        if (facilityIds.Count > 0)
        {
            // Try to find tariff by country name (case-insensitive match) from any facility
            var tariff = await _context.FacilityTariffs
                .Include(ft => ft.Tariff)
                .Where(ft => facilityIds.Contains(ft.FacilityId) && ft.IsActive)
                .Where(ft => ft.Tariff != null && ft.Tariff.CountryName.ToLower() == countryName.ToLower() && ft.Tariff.IsActive)
                .Select(ft => ft.Tariff)
                .FirstOrDefaultAsync(cancellationToken);

            if (tariff != null)
            {
                return (tariff.DistributionFee, tariff.TransmissionFee, tariff.TaxRate, tariff.Currency, false);
            }
        }

        // Fall back to defaults
        return (TariffDefaults.DistributionFee, TariffDefaults.TransmissionFee, TariffDefaults.TaxRate, "EUR", true);
    }

    private DateTimeOffset DetermineTargetDate(string moCode)
    {
        // Different MOs publish day-ahead prices at different times
        // ENTSO-E: Around 13:00 CET
        // EPIAS: Around 14:00 Turkey time (UTC+3)

        var now = DateTimeOffset.UtcNow;

        return moCode.ToUpperInvariant() switch
        {
            "ENTSOE" => DetermineEntsoeTargetDate(now),
            "EPIAS" => DetermineEpiasTargetDate(now),
            _ => now.Date // Default to today
        };
    }

    private DateTimeOffset DetermineEntsoeTargetDate(DateTimeOffset utcNow)
    {
        // ENTSO-E publishes around 13:00 CET (12:00 UTC in winter, 11:00 UTC in summer)
        // If after 13:00 CET, get tomorrow's prices; otherwise today's
        var cetOffset = TimeSpan.FromHours(1); // CET is UTC+1 (ignoring DST for simplicity)
        var cetTime = utcNow.ToOffset(cetOffset);

        if (cetTime.Hour >= 13)
        {
            return cetTime.Date.AddDays(1);
        }
        return cetTime.Date;
    }

    private DateTimeOffset DetermineEpiasTargetDate(DateTimeOffset utcNow)
    {
        // EPIAS publishes around 14:00 Turkey time (UTC+3)
        var turkeyOffset = TimeSpan.FromHours(3);
        var turkeyTime = utcNow.ToOffset(turkeyOffset);

        if (turkeyTime.Hour >= 14)
        {
            return turkeyTime.Date.AddDays(1);
        }
        return turkeyTime.Date;
    }
}
