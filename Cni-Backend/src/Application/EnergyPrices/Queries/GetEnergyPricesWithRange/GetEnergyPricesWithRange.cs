using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.EnergyPrices.Queries.GetEnergyPricesWithRange;

/// <summary>
/// Gets energy prices for a date range from the tenant's configured Market Operator.
/// Returns a time series of prices with timestamps.
/// </summary>
[Authorize(Roles = "Administrator,Tenant.Admin")]
public record GetEnergyPricesWithRangeQuery : IRequest<EnergyPriceRangeDto?>
{
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
}

public class GetEnergyPricesWithRangeQueryValidator : AbstractValidator<GetEnergyPricesWithRangeQuery>
{
    public GetEnergyPricesWithRangeQueryValidator()
    {
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
    private readonly int? _tenantId;

    public GetEnergyPricesWithRangeQueryHandler(IApplicationDbContext context, IMultiTenantService multiTenantService)
    {
        _context = context;
        _tenantId = multiTenantService.GetCurrentTenantId();
    }

    public async Task<EnergyPriceRangeDto?> Handle(GetEnergyPricesWithRangeQuery request, CancellationToken cancellationToken)
    {
        if (_tenantId is null)
        {
            return null;
        }

        // Get tenant's Market Operator configuration
        var tenantMo = await _context.TenantMarketOperators
            .Include(x => x.MarketOperator)
            .Where(x => x.TenantId == _tenantId && x.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        if (tenantMo?.MarketOperator == null)
        {
            return null;
        }

        var mo = tenantMo.MarketOperator;

        var prices = await _context.EnergyPriceHistories
            .Where(x => x.MarketOperatorId == mo.Id
                && x.AreaCode == tenantMo.AreaCode
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
            AreaCode = tenantMo.AreaCode,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Prices = prices
        };
    }
}
