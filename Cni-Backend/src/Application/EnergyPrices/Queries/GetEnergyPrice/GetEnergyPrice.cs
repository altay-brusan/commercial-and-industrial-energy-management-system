using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.EnergyPrices.Queries.GetEnergyPrice;

/// <summary>
/// Gets energy price for a specific date-time.
/// If the exact timestamp doesn't exist, returns the nearest available timestamp.
/// </summary>
[Authorize(Roles = "Administrator,Tenant.Admin")]
public record GetEnergyPriceQuery : IRequest<EnergyPriceDto?>
{
    public int TsoId { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}

public class GetEnergyPriceQueryValidator : AbstractValidator<GetEnergyPriceQuery>
{
    public GetEnergyPriceQueryValidator()
    {
        RuleFor(v => v.TsoId)
            .GreaterThan(0);

        RuleFor(v => v.Timestamp)
            .NotEmpty();
    }
}

public class GetEnergyPriceQueryHandler : IRequestHandler<GetEnergyPriceQuery, EnergyPriceDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetEnergyPriceQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<EnergyPriceDto?> Handle(GetEnergyPriceQuery request, CancellationToken cancellationToken)
    {
        // First try to find exact match
        var exactMatch = await _context.EnergyPriceHistories
            .Include(x => x.Tso)
            .Where(x => x.TsoId == request.TsoId && x.PriceTimestamp == request.Timestamp)
            .FirstOrDefaultAsync(cancellationToken);

        if (exactMatch != null)
        {
            return _mapper.Map<EnergyPriceDto>(exactMatch);
        }

        // If no exact match, find nearest timestamp (round to nearest)
        // Get the closest price before and after the requested timestamp
        var priceBefore = await _context.EnergyPriceHistories
            .Include(x => x.Tso)
            .Where(x => x.TsoId == request.TsoId && x.PriceTimestamp <= request.Timestamp)
            .OrderByDescending(x => x.PriceTimestamp)
            .FirstOrDefaultAsync(cancellationToken);

        var priceAfter = await _context.EnergyPriceHistories
            .Include(x => x.Tso)
            .Where(x => x.TsoId == request.TsoId && x.PriceTimestamp > request.Timestamp)
            .OrderBy(x => x.PriceTimestamp)
            .FirstOrDefaultAsync(cancellationToken);

        // Return the nearest one
        if (priceBefore == null && priceAfter == null)
        {
            return null;
        }

        if (priceBefore == null)
        {
            return _mapper.Map<EnergyPriceDto>(priceAfter);
        }

        if (priceAfter == null)
        {
            return _mapper.Map<EnergyPriceDto>(priceBefore);
        }

        // Both exist, return the nearest
        var diffBefore = request.Timestamp - priceBefore.PriceTimestamp;
        var diffAfter = priceAfter.PriceTimestamp - request.Timestamp;

        var nearest = diffBefore <= diffAfter ? priceBefore : priceAfter;
        return _mapper.Map<EnergyPriceDto>(nearest);
    }
}
