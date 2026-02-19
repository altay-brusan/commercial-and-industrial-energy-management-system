namespace Backend.Application.Common.Models.Monitoring;

public record FaultFlagsResponseDto
{
    public List<FaultFlagDto> Flags { get; init; } = new();
    public List<FaultSourceDto>? Sources { get; init; }
    public List<FaultCategoryDto> Categories { get; init; } = new();
    public FaultFlagSummaryDto Summary { get; init; } = new();
}

public record FaultFlagDto
{
    public string Key { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public string? Source { get; init; }
    public string? Category { get; init; }
    public string Severity { get; init; } = "Warning";
}

public record FaultSourceDto
{
    public string Key { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public int ActiveCount { get; init; }
    public int TotalCount { get; init; }
}

public record FaultCategoryDto
{
    public string Key { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public int ActiveCount { get; init; }
    public int TotalCount { get; init; }
}

public record FaultFlagSummaryDto
{
    public int TotalFlags { get; init; }
    public int ActiveFlags { get; init; }
    public Dictionary<string, int>? ActiveBySource { get; init; }
    public Dictionary<string, int> ActiveByCategory { get; init; } = new();
}
