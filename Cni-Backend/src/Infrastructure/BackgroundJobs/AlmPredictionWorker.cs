using Backend.Application.Common.Interfaces;
using Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backend.Infrastructure.BackgroundJobs;

/// <summary>
/// Background worker that submits ALM prediction jobs for all enabled facilities.
/// Scheduled daily at midnight. For each AlmEnabled facility, submits a job
/// for every registered AlmJobType.
/// </summary>
public class AlmPredictionWorker
{
    private static readonly AlmJobType[] JobTypes = Enum.GetValues<AlmJobType>();

    private readonly ILogger<AlmPredictionWorker> _logger;
    private readonly IApplicationDbContext _context;
    private readonly IAlmJobSubmitter _submitter;

    public AlmPredictionWorker(
        ILogger<AlmPredictionWorker> logger,
        IApplicationDbContext context,
        IAlmJobSubmitter submitter)
    {
        _logger = logger;
        _context = context;
        _submitter = submitter;
    }

    /// <summary>
    /// Runs prediction jobs for all ALM-enabled facilities.
    /// Errors are collected and thrown as AggregateException for Hangfire retry.
    /// </summary>
    public async Task RunPredictions(CancellationToken ct = default)
    {
        _logger.LogInformation("Starting ALM prediction run for {JobTypeCount} module(s)", JobTypes.Length);

        var facilities = await _context.Facilities
            .IgnoreQueryFilters()
            .Where(f => f.AlmEnabled)
            .Select(f => new { f.Id, f.Name })
            .ToListAsync(ct);

        if (facilities.Count == 0)
        {
            _logger.LogInformation("No ALM-enabled facilities found, skipping");
            return;
        }

        _logger.LogInformation("Found {Count} ALM-enabled facilities", facilities.Count);

        var errors = new List<string>();

        foreach (var facility in facilities)
        {
            foreach (var jobType in JobTypes)
            {
                try
                {
                    await _submitter.SubmitJobAsync(facility.Id, jobType, ct);
                }
                catch (Exception ex)
                {
                    var msg = $"{jobType}/{facility.Name}: {ex.Message}";
                    _logger.LogError(ex, "Failed to submit {JobType} job for facility {FacilityName} ({FacilityId})",
                        jobType, facility.Name, facility.Id);
                    errors.Add(msg);
                }
            }
        }

        if (errors.Count > 0)
        {
            throw new AggregateException(
                $"ALM prediction failed for {errors.Count} job(s): {string.Join("; ", errors)}",
                errors.Select(e => new InvalidOperationException(e)));
        }

        _logger.LogInformation("ALM prediction run completed successfully");
    }
}
