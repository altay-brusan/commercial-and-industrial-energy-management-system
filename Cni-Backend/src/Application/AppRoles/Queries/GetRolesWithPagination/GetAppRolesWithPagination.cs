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
    private readonly IMultiTenantService _multiTenantService;

    public GetAppRolesWithPaginationQueryHandler(RoleManager<IdentityRole> roleManager, IMapper mapper, IMultiTenantService multiTenantService)
    {
        _roleManager = roleManager;
        _mapper = mapper;
        _multiTenantService = multiTenantService;
    }

    public async Task<PaginatedList<AppRoleBriefDto>> Handle(GetAppRolesWithPaginationQuery request, CancellationToken cancellationToken)
    {
        var isSuperAdmin = _multiTenantService.IsSuperAdmin();

        return await _roleManager.Roles.Where(x => isSuperAdmin || x.Name != "Administrator")
            .OrderBy(x => x.Name)
             .ProjectTo<AppRoleBriefDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
