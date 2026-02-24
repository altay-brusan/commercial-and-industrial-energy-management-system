using System.Text.Json;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models.Monitoring;
using Backend.Application.Common.Security;
using Backend.Application.Common.Services;
using Backend.Domain.Constants;

namespace Backend.Application.BatteryStorage.Queries.GetBessUnitDetail;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.BessView)]
public record GetBessUnitDetailQuery : IRequest<BessUnitDetailDto>
{
    public Guid UnitId { get; init; }
}

public class GetBessUnitDetailQueryHandler : IRequestHandler<GetBessUnitDetailQuery, BessUnitDetailDto>
{
    private readonly IApplicationDbContext _context;

    public GetBessUnitDetailQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BessUnitDetailDto> Handle(GetBessUnitDetailQuery request, CancellationToken cancellationToken)
    {
        var device = await _context.Devices
            .Include(d => d.DeviceType)
            .Include(d => d.SpecificationTemplate)
            .Include(d => d.Rtu!).ThenInclude(r => r.Facility)
            .Where(d => d.Id == request.UnitId && d.DeviceType != null && d.DeviceType.Name == DeviceTypeNames.EnergyStorage)
            .FirstOrDefaultAsync(cancellationToken);

        Guard.Against.NotFound(request.UnitId, device);

        var tabs = GetAvailableTabs(device.SpecificationTemplate?.SpecificationsTemplate);

        return new BessUnitDetailDto
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
            LastSeenUtc = device.LastSeenUtc
        };
    }

    private static List<TabInfoDto> GetAvailableTabs(string? specifications)
    {
        var defaultTabs = new List<TabInfoDto>
        {
            new() { Key = "summary", Label = "Summary", Order = 1 },
            new() { Key = "health", Label = "Health", Order = 2 },
            new() { Key = "curves", Label = "Curves", Order = 3 },
            new() { Key = "hierarchy", Label = "Hierarchy", Order = 4 },
            new() { Key = "faultFlags", Label = "Fault Flags", Order = 5 },
            new() { Key = "faults", Label = "Faults", Order = 6 }
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
