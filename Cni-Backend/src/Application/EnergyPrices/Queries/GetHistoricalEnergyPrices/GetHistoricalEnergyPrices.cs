using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.EnergyPrices.Queries.GetHistoricalEnergyPrices;

/// <summary>
/// Gets historical energy prices from the database by country and date range.
/// This is NOT tenant-scoped - it queries the shared price history.
/// </summary>
[Authorize(Roles = "Administrator,Tenant.Admin")]
public record GetHistoricalEnergyPricesQuery : IRequest<HistoricalEnergyPricesDto?>
{
    /// <summary>
    /// The country name to query prices for (e.g., "Netherlands", "Germany", "Turkey")
    /// </summary>
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// Start date of the range (inclusive)
    /// </summary>
    public DateTimeOffset StartDate { get; set; }

    /// <summary>
    /// End date of the range (inclusive). If not provided, only StartDate's data is returned.
    /// </summary>
    public DateTimeOffset? EndDate { get; set; }
}

public class GetHistoricalEnergyPricesQueryValidator : AbstractValidator<GetHistoricalEnergyPricesQuery>
{
    public GetHistoricalEnergyPricesQueryValidator()
    {
        RuleFor(v => v.Country)
            .NotEmpty()
            .WithMessage("Country is required.");

        RuleFor(v => v.StartDate)
            .NotEmpty()
            .WithMessage("Start date is required.");

        RuleFor(v => v.EndDate)
            .GreaterThanOrEqualTo(v => v.StartDate)
            .When(v => v.EndDate.HasValue)
            .WithMessage("End date must be greater than or equal to start date.");
    }
}

public class GetHistoricalEnergyPricesQueryHandler : IRequestHandler<GetHistoricalEnergyPricesQuery, HistoricalEnergyPricesDto?>
{
    private readonly IApplicationDbContext _context;

    public GetHistoricalEnergyPricesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HistoricalEnergyPricesDto?> Handle(GetHistoricalEnergyPricesQuery request, CancellationToken cancellationToken)
    {
        // Determine the date range
        var startDate = request.StartDate.Date;
        var endDate = request.EndDate?.Date.AddDays(1).AddTicks(-1) ?? startDate.AddDays(1).AddTicks(-1);

        // Query prices by country (case-insensitive)
        var query = _context.EnergyPriceHistories
            .Include(x => x.MarketOperator)
            .Where(x => x.Country.ToLower() == request.Country.ToLower())
            .Where(x => x.PriceLocalTimestamp >= startDate && x.PriceLocalTimestamp <= endDate)
            .OrderBy(x => x.PriceLocalTimestamp);

        var prices = await query
            .Select(x => new HistoricalPricePointDto
            {
                Timestamp = x.PriceLocalTimestamp,
                NetPrice = x.NetPrice,
                GrossPrice = x.GrossPrice,
                MarketOperatorId = x.MarketOperatorId,
                MarketOperatorCode = x.MarketOperator != null ? x.MarketOperator.Code : string.Empty
            })
            .ToListAsync(cancellationToken);

        // Return null if no data found
        if (prices.Count == 0)
        {
            return null;
        }

        return new HistoricalEnergyPricesDto
        {
            Country = request.Country,
            StartDate = startDate,
            EndDate = request.EndDate ?? request.StartDate,
            DataPointCount = prices.Count,
            Prices = prices
        };
    }
}
