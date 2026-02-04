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
    private readonly int? _tenantId;
    private readonly bool _isSuperAdmin;

    public GetTenantMarketOperatorQueryHandler(IApplicationDbContext context, IMapper mapper, IMultiTenantService multiTenantService)
    {
        _context = context;
        _mapper = mapper;
        _tenantId = multiTenantService.GetCurrentTenantId();
        _isSuperAdmin = multiTenantService.IsSuperAdmin();
    }

    public async Task<TenantMarketOperatorDto?> Handle(GetTenantMarketOperatorQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _isSuperAdmin ? request.TenantId : _tenantId;

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
