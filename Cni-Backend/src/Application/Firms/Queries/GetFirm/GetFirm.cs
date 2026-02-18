using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.Firms.Queries.GetFirm;
[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.FirmView)]
public record GetFirmQuery : IRequest<FirmDto?>
{
    public Guid Id { get; set; }
}

public class GetFirmQueryHandler : IRequestHandler<GetFirmQuery, FirmDto?>
{
    private readonly IApplicationDbContext _context; 
    private readonly IMapper _mapper;

    public GetFirmQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<FirmDto?> Handle(GetFirmQuery request, CancellationToken cancellationToken)
    {
        return await _context.Firms.Where(x => x.Id == request.Id)
            .ProjectTo<FirmDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken); 
    }
}
