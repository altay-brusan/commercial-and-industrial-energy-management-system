using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Backend.Application.Common.Security;
using Backend.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Backend.Application.Users.Commands.AddUserClaim;
[Authorize(Roles = "Administrator,Tenant.Admin")]
public record AddUserClaimCommand : IRequest<bool>
{
    public string UserId { get; set; } = default!;
    public string ClaimType { get; set; } = default!;
    public string ClaimValue { get; set; } = default!;
}

public class AddUserClaimCommandHandler : IRequestHandler<AddUserClaimCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AddUserClaimCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<bool> Handle(AddUserClaimCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null) throw new NotFoundException(request.UserId,"User not found");


        //EXEC:tenant.admin ve diğer userlar için bu kısmın sıkıntı sadece kendi tenantına ekleyebilmeli

        var claim = new Claim(request.ClaimType, request.ClaimValue);
        var result = await _userManager.AddClaimAsync(user, claim);
        return result.Succeeded;
    }
}
