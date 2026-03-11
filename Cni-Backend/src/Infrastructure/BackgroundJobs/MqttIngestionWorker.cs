using System.Text;
using System.Text.Json;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Services;
using Backend.Application.DeviceData.Commands.IngestDeviceAlarm;
using Backend.Application.DeviceData.Commands.IngestDeviceData;
using Backend.Infrastructure.Configurations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;

namespace Backend.Infrastructure.BackgroundJobs;

/// <summary>
/// BackgroundService that subscribes to EMQX via MQTTnet and routes incoming messages
/// through MediatR. Telemetry messages are sent as IngestDeviceDataCommand; alarm messages
/// are sent as IngestDeviceAlarmCommand. Topics use the format cni/{facilityId}/{rtuId}/{channel}.
/// </summary>
public class MqttIngestionWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly MqttSettings _settings;
    private readonly ILogger<MqttIngestionWorker> _logger;
    private readonly IMemoryCache _cache;
    private IMqttClient? _mqttClient;

    private static readonly MemoryCacheEntryOptions RtuCacheOptions = new()
    {
        SlidingExpiration = TimeSpan.FromMinutes(5)
    };

    public MqttIngestionWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<MqttSettings> settings,
        ILogger<MqttIngestionWorker> logger,
        IMemoryCache cache)
    {
        _scopeFactory = scopeFactory;
        _settings = settings.Value;
        _logger = logger;
        _cache = cache;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation("MqttIngestionWorker is disabled via configuration");
            return;
        }

        if (_settings.TopicSubscriptions.Length == 0)
        {
            _logger.LogWarning("MqttIngestionWorker has no topic subscriptions configured");
            return;
        }

        _logger.LogInformation(
            "MqttIngestionWorker starting. Broker: {Host}:{Port}, Topics: [{Topics}], ClientId: {ClientId}",
            _settings.Host, _settings.Port,
            string.Join(", ", _settings.TopicSubscriptions),
            _settings.ClientId + "-ingestion");

        var factory = new MqttFactory();
        _mqttClient = factory.CreateMqttClient();

        _mqttClient.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;
        _mqttClient.DisconnectedAsync += e =>
        {
            if (e.ClientWasConnected)
            {
                _logger.LogWarning(e.Exception, "MqttIngestionWorker disconnected from broker");
            }
            return Task.CompletedTask;
        };

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
                _logger.LogError(ex, "MqttIngestionWorker connection attempt failed. Retrying in {Delay}s",
                    _settings.ReconnectDelaySeconds);
            }

            await Task.Delay(TimeSpan.FromSeconds(_settings.ReconnectDelaySeconds), stoppingToken);
        }
    }

    private async Task ConnectAndSubscribeAsync(CancellationToken cancellationToken)
    {
        var optionsBuilder = new MqttClientOptionsBuilder()
            .WithTcpServer(_settings.Host, _settings.Port)
            .WithClientId(_settings.ClientId + "-ingestion");

        if (_settings.CleanSession)
        {
            optionsBuilder.WithCleanSession();
        }

        if (!string.IsNullOrEmpty(_settings.Username))
        {
            optionsBuilder.WithCredentials(_settings.Username, _settings.Password);
        }

        if (_settings.UseTls)
        {
            optionsBuilder.WithTlsOptions(o => o.UseTls());
        }

        await _mqttClient!.ConnectAsync(optionsBuilder.Build(), cancellationToken);
        _logger.LogInformation("MqttIngestionWorker connected to broker {Host}:{Port}", _settings.Host, _settings.Port);

        var subscribeOptionsBuilder = new MqttClientSubscribeOptionsBuilder();
        foreach (var topic in _settings.TopicSubscriptions)
        {
            subscribeOptionsBuilder.WithTopicFilter(topic, MqttQualityOfServiceLevel.AtLeastOnce);
        }

        await _mqttClient.SubscribeAsync(subscribeOptionsBuilder.Build(), cancellationToken);
        _logger.LogInformation("MqttIngestionWorker subscribed to {Count} topic(s): [{Topics}]",
            _settings.TopicSubscriptions.Length, string.Join(", ", _settings.TopicSubscriptions));
    }

    private async Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
    {
        try
        {
            var topic = e.ApplicationMessage.Topic;
            var parsed = MqttTopicParser.Parse(topic);

            if (parsed == null)
            {
                _logger.LogWarning("MqttIngestionWorker received message on unparseable topic: {Topic}", topic);
                return;
            }

            var parsedTopic = parsed.Value;
            var payloadBytes = e.ApplicationMessage.PayloadSegment;
            var json = Encoding.UTF8.GetString(payloadBytes);

            using var scope = _scopeFactory.CreateScope();

            var rtuSn = await ResolveRtuSerialNumberAsync(parsedTopic.RtuId, scope);
            if (rtuSn == null)
            {
                _logger.LogWarning("RTU not found for ID {RtuId} from topic {Topic}", parsedTopic.RtuId, topic);
                return;
            }

            switch (parsedTopic.Channel.ToLowerInvariant())
            {
                case "telemetry":
                    await HandleTelemetryAsync(json, rtuSn, scope);
                    break;

                case "alarm":
                    await HandleAlarmAsync(json, rtuSn, scope);
                    break;

                default:
                    _logger.LogDebug("MqttIngestionWorker ignoring unknown channel {Channel} on topic {Topic}",
                        parsedTopic.Channel, topic);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MqttIngestionWorker error processing message on topic {Topic}",
                e.ApplicationMessage.Topic);
        }
    }

    private static async Task HandleTelemetryAsync(string json, string rtuSn, IServiceScope scope)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var deviceName = root.GetProperty("deviceName").GetString() ?? string.Empty;
        var kind = root.GetProperty("kind").GetString() ?? string.Empty;
        var payload = root.GetProperty("payload").GetRawText();

        string? path = null;
        if (root.TryGetProperty("path", out var pathElement) && pathElement.ValueKind == JsonValueKind.String)
        {
            path = pathElement.GetString();
        }

        DateTimeOffset? deviceLocalTime = null;
        if (root.TryGetProperty("timestampUtc", out var timeElement) && timeElement.TryGetDateTimeOffset(out var parsedTime))
        {
            deviceLocalTime = parsedTime;
        }

        var command = new IngestDeviceDataCommand
        {
            RtuSn = rtuSn,
            DeviceName = deviceName,
            Kind = kind,
            Path = path,
            Payload = payload,
            DeviceLocalTime = deviceLocalTime
        };

        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        await sender.Send(command);
    }

    private static async Task HandleAlarmAsync(string json, string rtuSn, IServiceScope scope)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var deviceName = root.GetProperty("deviceName").GetString() ?? string.Empty;

        var alarms = new List<AlarmPayloadItem>();
        if (root.TryGetProperty("alarms", out var alarmsElement) && alarmsElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var alarmElement in alarmsElement.EnumerateArray())
            {
                var item = new AlarmPayloadItem
                {
                    Code = alarmElement.TryGetProperty("code", out var codeEl)
                        ? codeEl.GetString() ?? string.Empty
                        : string.Empty,
                    Active = !alarmElement.TryGetProperty("active", out var activeEl) || activeEl.GetBoolean(),
                    Value = alarmElement.TryGetProperty("value", out var valEl) && valEl.ValueKind == JsonValueKind.Number
                        ? valEl.GetDouble()
                        : null
                };
                alarms.Add(item);
            }
        }

        DateTimeOffset? timestamp = null;
        if (root.TryGetProperty("timestampUtc", out var tsElement) && tsElement.TryGetDateTimeOffset(out var parsedTs))
        {
            timestamp = parsedTs;
        }

        var command = new IngestDeviceAlarmCommand
        {
            RtuSn = rtuSn,
            DeviceName = deviceName,
            Alarms = alarms,
            DeviceLocalTime = timestamp
        };

        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        await sender.Send(command);
    }

    private async Task<string?> ResolveRtuSerialNumberAsync(Guid rtuId, IServiceScope scope)
    {
        var cacheKey = $"rtu:id-to-sn:{rtuId}";

        if (_cache.TryGetValue(cacheKey, out string? cachedSn))
            return cachedSn;

        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var serialNumber = await context.Rtus
            .IgnoreQueryFilters()
            .Where(r => r.Id == rtuId)
            .Select(r => r.RtuSerialNumber)
            .FirstOrDefaultAsync();

        if (serialNumber != null)
        {
            _cache.Set(cacheKey, serialNumber, RtuCacheOptions);
        }

        return serialNumber;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_mqttClient is { IsConnected: true })
        {
            _logger.LogInformation("MqttIngestionWorker disconnecting");
            await _mqttClient.DisconnectAsync(
                new MqttClientDisconnectOptionsBuilder().Build(), cancellationToken);
        }

        _mqttClient?.Dispose();
        await base.StopAsync(cancellationToken);
    }
}
