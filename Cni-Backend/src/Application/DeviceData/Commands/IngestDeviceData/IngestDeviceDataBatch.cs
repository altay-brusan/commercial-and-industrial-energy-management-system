using Microsoft.Extensions.Logging;

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

    public IngestDeviceDataBatchCommandHandler(ISender sender, ILogger<IngestDeviceDataBatchCommandHandler> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    public async Task<IngestBatchResultDto> Handle(IngestDeviceDataBatchCommand request, CancellationToken cancellationToken)
    {
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

        return new IngestBatchResultDto
        {
            Total = request.Messages.Count,
            Succeeded = succeeded,
            Failed = failed
        };
    }
}
