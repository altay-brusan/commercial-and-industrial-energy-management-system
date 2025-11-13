using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Domain.Entities.Proto;

public class AppGrpcSystemData : BaseAuditableEntity, IMultiTenantEntity
{
    public int TenantId { get; set; }
    public int FacilityId { get; set; }
    public double? GeneratorPower { get; set; }
    public double? GridPower { get; set; }
    public double? LoadGeneratorContribution { get; set; }
    public double? LoadGridContribution { get; set; }
    public double? LoadPcsContribution { get; set; }
    public double? LoadSolarContribution { get; set; }
    public double? PcsPower { get; set; }
    public double? SolarPower { get; set; }
}
