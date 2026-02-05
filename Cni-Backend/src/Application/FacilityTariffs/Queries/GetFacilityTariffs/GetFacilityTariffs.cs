using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Mappings;
using Backend.Application.Common.Models;
using Backend.Application.Common.Security;

namespace Backend.Application.FacilityTariffs.Queries.GetFacilityTariffs;

/// <summary>
/// Gets a paginated list of facility-tariff assignments.
/// Can filter by facility, tariff, or active status.
/// </summary>
[Authorize(Roles = "Administrator")]
public record GetFacilityTariffsQuery : IRequest<PaginatedList<FacilityTariffBriefDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;

    /// <summary>
    /// Filter by facility ID (optional).
    /// </summary>
    public int? FacilityId { get; init; }

    /// <summary>
    /// Filter by tariff ID (optional).
    /// </summary>
    public int? TariffId { get; init; }

    /// <summary>
    /// Filter by active status. If null, returns all assignments.
    /// </summary>
    public bool? IsActive { get; init; }
}

public class GetFacilityTariffsQueryHandler : IRequestHandler<GetFacilityTariffsQuery, PaginatedList<FacilityTariffBriefDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetFacilityTariffsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<FacilityTariffBriefDto>> Handle(GetFacilityTariffsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.FacilityTariffs
            .Include(ft => ft.Facility)
            .Include(ft => ft.Tariff)
            .AsQueryable();

        // Apply filters
        if (request.FacilityId.HasValue)
        {
            query = query.Where(ft => ft.FacilityId == request.FacilityId.Value);
        }

        if (request.TariffId.HasValue)
        {
            query = query.Where(ft => ft.TariffId == request.TariffId.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(ft => ft.IsActive == request.IsActive.Value);
        }

        // Order by facility name, then by created date descending
        query = query.OrderBy(ft => ft.Facility!.Name).ThenByDescending(ft => ft.Created);

        return await query
            .ProjectTo<FacilityTariffBriefDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
