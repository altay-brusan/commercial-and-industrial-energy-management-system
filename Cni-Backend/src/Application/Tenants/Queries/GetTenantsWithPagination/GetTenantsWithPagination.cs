using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Mappings;
using Backend.Application.Common.Models;
using Backend.Application.Common.Security;

namespace Backend.Application.Tenants.Queries.GetTenantsWithPagination;
[Authorize]
public record GetTenantsWithPaginationQuery : IRequest<PaginatedList<TenantBriefDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class GetTenantsWithPaginationQueryHandler : IRequestHandler<GetTenantsWithPaginationQuery, PaginatedList<TenantBriefDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetTenantsWithPaginationQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<TenantBriefDto>> Handle(GetTenantsWithPaginationQuery request, CancellationToken cancellationToken)
    {
        return await _context.Tenants
            .OrderBy(x => x.Name)
            .ProjectTo<TenantBriefDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
