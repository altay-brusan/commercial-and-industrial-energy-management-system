using System.Security.Claims;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models;
using Backend.Application.Users;
using Backend.Application.Users.Queries.GetUserInfo;
using Backend.Domain.Constants;
using Backend.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserClaimsPrincipalFactory<ApplicationUser> _userClaimsPrincipalFactory;
    private readonly IAuthorizationService _authorizationService;

    private readonly IHttpContextAccessor _httpContextAccessor;
    public IdentityService(
        UserManager<ApplicationUser> userManager,
        IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
        IAuthorizationService authorizationService, IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user?.UserName;
    }

    public async Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password, Guid tenantId)
    {
        var user = new ApplicationUser
        {
            UserName = userName,
            Email = userName,
            TenantId = tenantId
        };

        var result = await _userManager.CreateAsync(user, password);

        return (result.ToApplicationResult(), user.Id);
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        var roles = await _userManager.GetRolesAsync(user!);
        return user != null &&  roles.Any(x => x == role);
        //Aşağıdaki kodda neden rol false döndüğü kontrol edilecek.
        //return user != null && await _userManager.IsInRoleAsync(user, role);
    }

    public async Task<bool> AuthorizeAsync(string userId, string policyName)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return false;
        }

        var principal = await _userClaimsPrincipalFactory.CreateAsync(user);

        var result = await _authorizationService.AuthorizeAsync(principal, policyName);

        return result.Succeeded;
    }

    public async Task<Result> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user != null ? await DeleteUserAsync(user) : Result.Success();
    }

    public async Task<Result> DeleteUserAsync(ApplicationUser user)
    {
        var result = await _userManager.DeleteAsync(user);

        return result.ToApplicationResult();
    }
    public async Task<UserDto?> GetUserInfoAsync(CancellationToken cancellationToken)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user is null)
        {
            return null;
        }

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return null;
        }

        // Kullanıcıyı UserFirmFacilities ile birlikte yükle
        var applicationUser = await _userManager.Users
            .Include(u => u.UserFirmFacilities)
                .ThenInclude(uf => uf.Firm) // Firm bilgilerini dahil et
            .Include(u => u.UserFirmFacilities)
                .ThenInclude(uf => uf.Facility) // Facility bilgilerini dahil et
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (applicationUser is null)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(applicationUser!);
        return new UserDto
        {
            UserId = userId,
            Email = applicationUser.Email!,
            TenantId = applicationUser.TenantId,
            Name = applicationUser.Name,
            Surname = applicationUser.Surname,
            Title = applicationUser.Title,
            Roles = roles.ToList(),
            UserFirmFacilities = applicationUser.UserFirmFacilities
            .Select(uf => new UserFirmFacilityUserDto
            {
                FirmId = uf.Firm?.Id,
                FacilityId = uf.Facility?.Id,
            }).ToList()
        };
    }
    public async Task<ApplicationUser?> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user is null)
        {
            return null;
        }

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return null;
        }

        // Kullanıcıyı UserFirmFacilities ile birlikte yükle
        return await _userManager.Users
             .Include(u => u.UserFirmFacilities)
                 .ThenInclude(uf => uf.Firm) // Firm bilgilerini dahil et
             .Include(u => u.UserFirmFacilities)
                 .ThenInclude(uf => uf.Facility) // Facility bilgilerini dahil et
             .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);


    }
}
