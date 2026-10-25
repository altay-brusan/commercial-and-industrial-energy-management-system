import type {
  PcsUnit,
  PcsFaultFlag,
  PcsFaultCategory,
} from "./types";
import { bessUnits } from "@/lib/bess/mock-data";

// --- Seeded random ---
function seededRandom(seed: number) {
  let s = seed;
  return () => {
    s = (s * 16807 + 0) % 2147483647;
    return (s - 1) / 2147483646;
  };
}

function round(v: number, d = 2) {
  const f = 10 ** d;
  return Math.round(v * f) / f;
}

function gaussian(rand: () => number, mean: number, stddev: number): number {
  const u1 = rand();
  const u2 = rand();
  const z = Math.sqrt(-2 * Math.log(u1)) * Math.cos(2 * Math.PI * u2);
  return mean + z * stddev;
}

// --- PCS Unit Generator ---
function generatePcsUnit(bessUnit: (typeof bessUnits)[0], seed: number): PcsUnit {
  const rand = seededRandom(seed);
  const nominalLineV = 400;
  const nominalPhaseV = nominalLineV / Math.sqrt(3);

  const lineVab = round(gaussian(rand, nominalLineV, 2), 1);
  const lineVbc = round(gaussian(rand, nominalLineV, 2), 1);
  const lineVca = round(gaussian(rand, nominalLineV, 2), 1);

  const phaseVa = round(gaussian(rand, nominalPhaseV, 1.5), 1);
  const phaseVb = round(gaussian(rand, nominalPhaseV, 1.5), 1);
  const phaseVc = round(gaussian(rand, nominalPhaseV, 1.5), 1);

  const powerKw = bessUnit.live.power;
  const totalCurrent = powerKw !== 0 ? Math.abs(powerKw * 1000) / (Math.sqrt(3) * nominalLineV * 0.95) : rand() * 2;
  const phaseCurrA = round(gaussian(rand, totalCurrent / 3, totalCurrent * 0.02), 1);
  const phaseCurrB = round(gaussian(rand, totalCurrent / 3, totalCurrent * 0.02), 1);
  const phaseCurrC = round(gaussian(rand, totalCurrent / 3, totalCurrent * 0.02), 1);

  const dcBusV = round(bessUnit.live.voltage * 1.02, 1);
  const dcPosBus = round(dcBusV / 2 + rand() * 2, 1);
  const dcNegBus = round(dcBusV / 2 + rand() * 2, 1);
  const batV = round(bessUnit.live.voltage, 1);
  const batI = round(bessUnit.live.current, 1);
  const dcPwr = round(batV * batI / 1000, 2);

  const phaseAPwr = round(powerKw / 3 + gaussian(rand, 0, 0.5), 2);
  const phaseBPwr = round(powerKw / 3 + gaussian(rand, 0, 0.5), 2);
  const phaseCPwr = round(powerKw - phaseAPwr - phaseBPwr, 2);

  const pf = 0.95 + rand() * 0.04;
  const phaseAApp = round(Math.abs(phaseAPwr) / pf, 2);
  const phaseBApp = round(Math.abs(phaseBPwr) / pf, 2);
  const phaseCApp = round(Math.abs(phaseCPwr) / pf, 2);

  const phaseAReact = round(Math.sqrt(Math.max(0, phaseAApp ** 2 - phaseAPwr ** 2)), 2);
  const phaseBReact = round(Math.sqrt(Math.max(0, phaseBApp ** 2 - phaseBPwr ** 2)), 2);
  const phaseCReact = round(Math.sqrt(Math.max(0, phaseCApp ** 2 - phaseCPwr ** 2)), 2);

  const status = bessUnit.status === "offline" ? "offline" as const
    : bessUnit.status === "fault" ? "fault" as const
    : bessUnit.status === "standby" ? "standby" as const
    : "online" as const;

  const workingConditions = ["Normal Operation", "CV Charging", "CC Charging", "Float Charging", "Standby"];
  const sourceModes = ["Grid-tied", "Off-grid", "Hybrid"];
  const gridModes = ["Grid Following", "Grid Forming", "Droop Control"];

  return {
    id: `pcs-${bessUnit.id.slice(-2)}`,
    deviceId: `PCS-${bessUnit.id.slice(-2).toUpperCase()}-01`,
    name: `PCS Unit ${bessUnit.id.slice(-2).toUpperCase()}`,
    linkedBessUnitId: bessUnit.id,
    linkedBessUnitName: bessUnit.name,
    status,
    ac: {
      lineVoltageAbV: lineVab,
      lineVoltageBcV: lineVbc,
      lineVoltageCaV: lineVca,
      phaseVoltageAV: phaseVa,
      phaseVoltageBV: phaseVb,
      phaseVoltageCV: phaseVc,
      phaseCurrentAA: phaseCurrA,
      phaseCurrentBA: phaseCurrB,
      phaseCurrentCA: phaseCurrC,
      inductorCurrentAA: round(phaseCurrA * 1.02, 1),
      inductorCurrentBA: round(phaseCurrB * 1.02, 1),
      inductorCurrentCA: round(phaseCurrC * 1.02, 1),
    },
    dc: {
      totalBusV: dcBusV,
      posBusV: dcPosBus,
      negBusV: dcNegBus,
      batV,
      batI,
      dcPwrKw: dcPwr,
      acPwrFactor: round(pf, 3),
      totalDcI: round(batI * 1.01, 1),
    },
    power: {
      phaseA: { activeKw: phaseAPwr, apparentKva: phaseAApp, reactiveKvar: phaseAReact },
      phaseB: { activeKw: phaseBPwr, apparentKva: phaseBApp, reactiveKvar: phaseBReact },
      phaseC: { activeKw: phaseCPwr, apparentKva: phaseCApp, reactiveKvar: phaseCReact },
      totalActiveKw: round(phaseAPwr + phaseBPwr + phaseCPwr, 2),
      totalApparentKva: round(phaseAApp + phaseBApp + phaseCApp, 2),
      totalReactiveKvar: round(phaseAReact + phaseBReact + phaseCReact, 2),
    },
    pcsStatus: {
      gridFreqHz: round(49.95 + rand() * 0.1, 2),
      gridPhaseSeq: "positive",
      deratCoeff: round(0.95 + rand() * 0.05, 3),
      workingCondition: workingConditions[Math.floor(rand() * workingConditions.length)],
      sourceMode: sourceModes[Math.floor(rand() * sourceModes.length)],
      gridMode: gridModes[Math.floor(rand() * gridModes.length)],
    },
    temperatures: {
      igbtTempC: round(35 + rand() * 25, 1),
      ambientTempC: round(20 + rand() * 15, 1),
      inductanceTempC: round(30 + rand() * 20, 1),
    },
    algoParams: {
      ccCvRechgV: round(bessUnit.nameplate.chemistry === "LFP" ? 3.45 : 4.1, 2),
      batCvChgV: round(bessUnit.nameplate.chemistry === "LFP" ? 3.65 : 4.2, 2),
      cvChgMaxTimeMin: Math.floor(30 + rand() * 60),
      cvToFloatI: round(0.05 + rand() * 0.05, 3),
      floatChgV: round(bessUnit.nameplate.chemistry === "LFP" ? 3.35 : 3.9, 2),
      floatToCcI: round(0.02 + rand() * 0.03, 3),
    },
  };
}

