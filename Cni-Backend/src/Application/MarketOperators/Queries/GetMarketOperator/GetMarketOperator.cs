using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.MarketOperators.Queries.GetMarketOperator;

[Authorize(Roles = "Administrator,Tenant.Admin")]
public record GetMarketOperatorQuery : IRequest<MarketOperatorDto?>
{
    public Guid Id { get; set; }
}

public class GetMarketOperatorQueryHandler : IRequestHandler<GetMarketOperatorQuery, MarketOperatorDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetMarketOperatorQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<MarketOperatorDto?> Handle(GetMarketOperatorQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.MarketOperators
            .FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            return null;
        }

        return _mapper.Map<MarketOperatorDto>(entity);
    }
}
