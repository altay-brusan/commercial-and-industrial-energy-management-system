using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Application.Graphs.Queries.GetSolarPowerProduction;

namespace Backend.Application.Graphs.Queries.GetSolarPowerProduction;

[Authorize(Roles = "Administrator,Tenant.Admin")]
public record GetSolarPowerProductionQuery : IRequest<SolarGraphDto?>
{
    public int Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class GetSolarPowerProductionQueryValidator : AbstractValidator<GetSolarPowerProductionQuery>
{
    public GetSolarPowerProductionQueryValidator()
    {
    }
}

public class GetSolarPowerProductionQueryHandler : IRequestHandler<GetSolarPowerProductionQuery, SolarGraphDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IExternalDataService _externalDataService;
    public GetSolarPowerProductionQueryHandler(IApplicationDbContext context, IMapper mapper, IExternalDataService externalDataService)
    {
        _context = context;
        _mapper = mapper;
        _externalDataService = externalDataService;
    }

    public async Task<SolarGraphDto?> Handle(GetSolarPowerProductionQuery request, CancellationToken cancellationToken)
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
        var realData = await _context.SolarDatas
              .Where(f => f.SolarId == request.Id &&
                        f.Created >= request.StartDate &&
                 f.Created <= request.EndDate &&
                (f.Created.Minute % 5) == 0)
              .Select(f => new TimeSeriesData
              {
                  Timestamp = f.Created.ToLocalTime().DateTime,
                  Value = f.DCOutput.Power
              })
              .ToListAsync(cancellationToken);


        var predictionData = (await _externalDataService.GetTimeSeriesDataAsync("external_solar_power_prediction"))
            .ToList();

        // Tüm dakikaları içeren bir zaman listesi oluştur
        var allMinutes = Enumerable.Range(0, (int)(request.EndDate!.Value - request.StartDate).TotalMinutes)
             .Select(i => request.StartDate.AddMinutes(i))
             .ToList();
        // Sonuçları oluştur
        var result = new SolarGraphDto
        {
            DataPoints = allMinutes.Where(minute => minute.Minute % 15 == 0).Select(minute => new TimeDataPoint
            {
                Time = minute,
                Production = realData
                    .Where(r => r.Timestamp.Hour == minute.Hour&&r.Timestamp.Minute==minute.Minute)
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
}
