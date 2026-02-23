using System.Text.Json;
using Backend.Application.Common.Interfaces;
using Backend.Domain.Constants;
using Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Common.Services;

/// <summary>
/// Loads telemetry data for all devices within a BESS container (root + children)
/// from multiple telemetry tables (Battery, PCS, Solar, SmartMeter).
/// </summary>
public class ContainerTelemetryLoader
{
    private readonly IApplicationDbContext _context;

    public ContainerTelemetryLoader(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Load the latest telemetry for a root device and all its descendants,
    /// querying the appropriate table per device category.
    /// </summary>
    public async Task<ContainerTelemetryContext> LoadLatest(Guid rootDeviceId, CancellationToken ct)
    {
        // 1. Load root + all descendants
        var root = await _context.Devices
            .Include(d => d.DeviceType)
            .FirstOrDefaultAsync(d => d.Id == rootDeviceId, ct);

        if (root == null)
            return new ContainerTelemetryContext();

        var descendants = await LoadDescendants(rootDeviceId, ct);
        var allDevices = new List<ContainerDevice> { MapDevice(root) };
        allDevices.AddRange(descendants);

        // 2. Group device IDs by which telemetry table to query
        var besIds = new List<Guid>();
        var pcsIds = new List<Guid>();
        var solarIds = new List<Guid>();
        var meterIds = new List<Guid>();

        foreach (var dev in allDevices)
        {
            switch (dev.Category)
            {
                case DeviceDataCategory.Bess:
                    besIds.Add(dev.Id);
                    break;
                case DeviceDataCategory.Pcs:
                    pcsIds.Add(dev.Id);
                    break;
                case DeviceDataCategory.Solar:
                    solarIds.Add(dev.Id);
                    break;
                case DeviceDataCategory.SmartMeter:
                    meterIds.Add(dev.Id);
                    break;
                default:
                    // Unknown category — try Battery table as fallback (most BESS sub-devices land here)
                    besIds.Add(dev.Id);
                    break;
            }
        }

        // 3. Query each table for latest data per device per Kind
        var entries = new List<TelemetryEntry>();

        if (besIds.Count > 0)
        {
            var batteryData = await _context.DeviceBatteryData
                .Where(dd => besIds.Contains(dd.DeviceId))
                .GroupBy(dd => new { dd.DeviceId, dd.Kind })
                .Select(g => g.OrderByDescending(dd => dd.DeviceLocalTime).First())
                .ToListAsync(ct);

            entries.AddRange(batteryData.Select(d => new TelemetryEntry
            {
                DeviceId = d.DeviceId,
                Kind = d.Kind,
                Payload = d.Payload,
                Category = DeviceDataCategory.Bess,
                Timestamp = d.DeviceLocalTime
            }));
        }

        if (pcsIds.Count > 0)
        {
            var pcsData = await _context.DevicePcsData
                .Where(dd => pcsIds.Contains(dd.DeviceId))
                .GroupBy(dd => new { dd.DeviceId, dd.Kind })
                .Select(g => g.OrderByDescending(dd => dd.DeviceLocalTime).First())
                .ToListAsync(ct);

            entries.AddRange(pcsData.Select(d => new TelemetryEntry
            {
                DeviceId = d.DeviceId,
                Kind = d.Kind,
                Payload = d.Payload,
                Category = DeviceDataCategory.Pcs,
                Timestamp = d.DeviceLocalTime
            }));
        }

        if (solarIds.Count > 0)
        {
            var solarData = await _context.DeviceSolarData
                .Where(dd => solarIds.Contains(dd.DeviceId))
                .GroupBy(dd => new { dd.DeviceId, dd.Kind })
                .Select(g => g.OrderByDescending(dd => dd.DeviceLocalTime).First())
                .ToListAsync(ct);

            entries.AddRange(solarData.Select(d => new TelemetryEntry
            {
                DeviceId = d.DeviceId,
                Kind = d.Kind,
                Payload = d.Payload,
                Category = DeviceDataCategory.Solar,
                Timestamp = d.DeviceLocalTime
            }));
        }

        if (meterIds.Count > 0)
        {
            var meterData = await _context.DeviceSmartMeterData
                .Where(dd => meterIds.Contains(dd.DeviceId))
                .GroupBy(dd => new { dd.DeviceId, dd.Kind })
                .Select(g => g.OrderByDescending(dd => dd.DeviceLocalTime).First())
                .ToListAsync(ct);

            entries.AddRange(meterData.Select(d => new TelemetryEntry
            {
                DeviceId = d.DeviceId,
                Kind = d.Kind,
                Payload = d.Payload,
                Category = DeviceDataCategory.SmartMeter,
                Timestamp = d.DeviceLocalTime
            }));
        }

        return new ContainerTelemetryContext(allDevices, entries);
    }

    private async Task<List<ContainerDevice>> LoadDescendants(Guid rootId, CancellationToken ct)
    {
        // Load all descendants in a single query using recursive CTE pattern
        // For now, do two levels (children + grandchildren) which covers Container > Rack > Pack > Module
        var children = await _context.Devices
            .Include(d => d.DeviceType)
            .Where(d => d.ParentDeviceId == rootId)
            .ToListAsync(ct);

        if (children.Count == 0)
            return new List<ContainerDevice>();

        var result = children.Select(MapDevice).ToList();

        var childIds = children.Select(d => d.Id).ToList();
        var grandchildren = await _context.Devices
            .Include(d => d.DeviceType)
            .Where(d => d.ParentDeviceId.HasValue && childIds.Contains(d.ParentDeviceId.Value))
            .ToListAsync(ct);

        result.AddRange(grandchildren.Select(MapDevice));

        if (grandchildren.Count > 0)
        {
            var grandchildIds = grandchildren.Select(d => d.Id).ToList();
            var greatGrandchildren = await _context.Devices
                .Include(d => d.DeviceType)
                .Where(d => d.ParentDeviceId.HasValue && grandchildIds.Contains(d.ParentDeviceId.Value))
                .ToListAsync(ct);

            result.AddRange(greatGrandchildren.Select(MapDevice));
        }

        return result;
    }

    private static ContainerDevice MapDevice(Domain.Entities.Device device)
    {
        var typeName = device.DeviceType?.Name;
        var category = CategorizeByDeviceType(typeName);

        return new ContainerDevice
        {
            Id = device.Id,
            ParentDeviceId = device.ParentDeviceId,
            Name = device.Name,
            DeviceTypeName = typeName,
            Category = category
        };
    }

    /// <summary>
    /// Maps DeviceType.Name to the telemetry table category.
    /// </summary>
    private static DeviceDataCategory CategorizeByDeviceType(string? deviceTypeName)
    {
        if (string.IsNullOrEmpty(deviceTypeName))
            return DeviceDataCategory.Unknown;

        if (deviceTypeName == DeviceTypeNames.Pcs)
            return DeviceDataCategory.Pcs;

        if (deviceTypeName == DeviceTypeNames.Solar)
            return DeviceDataCategory.Solar;

        if (deviceTypeName == DeviceTypeNames.SmartMeter)
            return DeviceDataCategory.SmartMeter;

        // EnergyStorage and all sub-types → Battery table
        if (deviceTypeName == DeviceTypeNames.EnergyStorage ||
            deviceTypeName.StartsWith(DeviceTypeNames.EnergyStoragePrefix, StringComparison.Ordinal))
            return DeviceDataCategory.Bess;

        return DeviceDataCategory.Unknown;
    }
}

public class ContainerDevice
{
    public Guid Id { get; init; }
    public Guid? ParentDeviceId { get; init; }
    public string? Name { get; init; }
    public string? DeviceTypeName { get; init; }
    public DeviceDataCategory Category { get; init; }
}

public class TelemetryEntry
{
    public Guid DeviceId { get; init; }
    public string? Kind { get; init; }
    public string? Payload { get; init; }
    public DeviceDataCategory Category { get; init; }
    public DateTimeOffset Timestamp { get; init; }
}

public class ContainerTelemetryContext
{
    private readonly List<ContainerDevice> _devices;
    private readonly List<TelemetryEntry> _entries;

