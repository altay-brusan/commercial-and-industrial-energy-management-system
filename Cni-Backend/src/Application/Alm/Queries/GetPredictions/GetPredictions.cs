using Backend.Application.Alm.Queries.GetLatestPrediction;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Constants;
using Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Alm.Queries.GetPredictions;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.AnalyticsView)]
public record GetPredictionsQuery : IRequest<List<AlmPredictionSummaryDto>>
{
    public Guid FacilityId { get; init; }
    public AlmJobType? JobType { get; init; }
    public AlmJobStatus? Status { get; init; }
    public int Limit { get; init; } = 20;
}

public class GetPredictionsQueryHandler : IRequestHandler<GetPredictionsQuery, List<AlmPredictionSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPredictionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AlmPredictionSummaryDto>> Handle(GetPredictionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.AlmPredictionJobs
            .Where(j => j.FacilityId == request.FacilityId);

        if (request.JobType.HasValue)
            query = query.Where(j => j.JobType == request.JobType.Value);

        if (request.Status.HasValue)
            query = query.Where(j => j.Status == request.Status.Value);

        return await query
            .OrderByDescending(j => j.Created)
            .Take(request.Limit)
            .Select(j => new AlmPredictionSummaryDto
            {
                Id = j.Id,
                FacilityId = j.FacilityId,
                JobType = j.JobType,
                Status = j.Status,
                ExternalJobId = j.ExternalJobId,
                ResultStatusCode = j.ResultStatusCode,
                ResultMessage = j.ResultMessage,
                DataPointCount = j.DataPointCount,
                ForecastPointCount = j.ForecastPoints.Count,
                SubmittedAtUtc = j.SubmittedAtUtc,
                CompletedAtUtc = j.CompletedAtUtc,
                Created = j.Created
            })
            .ToListAsync(cancellationToken);
    }
}

public record AlmPredictionSummaryDto
{
    public Guid Id { get; init; }
    public Guid FacilityId { get; init; }
    public AlmJobType JobType { get; init; }
    public AlmJobStatus Status { get; init; }
    public string? ExternalJobId { get; init; }
    public int? ResultStatusCode { get; init; }
    public string? ResultMessage { get; init; }
    public int DataPointCount { get; init; }
    public int ForecastPointCount { get; init; }
    public DateTimeOffset? SubmittedAtUtc { get; init; }
    public DateTimeOffset? CompletedAtUtc { get; init; }
    public DateTimeOffset Created { get; init; }
}
