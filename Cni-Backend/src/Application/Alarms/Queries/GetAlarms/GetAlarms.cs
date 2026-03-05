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
            query = query.Where(a => a.AlarmType!.Severity.ToLower() == request.Severity.ToLower());

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

        var alarms = await query
            .OrderByDescending(a => a.OccurredAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new AlarmDto
            {
                Id = a.Id,
                Code = a.AlarmType!.Code,
                Message = a.AlarmType.Description,
                Severity = a.AlarmType.Severity.ToLower(),
                Status = !a.IsActive ? "resolved" : (a.AcknowledgedAt != null ? "acknowledged" : "active"),
                Source = a.Device != null ? a.Device.Name : a.AlarmType.Source,
                SourceType = a.AlarmType.Category != null ? a.AlarmType.Category.ToLower() : null,
                TriggeredAt = a.OccurredAt,
                Value = null, // Populated from Details JSON if available
                Threshold = null,
                AcknowledgedAt = a.AcknowledgedAt,
                AcknowledgedBy = a.AcknowledgedBy != null ? new AlarmUserDto
                {
                    Id = a.AcknowledgedBy.Id,
                    Name = a.AcknowledgedBy.Name,
                    Email = a.AcknowledgedBy.Email,
                    Initials = a.AcknowledgedBy.Name != null && a.AcknowledgedBy.Surname != null
                        ? (a.AcknowledgedBy.Name.Substring(0, 1) + a.AcknowledgedBy.Surname.Substring(0, 1)).ToUpper()
                        : null,
                    Role = a.AcknowledgedBy.Title
                } : null,
                ResolvedAt = a.ResolvedAt,
                ResolvedBy = a.ResolvedBy != null ? new AlarmUserDto
                {
                    Id = a.ResolvedBy.Id,
                    Name = a.ResolvedBy.Name,
                    Email = a.ResolvedBy.Email,
                    Initials = a.ResolvedBy.Name != null && a.ResolvedBy.Surname != null
                        ? (a.ResolvedBy.Name.Substring(0, 1) + a.ResolvedBy.Surname.Substring(0, 1)).ToUpper()
                        : null,
                    Role = a.ResolvedBy.Title
                } : null,
                RelatedAsset = a.Device != null ? new AlarmRelatedAssetDto
                {
                    Id = a.DeviceId,
                    Name = a.Device.Name,
                    Type = a.AlarmType.Category != null ? a.AlarmType.Category.ToLower() : null,
                    Location = a.Device.Rtu != null && a.Device.Rtu.Facility != null
                        ? a.Device.Rtu.Facility.Name : null,
                    Status = a.Device.Status,
                    SerialNumber = a.Device.SerialNumber,
                    Model = a.Device.Model
                } : null,
                RelatedMaintenanceTaskId = a.MaintenanceTaskId,
                Location = a.Device != null && a.Device.Rtu != null && a.Device.Rtu.Facility != null
                    ? a.Device.Rtu.Facility.Name : null
            })
            .ToListAsync(cancellationToken);

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
}
