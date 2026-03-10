using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models.Monitoring;
using Backend.Application.Common.Security;
using Backend.Domain.Constants;

namespace Backend.Application.Pcs.Queries.GetPcsFaultFlags;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.PcsView)]
public record GetPcsFaultFlagsQuery : IRequest<FaultFlagsResponseDto>
{
    public Guid UnitId { get; init; }
}

public class GetPcsFaultFlagsQueryHandler : IRequestHandler<GetPcsFaultFlagsQuery, FaultFlagsResponseDto>
{
    private readonly IApplicationDbContext _context;

    public GetPcsFaultFlagsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FaultFlagsResponseDto> Handle(GetPcsFaultFlagsQuery request, CancellationToken cancellationToken)
    {
        var device = await _context.Devices
            .Include(d => d.DeviceType)
            .Include(d => d.LogicalDevice)
            .Where(d => d.Id == request.UnitId &&
                ((d.DeviceType != null && d.DeviceType.Name == DeviceTypeNames.Pcs) ||
                 (d.LogicalDevice != null && d.LogicalDevice.Category == "PCS")))
            .FirstOrDefaultAsync(cancellationToken);

        Guard.Against.NotFound(request.UnitId, device);

        // Load all alarm type definitions for this device's model
        var alarmTypes = await _context.DeviceAlarmTypes
            .Where(t => t.DeviceTypeId == device.DeviceTypeId
                        && t.Model == device.Model
                        && t.IsEnabled)
            .OrderBy(t => t.Category)
            .ThenBy(t => t.Code)
            .ToListAsync(cancellationToken);

        // Load active alarm IDs for this device
        var activeAlarmTypeIds = (await _context.DeviceAlarms
            .Where(a => a.DeviceId == request.UnitId && a.IsActive)
            .Select(a => a.DeviceAlarmTypeId)
            .ToListAsync(cancellationToken))
            .ToHashSet();

        // Map alarm types → fault flags, marking active ones
        var flags = alarmTypes.Select(at => new FaultFlagDto
        {
            Key = at.FaultKey,
            Label = at.Name,
            IsActive = activeAlarmTypeIds.Contains(at.Id),
            Category = at.Category,
            Severity = at.Severity
        }).ToList();

        // Build categories from grouped flags
        var categories = flags
            .GroupBy(f => f.Category ?? "general")
            .Select(g => new FaultCategoryDto
            {
                Key = g.Key,
                Label = FormatLabel(g.Key),
                ActiveCount = g.Count(f => f.IsActive),
                TotalCount = g.Count()
            })
            .ToList();

        return new FaultFlagsResponseDto
        {
            Flags = flags,
            Sources = null,
            Categories = categories,
            Summary = new FaultFlagSummaryDto
            {
                TotalFlags = flags.Count,
                ActiveFlags = flags.Count(f => f.IsActive),
                ActiveByCategory = categories.Where(c => c.ActiveCount > 0)
                    .ToDictionary(c => c.Key, c => c.ActiveCount)
            }
        };
    }

    private static string FormatLabel(string key)
    {
        if (string.IsNullOrEmpty(key)) return "General";
        return string.Concat(
            key[0].ToString().ToUpperInvariant(),
            key.AsSpan(1)).Replace("_", " ");
    }
}
