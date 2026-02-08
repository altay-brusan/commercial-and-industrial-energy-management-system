using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Mappings;
using Backend.Application.Common.Models;
using Backend.Application.Common.Security;

namespace Backend.Application.Firms.Queries.GetFirmsWithPagination;
[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.FirmView)]
public record GetFirmsWithPaginationQuery : IRequest<PaginatedList<FirmBriefDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class GetFirmsWithPaginationQueryHandler : IRequestHandler<GetFirmsWithPaginationQuery, PaginatedList<FirmBriefDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetFirmsWithPaginationQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<FirmBriefDto>> Handle(GetFirmsWithPaginationQuery request, CancellationToken cancellationToken)
    {
        return await _context.Firms
            .OrderBy(x => x.Name)
            .ProjectTo<FirmBriefDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
