using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.Alarms.Queries.GetAlarmStatistics;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.AlarmView)]
public record GetAlarmStatisticsQuery : IRequest<AlarmStatisticsDto>
{
    public Guid? FacilityId { get; init; }
}

public class GetAlarmStatisticsQueryHandler : IRequestHandler<GetAlarmStatisticsQuery, AlarmStatisticsDto>
{
    private readonly IApplicationDbContext _context;

    public GetAlarmStatisticsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AlarmStatisticsDto> Handle(GetAlarmStatisticsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.DeviceAlarms
            .Include(a => a.AlarmType)
            .AsQueryable();

        if (request.FacilityId.HasValue)
            query = query.Where(a => a.Device!.Rtu!.FacilityId == request.FacilityId.Value);

        var alarms = await query.Select(a => new
        {
            a.IsActive,
            a.AcknowledgedAt,
            Severity = a.AlarmType!.Severity,
            Category = a.AlarmType.Category
        }).ToListAsync(cancellationToken);

        var bySourceType = alarms
            .Where(a => !string.IsNullOrEmpty(a.Category))
            .GroupBy(a => a.Category!.ToLower())
            .ToDictionary(g => g.Key, g => g.Count());

        return new AlarmStatisticsDto
        {
            TotalAlarms = alarms.Count,
            ActiveAlarms = alarms.Count(a => a.IsActive && a.AcknowledgedAt == null),
            CriticalAlarms = alarms.Count(a => string.Equals(a.Severity, "Critical", StringComparison.OrdinalIgnoreCase)),
            HighAlarms = alarms.Count(a => string.Equals(a.Severity, "High", StringComparison.OrdinalIgnoreCase)),
            MediumAlarms = alarms.Count(a => string.Equals(a.Severity, "Warning", StringComparison.OrdinalIgnoreCase)
                                           || string.Equals(a.Severity, "Medium", StringComparison.OrdinalIgnoreCase)),
            LowAlarms = alarms.Count(a => string.Equals(a.Severity, "Low", StringComparison.OrdinalIgnoreCase)),
            AcknowledgedCount = alarms.Count(a => a.IsActive && a.AcknowledgedAt != null),
            ResolvedCount = alarms.Count(a => !a.IsActive),
            BySourceType = bySourceType
        };
    }
}
