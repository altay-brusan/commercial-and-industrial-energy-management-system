using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.Tariffs.Queries.GetActiveTariffByCountry;

/// <summary>
/// Gets the active tariff for a specific country.
/// Returns the most recently created active tariff for the country.
/// </summary>
[Authorize(Roles = "Administrator,Tenant.Admin")]
public record GetActiveTariffByCountryQuery : IRequest<TariffDto?>
{
    public string CountryName { get; init; } = string.Empty;
}

public class GetActiveTariffByCountryQueryHandler : IRequestHandler<GetActiveTariffByCountryQuery, TariffDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetActiveTariffByCountryQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<TariffDto?> Handle(GetActiveTariffByCountryQuery request, CancellationToken cancellationToken)
    {
        var tariff = await _context.Tariffs
            .Where(t => t.IsActive && t.CountryName.ToLower() == request.CountryName.ToLower())
            .OrderByDescending(t => t.Created)
            .ProjectTo<TariffDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        return tariff;
    }
}
