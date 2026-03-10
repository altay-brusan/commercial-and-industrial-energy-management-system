using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.LogicalNodes.Queries;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.DeviceView)]
public record GetLogicalNodeQuery : IRequest<LogicalNodeDto>
{
    public Guid Id { get; init; }
}

public class GetLogicalNodeQueryHandler : IRequestHandler<GetLogicalNodeQuery, LogicalNodeDto>
{
    private readonly IApplicationDbContext _context;

    public GetLogicalNodeQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LogicalNodeDto> Handle(GetLogicalNodeQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.LogicalNodes
            .FirstOrDefaultAsync(n => n.Id == request.Id, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        return new LogicalNodeDto
        {
            Id = entity.Id,
            Name = entity.Name,
            DisplayName = entity.DisplayName,
            Description = entity.Description,
            Parameters = entity.Parameters
        };
    }
}
