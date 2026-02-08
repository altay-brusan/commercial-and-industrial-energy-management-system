using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Constants;
using Backend.Domain.Entities;

namespace Backend.Application.Users.Queries.GetUserInfo;

[Authorize]
public sealed class UserInfoQuery : IRequest<UserInfoDto?>
{
}
public sealed class UserInfoQueryHandler : IRequestHandler<UserInfoQuery, UserInfoDto?>
{
    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IPermissionService _permissionService;
    private readonly IMultiTenantService _multiTenantService;
    public UserInfoQueryHandler(IIdentityService identityService, IApplicationDbContext context, IMapper mapper, IPermissionService permissionService, IMultiTenantService multiTenantService)
    {
        _identityService = identityService;
        _context = context;
        _mapper = mapper;
        _permissionService = permissionService;
        _multiTenantService = multiTenantService;
    }

    public async Task<UserInfoDto?> Handle(UserInfoQuery request, CancellationToken cancellationToken)
    {
        var appUserInfo = await _identityService.GetUserInfoAsync(cancellationToken);
        if (appUserInfo == null)
            return null;

        var userTenants = new List<UserInfoTenantDto>();

        // Kullanıcının rollerini al
        var userRoles = appUserInfo.Roles ?? new List<string>();
        bool isAdmin = userRoles.Contains("Administrator");
        bool isTenantAdmin = userRoles.Contains("Tenant.Admin");

        List<Tenant> tenants = await _context.Tenants.ToListAsync(cancellationToken);

        var tenantIds = tenants.Select(t => t.Id).ToList();

        // Kullanıcıya atanmış firma ve tesisleri çekiyoruz
        var assignedFirmFacilities = appUserInfo.UserFirmFacilities ?? new List<UserFirmFacilityUserDto>();

        var assignedFirmIds = assignedFirmFacilities
            .Where(x => x.FirmId != null)
            .Select(x => x.FirmId)
            .Distinct()
            .ToList();

        var assignedFacilityIds = assignedFirmFacilities
            .Where(x => x.FacilityId != null)
            .Select(x => x.FacilityId)
            .Distinct()
            .ToList();

        List<Firm> firms = new();
        List<Facility> facilities = new();

        if (isAdmin || isTenantAdmin)
        {
            // Admin ve Tenant.Admin kullanıcıları tüm firmaları ve tesisleri alır
            firms = await _context.Firms
                .Where(f => tenantIds.Contains(f.TenantId))
                .ToListAsync(cancellationToken);

            facilities = await _context.Facilities
                .Where(f => firms.Select(firm => firm.Id).Contains(f.FirmId))
                .ToListAsync(cancellationToken);
        }
        else
        {
            // Normal kullanıcı ise
            if (assignedFirmIds.Any())
            {
                // Kullanıcıya atanmış firmaları al
                firms = await _context.Firms
                    .Where(f => assignedFirmIds.Contains(f.Id))
                    .ToListAsync(cancellationToken);

                // Eğer kullanıcıya tesis atanmadıysa, firmalara bağlı tüm tesisleri al
                if (!assignedFacilityIds.Any())
                {
                    facilities = await _context.Facilities
                        .Where(f => assignedFirmIds.Contains(f.FirmId))
                        .ToListAsync(cancellationToken);
                }
            }
            else
            {
                // Eğer atanmış firma yoksa, tenant'a bağlı tüm firmaları al
                firms = await _context.Firms
                    .Where(f => tenantIds.Contains(f.TenantId))
                    .ToListAsync(cancellationToken);
            }

            if (assignedFacilityIds.Any())
            {
                // Kullanıcıya atanmış tesisleri al
                facilities = await _context.Facilities
                    .Where(f => assignedFacilityIds.Contains(f.Id))
                    .ToListAsync(cancellationToken);
            }
        }

        // Verileri DTO'ya dönüştürme
        foreach (var tenant in tenants)
        {
            var tenantFirms = firms
                .Where(f => f.TenantId == tenant.Id)
                .Select(firm => new UserInfoTenantDto
                {
                    Id = firm.Id,
                    Name = firm.Name,
                    Items = facilities
                        .Where(facility => facility.FirmId == firm.Id)
                        .Select(facility => new UserInfoTenantDto
                        {
                            Id = facility.Id,
                            Name = facility.Name
                        }).ToList()
                })
                .Where(firm => isAdmin || isTenantAdmin || firm.Items!.Any()) // Eğer Tenant.Admin değilse ve firmaya bağlı tesis yoksa firmayı da göstermiyoruz
                .ToList();

            userTenants.Add(new UserInfoTenantDto
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Items = isAdmin || isTenantAdmin ? tenantFirms : (tenantFirms.Any() ? tenantFirms : null) // Eğer firma ve tesis yoksa null dönecek
            });
        }

        // Populate permissions and plan features
        List<string>? permissions = null;
        List<string>? planFeatures = null;

        var effectiveTenantId = _multiTenantService.GetEffectiveTenantId()
            ?? _multiTenantService.GetAuthenticatedTenantId();

        if (effectiveTenantId != null)
        {
            planFeatures = await _permissionService.GetTenantFeaturesAsync(effectiveTenantId.Value, cancellationToken);

            if (isAdmin)
            {
                // Administrators get all permissions
                permissions = await _context.Permissions
                    .Select(p => p.Key)
                    .ToListAsync(cancellationToken);
            }
            else if (isTenantAdmin)
            {
                // TenantAdmins get all permissions within their plan features
                permissions = await _context.Permissions
                    .Where(p => planFeatures.Contains(p.Feature.Key))
                    .Select(p => p.Key)
                    .ToListAsync(cancellationToken);
            }
            else
            {
                permissions = await _permissionService.GetUserPermissionsAsync(
                    appUserInfo.UserId, effectiveTenantId.Value, cancellationToken);
            }
        }

        return new UserInfoDto
        {
            Email = appUserInfo.Email,
            Name = appUserInfo.Name,
            Surname = appUserInfo.Surname,
            Tenants = userTenants,
            Title = appUserInfo.Title,
            Roles = appUserInfo.Roles,
            Permissions = permissions,
            PlanFeatures = planFeatures
        };
    }
}
