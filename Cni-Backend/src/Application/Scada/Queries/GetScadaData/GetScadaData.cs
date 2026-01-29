using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Application.Devices.Queries;
using Backend.Domain.Entities;
using Backend.Domain.Entities.Proto.DTO;
using Backend.Domain.Enums;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Backend.Application.Scada.Queries.GetScadaData;


public record GetScadaQuery : IRequest<FacilityHubDto>
{
    public int Id { get; set; }
    public DateTime Time { get; set; }
}

public class GetScadaQueryValidator : AbstractValidator<GetScadaQuery>
{
    public GetScadaQueryValidator()
    {
    }
}

public class GetScadaQueryHandler : IRequestHandler<GetScadaQuery, FacilityHubDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IExternalDataService _externalDataService;
    public GetScadaQueryHandler(IApplicationDbContext context, IMapper mapper, IExternalDataService externalDataService)
    {
        _context = context;
        _mapper = mapper;
        _externalDataService = externalDataService;
    }

    public async Task<FacilityHubDto> Handle(GetScadaQuery request, CancellationToken cancellationToken)
    {
        var facilityHub = new FacilityHubDto();

        // Tüm ilgili cihazları çek
        var devices = await _context.Devices
            .Include(d => d.TopologyNode)
            .Include(d => d.DeviceType)
            .Include(d => d.ConnectedDevice)
                .ThenInclude(cd => cd!.DeviceType)
            .Include(d => d.ConnectedDevice)
                .ThenInclude(cd => cd!.TopologyNode)
            .IgnoreQueryFilters()
            .Where(d => d.FacilityId == request.Id && d.GrpcConnection)
            .ToListAsync(cancellationToken);

        foreach (var device in devices)
        {
            switch (device.DeviceTypeId)
            {
                case (int)DeviceTypeEnum.SolarPanel:
                    await ProcessSolarDevice(device, facilityHub, cancellationToken, request.Time);
                    break;
                case (int)DeviceTypeEnum.Bess:
                    await ProcessBessDevice(device, facilityHub, cancellationToken, request.Time);
                    break;
                case (int)DeviceTypeEnum.PCS:
                    await ProcessPCSDevice(device, facilityHub, cancellationToken, request.Time);
                    break;
                case (int)DeviceTypeEnum.SmartMeter:
                    await ProcessSmartMeterDevice(device, facilityHub, cancellationToken, request.Time);
                    break;
                case (int)DeviceTypeEnum.FacilitySmartMeter:
                    await ProcessFacilitySmartMeter(device, facilityHub, cancellationToken, request.Time);
                    break;
                case (int)DeviceTypeEnum.Generator:
                    await ProcessGeneratorDevice(device, facilityHub, cancellationToken, request.Time);
                    break;
            }
        }

        return facilityHub;
    }

    private async Task ProcessSolarDevice(Device device, FacilityHubDto hub, CancellationToken ct, DateTime time)
    {
        var latestData = await _context.SolarDatas
    .Where(x => x.SolarId == device.Id)
   .OrderBy(x => Math.Abs((x.Created - time).TotalSeconds))
    .FirstOrDefaultAsync(ct);

        if (latestData == null) return;

        var dto = _mapper.Map<SolarDeviceDto>(latestData);
        dto.DeviceId = device.Id;
        dto.NodeId = device.TopologyNode?.Id;

        dto.DeviceName = device.DeviceName;

        dto.Weather = _externalDataService.GetValueByDateTime("external_Weather_Temperature_C", time);
        dto.Irradiance = _externalDataService.GetValueByDateTime("external_Irradiance_wm2", time);
        hub.SolarDevices.Add(dto);
    }
    private async Task ProcessPCSDevice(Device device, FacilityHubDto hub, CancellationToken ct, DateTime time)
    {
        var latestData = await _context.PCSDatas
    .Where(x => x.PCSId == device.Id)
   .OrderBy(x => Math.Abs((x.Created - time).TotalSeconds))
    .FirstOrDefaultAsync(ct);

        if (latestData == null) return;

        var dto = _mapper.Map<PCSDeviceDto>(latestData);
        dto.DeviceId = device.Id;
        dto.NodeId = device.TopologyNode?.Id;
        dto.DeviceName = device.DeviceName;
        hub.PcsDevices.Add(dto);
    }
    private async Task ProcessSmartMeterDevice(Device device, FacilityHubDto hub, CancellationToken ct, DateTime time)
    {
        var latestData = await _context.SmartMeterDatas
    .Where(x => x.SmartMeterId == device.Id)
   .OrderBy(x => Math.Abs((x.Created - time).TotalSeconds))
    .FirstOrDefaultAsync(ct);

        if (latestData == null) return;



        if (device.ConnectedDevice != null)
        {
            if (device.ConnectedDevice.DeviceTypeId == (int)DeviceTypeEnum.Grid)
            {
                var deviceDto = _mapper.Map<GridPowerDto>(latestData);
                deviceDto.DeviceId = device.ConnectedDevice.Id;
                try
                {
                    var nodeID = device.ConnectedDevice.TopologyNode?.Id;
                    deviceDto.NodeId = nodeID;
                    deviceDto.DeviceName = device.ConnectedDevice.DeviceName;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    throw;
                }

                hub.GridDevices.RemoveAll(x => x.DeviceId == device.ConnectedDevice.Id);
                hub.GridDevices.Add(deviceDto);

            }
            else if (device.ConnectedDevice.DeviceTypeId == (uint)DeviceTypeEnum.Heat_Pump)
            {
                //şuanverielde olmadığı için yapmıyoruz
            }
            else if (device.ConnectedDevice.DeviceTypeId == (uint)DeviceTypeEnum.EV_Chanrger)
            {
                //şuanverielde olmadığı için yapmıyoruz
            }

        }
    }

    private async Task ProcessBessDevice(Device device, FacilityHubDto hub, CancellationToken ct, DateTime time)
    {
        var latestData = await _context.BESSDatas
            .Where(x => x.BESSId == device.Id)
   .OrderBy(x => Math.Abs((x.Created - time).TotalSeconds))
    .FirstOrDefaultAsync(ct);

        if (latestData == null) return;

        var dto = _mapper.Map<BESSDeviceDto>(latestData);
        dto.DeviceId = device.Id;
        dto.NodeId = device.TopologyNode?.Id;
        dto.DeviceName = device.DeviceName;

        hub.BessDevices.Add(dto);
    }

    // Diğer cihaz tipleri için benzer metodlar...

    private async Task ProcessFacilitySmartMeter(Device device, FacilityHubDto hub, CancellationToken ct, DateTime time)
    {
        var latestData = await _context.SmartMeterDatas
            .Where(x => x.SmartMeterId == device.Id)
   .OrderBy(x => Math.Abs((x.Created - time).TotalSeconds))
    .FirstOrDefaultAsync(ct);

        if (latestData == null) return;

        var dto = _mapper.Map<FacilityPowerDto>(latestData);
        dto.DeviceId = device.Id;
        dto.NodeId = device.TopologyNode?.Id;

        hub.FacilityData = dto;
    }

    private async Task ProcessGeneratorDevice(Device device, FacilityHubDto hub, CancellationToken ct, DateTime time)
    {
        var latestSystemData = await _context.GrpcFacilityDatas
            .Where(x => x.FacilityId == device.FacilityId)
   .OrderBy(x => Math.Abs((x.Created - time).TotalSeconds))
    .FirstOrDefaultAsync(ct);

        if (latestSystemData == null) return;

        var systemDto = _mapper.Map<SystemDataDto>(latestSystemData);
        systemDto.Cost_Saving_TL_Daily = _externalDataService.GetValueByDateTime("external_Cost_Saving_TL_Daily", time);
        systemDto.Grid_Consumption_TL_Daily = _externalDataService.GetValueByDateTime("external_Grid_Consumption_TL_Daily", time);

        hub.SystemData = systemDto;

        var generatorDto = new GeneratorDeviceDto
        {
            DeviceId = device.Id,
            LoadGeneratorContribution = systemDto.LoadGeneratorContribution,
            TotalPower = systemDto.GeneratorPower,
            NodeId = device.TopologyNode?.Id,
        };


        hub.GeneratorDevices.Add(generatorDto);
    }
}

