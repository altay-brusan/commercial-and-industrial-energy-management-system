using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Entities.Topology;
using Backend.Domain.Enums;
using Backend.Domain.Events;

namespace Backend.Application.Tenants.Commands.CreateTenant;

[Authorize(Roles = "Administrator,Tenant.Admin")]
public record CreateTopologyNodeCommand : IRequest<int>
{
    public int? DeviceId { get; init; }
    public int TenantId { get; init; }
    public TopologyNodeTypes Type { get; init; }
    public string? Extent { get; init; }
    public int? ParentId { get; init; }
    public int? TargetHandleCount { get; init; }
    public int? SourceHandleCount { get; init; }
    public string? Data { get; init; }
    public string? Position { get; init; }
    public string? Style { get; init; }
    public int FacilityId { get; init; }
}

public class CreateTopologyNodeCommandValidator : AbstractValidator<CreateTopologyNodeCommand>
{
    public CreateTopologyNodeCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty().WithMessage("TenantId is required.");
        RuleFor(x => x.Type).IsInEnum().WithMessage("Invalid node type.");
        RuleFor(x => x.FacilityId).NotEmpty().WithMessage("FacilityId is required.");
    }
}


public class CreateTopologyNodeCommandHandler : IRequestHandler<CreateTopologyNodeCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateTopologyNodeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateTopologyNodeCommand request, CancellationToken cancellationToken)
    {
        var entity = new TopologyNode
        {
            DeviceId = request.DeviceId,
            TenantId = request.TenantId,
            Type = request.Type,
            Extent = request.Extent,
            ParentId = request.ParentId,
            TargetHandleCount = request.TargetHandleCount,
            SourceHandleCount = request.SourceHandleCount,
            Data = request.Data,
            Position = request.Position,
            Style = request.Style,
            FacilityId = request.FacilityId
        };

        entity.AddDomainEvent(new TopologyNodeCreatedEvent(entity));

        _context.TopologyNodes.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
