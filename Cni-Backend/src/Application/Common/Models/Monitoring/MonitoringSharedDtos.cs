namespace Backend.Application.Common.Models.Monitoring;

public record TabInfoDto
{
    public string Key { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public bool Enabled { get; init; } = true;
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
    public const string OneHour = "1h";
    public const string SixHours = "6h";
    public const string TwelveHours = "12h";
    public const string TwentyFourHours = "24h";
    public const string FortyEightHours = "48h";
    public const string SevenDays = "7d";
    public const string ThirtyDays = "30d";

    public static readonly string[] All = { OneHour, SixHours, TwelveHours, TwentyFourHours, FortyEightHours, SevenDays, ThirtyDays };
}

public static class MonitoringIntervals
{
    public const string OneMin = "1m";
    public const string FiveMin = "5m";
    public const string FifteenMin = "15m";
    public const string ThirtyMin = "30m";
    public const string OneHour = "1h";
    public const string OneDay = "1d";

    public static readonly string[] All = { OneMin, FiveMin, FifteenMin, ThirtyMin, OneHour, OneDay };
}
