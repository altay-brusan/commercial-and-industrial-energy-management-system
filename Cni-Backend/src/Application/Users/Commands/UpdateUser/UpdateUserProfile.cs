using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Backend.Application.Users.Commands.UpdateUser;
internal class UpdateUserProfile
{
}

[Authorize(Roles = "Administrator,Tenant.Admin")]
public record UpdateUserProfileCommand : IRequest<Unit>  // Değişiklik: IRequest<Unit> olmalı
{
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public string? Description { get; set; }
    
}

public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, Unit>  // Değişiklik: IRequestHandler<UpdateUserCommand, Unit>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IIdentityService _identityService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserProfileCommandHandler(IIdentityService identityService ,UserManager<ApplicationUser> userManager, IUser user, IUnitOfWork unitOfWork)
    {

        _identityService = identityService;
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var user = await _identityService.GetCurrentUserAsync(cancellationToken);
            Guard.Against.Null(user, nameof(user));

            if (!string.IsNullOrEmpty(request.Email))
            {
                user.Email = request.Email;
                user.UserName = request.Email;
            }

            if (!string.IsNullOrEmpty(request.Name))
                user.Name = request.Name;

            if (!string.IsNullOrEmpty(request.Surname))
                user.Surname = request.Surname;
            if (!string.IsNullOrEmpty(request.Description))
                user.Description = request.Description;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                throw new Exception(string.Join(", ", updateResult.Errors.Select(e => e.Description)));
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            return Unit.Value;  // Burada Unit.Value döndürülüyor
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new Exception($"User update failed: {ex.Message}");
        }
    }
}
