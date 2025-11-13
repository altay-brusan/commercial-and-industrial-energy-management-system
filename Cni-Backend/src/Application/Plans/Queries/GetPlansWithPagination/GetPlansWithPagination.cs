using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Mappings;
using Backend.Application.Common.Models;
using Backend.Application.Common.Security;

namespace Backend.Application.Plans.Queries.GetPlansWithPagination;
[Authorize(Roles = "Administrator")]
public record GetPlansWithPaginationQuery : IRequest<PaginatedList<PlanBriefDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class GetPlansWithPaginationQueryHandler : IRequestHandler<GetPlansWithPaginationQuery, PaginatedList<PlanBriefDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetPlansWithPaginationQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<PlanBriefDto>> Handle(GetPlansWithPaginationQuery request, CancellationToken cancellationToken)
    {
        return await _context.Plans
            .OrderBy(x => x.Name)
            .ProjectTo<PlanBriefDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
