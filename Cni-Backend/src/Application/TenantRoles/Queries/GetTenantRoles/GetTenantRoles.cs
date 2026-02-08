using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.TenantRoles.Queries.GetTenantRoles;

[Authorize(Roles = "Administrator,TenantAdmin")]
public record GetTenantRolesQuery : IRequest<List<TenantRoleBriefDto>>;

public class GetTenantRolesQueryHandler : IRequestHandler<GetTenantRolesQuery, List<TenantRoleBriefDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetTenantRolesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<TenantRoleBriefDto>> Handle(GetTenantRolesQuery request, CancellationToken cancellationToken)
    {
        return await _context.TenantRoles
            .OrderBy(tr => tr.Name)
            .ProjectTo<TenantRoleBriefDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
