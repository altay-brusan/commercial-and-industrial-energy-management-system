using Backend.Domain.Common;
using Backend.Domain.Enums;

namespace Backend.Domain.Entities.Alm;

public class AlmPredictionJob : BaseAuditableEntity
{
    public Guid FacilityId { get; set; }
    public AlmJobType JobType { get; set; }
    public AlmJobStatus Status { get; set; }
    public string? ExternalJobId { get; set; }
    public int? ResultStatusCode { get; set; }
    public string? ResultMessage { get; set; }
    public string? ErrorDetails { get; set; }
    public int DataPointCount { get; set; }
    public DateTimeOffset? SubmittedAtUtc { get; set; }
    public DateTimeOffset? CompletedAtUtc { get; set; }

    // Navigation properties
    public Facility? Facility { get; set; }
    public ICollection<AlmForecastPoint> ForecastPoints { get; set; } = new List<AlmForecastPoint>();
}
