using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.EnergyPrices.Queries.GetEnergyPricesWithRange;

/// <summary>
/// Gets energy prices for a date range from a specific TSO.
/// Returns a time series of prices with timestamps.
/// </summary>
[Authorize(Roles = "Administrator,Tenant.Admin")]
public record GetEnergyPricesWithRangeQuery : IRequest<EnergyPriceRangeDto?>
{
    public int TsoId { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
}

public class GetEnergyPricesWithRangeQueryValidator : AbstractValidator<GetEnergyPricesWithRangeQuery>
{
    public GetEnergyPricesWithRangeQueryValidator()
    {
        RuleFor(v => v.TsoId)
            .GreaterThan(0);

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

    public GetEnergyPricesWithRangeQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EnergyPriceRangeDto?> Handle(GetEnergyPricesWithRangeQuery request, CancellationToken cancellationToken)
    {
        var tso = await _context.TransmissionSystemOperators
            .FindAsync(new object[] { request.TsoId }, cancellationToken);

        if (tso == null)
        {
            return null;
        }

        var prices = await _context.EnergyPriceHistories
            .Where(x => x.TsoId == request.TsoId
                && x.PriceTimestamp >= request.StartDate
                && x.PriceTimestamp <= request.EndDate)
            .OrderBy(x => x.PriceTimestamp)
            .Select(x => new EnergyPricePointDto
            {
                Timestamp = x.PriceTimestamp,
                RawPrice = x.RawPrice,
                TotalPrice = x.TotalPrice
            })
            .ToListAsync(cancellationToken);

        return new EnergyPriceRangeDto
        {
            TsoId = tso.Id,
            TsoName = tso.Name,
            TsoCode = tso.Code,
            Currency = tso.Currency,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Prices = prices
        };
    }
}
