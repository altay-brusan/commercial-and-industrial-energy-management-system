using Backend.Application.Common;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.EnergyPrices.Queries.GetLiveTsoPrices;

/// <summary>
/// Fetches live/current day-ahead prices directly from a Market Operator API.
/// Uses the tenant's configured Market Operator or falls back to default based on Firm's country.
/// This does NOT store the data - it's for real-time display.
/// </summary>
[Authorize(Roles = "Administrator,Tenant.Admin")]
public record GetLiveEnergyPricesQuery : IRequest<LiveEnergyPricesDto>
{
    /// <summary>
    /// Optional tenant ID. If provided (admin only), fetches prices for that tenant's configuration.
    /// If not provided, uses the current tenant context.
    /// </summary>
    public int? TenantId { get; set; }

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

public class GetLiveEnergyPricesQueryValidator : AbstractValidator<GetLiveEnergyPricesQuery>
{
    public GetLiveEnergyPricesQueryValidator()
    {
        RuleFor(v => v.Period)
            .Must(p => p == null || new[] { 15, 30, 60 }.Contains(p.Value))
            .WithMessage("Period must be 15, 30, or 60 minutes");
    }
}

public class GetLiveEnergyPricesQueryHandler : IRequestHandler<GetLiveEnergyPricesQuery, LiveEnergyPricesDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITsoService _tsoService;
    private readonly ICountryMappingService _countryMappingService;
    private readonly IMultiTenantService _multiTenantService;

    public GetLiveEnergyPricesQueryHandler(
        IApplicationDbContext context,
        ITsoService tsoService,
        ICountryMappingService countryMappingService,
        IMultiTenantService multiTenantService)
    {
        _context = context;
        _tsoService = tsoService;
        _countryMappingService = countryMappingService;
        _multiTenantService = multiTenantService;
    }

    public async Task<LiveEnergyPricesDto> Handle(GetLiveEnergyPricesQuery request, CancellationToken cancellationToken)
    {
        int tenantId;

        // If TenantId is explicitly provided in the request
        if (request.TenantId.HasValue)
        {
            // Verify the user has admin access to query other tenants
            if (!_multiTenantService.IsSuperAdmin())
            {
                return new LiveEnergyPricesDto
                {
                    Success = false,
                    ErrorMessage = "Only administrators can query prices for other tenants"
                };
            }

            // Verify the tenant exists
            var tenantExists = await _context.Tenants.AnyAsync(t => t.Id == request.TenantId.Value, cancellationToken);
            if (!tenantExists)
            {
                return new LiveEnergyPricesDto
                {
                    Success = false,
                    ErrorMessage = $"Tenant with ID {request.TenantId.Value} not found"
                };
            }

            tenantId = request.TenantId.Value;
        }
        else
        {
            // Use the current tenant context
            if (!_multiTenantService.IsTenantScoped())
            {
                return new LiveEnergyPricesDto
                {
                    Success = false,
                    ErrorMessage = "Please select a tenant or provide TenantId to view energy prices"
                };
            }

            tenantId = _multiTenantService.GetEffectiveTenantId()!.Value;
        }

        // Get tenant's Market Operator configuration
        var tenantMo = await _context.TenantMarketOperators
            .Include(x => x.MarketOperator)
            .Where(x => x.TenantId == tenantId && x.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        // If no tenant MO configured, try to determine default based on Firm's country
        if (tenantMo?.MarketOperator == null)
        {
            var fallbackResult = await GetFallbackMarketOperatorAsync(tenantId, cancellationToken);
            if (!fallbackResult.Result.Success)
            {
                return fallbackResult.Result;
            }
            // If fallback found, use that MO
            tenantMo = fallbackResult.TenantMarketOperator;
        }

        var mo = tenantMo!.MarketOperator!;
        var areaCode = tenantMo.AreaCode;

        // Determine the date to fetch
        var targetDate = request.Date ?? DetermineTargetDate(mo.Code);

        // Get country name from area code for tariff lookup
        var countryName = _countryMappingService.GetCountryNameFromAreaCode(areaCode);

        // Fetch prices from the Market Operator
        var result = await _tsoService.GetDayAheadPricesAsync(
            mo.Code,
            mo.BaseUrl,
            mo.ApiKey,
            countryName,
            targetDate,
            request.Period ?? tenantMo.SamplingPeriodMinutes,
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
        var (distributionFee, transmissionFee, taxRate, currency, usingDefaultTariff) =
            await GetTariffForCountryAsync(tenantId, countryName, cancellationToken);

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
            TenantId = tenantId,
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

    private async Task<(LiveEnergyPricesDto Result, Domain.Entities.TenantMarketOperator? TenantMarketOperator)> GetFallbackMarketOperatorAsync(int tenantId, CancellationToken cancellationToken)
    {
        // Try to get tenant's firm to determine country
        var firm = await _context.Firms
            .Where(f => f.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken);

        if (firm?.Country == null)
        {
            return (new LiveEnergyPricesDto
            {
                Success = false,
                ErrorMessage = "No Market Operator configured for this tenant and no firm country found for fallback"
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

        // Create a virtual TenantMarketOperator for the fallback
        var virtualTmo = new Domain.Entities.TenantMarketOperator
        {
            TenantId = tenantId,
            MarketOperatorId = mo.Id,
            MarketOperator = mo,
            AreaCode = areaCode,
            SamplingPeriodMinutes = mo.SupportedResolutions.FirstOrDefault(r => r == 60) != 0
                ? 60
                : mo.SupportedResolutions.FirstOrDefault(),
            IsActive = true
        };

        return (new LiveEnergyPricesDto { Success = true }, virtualTmo);
    }

    private async Task<(decimal distributionFee, decimal transmissionFee, decimal taxRate, string currency, bool usingDefault)> GetTariffForCountryAsync(
        int tenantId,
        string? countryName,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(countryName))
        {
            return (TariffDefaults.DistributionFee, TariffDefaults.TransmissionFee, TariffDefaults.TaxRate, "EUR", true);
        }

        // Try to find tariff by country name (case-insensitive match)
        var tariff = await _context.CountryTariffs
            .Where(t => t.TenantId == tenantId && t.IsActive)
            .Where(t => t.CountryName.ToLower() == countryName.ToLower())
            .FirstOrDefaultAsync(cancellationToken);

        if (tariff != null)
        {
            return (tariff.DistributionFee, tariff.TransmissionFee, tariff.TaxRate, tariff.Currency, false);
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
