using System.Text.Json;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models.Monitoring;
using Backend.Application.Common.Security;
using Backend.Application.Common.Services;
using Backend.Domain.Constants;

namespace Backend.Application.BatteryStorage.Queries.GetBessHealth;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.BessView)]
public record GetBessHealthQuery : IRequest<SummaryResponseDto>
{
    public Guid UnitId { get; init; }
}

public class GetBessHealthQueryHandler : IRequestHandler<GetBessHealthQuery, SummaryResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ContainerTelemetryLoader _telemetryLoader;

    public GetBessHealthQueryHandler(IApplicationDbContext context)
    {
        _context = context;
        _telemetryLoader = new ContainerTelemetryLoader(context);
    }

    public async Task<SummaryResponseDto> Handle(GetBessHealthQuery request, CancellationToken cancellationToken)
    {
        var device = await _context.Devices
            .Include(d => d.DeviceType)
            .Include(d => d.LogicalDevice)
            .Include(d => d.SpecificationTemplate)
            .FirstOrDefaultAsync(d => d.Id == request.UnitId, cancellationToken);

        Guard.Against.NotFound(request.UnitId, device);

        // Check if Specifications defines custom health sections
        var specs = PayloadDeserializer.GetElement(device.SpecificationTemplate?.SpecificationsTemplate);
        if (specs.HasValue && specs.Value.TryGetProperty("tabs", out var tabs) &&
            tabs.TryGetProperty("health", out var healthTab) &&
            healthTab.TryGetProperty("sections", out var sectionsElement) &&
            sectionsElement.ValueKind == JsonValueKind.Array)
        {
            var telemetry = await _telemetryLoader.LoadLatest(request.UnitId, cancellationToken);
            var mergedPayloads = telemetry.GetPayloadsByKind();
            var payload = mergedPayloads.Values.FirstOrDefault(p => p.HasValue);
            return new SummaryResponseDto { Sections = BuildFromSchema(sectionsElement, payload) };
        }

        // Default: build based on device type (or LogicalDevice category)
        var deviceTypeName = device.DeviceType?.Name;
        var sections = deviceTypeName switch
        {
            DeviceTypeNames.EnergyStorage => await BuildBatteryHealthSections(device, cancellationToken),
            _ when device.LogicalDevice?.Category == "BESS" => await BuildBatteryHealthSections(device, cancellationToken),
            _ => await BuildGenericHealthSections(device, cancellationToken)
        };

        return new SummaryResponseDto { Sections = sections };
    }

    // ── Battery Container Health ─────────────────────────────────────────

    private async Task<List<DynamicSectionDto>> BuildBatteryHealthSections(
        Domain.Entities.Device device, CancellationToken ct)
    {
        // Load multi-table telemetry for the container and all child devices
        var telemetry = await _telemetryLoader.LoadLatest(device.Id, ct);
        var payloads = telemetry.GetPayloadsByKind();

        var batt = payloads.GetValueOrDefault("ContainerBatteryState");
        var metadata = PayloadDeserializer.GetElement(device.Metadata);

        // Load monthly SoH history for trend chart (last 12 months of data)
        var twelveMonthsAgo = DateTimeOffset.UtcNow.AddMonths(-12);
        var monthlyHistory = await _context.DeviceBatteryData
            .Where(dd => dd.DeviceId == device.Id
                && dd.Kind == "ContainerBatteryState"
                && dd.DeviceLocalTime >= twelveMonthsAgo)
            .GroupBy(dd => new { dd.DeviceLocalTime.Year, dd.DeviceLocalTime.Month })
            .Select(g => new MonthlyPayloadEntry
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Payload = g.OrderByDescending(dd => dd.DeviceLocalTime).First().Payload
            })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync(ct);

        var sections = new List<DynamicSectionDto>();

        sections.Add(BuildHealthOverviewSection(batt, metadata));
        sections.Add(BuildSohTrendSection(monthlyHistory));
        sections.Add(BuildDegradationMetricsSection(batt, metadata));
        sections.Add(BuildEnergyThroughputSection(batt));
        sections.Add(BuildThermalStressSection(batt));
        sections.Add(BuildOperationalHistorySection(batt, metadata));
        sections.Add(BuildMaintenanceWarrantySection(metadata));

        return sections;
    }

    private static DynamicSectionDto BuildHealthOverviewSection(JsonElement? batt, JsonElement? metadata)
    {
        var soh = ExtractNum(batt, "sohPercent", "stateOfHealth", "soh");
        var cycleCount = ExtractNum(batt, "cycleCount", "equivalentFullCycles");
        var designedCycles = ExtractNum(metadata, "designedCycleLife") ?? 6000;
        var designedLifeYears = ExtractNum(metadata, "designedCalendarLife") ?? 15;

        var cycleLifeUsed = cycleCount.HasValue ? Math.Round(cycleCount.Value / designedCycles * 100, 1) : (double?)null;
        var rte = ExtractNum(batt, "roundTripEfficiency");

        // Calendar life: from commissioning date in metadata
        double? calendarLifeUsed = null;
        string? calendarDetail = null;
        var commDateStr = metadata.HasValue ? PayloadDeserializer.ExtractValue(metadata.Value, "commissioningDate") as string : null;
        if (commDateStr != null && DateTimeOffset.TryParse(commDateStr, out var commDate))
        {
            var yearsInService = (DateTimeOffset.UtcNow - commDate).TotalDays / 365.25;
            calendarLifeUsed = Math.Round(yearsInService / designedLifeYears * 100, 1);
            calendarDetail = $"{calendarLifeUsed}% of {designedLifeYears} year design life";
        }

        var estimatedRul = (soh.HasValue && soh.Value > 80 && calendarLifeUsed.HasValue)
            ? Math.Round(designedLifeYears * (1 - calendarLifeUsed.Value / 100), 1)
            : (double?)null;

        var fields = new List<SectionFieldDto>
        {
            new() { Key = "soh", Label = "State of Health", Value = soh, Type = "number", Unit = "%", Format = "progress",
                Meta = new Dictionary<string, object?> { ["thresholds"] = new[] {
                    new Dictionary<string, object?> { ["min"] = 90, ["color"] = "green" },
                    new Dictionary<string, object?> { ["min"] = 80, ["color"] = "orange" },
                    new Dictionary<string, object?> { ["min"] = 0, ["color"] = "red" }
                }}},
            new() { Key = "cycleLifeUsed", Label = "Cycle Life Used", Value = cycleLifeUsed, Type = "number", Unit = "%", Format = "progress",
                Meta = new Dictionary<string, object?> {
                    ["thresholds"] = new[] {
                        new Dictionary<string, object?> { ["min"] = 0, ["color"] = "green" },
                        new Dictionary<string, object?> { ["min"] = 40, ["color"] = "orange" },
                        new Dictionary<string, object?> { ["min"] = 70, ["color"] = "red" }
                    },
                    ["detail"] = cycleCount.HasValue ? $"{cycleCount.Value} / {designedCycles} cycles" : null
                }},
            new() { Key = "calendarLifeUsed", Label = "Calendar Life Used", Value = calendarLifeUsed, Type = "number", Unit = "%", Format = "progress",
                Meta = new Dictionary<string, object?> {
                    ["thresholds"] = new[] {
                        new Dictionary<string, object?> { ["min"] = 0, ["color"] = "green" },
                        new Dictionary<string, object?> { ["min"] = 40, ["color"] = "orange" },
                        new Dictionary<string, object?> { ["min"] = 70, ["color"] = "red" }
                    },
                    ["detail"] = calendarDetail
                }},
            new() { Key = "roundTripEfficiency", Label = "Round-Trip Efficiency", Value = rte, Type = "number", Unit = "%", Format = "progress",
                Meta = new Dictionary<string, object?> {
                    ["thresholds"] = new[] {
                        new Dictionary<string, object?> { ["min"] = 92, ["color"] = "green" },
                        new Dictionary<string, object?> { ["min"] = 88, ["color"] = "orange" },
                        new Dictionary<string, object?> { ["min"] = 0, ["color"] = "red" }
                    },
                    ["detail"] = rte.HasValue ? $"Initial: {ExtractNum(batt, "initialRte") ?? rte}%, Degradation: {ExtractNum(batt, "efficiencyDegradation") ?? 0}%" : null
                }},
            new() { Key = "estimatedRul", Label = "Estimated Remaining Useful Life", Value = estimatedRul, Type = "number", Unit = "years" },
        };

        return new DynamicSectionDto
        {
            Key = "healthOverview",
            Title = "Health Overview",
            Icon = "heart-pulse",
            Variant = "card",
            Columns = 2,
            Fields = fields
        };
    }

    private static DynamicSectionDto BuildSohTrendSection(
        List<MonthlyPayloadEntry> monthlyHistory)
    {
        var chartData = new List<Dictionary<string, object?>>();

        foreach (var entry in monthlyHistory)
        {
            var entryPayload = PayloadDeserializer.GetElement(entry.Payload);
            var soh = ExtractNum(entryPayload, "sohPercent", "stateOfHealth", "soh");
            if (!soh.HasValue) continue;

            var monthLabel = new DateTime(entry.Year, entry.Month, 1).ToString("MMM yy");
            chartData.Add(new Dictionary<string, object?>
            {
                ["month"] = monthLabel,
                ["soh"] = soh.Value
            });
        }

        return new DynamicSectionDto
        {
            Key = "sohTrend",
            Title = "SoH Degradation Trend (12 Months)",
            Icon = "trending-down",
            Variant = "chart",
            ChartConfig = new ChartConfigDto
            {
                Type = "area",
                XAxis = new ChartAxisDto { Key = "month", Label = "Month" },
                YAxis = new ChartAxisDto { Key = "soh", Label = "SoH", Unit = "%", Domain = new List<double> { 80, 100 } },
                ReferenceLines = new List<ChartReferenceLineDto>
                {
                    new() { Y = 80, Label = "EOL Threshold", Color = "red", StrokeDasharray = "5 5" }
                },
                Color = "#8b5cf6"
            },
            ChartData = chartData
        };
    }

    private static DynamicSectionDto BuildDegradationMetricsSection(JsonElement? batt, JsonElement? metadata)
    {
        return new DynamicSectionDto
        {
            Key = "degradationMetrics",
            Title = "Degradation Metrics",
            Icon = "bar-chart-3",
            Variant = "card",
            Columns = 3,
            Fields = new List<SectionFieldDto>
            {
                NumField("equivalentFullCycles", "Equivalent Full Cycles", ExtractNum(batt, "equivalentFullCycles", "cycleCount"), ""),
                NumField("designedCycleLife", "Designed Cycle Life", ExtractNum(metadata, "designedCycleLife"), "cycles"),
                NumField("averageDod", "Average DoD", ExtractNum(batt, "averageDod", "avgDepthOfDischarge"), "%"),
                NumField("totalCycleCount", "Total Cycle Count", ExtractNum(batt, "totalCycleCount", "cycleCount"), ""),
                NumField("currentSoh", "Current SoH", ExtractNum(batt, "sohPercent", "stateOfHealth", "soh"), "%"),
                NumField("designedCalendarLife", "Designed Calendar Life", ExtractNum(metadata, "designedCalendarLife"), "years"),
            }
        };
    }

    private static DynamicSectionDto BuildEnergyThroughputSection(JsonElement? batt)
    {
        return new DynamicSectionDto
        {
            Key = "energyThroughput",
            Title = "Energy Throughput",
            Icon = "zap",
            Variant = "card",
            Columns = 3,
            Fields = new List<SectionFieldDto>
            {
                NumField("lifetimeCharged", "Lifetime Charged", ExtractNum(batt, "lifetimeChargedMwh", "totalCharged"), "MWh"),
                NumField("lifetimeDischarged", "Lifetime Discharged", ExtractNum(batt, "lifetimeDischargedMwh", "totalDischarged"), "MWh"),
                NumField("rte", "RTE", ExtractNum(batt, "roundTripEfficiency"), "%"),
                NumField("initialRte", "Initial RTE", ExtractNum(batt, "initialRte"), "%"),
                NumField("efficiencyDegradation", "Efficiency Degradation", ExtractNum(batt, "efficiencyDegradation"), "%"),
                NumField("dailyCharged", "Daily Charged", ExtractNum(batt, "dailyChargedKwh", "dailyCharged"), "kWh"),
                NumField("dailyDischarged", "Daily Discharged", ExtractNum(batt, "dailyDischargedKwh", "dailyDischarged"), "kWh"),
            }
        };
    }

    private static DynamicSectionDto BuildThermalStressSection(JsonElement? batt)
    {
        return new DynamicSectionDto
        {
            Key = "thermalStress",
            Title = "Thermal Stress",
            Icon = "thermometer",
            Variant = "card",
            Columns = 2,
            Fields = new List<SectionFieldDto>
            {
                new() { Key = "maxRecordedTemp", Label = "Max Recorded Temp",
                    Value = ExtractNum(batt, "maxRecordedTempC", "maxCellTempC"),
                    Type = "number", Unit = "\u00b0C", Format = "temperature" },
                NumField("thermalExcursions", "Thermal Excursions", ExtractNum(batt, "thermalExcursions"), ""),
                new() { Key = "avgOperatingTemp", Label = "Avg Operating Temp",
                    Value = ExtractNum(batt, "avgOperatingTempC", "ambientTempC"),
                    Type = "number", Unit = "\u00b0C", Format = "temperature" },
            }
        };
    }

    private static DynamicSectionDto BuildOperationalHistorySection(JsonElement? batt, JsonElement? metadata)
    {
        var firmwareVersion = batt.HasValue
            ? PayloadDeserializer.ExtractValue(batt.Value, "firmwareVersion") as string
            : null;
        firmwareVersion ??= metadata.HasValue
            ? PayloadDeserializer.ExtractValue(metadata.Value, "firmwareVersion") as string
            : null;

        return new DynamicSectionDto
        {
            Key = "operationalHistory",
            Title = "Operational History",
            Icon = "clock",
            Variant = "card",
            Columns = 3,
            Fields = new List<SectionFieldDto>
            {
                NumField("totalOperatingHours", "Total Operating Hours", ExtractNum(batt, "totalOperatingHours"), "h"),
                NumField("totalStandbyHours", "Total Standby Hours", ExtractNum(batt, "totalStandbyHours"), "h"),
                new() { Key = "firmwareVersion", Label = "Firmware Version", Value = firmwareVersion, Type = "string" },
            }
        };
    }

    private static DynamicSectionDto BuildMaintenanceWarrantySection(JsonElement? metadata)
    {
        var lastMaint = metadata.HasValue ? PayloadDeserializer.ExtractValue(metadata.Value, "lastMaintenance") as string : null;
        var nextMaint = metadata.HasValue ? PayloadDeserializer.ExtractValue(metadata.Value, "nextMaintenanceDue") as string : null;
        var warrantyExpiry = metadata.HasValue ? PayloadDeserializer.ExtractValue(metadata.Value, "warrantyExpiry") as string : null;

        var warrantyActive = false;
        if (warrantyExpiry != null && DateTimeOffset.TryParse(warrantyExpiry, out var expiryDate))
            warrantyActive = expiryDate > DateTimeOffset.UtcNow;

        var fields = new List<SectionFieldDto>
        {
            new() { Key = "lastMaintenance", Label = "Last Maintenance", Value = lastMaint, Type = "string", Format = "date" },
            new() { Key = "nextMaintenanceDue", Label = "Next Maintenance Due", Value = nextMaint, Type = "string", Format = "date" },
            new() { Key = "warrantyExpiry", Label = "Warranty Expiry", Value = warrantyExpiry, Type = "string", Format = "date",
                Meta = warrantyExpiry != null ? new Dictionary<string, object?>
                {
                    ["badgeLabel"] = warrantyActive ? "Active" : "Expired",
                    ["badgeColor"] = warrantyActive ? "green" : "red"
                } : null },
        };

        return new DynamicSectionDto
        {
            Key = "maintenanceWarranty",
            Title = "Maintenance & Warranty",
            Icon = "wrench",
            Variant = "highlighted",
            Columns = 3,
            Fields = fields
        };
    }

    // ── Generic Health (non-Battery devices) ──────────────────────────────

    private async Task<List<DynamicSectionDto>> BuildGenericHealthSections(
        Domain.Entities.Device device, CancellationToken ct)
    {
        var latestData = await _context.DeviceBatteryData
            .Where(dd => dd.DeviceId == device.Id)
            .OrderByDescending(dd => dd.DeviceLocalTime)
            .FirstOrDefaultAsync(ct);

        var payload = PayloadDeserializer.GetElement(latestData?.Payload);
        var metadata = PayloadDeserializer.GetElement(device.Metadata);

        var sections = new List<DynamicSectionDto>
        {
            BuildHealthOverviewSection(payload, metadata)
        };

        return sections;
    }

    // ── Schema-driven builder ─────────────────────────────────────────────

    private static List<DynamicSectionDto> BuildFromSchema(JsonElement sectionsElement, JsonElement? payload)
    {
        var sections = new List<DynamicSectionDto>();

        foreach (var section in sectionsElement.EnumerateArray())
        {
            var key = section.TryGetProperty("key", out var k) ? k.GetString() ?? "" : "";
            var title = section.TryGetProperty("title", out var t) ? t.GetString() ?? key : key;
            var icon = section.TryGetProperty("icon", out var ic) ? ic.GetString() : null;
            var variant = section.TryGetProperty("variant", out var v) ? v.GetString() ?? "card" : "card";
            var columns = section.TryGetProperty("columns", out var c) && c.TryGetInt32(out var cols) ? cols : 3;

            var fields = new List<SectionFieldDto>();
            if (section.TryGetProperty("fields", out var fieldsElement) &&
                fieldsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var field in fieldsElement.EnumerateArray())
                {
                    var fKey = field.TryGetProperty("key", out var fk) ? fk.GetString() ?? "" : "";
                    var fLabel = field.TryGetProperty("label", out var fl) ? fl.GetString() ?? fKey : fKey;
                    var fUnit = field.TryGetProperty("unit", out var fu) ? fu.GetString() : null;
                    var fFormat = field.TryGetProperty("format", out var ff) ? ff.GetString() : null;
                    var sourcePath = field.TryGetProperty("sourcePath", out var sp) ? sp.GetString() : null;

                    object? value = null;
                    if (payload.HasValue && !string.IsNullOrEmpty(sourcePath))
                        value = PayloadDeserializer.ExtractValue(payload.Value, sourcePath);

                    fields.Add(new SectionFieldDto
                    {
                        Key = fKey,
                        Label = fLabel,
                        Value = value,
                        Unit = fUnit,
                        Format = fFormat,
                        SourcePath = sourcePath
                    });
                }
            }

            sections.Add(new DynamicSectionDto
            {
                Key = key,
                Title = title,
                Icon = icon,
                Variant = variant,
                Columns = columns,
                Fields = fields
            });
        }

        return sections;
    }

    // ── Field helpers ─────────────────────────────────────────────────────

    private static SectionFieldDto NumField(string key, string label, double? value, string? unit) =>
        new() { Key = key, Label = label, Value = value, Type = "number", Unit = unit };

    private static double? ExtractNum(JsonElement? element, params string[] paths)
    {
        if (!element.HasValue) return null;
        foreach (var path in paths)
        {
            var value = PayloadDeserializer.ExtractValue(element.Value, path);
            var result = value switch
            {
                double d => d,
                int i => i,
                long l => l,
                _ => (double?)null
            };
            if (result.HasValue) return result;
        }
        return null;
    }

    private sealed class MonthlyPayloadEntry
    {
        public int Year { get; init; }
        public int Month { get; init; }
        public string? Payload { get; init; }
    }
}
