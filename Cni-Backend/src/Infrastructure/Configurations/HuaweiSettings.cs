namespace Backend.Infrastructure.Configurations;

public class HuaweiSettings
{
    public bool Enabled { get; set; }
    public int PollIntervalSeconds { get; set; } = 300;
    public int TokenCacheMinutes { get; set; } = 25;
    public int RawDataRetentionDays { get; set; } = 30;
    public string EncryptionKey { get; set; } = string.Empty;
}
