using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models.Monitoring;
using Backend.Application.Common.Security;

namespace Backend.Application.BatteryStorage.Queries.GetBessFaults;

// TODO: Review and rework this notifications/faults endpoint.
//
// 1. SCOPE: Currently fetches alarms for ALL devices in the same RTU (line 35-38).
//    This means requesting notifications for a single rack returns faults from the
//    entire container + all siblings. It should scope to the requested device and its
//    descendants only (walk ParentDeviceId tree), so the frontend can show faults
//    specific to the selected node in the device tree.
//
// 2. DEVICE INFO: The response only includes alarm type details (code, name, severity)
//    but not which device raised the alarm. Add DeviceId, DeviceName, and DeviceType
//    to FaultRecordDto so the frontend can show "Rack-01 > Pack-03: Over-temperature"
//    instead of just "Over-temperature".
//
// 3. IsRead / IsProcessed: DeviceAlarm has IsRead and IsProcessed fields but they are
//    not exposed in the query or response. The frontend needs these to show read/unread
//    badge counts and to mark notifications as read. Add filter parameters and include
//    them in FaultRecordDto.
//
// 4. DATE RANGE FILTER: No time-based filtering — the query returns all historical
//    alarms paginated. Consider adding optional startDate/endDate parameters so the
//    frontend can request "last 24h" or "last 7d" of notifications.
//
// 5. SORTING: Currently sorts by OccurredAt descending only. Consider adding a sortBy
//    parameter (e.g. severity, status, device) for frontend table sorting support.

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.BessView)]
public record GetBessFaultsQuery : IRequest<FaultsPaginatedResponseDto>
{
    public Guid UnitId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? Severity { get; init; }
    public bool? IsActive { get; init; }
}

public class GetBessFaultsQueryHandler : IRequestHandler<GetBessFaultsQuery, FaultsPaginatedResponseDto>
{
    private readonly IApplicationDbContext _context;

    public GetBessFaultsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FaultsPaginatedResponseDto> Handle(GetBessFaultsQuery request, CancellationToken cancellationToken)
    {
        // Get the root device and all descendant devices in the same RTU
        var device = await _context.Devices
            .FirstOrDefaultAsync(d => d.Id == request.UnitId, cancellationToken);

        Guard.Against.NotFound(request.UnitId, device);

        var rtuDeviceIds = await _context.Devices
            .Where(d => d.RtuId == device.RtuId)
            .Select(d => d.Id)
            .ToListAsync(cancellationToken);

        var query = _context.DeviceAlarms
            .Include(a => a.AlarmType)
            .Where(a => rtuDeviceIds.Contains(a.DeviceId));

        if (!string.IsNullOrWhiteSpace(request.Severity))
        {
            var sev = request.Severity.ToLowerInvariant();
            query = query.Where(a => a.AlarmType!.Severity.ToLower() == sev);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(a => a.IsActive == request.IsActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(a => a.OccurredAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new FaultRecordDto
            {
                Id = a.Id,
                Code = a.AlarmType!.Code,
                Name = a.AlarmType.Name,
                Severity = FaultRecordDto.SeverityBadge(a.AlarmType.Severity),
                Status = FaultRecordDto.FaultStatusBadge(a.IsActive),
                Source = a.AlarmType.Source,
                OccurredAt = a.OccurredAt,
                RecoveredAt = a.RecoveredAt,
                RelatedAlarmId = a.Id
            })
            .ToListAsync(cancellationToken);

        return new FaultsPaginatedResponseDto
        {
            Faults = items,
            Pagination = new PaginationDto
            {
                Page = request.Page,
                PageSize = request.PageSize,
                TotalItems = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
            }
        };
    }
}
