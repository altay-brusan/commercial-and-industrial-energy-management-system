using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backend.Domain.Entities.Proto;

namespace Backend.Application.Graphs.Queries.GetInverterGraph;
public class InverterGraphDto
{
    public DateTimeOffset Time { get; set; }
    public int PcsId { get; set; }
    public double? ActivePower { get; set; }
    public double? ReactivePower { get; set; }
    public double? PowerFactor { get; set; }
    public double? AcVoltage { get; set; }
    public double? AcCurrent { get; set; }
    public double? DcVoltage { get; set; }
    public double? DcCurrent { get; set; }
    public double? DcPower { get; set; }
    public double? Temperature { get; set; }

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<AppPCSData, InverterGraphDto>()
                .ForMember(dest => dest.Time, opt => opt.MapFrom(src => src.Created.DateTime)) // BaseAuditableEntity'den Created alanını kullan
                .ForMember(dest => dest.PcsId, opt => opt.MapFrom(src => src.PCSId))
                .ForMember(dest => dest.ActivePower, opt => opt.MapFrom(src => src.AcPower.TotalActivePower))
                .ForMember(dest => dest.ReactivePower, opt => opt.MapFrom(src => src.AcPower.TotalReactivePower))
                .ForMember(dest => dest.PowerFactor, opt => opt.MapFrom(src => src.AcPower.TotalPowerFactor))
                .ForMember(dest => dest.AcVoltage, opt => opt.MapFrom(src => src.AcPower.VoltageRms))
                .ForMember(dest => dest.AcCurrent, opt => opt.MapFrom(src => src.AcPower.CurrentRms))
                .ForMember(dest => dest.DcVoltage, opt => opt.MapFrom(src => src.DcPower.Voltage))
                .ForMember(dest => dest.DcCurrent, opt => opt.MapFrom(src => src.DcPower.Current))
                .ForMember(dest => dest.DcPower, opt => opt.MapFrom(src => src.DcPower.Power))
                .ForMember(dest => dest.Temperature, opt => opt.MapFrom(src => src.TemperaturePcs));
        }
    }
}
