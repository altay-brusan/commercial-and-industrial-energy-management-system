using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Constants;
using Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Alm.Queries.GetLatestPrediction;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.AnalyticsView)]
public record GetLatestPredictionQuery : IRequest<AlmPredictionDto?>
{
    public Guid FacilityId { get; init; }
    public AlmJobType JobType { get; init; }
}

public class GetLatestPredictionQueryHandler : IRequestHandler<GetLatestPredictionQuery, AlmPredictionDto?>
{
    private readonly IApplicationDbContext _context;

    public GetLatestPredictionQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AlmPredictionDto?> Handle(GetLatestPredictionQuery request, CancellationToken cancellationToken)
    {
        var job = await _context.AlmPredictionJobs
            .Where(j => j.FacilityId == request.FacilityId &&
                        j.JobType == request.JobType &&
                        j.Status == AlmJobStatus.Completed)
            .OrderByDescending(j => j.CompletedAtUtc)
            .Select(j => new AlmPredictionDto
            {
                Id = j.Id,
                FacilityId = j.FacilityId,
                JobType = j.JobType,
                Status = j.Status,
                ExternalJobId = j.ExternalJobId,
                DataPointCount = j.DataPointCount,
                SubmittedAtUtc = j.SubmittedAtUtc,
                CompletedAtUtc = j.CompletedAtUtc,
                ForecastPoints = j.ForecastPoints
                    .OrderBy(p => p.Step)
                    .Select(p => new AlmForecastPointDto
                    {
                        Timestamp = p.Timestamp,
                        Step = p.Step,
                        Value = p.Value
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        return job;
    }
}

public record AlmPredictionDto
{
    public Guid Id { get; init; }
    public Guid FacilityId { get; init; }
    public AlmJobType JobType { get; init; }
    public AlmJobStatus Status { get; init; }
    public string? ExternalJobId { get; init; }
    public int DataPointCount { get; init; }
    public DateTimeOffset? SubmittedAtUtc { get; init; }
    public DateTimeOffset? CompletedAtUtc { get; init; }
    public List<AlmForecastPointDto> ForecastPoints { get; init; } = new();
}

public record AlmForecastPointDto
{
    public DateTimeOffset Timestamp { get; init; }
    public int Step { get; init; }
    public string Value { get; init; } = string.Empty;
}
