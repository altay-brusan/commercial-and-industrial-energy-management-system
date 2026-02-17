using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Entities;

namespace Backend.Application.TenantRoles.Commands.CreateTenantRole;

[Authorize(Roles = "Administrator,TenantAdmin")]
public record CreateTenantRoleCommand : IRequest<int>
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}

public class CreateTenantRoleCommandHandler : IRequestHandler<CreateTenantRoleCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IMultiTenantService _multiTenantService;

    public CreateTenantRoleCommandHandler(IApplicationDbContext context, IMultiTenantService multiTenantService)
    {
        _context = context;
        _multiTenantService = multiTenantService;
    }

    public async Task<int> Handle(CreateTenantRoleCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _multiTenantService.GetTenantIdForCreation();

        var entity = new TenantRole
        {
            TenantId = tenantId,
            Name = request.Name,
            Description = request.Description,
            IsActive = true
        };

        _context.TenantRoles.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
