using System.Text.Json;
using AutoMapper;
using Backend.Domain.Entities.DeviceData;
using Ems.Bess.Stream;
using Ems.Pcs.Stream;
using Ems.Smartmeter.Stream;
using Ems.Solar.Stream;
using Ems.Types.Generic;

namespace Backend.Infrastructure.Mappings;

/// <summary>
/// Mappings from gRPC Protobuf messages to device data entities.
/// All telemetry data is serialized to JSON and stored in the Payload field.
/// </summary>
public class GrpcMappingProfile : Profile
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public GrpcMappingProfile()
    {
        // BESS Mapping - serialize entire message to JSON payload
        CreateMap<BESSData, DeviceBessData>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.DeviceId, opt => opt.Ignore()) // Set by caller
            .ForMember(dest => dest.FirmId, opt => opt.Ignore())   // Set by caller
            .ForMember(dest => dest.DeviceLocalTime, opt => opt.MapFrom(_ => DateTimeOffset.UtcNow))
            .ForMember(dest => dest.Payload, opt => opt.MapFrom(src => SerializeBessData(src)))
            .ForMember(dest => dest.Device, opt => opt.Ignore())
            .ForMember(dest => dest.Firm, opt => opt.Ignore());

        // PCS Mapping
        CreateMap<PCSData, DevicePcsData>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.DeviceId, opt => opt.Ignore())
            .ForMember(dest => dest.FirmId, opt => opt.Ignore())
            .ForMember(dest => dest.DeviceLocalTime, opt => opt.MapFrom(_ => DateTimeOffset.UtcNow))
            .ForMember(dest => dest.Payload, opt => opt.MapFrom(src => SerializePcsData(src)))
            .ForMember(dest => dest.Device, opt => opt.Ignore())
            .ForMember(dest => dest.Firm, opt => opt.Ignore());

        // SmartMeter Mapping
        CreateMap<SmartMeterData, DeviceSmartMeterData>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.DeviceId, opt => opt.Ignore())
            .ForMember(dest => dest.FirmId, opt => opt.Ignore())
            .ForMember(dest => dest.DeviceLocalTime, opt => opt.MapFrom(_ => DateTimeOffset.UtcNow))
            .ForMember(dest => dest.Payload, opt => opt.MapFrom(src => SerializeSmartMeterData(src)))
            .ForMember(dest => dest.Device, opt => opt.Ignore())
            .ForMember(dest => dest.Firm, opt => opt.Ignore());

        // Solar Mapping
        CreateMap<SolarData, DeviceSolarData>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.DeviceId, opt => opt.Ignore())
            .ForMember(dest => dest.FirmId, opt => opt.Ignore())
            .ForMember(dest => dest.DeviceLocalTime, opt => opt.MapFrom(_ => DateTimeOffset.UtcNow))
            .ForMember(dest => dest.Payload, opt => opt.MapFrom(src => SerializeSolarData(src)))
            .ForMember(dest => dest.Device, opt => opt.Ignore())
            .ForMember(dest => dest.Firm, opt => opt.Ignore());
    }

    private static string SerializeBessData(BESSData src)
    {
        var payload = new
        {
            bessId = src.Id,
            stateOfCharge = src.StateOfCharge,
            stateOfHealth = src.StateOfHealth,
            batteryTemperature = src.BatteryTemperature,
            totalEnergyStored = src.TotalEnergyStored,
            energyThroughput = src.EnergyThroughput,
            chargeLimit = src.ChargeLimit,
            dischargeLimit = src.DischargeLimit,
            numberOfPacks = src.NumberOfPacks,
            numberOfModules = src.NumberOfModules,
            numberOfCells = src.NumberOfCells,
            capacity = src.Capacity,
            coolingActive = src.CoolingActive,
            chargeStatus = src.ChargeStatus.ToString(),
            dcPower = MapDcPower(src.DcPower),
            status = MapStatus(src.Status)
        };
        return JsonSerializer.Serialize(payload, JsonOptions);
    }

    private static string SerializePcsData(PCSData src)
    {
        var payload = new
        {
            pcsId = src.Id,
            temperaturePcs = src.TemperaturePcs,
            efficiency = src.Efficiency,
            totalEnergyDelivered = src.TotalEnergyDelivered,
            totalEnergyAbsorbed = src.TotalEnergyAbsorbed,
            dcPower = MapDcPower(src.DcPower),
            acPower = MapAcPower(src.AcPower),
            status = MapStatus(src.Status)
        };
        return JsonSerializer.Serialize(payload, JsonOptions);
    }

    private static string SerializeSmartMeterData(SmartMeterData src)
    {
        var payload = new
        {
            smartMeterId = src.Id,
            activeEnergyImport = src.ActiveEnergyImport,
            activeEnergyExport = src.ActiveEnergyExport,
            reactiveEnergyImport = src.ReactiveEnergyImport,
            reactiveEnergyExport = src.ReactiveEnergyExport,
            totalActiveEnergy = src.TotalActiveEnergy,
            totalReactiveEnergy = src.TotalReactiveEnergy,
            currentDemand = src.CurrentDemand,
            powerDemand = src.PowerDemand,
            acPower = MapAcPower(src.AcPower),
            status = MapStatus(src.Status)
        };
        return JsonSerializer.Serialize(payload, JsonOptions);
    }

    private static string SerializeSolarData(SolarData src)
    {
        var payload = new
        {
            solarId = src.Id,
            performanceRatio = src.PerformanceRatio,
            pvPanelTemperature = src.PvPanelTemperature,
            solarEnergyProduced = src.SolarEnergyProduced,
            panelPower = MapDcPower(src.PanelPower),
            chargerPower = MapDcPower(src.ChargerPower),
            dcOutput = MapDcPower(src.DCOutput),
            acOutput = MapAcPower(src.ACOutput),
            status = MapStatus(src.Status)
        };
        return JsonSerializer.Serialize(payload, JsonOptions);
    }

    private static object? MapDcPower(DC_Power? src)
    {
        if (src == null) return null;
        return new
        {
            voltage = src.Voltage,
            current = src.Current,
            power = src.Power,
            totalEnergyDaily = src.TotalEnergyDaily,
            totalEnergyLifetime = src.TotalEnergyLifetime
        };
    }

    private static object? MapAcPower(AC_Power? src)
    {
        if (src == null) return null;
        return new
        {
            voltagePhase1 = src.VoltagePhase1,
            voltagePhase2 = src.VoltagePhase2,
            voltagePhase3 = src.VoltagePhase3,
            lineToLineVoltagePhase1 = src.LineToLineVoltagePhase1,
            lineToLineVoltagePhase2 = src.LineToLineVoltagePhase2,
            lineToLineVoltagePhase3 = src.LineToLineVoltagePhase3,
            voltageRms = src.VoltageRms,
            currentPhase1 = src.CurrentPhase1,
            currentPhase2 = src.CurrentPhase2,
            currentPhase3 = src.CurrentPhase3,
            currentRms = src.CurrentRms,
            activePowerPhase1 = src.ActivePowerPhase1,
            activePowerPhase2 = src.ActivePowerPhase2,
            activePowerPhase3 = src.ActivePowerPhase3,
            totalActivePower = src.TotalActivePower,
            reactivePowerPhase1 = src.ReactivePowerPhase1,
            reactivePowerPhase2 = src.ReactivePowerPhase2,
            reactivePowerPhase3 = src.ReactivePowerPhase3,
            totalReactivePower = src.TotalReactivePower,
            apparentPowerPhase1 = src.ApparentPowerPhase1,
            apparentPowerPhase2 = src.ApparentPowerPhase2,
            apparentPowerPhase3 = src.ApparentPowerPhase3,
            totalApparentPower = src.TotalApparentPower,
            powerFactorPhase1 = src.PowerFactorPhase1,
            powerFactorPhase2 = src.PowerFactorPhase2,
            powerFactorPhase3 = src.PowerFactorPhase3,
            totalPowerFactor = src.TotalPowerFactor,
            totalEnergyDaily = src.TotalEnergyDaily,
            totalEnergyLifetime = src.TotalEnergyLifetime,
            frequency = src.Frequency
        };
    }

    private static object? MapStatus(Status? src)
    {
        if (src == null) return null;
        return new
        {
            operatingMode = src.OperatingMode.ToString(),
            warningStatus = src.WarningStatus,
            warningMessages = src.WarningMessages.ToList(),
            faultStatus = src.FaultStatus,
            faultMessages = src.FaultMessages.ToList()
        };
    }
}
