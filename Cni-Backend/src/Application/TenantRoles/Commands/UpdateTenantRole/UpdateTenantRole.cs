using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.TenantRoles.Commands.UpdateTenantRole;

[Authorize(Roles = "Administrator,TenantAdmin")]
public record UpdateTenantRoleCommand : IRequest
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
}

public class UpdateTenantRoleCommandHandler : IRequestHandler<UpdateTenantRoleCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateTenantRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateTenantRoleCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.TenantRoles
            .FirstOrDefaultAsync(tr => tr.Id == request.Id, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
