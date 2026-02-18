using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models;
using Backend.Application.Common.Security;
using Backend.Application.Users.Queries.GetUserInfo;
using Backend.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Backend.Application.Users.Commands.UpdateUser;
[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.UserManagementEdit)]
public record UpdateUserCommand : IRequest<Unit>  // Degisiklik: IRequest<Unit> olmali
{
    public string UserId { get; set; } = default!;
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public List<UpdateUserFirmFacilityDto> UserFirmFacilities { get; set; } = new();
}
public sealed class UpdateUserFirmFacilityDto
{
    public Guid? FirmId { get; set; }
    public Guid? FacilityId { get; set; }
}
public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Unit>  // Degisiklik: IRequestHandler<UpdateUserCommand, Unit>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserCommandHandler(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            Guard.Against.NotFound(request.UserId, user);

            // Kullanici bilgilerini g�ncelle
            if (!string.IsNullOrEmpty(request.Email))
            {
                user.Email = request.Email;
                user.UserName = request.Email;
            }

            if (!string.IsNullOrEmpty(request.Name))
                user.Name = request.Name;

            if (!string.IsNullOrEmpty(request.Surname))
                user.Surname = request.Surname;
            if (!string.IsNullOrEmpty(request.Title))
                user.Title = request.Title;
            if (!string.IsNullOrEmpty(request.Description))
                user.Description = request.Description;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                throw new Exception(string.Join(", ", updateResult.Errors.Select(e => e.Description)));
            }

            // Kullanicinin firma/tesis yetkilerini g�ncelle
            var existingUserFirmFacilities = await _unitOfWork.DbContext.UserFirmFacilities
                .Where(uf => uf.UserId == user.Id)
                .ToListAsync(cancellationToken);

            _unitOfWork.DbContext.UserFirmFacilities.RemoveRange(existingUserFirmFacilities);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var newUserFirmFacilities = request.UserFirmFacilities
                .Select(f => new UserFirmFacility
                {
                    TenantId = user.TenantId ?? Guid.Empty,
                    UserId = user.Id,
                    FirmId = f.FirmId,
                    FacilityId = f.FacilityId
                })
                .ToList();

            await _unitOfWork.DbContext.UserFirmFacilities.AddRangeAsync(newUserFirmFacilities, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Eger her sey basariliysa, transaction commit edilir
            await transaction.CommitAsync(cancellationToken);
            return Unit.Value;  // Burada Unit.Value d�nd�r�l�yor
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new Exception($"User update failed: {ex.Message}");
        }
    }
}
