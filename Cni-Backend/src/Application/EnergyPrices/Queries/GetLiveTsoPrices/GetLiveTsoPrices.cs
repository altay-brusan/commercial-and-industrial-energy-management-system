using Backend.Application.Common;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.EnergyPrices.Queries.GetLiveTsoPrices;

/// <summary>
/// Fetches live/current day-ahead prices directly from a TSO API.
/// This does NOT store the data - it's for real-time display.
/// </summary>
[Authorize(Roles = "Administrator,Tenant.Admin")]
public record GetLiveTsoPricesQuery : IRequest<LiveTsoPricesDto>
{
    /// <summary>
    /// The TSO ID to fetch prices from
    /// </summary>
    public int TsoId { get; set; }

    /// <summary>
    /// The date to fetch prices for. If not specified, uses today or tomorrow based on TSO rules.
    /// </summary>
    public DateTimeOffset? Date { get; set; }

    /// <summary>
    /// The country to fetch prices for (e.g., "Netherlands", "Germany", "Türkiye").
    /// If not specified, uses the TSO's default configuration.
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// Requested resolution in minutes (15, 30, or 60). Defaults to 60 (hourly).
    /// If the source data has higher resolution, it will be aggregated.
    /// </summary>
    public int? Period { get; set; }
}

public class GetLiveTsoPricesQueryValidator : AbstractValidator<GetLiveTsoPricesQuery>
{
    public GetLiveTsoPricesQueryValidator()
    {
        RuleFor(v => v.TsoId)
            .GreaterThan(0);

        RuleFor(v => v.Period)
            .Must(p => p == null || new[] { 15, 30, 60 }.Contains(p.Value))
            .WithMessage("Period must be 15, 30, or 60 minutes");
    }
}

public class GetLiveTsoPricesQueryHandler : IRequestHandler<GetLiveTsoPricesQuery, LiveTsoPricesDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITsoService _tsoService;

    public GetLiveTsoPricesQueryHandler(IApplicationDbContext context, ITsoService tsoService)
    {
        _context = context;
        _tsoService = tsoService;
    }

    public async Task<LiveTsoPricesDto> Handle(GetLiveTsoPricesQuery request, CancellationToken cancellationToken)
    {
        // Get the TSO configuration
        var tso = await _context.TransmissionSystemOperators
            .FindAsync(new object[] { request.TsoId }, cancellationToken);

        if (tso == null)
        {
            return new LiveTsoPricesDto
            {
                Success = false,
                ErrorMessage = $"TSO with ID {request.TsoId} not found"
            };
        }

        // Determine the date to fetch
        var targetDate = request.Date ?? DetermineTargetDate(tso.Code);

        // Use provided country or null (service will handle appropriately)
        var country = request.Country;

        // Fetch prices from the TSO
        var result = await _tsoService.GetDayAheadPricesAsync(
            tso.Code,
            tso.BaseUrl,
            tso.ApiKey,
            country,
            targetDate,
            request.Period,
            cancellationToken);

        if (!result.Success)
        {
            return new LiveTsoPricesDto
            {
                Success = false,
                ErrorMessage = result.ErrorMessage,
                TsoId = tso.Id,
                TsoName = tso.Name,
                TsoCode = tso.Code,
                Country = country,
                Date = targetDate,
                ResolutionMinutes = result.ResolutionMinutes,
                Timezone = result.Timezone
            };
        }

        // Fetch country tariff from database (or use defaults)
        var (distributionFee, transmissionFee, taxRate, usingDefaultTariff) =
            await GetTariffForCountryAsync(tso.TenantId, country, cancellationToken);

        // Calculate prices with new formula: (NetPrice + DistributionFee + TransmissionFee) × (1 + TaxRate)
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

        return new LiveTsoPricesDto
        {
            Success = true,
            TsoId = tso.Id,
            TsoName = tso.Name,
            TsoCode = tso.Code,
            Country = country,
            Currency = result.Currency,
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

    private async Task<(decimal distributionFee, decimal transmissionFee, decimal taxRate, bool usingDefault)> GetTariffForCountryAsync(
        int tenantId,
        string? countryName,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(countryName))
        {
            return (TariffDefaults.DistributionFee, TariffDefaults.TransmissionFee, TariffDefaults.TaxRate, true);
        }

        // Try to find tariff by country name (case-insensitive match)
        var tariff = await _context.CountryTariffs
            .Where(t => t.TenantId == tenantId && t.IsActive)
            .Where(t => t.CountryName.ToLower() == countryName.ToLower())
            .FirstOrDefaultAsync(cancellationToken);

        if (tariff != null)
        {
            return (tariff.DistributionFee, tariff.TransmissionFee, tariff.TaxRate, false);
        }

        // Fall back to defaults
        return (TariffDefaults.DistributionFee, TariffDefaults.TransmissionFee, TariffDefaults.TaxRate, true);
    }

    private DateTimeOffset DetermineTargetDate(string tsoCode)
    {
        // Different TSOs publish day-ahead prices at different times
        // ENTSO-E: Around 13:00 CET
        // EPIAS: Around 14:00 Turkey time (UTC+3)

        var now = DateTimeOffset.UtcNow;

        return tsoCode.ToUpperInvariant() switch
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
