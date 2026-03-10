using System.Text;
using Backend.Application.Common.Interfaces;
using Backend.Infrastructure.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;

namespace Backend.Infrastructure.Services;

public class MqttPublisherService : IMqttPublisherService, IDisposable
{
    private readonly MqttSettings _settings;
    private readonly ILogger<MqttPublisherService> _logger;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private IMqttClient? _mqttClient;
    private bool _disposed;

    public MqttPublisherService(
        IOptions<MqttSettings> settings,
        ILogger<MqttPublisherService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task PublishAsync(string topic, string jsonPayload, CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled)
        {
            _logger.LogDebug("MQTT publisher is disabled, skipping publish to {Topic}", topic);
            return;
        }

        await EnsureConnectedAsync(cancellationToken);

        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(Encoding.UTF8.GetBytes(jsonPayload))
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .WithRetainFlag(false)
            .Build();

        await _mqttClient!.PublishAsync(message, cancellationToken);
        _logger.LogDebug("Published MQTT message to {Topic} ({Bytes} bytes)", topic, jsonPayload.Length);
    }

    public async Task PublishToManyAsync(IEnumerable<(string topic, string payload)> messages, CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled)
        {
            _logger.LogDebug("MQTT publisher is disabled, skipping batch publish");
            return;
        }

        await EnsureConnectedAsync(cancellationToken);

        var published = 0;
        var failed = 0;

        foreach (var (topic, payload) in messages)
        {
            try
            {
                var message = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(Encoding.UTF8.GetBytes(payload))
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                    .WithRetainFlag(false)
                    .Build();

                await _mqttClient!.PublishAsync(message, cancellationToken);
                published++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to publish to {Topic}", topic);
                failed++;
            }
        }

        _logger.LogInformation("MQTT batch publish complete: {Published} sent, {Failed} failed", published, failed);
    }

    private async Task EnsureConnectedAsync(CancellationToken cancellationToken)
    {
        if (_mqttClient is { IsConnected: true })
        {
            try
            {
                if (await _mqttClient.TryPingAsync(cancellationToken))
                    return;
            }
            catch
            {
                // Connection is stale, reconnect below
            }
        }

        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            // Double-check after acquiring lock
            if (_mqttClient is { IsConnected: true })
            {
                if (await _mqttClient.TryPingAsync(cancellationToken))
                    return;
            }

            _mqttClient?.Dispose();
            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();

            var optionsBuilder = new MqttClientOptionsBuilder()
                .WithTcpServer(_settings.Host, _settings.Port)
                .WithClientId(_settings.PublisherClientId)
                .WithCleanSession();

            if (!string.IsNullOrEmpty(_settings.Username))
            {
                optionsBuilder.WithCredentials(_settings.Username, _settings.Password);
            }

            if (_settings.UseTls)
            {
                optionsBuilder.WithTlsOptions(o => o.UseTls());
            }

            await _mqttClient.ConnectAsync(optionsBuilder.Build(), cancellationToken);
            _logger.LogInformation("MQTT publisher connected to {Host}:{Port} as {ClientId}",
                _settings.Host, _settings.Port, _settings.PublisherClientId);
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_mqttClient is { IsConnected: true })
        {
            try
            {
                _mqttClient.DisconnectAsync(new MqttClientDisconnectOptionsBuilder().Build())
                    .GetAwaiter().GetResult();
            }
            catch
            {
                // Best-effort disconnect during disposal
            }
        }

        _mqttClient?.Dispose();
        _connectionLock.Dispose();
    }
}
