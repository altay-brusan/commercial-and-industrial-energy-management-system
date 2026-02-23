using System.Text.Json;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models.Monitoring;
using Backend.Application.Common.Security;
using Backend.Application.Common.Services;

namespace Backend.Application.BatteryStorage.Queries.GetBessSettings;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.BessView)]
public record GetBessSettingsQuery : IRequest<SummaryResponseDto>
{
    public Guid UnitId { get; init; }
}

public class GetBessSettingsQueryHandler : IRequestHandler<GetBessSettingsQuery, SummaryResponseDto>
{
    private readonly IApplicationDbContext _context;

    public GetBessSettingsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SummaryResponseDto> Handle(GetBessSettingsQuery request, CancellationToken cancellationToken)
    {
        var device = await _context.Devices
            .Include(d => d.DeviceType)
            .FirstOrDefaultAsync(d => d.Id == request.UnitId, cancellationToken);

        Guard.Against.NotFound(request.UnitId, device);

        // Check if Specifications defines custom settings sections
        var specs = PayloadDeserializer.GetElement(device.Specifications);
        if (specs.HasValue && specs.Value.TryGetProperty("tabs", out var tabs) &&
            tabs.TryGetProperty("settings", out var settingsTab) &&
            settingsTab.TryGetProperty("sections", out var sectionsElement) &&
            sectionsElement.ValueKind == JsonValueKind.Array)
        {
            return new SummaryResponseDto { Sections = BuildFromSchema(sectionsElement, device) };
        }

        var metadata = PayloadDeserializer.GetElement(device.Metadata);
        var sections = BuildDefaultSettings(device, metadata);
        return new SummaryResponseDto { Sections = sections };
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
