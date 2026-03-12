namespace Backend.Domain.Entities.Alm;

public class AlmForecastPoint
{
    public long Id { get; set; }
    public Guid AlmPredictionJobId { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public int Step { get; set; }
    public string Value { get; set; } = string.Empty;

    // Navigation property
    public AlmPredictionJob? AlmPredictionJob { get; set; }
}
