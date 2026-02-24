using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models.Monitoring;
using Backend.Application.Common.Security;

namespace Backend.Application.BatteryStorage.Queries.GetBessFaults;

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
