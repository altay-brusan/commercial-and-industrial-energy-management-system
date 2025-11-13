using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Mappings;
using Backend.Application.Common.Models;
using Backend.Application.Common.Security;
using Microsoft.AspNetCore.Identity;

namespace Backend.Application.AppRoles.Queries.GetAppRolesWithPagination;
[Authorize(Roles =  "Administrator,Tenant.Admin")]
public record GetAppRolesWithPaginationQuery : IRequest<PaginatedList<AppRoleBriefDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class GetAppRolesWithPaginationQueryHandler : IRequestHandler<GetAppRolesWithPaginationQuery, PaginatedList<AppRoleBriefDto>>
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IMapper _mapper;
    private readonly int? _tenantId;
    private readonly bool _isSuperAdmin;
    public GetAppRolesWithPaginationQueryHandler(RoleManager<IdentityRole> roleManager, IMapper mapper, IMultiTenantService multiTenantService)
    {
        _roleManager = roleManager;
        _mapper = mapper; 
        _tenantId = multiTenantService.GetCurrentTenantId();
        _isSuperAdmin = multiTenantService.IsSuperAdmin();
    }

    public async Task<PaginatedList<AppRoleBriefDto>> Handle(GetAppRolesWithPaginationQuery request, CancellationToken cancellationToken)
    {
        
        return await _roleManager.Roles.Where(x => _isSuperAdmin || x.Name != "Administrator")
            .OrderBy(x => x.Name)
             .ProjectTo<AppRoleBriefDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
