using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Application.Permissions.Queries.GetAllPermissions;

namespace Backend.Application.Permissions.Queries.GetPermissionsByFeature;

[Authorize]
public record GetPermissionsByFeatureQuery(string FeatureKey) : IRequest<List<PermissionDto>>;

public class GetPermissionsByFeatureQueryHandler : IRequestHandler<GetPermissionsByFeatureQuery, List<PermissionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetPermissionsByFeatureQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<PermissionDto>> Handle(GetPermissionsByFeatureQuery request, CancellationToken cancellationToken)
    {
        return await _context.Permissions
            .Include(p => p.Feature)
            .Where(p => p.Feature.Key == request.FeatureKey && p.Feature.IsActive)
            .OrderBy(p => p.Key)
            .ProjectTo<PermissionDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
