namespace Backend.Domain.Entities;

public class DeviceSpecificationTemplate : BaseAuditableEntity
{
    public Guid DeviceTypeId { get; set; }
    public string Model { get; set; } = string.Empty;
    public string? Manufacturer { get; set; }

    /// <summary>
    /// UI schema defining which fields/sections to render for this device model.
    /// Example: { "sections": [{ "title": "General", "fields": [{ "key": "capacity", "label": "Capacity", "unit": "kWh" }] }] }
    /// </summary>
    public string SpecificationsTemplate { get; set; } = "{}";

    /// <summary>
    /// Default instance metadata copied to Device.Metadata when a device is registered from this template.
    /// Contains model-specific defaults like rated capacity, chemistry, designed cycle life.
    /// Example: { "nominalCapacity": 3916, "cellChemistry": "LFP", "designedCycleLife": 6000 }
    /// </summary>
    public string MetadataTemplate { get; set; } = "{}";

    /// <summary>
    /// Defines the internal sub-device structure this device model supports (max counts).
    /// Used as a schema/validation reference — actual counts come from RTU handshake.
    /// Example: { "children": [{ "key": "rack", "deviceTypeName": "EnergyStorage.Rack", "count": 14, "namePattern": "Rack {index}",
    ///   "children": [{ "key": "pack", "deviceTypeName": "EnergyStorage.Pack", "count": 4, "namePattern": "Pack {index}" }] }] }
    /// </summary>
    public string? HierarchyTemplate { get; set; }

    /// <summary>
    /// Defines how to map raw telemetry JSON payloads to typed parameters (ParameterKeys).
    /// Used by ParameterResolver as a fallback when the RTU does not normalize parameter names.
    /// Example: { "version": 1, "parameters": { "soc": { "type": "number", "unit": "%",
    ///   "source": { "kind": "ContainerBatteryState", "paths": ["stateOfCharge", "soc"] } } } }
    /// </summary>
    public string? ParameterMapTemplate { get; set; }

    public bool IsDefault { get; set; }

    // Navigation
    public DeviceType? DeviceType { get; set; }
    public ICollection<Device> Devices { get; set; } = new List<Device>();
}
