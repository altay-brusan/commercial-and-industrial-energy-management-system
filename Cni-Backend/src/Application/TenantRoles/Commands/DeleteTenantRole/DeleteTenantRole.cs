using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.TenantRoles.Commands.DeleteTenantRole;

[Authorize(Roles = "Administrator,TenantAdmin")]
public record DeleteTenantRoleCommand(int Id) : IRequest;

public class DeleteTenantRoleCommandHandler : IRequestHandler<DeleteTenantRoleCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteTenantRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteTenantRoleCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.TenantRoles
            .FirstOrDefaultAsync(tr => tr.Id == request.Id, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        _context.TenantRoles.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
