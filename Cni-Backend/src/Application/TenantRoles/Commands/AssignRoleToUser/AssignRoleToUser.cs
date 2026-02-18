using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Entities;

namespace Backend.Application.TenantRoles.Commands.AssignRoleToUser;

[Authorize(Roles = "Administrator,TenantAdmin")]
public record AssignRoleToUserCommand : IRequest
{
    public Guid TenantRoleId { get; init; }
    public string UserId { get; init; } = string.Empty;
}

public class AssignRoleToUserCommandHandler : IRequestHandler<AssignRoleToUserCommand>
{
    private readonly IApplicationDbContext _context;

    public AssignRoleToUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(AssignRoleToUserCommand request, CancellationToken cancellationToken)
    {
        var role = await _context.TenantRoles
            .FirstOrDefaultAsync(tr => tr.Id == request.TenantRoleId, cancellationToken);

        Guard.Against.NotFound(request.TenantRoleId, role);

        var alreadyAssigned = await _context.UserTenantRoles
            .AnyAsync(utr => utr.UserId == request.UserId && utr.TenantRoleId == request.TenantRoleId, cancellationToken);

        if (!alreadyAssigned)
        {
            _context.UserTenantRoles.Add(new UserTenantRole
            {
                UserId = request.UserId,
                TenantRoleId = request.TenantRoleId
            });

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
