using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Application.Users.Queries.GetUserInfo;
using Backend.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Backend.Application.Users.Queries.GetUser;
[Authorize(Roles = "Administrator,Tenant.Admin")]
public record GetUserQuery : IRequest<UserDto?>
{
    public required string Id { get; set; }
}

public class GetUserQueryHandler : IRequestHandler<GetUserQuery, UserDto?>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IMultiTenantService _multiTenantService;

    public GetUserQueryHandler(UserManager<ApplicationUser> userManager, IApplicationDbContext dbContext, IMapper mapper, IMultiTenantService multiTenantService)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _mapper = mapper;
        _multiTenantService = multiTenantService;
    }

    public async Task<UserDto?> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var effectiveTenantId = _multiTenantService.GetEffectiveTenantId();

        // Find the user
        var entity = await _userManager.Users
            .Where(x => x.Id == request.Id)
            .Where(x => effectiveTenantId == null || x.TenantId == effectiveTenantId)
            .FirstOrDefaultAsync(cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        // Get user roles
        var roles = await _userManager.GetRolesAsync(entity);

        // Get UserFirmFacilities info
        var userFirmFacilities = await _dbContext.UserFirmFacilities
            .Where(uf => uf.UserId == entity.Id)
            .Include(uf => uf.Firm)
            .Include(uf => uf.Facility)
            .ToListAsync(cancellationToken);

        // Map to UserDto and add roles
        var userDto = _mapper.Map<UserDto>(entity);
        userDto.Roles = roles.ToList();

        // Add UserFirmFacilities info
        userDto.UserFirmFacilities = _mapper.Map<List<UserFirmFacilityDto>>(userFirmFacilities);

        return userDto;

    }
}
