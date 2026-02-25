namespace Backend.Application.Common.Interfaces;

public interface IDeviceStatusService
{
    Task<string> GetStatusAsync(Guid deviceId, CancellationToken ct);
    Task<Dictionary<Guid, string>> GetStatusBatchAsync(IEnumerable<Guid> deviceIds, CancellationToken ct);
}
