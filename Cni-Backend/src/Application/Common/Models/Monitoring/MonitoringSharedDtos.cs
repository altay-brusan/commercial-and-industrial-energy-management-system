namespace Backend.Application.Common.Models.Monitoring;

public record TabInfoDto
{
    public string Key { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public bool IsEnabled { get; init; } = true;
    public int Order { get; init; }
}

public record SchemaResponseDto
{
    public List<ProductTypeSchemaDto> ProductTypes { get; init; } = new();
}

public record ProductTypeSchemaDto
{
    public string Key { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public List<string> Hierarchy { get; init; } = new();
    public List<string> SupportedTabs { get; init; } = new();
    public List<string> SummarySections { get; init; } = new();
    public List<string> HealthSections { get; init; } = new();
    public List<string> FaultSources { get; init; } = new();
    public List<string> CurveCharts { get; init; } = new();
    public Dictionary<string, object?> Features { get; init; } = new();
    public Dictionary<string, object?> ConnectedDevices { get; init; } = new();
}

public static class MonitoringPeriods
{
    public const string Day = "day";
    public const string Week = "week";
    public const string Month = "month";
    public const string Year = "year";

    public static readonly string[] All = { Day, Week, Month, Year };
}

public static class MonitoringIntervals
{
    public const string FiveMin = "5m";
    public const string FifteenMin = "15m";
    public const string OneHour = "1h";
    public const string OneDay = "1d";

    public static readonly string[] All = { FiveMin, FifteenMin, OneHour, OneDay };
}
