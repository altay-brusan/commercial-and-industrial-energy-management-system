using Backend.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Common.Services;

public class DeviceStatusService : IDeviceStatusService
{
    private readonly IApplicationDbContext _context;

    public DeviceStatusService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> GetStatusAsync(Guid deviceId, CancellationToken ct)
    {
        var result = await GetStatusBatchAsync([deviceId], ct);
        return result.GetValueOrDefault(deviceId, "normal");
    }

    public async Task<Dictionary<Guid, string>> GetStatusBatchAsync(IEnumerable<Guid> deviceIds, CancellationToken ct)
    {
        var idList = deviceIds.ToList();
        if (idList.Count == 0)
            return new Dictionary<Guid, string>();

        var activeAlarms = await _context.DeviceAlarms
            .Where(a => a.IsActive && idList.Contains(a.DeviceId))
            .Include(a => a.AlarmType)
            .Select(a => new { a.DeviceId, Severity = a.AlarmType!.Severity })
            .ToListAsync(ct);

        return activeAlarms
            .GroupBy(a => a.DeviceId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    if (g.Any(a => a.Severity == "Critical"))
                        return "fault";
                    if (g.Any(a => a.Severity is "Warning" or "Major"))
                        return "warning";
                    return "normal";
                });
    }
}