// --- PCS Fault Flag Generator ---
function generatePcsFaultFlags(seed: number): PcsFaultFlag[] {
  const rand = seededRandom(seed + 9000);
  const defs: { key: string; label: string; category: PcsFaultCategory }[] = [
    // Hardware
    { key: "hwIgbtFaultA", label: "IGBT Fault Phase A", category: "hardware" },
    { key: "hwIgbtFaultB", label: "IGBT Fault Phase B", category: "hardware" },
    { key: "hwIgbtFaultC", label: "IGBT Fault Phase C", category: "hardware" },
    { key: "hwIgbtOverTemp", label: "IGBT Over Temperature", category: "hardware" },
    { key: "hwFanFault1", label: "Fan 1 Fault", category: "hardware" },
    { key: "hwFanFault2", label: "Fan 2 Fault", category: "hardware" },
    { key: "hwFuseFaultA", label: "Fuse Fault Phase A", category: "hardware" },
    { key: "hwFuseFaultB", label: "Fuse Fault Phase B", category: "hardware" },
    { key: "hwFuseFaultC", label: "Fuse Fault Phase C", category: "hardware" },
    { key: "hwCtFault", label: "CT Sensor Fault", category: "hardware" },
    { key: "hwDspFault", label: "DSP Fault", category: "hardware" },
    { key: "hwAuxPowerFault", label: "Auxiliary Power Supply Fault", category: "hardware" },
    { key: "hwInductorOverTemp", label: "Inductor Over Temperature", category: "hardware" },
    { key: "hwAmbientOverTemp", label: "Ambient Over Temperature", category: "hardware" },
    { key: "hwDcRelayFault", label: "DC Relay Fault", category: "hardware" },
    { key: "hwPreChargeRelay", label: "Pre-charge Relay Fault", category: "hardware" },

    // Grid
    { key: "gridOverVoltageA", label: "Grid Over Voltage Phase A", category: "grid" },
    { key: "gridOverVoltageB", label: "Grid Over Voltage Phase B", category: "grid" },
    { key: "gridOverVoltageC", label: "Grid Over Voltage Phase C", category: "grid" },
    { key: "gridUnderVoltageA", label: "Grid Under Voltage Phase A", category: "grid" },
    { key: "gridUnderVoltageB", label: "Grid Under Voltage Phase B", category: "grid" },
    { key: "gridUnderVoltageC", label: "Grid Under Voltage Phase C", category: "grid" },
    { key: "gridOverFreq", label: "Grid Over Frequency", category: "grid" },
    { key: "gridUnderFreq", label: "Grid Under Frequency", category: "grid" },
    { key: "gridPhaseSeqFault", label: "Grid Phase Sequence Fault", category: "grid" },
    { key: "gridVoltageThdHigh", label: "Grid Voltage THD High", category: "grid" },
    { key: "gridCurrentThdHigh", label: "Grid Current THD High", category: "grid" },
    { key: "gridIslandDetect", label: "Grid Island Detection", category: "grid" },
    { key: "gridLossOfSync", label: "Grid Loss of Sync", category: "grid" },
    { key: "gridReclosingFault", label: "Grid Reclosing Fault", category: "grid" },
    { key: "gridVoltageUnbalance", label: "Grid Voltage Unbalance", category: "grid" },
    { key: "gridCurrentUnbalance", label: "Grid Current Unbalance", category: "grid" },

    // Bus / Battery
    { key: "busOverVoltage", label: "DC Bus Over Voltage", category: "bus_battery" },
    { key: "busUnderVoltage", label: "DC Bus Under Voltage", category: "bus_battery" },
    { key: "busGroundFault", label: "DC Bus Ground Fault", category: "bus_battery" },
    { key: "busVoltageUnbalance", label: "DC Bus Voltage Unbalance", category: "bus_battery" },
    { key: "batOverVoltage", label: "Battery Over Voltage", category: "bus_battery" },
    { key: "batUnderVoltage", label: "Battery Under Voltage", category: "bus_battery" },
    { key: "batOverCurrent", label: "Battery Over Current", category: "bus_battery" },
    { key: "batReversePolarity", label: "Battery Reverse Polarity", category: "bus_battery" },
    { key: "batInsulationLow", label: "Battery Insulation Low", category: "bus_battery" },
    { key: "busPreChargeFail", label: "Bus Pre-charge Failure", category: "bus_battery" },
    { key: "busSoftStartFail", label: "Bus Soft Start Failure", category: "bus_battery" },

    // AC Protection
    { key: "acOverCurrentA", label: "AC Over Current Phase A", category: "ac_protection" },
    { key: "acOverCurrentB", label: "AC Over Current Phase B", category: "ac_protection" },
    { key: "acOverCurrentC", label: "AC Over Current Phase C", category: "ac_protection" },
    { key: "acShortCircuit", label: "AC Short Circuit", category: "ac_protection" },
    { key: "acOverVoltage", label: "AC Output Over Voltage", category: "ac_protection" },
    { key: "acUnderVoltage", label: "AC Output Under Voltage", category: "ac_protection" },
    { key: "acOverPower", label: "AC Over Power", category: "ac_protection" },
    { key: "acGroundFault", label: "AC Ground Fault", category: "ac_protection" },
    { key: "acLeakageCurrent", label: "AC Leakage Current High", category: "ac_protection" },
    { key: "acDcInjection", label: "AC DC Injection High", category: "ac_protection" },
    { key: "acInductorOverCurrent", label: "Inductor Over Current", category: "ac_protection" },
    { key: "acNeutralOverCurrent", label: "Neutral Over Current", category: "ac_protection" },

    // System
    { key: "sysEmergencyStop", label: "Emergency Stop", category: "system" },
    { key: "sysSpdFault", label: "SPD (Surge Protector) Fault", category: "system" },
    { key: "sysDoorOpen", label: "Cabinet Door Open", category: "system" },
    { key: "sysFireDetect", label: "Fire Detection", category: "system" },
    { key: "sysWaterDetect", label: "Water Ingress Detection", category: "system" },
    { key: "sysModeConflict", label: "Operating Mode Conflict", category: "system" },
    { key: "sysConfigError", label: "Configuration Error", category: "system" },
    { key: "sysFirmwareMismatch", label: "Firmware Version Mismatch", category: "system" },
    { key: "sysCalibrationNeeded", label: "Calibration Needed", category: "system" },
    { key: "sysEepromFault", label: "EEPROM Fault", category: "system" },
    { key: "sysWatchdogReset", label: "Watchdog Reset", category: "system" },
    { key: "sysParamOutOfRange", label: "Parameter Out Of Range", category: "system" },

    // Relay
    { key: "relayAcContactorFault", label: "AC Contactor Fault", category: "relay" },
    { key: "relayDcContactorFault", label: "DC Contactor Fault", category: "relay" },
    { key: "relayPreChargeFault", label: "Pre-charge Relay Fault", category: "relay" },
    { key: "relayAntiPidFault", label: "Anti-PID Relay Fault", category: "relay" },
    { key: "relayGroundFault", label: "Ground Relay Fault", category: "relay" },
    { key: "relayBypassFault", label: "Bypass Relay Fault", category: "relay" },
    { key: "relayTransferFault", label: "Transfer Switch Fault", category: "relay" },
    { key: "relayNeutralFault", label: "Neutral Relay Fault", category: "relay" },

    // Inverter
    { key: "invSoftwareTrip", label: "Inverter Software Trip", category: "inverter" },
    { key: "invHardwareTrip", label: "Inverter Hardware Trip", category: "inverter" },
    { key: "invOverload", label: "Inverter Overload", category: "inverter" },
    { key: "invEfficiencyLow", label: "Inverter Efficiency Low", category: "inverter" },
    { key: "invPllLockFail", label: "PLL Lock Failure", category: "inverter" },
    { key: "invPwmFault", label: "PWM Generation Fault", category: "inverter" },
    { key: "invDeadTimeFault", label: "Dead Time Fault", category: "inverter" },
    { key: "invModulationError", label: "Modulation Error", category: "inverter" },
    { key: "invDcOffsetHigh", label: "DC Offset High", category: "inverter" },
    { key: "invStartupFail", label: "Startup Failure", category: "inverter" },
    { key: "invShutdownFail", label: "Shutdown Failure", category: "inverter" },
    { key: "invModeTransitionFail", label: "Mode Transition Failure", category: "inverter" },

    // Timeout
    { key: "timeoutBmsComm", label: "BMS Communication Timeout", category: "timeout" },
    { key: "timeoutEmsComm", label: "EMS Communication Timeout", category: "timeout" },
    { key: "timeoutScadaComm", label: "SCADA Communication Timeout", category: "timeout" },
    { key: "timeoutMeterComm", label: "Meter Communication Timeout", category: "timeout" },
    { key: "timeoutGridRelay", label: "Grid Relay Response Timeout", category: "timeout" },
    { key: "timeoutPreCharge", label: "Pre-charge Timeout", category: "timeout" },
    { key: "timeoutSyncTimeout", label: "Grid Sync Timeout", category: "timeout" },
    { key: "timeoutStartup", label: "Startup Timeout", category: "timeout" },
    { key: "timeoutShutdown", label: "Shutdown Timeout", category: "timeout" },
    { key: "timeoutModeSwitch", label: "Mode Switch Timeout", category: "timeout" },
    { key: "timeoutHeartbeat", label: "Heartbeat Timeout", category: "timeout" },
    { key: "timeoutFirmwareUpdate", label: "Firmware Update Timeout", category: "timeout" },

    // Additional hardware
    { key: "hwSurgeProtFault", label: "Surge Protector Fault", category: "hardware" },
    { key: "hwTransformerFault", label: "Transformer Fault", category: "hardware" },
    { key: "hwCapacitorFault", label: "DC Link Capacitor Fault", category: "hardware" },
    { key: "hwFilterFault", label: "EMI Filter Fault", category: "hardware" },

    // Additional grid
    { key: "gridRocofTrip", label: "ROCOF Trip", category: "grid" },
    { key: "gridVectorShift", label: "Vector Shift Trip", category: "grid" },

    // Additional system
    { key: "sysRtcFault", label: "RTC Clock Fault", category: "system" },
    { key: "sysLoggingFull", label: "Logging Storage Full", category: "system" },
    { key: "sysLicenseExpired", label: "License Expired", category: "system" },

    // Additional inverter
    { key: "invCurrentLoop", label: "Current Loop Fault", category: "inverter" },
    { key: "invVoltageLoop", label: "Voltage Loop Fault", category: "inverter" },
  ];

  return defs.map((d) => ({
    ...d,
    active: rand() < 0.03, // ~3-4 active out of 130+
  }));
}

// --- Generate PCS units ---
export const pcsUnits: PcsUnit[] = bessUnits.map((bu, i) =>
  generatePcsUnit(bu, 10000 + i * 333)
);

// --- Fault flag maps ---
const pcsFaultFlagMap = new Map<string, PcsFaultFlag[]>();
pcsUnits.forEach((pcs, i) => {
  pcsFaultFlagMap.set(pcs.id, generatePcsFaultFlags(10000 + i * 333));
});

// --- Accessor functions ---
export function getPcsUnitById(id: string): PcsUnit | undefined {
  return pcsUnits.find((u) => u.id === id);
}

export function getPcsUnitByBessId(bessUnitId: string): PcsUnit | undefined {
  return pcsUnits.find((u) => u.linkedBessUnitId === bessUnitId);
}

export function getPcsFaultFlagsForUnit(pcsId: string): PcsFaultFlag[] {
  return pcsFaultFlagMap.get(pcsId) ?? [];
}
