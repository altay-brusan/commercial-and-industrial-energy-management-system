using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Constants;

namespace Backend.Application.Devices.Queries.GetDeviceStatus;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.DeviceView)]
public record GetDeviceStatusQuery : IRequest<DeviceStatusDto>
{
    public Guid DeviceId { get; init; }
}

public class GetDeviceStatusQueryHandler : IRequestHandler<GetDeviceStatusQuery, DeviceStatusDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IDeviceStatusService _statusService;

    public GetDeviceStatusQueryHandler(IApplicationDbContext context, IDeviceStatusService statusService)
    {
        _context = context;
        _statusService = statusService;
    }

    public async Task<DeviceStatusDto> Handle(GetDeviceStatusQuery request, CancellationToken cancellationToken)
    {
        var exists = await _context.Devices.AnyAsync(d => d.Id == request.DeviceId, cancellationToken);
        Guard.Against.NotFound(request.DeviceId, exists ? request.DeviceId : (Guid?)null);

        var status = await _statusService.GetStatusAsync(request.DeviceId, cancellationToken);
        return new DeviceStatusDto { DeviceId = request.DeviceId, Status = status };
    }
}

public record DeviceStatusDto
{
    public Guid DeviceId { get; init; }
    public string Status { get; init; } = "normal";
}
