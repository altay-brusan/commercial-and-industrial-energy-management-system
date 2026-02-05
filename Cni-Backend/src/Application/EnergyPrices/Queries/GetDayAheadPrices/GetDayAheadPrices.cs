using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.EnergyPrices.Queries.GetDayAheadPrices;

/// <summary>
/// Gets Day-Ahead energy prices for a facility within a date range.
/// Uses the facility's country (via Firm's MarketOperator configuration) to fetch historical prices.
/// </summary>
[Authorize(Roles = "Administrator,Tenant.Admin")]
public record GetDayAheadPricesQuery : IRequest<DayAheadPricesDto?>
{
    /// <summary>
    /// The facility to get prices for. The country is determined from the facility's firm configuration.
    /// </summary>
    public int FacilityId { get; set; }

    /// <summary>
    /// Start date (inclusive). If EndDate is null, only this day's prices are returned.
    /// </summary>
    public DateTimeOffset StartDate { get; set; }

    /// <summary>
    /// End date (inclusive). If null, only StartDate's data is returned.
    /// </summary>
    public DateTimeOffset? EndDate { get; set; }
}

public class GetDayAheadPricesQueryHandler : IRequestHandler<GetDayAheadPricesQuery, DayAheadPricesDto?>
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

    public async Task<DayAheadPricesDto?> Handle(GetDayAheadPricesQuery request, CancellationToken cancellationToken)
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

        // Determine the date range
        var startDate = request.StartDate.Date;
        var endDate = request.EndDate?.Date.AddDays(1).AddTicks(-1) ?? startDate.AddDays(1).AddTicks(-1);

        // Query Day-Ahead prices
        var prices = await _context.DayAheadEnergyPriceHistories
            .Where(x => x.MarketOperatorId == mo.Id
                && x.Country == countryName
                && x.PriceLocalTimestamp >= startDate
                && x.PriceLocalTimestamp <= endDate)
            .OrderBy(x => x.PriceLocalTimestamp)
            .Select(x => new DayAheadPricePointDto
            {
                Timestamp = x.PriceLocalTimestamp,
                NetPrice = x.NetPrice,
                GrossPrice = x.GrossPrice
            })
            .ToListAsync(cancellationToken);

        if (prices.Count == 0)
        {
            return null;
        }

        // Get facility's active tariff for discount information
        var facilityTariff = await _context.FacilityTariffs
            .Include(ft => ft.Tariff)
            .Where(ft => ft.FacilityId == request.FacilityId && ft.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        decimal discountRate = facilityTariff?.DiscountRate ?? 0m;
        decimal distributionFee = facilityTariff?.Tariff?.DistributionFee ?? 0m;
        decimal transmissionFee = facilityTariff?.Tariff?.TransmissionFee ?? 0m;
        decimal taxRate = facilityTariff?.Tariff?.TaxRate ?? 0m;

        // Apply facility discount if any
        if (discountRate > 0)
        {
            foreach (var price in prices)
            {
                price.DiscountedGrossPrice = price.GrossPrice * (1 - discountRate);
            }
        }

        return new DayAheadPricesDto
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
