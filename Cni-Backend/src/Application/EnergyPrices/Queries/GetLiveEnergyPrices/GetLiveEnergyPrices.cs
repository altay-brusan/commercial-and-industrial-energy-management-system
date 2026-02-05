using Backend.Application.Common;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.EnergyPrices.Queries.GetLiveEnergyPrices;

/// <summary>
/// Fetches live/current day-ahead prices directly from a Market Operator API.
/// Uses the firm's configured Market Operator or falls back to default based on Firm's country.
/// This does NOT store the data - it's for real-time display.
/// </summary>
[Authorize(Roles = "Administrator,Tenant.Admin")]
public record GetLiveEnergyPricesQuery : IRequest<LiveEnergyPricesDto>
{
    /// <summary>
    /// Firm ID to fetch prices for. Required.
    /// </summary>
    public int FirmId { get; set; }

    /// <summary>
    /// The date to fetch prices for. If not specified, uses today or tomorrow based on MO rules.
    /// </summary>
    public DateTimeOffset? Date { get; set; }

    /// <summary>
    /// Requested resolution in minutes (15, 30, or 60). Defaults to 60 (hourly).
    /// If the source data has higher resolution, it will be aggregated.
    /// </summary>
    public int? Period { get; set; }
}

public class GetLiveEnergyPricesQueryHandler : IRequestHandler<GetLiveEnergyPricesQuery, LiveEnergyPricesDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IEnergyPriceService _energyPriceService;
    private readonly ICountryMappingService _countryMappingService;

    public GetLiveEnergyPricesQueryHandler(
        IApplicationDbContext context,
        IEnergyPriceService energyPriceService,
        ICountryMappingService countryMappingService)
    {
        _context = context;
        _energyPriceService = energyPriceService;
        _countryMappingService = countryMappingService;
    }

    public async Task<LiveEnergyPricesDto> Handle(GetLiveEnergyPricesQuery request, CancellationToken cancellationToken)
    {
        // Verify the firm exists
        var firm = await _context.Firms
            .Where(f => f.Id == request.FirmId)
            .FirstOrDefaultAsync(cancellationToken);

        if (firm == null)
        {
            return new LiveEnergyPricesDto
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

        // Determine the date to fetch
        var targetDate = request.Date ?? DetermineTargetDate(mo.Code);

        // Get country name from area code for tariff lookup
        var countryName = _countryMappingService.GetCountryNameFromAreaCode(areaCode);

        // Fetch prices from the Market Operator
        var result = await _energyPriceService.GetDayAheadPricesAsync(
            mo.Code,
            mo.BaseUrl,
            mo.ApiKey,
            countryName,
            targetDate,
            request.Period ?? firmMo.SamplingPeriodMinutes,
            cancellationToken);

        if (!result.Success)
        {
            return new LiveEnergyPricesDto
            {
                Success = false,
                ErrorMessage = result.ErrorMessage,
                MarketOperatorId = mo.Id,
                MarketOperatorName = mo.Name,
                MarketOperatorCode = mo.Code,
                AreaCode = areaCode,
                Date = targetDate,
                ResolutionMinutes = result.ResolutionMinutes,
                Timezone = result.Timezone
            };
        }

        // Fetch country tariff from database (or use defaults)
        // For tariffs, we'll use the first facility of this firm (or use defaults)
        var (distributionFee, transmissionFee, taxRate, currency, usingDefaultTariff) =
            await GetTariffForCountryAsync(request.FirmId, countryName, cancellationToken);

        // Calculate prices with formula: (NetPrice + DistributionFee + TransmissionFee) × (1 + TaxRate)
        var prices = result.Prices.Select(p =>
        {
            var grossPrice = (p.PricePerMWh + distributionFee + transmissionFee) * (1 + taxRate);
            return new LivePricePointDto
            {
                Timestamp = p.Timestamp,
                LocalTimestamp = p.LocalTimestamp,
                NetPrice = p.PricePerMWh,
                GrossPrice = grossPrice
            };
        }).ToList();

        // Calculate statistics
        LivePriceStatisticsDto? statistics = null;
        if (prices.Count > 0)
        {
            var minNetPrice = prices.Min(p => p.NetPrice);
            var maxNetPrice = prices.Max(p => p.NetPrice);

            statistics = new LivePriceStatisticsDto
            {
                MinNetPrice = minNetPrice,
                MaxNetPrice = maxNetPrice,
                AvgNetPrice = prices.Average(p => p.NetPrice),
                MinGrossPrice = prices.Min(p => p.GrossPrice),
                MaxGrossPrice = prices.Max(p => p.GrossPrice),
                AvgGrossPrice = prices.Average(p => p.GrossPrice),
                MinPriceHour = prices.FirstOrDefault(p => p.NetPrice == minNetPrice)?.LocalTimestamp.Hour,
                MaxPriceHour = prices.FirstOrDefault(p => p.NetPrice == maxNetPrice)?.LocalTimestamp.Hour
            };
        }

        return new LiveEnergyPricesDto
        {
            Success = true,
            FirmId = request.FirmId,
            MarketOperatorId = mo.Id,
            MarketOperatorName = mo.Name,
            MarketOperatorCode = mo.Code,
            AreaCode = areaCode,
            Country = countryName ?? string.Empty,
            Currency = currency,
            Date = targetDate,
            DistributionFee = distributionFee,
            TransmissionFee = transmissionFee,
            TaxRate = taxRate,
            UsingDefaultTariff = usingDefaultTariff,
            Prices = prices,
            Statistics = statistics,
            ResolutionMinutes = result.ResolutionMinutes,
            Timezone = result.Timezone
        };
    }

    private async Task<(LiveEnergyPricesDto Result, Domain.Entities.FirmMarketOperator? FirmMarketOperator)> GetFallbackMarketOperatorAsync(Domain.Entities.Firm firm, CancellationToken cancellationToken)
    {
        if (firm.Country == null)
        {
            return (new LiveEnergyPricesDto
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
            return (new LiveEnergyPricesDto
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

        return (new LiveEnergyPricesDto { Success = true }, virtualFmo);
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
