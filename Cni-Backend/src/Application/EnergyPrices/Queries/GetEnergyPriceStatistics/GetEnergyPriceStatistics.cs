using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.EnergyPrices.Queries.GetEnergyPriceStatistics;

/// <summary>
/// Gets statistical values (min, max, average) for energy prices within a date range.
/// Uses the tenant's configured Market Operator.
/// </summary>
[Authorize(Roles = "Administrator,Tenant.Admin")]
public record GetEnergyPriceStatisticsQuery : IRequest<EnergyPriceStatisticsDto?>
{
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
}

public class GetEnergyPriceStatisticsQueryValidator : AbstractValidator<GetEnergyPriceStatisticsQuery>
{
    public GetEnergyPriceStatisticsQueryValidator()
    {
        RuleFor(v => v.StartDate)
            .NotEmpty();

        RuleFor(v => v.EndDate)
            .NotEmpty()
            .GreaterThan(v => v.StartDate)
            .WithMessage("End date must be after start date.");
    }
}

public class GetEnergyPriceStatisticsQueryHandler : IRequestHandler<GetEnergyPriceStatisticsQuery, EnergyPriceStatisticsDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMultiTenantService _multiTenantService;

    public GetEnergyPriceStatisticsQueryHandler(IApplicationDbContext context, IMultiTenantService multiTenantService)
    {
        _context = context;
        _multiTenantService = multiTenantService;
    }

    public async Task<EnergyPriceStatisticsDto?> Handle(GetEnergyPriceStatisticsQuery request, CancellationToken cancellationToken)
    {
        if (!_multiTenantService.IsTenantScoped())
        {
            return null;
        }

        var tenantId = _multiTenantService.GetEffectiveTenantId()!.Value;

        // Get tenant's Market Operator configuration
        var tenantMo = await _context.TenantMarketOperators
            .Include(x => x.MarketOperator)
            .Where(x => x.TenantId == tenantId && x.IsActive)
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
            .ToListAsync(cancellationToken);

        if (prices.Count == 0)
        {
            return new EnergyPriceStatisticsDto
            {
                MarketOperatorId = mo.Id,
                MarketOperatorName = mo.Name,
                MarketOperatorCode = mo.Code,
                AreaCode = tenantMo.AreaCode,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                DataPointCount = 0
            };
        }

        var minNetPriceItem = prices.MinBy(x => x.NetPrice);
        var maxNetPriceItem = prices.MaxBy(x => x.NetPrice);
        var minGrossPriceItem = prices.MinBy(x => x.GrossPrice);
        var maxGrossPriceItem = prices.MaxBy(x => x.GrossPrice);

        return new EnergyPriceStatisticsDto
        {
            MarketOperatorId = mo.Id,
            MarketOperatorName = mo.Name,
            MarketOperatorCode = mo.Code,
            AreaCode = tenantMo.AreaCode,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            DataPointCount = prices.Count,

            MinNetPrice = prices.Min(x => x.NetPrice),
            MaxNetPrice = prices.Max(x => x.NetPrice),
            AvgNetPrice = prices.Average(x => x.NetPrice),

            MinGrossPrice = prices.Min(x => x.GrossPrice),
            MaxGrossPrice = prices.Max(x => x.GrossPrice),
            AvgGrossPrice = prices.Average(x => x.GrossPrice),

            MinNetPriceTimestamp = minNetPriceItem?.PriceLocalTimestamp,
            MaxNetPriceTimestamp = maxNetPriceItem?.PriceLocalTimestamp,
            MinGrossPriceTimestamp = minGrossPriceItem?.PriceLocalTimestamp,
            MaxGrossPriceTimestamp = maxGrossPriceItem?.PriceLocalTimestamp
        };
    }
}
