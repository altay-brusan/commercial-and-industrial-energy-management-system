using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backend.Application.Firms.Queries.GetFirm;
using Backend.Domain.Entities;
using Backend.Domain.Entities.Proto;
using Backend.Domain.Entities.Proto.DTO;

namespace Backend.Application.Graphs.Queries.GetPowerGeneralGraph;
public class PowerGeneralGraphDto
{
    public int FacilityId { get; set; }
    public DateTime Time { get; set; }

    public double? GeneratorPower { get; set; }
    public double? GridPower { get; set; }
   
    public double? BessPower { get; set; }
    public double? SolarPower { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<AppGrpcSystemData, PowerGeneralGraphDto>()
                .ForMember(dest => dest.Time, opt => opt.MapFrom(src => src.Created.DateTime))
                .ForMember(dest => dest.BessPower, opt => opt.MapFrom(src => src.PcsPower));
        }
    }
}
