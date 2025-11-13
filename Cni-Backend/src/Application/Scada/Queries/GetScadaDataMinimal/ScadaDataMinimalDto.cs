using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Application.Scada.Queries.GetScadaDataMinimal;

public class ScadaDataMinimalDto
{
    public string? Timestamp { get; set; }
    public bool IsDaytime { get; set; }
    public FacilityDataMinimal Facility { get; set; } = new();
    public SolarDataMinimal Solar { get; set; } = new();
    public PcsDataMinimal Pcs { get; set; } = new();
    public BessDataMinimal Bess { get; set; } = new();

    public class FacilityDataMinimal
    {
        public string? PowerConsumption { get; set; } // kW
        public string? PowerFactor { get; set; }       // kW
    }

    public class SolarDataMinimal
    {
        public string? Generation { get; set; }    // kW
        public string? Efficiency { get; set; }    // %
        public string? Temperature { get; set; }   // °C
        public string? MpptdcPower { get; set; }   // kW
    }

    public class PcsDataMinimal
    {
        public string? Efficiency { get; set; }   // %
        public string? AcPower { get; set; }       // kW
        public string? DcPower { get; set; }       // kW
        public string? Status { get; set; }          // 0:Off, 1:Standby, 2:Running
    }

    public class BessDataMinimal
    {
        public string? StateOfCharge { get; set; } // %
        public string? Temperature { get; set; }    // °C
        public string? Status { get; set; }
        public string ? ChargingStatus { get; set; }
    }
}
