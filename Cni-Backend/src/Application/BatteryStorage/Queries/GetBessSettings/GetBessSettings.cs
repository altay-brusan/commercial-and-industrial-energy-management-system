using System.Text.Json;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models.Monitoring;
using Backend.Application.Common.Security;
using Backend.Application.Common.Services;
using Backend.Domain.Constants;

namespace Backend.Application.BatteryStorage.Queries.GetBessSettings;

// TODO: Rework settings storage and this endpoint.
//
// CURRENT STATE:
// Settings values are read from Device.Metadata (a general-purpose JSONB column).
// Most values come back as null because Metadata doesn't contain setting keys like
// tempAlarmThreshold, socLowAlarm, chargeStopSoc, firmwareVersion, balancingEnabled, etc.
// There is no dedicated settings storage and no update command to write settings back.
//
// PROBLEMS:
// 1. NO DEDICATED STORAGE: Device.Metadata mixes device identity info (protocol, firmware)
//    with user-editable configuration (alarm thresholds, operating mode, balancing).
//    There is no DeviceSettings table or column — settings have nowhere to persist.
//
// 2. NO DEFAULT VALUES: When a setting has no stored value, it returns null. The template
//    (SpecificationsTemplate) defines which settings fields exist but not their default
//    values. A new device should inherit sensible defaults from its template.
//
// 3. NO UPDATE COMMAND: There is no UpdateBessSettings command. The frontend has editable
//    fields but no way to save changes. Need a command that validates field types/ranges
//    and persists to the settings storage.
//
// 4. NO WRITE-BACK TO DEVICE: For settings like operatingMode or chargeStopSoc, the
//    actual device (RTU/edge) needs to receive the new value via Modbus/MQTT command.
//    The update flow should be: API → DB → MQTT publish → device confirms → update status.
//
// PROPOSED SOLUTION:
// 1. Add a Device.Settings JSONB column (or a DeviceSettings table with DeviceId FK)
//    to store per-device configuration key-value pairs separately from Metadata.
//
// 2. Define default setting values in SpecificationsTemplate under a "settingsDefaults"
//    section. When reading, merge: template defaults ← device-specific overrides.
//    Example in template: { "settingsDefaults": { "tempAlarmThreshold": 45, "socLowAlarm": 10 } }
//
// 3. Create UpdateBessSettings command that:
//    - Validates keys exist in the template's settings schema
//    - Validates value types and ranges (e.g. SoC between 0-100)
//    - Writes to Device.Settings
//    - Optionally publishes MQTT command for real-time device config push
//
// 4. Populate Metadata with read-only device info (protocol, firmwareVersion, modbusAddress)
//    during device registration or from telemetry, keeping it separate from editable settings.

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.BessView)]
public record GetBessSettingsQuery : IRequest<SummaryResponseDto>
{
    public Guid UnitId { get; init; }
}

