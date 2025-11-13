using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Backend.Application.Users.Commands.DeleteUser;
[Authorize(Roles = "Administrator,Tenant.Admin")]
public record DeleteUserCommand(string Id) : IRequest<Unit>;  // Değişiklik: IRequest<Unit> olmalı

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Unit>  // Değişiklik: IRequestHandler<DeleteUserCommand, Unit>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteUserCommandHandler(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        // Kullanıcıyı bul
        using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var user = await _userManager.FindByIdAsync(request.Id);
            Guard.Against.NotFound(request.Id, user);

            // Kullanıcının bağlı olduğu UserFirmFacilities verisini sil
            var userFirmFacilities = _unitOfWork.DbContext.UserFirmFacilities
                .Where(uf => uf.UserId == user.Id)
                .ToList();

            _unitOfWork.DbContext.UserFirmFacilities.RemoveRange(userFirmFacilities);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Kullanıcıyı sil
            var deleteResult = await _userManager.DeleteAsync(user);
            if (!deleteResult.Succeeded)
            {
                throw new Exception(string.Join(", ", deleteResult.Errors.Select(e => e.Description)));
            }

            // Eğer her şey başarılıysa, transaction commit edilir
            await transaction.CommitAsync(cancellationToken);

            // İşlem tamamlandığında "Unit" dönüyoruz
            return Unit.Value;  // Burada "Unit.Value" ile işlem tamamlanır.
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new Exception($"User delete failed: {ex.Message}");
        }
    }
}
