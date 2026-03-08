namespace Backend.Domain.Entities.Huawei;

public class HuaweiRawDataLog
{
    public long Id { get; set; }
    public Guid HuaweiAccountId { get; set; }
    public string Endpoint { get; set; } = string.Empty;
    public string? RequestPayload { get; set; }
    public string? ResponsePayload { get; set; }
    public int HttpStatusCode { get; set; }
    public DateTimeOffset CollectedAtUtc { get; set; }

    // Navigation
    public HuaweiAccount? HuaweiAccount { get; set; }
}
