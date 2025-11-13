using System.Security.Claims;
using Backend.Application.Common.Security;
using Backend.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Backend.Application.Users.Commands.RemoveUserClaim;
[Authorize(Roles = "Administrator,Tenant.Admin")]
public record RemoveUserClaimCommand : IRequest<bool>
{
    public string UserId { get; set; } = default!;
    public string ClaimType { get; set; } = default!;
    public string ClaimValue { get; set; } = default!;
}

public class RemoveUserClaimCommandHandler : IRequestHandler<RemoveUserClaimCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public RemoveUserClaimCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<bool> Handle(RemoveUserClaimCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null) throw new NotFoundException(request.UserId,"User not found");

        var claim = new Claim(request.ClaimType, request.ClaimValue);
        var result = await _userManager.RemoveClaimAsync(user, claim);
        return result.Succeeded;
    }
}
