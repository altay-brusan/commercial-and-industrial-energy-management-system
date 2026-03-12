namespace Backend.Domain.Enums;

public enum AlmJobType
{
    PowerConsumption = 0,
    PowerGeneration = 1
}

public static class AlmJobTypeExtensions
{
    /// <summary>
    /// Returns the ALM service module endpoint name for this job type.
    /// New modules: add enum value + mapping here.
    /// </summary>
    public static string GetModuleName(this AlmJobType jobType) => jobType switch
    {
        AlmJobType.PowerConsumption => "module-tsf-pc",
        AlmJobType.PowerGeneration => "module-tsf-pg",
        _ => throw new ArgumentOutOfRangeException(nameof(jobType), jobType, "Unknown ALM job type")
    };

    /// <summary>
    /// Returns the default number of historical data days needed for this job type.
    /// </summary>
    public static int GetDefaultDataDays(this AlmJobType jobType) => jobType switch
    {
        AlmJobType.PowerConsumption => 7,
        AlmJobType.PowerGeneration => 1,
        _ => 1
    };
}
