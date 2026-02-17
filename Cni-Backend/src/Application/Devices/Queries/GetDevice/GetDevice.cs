using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.Devices.Queries.GetDevice;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.DeviceView)]
public record GetDeviceQuery : IRequest<DeviceDto?>
{
    public int Id { get; set; }
}

public class GetDeviceQueryHandler : IRequestHandler<GetDeviceQuery, DeviceDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetDeviceQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<DeviceDto?> Handle(GetDeviceQuery request, CancellationToken cancellationToken)
    {
        return await _context.Devices.Where(x => x.Id == request.Id)
            .ProjectTo<DeviceDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
