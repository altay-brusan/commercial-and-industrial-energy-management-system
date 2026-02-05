using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.EnergyPrices.Queries.GetEnergyPricesWithRange;

/// <summary>
/// Gets energy prices for a date range from the firm's configured Market Operator.
/// Returns a time series of prices with timestamps.
/// </summary>
[Authorize(Roles = "Administrator,Tenant.Admin")]
public record GetEnergyPricesWithRangeQuery : IRequest<EnergyPriceRangeDto?>
{
    public int FirmId { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
}

public class GetEnergyPricesWithRangeQueryValidator : AbstractValidator<GetEnergyPricesWithRangeQuery>
{
    public GetEnergyPricesWithRangeQueryValidator()
    {
        RuleFor(v => v.FirmId)
            .GreaterThan(0)
            .WithMessage("FirmId is required.");

        RuleFor(v => v.StartDate)
            .NotEmpty();

        RuleFor(v => v.EndDate)
            .NotEmpty()
            .GreaterThan(v => v.StartDate)
            .WithMessage("End date must be after start date.");
    }
}

public class GetEnergyPricesWithRangeQueryHandler : IRequestHandler<GetEnergyPricesWithRangeQuery, EnergyPriceRangeDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ICountryMappingService _countryMappingService;

    public GetEnergyPricesWithRangeQueryHandler(
        IApplicationDbContext context,
        ICountryMappingService countryMappingService)
    {
        _context = context;
        _countryMappingService = countryMappingService;
    }

    public async Task<EnergyPriceRangeDto?> Handle(GetEnergyPricesWithRangeQuery request, CancellationToken cancellationToken)
    {
        // Get firm's Market Operator configuration
        var firmMo = await _context.FirmMarketOperators
            .Include(x => x.MarketOperator)
            .Where(x => x.FirmId == request.FirmId && x.IsActive)
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

        var prices = await _context.EnergyPriceHistories
            .Where(x => x.MarketOperatorId == mo.Id
                && x.Country == countryName
                && x.PriceLocalTimestamp >= request.StartDate
                && x.PriceLocalTimestamp <= request.EndDate)
            .OrderBy(x => x.PriceLocalTimestamp)
            .Select(x => new EnergyPricePointDto
            {
                Timestamp = x.PriceLocalTimestamp,
                NetPrice = x.NetPrice,
                GrossPrice = x.GrossPrice
            })
            .ToListAsync(cancellationToken);

        return new EnergyPriceRangeDto
        {
            MarketOperatorId = mo.Id,
            MarketOperatorName = mo.Name,
            MarketOperatorCode = mo.Code,
            Country = countryName,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Prices = prices
        };
    }
}
