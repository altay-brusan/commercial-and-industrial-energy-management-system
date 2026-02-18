namespace Backend.Application.Common.Models.Monitoring;

public record DynamicSectionDto
{
    public string Key { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public int Order { get; init; }
    public List<SectionFieldDto> Fields { get; init; } = new();
    public List<SubsectionDto>? Subsections { get; init; }
}

public record SectionFieldDto
{
    public string Key { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public object? Value { get; init; }
    public string? Unit { get; init; }
    public string? Format { get; init; }
    public string? SourcePath { get; init; }
    public int Order { get; init; }
}

public record SubsectionDto
{
    public string Key { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public List<SectionFieldDto> Fields { get; init; } = new();
}
