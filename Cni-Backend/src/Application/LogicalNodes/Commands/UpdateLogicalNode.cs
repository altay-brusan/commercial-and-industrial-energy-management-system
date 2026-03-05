using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.LogicalNodes.Commands;

[Authorize(Roles = "Administrator")]
[RequirePermission(Backend.Domain.Constants.Permissions.DeviceCreate)]
public record UpdateLogicalNodeCommand : IRequest
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
    public string? Description { get; init; }
    public string Parameters { get; init; } = "[]";
}

public class UpdateLogicalNodeCommandHandler : IRequestHandler<UpdateLogicalNodeCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateLogicalNodeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateLogicalNodeCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.LogicalNodes
            .FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        entity.Name = request.Name;
        entity.DisplayName = request.DisplayName;
        entity.Description = request.Description;
        entity.Parameters = request.Parameters;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
