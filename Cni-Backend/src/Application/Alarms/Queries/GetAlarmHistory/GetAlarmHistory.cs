using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.Alarms.Queries.GetAlarmHistory;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.AlarmView)]
public record GetAlarmHistoryQuery : IRequest<AlarmHistoryResponseDto>
{
    public string DeviceName { get; init; } = string.Empty;
}

public class GetAlarmHistoryQueryHandler : IRequestHandler<GetAlarmHistoryQuery, AlarmHistoryResponseDto>
{
    private readonly IApplicationDbContext _context;

    public GetAlarmHistoryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AlarmHistoryResponseDto> Handle(GetAlarmHistoryQuery request, CancellationToken cancellationToken)
    {
        var alarms = await _context.DeviceAlarms
            .Include(a => a.AlarmType)
            .Include(a => a.Device)
                .ThenInclude(d => d!.DeviceType)
            .Include(a => a.Device)
                .ThenInclude(d => d!.Rtu)
                    .ThenInclude(r => r!.Facility)
            .Include(a => a.AcknowledgedBy)
            .Include(a => a.ResolvedBy)
            .Where(a => a.Device!.Name == request.DeviceName)
            .OrderByDescending(a => a.OccurredAt)
            .ToListAsync(cancellationToken);

        return new AlarmHistoryResponseDto
        {
            Alarms = alarms.Select(AlarmDtoMapper.ToDto).ToList()
        };
    }
}
