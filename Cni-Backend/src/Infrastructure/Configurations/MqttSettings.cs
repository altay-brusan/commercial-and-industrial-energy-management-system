namespace Backend.Infrastructure.Configurations;

public class MqttSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 1883;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ClientId { get; set; } = "cni-backend";
    public string PublisherClientId { get; set; } = "cni-backend-publisher";
    public string TopicPattern { get; set; } = "devices/+/telemetry";
    public bool Enabled { get; set; }
    public int ReconnectDelaySeconds { get; set; } = 5;
    public bool UseTls { get; set; }
}
