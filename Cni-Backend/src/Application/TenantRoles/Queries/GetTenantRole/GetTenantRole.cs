using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.TenantRoles.Queries.GetTenantRole;

[Authorize(Roles = "Administrator,TenantAdmin")]
public record GetTenantRoleQuery(Guid Id) : IRequest<TenantRoleDto>;

public class GetTenantRoleQueryHandler : IRequestHandler<GetTenantRoleQuery, TenantRoleDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetTenantRoleQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<TenantRoleDto> Handle(GetTenantRoleQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.TenantRoles
            .Include(tr => tr.TenantRolePermissions)
                .ThenInclude(trp => trp.Permission)
                    .ThenInclude(p => p.Feature)
            .FirstOrDefaultAsync(tr => tr.Id == request.Id, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        return _mapper.Map<TenantRoleDto>(entity);
    }
}
