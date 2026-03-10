using System.Text.Json;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Entities;
using Backend.Domain.Events;

namespace Backend.Application.Devices.Commands.RegisterDeviceFromLogicalDevice;

[Authorize(Roles = "Administrator")]
[RequirePermission(Backend.Domain.Constants.Permissions.DeviceCreate)]
public record RegisterDeviceFromLogicalDeviceCommand : IRequest<Guid>
{
    public Guid RtuId { get; init; }
    public Guid LogicalDeviceId { get; init; }
    public Guid DeviceTypeId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string SerialNumber { get; init; } = string.Empty;
    public string? Manufacturer { get; init; }
    public string? Model { get; init; }
    public Guid? ParentDeviceId { get; init; }
}

public class RegisterDeviceFromLogicalDeviceCommandHandler : IRequestHandler<RegisterDeviceFromLogicalDeviceCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public RegisterDeviceFromLogicalDeviceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(RegisterDeviceFromLogicalDeviceCommand request, CancellationToken cancellationToken)
    {
        var logicalDevice = await _context.LogicalDevices
            .FirstOrDefaultAsync(ld => ld.Id == request.LogicalDeviceId, cancellationToken);

        Guard.Against.NotFound(request.LogicalDeviceId, logicalDevice);

        // Build lookup: logicalDeviceName → (LogicalDevice.Id, DeviceType.Id)
        // The hierarchy template references child LogicalDevice names
        var childLogicalDeviceNames = CollectLogicalDeviceNames(logicalDevice.HierarchyTemplate);
        var logicalDeviceLookup = new Dictionary<string, LogicalDevice>();

        if (childLogicalDeviceNames.Count > 0)
        {
            logicalDeviceLookup = await _context.LogicalDevices
                .Where(ld => childLogicalDeviceNames.Contains(ld.Name))
                .ToDictionaryAsync(ld => ld.Name, ld => ld, cancellationToken);
        }

        // Build DeviceType lookup for child devices
        // Collect device type names from hierarchy template
        var deviceTypeNames = CollectDeviceTypeNames(logicalDevice.HierarchyTemplate);
        // Include the root device type
        var rootDeviceType = await _context.DeviceTypes
            .FirstOrDefaultAsync(dt => dt.Id == request.DeviceTypeId, cancellationToken);
        Guard.Against.NotFound(request.DeviceTypeId, rootDeviceType);

        var deviceTypeLookup = new Dictionary<string, Guid> { { rootDeviceType.Name, rootDeviceType.Id } };
        if (deviceTypeNames.Count > 0)
        {
            var childTypes = await _context.DeviceTypes
                .Where(dt => deviceTypeNames.Contains(dt.Name))
                .ToDictionaryAsync(dt => dt.Name, dt => dt.Id, cancellationToken);
            foreach (var kvp in childTypes)
                deviceTypeLookup.TryAdd(kvp.Key, kvp.Value);
        }

        // Create root device
        var rootDevice = new Device
        {
            RtuId = request.RtuId,
            DeviceTypeId = request.DeviceTypeId,
            LogicalDeviceId = logicalDevice.Id,
            ParentDeviceId = request.ParentDeviceId,
            Name = request.Name,
            Model = request.Model ?? logicalDevice.Name,
            SerialNumber = request.SerialNumber,
            Manufacturer = request.Manufacturer,
            Ports = logicalDevice.Ports
        };

        rootDevice.AddDomainEvent(new DeviceCreatedEvent(rootDevice));
        _context.Devices.Add(rootDevice);

        // Create child hierarchy from LogicalDevice.HierarchyTemplate
        if (!string.IsNullOrWhiteSpace(logicalDevice.HierarchyTemplate))
        {
            CreateChildDevices(rootDevice, logicalDevice.HierarchyTemplate, request.RtuId,
                logicalDeviceLookup, deviceTypeLookup);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return rootDevice.Id;
    }

    private void CreateChildDevices(
        Device parent,
        string hierarchyTemplateJson,
        Guid rtuId,
        Dictionary<string, LogicalDevice> logicalDeviceLookup,
        Dictionary<string, Guid> deviceTypeLookup)
    {
        var doc = JsonDocument.Parse(hierarchyTemplateJson);
        if (!doc.RootElement.TryGetProperty("children", out var childrenElement) ||
            childrenElement.ValueKind != JsonValueKind.Array)
            return;

        foreach (var childDef in childrenElement.EnumerateArray())
        {
            CreateChildrenFromDefinition(parent, childDef, rtuId, logicalDeviceLookup, deviceTypeLookup);
        }
    }

    private void CreateChildrenFromDefinition(
        Device parent,
        JsonElement definition,
        Guid rtuId,
        Dictionary<string, LogicalDevice> logicalDeviceLookup,
        Dictionary<string, Guid> deviceTypeLookup)
    {
        var logicalDeviceName = definition.GetProperty("logicalDeviceName").GetString()!;
        var count = definition.TryGetProperty("count", out var countProp) ? countProp.GetInt32() : 1;
        var namePattern = definition.TryGetProperty("namePattern", out var nameProp)
            ? nameProp.GetString() ?? "{index}"
            : "{index}";

        // Resolve the child LogicalDevice and DeviceType
        logicalDeviceLookup.TryGetValue(logicalDeviceName, out var childLogicalDevice);

        // Try to find a device type matching the logical device name, or fallback to deviceTypeName if specified
        Guid? childDeviceTypeId = null;
        if (definition.TryGetProperty("deviceTypeName", out var dtNameProp))
        {
            var dtName = dtNameProp.GetString()!;
            if (deviceTypeLookup.TryGetValue(dtName, out var dtId))
                childDeviceTypeId = dtId;
        }
        else if (deviceTypeLookup.TryGetValue(logicalDeviceName, out var dtId2))
        {
            childDeviceTypeId = dtId2;
        }

        // If we can't resolve a device type, skip
        if (!childDeviceTypeId.HasValue)
            return;

        var hasChildren = definition.TryGetProperty("children", out var childrenElement) &&
                          childrenElement.ValueKind == JsonValueKind.Array;

        // Also check if the child LogicalDevice has its own hierarchy template
        var childHasLogicalHierarchy = childLogicalDevice != null &&
                                       !string.IsNullOrWhiteSpace(childLogicalDevice.HierarchyTemplate);

        for (var i = 1; i <= count; i++)
        {
            var childName = namePattern.Replace("{index}", i.ToString());

            var child = new Device
            {
                RtuId = rtuId,
                DeviceTypeId = childDeviceTypeId.Value,
                LogicalDeviceId = childLogicalDevice?.Id,
                ParentDeviceId = parent.Id,
                Name = childName,
                Model = logicalDeviceName,
                SerialNumber = $"{parent.SerialNumber}-{childName.Replace(" ", "-")}",
                Manufacturer = parent.Manufacturer,
                Ports = childLogicalDevice?.Ports ?? "[]",
                HierarchyIndex = i - 1
            };

            child.AddDomainEvent(new DeviceCreatedEvent(child));
            _context.Devices.Add(child);

            // Recursively create from inline children definition
            if (hasChildren)
            {
                foreach (var nestedChildDef in childrenElement.EnumerateArray())
                {
                    CreateChildrenFromDefinition(child, nestedChildDef, rtuId, logicalDeviceLookup, deviceTypeLookup);
                }
            }
            // Or create from the child LogicalDevice's own hierarchy template
            else if (childHasLogicalHierarchy)
            {
                CreateChildDevices(child, childLogicalDevice!.HierarchyTemplate!, rtuId,
                    logicalDeviceLookup, deviceTypeLookup);
            }
        }
    }

    private static HashSet<string> CollectLogicalDeviceNames(string? hierarchyTemplateJson)
    {
        var names = new HashSet<string>();
        if (string.IsNullOrWhiteSpace(hierarchyTemplateJson))
            return names;

        var doc = JsonDocument.Parse(hierarchyTemplateJson);
        if (doc.RootElement.TryGetProperty("children", out var childrenElement))
            CollectNamesRecursive(childrenElement, names, "logicalDeviceName");

        return names;
    }

    private static HashSet<string> CollectDeviceTypeNames(string? hierarchyTemplateJson)
    {
        var names = new HashSet<string>();
        if (string.IsNullOrWhiteSpace(hierarchyTemplateJson))
            return names;

        var doc = JsonDocument.Parse(hierarchyTemplateJson);
        if (doc.RootElement.TryGetProperty("children", out var childrenElement))
        {
            CollectNamesRecursive(childrenElement, names, "deviceTypeName");
            CollectNamesRecursive(childrenElement, names, "logicalDeviceName");
        }

        return names;
    }

    private static void CollectNamesRecursive(JsonElement element, HashSet<string> names, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Array) return;

        foreach (var item in element.EnumerateArray())
        {
            if (item.TryGetProperty(propertyName, out var nameElement))
                names.Add(nameElement.GetString()!);

            if (item.TryGetProperty("children", out var children))
                CollectNamesRecursive(children, names, propertyName);
        }
    }
}
