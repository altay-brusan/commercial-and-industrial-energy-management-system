using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Constants;

namespace Backend.Application.Alarms.Queries.GetAlarmSummary;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.AlarmView)]
public record GetAlarmSummaryQuery : IRequest<AlarmSummaryDto>
{
    public Guid? FacilityId { get; init; }
}

public class GetAlarmSummaryQueryHandler : IRequestHandler<GetAlarmSummaryQuery, AlarmSummaryDto>
{
    private readonly IApplicationDbContext _context;

    public GetAlarmSummaryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AlarmSummaryDto> Handle(GetAlarmSummaryQuery request, CancellationToken cancellationToken)
    {
        var query = _context.DeviceAlarms
            .Include(a => a.AlarmType)
            .Where(a => a.IsActive);

        if (request.FacilityId.HasValue)
            query = query.Where(a => a.Device!.Rtu!.FacilityId == request.FacilityId.Value);

        var totalActive = await query.CountAsync(cancellationToken);
        var critical = await query.Where(a => a.AlarmType!.Severity == "Critical").CountAsync(cancellationToken);
        var high = await query.Where(a => a.AlarmType!.Severity == "Major" || a.AlarmType!.Severity == "High").CountAsync(cancellationToken);
        var acknowledged = await query.Where(a => a.AcknowledgedAt != null).CountAsync(cancellationToken);

        return new AlarmSummaryDto
        {
            TotalActive = totalActive,
            Critical = critical,
            High = high,
            Acknowledged = acknowledged
        };
    }
}
