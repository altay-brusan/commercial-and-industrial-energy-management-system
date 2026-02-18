using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.FacilityTariffs.Queries.GetFacilityTariff;

/// <summary>
/// Gets a single facility-tariff assignment by ID.
/// </summary>
[Authorize(Roles = "Administrator")]
public record GetFacilityTariffQuery : IRequest<FacilityTariffDto?>
{
    public Guid Id { get; init; }
}

public class GetFacilityTariffQueryHandler : IRequestHandler<GetFacilityTariffQuery, FacilityTariffDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetFacilityTariffQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<FacilityTariffDto?> Handle(GetFacilityTariffQuery request, CancellationToken cancellationToken)
    {
        var facilityTariff = await _context.FacilityTariffs
            .Include(ft => ft.Facility)
            .Include(ft => ft.Tariff)
            .Where(ft => ft.Id == request.Id)
            .ProjectTo<FacilityTariffDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        return facilityTariff;
    }
}
