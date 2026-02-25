using System.Text;
using System.Text.Json;
using Backend.Application.DeviceData.Commands.IngestDeviceData;
using Backend.Infrastructure.Configurations;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;

namespace Backend.Infrastructure.BackgroundJobs;

public class MqttSubscriberService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly MqttSettings _settings;
    private readonly ILogger<MqttSubscriberService> _logger;
    private IMqttClient? _mqttClient;

    public MqttSubscriberService(
        IServiceScopeFactory scopeFactory,
        IOptions<MqttSettings> settings,
        ILogger<MqttSubscriberService> logger)
    {
        _scopeFactory = scopeFactory;
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation("MQTT subscriber is disabled via configuration");
            return;
        }

        _logger.LogInformation(
            "MQTT subscriber starting. Broker: {Host}:{Port}, Topic: {Topic}, ClientId: {ClientId}",
            _settings.Host, _settings.Port, _settings.TopicPattern, _settings.ClientId);

        var factory = new MqttFactory();
        _mqttClient = factory.CreateMqttClient();

        _mqttClient.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;
        _mqttClient.DisconnectedAsync += e =>
        {
            if (e.ClientWasConnected)
            {
                _logger.LogWarning(e.Exception, "Disconnected from MQTT broker");
            }
            return Task.CompletedTask;
        };

        // Reconnection loop using TryPingAsync pattern
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (!await _mqttClient.TryPingAsync(stoppingToken))
                {
                    await ConnectAndSubscribeAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MQTT connection attempt failed. Retrying in {Delay}s", _settings.ReconnectDelaySeconds);
            }

            await Task.Delay(TimeSpan.FromSeconds(_settings.ReconnectDelaySeconds), stoppingToken);
        }
    }

    private async Task ConnectAndSubscribeAsync(CancellationToken cancellationToken)
    {
        var optionsBuilder = new MqttClientOptionsBuilder()
            .WithTcpServer(_settings.Host, _settings.Port)
            .WithClientId(_settings.ClientId)
            .WithCleanSession();

        if (!string.IsNullOrEmpty(_settings.Username))
        {
            optionsBuilder.WithCredentials(_settings.Username, _settings.Password);
        }

        if (_settings.UseTls)
        {
            optionsBuilder.WithTlsOptions(o => o.UseTls());
        }

        await _mqttClient!.ConnectAsync(optionsBuilder.Build(), cancellationToken);
        _logger.LogInformation("Connected to MQTT broker {Host}:{Port}", _settings.Host, _settings.Port);

        var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
            .WithTopicFilter(_settings.TopicPattern, MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();

        await _mqttClient.SubscribeAsync(subscribeOptions, cancellationToken);
        _logger.LogInformation("Subscribed to MQTT topic {Topic}", _settings.TopicPattern);
    }

    private async Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
    {
        try
        {
            var topic = e.ApplicationMessage.Topic;
            var segments = topic.Split('/');

            if (segments.Length < 3)
            {
                _logger.LogWarning("Unexpected MQTT topic format: {Topic}", topic);
                return;
            }

            var rtuSerialNumber = segments[1];
            var payloadBytes = e.ApplicationMessage.PayloadSegment;
            var json = Encoding.UTF8.GetString(payloadBytes);

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var deviceSerialNumber = root.GetProperty("deviceSerialNumber").GetString() ?? string.Empty;
            var kind = root.GetProperty("kind").GetString() ?? string.Empty;
            var payload = root.GetProperty("payload").GetRawText();

            DateTimeOffset? deviceLocalTime = null;
            if (root.TryGetProperty("deviceLocalTime", out var timeElement))
            {
                if (timeElement.TryGetDateTimeOffset(out var parsedTime))
                {
                    deviceLocalTime = parsedTime;
                }
            }

            var command = new IngestDeviceDataCommand
            {
                RtuSn = rtuSerialNumber,
                DeviceSn = deviceSerialNumber,
                Kind = kind,
                Payload = payload,
                DeviceLocalTime = deviceLocalTime
            };

            using var scope = _scopeFactory.CreateScope();
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            await sender.Send(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing MQTT message on topic {Topic}", e.ApplicationMessage.Topic);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_mqttClient is { IsConnected: true })
        {
            _logger.LogInformation("MQTT subscriber disconnecting");
            await _mqttClient.DisconnectAsync(new MqttClientDisconnectOptionsBuilder().Build(), cancellationToken);
        }

        _mqttClient?.Dispose();
        await base.StopAsync(cancellationToken);
    }
}
