using System.Text.Json;
using Backend.Application.Common.Interfaces;

namespace Backend.Application.Scada.Queries.GetScadaDataMinimal;

public record GetScadaDataMinimalQuery : IRequest<ScadaDataMinimalDto?>
{
}

public class GetScadaDataMinimalQueryHandler : IRequestHandler<GetScadaDataMinimalQuery, ScadaDataMinimalDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IVirtualClockService _virtualClockService;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public GetScadaDataMinimalQueryHandler(IApplicationDbContext context, IVirtualClockService virtualClockService)
    {
        _context = context;
        _virtualClockService = virtualClockService;
    }

    public async Task<ScadaDataMinimalDto?> Handle(GetScadaDataMinimalQuery request, CancellationToken cancellationToken)
    {
        var time = _virtualClockService.CurrentTime;

        // Get device data from new tables with JSON payloads
        var solarData = await _context.DeviceSolarData
            .Where(f => f.DeviceId == 2)
            .OrderBy(x => Math.Abs((x.Created - time).TotalSeconds))
            .FirstOrDefaultAsync(cancellationToken);

        var bessData = await _context.DeviceBessData
            .Where(f => f.DeviceId == 1)
            .OrderBy(x => Math.Abs((x.Created - time).TotalSeconds))
            .FirstOrDefaultAsync(cancellationToken);

        var pcsData = await _context.DevicePcsData
            .Where(f => f.DeviceId == 3)
            .OrderBy(x => Math.Abs((x.Created - time).TotalSeconds))
            .FirstOrDefaultAsync(cancellationToken);

        var facilityData = await _context.DeviceSmartMeterData
            .Where(f => f.DeviceId == 5)
            .OrderBy(x => Math.Abs((x.Created - time).TotalSeconds))
            .FirstOrDefaultAsync(cancellationToken);

        // Parse JSON payloads
        var solarPayload = solarData != null ? DeserializePayload<SolarPayload>(solarData.Payload) : null;
        var bessPayload = bessData != null ? DeserializePayload<BessPayload>(bessData.Payload) : null;
        var pcsPayload = pcsData != null ? DeserializePayload<PcsPayload>(pcsData.Payload) : null;
        var facilityPayload = facilityData != null ? DeserializePayload<SmartMeterPayload>(facilityData.Payload) : null;

        var scadaMinimalData = new ScadaDataMinimalDto
        {
            Bess = new ScadaDataMinimalDto.BessDataMinimal
            {
                StateOfCharge = string.Format("{0:0} %", bessPayload?.StateOfCharge ?? 0.0),
                Temperature = string.Format("{0:0.0} ℃", bessPayload?.BatteryTemperature ?? 0.0),
                Status = bessPayload?.Status?.OperatingMode ?? "Unknown",
                ChargingStatus = bessPayload?.ChargeStatus ?? "Unknown",
            },
            Solar = new ScadaDataMinimalDto.SolarDataMinimal
            {
                Efficiency = string.Format("{0:0} %", solarPayload?.PerformanceRatio ?? 0.0),
                Generation = string.Format("{0} kW", Math.Ceiling(solarPayload?.PanelPower?.Power ?? 0.0)),
                Temperature = string.Format("{0:0.0} ℃", solarPayload?.PvPanelTemperature ?? 0.0),
                MpptdcPower = string.Format("{0} kW", Math.Ceiling(solarPayload?.DcOutput?.Power ?? 0.0))
            },
            Pcs = new ScadaDataMinimalDto.PcsDataMinimal
            {
                DcPower = string.Format("{0} kW", Math.Ceiling(pcsPayload?.DcPower?.Power ?? 0.0)),
                Efficiency = string.Format("{0:0} %", pcsPayload?.Efficiency ?? 0.0),
                Status = pcsPayload?.Status?.OperatingMode ?? "Unknown",
                AcPower = string.Format("{0} kW", Math.Ceiling(pcsPayload?.AcPower?.TotalActivePower ?? 0.0))
            },
            Facility = new ScadaDataMinimalDto.FacilityDataMinimal
            {
                PowerFactor = string.Format("{0} kW", Math.Ceiling(facilityPayload?.AcPower?.TotalPowerFactor ?? 0.0)),
                PowerConsumption = string.Format("{0} kW", Math.Ceiling(facilityPayload?.AcPower?.TotalActivePower ?? 0.0))
            },
            Timestamp = time.ToString("dd.MM.yyyy HH.mm"),
            IsDaytime = time.Hour >= 6 && time.Hour < 18
        };

        return scadaMinimalData;
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
        public DcPowerDto? PanelPower { get; set; }
        public DcPowerDto? DcOutput { get; set; }
        public double? PerformanceRatio { get; set; }
        public double? PvPanelTemperature { get; set; }
    }

    private class BessPayload
    {
        public double? StateOfCharge { get; set; }
        public double? BatteryTemperature { get; set; }
        public string? ChargeStatus { get; set; }
        public StatusDto? Status { get; set; }
    }

    private class PcsPayload
    {
        public DcPowerDto? DcPower { get; set; }
        public AcPowerDto? AcPower { get; set; }
        public double? Efficiency { get; set; }
        public StatusDto? Status { get; set; }
    }

    private class SmartMeterPayload
    {
        public AcPowerDto? AcPower { get; set; }
    }

    private class DcPowerDto
    {
        public double? Power { get; set; }
    }

    private class AcPowerDto
    {
        public double? TotalActivePower { get; set; }
        public double? TotalPowerFactor { get; set; }
    }

    private class StatusDto
    {
        public string? OperatingMode { get; set; }
    }
}
