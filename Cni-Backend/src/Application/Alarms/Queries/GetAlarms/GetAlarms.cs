using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models;
using Backend.Application.Common.Security;

namespace Backend.Application.Alarms.Queries.GetAlarms;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.AlarmView)]
public record GetAlarmsQuery : IRequest<AlarmListResponseDto>
{
    public Guid? FacilityId { get; init; }
    public string? Severity { get; init; }
    public string? Status { get; init; } // "active", "acknowledged", "resolved"
    public Guid? DeviceId { get; init; }
    public bool? IsActive { get; init; }
    public DateTimeOffset? DateFrom { get; init; }
    public DateTimeOffset? DateTo { get; init; }
    public string? SearchTerm { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public class GetAlarmsQueryHandler : IRequestHandler<GetAlarmsQuery, AlarmListResponseDto>
{
    private readonly IApplicationDbContext _context;

    public GetAlarmsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AlarmListResponseDto> Handle(GetAlarmsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.DeviceAlarms
            .Include(a => a.AlarmType)
            .Include(a => a.Device)
                .ThenInclude(d => d!.Rtu)
                    .ThenInclude(r => r!.Facility)
            .Include(a => a.AcknowledgedBy)
            .Include(a => a.ResolvedBy)
            .AsQueryable();

        if (request.FacilityId.HasValue)
            query = query.Where(a => a.Device!.Rtu!.FacilityId == request.FacilityId.Value);

        if (!string.IsNullOrEmpty(request.Severity))
        {
            var dbSeverities = MapFrontendSeverityToDb(request.Severity);
            query = query.Where(a => dbSeverities.Contains(a.AlarmType!.Severity));
        }

        if (request.DeviceId.HasValue)
            query = query.Where(a => a.DeviceId == request.DeviceId.Value);

        if (request.IsActive.HasValue)
            query = query.Where(a => a.IsActive == request.IsActive.Value);

        if (request.DateFrom.HasValue)
            query = query.Where(a => a.OccurredAt >= request.DateFrom.Value);

        if (request.DateTo.HasValue)
            query = query.Where(a => a.OccurredAt <= request.DateTo.Value);

        // Status filter (case-insensitive)
        if (!string.IsNullOrEmpty(request.Status))
        {
            var statusLower = request.Status.ToLower();
            query = statusLower switch
            {
                "active" => query.Where(a => a.IsActive && a.AcknowledgedAt == null),
                "acknowledged" => query.Where(a => a.IsActive && a.AcknowledgedAt != null),
                "resolved" => query.Where(a => !a.IsActive),
                _ => query
            };
        }

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(a =>
                a.AlarmType!.Name.ToLower().Contains(term) ||
                a.AlarmType.Code.ToLower().Contains(term) ||
                (a.Device != null && a.Device.Name.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var entities = await query
            .OrderByDescending(a => a.OccurredAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var alarms = entities.Select(AlarmDtoMapper.ToDto).ToList();

        return new AlarmListResponseDto
        {
            Alarms = alarms,
            Pagination = new AlarmPaginationDto
            {
                Page = request.Page,
                PageSize = request.PageSize,
                TotalItems = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
            }
        };
    }

    /// <summary>
    /// Maps frontend severity names (critical/high/medium/low) to DB severity names
    /// so the EF filter matches stored values (Critical/Major/Minor/Warning).
    /// </summary>
    private static List<string> MapFrontendSeverityToDb(string frontendSeverity) =>
        frontendSeverity.ToLower() switch
        {
            "critical" => ["Critical"],
            "high" => ["Major", "High"],
            "medium" => ["Minor", "Medium"],
            "low" => ["Warning", "Low"],
            _ => [frontendSeverity]
        };
}
