using System.Text.Json;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models.Monitoring;
using Backend.Application.Common.Security;
using Backend.Application.Common.Services;
using Backend.Domain.Constants;

namespace Backend.Application.Pcs.Queries.GetPcsHeader;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.PcsView)]
public record GetPcsHeaderQuery : IRequest<PcsHeaderDto>
{
    public Guid UnitId { get; init; }
}

public class GetPcsHeaderQueryHandler : IRequestHandler<GetPcsHeaderQuery, PcsHeaderDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IDeviceStatusService _statusService;

    public GetPcsHeaderQueryHandler(IApplicationDbContext context, IDeviceStatusService statusService)
    {
        _context = context;
        _statusService = statusService;
    }

    public async Task<PcsHeaderDto> Handle(GetPcsHeaderQuery request, CancellationToken cancellationToken)
    {
        var device = await _context.Devices
            .Include(d => d.DeviceType)
            .Include(d => d.LogicalDevice)
            .Include(d => d.SpecificationTemplate)
            .Include(d => d.Rtu!).ThenInclude(r => r.Facility)
            .Where(d => d.Id == request.UnitId &&
                ((d.DeviceType != null && d.DeviceType.Name == DeviceTypeNames.Pcs) ||
                 (d.LogicalDevice != null && d.LogicalDevice.Category == "PCS")))
            .FirstOrDefaultAsync(cancellationToken);

        Guard.Against.NotFound(request.UnitId, device);

        // Status from active alarms
        var status = await _statusService.GetStatusAsync(device.Id, cancellationToken);

        // Linked BESS: prefer parent if EnergyStorage, else fallback to facility
        var linkedBess = await ResolveLinkedBess(device, cancellationToken);

        // Tabs from specification template
        var tabs = GetAvailableTabs(device.SpecificationTemplate?.SpecificationsTemplate);

        return new PcsHeaderDto
        {
            Id = device.Id,
            DeviceId = device.SerialNumber ?? string.Empty,
            Name = device.Name,
            Status = StatusBadgeDto.FromStatus(status),
            LinkedBessUnit = linkedBess,
            AvailableTabs = tabs
        };
    }

    private async Task<PcsHeaderLinkedBessDto?> ResolveLinkedBess(
        Domain.Entities.Device device, CancellationToken ct)
    {
        if (device.ParentDeviceId.HasValue)
        {
            var parent = await _context.Devices
                .Include(d => d.DeviceType)
                .FirstOrDefaultAsync(d => d.Id == device.ParentDeviceId.Value, ct);

            if (parent?.DeviceType?.Name == DeviceTypeNames.EnergyStorage ||
                parent?.LogicalDevice?.Category == "BESS")
                return new PcsHeaderLinkedBessDto { Id = parent.Id, Name = parent.Name };
        }

        return await _context.Devices
            .Include(d => d.DeviceType)
            .Include(d => d.LogicalDevice)
            .Where(d => d.Rtu!.FacilityId == device.Rtu!.FacilityId
                && ((d.DeviceType != null && d.DeviceType.Name == DeviceTypeNames.EnergyStorage) ||
                    (d.LogicalDevice != null && d.LogicalDevice.Category == "BESS")))
            .Select(d => new PcsHeaderLinkedBessDto { Id = d.Id, Name = d.Name })
            .FirstOrDefaultAsync(ct);
    }

    private static List<TabInfoDto> GetAvailableTabs(string? specifications)
    {
        var defaultTabs = new List<TabInfoDto>
        {
            new() { Key = "general", Label = "General", Order = 1 },
            new() { Key = "fault", Label = "Fault", Order = 2 },
            new() { Key = "curve", Label = "Curve", Order = 3 },
            new() { Key = "settings", Label = "Settings", Order = 4 }
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
                tabs.Add(new TabInfoDto { Key = key, Label = label, IsEnabled = enabled, Order = order });
            }
            return tabs.Count > 0 ? tabs : defaultTabs;
        }

        return defaultTabs;
    }
}
