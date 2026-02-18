using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.FacilityTariffs.Queries.GetActiveTariffForFacility;

/// <summary>
/// Gets the active tariff assignment for a specific facility.
/// Returns the most recently created active assignment.
/// </summary>
[Authorize(Roles = "Administrator,Tenant.Admin")]
public record GetActiveTariffForFacilityQuery : IRequest<FacilityTariffDto?>
{
    public Guid FacilityId { get; init; }
}

public class GetActiveTariffForFacilityQueryHandler : IRequestHandler<GetActiveTariffForFacilityQuery, FacilityTariffDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetActiveTariffForFacilityQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<FacilityTariffDto?> Handle(GetActiveTariffForFacilityQuery request, CancellationToken cancellationToken)
    {
        var facilityTariff = await _context.FacilityTariffs
            .Include(ft => ft.Facility)
            .Include(ft => ft.Tariff)
            .Where(ft => ft.FacilityId == request.FacilityId && ft.IsActive)
            .OrderByDescending(ft => ft.Created)
            .ProjectTo<FacilityTariffDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        return facilityTariff;
    }
}
