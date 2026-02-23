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
}

public record SectionFieldDto
{
    public string Key { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public object? Value { get; init; }
    public string Type { get; init; } = "string";
    public string? Unit { get; init; }
    public string? Format { get; init; }
    public string? Status { get; init; }
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
