using System.Text.Json;
using Backend.Application.Common.Interfaces;
using Backend.Domain.Entities.Alm;
using Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backend.Application.Alm.Commands.ProcessAlmCallback;

public record ProcessAlmCallbackCommand : IRequest<bool>
{
    public string JobId { get; init; } = string.Empty;
    public string JobStatus { get; init; } = string.Empty;
    public int? ResultStatusCode { get; init; }
    public string? ResultMessage { get; init; }
    public List<ProcessAlmForecastItem>? Forecast { get; init; }
}

public record ProcessAlmForecastItem
{
    public string Timestamp { get; init; } = string.Empty;
    public JsonElement Value { get; init; }
    public int Step { get; init; }
}

public class ProcessAlmCallbackCommandHandler : IRequestHandler<ProcessAlmCallbackCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ProcessAlmCallbackCommandHandler> _logger;

    public ProcessAlmCallbackCommandHandler(IApplicationDbContext context, ILogger<ProcessAlmCallbackCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> Handle(ProcessAlmCallbackCommand request, CancellationToken cancellationToken)
    {
        var job = await _context.AlmPredictionJobs
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(j => j.ExternalJobId == request.JobId, cancellationToken);

        if (job == null)
        {
            _logger.LogWarning("ALM callback received for unknown job {JobId}", request.JobId);
            return false;
        }

        var isSuccess = string.Equals(request.JobStatus, "completed", StringComparison.OrdinalIgnoreCase);
        job.Status = isSuccess ? AlmJobStatus.Completed : AlmJobStatus.Failed;
        job.CompletedAtUtc = DateTimeOffset.UtcNow;
        job.ResultStatusCode = request.ResultStatusCode;
        job.ResultMessage = request.ResultMessage;

        if (!isSuccess)
        {
            job.ErrorDetails = request.ResultMessage;
            _logger.LogWarning("ALM job {JobId} failed: {Message}", request.JobId, request.ResultMessage);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        // Store forecast points
        if (request.Forecast is { Count: > 0 })
        {
            foreach (var item in request.Forecast)
            {
                if (!DateTimeOffset.TryParse(item.Timestamp, out var timestamp))
                {
                    _logger.LogWarning("Could not parse timestamp {Timestamp} in job {JobId}",
                        item.Timestamp, request.JobId);
                    continue;
                }

                var point = new AlmForecastPoint
                {
                    AlmPredictionJobId = job.Id,
                    Timestamp = timestamp,
                    Step = item.Step,
                    Value = item.Value.GetRawText()
                };
                _context.AlmForecastPoints.Add(point);
            }

            _logger.LogInformation("ALM job {JobId} completed with {Count} forecast points",
                request.JobId, request.Forecast.Count);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
