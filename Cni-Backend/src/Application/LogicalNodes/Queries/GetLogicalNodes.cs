using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.LogicalNodes.Queries;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.DeviceView)]
public record GetLogicalNodesQuery : IRequest<List<LogicalNodeDto>>
{
    public string? NamePrefix { get; init; }
}

public class GetLogicalNodesQueryHandler : IRequestHandler<GetLogicalNodesQuery, List<LogicalNodeDto>>
{
    private readonly IApplicationDbContext _context;

    public GetLogicalNodesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<LogicalNodeDto>> Handle(GetLogicalNodesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.LogicalNodes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.NamePrefix))
            query = query.Where(n => n.Name.StartsWith(request.NamePrefix));

        return await query
            .OrderBy(n => n.Name)
            .Select(n => new LogicalNodeDto
            {
                Id = n.Id,
                Name = n.Name,
                DisplayName = n.DisplayName,
                Description = n.Description,
                Parameters = n.Parameters
            })
            .ToListAsync(cancellationToken);
    }
}
