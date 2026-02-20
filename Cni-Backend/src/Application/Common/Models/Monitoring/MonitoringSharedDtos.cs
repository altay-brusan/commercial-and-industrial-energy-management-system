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
    public Guid DeviceId { get; init; }
    public string ModelName { get; init; } = string.Empty;
    public string? Manufacturer { get; init; }
    public object? Specifications { get; init; }
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
