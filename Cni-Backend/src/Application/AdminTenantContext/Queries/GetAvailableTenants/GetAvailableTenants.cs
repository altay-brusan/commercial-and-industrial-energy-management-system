using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.AdminTenantContext.Queries.GetAvailableTenants;

/// <summary>
/// Returns a list of all tenants available for admin selection.
/// </summary>
[Authorize(Roles = "Administrator")]
public record GetAvailableTenantsQuery : IRequest<List<TenantSelectorDto>>;

public class GetAvailableTenantsQueryHandler : IRequestHandler<GetAvailableTenantsQuery, List<TenantSelectorDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAvailableTenantsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TenantSelectorDto>> Handle(GetAvailableTenantsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Tenants
            .IgnoreQueryFilters()
            .OrderBy(t => t.Name)
            .Select(t => new TenantSelectorDto
            {
                Id = t.Id,
                Name = t.Name ?? t.Identifier ?? $"Tenant {t.Id}",
                Identifier = t.Identifier
            })
            .ToListAsync(cancellationToken);
    }
}
