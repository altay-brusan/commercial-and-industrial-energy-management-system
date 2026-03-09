using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Entities;

namespace Backend.Application.Alarms.Queries.GetAlarm;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.AlarmView)]
public record GetAlarmQuery : IRequest<AlarmDto>
{
    public Guid Id { get; init; }
}

public class GetAlarmQueryHandler : IRequestHandler<GetAlarmQuery, AlarmDto>
{
    private readonly IApplicationDbContext _context;

    public GetAlarmQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AlarmDto> Handle(GetAlarmQuery request, CancellationToken cancellationToken)
    {
        var alarm = await _context.DeviceAlarms
            .Include(a => a.AlarmType)
            .Include(a => a.Device)
                .ThenInclude(d => d!.Rtu)
                    .ThenInclude(r => r!.Facility)
            .Include(a => a.AcknowledgedBy)
            .Include(a => a.ResolvedBy)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (alarm == null)
            throw new NotFoundException(nameof(DeviceAlarm), request.Id.ToString());

        return AlarmDtoMapper.ToDto(alarm);
    }
}
