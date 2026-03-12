using Backend.Domain.Enums;

namespace Backend.Application.Common.Interfaces;

public record AlmSubmissionResult(bool Success, Guid? JobId, string? ExternalJobId, string? Message);

public interface IAlmJobSubmitter
{
    Task<AlmSubmissionResult> SubmitJobAsync(Guid facilityId, AlmJobType jobType, CancellationToken ct = default);
}
