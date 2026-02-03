using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.EnergyPrices.Queries.GetEnergyPriceStatistics;

/// <summary>
/// Gets statistical values (min, max, average) for energy prices within a date range.
/// </summary>
[Authorize(Roles = "Administrator,Tenant.Admin")]
public record GetEnergyPriceStatisticsQuery : IRequest<EnergyPriceStatisticsDto?>
{
    public int TsoId { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
}

public class GetEnergyPriceStatisticsQueryValidator : AbstractValidator<GetEnergyPriceStatisticsQuery>
{
    public GetEnergyPriceStatisticsQueryValidator()
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

public class GetEnergyPriceStatisticsQueryHandler : IRequestHandler<GetEnergyPriceStatisticsQuery, EnergyPriceStatisticsDto?>
{
    private readonly IApplicationDbContext _context;

    public GetEnergyPriceStatisticsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EnergyPriceStatisticsDto?> Handle(GetEnergyPriceStatisticsQuery request, CancellationToken cancellationToken)
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
            .ToListAsync(cancellationToken);

        if (prices.Count == 0)
        {
            return new EnergyPriceStatisticsDto
            {
                TsoId = tso.Id,
                TsoName = tso.Name,
                TsoCode = tso.Code,
                Currency = tso.Currency,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                DataPointCount = 0
            };
        }

        var minRawPriceItem = prices.MinBy(x => x.RawPrice);
        var maxRawPriceItem = prices.MaxBy(x => x.RawPrice);
        var minTotalPriceItem = prices.MinBy(x => x.TotalPrice);
        var maxTotalPriceItem = prices.MaxBy(x => x.TotalPrice);

        return new EnergyPriceStatisticsDto
        {
            TsoId = tso.Id,
            TsoName = tso.Name,
            TsoCode = tso.Code,
            Currency = tso.Currency,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            DataPointCount = prices.Count,

            MinRawPrice = prices.Min(x => x.RawPrice),
            MaxRawPrice = prices.Max(x => x.RawPrice),
            AvgRawPrice = prices.Average(x => x.RawPrice),

            MinTotalPrice = prices.Min(x => x.TotalPrice),
            MaxTotalPrice = prices.Max(x => x.TotalPrice),
            AvgTotalPrice = prices.Average(x => x.TotalPrice),

            MinRawPriceTimestamp = minRawPriceItem?.PriceTimestamp,
            MaxRawPriceTimestamp = maxRawPriceItem?.PriceTimestamp,
            MinTotalPriceTimestamp = minTotalPriceItem?.PriceTimestamp,
            MaxTotalPriceTimestamp = maxTotalPriceItem?.PriceTimestamp
        };
    }
}
