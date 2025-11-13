using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Domain.Entities.Proto.Shared;
public class AppDeviceStatus
{
    public AppOperatingMode OperatingMode { get; set; }
    public bool WarningStatus { get; set; }
    public bool FaultStatus { get; set; }
}
public enum AppOperatingMode
{
    Standby = 0,
    Active = 1,
    Fault = 2
}
