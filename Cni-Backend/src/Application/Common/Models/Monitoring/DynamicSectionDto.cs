using System.Text.Json.Serialization;

namespace Backend.Application.Common.Models.Monitoring;

public record DynamicSectionDto
{
    public string Key { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Icon { get; init; }
    public string Variant { get; init; } = "card";
    public int Columns { get; init; } = 3;
    public bool Optional { get; init; }
    public List<SectionFieldDto> Fields { get; init; } = new();
    public TableDataDto? TableData { get; init; }
    public ChartConfigDto? ChartConfig { get; init; }
    public List<Dictionary<string, object?>>? ChartData { get; init; }
}

public record ChartConfigDto
{
    public string Type { get; init; } = "line";
    public ChartAxisDto? XAxis { get; init; }
    public ChartAxisDto? YAxis { get; init; }
    public List<ChartReferenceLineDto>? ReferenceLines { get; init; }
    public string? Color { get; init; }
}

public record ChartAxisDto
{
    public string Key { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string? Unit { get; init; }
    public List<double>? Domain { get; init; }
}

public record ChartReferenceLineDto
{
    public double Y { get; init; }
    public string Label { get; init; } = string.Empty;
    public string? Color { get; init; }
    public string? StrokeDasharray { get; init; }
}

// TODO: Review SectionFieldDto field types for settings use cases.
//
// CURRENTLY SUPPORTED:
// - type: "string"   — plain text, editable or read-only
// - type: "number"   — numeric input with optional Unit
// - type: "boolean"  — single checkbox/toggle (Value is bool)
// - type: "select"   — single-select dropdown (Options: List<string>, Value is string)
//
// MISSING FOR SETTINGS:
// 1. RADIO BUTTONS: No explicit "radio" type. The frontend could render "select" with
//    a small options list as radio buttons (styling choice), but if the backend needs
//    to distinguish radio vs dropdown behavior, add type: "radio" with the same
//    Options property.
//
// 2. MULTI-SELECT / CHECKBOX GROUP: Options exists but Value is a single object?,
//    not a list. If settings need "pick multiple from a set" (e.g. enabled protocols,
//    notification channels), either:
//    a) Add type: "multiselect" and allow Value to be List<string>, or
//    b) Use multiple type: "boolean" fields (one per option)
//
// 3. DEFAULT VALUES: There is no "defaultValue" property. For settings, the template
//    should define defaults so the frontend can show "Reset to default" and distinguish
//    user-modified values from factory defaults.
//
// 4. VALIDATION CONSTRAINTS: No min/max/step for numbers, no minLength/maxLength for
//    strings, no pattern regex. Settings like SoC thresholds (0-100%) or temperature
//    limits need range validation. Consider adding a "constraints" dictionary in Meta:
//    Meta = { "min": 0, "max": 100, "step": 1 }
//
// Discuss with frontend team which types and constraints are needed before extending.
public record SectionFieldDto
{
    public string Key { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public object? Value { get; init; }
    public string Type { get; init; } = "string";
    public string? Unit { get; init; }
    public string? Format { get; init; }
    public string? Status { get; init; }
    public bool? Editable { get; init; }
    public List<string>? Options { get; init; }
    public Dictionary<string, object?>? Meta { get; init; }

    /// <summary>
    /// Internal only — dot-notation path into the payload JSON.
    /// Not serialized to the API response.
    /// </summary>
    [JsonIgnore]
    public string? SourcePath { get; init; }
}

public record TableDataDto
{
    public List<TableColumnDto> Columns { get; init; } = new();
    public List<Dictionary<string, object?>> Rows { get; init; } = new();
}

public record TableColumnDto
{
    public string Key { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Type { get; init; } = "string";
}

public record SummaryResponseDto
{
    public List<DynamicSectionDto> Sections { get; init; } = new();
}
