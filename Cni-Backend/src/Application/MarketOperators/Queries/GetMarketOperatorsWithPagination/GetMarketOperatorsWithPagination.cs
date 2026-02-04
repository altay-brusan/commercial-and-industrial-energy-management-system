using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Mappings;
using Backend.Application.Common.Models;
using Backend.Application.Common.Security;

namespace Backend.Application.MarketOperators.Queries.GetMarketOperatorsWithPagination;

[Authorize(Roles = "Administrator,Tenant.Admin")]
public record GetMarketOperatorsWithPaginationQuery : IRequest<PaginatedList<MarketOperatorBriefDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public bool? IsActive { get; set; }
}

public class GetMarketOperatorsWithPaginationQueryHandler : IRequestHandler<GetMarketOperatorsWithPaginationQuery, PaginatedList<MarketOperatorBriefDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetMarketOperatorsWithPaginationQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<MarketOperatorBriefDto>> Handle(GetMarketOperatorsWithPaginationQuery request, CancellationToken cancellationToken)
    {
        var query = _context.MarketOperators.AsQueryable();

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        return await query
            .OrderBy(x => x.Name)
            .ProjectTo<MarketOperatorBriefDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
