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

public class GetUserQueryValidator : AbstractValidator<GetUserQuery>
{
    public GetUserQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("UserId is required.");
    }
}

public class GetUserQueryHandler : IRequestHandler<GetUserQuery, UserDto?>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly int? _tenantId;
    private readonly bool _isSuperAdmin;

    public GetUserQueryHandler(UserManager<ApplicationUser> userManager, IApplicationDbContext dbContext, IMapper mapper, IMultiTenantService multiTenantService)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _mapper = mapper; 
        _tenantId = multiTenantService.GetCurrentTenantId();
        _isSuperAdmin = multiTenantService.IsSuperAdmin();
    }

    public async Task<UserDto?> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        // Kullanıcıyı bul
        var entity = await _userManager.Users.Where(x => x.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        // Kullanıcının rollerini al
        var roles = await _userManager.GetRolesAsync(entity);

        // Kullanıcının UserFirmFacilities bilgilerini al
        var userFirmFacilities = await _dbContext.UserFirmFacilities
            .Where(uf => uf.UserId == entity.Id)
            .Include(uf => uf.Firm) // Firm bilgilerini dahil et
            .Include(uf => uf.Facility) // Facility bilgilerini dahil et
            .ToListAsync(cancellationToken);

        // UserDto'yu map et ve rollerini ekle
        var userDto = _mapper.Map<UserDto>(entity);
        userDto.Roles = roles.ToList();

        // UserFirmFacilities bilgilerini DTO'ya dönüştür ve ekle
        userDto.UserFirmFacilities = _mapper.Map<List<UserFirmFacilityDto>>(userFirmFacilities);

        return userDto;

    }
}
