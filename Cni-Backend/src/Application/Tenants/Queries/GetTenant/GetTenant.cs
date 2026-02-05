using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.Tenants.Queries.GetTenant;
[Authorize]
public record GetTenantQuery : IRequest<TenantDto?>
{
    public int Id { get; set; }
}

public class GetTenantQueryHandler : IRequestHandler<GetTenantQuery, TenantDto?>
{
    private readonly IApplicationDbContext _context; 
    private readonly IMapper _mapper;

    public GetTenantQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<TenantDto?> Handle(GetTenantQuery request, CancellationToken cancellationToken)
    {
        return await _context.Tenants.Where(x => x.Id == request.Id)
            .ProjectTo<TenantDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken); 
    }
}
