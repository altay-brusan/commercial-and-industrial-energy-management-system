using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.TenantMarketOperators.Queries.GetTenantMarketOperator;

[Authorize(Roles = "Administrator,Tenant.Admin")]
public record GetTenantMarketOperatorQuery : IRequest<TenantMarketOperatorDto?>
{
    public int TenantId { get; set; }
}

public class GetTenantMarketOperatorQueryHandler : IRequestHandler<GetTenantMarketOperatorQuery, TenantMarketOperatorDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IMultiTenantService _multiTenantService;

    public GetTenantMarketOperatorQueryHandler(IApplicationDbContext context, IMapper mapper, IMultiTenantService multiTenantService)
    {
        _context = context;
        _mapper = mapper;
        _multiTenantService = multiTenantService;
    }

    public async Task<TenantMarketOperatorDto?> Handle(GetTenantMarketOperatorQuery request, CancellationToken cancellationToken)
    {
        // For this query, use the request TenantId if provided and user is admin,
        // otherwise use effective tenant from context
        int? tenantId;
        if (_multiTenantService.IsSuperAdmin() && request.TenantId > 0)
        {
            tenantId = request.TenantId;
        }
        else
        {
            tenantId = _multiTenantService.GetEffectiveTenantId();
        }

        if (tenantId is null)
        {
            return null;
        }

        var entity = await _context.TenantMarketOperators
            .Include(x => x.MarketOperator)
            .Where(x => x.TenantId == tenantId && x.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity == null)
        {
            return null;
        }

        return _mapper.Map<TenantMarketOperatorDto>(entity);
    }
}
