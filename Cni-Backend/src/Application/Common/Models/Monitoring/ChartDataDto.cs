namespace Backend.Application.Common.Models.Monitoring;

public record ChartResponseDto
{
    public List<ChartDefinitionDto> Charts { get; init; } = new();
    public string Period { get; init; } = string.Empty;
    public string Interval { get; init; } = string.Empty;
}

public record ChartDefinitionDto
{
    public string Key { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string ChartType { get; init; } = "line";
    public XAxisDto XAxis { get; init; } = new();
    public List<ChartSeriesDto> Series { get; init; } = new();
}

public record ChartSeriesDto
{
    public string Key { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string? Unit { get; init; }
    public string? Color { get; init; }
    public string? SourcePath { get; init; }
    public List<object?> Data { get; init; } = new();
}

public record XAxisDto
{
    public string Type { get; init; } = "time";
    public List<string> Labels { get; init; } = new();
}
