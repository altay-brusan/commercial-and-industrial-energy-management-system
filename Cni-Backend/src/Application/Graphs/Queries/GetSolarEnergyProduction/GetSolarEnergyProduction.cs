using System.Text.Json;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.Graphs.Queries.GetSolarEnergyProduction;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.SolarView)]
public record GetSolarEnergyProductionQuery : IRequest<SolarGraphDto?>
{
    public int Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class GetSolarEnergyProductionQueryHandler : IRequestHandler<GetSolarEnergyProductionQuery, SolarGraphDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IExternalDataService _externalDataService;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public GetSolarEnergyProductionQueryHandler(IApplicationDbContext context, IExternalDataService externalDataService)
    {
        _context = context;
        _externalDataService = externalDataService;
    }

    public async Task<SolarGraphDto?> Handle(GetSolarEnergyProductionQuery request, CancellationToken cancellationToken)
    {
        if (request.EndDate == null)
        {
            request.EndDate = DateTime.Now;
        }
        else
        {
            request.EndDate = request.EndDate.Value.ToLocalTime();
        }
        request.StartDate = request.StartDate.ToLocalTime();

        // Get solar data from new table and extract values from JSON payload
        var solarDataRaw = await _context.DeviceSolarData
            .Where(f => f.DeviceId == request.Id &&
                      f.Created >= request.StartDate &&
                      f.Created <= request.EndDate &&
                      (f.Created.Minute % 5) == 0)
            .Select(f => new { f.Created, f.Payload })
            .ToListAsync(cancellationToken);

        var realData = solarDataRaw.Select(f =>
        {
            var payload = DeserializePayload(f.Payload);
            return new TimeSeriesData
            {
                Timestamp = f.Created.ToLocalTime().DateTime,
                Value = payload?.DcOutput?.TotalEnergyDaily
            };
        }).ToList();

        var predictionData = (await _externalDataService.GetTimeSeriesDataAsync("external_solar_energy_prediction"))
            .ToList();

        var allMinutes = Enumerable.Range(0, (int)(request.EndDate!.Value - request.StartDate).TotalMinutes)
            .Select(i => request.StartDate.AddMinutes(i))
            .ToList();

        var result = new SolarGraphDto
        {
            DataPoints = allMinutes.Where(minute => minute.Minute % 15 == 0).Select(minute => new TimeDataPoint
            {
                Time = minute,
                Production = realData
                    .Where(r => r.Timestamp.Hour == minute.Hour && r.Timestamp.Minute == minute.Minute)
                    .Select(r => r.Value)
                    .FirstOrDefault(),
                Prediction = predictionData
                    .Where(p => p.Timestamp.Hour == minute.Hour && p.Timestamp.Minute == minute.Minute)
                    .Select(p => p.Value)
                    .FirstOrDefault()
            }).ToList()
        };

        return result;
    }

    private static SolarPayload? DeserializePayload(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload) || payload == "{}")
            return null;

        try
        {
            return JsonSerializer.Deserialize<SolarPayload>(payload, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private class SolarPayload
    {
        public DcPowerDto? DcOutput { get; set; }
    }

    private class DcPowerDto
    {
        public double? TotalEnergyDaily { get; set; }
        public double? Power { get; set; }
    }
}
