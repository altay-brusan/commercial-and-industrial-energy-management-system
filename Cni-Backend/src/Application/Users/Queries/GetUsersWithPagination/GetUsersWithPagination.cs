using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Mappings;
using Backend.Application.Common.Models;
using Backend.Application.Common.Security;
using Backend.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Backend.Application.Users.Queries.GetUsersWithPagination;
[Authorize(Roles = "Administrator,Tenant.Admin")]
public record GetUsersWithPaginationQuery : IRequest<PaginatedList<UserBriefDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class GetUsersWithPaginationQueryHandler : IRequestHandler<GetUsersWithPaginationQuery, PaginatedList<UserBriefDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;
    private readonly int? _tenantId;
    private readonly bool _isSuperAdmin;
    public GetUsersWithPaginationQueryHandler(UserManager<ApplicationUser> userManager, IMapper mapper, IMultiTenantService multiTenantService)
    {
        _userManager = userManager;
        _mapper = mapper; 
        _tenantId = multiTenantService.GetCurrentTenantId();
        _isSuperAdmin = multiTenantService.IsSuperAdmin();
    }

    public async Task<PaginatedList<UserBriefDto>> Handle(GetUsersWithPaginationQuery request, CancellationToken cancellationToken)
    {
        
        return await _userManager.Users.Where(x => _isSuperAdmin || x.TenantId == _tenantId)
            .OrderBy(x => x.Name)
             .ProjectTo<UserBriefDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
