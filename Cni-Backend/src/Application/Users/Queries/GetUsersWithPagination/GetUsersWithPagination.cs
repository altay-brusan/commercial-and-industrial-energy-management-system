using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Mappings;
using Backend.Application.Common.Models;
using Backend.Application.Common.Security;
using Backend.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Backend.Application.Users.Queries.GetUsersWithPagination;
[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.UserManagementView)]
public record GetUsersWithPaginationQuery : IRequest<PaginatedList<UserBriefDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class GetUsersWithPaginationQueryHandler : IRequestHandler<GetUsersWithPaginationQuery, PaginatedList<UserBriefDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;
    private readonly IMultiTenantService _multiTenantService;

    public GetUsersWithPaginationQueryHandler(UserManager<ApplicationUser> userManager, IMapper mapper, IMultiTenantService multiTenantService)
    {
        _userManager = userManager;
        _mapper = mapper;
        _multiTenantService = multiTenantService;
    }

    public async Task<PaginatedList<UserBriefDto>> Handle(GetUsersWithPaginationQuery request, CancellationToken cancellationToken)
    {
        var effectiveTenantId = _multiTenantService.GetEffectiveTenantId();

        return await _userManager.Users
            .Where(x => effectiveTenantId == null || x.TenantId == effectiveTenantId)
            .OrderBy(x => x.Name)
            .ProjectTo<UserBriefDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
