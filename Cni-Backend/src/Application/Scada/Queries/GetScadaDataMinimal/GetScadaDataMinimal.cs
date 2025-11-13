using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Enums;
namespace Backend.Application.Scada.Queries.GetScadaDataMinimal;

public record GetScadaDataMinimalQuery : IRequest<ScadaDataMinimalDto?>
{
}

public class GetScadaDataMinimalQueryValidator : AbstractValidator<GetScadaDataMinimalQuery>
{
    public GetScadaDataMinimalQueryValidator()
    {
    }
}

public class GetScadaDataMinimalQueryHandler : IRequestHandler<GetScadaDataMinimalQuery, ScadaDataMinimalDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IVirtualClockService _virtualClockService;
    public GetScadaDataMinimalQueryHandler(IApplicationDbContext context, IMapper mapper, IVirtualClockService virtualClockService)
    {
        _context = context;
        _mapper = mapper;
        _virtualClockService = virtualClockService;
    }

    public async Task<ScadaDataMinimalDto?> Handle(GetScadaDataMinimalQuery request, CancellationToken cancellationToken)
    {
        var time = _virtualClockService.CurrentTime;


        var solarData = await _context.SolarDatas.Where(f => f.SolarId == 2).OrderBy(x => Math.Abs(EF.Functions.DateDiffSecond(x.Created, time)))
    .FirstOrDefaultAsync(cancellationToken);
        var bessData = await _context.BESSDatas.Where(f => f.BESSId == 1).OrderBy(x => Math.Abs(EF.Functions.DateDiffSecond(x.Created, time)))
    .FirstOrDefaultAsync(cancellationToken);
        var pcs = await _context.PCSDatas.Where(f => f.PCSId == 3).OrderBy(x => Math.Abs(EF.Functions.DateDiffSecond(x.Created, time)))
    .FirstOrDefaultAsync(cancellationToken);
        var facilityData = await _context.SmartMeterDatas.Where(f => f.SmartMeterId == 5).OrderBy(x => Math.Abs(EF.Functions.DateDiffSecond(x.Created, time)))
    .FirstOrDefaultAsync(cancellationToken);
        var gridData = await _context.SmartMeterDatas.Where(f => f.SmartMeterId == 4).OrderBy(x => Math.Abs(EF.Functions.DateDiffSecond(x.Created, time)))
    .FirstOrDefaultAsync(cancellationToken);
        var systemdata = await _context.GrpcFacilityDatas.Where(f => f.FacilityId == 6).OrderBy(x => Math.Abs(EF.Functions.DateDiffSecond(x.Created, time)))
    .FirstOrDefaultAsync(cancellationToken);


        var scadaminimaldata = new ScadaDataMinimalDto
        {
            Bess = new ScadaDataMinimalDto.BessDataMinimal
            {
                StateOfCharge = string.Format("{0:0} %", bessData!.StateOfCharge ?? 0.0),
                Temperature = string.Format("{0:0.0} ℃", bessData.BatteryTemperature ?? 0.0),
                Status = bessData.Status.OperatingMode.ToString(),
                ChargingStatus = bessData.ChargeStatus.ToString(),
            },
            Solar = new ScadaDataMinimalDto.SolarDataMinimal
            {
                Efficiency = string.Format("{0:0} %", solarData!.PerformanceRatio ?? 0.0),
                // Generation için değeri yukarı yuvarlayıp tam sayı yapıyoruz
                Generation = string.Format("{0} kW", Math.Ceiling(solarData.PanelPower.Power ?? 0.0)),
                Temperature = string.Format("{0:0.0} ℃", solarData.PvPanelTemperature ?? 0.0),
                // MpptdcPower için de aynısı
                MpptdcPower = string.Format("{0} kW", Math.Ceiling(solarData.DCOutput.Power ?? 0.0))
            },
            Pcs = new ScadaDataMinimalDto.PcsDataMinimal
            {
                // DcPower ve AcPower için de yukarı yuvarlıyoruz
                DcPower = string.Format("{0} kW", Math.Ceiling(pcs!.DcPower.Power ?? 0.0)),
                Efficiency = string.Format("{0:0} %", pcs.Efficiency ?? 0.0),
                Status = pcs.Status.OperatingMode.ToString(),
                AcPower = string.Format("{0} kW", Math.Ceiling(pcs.AcPower.TotalActivePower ?? 0.0))
            },
            Facility = new ScadaDataMinimalDto.FacilityDataMinimal
            {
                // Eğer PowerFactor da kW olarak gösteriliyor ve yukarı yuvarlama isteniyorsa:
                PowerFactor = string.Format("{0} kW", Math.Ceiling(facilityData!.AcPower.TotalPowerFactor ?? 0.0)),
                PowerConsumption = string.Format("{0} kW", Math.Ceiling(facilityData.AcPower.TotalActivePower ?? 0.0))
            },
            Timestamp = time.ToString("dd.MM.yyyy HH.mm"),
            IsDaytime = time.Hour >= 6 && time.Hour < 18
        };
        return scadaminimaldata;

    }
}
