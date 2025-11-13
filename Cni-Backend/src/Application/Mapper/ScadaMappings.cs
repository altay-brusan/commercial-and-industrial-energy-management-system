using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Backend.Application.Graphs.Queries.GetPowerGeneralGraph;
using Backend.Domain.Entities.Proto;
using Backend.Domain.Entities.Proto.DTO;
using Backend.Domain.Entities.Proto.Shared;

namespace Backend.Application.Mapper;

// Application/Common/Mappings/PowerMappings.cs
public class ScadaMappings : Profile
{
    public ScadaMappings()
    {
        // Ortak mapping fonksiyonları
        CreateMap<AppDCPower, DCPowerDto>()
         .ConvertUsing((src, dest) => MapDCPower(src));

        CreateMap<AppACPower, ACPowerDto>()
            .ConvertUsing((src, dest) => MapACPower(src));

        CreateMap<AppDeviceStatus, DeviceStatusDto>()
            .ConvertUsing((src, dest) => MapDeviceStatus(src));

        // BESSData -> BESSDeviceDto
        CreateMap<AppBESSData, BESSDeviceDto>()
            .ForMember(dest => dest.DeviceId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.DcPower, opt => opt.MapFrom(src => src.DcPower))
            .ForMember(dest => dest.ChargeStatus, opt => opt.MapFrom(src => (BESSStatusDto)src.ChargeStatus))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
        // Diğer property'ler...
            ;

        // PCSData -> PCSDeviceDto
        CreateMap<AppPCSData, PCSDeviceDto>()
            .ForMember(dest => dest.DeviceId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.AcPower, opt => opt.MapFrom(src => src.AcPower))
            .ForMember(dest => dest.DcPower, opt => opt.MapFrom(src => src.DcPower))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            // Diğer property'ler...
            ;


        CreateMap<AppSolarData, SolarDeviceDto>()
           .ForMember(dest => dest.DeviceId, opt => opt.MapFrom(src => src.Id))
           .ForMember(dest => dest.DCOutput, opt => opt.MapFrom(src => src.DCOutput))
           .ForMember(dest => dest.PanelPower, opt => opt.MapFrom(src => src.PanelPower))
           .ForMember(dest => dest.PerformanceRatio, opt => opt.MapFrom(src => src.PerformanceRatio))
           .ForMember(dest => dest.PvPanelTemperature, opt => opt.MapFrom(src => src.PvPanelTemperature))
           .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));



        // SmartMeterData -> GridPowerDto
        CreateMap<AppSmartMeterData, GridPowerDto>()
            .ForMember(dest => dest.AcPower, opt => opt.MapFrom(src => src.AcPower))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

        // SmartMeterData -> FacilityPowerDto
        CreateMap<AppSmartMeterData, FacilityPowerDto>()
            .ForMember(dest => dest.AcPower, opt => opt.MapFrom(src => src.AcPower))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

        // SystemData -> SystemDataDto
        CreateMap<AppGrpcSystemData, SystemDataDto>()
                 .ForMember(dest => dest.GeneratorPower, opt => opt.MapFrom(src => src.GeneratorPower))
           .ForMember(dest => dest.GridPower, opt => opt.MapFrom(src => src.GridPower));
        // Diğer ilgili mapping'ler
    }

    private static DCPowerDto MapDCPower(AppDCPower src)
    {
        return new DCPowerDto
        {
            Voltage = src.Voltage,
            Current = src.Current,
            Power = src.Power,
            TotalEnergyDaily = src.TotalEnergyDaily,
            TotalEnergyLifetime = src.TotalEnergyLifetime
        };
    }

    private static ACPowerDto MapACPower(AppACPower src)
    {
        return new ACPowerDto
        {
            TotalActivePower = src.TotalActivePower,
            TotalReactivePower = src.TotalReactivePower,
            TotalPowerFactor = src.TotalPowerFactor,
            VoltageRms = src.VoltageRms,
            CurrentRms = src.CurrentRms,
            TotalEnergyDaily = src.TotalEnergyDaily
        };
    }

    private static DeviceStatusDto MapDeviceStatus(AppDeviceStatus src)
    {
        return new DeviceStatusDto
        {
            OperatingMode = (Domain.Entities.Proto.DTO.OperatingMode)src.OperatingMode,
            WarningStatus = src.WarningStatus,
            FaultStatus = src.FaultStatus,
        };
    }
}
