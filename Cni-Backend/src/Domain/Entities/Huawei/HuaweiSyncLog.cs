namespace Backend.Domain.Entities.Huawei;

public class HuaweiSyncLog
{
    public long Id { get; set; }
    public Guid HuaweiAccountId { get; set; }
    public string SyncType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int DeviceCount { get; set; }
    public int RecordsInserted { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTimeOffset StartedAtUtc { get; set; }
    public DateTimeOffset? CompletedAtUtc { get; set; }

    // Navigation
    public HuaweiAccount? HuaweiAccount { get; set; }
}
