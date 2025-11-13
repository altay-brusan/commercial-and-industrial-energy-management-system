using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Domain.Entities.Proto.Shared;
public class AppDCPower
{
    public double? Voltage { get; set; }
    public double? Current { get; set; }
    public double? Power { get; set; }
    public double? TotalEnergyDaily { get; set; }
    public double? TotalEnergyLifetime { get; set; }
}
