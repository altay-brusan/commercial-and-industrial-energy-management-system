using System.Text.Json;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models.Monitoring;
using Backend.Application.Common.Security;
using Backend.Application.Common.Services;
using Backend.Domain.Constants;

namespace Backend.Application.SolarInverters.Queries.GetSolarUnitHeader;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.SolarView)]
public record GetSolarUnitHeaderQuery : IRequest<SolarUnitHeaderDto>
{
    public Guid UnitId { get; init; }
}

public class GetSolarUnitHeaderQueryHandler : IRequestHandler<GetSolarUnitHeaderQuery, SolarUnitHeaderDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IDeviceStatusService _statusService;

    public GetSolarUnitHeaderQueryHandler(IApplicationDbContext context, IDeviceStatusService statusService)
    {
        _context = context;
        _statusService = statusService;
    }

    public async Task<SolarUnitHeaderDto> Handle(GetSolarUnitHeaderQuery request, CancellationToken cancellationToken)
    {
        var device = await _context.Devices
            .Include(d => d.DeviceType)
            .Include(d => d.LogicalDevice)
            .Include(d => d.SpecificationTemplate)
            .Include(d => d.Rtu!).ThenInclude(r => r.Facility)
            .Where(d => d.Id == request.UnitId &&
                ((d.DeviceType != null && (d.DeviceType.Name == DeviceTypeNames.Solar || d.DeviceType.Name == DeviceTypeNames.Inverter)) ||
                 (d.LogicalDevice != null && d.LogicalDevice.Category == "Solar")))
            .FirstOrDefaultAsync(cancellationToken);

        Guard.Against.NotFound(request.UnitId, device);

        var status = await _statusService.GetStatusAsync(device.Id, cancellationToken);

        var alarmCount = await _context.DeviceAlarms
            .CountAsync(a => a.DeviceId == device.Id && a.IsActive, cancellationToken);

        var tabs = GetAvailableTabs(device.SpecificationTemplate?.SpecificationsTemplate);

        return new SolarUnitHeaderDto
        {
            Id = device.Id,
            Name = device.Name,
            SerialNumber = device.SerialNumber,
            ModelName = device.Model,
            Manufacturer = device.Manufacturer,
            FacilityId = device.Rtu!.FacilityId,
            FacilityName = device.Rtu?.Facility?.Name,
            Status = StatusBadgeDto.FromStatus(status),
            ActiveAlarmCount = alarmCount,
            AvailableTabs = tabs,
            LastSeenUtc = device.LastSeenUtc
        };
    }

    private static List<TabInfoDto> GetAvailableTabs(string? specifications)
    {
        var defaultTabs = new List<TabInfoDto>
        {
            new() { Key = "general", Label = "General", Order = 1 },
            new() { Key = "faults", Label = "Faults", Order = 2 },
            new() { Key = "curves", Label = "Curves", Order = 3 },
            new() { Key = "history", Label = "History", Order = 4 }
        };

        if (string.IsNullOrWhiteSpace(specifications))
            return defaultTabs;

        var element = PayloadDeserializer.GetElement(specifications);
        if (!element.HasValue)
            return defaultTabs;

        if (element.Value.TryGetProperty("supportedTabs", out var tabsElement) &&
            tabsElement.ValueKind == JsonValueKind.Array)
        {
            var order = 0;
            var tabs = new List<TabInfoDto>();
            foreach (var tab in tabsElement.EnumerateArray())
            {
                order++;
                var key = tab.TryGetProperty("key", out var k) ? k.GetString() ?? "" : "";
                var label = tab.TryGetProperty("label", out var l) ? l.GetString() ?? key : key;
                var enabled = !tab.TryGetProperty("enabled", out var e) || e.GetBoolean();
                tabs.Add(new TabInfoDto { Key = key, Label = label, Enabled = enabled, Order = order });
            }
            return tabs.Count > 0 ? tabs : defaultTabs;
        }

        return defaultTabs;
    }
}

public record SolarUnitHeaderDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? SerialNumber { get; init; }
    public string? ModelName { get; init; }
    public string? Manufacturer { get; init; }
    public Guid FacilityId { get; init; }
    public string? FacilityName { get; init; }
    public StatusBadgeDto Status { get; init; } = new();
    public int ActiveAlarmCount { get; init; }
    public List<TabInfoDto> AvailableTabs { get; init; } = new();
    public DateTimeOffset? LastSeenUtc { get; init; }
}
