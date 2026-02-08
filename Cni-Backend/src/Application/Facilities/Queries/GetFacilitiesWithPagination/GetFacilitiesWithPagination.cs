using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Mappings;
using Backend.Application.Common.Models;
using Backend.Application.Common.Security;

namespace Backend.Application.Facilities.Queries.GetFacilitiesWithPagination;
[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.FacilityView)]
public record GetFacilitiesWithPaginationQuery : IRequest<PaginatedList<FacilityBriefDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? FirmIds { get; set; }
}

public class GetFacilitiesWithPaginationQueryHandler : IRequestHandler<GetFacilitiesWithPaginationQuery, PaginatedList<FacilityBriefDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetFacilitiesWithPaginationQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<FacilityBriefDto>> Handle(GetFacilitiesWithPaginationQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Facilities.AsQueryable();

        List<int>? FirmIds = null;

        if (!string.IsNullOrEmpty(request.FirmIds))
        {
            FirmIds = request.FirmIds.Split(',').Select(int.Parse).ToList();
        }
        if (FirmIds is { Count: > 0 }) // Null degilse ve içinde eleman varsa filtre uygula
        {
            query = query.Where(x => FirmIds.Contains(x.FirmId));
        }

        return await query
            .OrderBy(x => x.Name)
            .ProjectTo<FacilityBriefDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
