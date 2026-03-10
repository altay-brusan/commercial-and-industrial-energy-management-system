using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.Firms.Queries.GetUserFirmFacilities;

[Authorize]
public record GetUserFirmFacilitiesQuery : IRequest<List<UserFirmFacilityDto>>
{
    public Guid FirmId { get; init; }
}

public class GetUserFirmFacilitiesQueryHandler : IRequestHandler<GetUserFirmFacilitiesQuery, List<UserFirmFacilityDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IMapper _mapper;

    public GetUserFirmFacilitiesQueryHandler(IApplicationDbContext context, IUser user, IMapper mapper)
    {
        _context = context;
        _user = user;
        _mapper = mapper;
    }

    public async Task<List<UserFirmFacilityDto>> Handle(GetUserFirmFacilitiesQuery request, CancellationToken cancellationToken)
    {
        var userId = _user.Id;

        var facilityIds = await _context.UserFirmFacilities
            .Where(uff => uff.UserId == userId && uff.FirmId == request.FirmId && uff.FacilityId != null)
            .Select(uff => uff.FacilityId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        return await _context.Facilities
            .Where(f => facilityIds.Contains(f.Id))
            .OrderBy(f => f.Name)
            .ProjectTo<UserFirmFacilityDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
