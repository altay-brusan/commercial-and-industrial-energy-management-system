using System.Text.Json;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models.Monitoring;
using Backend.Application.Common.Security;
using Backend.Application.Common.Services;
using Backend.Domain.Constants;

namespace Backend.Application.Pcs.Queries.GetPcsUnitDetail;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.PcsView)]
public record GetPcsUnitDetailQuery : IRequest<PcsUnitDetailDto>
{
    public Guid UnitId { get; init; }
}

public class GetPcsUnitDetailQueryHandler : IRequestHandler<GetPcsUnitDetailQuery, PcsUnitDetailDto>
{
    private readonly IApplicationDbContext _context;

    public GetPcsUnitDetailQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PcsUnitDetailDto> Handle(GetPcsUnitDetailQuery request, CancellationToken cancellationToken)
    {
        var device = await _context.Devices
            .Include(d => d.DeviceType)
            .Include(d => d.SpecificationTemplate)
            .Include(d => d.Rtu!).ThenInclude(r => r.Facility)
            .Where(d => d.Id == request.UnitId && d.DeviceType != null && d.DeviceType.Name == "PCS")
            .FirstOrDefaultAsync(cancellationToken);

        Guard.Against.NotFound(request.UnitId, device);

        // Find linked BESS unit (Battery device in same facility or via ParentDeviceId)
        var linkedBess = await _context.Devices
            .Include(d => d.DeviceType)
            .Where(d => d.Rtu!.FacilityId == device.Rtu!.FacilityId && d.DeviceType != null && d.DeviceType.Name == DeviceTypeNames.EnergyStorage)
            .FirstOrDefaultAsync(cancellationToken);

        var tabs = GetAvailableTabs(device.SpecificationTemplate?.SpecificationsTemplate);

        return new PcsUnitDetailDto
        {
            Id = device.Id,
            Name = device.Name,
            SerialNumber = device.SerialNumber,
            ModelName = device.Model,
            Manufacturer = device.Manufacturer,
            FacilityId = device.Rtu!.FacilityId,
            FacilityName = device.Rtu?.Facility?.Name,
            Status = StatusBadgeDto.FromStatus(device.Status),
            AvailableTabs = tabs,
            LinkedBessUnit = linkedBess != null ? new PcsLinkedBessDto
            {
                Id = linkedBess.Id,
                Name = linkedBess.Name,
                Status = linkedBess.Status
            } : null,
            LastSeenUtc = device.LastSeenUtc
        };
    }

    private static List<TabInfoDto> GetAvailableTabs(string? specifications)
    {
        var defaultTabs = new List<TabInfoDto>
        {
            new() { Key = "general", Label = "General", Order = 1 },
            new() { Key = "curves", Label = "Curves", Order = 2 },
            new() { Key = "faultFlags", Label = "Fault Flags", Order = 3 }
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
