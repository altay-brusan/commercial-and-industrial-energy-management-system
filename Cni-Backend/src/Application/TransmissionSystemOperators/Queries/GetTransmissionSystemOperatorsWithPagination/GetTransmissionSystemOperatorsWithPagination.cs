using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Mappings;
using Backend.Application.Common.Models;
using Backend.Application.Common.Security;

namespace Backend.Application.TransmissionSystemOperators.Queries.GetTransmissionSystemOperatorsWithPagination;

[Authorize(Roles = "Administrator,Tenant.Admin")]
public record GetTransmissionSystemOperatorsWithPaginationQuery : IRequest<PaginatedList<TransmissionSystemOperatorBriefDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public bool? IsActive { get; set; }
}

public class GetTransmissionSystemOperatorsWithPaginationQueryHandler : IRequestHandler<GetTransmissionSystemOperatorsWithPaginationQuery, PaginatedList<TransmissionSystemOperatorBriefDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetTransmissionSystemOperatorsWithPaginationQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<TransmissionSystemOperatorBriefDto>> Handle(GetTransmissionSystemOperatorsWithPaginationQuery request, CancellationToken cancellationToken)
    {
        var query = _context.TransmissionSystemOperators.AsQueryable();

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        return await query
            .OrderBy(x => x.Name)
            .ProjectTo<TransmissionSystemOperatorBriefDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