    public ContainerTelemetryContext()
    {
        _devices = new List<ContainerDevice>();
        _entries = new List<TelemetryEntry>();
    }

    public ContainerTelemetryContext(List<ContainerDevice> devices, List<TelemetryEntry> entries)
    {
        _devices = devices;
        _entries = entries;
    }

    public IReadOnlyList<ContainerDevice> Devices => _devices;
    public IReadOnlyList<TelemetryEntry> Entries => _entries;

    /// <summary>
    /// Get all latest payloads indexed by Kind (merging across all devices/tables).
    /// If multiple devices have the same Kind, root device takes precedence.
    /// </summary>
    public Dictionary<string, JsonElement?> GetPayloadsByKind()
    {
        var result = new Dictionary<string, JsonElement?>();

        // Process root device entries first, then children (root wins on conflict)
        var rootId = _devices.Count > 0 ? _devices[0].Id : Guid.Empty;
        var rootEntries = _entries.Where(e => e.DeviceId == rootId);
        var childEntries = _entries.Where(e => e.DeviceId != rootId);

        foreach (var entry in rootEntries)
        {
            if (entry.Kind != null)
                result[entry.Kind] = PayloadDeserializer.GetElement(entry.Payload);
        }

        foreach (var entry in childEntries)
        {
            if (entry.Kind != null && !result.ContainsKey(entry.Kind))
                result[entry.Kind] = PayloadDeserializer.GetElement(entry.Payload);
        }

        return result;
    }

    /// <summary>
    /// Get telemetry payload for a specific device.
    /// </summary>
    public JsonElement? GetPayloadForDevice(Guid deviceId, string? kind = null)
    {
        var entry = kind != null
            ? _entries.FirstOrDefault(e => e.DeviceId == deviceId && e.Kind == kind)
            : _entries.Where(e => e.DeviceId == deviceId)
                      .OrderByDescending(e => e.Timestamp)
                      .FirstOrDefault();

        return entry != null ? PayloadDeserializer.GetElement(entry.Payload) : null;
    }

    /// <summary>
    /// Get all entries from a specific telemetry category (e.g., all PCS data).
    /// </summary>
    public IEnumerable<TelemetryEntry> GetEntriesByCategory(DeviceDataCategory category)
    {
        return _entries.Where(e => e.Category == category);
    }

    /// <summary>
    /// Get device IDs that belong to a specific category.
    /// </summary>
    public IEnumerable<Guid> GetDeviceIdsByCategory(DeviceDataCategory category)
    {
        return _devices.Where(d => d.Category == category).Select(d => d.Id);
    }
}
