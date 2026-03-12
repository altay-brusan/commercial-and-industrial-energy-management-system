using Backend.Domain.Enums;

namespace Backend.Infrastructure.Configurations;

public class AlmSettings
{
    public bool Enabled { get; set; }
    public string BaseUrl { get; set; } = string.Empty;
    public string CallbackBaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public int JobTimeoutMinutes { get; set; } = 30;
    public string CronSchedule { get; set; } = "1 0 * * *";

    /// <summary>
    /// Override default data days per module. Key = AlmJobType enum name (e.g. "PowerConsumption").
    /// If not specified, falls back to AlmJobTypeExtensions.GetDefaultDataDays().
    /// </summary>
    public Dictionary<string, int> ModuleDataDays { get; set; } = new();

    public int GetDataDays(AlmJobType jobType)
    {
        return ModuleDataDays.TryGetValue(jobType.ToString(), out var days)
            ? days
            : jobType.GetDefaultDataDays();
    }
}
