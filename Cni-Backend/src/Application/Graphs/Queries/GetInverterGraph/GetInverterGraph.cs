using System.Text.Json;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.Graphs.Queries.GetInverterGraph;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.MonitoringView)]
public record GetInverterGraphQuery : IRequest<List<InverterGraphDto>>
{
    public Guid Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class GetInverterGraphQueryHandler : IRequestHandler<GetInverterGraphQuery, List<InverterGraphDto>>
{
    private readonly IApplicationDbContext _context;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public GetInverterGraphQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<InverterGraphDto>> Handle(GetInverterGraphQuery request, CancellationToken cancellationToken)
    {
        var endDate = request.EndDate ?? DateTime.UtcNow;

        var pcsData = await _context.DevicePcsData
            .Where(f => f.DeviceId == request.Id &&
                       f.Created >= request.StartDate &&
                       f.Created <= endDate &&
                       (f.Created.Minute % 5) == 0)
            .OrderBy(f => f.Created)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var result = pcsData.Select(data =>
        {
            var payload = DeserializePayload(data.Payload);
            return new InverterGraphDto
            {
                Time = data.DeviceLocalTime,
                PcsId = data.DeviceId,
                ActivePower = payload?.AcPower?.TotalActivePower,
                ReactivePower = payload?.AcPower?.TotalReactivePower,
                PowerFactor = payload?.AcPower?.TotalPowerFactor,
                AcVoltage = payload?.AcPower?.VoltageRms,
                AcCurrent = payload?.AcPower?.CurrentRms,
                DcVoltage = payload?.DcPower?.Voltage,
                DcCurrent = payload?.DcPower?.Current,
                DcPower = payload?.DcPower?.Power,
                Temperature = payload?.TemperaturePcs
            };
        }).ToList();

        return result;
    }

    private static PcsPayload? DeserializePayload(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload) || payload == "{}")
            return null;

        try
        {
            return JsonSerializer.Deserialize<PcsPayload>(payload, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private class PcsPayload
    {
        public DcPowerDto? DcPower { get; set; }
        public AcPowerDto? AcPower { get; set; }
        public double? TemperaturePcs { get; set; }
    }

    private class DcPowerDto
    {
        public double? Voltage { get; set; }
        public double? Current { get; set; }
        public double? Power { get; set; }
    }

    private class AcPowerDto
    {
        public double? TotalActivePower { get; set; }
        public double? TotalReactivePower { get; set; }
        public double? TotalPowerFactor { get; set; }
        public double? VoltageRms { get; set; }
        public double? CurrentRms { get; set; }
    }
}
