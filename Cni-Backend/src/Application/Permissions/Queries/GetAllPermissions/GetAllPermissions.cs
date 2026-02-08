using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.Permissions.Queries.GetAllPermissions;

[Authorize]
public record GetAllPermissionsQuery : IRequest<List<FeaturePermissionsDto>>;

public class GetAllPermissionsQueryHandler : IRequestHandler<GetAllPermissionsQuery, List<FeaturePermissionsDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAllPermissionsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<FeaturePermissionsDto>> Handle(GetAllPermissionsQuery request, CancellationToken cancellationToken)
    {
        var permissions = await _context.Permissions
            .Include(p => p.Feature)
            .Where(p => p.Feature.IsActive)
            .OrderBy(p => p.Feature.Key)
            .ThenBy(p => p.Key)
            .ProjectTo<PermissionDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return permissions
            .GroupBy(p => new { p.FeatureKey })
            .Select(g => new FeaturePermissionsDto
            {
                FeatureKey = g.Key.FeatureKey,
                FeatureName = g.First().FeatureKey,
                Permissions = g.ToList()
            })
            .ToList();
    }
}
