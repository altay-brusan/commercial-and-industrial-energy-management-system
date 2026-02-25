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

    // PCS — summary
    public const string PcsActivePower = "pcsActivePower";
    public const string PcsReactivePower = "pcsReactivePower";
    public const string PcsDcPower = "pcsDcPower";
    public const string PcsGridFreq = "pcsGridFreq";
    public const string PcsGridMode = "pcsGridMode";
    public const string PcsIgbtTemp = "pcsIgbtTemp";
    public const string PcsPowerFactor = "pcsPowerFactor";
    public const string PcsEfficiency = "pcsEfficiency";

    // PCS — detail status
    public const string PcsPhaseSequence = "pcsPhaseSequence";
    public const string PcsDeratingCoefficient = "pcsDeratingCoefficient";
    public const string PcsWorkingCondition = "pcsWorkingCondition";
    public const string PcsSourceMode = "pcsSourceMode";

    // PCS — AC line voltages
    public const string PcsLineVoltageAB = "pcsLineVoltageAB";
    public const string PcsLineVoltageBC = "pcsLineVoltageBC";
    public const string PcsLineVoltageCA = "pcsLineVoltageCA";

    // PCS — AC phase voltages
    public const string PcsPhaseVoltageA = "pcsPhaseVoltageA";
    public const string PcsPhaseVoltageB = "pcsPhaseVoltageB";
    public const string PcsPhaseVoltageC = "pcsPhaseVoltageC";

    // PCS — AC phase currents
    public const string PcsPhaseCurrentA = "pcsPhaseCurrentA";
    public const string PcsPhaseCurrentB = "pcsPhaseCurrentB";
    public const string PcsPhaseCurrentC = "pcsPhaseCurrentC";

    // PCS — AC inductor currents
    public const string PcsInductorCurrentA = "pcsInductorCurrentA";
    public const string PcsInductorCurrentB = "pcsInductorCurrentB";
    public const string PcsInductorCurrentC = "pcsInductorCurrentC";

    // PCS — DC measurements
    public const string PcsTotalDcBusVoltage = "pcsTotalDcBusVoltage";
    public const string PcsDcBusPosVoltage = "pcsDcBusPosVoltage";
    public const string PcsDcBusNegVoltage = "pcsDcBusNegVoltage";
    public const string PcsBatteryVoltage = "pcsBatteryVoltage";
    public const string PcsBatteryCurrent = "pcsBatteryCurrent";
    public const string PcsTotalDcCurrent = "pcsTotalDcCurrent";

    // PCS — per-phase power
    public const string PcsActivePowerA = "pcsActivePowerA";
    public const string PcsActivePowerB = "pcsActivePowerB";
    public const string PcsActivePowerC = "pcsActivePowerC";
    public const string PcsApparentPowerA = "pcsApparentPowerA";
    public const string PcsApparentPowerB = "pcsApparentPowerB";
    public const string PcsApparentPowerC = "pcsApparentPowerC";
    public const string PcsApparentPowerTotal = "pcsApparentPowerTotal";
    public const string PcsReactivePowerA = "pcsReactivePowerA";
    public const string PcsReactivePowerB = "pcsReactivePowerB";
    public const string PcsReactivePowerC = "pcsReactivePowerC";

    // PCS — temperatures
    public const string PcsAmbientTemp = "pcsAmbientTemp";
    public const string PcsInductanceTemp = "pcsInductanceTemp";

    // PCS — charge algorithm
    public const string PcsCcCvRechargeV = "pcsCcCvRechargeV";
    public const string PcsCvChargeV = "pcsCvChargeV";
    public const string PcsCvMaxTime = "pcsCvMaxTime";
    public const string PcsCvFloatI = "pcsCvFloatI";
    public const string PcsFloatChargeV = "pcsFloatChargeV";
    public const string PcsFloatCcI = "pcsFloatCcI";

    // Chiller/TMS
    public const string ChillerCoolantOut = "chillerCoolantOut";
    public const string ChillerCoolantIn = "chillerCoolantIn";
    public const string CompressorRunning = "compressorRunning";

    // Health
    public const string Rte = "rte";
    public const string CalendarLifePct = "calendarLifePct";
    public const string EstimatedRul = "estimatedRul";
}
