using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backend.Application.Alm.Commands.TriggerPrediction;

[Authorize(Roles = "Administrator")]
public record TriggerPredictionCommand : IRequest<TriggerPredictionResult>
{
    public Guid FacilityId { get; init; }
    public AlmJobType JobType { get; init; }
}

public record TriggerPredictionResult
{
    public bool Success { get; init; }
    public Guid? JobId { get; init; }
    public string? ExternalJobId { get; init; }
    public string? Message { get; init; }
}

public class TriggerPredictionCommandHandler : IRequestHandler<TriggerPredictionCommand, TriggerPredictionResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IAlmJobSubmitter _submitter;
    private readonly ILogger<TriggerPredictionCommandHandler> _logger;

    public TriggerPredictionCommandHandler(
        IApplicationDbContext context,
        IAlmJobSubmitter submitter,
        ILogger<TriggerPredictionCommandHandler> logger)
    {
        _context = context;
        _submitter = submitter;
        _logger = logger;
    }

    public async Task<TriggerPredictionResult> Handle(TriggerPredictionCommand request, CancellationToken cancellationToken)
    {
        var facilityExists = await _context.Facilities
            .AnyAsync(f => f.Id == request.FacilityId, cancellationToken);

        if (!facilityExists)
        {
            return new TriggerPredictionResult { Success = false, Message = "Facility not found" };
        }

        try
        {
            var result = await _submitter.SubmitJobAsync(request.FacilityId, request.JobType, cancellationToken);

            return new TriggerPredictionResult
            {
                Success = result.Success,
                JobId = result.JobId,
                ExternalJobId = result.ExternalJobId,
                Message = result.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to trigger {JobType} prediction for facility {FacilityId}",
                request.JobType, request.FacilityId);
            return new TriggerPredictionResult
            {
                Success = false,
                Message = $"Failed to submit job: {ex.Message}"
            };
        }
    }
}
