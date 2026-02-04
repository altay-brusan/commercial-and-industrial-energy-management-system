using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.AdminTenantContext.Queries.GetTenantContext;

/// <summary>
/// Returns the current tenant context information for debugging/admin UI purposes.
/// </summary>
[Authorize(Roles = "Administrator")]
public record GetTenantContextQuery : IRequest<TenantContextDto>;

public class GetTenantContextQueryHandler : IRequestHandler<GetTenantContextQuery, TenantContextDto>
{
    private readonly IMultiTenantService _multiTenantService;

    public GetTenantContextQueryHandler(IMultiTenantService multiTenantService)
    {
        _multiTenantService = multiTenantService;
    }

    public Task<TenantContextDto> Handle(GetTenantContextQuery request, CancellationToken cancellationToken)
    {
        var result = new TenantContextDto
        {
            AuthenticatedTenantId = _multiTenantService.GetAuthenticatedTenantId(),
            SelectedTenantId = _multiTenantService.GetSelectedTenantId(),
            EffectiveTenantId = _multiTenantService.GetEffectiveTenantId(),
            IsSuperAdmin = _multiTenantService.IsSuperAdmin(),
            IsTenantScoped = _multiTenantService.IsTenantScoped(),
            IsImpersonating = _multiTenantService.IsImpersonating()
        };

        return Task.FromResult(result);
    }
}
