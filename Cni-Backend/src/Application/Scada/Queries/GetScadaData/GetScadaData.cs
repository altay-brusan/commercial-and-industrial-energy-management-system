using System.Text.Json;
using Backend.Application.Common.Interfaces;
using Backend.Application.Scada.DTOs;
using Backend.Domain.Entities;
using Backend.Domain.Enums;

namespace Backend.Application.Scada.Queries.GetScadaData;

public record GetScadaQuery : IRequest<FacilityHubDto>
{
    public Guid Id { get; set; }
    public DateTime Time { get; set; }
}

public class GetScadaQueryHandler : IRequestHandler<GetScadaQuery, FacilityHubDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IExternalDataService _externalDataService;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public GetScadaQueryHandler(IApplicationDbContext context, IExternalDataService externalDataService)
    {
        _context = context;
        _externalDataService = externalDataService;
    }

    public async Task<FacilityHubDto> Handle(GetScadaQuery request, CancellationToken cancellationToken)
    {
        var facilityHub = new FacilityHubDto();

        // Get all devices for the facility
        var devices = await _context.Devices
            .Include(d => d.DeviceType)
            .IgnoreQueryFilters()
            .Where(d => d.FacilityId == request.Id)
            .ToListAsync(cancellationToken);

        foreach (var device in devices)
        {
            var deviceTypeName = device.DeviceType?.Name;
            switch (deviceTypeName)
            {
                case "Solar":
                    await ProcessSolarDevice(device, facilityHub, cancellationToken, request.Time);
                    break;
                case "Battery":
                    await ProcessBessDevice(device, facilityHub, cancellationToken, request.Time);
                    break;
                case "PCS":
                case "Inverter":
                    await ProcessPCSDevice(device, facilityHub, cancellationToken, request.Time);
                    break;
                case "Smart Meter":
                    await ProcessSmartMeterDevice(device, facilityHub, cancellationToken, request.Time);
                    break;
                case "Generator":
                    await ProcessGeneratorDevice(device, facilityHub, cancellationToken, request.Time);
                    break;
            }
        }

        return facilityHub;
    }

    private async Task ProcessSolarDevice(Device device, FacilityHubDto hub, CancellationToken ct, DateTime time)
    {
        var latestData = await _context.DeviceSolarData
            .Where(x => x.DeviceId == device.Id)
            .OrderBy(x => Math.Abs((x.Created - time).TotalSeconds))
            .FirstOrDefaultAsync(ct);

        if (latestData == null) return;

        var payload = DeserializePayload<SolarPayload>(latestData.Payload);
        var dto = new SolarDeviceDto
        {
            DeviceId = device.Id,
            DeviceName = device.Name,
            NodeId = null,
            Timestamp = latestData.DeviceLocalTime,
            PanelPower = payload?.PanelPower,
            DCOutput = payload?.DcOutput,
            PerformanceRatio = payload?.PerformanceRatio,
            PvPanelTemperature = payload?.PvPanelTemperature,
            SolarEnergyProduced = payload?.SolarEnergyProduced,
            Weather = _externalDataService.GetValueByDateTime("external_Weather_Temperature_C", time),
            Irradiance = _externalDataService.GetValueByDateTime("external_Irradiance_wm2", time),
            Status = payload?.Status
        };

        hub.SolarDevices.Add(dto);
    }

    private async Task ProcessPCSDevice(Device device, FacilityHubDto hub, CancellationToken ct, DateTime time)
    {
        var latestData = await _context.DevicePcsData
            .Where(x => x.DeviceId == device.Id)
            .OrderBy(x => Math.Abs((x.Created - time).TotalSeconds))
            .FirstOrDefaultAsync(ct);

        if (latestData == null) return;

        var payload = DeserializePayload<PcsPayload>(latestData.Payload);
        var dto = new PCSDeviceDto
        {
            DeviceId = device.Id,
            DeviceName = device.Name,
            NodeId = null,
            Timestamp = latestData.DeviceLocalTime,
            DcPower = payload?.DcPower,
            AcPower = payload?.AcPower,
            TemperaturePcs = payload?.TemperaturePcs,
            Efficiency = payload?.Efficiency,
            TotalEnergyDelivered = payload?.TotalEnergyDelivered,
            TotalEnergyAbsorbed = payload?.TotalEnergyAbsorbed,
            Status = payload?.Status
        };

        hub.PcsDevices.Add(dto);
    }

    private async Task ProcessSmartMeterDevice(Device device, FacilityHubDto hub, CancellationToken ct, DateTime time)
    {
        var latestData = await _context.DeviceSmartMeterData
            .Where(x => x.DeviceId == device.Id)
            .OrderBy(x => Math.Abs((x.Created - time).TotalSeconds))
            .FirstOrDefaultAsync(ct);

        if (latestData == null) return;

        var payload = DeserializePayload<SmartMeterPayload>(latestData.Payload);
        var dto = new GridPowerDto
        {
            DeviceId = device.Id,
            DeviceName = device.Name,
            NodeId = null,
            Timestamp = latestData.DeviceLocalTime,
            AcPower = payload?.AcPower,
            ActiveEnergyImport = payload?.ActiveEnergyImport,
            ActiveEnergyExport = payload?.ActiveEnergyExport,
            Status = payload?.Status
        };

        hub.GridDevices.Add(dto);
    }

    private async Task ProcessBessDevice(Device device, FacilityHubDto hub, CancellationToken ct, DateTime time)
    {
        var latestData = await _context.DeviceBessData
            .Where(x => x.DeviceId == device.Id)
            .OrderBy(x => Math.Abs((x.Created - time).TotalSeconds))
            .FirstOrDefaultAsync(ct);

        if (latestData == null) return;

        var payload = DeserializePayload<BessPayload>(latestData.Payload);
        var dto = new BESSDeviceDto
        {
            DeviceId = device.Id,
            DeviceName = device.Name,
            NodeId = null,
            Timestamp = latestData.DeviceLocalTime,
            StateOfCharge = payload?.StateOfCharge,
            StateOfHealth = payload?.StateOfHealth,
            DcPower = payload?.DcPower,
            BatteryTemperature = payload?.BatteryTemperature,
            TotalEnergyStored = payload?.TotalEnergyStored,
            ChargeLimit = payload?.ChargeLimit,
            DischargeLimit = payload?.DischargeLimit,
            ChargeStatus = payload?.ChargeStatus,
            Status = payload?.Status
        };

        hub.BessDevices.Add(dto);
    }

    private async Task ProcessFacilitySmartMeter(Device device, FacilityHubDto hub, CancellationToken ct, DateTime time)
    {
        var latestData = await _context.DeviceSmartMeterData
            .Where(x => x.DeviceId == device.Id)
            .OrderBy(x => Math.Abs((x.Created - time).TotalSeconds))
            .FirstOrDefaultAsync(ct);

        if (latestData == null) return;

        var payload = DeserializePayload<SmartMeterPayload>(latestData.Payload);
        var dto = new FacilityPowerDto
        {
            DeviceId = device.Id,
            NodeId = null,
            Timestamp = latestData.DeviceLocalTime,
            AcPower = payload?.AcPower,
            TotalActiveEnergy = payload?.TotalActiveEnergy,
            Status = payload?.Status
        };

        hub.FacilityData = dto;
    }

    private Task ProcessGeneratorDevice(Device device, FacilityHubDto hub, CancellationToken ct, DateTime time)
    {
        // Generator data would come from system-level aggregation
        // For now, create a placeholder
        var dto = new GeneratorDeviceDto
        {
            DeviceId = device.Id,
            DeviceName = device.Name,
            NodeId = null,
            Timestamp = DateTimeOffset.UtcNow
        };

        hub.GeneratorDevices.Add(dto);
        return Task.CompletedTask;
    }

    private static T? DeserializePayload<T>(string payload) where T : class
    {
        if (string.IsNullOrWhiteSpace(payload) || payload == "{}")
            return null;

        try
        {
            return JsonSerializer.Deserialize<T>(payload, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    // Payload classes for JSON deserialization
    private class SolarPayload
    {
        public DCPowerDto? PanelPower { get; set; }
        public DCPowerDto? DcOutput { get; set; }
        public double? PerformanceRatio { get; set; }
        public double? PvPanelTemperature { get; set; }
        public double? SolarEnergyProduced { get; set; }
        public DeviceStatusDto? Status { get; set; }
    }

    private class PcsPayload
    {
        public DCPowerDto? DcPower { get; set; }
        public ACPowerDto? AcPower { get; set; }
        public double? TemperaturePcs { get; set; }
        public double? Efficiency { get; set; }
        public double? TotalEnergyDelivered { get; set; }
        public double? TotalEnergyAbsorbed { get; set; }
        public DeviceStatusDto? Status { get; set; }
    }

    private class SmartMeterPayload
    {
        public ACPowerDto? AcPower { get; set; }
        public double? ActiveEnergyImport { get; set; }
        public double? ActiveEnergyExport { get; set; }
        public double? TotalActiveEnergy { get; set; }
        public DeviceStatusDto? Status { get; set; }
    }

    private class BessPayload
    {
        public double? StateOfCharge { get; set; }
        public double? StateOfHealth { get; set; }
        public DCPowerDto? DcPower { get; set; }
        public double? BatteryTemperature { get; set; }
        public double? TotalEnergyStored { get; set; }
        public double? ChargeLimit { get; set; }
        public double? DischargeLimit { get; set; }
        public string? ChargeStatus { get; set; }
        public DeviceStatusDto? Status { get; set; }
    }
}
