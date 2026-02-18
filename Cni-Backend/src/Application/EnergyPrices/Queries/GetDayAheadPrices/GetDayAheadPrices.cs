using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.EnergyPrices.Queries.GetDayAheadPrices;

/// <summary>
/// Gets Day-Ahead energy prices for a facility within a date range.
/// Uses the facility's country (via Firm's MarketOperator configuration) to fetch historical prices.
/// </summary>
[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.FinancialsView)]
public record GetDayAheadPricesQuery : IRequest<DayAheadEnergyPricesDto?>
{
    /// <summary>
    /// The facility to get prices for. The country is determined from the facility's firm configuration.
    /// </summary>
    public Guid FacilityId { get; set; }

    /// <summary>
    /// Start date (inclusive). If EndDate is null, only this day's prices are returned.
    /// </summary>
    public DateTimeOffset StartDate { get; set; }

    /// <summary>
    /// End date (inclusive). If null, only StartDate's data is returned.
    /// </summary>
    public DateTimeOffset? EndDate { get; set; }
}

public class GetDayAheadPricesQueryHandler : IRequestHandler<GetDayAheadPricesQuery, DayAheadEnergyPricesDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ICountryMappingService _countryMappingService;

    public GetDayAheadPricesQueryHandler(
        IApplicationDbContext context,
        ICountryMappingService countryMappingService)
    {
        _context = context;
        _countryMappingService = countryMappingService;
    }

    public async Task<DayAheadEnergyPricesDto?> Handle(GetDayAheadPricesQuery request, CancellationToken cancellationToken)
    {
        // Get facility with its firm relationship
        var facility = await _context.Facilities
            .Include(f => f.Firm)
            .FirstOrDefaultAsync(f => f.Id == request.FacilityId, cancellationToken);

        if (facility?.Firm == null)
        {
            return null;
        }

        // Get firm's Market Operator configuration
        var firmMo = await _context.FirmMarketOperators
            .Include(x => x.MarketOperator)
            .Where(x => x.FirmId == facility.FirmId && x.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        if (firmMo?.MarketOperator == null)
        {
            return null;
        }

        var mo = firmMo.MarketOperator;

        // Convert firm's AreaCode to Country name for querying history
        var countryName = _countryMappingService.GetCountryNameFromAreaCode(firmMo.AreaCode);
        if (string.IsNullOrEmpty(countryName))
        {
            return null;
        }

        // Determine the UTC date range
        var startDate = request.StartDate.ToUniversalTime();
        var endDate = request.EndDate?.ToUniversalTime().AddDays(1).AddTicks(-1)
                      ?? startDate.AddDays(1).AddTicks(-1);

        // Get facility's active tariff for gross price calculation
        // TODO: each facility could have special discount rates and contracts,
        // so we need to consider that in the price calculation
        var facilityTariff = await _context.FacilityTariffs
            .Include(ft => ft.Tariff)
            .Where(ft => ft.FacilityId == request.FacilityId && ft.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        decimal discountRate = facilityTariff?.DiscountRate ?? 0m;
        decimal distributionFee = facilityTariff?.Tariff?.DistributionFee ?? 0m;
        decimal transmissionFee = facilityTariff?.Tariff?.TransmissionFee ?? 0m;
        decimal taxRate = facilityTariff?.Tariff?.TaxRate ?? 0m;

        // Query Day-Ahead prices and calculate gross at query time
        var rawPrices = await _context.DayAheadNetEnergyPriceHistories
            .Where(x => x.MarketOperatorId == mo.Id
                && x.Country == countryName
                && x.TimestampUtc >= startDate
                && x.TimestampUtc <= endDate)
            .OrderBy(x => x.TimestampUtc)
            .Select(x => new { x.TimestampUtc, x.NetPrice })
            .ToListAsync(cancellationToken);

        if (rawPrices.Count == 0)
        {
            return null;
        }

        // Calculate gross price: (NetPrice + DistributionFee + TransmissionFee) × (1 + TaxRate)
        var prices = rawPrices.Select(x =>
        {
            var grossPrice = (x.NetPrice + distributionFee + transmissionFee) * (1 + taxRate);
            return new DayAheadEnergyPricePointDto
            {
                TimestampUtc = x.TimestampUtc,
                NetPrice = x.NetPrice,
                GrossPrice = grossPrice,
                DiscountedGrossPrice = discountRate > 0 ? grossPrice * (1 - discountRate) : null
            };
        }).ToList();

        return new DayAheadEnergyPricesDto
        {
            FacilityId = request.FacilityId,
            FacilityName = facility.Name,
            Country = countryName,
            MarketOperatorId = mo.Id,
            MarketOperatorCode = mo.Code,
            StartDate = startDate,
            EndDate = request.EndDate ?? request.StartDate,
            DataPointCount = prices.Count,
            DiscountRate = discountRate,
            DistributionFee = distributionFee,
            TransmissionFee = transmissionFee,
            TaxRate = taxRate,
            Currency = facilityTariff?.Tariff?.Currency ?? "EUR",
            Prices = prices
        };
    }
}
