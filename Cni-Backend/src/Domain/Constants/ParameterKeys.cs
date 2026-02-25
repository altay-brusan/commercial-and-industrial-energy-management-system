namespace Backend.Domain.Constants;

/// <summary>
/// Well-known parameter keys used in device specification parameterMap definitions.
/// These keys are the contract between Specifications JSON and C# query handlers.
/// </summary>
public abstract class ParameterKeys
{
    // Battery core
    public const string Soc = "soc";
    public const string Soh = "soh";
    public const string Voltage = "voltage";
    public const string Current = "current";
    public const string Power = "power";
    public const string PowerDirection = "powerDirection";
    public const string OperatingMode = "operatingMode";
    public const string CycleCount = "cycleCount";
    public const string CycleLifePct = "cycleLifePct";
    public const string RemainingEnergy = "remainingEnergy";
    public const string AvailableEnergy = "availableEnergy";

    // Temperature
    public const string TempMin = "tempMin";
    public const string TempMax = "tempMax";
    public const string TempAvg = "tempAvg";
    public const string AmbientTemp = "ambientTemp";

    // Cell extremes
    public const string CellVoltageMax = "cellVoltageMax";
    public const string CellVoltageMin = "cellVoltageMin";
    public const string CellVoltageAvg = "cellVoltageAvg";
    public const string VoltageSpread = "voltageSpread";

    // PCS
    public const string PcsActivePower = "pcsActivePower";
    public const string PcsReactivePower = "pcsReactivePower";
    public const string PcsDcPower = "pcsDcPower";
    public const string PcsGridFreq = "pcsGridFreq";
    public const string PcsGridMode = "pcsGridMode";
    public const string PcsIgbtTemp = "pcsIgbtTemp";
    public const string PcsPowerFactor = "pcsPowerFactor";
    public const string PcsEfficiency = "pcsEfficiency";

    // Chiller/TMS
    public const string ChillerCoolantOut = "chillerCoolantOut";
    public const string ChillerCoolantIn = "chillerCoolantIn";
    public const string CompressorRunning = "compressorRunning";

    // Health
    public const string Rte = "rte";
    public const string CalendarLifePct = "calendarLifePct";
    public const string EstimatedRul = "estimatedRul";
}
