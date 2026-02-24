using System.Text.Json;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Entities;
using Backend.Domain.Events;

namespace Backend.Application.Devices.Commands.RegisterDeviceComposite;

[Authorize(Roles = "Administrator")]
[RequirePermission(Backend.Domain.Constants.Permissions.DeviceCreate)]
public record RegisterDeviceCompositeCommand : IRequest<Guid>
{
    public Guid RtuId { get; init; }
    public Guid TemplateId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string SerialNumber { get; init; } = string.Empty;
    public Guid? ParentDeviceId { get; init; }
}

public class RegisterDeviceCompositeCommandHandler : IRequestHandler<RegisterDeviceCompositeCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public RegisterDeviceCompositeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(RegisterDeviceCompositeCommand request, CancellationToken cancellationToken)
    {
        var template = await _context.DeviceSpecificationTemplates
            .Include(t => t.DeviceType)
            .FirstOrDefaultAsync(t => t.Id == request.TemplateId, cancellationToken);

        Guard.Against.NotFound(request.TemplateId, template);

        // Build lookup for device type names → IDs
        var deviceTypeNames = CollectDeviceTypeNames(template.HierarchyTemplate);
        deviceTypeNames.Add(template.DeviceType!.Name);

        var deviceTypeLookup = await _context.DeviceTypes
            .Where(dt => deviceTypeNames.Contains(dt.Name))
            .ToDictionaryAsync(dt => dt.Name, dt => dt.Id, cancellationToken);

        // Create root device
        var rootDevice = new Device
        {
            RtuId = request.RtuId,
            DeviceTypeId = template.DeviceTypeId,
            ParentDeviceId = request.ParentDeviceId,
            Name = request.Name,
            Model = template.Model,
            SerialNumber = request.SerialNumber,
            Manufacturer = template.Manufacturer,
            Specifications = template.SpecificationsTemplate,
            Metadata = template.MetadataTemplate
        };

        rootDevice.AddDomainEvent(new DeviceCreatedEvent(rootDevice));
        _context.Devices.Add(rootDevice);

        // Create child hierarchy from template
        if (!string.IsNullOrWhiteSpace(template.HierarchyTemplate))
        {
            CreateChildDevices(rootDevice, template.HierarchyTemplate, request.RtuId, deviceTypeLookup);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return rootDevice.Id;
    }

    private void CreateChildDevices(
        Device parent,
        string hierarchyTemplateJson,
        Guid rtuId,
        Dictionary<string, Guid> deviceTypeLookup)
    {
        var doc = JsonDocument.Parse(hierarchyTemplateJson);
        if (!doc.RootElement.TryGetProperty("children", out var childrenElement) ||
            childrenElement.ValueKind != JsonValueKind.Array)
            return;

        foreach (var childDef in childrenElement.EnumerateArray())
        {
            CreateChildrenFromDefinition(parent, childDef, rtuId, deviceTypeLookup);
        }
    }

    private void CreateChildrenFromDefinition(
        Device parent,
        JsonElement definition,
        Guid rtuId,
        Dictionary<string, Guid> deviceTypeLookup)
    {
        var deviceTypeName = definition.GetProperty("deviceTypeName").GetString()!;
        var count = definition.TryGetProperty("count", out var countProp) ? countProp.GetInt32() : 1;
        var namePattern = definition.TryGetProperty("namePattern", out var nameProp)
            ? nameProp.GetString() ?? "{index}"
            : "{index}";

        if (!deviceTypeLookup.TryGetValue(deviceTypeName, out var deviceTypeId))
            return;

        // Check for nested children definition
        var hasChildren = definition.TryGetProperty("children", out var childrenElement) &&
                          childrenElement.ValueKind == JsonValueKind.Array;

        for (var i = 1; i <= count; i++)
        {
            var childName = namePattern.Replace("{index}", i.ToString());

            var child = new Device
            {
                RtuId = rtuId,
                DeviceTypeId = deviceTypeId,
                ParentDeviceId = parent.Id,
                Name = childName,
                Model = deviceTypeName,
                SerialNumber = $"{parent.SerialNumber}-{childName.Replace(" ", "-")}",
                Manufacturer = parent.Manufacturer,
                HierarchyIndex = i - 1 // 0-based to match edge addressing (e.g., bmuLoc)
            };

            child.AddDomainEvent(new DeviceCreatedEvent(child));
            _context.Devices.Add(child);

            // Recursively create nested children
            if (hasChildren)
            {
                foreach (var nestedChildDef in childrenElement.EnumerateArray())
                {
                    CreateChildrenFromDefinition(child, nestedChildDef, rtuId, deviceTypeLookup);
                }
            }
        }
    }

    private static HashSet<string> CollectDeviceTypeNames(string? hierarchyTemplateJson)
    {
        var names = new HashSet<string>();
        if (string.IsNullOrWhiteSpace(hierarchyTemplateJson))
            return names;

        var doc = JsonDocument.Parse(hierarchyTemplateJson);
        if (doc.RootElement.TryGetProperty("children", out var childrenElement))
            CollectNamesRecursive(childrenElement, names);

        return names;
    }

    private static void CollectNamesRecursive(JsonElement element, HashSet<string> names)
    {
        if (element.ValueKind != JsonValueKind.Array) return;

        foreach (var item in element.EnumerateArray())
        {
            if (item.TryGetProperty("deviceTypeName", out var nameElement))
                names.Add(nameElement.GetString()!);

            if (item.TryGetProperty("children", out var children))
                CollectNamesRecursive(children, names);
        }
    }
}
