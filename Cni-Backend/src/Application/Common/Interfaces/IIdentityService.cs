using Backend.Application.Common.Models;
using Backend.Application.Users;
using Backend.Domain.Entities;

namespace Backend.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);

    Task<bool> IsInRoleAsync(string userId, string role);

    Task<bool> AuthorizeAsync(string userId, string policyName);

    Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password, int tenantId);

    Task<Result> DeleteUserAsync(string userId);
    Task<UserDto?> GetUserInfoAsync(CancellationToken cancellationToken);
    Task<ApplicationUser?> GetCurrentUserAsync(CancellationToken cancellationToken);
}
