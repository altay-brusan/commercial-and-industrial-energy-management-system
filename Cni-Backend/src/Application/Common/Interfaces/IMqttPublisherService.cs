namespace Backend.Application.Common.Interfaces;

public interface IMqttPublisherService
{
    Task PublishAsync(string topic, string jsonPayload, CancellationToken cancellationToken = default);
    Task PublishToManyAsync(IEnumerable<(string topic, string payload)> messages, CancellationToken cancellationToken = default);
}
