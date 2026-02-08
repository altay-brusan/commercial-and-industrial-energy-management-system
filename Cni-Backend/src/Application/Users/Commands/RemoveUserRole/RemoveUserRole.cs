using Backend.Application.Common.Security;
using Backend.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Backend.Application.Users.Commands.RemoveUserRole;
[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.UserManagementEdit)]
public record RemoveUserRoleCommand : IRequest<bool>
{
    public string UserId { get; set; } = default!;
    public string RoleName { get; set; } = default!;
}

public class RemoveUserRoleCommandHandler : IRequestHandler<RemoveUserRoleCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public RemoveUserRoleCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<bool> Handle(RemoveUserRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null) throw new NotFoundException(request.UserId, "User not found");

        var result = await _userManager.RemoveFromRoleAsync(user, request.RoleName);
        return result.Succeeded;
    }
}
