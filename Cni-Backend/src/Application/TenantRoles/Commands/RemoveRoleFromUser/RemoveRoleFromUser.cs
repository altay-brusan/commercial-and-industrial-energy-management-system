using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.TenantRoles.Commands.RemoveRoleFromUser;

[Authorize(Roles = "Administrator,TenantAdmin")]
public record RemoveRoleFromUserCommand : IRequest
{
    public Guid TenantRoleId { get; init; }
    public string UserId { get; init; } = string.Empty;
}

public class RemoveRoleFromUserCommandHandler : IRequestHandler<RemoveRoleFromUserCommand>
{
    private readonly IApplicationDbContext _context;

    public RemoveRoleFromUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(RemoveRoleFromUserCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _context.UserTenantRoles
            .FirstOrDefaultAsync(utr => utr.UserId == request.UserId && utr.TenantRoleId == request.TenantRoleId, cancellationToken);

        if (assignment != null)
        {
            _context.UserTenantRoles.Remove(assignment);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
