using System.Diagnostics;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models.Alm;
using Backend.Application.Common.Services;
using Backend.Domain.Entities.Alm;
using Backend.Domain.Enums;
using Backend.Infrastructure.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prometheus;

namespace Backend.Infrastructure.Alm;

public class AlmJobSubmitter : IAlmJobSubmitter
{
    private static readonly Histogram AlmRequestDuration = Metrics.CreateHistogram(
        "cni_alm_request_duration_seconds",
        "Duration of requests to ALM service",
        new HistogramConfiguration
        {
            LabelNames = ["endpoint", "status"],
            Buckets = Histogram.ExponentialBuckets(0.01, 2, 12)
        });

    private readonly IApplicationDbContext _context;
    private readonly IAlmApiClient _almClient;
    private readonly AlmDataPreparationService _dataPrep;
    private readonly AlmSettings _settings;
    private readonly ILogger<AlmJobSubmitter> _logger;

    public AlmJobSubmitter(
        IApplicationDbContext context,
        IAlmApiClient almClient,
        AlmDataPreparationService dataPrep,
        IOptions<AlmSettings> settings,
        ILogger<AlmJobSubmitter> logger)
    {
        _context = context;
        _almClient = almClient;
        _dataPrep = dataPrep;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<AlmSubmissionResult> SubmitJobAsync(Guid facilityId, AlmJobType jobType, CancellationToken ct = default)
    {
        var module = jobType.GetModuleName();
        var stopwatch = Stopwatch.StartNew();
        var statusLabel = "success";

        try
        {
            var dataDays = _settings.GetDataDays(jobType);

            var preparedData = await _dataPrep.PrepareDataAsync(facilityId, jobType, dataDays, ct);
            if (preparedData == null)
            {
                return new AlmSubmissionResult(false, null, null, $"Insufficient data for {jobType} prediction");
            }

            // Create job record
            var job = new AlmPredictionJob
            {
                FacilityId = facilityId,
                JobType = jobType,
                Status = AlmJobStatus.Queued,
                DataPointCount = preparedData.DataPointCount,
                SubmittedAtUtc = DateTimeOffset.UtcNow
            };
            _context.AlmPredictionJobs.Add(job);
            await _context.SaveChangesAsync(ct);

            // Submit to ALM
            var callbackBase = _settings.CallbackBaseUrl.TrimEnd('/');
            var request = new AlmSubmitJobRequest
            {
                Schema = preparedData.Schema,
                Data = preparedData.CsvData,
                CallbackUrl = $"{callbackBase}/api/v1/Alm/Callback"
            };

            var response = await _almClient.SubmitJobAsync(module, request, ct);

            // Update job with external ID
            job.ExternalJobId = response.JobId;
            job.Status = AlmJobStatus.Processing;
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Submitted {JobType} job for facility {FacilityId}: ExternalJobId={ExternalJobId}, DataPoints={DataPoints}",
                jobType, facilityId, response.JobId, preparedData.DataPointCount);

            return new AlmSubmissionResult(true, job.Id, response.JobId, "Job submitted successfully");
        }
        catch (Exception ex)
        {
            statusLabel = "error";
            _logger.LogError(ex, "Failed to submit {JobType} job for facility {FacilityId}", jobType, facilityId);
            throw;
        }
        finally
        {
            stopwatch.Stop();
            AlmRequestDuration
                .WithLabels(module, statusLabel)
                .Observe(stopwatch.Elapsed.TotalSeconds);
        }
    }
}
