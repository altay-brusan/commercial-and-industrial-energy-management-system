using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.FirmMarketOperators.Queries.GetFirmMarketOperator;

[Authorize(Roles = "Administrator,Tenant.Admin")]
public record GetFirmMarketOperatorQuery : IRequest<FirmMarketOperatorDto?>
{
    public int FirmId { get; set; }
}

public class GetFirmMarketOperatorQueryHandler : IRequestHandler<GetFirmMarketOperatorQuery, FirmMarketOperatorDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetFirmMarketOperatorQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<FirmMarketOperatorDto?> Handle(GetFirmMarketOperatorQuery request, CancellationToken cancellationToken)
    {
        if (request.FirmId <= 0)
        {
            return null;
        }

        var entity = await _context.FirmMarketOperators
            .Include(x => x.MarketOperator)
            .Where(x => x.FirmId == request.FirmId && x.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity == null)
        {
            return null;
        }

        return _mapper.Map<FirmMarketOperatorDto>(entity);
    }
}
