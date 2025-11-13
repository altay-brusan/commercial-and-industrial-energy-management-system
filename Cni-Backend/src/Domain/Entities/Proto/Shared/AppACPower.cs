using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Domain.Entities.Proto.Shared;
public class AppACPower
{
    public double? VoltagePhase1 { get; set; }
    public double? VoltagePhase2 { get; set; }
    public double? VoltagePhase3 { get; set; }
    public double? LineToLineVoltagePhase1 { get; set; }
    public double? LineToLineVoltagePhase2 { get; set; }
    public double? LineToLineVoltagePhase3 { get; set; }
    public double? VoltageRms { get; set; }
    public double? CurrentPhase1 { get; set; }
    public double? CurrentPhase2 { get; set; }
    public double? CurrentPhase3 { get; set; }
    public double? CurrentRms { get; set; }
    public double? ActivePowerPhase1 { get; set; }
    public double? ActivePowerPhase2 { get; set; }
    public double? ActivePowerPhase3 { get; set; }
    public double? TotalActivePower { get; set; }
    public double? ReactivePowerPhase1 { get; set; }
    public double? ReactivePowerPhase2 { get; set; }
    public double? ReactivePowerPhase3 { get; set; }
    public double? TotalReactivePower { get; set; }
    public double? ApparentPowerPhase1 { get; set; }
    public double? ApparentPowerPhase2 { get; set; }
    public double? ApparentPowerPhase3 { get; set; }
    public double? TotalApparentPower { get; set; }
    public double? PowerFactorPhase1 { get; set; }
    public double? PowerFactorPhase2 { get; set; }
    public double? PowerFactorPhase3 { get; set; }
    public double? TotalPowerFactor { get; set; }
    public double? TotalEnergyDaily { get; set; }
    public double? TotalEnergyLifetime { get; set; }
    public double? Frequency { get; set; }
}
