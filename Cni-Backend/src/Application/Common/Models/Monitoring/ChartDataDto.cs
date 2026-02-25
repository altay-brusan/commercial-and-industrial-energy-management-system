using System.Text.Json.Serialization;

namespace Backend.Application.Common.Models.Monitoring;

public record ChartResponseDto
{
    public List<ChartDefinitionDto> Charts { get; init; } = new();
}

public record ChartDefinitionDto
{
    public string Key { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Type { get; init; } = "line";
    public bool? Optional { get; init; }
    public CurveXAxisDto XAxis { get; init; } = new();
    public List<CurveSeriesDto> Series { get; init; } = new();
    public List<ChartReferenceLineDto>? ReferenceLines { get; init; }
    public List<Dictionary<string, object?>> Data { get; init; } = new();
}

public record CurveXAxisDto
{
    public string Key { get; init; } = "time";
    public string Label { get; init; } = "Time";
    public string? Format { get; init; }
}

public record CurveSeriesDto
{
    public string Key { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string? Unit { get; init; }
    public string? Color { get; init; }
    public string? YAxis { get; init; }
    public string? Type { get; init; }

    /// <summary>
    /// Internal only — dot-notation path into the payload JSON.
    /// Not serialized to the API response.
    /// </summary>
    [JsonIgnore]
    public string? SourcePath { get; init; }
}
