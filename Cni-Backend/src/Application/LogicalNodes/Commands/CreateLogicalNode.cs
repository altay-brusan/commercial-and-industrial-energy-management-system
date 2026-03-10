using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Entities;

namespace Backend.Application.LogicalNodes.Commands;

[Authorize(Roles = "Administrator")]
[RequirePermission(Backend.Domain.Constants.Permissions.DeviceCreate)]
public record CreateLogicalNodeCommand : IRequest<Guid>
{
    public string Name { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
    public string? Description { get; init; }
    public string Parameters { get; init; } = "[]";
}

public class CreateLogicalNodeCommandHandler : IRequestHandler<CreateLogicalNodeCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateLogicalNodeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateLogicalNodeCommand request, CancellationToken cancellationToken)
    {
        var entity = new LogicalNode
        {
            Name = request.Name,
            DisplayName = request.DisplayName,
            Description = request.Description,
            Parameters = request.Parameters
        };

        _context.LogicalNodes.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
