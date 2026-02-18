using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Entities;

namespace Backend.Application.TenantRoles.Commands.AssignPermissionsToRole;

[Authorize(Roles = "Administrator,TenantAdmin")]
public record AssignPermissionsToRoleCommand : IRequest
{
    public Guid TenantRoleId { get; init; }
    public List<Guid> PermissionIds { get; init; } = new();
}

public class AssignPermissionsToRoleCommandHandler : IRequestHandler<AssignPermissionsToRoleCommand>
{
    private readonly IApplicationDbContext _context;

    public AssignPermissionsToRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(AssignPermissionsToRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _context.TenantRoles
            .FirstOrDefaultAsync(tr => tr.Id == request.TenantRoleId, cancellationToken);

        Guard.Against.NotFound(request.TenantRoleId, role);

        // Remove existing permissions
        var existing = await _context.TenantRolePermissions
            .Where(trp => trp.TenantRoleId == request.TenantRoleId)
            .ToListAsync(cancellationToken);

        _context.TenantRolePermissions.RemoveRange(existing);

        // Add new permissions
        foreach (var permissionId in request.PermissionIds.Distinct())
        {
            _context.TenantRolePermissions.Add(new TenantRolePermission
            {
                TenantRoleId = request.TenantRoleId,
                PermissionId = permissionId
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
