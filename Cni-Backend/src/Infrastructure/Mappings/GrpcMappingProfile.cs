using System.Net.NetworkInformation;
using AutoMapper;
using Backend.Domain.Entities.Proto;
using Backend.Domain.Entities.Proto.DTO;
using Backend.Domain.Entities.Proto.Shared;
using Ems.Bess.Stream;
using Ems.Pcs.Stream;
using Ems.Smartmeter.Stream;
using Ems.Solar.Stream;
using Ems.System.Stream;
using Ems.Types.Generic;

namespace Backend.Infrastructure.Mappings;

public class GrpcMappingProfile : Profile
{
    public GrpcMappingProfile()
    {
        #region DbEntity Mappings
        CreateMap<AC_Power, AppACPower>();  // Alan isimleri aynı, direkt eşleşir.
        CreateMap<DC_Power, AppDCPower>();  // Tüm double alanlar nullable, isimler aynı.
        CreateMap<Status, AppDeviceStatus>()
            .ForMember(dest => dest.OperatingMode, opt => opt.MapFrom(src => (AppOperatingMode)src.OperatingMode))
            .ForMember(dest => dest.WarningStatus,opt => opt.MapFrom(src => src.WarningStatus))
            .ForMember(dest => dest.FaultStatus,opt => opt.MapFrom(src => src.FaultStatus));

        // BESS Mapping
        CreateMap<BESSData, AppBESSData>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.BESSId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.DcPower, opt => opt.MapFrom(src => src.DcPower))
            .ForMember(dest => dest.Status,opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.ChargeStatus, opt => opt.MapFrom(src => (AppBESSStatus)src.ChargeStatus));

        //PCS Mapping
        CreateMap<PCSData, AppPCSData>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
           .ForMember(dest => dest.PCSId, opt => opt.MapFrom(src => src.Id))
           .ForMember(dest => dest.DcPower, opt => opt.MapFrom(src => src.DcPower))
           .ForMember(dest => dest.AcPower, opt => opt.MapFrom(src => src.AcPower))
           .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

        //SmartMeter Mapping
        CreateMap<SmartMeterData, AppSmartMeterData>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())

            .ForMember(dest => dest.SmartMeterId,
                       opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.AcPower,
                       opt => opt.MapFrom(src => src.AcPower))
            .ForMember(dest => dest.Status,
                       opt => opt.MapFrom(src => src.Status));

        //Solar Mapping
        CreateMap<SolarData, AppSolarData>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())

                 .ForMember(dest => dest.SolarId, opt => opt.MapFrom(src => src.Id))
                 .ForMember(dest => dest.PanelPower,
                       opt => opt.MapFrom(src => src.PanelPower))  
                 .ForMember(dest => dest.ChargerPower,
                       opt => opt.MapFrom(src => src.ChargerPower))
                 .ForMember(dest => dest.DCOutput,
                       opt => opt.MapFrom(src => src.DCOutput))
                 .ForMember(dest => dest.ACOutput,
                       opt => opt.MapFrom(src => src.ACOutput))
                  .ForMember(dest => dest.Status,
                       opt => opt.MapFrom(src => src.Status));

        CreateMap<SystemData, AppGrpcSystemData>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.GeneratorPower, opt => opt.MapFrom(src => src.GeneratorPower))
           .ForMember(dest => dest.GridPower, opt => opt.MapFrom(src => src.GridPower));



        #endregion



        // Ortak mapping fonksiyonları
        CreateMap<DC_Power, DCPowerDto>()
         .ConvertUsing((src, dest) => MapDCPower(src));

        CreateMap<AC_Power, ACPowerDto>()
            .ConvertUsing((src, dest) => MapACPower(src));

        CreateMap<Status, DeviceStatusDto>()
            .ConvertUsing((src, dest) => MapDeviceStatus(src));

        // BESSData -> BESSDeviceDto
        CreateMap<BESSData, BESSDeviceDto>()
            .ForMember(dest => dest.DeviceId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.DcPower, opt => opt.MapFrom(src => src.DcPower))
            .ForMember(dest => dest.ChargeStatus, opt => opt.MapFrom(src => (BESSStatusDto)src.ChargeStatus))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            // Diğer property'ler...
            ;

        // PCSData -> PCSDeviceDto
        CreateMap<PCSData, PCSDeviceDto>()
            .ForMember(dest => dest.DeviceId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.AcPower, opt => opt.MapFrom(src => src.AcPower))
            .ForMember(dest => dest.DcPower, opt => opt.MapFrom(src => src.DcPower))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            // Diğer property'ler...
            ;


        CreateMap<SolarData, SolarDeviceDto>()
           .ForMember(dest => dest.DeviceId, opt => opt.MapFrom(src => src.Id))
           .ForMember(dest => dest.DCOutput, opt => opt.MapFrom(src => src.DCOutput))
           .ForMember(dest => dest.PanelPower, opt => opt.MapFrom(src => src.PanelPower))
           .ForMember(dest => dest.PerformanceRatio, opt => opt.MapFrom(src => src.PerformanceRatio))
           .ForMember(dest => dest.PvPanelTemperature, opt => opt.MapFrom(src => src.PvPanelTemperature))
           .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));



        // SmartMeterData -> GridPowerDto
        CreateMap<SmartMeterData, GridPowerDto>()
            .ForMember(dest => dest.AcPower, opt => opt.MapFrom(src => src.AcPower))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

        // SmartMeterData -> FacilityPowerDto
        CreateMap<SmartMeterData, FacilityPowerDto>()
            .ForMember(dest => dest.AcPower, opt => opt.MapFrom(src => src.AcPower))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

        // SystemData -> SystemDataDto
        CreateMap<SystemData, SystemDataDto>()
                 .ForMember(dest => dest.GeneratorPower, opt => opt.MapFrom(src => src.GeneratorPower))
           .ForMember(dest => dest.GridPower, opt => opt.MapFrom(src => src.GridPower));
    }

    private static DCPowerDto MapDCPower(DC_Power src)
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

    private static ACPowerDto MapACPower(AC_Power src)
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

    private static DeviceStatusDto MapDeviceStatus(Status src)
    {
        return new DeviceStatusDto
        {
            OperatingMode = (Domain.Entities.Proto.DTO.OperatingMode)src.OperatingMode,
            WarningStatus = src.WarningStatus,
            WarningMessages = src.WarningMessages.ToList(),
            FaultStatus = src.FaultStatus,
            FaultMessages = src.FaultMessages.ToList()
        };
    }
}
