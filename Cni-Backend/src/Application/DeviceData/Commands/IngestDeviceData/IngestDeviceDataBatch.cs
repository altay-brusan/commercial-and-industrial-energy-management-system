using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Prometheus;

namespace Backend.Application.DeviceData.Commands.IngestDeviceData;

public record IngestDeviceDataBatchCommand : IRequest<IngestBatchResultDto>
{
    public List<IngestDeviceDataCommand> Messages { get; init; } = new();
}

public record IngestBatchResultDto
{
    public int Total { get; init; }
    public int Succeeded { get; init; }
    public int Failed { get; init; }
}

public class IngestDeviceDataBatchCommandHandler : IRequestHandler<IngestDeviceDataBatchCommand, IngestBatchResultDto>
{
    private readonly ISender _sender;
    private readonly ILogger<IngestDeviceDataBatchCommandHandler> _logger;

    private static readonly Histogram BatchProcessing = Metrics.CreateHistogram(
        "cni_mqtt_batch_processing_seconds",
        "Duration of MQTT batch processing in seconds",
        new HistogramConfiguration
        {
            Buckets = Histogram.ExponentialBuckets(0.01, 2, 12)
        });

    public IngestDeviceDataBatchCommandHandler(ISender sender, ILogger<IngestDeviceDataBatchCommandHandler> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    public async Task<IngestBatchResultDto> Handle(IngestDeviceDataBatchCommand request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var succeeded = 0;
        var failed = 0;

        foreach (var message in request.Messages)
        {
            try
            {
                var result = await _sender.Send(message, cancellationToken);
                if (result)
                    succeeded++;
                else
                    failed++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to ingest message for RTU {RtuSn} Device {DeviceName}",
                    message.RtuSn, message.DeviceName);
                failed++;
            }
        }

        stopwatch.Stop();
        BatchProcessing.Observe(stopwatch.Elapsed.TotalSeconds);

        return new IngestBatchResultDto
        {
            Total = request.Messages.Count,
            Succeeded = succeeded,
            Failed = failed
        };
    }
}