public class GetBessSettingsQueryHandler : IRequestHandler<GetBessSettingsQuery, SummaryResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ContainerTelemetryLoader _telemetryLoader;

    public GetBessSettingsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
        _telemetryLoader = new ContainerTelemetryLoader(context);
    }

    public async Task<SummaryResponseDto> Handle(GetBessSettingsQuery request, CancellationToken cancellationToken)
    {
        var device = await _context.Devices
            .Include(d => d.DeviceType)
            .Include(d => d.SpecificationTemplate)
            .FirstOrDefaultAsync(d => d.Id == request.UnitId, cancellationToken);

        Guard.Against.NotFound(request.UnitId, device);

        // TMS/HVAC devices get their own settings layout
        if (device.DeviceType?.Name == DeviceTypeNames.EnergyStorageTMS)
        {
            var sections = await BuildTmsSettings(device, cancellationToken);
            return new SummaryResponseDto { Sections = sections };
        }

        // Check if Specifications defines custom settings sections
        var specs = PayloadDeserializer.GetElement(device.SpecificationTemplate?.SpecificationsTemplate);
        if (specs.HasValue && specs.Value.TryGetProperty("tabs", out var tabs) &&
            tabs.TryGetProperty("settings", out var settingsTab) &&
            settingsTab.TryGetProperty("sections", out var sectionsElement) &&
            sectionsElement.ValueKind == JsonValueKind.Array)
        {
            return new SummaryResponseDto { Sections = BuildFromSchema(sectionsElement, device) };
        }

        var metadata = PayloadDeserializer.GetElement(device.Metadata);
        var sections2 = BuildDefaultSettings(device, metadata);
        return new SummaryResponseDto { Sections = sections2 };
    }

    private static List<DynamicSectionDto> BuildDefaultSettings(
        Domain.Entities.Device device, JsonElement? metadata)
    {
        return new List<DynamicSectionDto>
        {
            BuildGeneralSection(device, metadata),
            BuildAlarmThresholdsSection(metadata),
            BuildOperationSection(metadata),
            BuildMaintenanceSection(metadata),
        };
    }

    private static DynamicSectionDto BuildGeneralSection(Domain.Entities.Device device, JsonElement? metadata)
    {
        var protocol = metadata.HasValue ? PayloadDeserializer.ExtractValue(metadata.Value, "protocol") as string : null;
        var firmware = metadata.HasValue ? PayloadDeserializer.ExtractValue(metadata.Value, "firmwareVersion") as string : null;
        var modbusAddr = ExtractNum(metadata, "modbusAddress");

        var fields = new List<SectionFieldDto>
        {
            new() { Key = "deviceName", Label = "Device Name", Value = device.Name, Type = "string", Editable = true },
            new() { Key = "protocol", Label = "Protocol", Value = protocol, Type = "string", Editable = false },
            new() { Key = "firmwareVersion", Label = "Firmware Version", Value = firmware, Type = "string", Editable = false },
            new() { Key = "serialNumber", Label = "Serial Number", Value = device.SerialNumber, Type = "string", Editable = false },
        };

        if (modbusAddr.HasValue)
            fields.Add(new SectionFieldDto { Key = "modbusAddress", Label = "Modbus Address", Value = modbusAddr, Type = "number", Editable = true });

        return new DynamicSectionDto { Key = "general", Title = "General Settings", Fields = fields };
    }

    private static DynamicSectionDto BuildAlarmThresholdsSection(JsonElement? metadata)
    {
        return new DynamicSectionDto
        {
            Key = "alarms",
            Title = "Alarm Thresholds",
            Fields = new List<SectionFieldDto>
            {
                NumSetting("tempAlarmThreshold", "Temperature Alarm Threshold", ExtractNum(metadata, "tempAlarmThreshold"), "\u00b0C"),
                NumSetting("socLowAlarm", "SoC Low Alarm", ExtractNum(metadata, "socLowAlarm"), "%"),
                NumSetting("socHighAlarm", "SoC High Alarm", ExtractNum(metadata, "socHighAlarm"), "%"),
                NumSetting("insulationResistanceThreshold", "Insulation Resistance Threshold", ExtractNum(metadata, "insulationResistanceThreshold"), "k\u03a9"),
            }
        };
    }

    private static DynamicSectionDto BuildOperationSection(JsonElement? metadata)
    {
        var operatingMode = metadata.HasValue
            ? PayloadDeserializer.ExtractValue(metadata.Value, "operatingMode") as string
            : null;
        var balancingEnabled = metadata.HasValue
            ? PayloadDeserializer.ExtractValue(metadata.Value, "balancingEnabled")
            : null;
        var balancingBool = balancingEnabled switch { bool b => b, int i => i != 0, long l => l != 0, _ => (bool?)null };

        return new DynamicSectionDto
        {
            Key = "operation",
            Title = "Operation Settings",
            Fields = new List<SectionFieldDto>
            {
                new() { Key = "operatingMode", Label = "Operating Mode", Value = operatingMode ?? "auto", Type = "select",
                    Options = new List<string> { "auto", "manual", "maintenance" }, Editable = true },
                NumSetting("chargeStopSoc", "Charge Stop SoC", ExtractNum(metadata, "chargeStopSoc"), "%"),
                NumSetting("dischargeStopSoc", "Discharge Stop SoC", ExtractNum(metadata, "dischargeStopSoc"), "%"),
                NumSetting("socCalibrationIntervalHours", "SoC Calibration Interval", ExtractNum(metadata, "socCalibrationIntervalHours"), "h"),
                new() { Key = "balancingEnabled", Label = "Cell Balancing", Value = balancingBool, Type = "boolean", Editable = true },
                NumSetting("balancingThresholdMv", "Balancing Threshold", ExtractNum(metadata, "balancingThresholdMv"), "mV"),
            }
        };
    }

    private static DynamicSectionDto BuildMaintenanceSection(JsonElement? metadata)
    {
        return new DynamicSectionDto
        {
            Key = "maintenance",
            Title = "Maintenance",
            Fields = new List<SectionFieldDto>
            {
                NumSetting("maintenanceIntervalDays", "Maintenance Interval", ExtractNum(metadata, "maintenanceIntervalDays"), "days"),
            }
        };
    }

    // ── TMS/HVAC Settings ───────────────────────────────────────────────

    private async Task<List<DynamicSectionDto>> BuildTmsSettings(
        Domain.Entities.Device device, CancellationToken ct)
    {
        // Walk up to find root container (Battery.Thermal data is stored there)
        var rootId = device.Id;
        var currentId = device.ParentDeviceId;

        while (currentId.HasValue)
        {
            var parent = await _context.Devices
                .Include(d => d.DeviceType)
                .FirstOrDefaultAsync(d => d.Id == currentId.Value, ct);
            if (parent == null) break;
            rootId = parent.Id;
            currentId = parent.ParentDeviceId;
        }

        var telemetry = await _telemetryLoader.LoadLatest(rootId, ct);
        var payloads = telemetry.GetPayloadsByKind();
        var chiller = payloads.GetValueOrDefault("Battery.Thermal");
        var metadata = PayloadDeserializer.GetElement(device.Metadata);

        return new List<DynamicSectionDto>
        {
            BuildHvacGeneralSection(device, metadata),
            BuildTemperatureSettingsSection(chiller),
            BuildEquipmentSettingsSection(chiller, metadata),
        };
    }

    private static DynamicSectionDto BuildHvacGeneralSection(
        Domain.Entities.Device device, JsonElement? metadata)
    {
        var firmware = metadata.HasValue ? PayloadDeserializer.ExtractValue(metadata.Value, "firmware") as string : null;
        var protocol = metadata.HasValue ? PayloadDeserializer.ExtractValue(metadata.Value, "protocol") as string : null;
        var modbusAddress = ExtractNum(metadata, "modbusAddress");

        return new DynamicSectionDto
        {
            Key = "general",
            Title = "HVAC General Settings",
            Fields = new List<SectionFieldDto>
            {
                new() { Key = "deviceName", Label = "Device Name", Value = device.Name, Type = "string", Editable = false },
                new() { Key = "model", Label = "Model", Value = device.Model, Type = "string", Editable = false },
                new() { Key = "serialNumber", Label = "Serial Number", Value = device.SerialNumber, Type = "string", Editable = false },
                new() { Key = "firmware", Label = "Firmware", Value = firmware, Type = "string", Editable = false },
                new() { Key = "protocol", Label = "Protocol", Value = protocol, Type = "string", Editable = false },
                new() { Key = "modbusAddress", Label = "Modbus Address", Value = modbusAddress, Type = "number", Editable = true },
            }
        };
    }

    private static DynamicSectionDto BuildTemperatureSettingsSection(JsonElement? chiller)
    {
        var modeStr = chiller.HasValue
            ? PayloadDeserializer.ExtractValue(chiller.Value, "operatingModeStr") as string
            : null;

        return new DynamicSectionDto
        {
            Key = "temperature",
            Title = "Temperature Settings",
            Fields = new List<SectionFieldDto>
            {
                new() { Key = "operatingMode", Label = "Operating Mode", Value = modeStr, Type = "select",
                    Options = new List<string> { "Cooling", "Heating", "Standby", "Auto" }, Editable = true },
                new() { Key = "coolantTempSetpoint", Label = "Coolant Temp Setpoint", Value = ExtractNum(chiller, "coolantTempSetpointC"), Type = "number", Unit = "\u00b0C", Editable = true },
                new() { Key = "highTempAlarm", Label = "High Temp Alarm", Value = ExtractNum(chiller, "highAlarmTempC"), Type = "number", Unit = "\u00b0C", Editable = true },
                new() { Key = "lowTempAlarm", Label = "Low Temp Alarm", Value = ExtractNum(chiller, "lowAlarmTempC"), Type = "number", Unit = "\u00b0C", Editable = true },
            }
        };
    }

    private static DynamicSectionDto BuildEquipmentSettingsSection(JsonElement? chiller, JsonElement? metadata)
    {
        var compressorMaxSpeed = ExtractNum(chiller, "compressorSpeedPercent") ?? ExtractNum(metadata, "compressorMaxSpeed");
        var pumpMaxSpeed = ExtractNum(chiller, "pumpSpeedPercent") ?? ExtractNum(metadata, "pumpMaxSpeed");
        var autoDefrost = ExtractBool(chiller, "autoDefrost") ?? ExtractBool(metadata, "autoDefrost");

        return new DynamicSectionDto
        {
            Key = "equipment",
            Title = "Equipment Settings",
            Fields = new List<SectionFieldDto>
            {
                new() { Key = "compressorMaxSpeed", Label = "Compressor Max Speed", Value = compressorMaxSpeed, Type = "number", Unit = "%", Editable = true },
                new() { Key = "pumpMaxSpeed", Label = "Pump Max Speed", Value = pumpMaxSpeed, Type = "number", Unit = "%", Editable = true },
                new() { Key = "autoDefrost", Label = "Auto Defrost", Value = autoDefrost, Type = "boolean", Editable = true },
            }
        };
    }

    private static bool? ExtractBool(JsonElement? element, string path)
    {
        if (!element.HasValue) return null;
        var value = PayloadDeserializer.ExtractValue(element.Value, path);
        return value switch
        {
            bool b => b,
            int i => i != 0,
            long l => l != 0,
            double d => d != 0,
            _ => null
        };
    }

    // ── Schema-driven builder ─────────────────────────────────────────────

    private static List<DynamicSectionDto> BuildFromSchema(JsonElement sectionsElement, Domain.Entities.Device device)
    {
        var metadata = PayloadDeserializer.GetElement(device.Metadata);
        var sections = new List<DynamicSectionDto>();

        foreach (var section in sectionsElement.EnumerateArray())
        {
            var key = section.TryGetProperty("key", out var k) ? k.GetString() ?? "" : "";
            var title = section.TryGetProperty("title", out var t) ? t.GetString() ?? key : key;

            var fields = new List<SectionFieldDto>();
            if (section.TryGetProperty("fields", out var fieldsElement) &&
                fieldsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var field in fieldsElement.EnumerateArray())
                {
                    var fKey = field.TryGetProperty("key", out var fk) ? fk.GetString() ?? "" : "";
                    var fLabel = field.TryGetProperty("label", out var fl) ? fl.GetString() ?? fKey : fKey;
                    var fType = field.TryGetProperty("type", out var ft) ? ft.GetString() ?? "string" : "string";
                    var fUnit = field.TryGetProperty("unit", out var fu) ? fu.GetString() : null;
                    var editable = field.TryGetProperty("editable", out var fe) && fe.GetBoolean();
                    var sourcePath = field.TryGetProperty("sourcePath", out var sp) ? sp.GetString() : null;

                    // Resolve value: device fields, then metadata
                    object? value = fKey switch
                    {
                        "deviceName" => device.Name,
                        "serialNumber" => device.SerialNumber,
                        _ => metadata.HasValue && sourcePath != null
                            ? PayloadDeserializer.ExtractValue(metadata.Value, sourcePath)
                            : metadata.HasValue
                                ? PayloadDeserializer.ExtractValue(metadata.Value, fKey)
                                : null
                    };

                    List<string>? options = null;
                    if (field.TryGetProperty("options", out var opts) && opts.ValueKind == JsonValueKind.Array)
                    {
                        options = new List<string>();
                        foreach (var opt in opts.EnumerateArray())
                        {
                            var optStr = opt.GetString();
                            if (optStr != null) options.Add(optStr);
                        }
                    }

                    fields.Add(new SectionFieldDto
                    {
                        Key = fKey, Label = fLabel, Value = value, Type = fType,
                        Unit = fUnit, Editable = editable, Options = options, SourcePath = sourcePath
                    });
                }
            }

            sections.Add(new DynamicSectionDto { Key = key, Title = title, Fields = fields });
        }

        return sections;
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private static SectionFieldDto NumSetting(string key, string label, double? value, string? unit) =>
        new() { Key = key, Label = label, Value = value, Type = "number", Unit = unit, Editable = true };

    private static double? ExtractNum(JsonElement? element, string path)
    {
        if (!element.HasValue) return null;
        var value = PayloadDeserializer.ExtractValue(element.Value, path);
        return value switch { double d => d, int i => i, long l => l, _ => null };
    }
}
