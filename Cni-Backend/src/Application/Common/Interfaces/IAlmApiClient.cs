using Backend.Application.Common.Models.Alm;

namespace Backend.Application.Common.Interfaces;

public interface IAlmApiClient
{
    Task<AlmSubmitJobResponse> SubmitJobAsync(string module, AlmSubmitJobRequest request, CancellationToken ct = default);
    Task<AlmJobStatusResponse> PollJobStatusAsync(string module, string jobId, CancellationToken ct = default);
}
