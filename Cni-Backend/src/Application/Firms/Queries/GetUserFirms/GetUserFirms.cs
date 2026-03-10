using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.Firms.Queries.GetUserFirms;

[Authorize]
public record GetUserFirmsQuery : IRequest<List<UserFirmDto>>;

public class GetUserFirmsQueryHandler : IRequestHandler<GetUserFirmsQuery, List<UserFirmDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IMapper _mapper;

    public GetUserFirmsQueryHandler(IApplicationDbContext context, IUser user, IMapper mapper)
    {
        _context = context;
        _user = user;
        _mapper = mapper;
    }

    public async Task<List<UserFirmDto>> Handle(GetUserFirmsQuery request, CancellationToken cancellationToken)
    {
        var userId = _user.Id;

        var firmIds = await _context.UserFirmFacilities
            .Where(uff => uff.UserId == userId && uff.FirmId != null)
            .Select(uff => uff.FirmId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        return await _context.Firms
            .Where(f => firmIds.Contains(f.Id))
            .OrderBy(f => f.Name)
            .ProjectTo<UserFirmDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
