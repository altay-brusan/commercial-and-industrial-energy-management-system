// BESS Status types
export type BessUnitStatus = "online" | "offline" | "fault" | "standby";
export type BessOperatingMode = "charging" | "discharging" | "idle" | "standby" | "balancing";
export type BessComponentStatus = "normal" | "warning" | "fault" | "offline";

// Hierarchy (bottom-up)
export interface BessCell {
  id: string;
  cellNumber: number;
  status: BessComponentStatus;
  voltage: number;
  current: number;
  temperature: number;
  soc: number;
  soh: number;
  internalResistance: number; // mOhm
  balancing: boolean;
  cycleCount: number;
}

export interface BessModule {
  id: string;
  moduleNumber: number;
  status: BessComponentStatus;
  voltage: number;
  current: number;
  soc: number;
  soh: number;
  tempMin: number;
  tempMax: number;
  tempAvg: number;
  cells: BessCell[];
}

export interface BessPack {
  id: string;
  packNumber: number;
  status: BessComponentStatus;
  voltage: number;
  current: number;
  soc: number;
  soh: number;
  tempMin: number;
  tempMax: number;
  tempAvg: number;
  modules: BessModule[];
}

export interface BessRack {
  id: string;
  rackNumber: number;
  status: BessComponentStatus;
  voltage: number;
  current: number;
  soc: number;
  soh: number;
  power: number;
  tempMin: number;
  tempMax: number;
  tempAvg: number;
  packs: BessPack[];
}

export interface BessUnit {
  id: string;
  name: string;
  status: BessUnitStatus;
  operatingMode: BessOperatingMode;
  nameplate: {
    brand: string;
    model: string;
    serialNumber: string;
    chemistry: string;
    commissioningDate: string;
    capacityKWh: number;
    maxPowerKW: number;
    nominalVoltage: number;
    voltageRange: [number, number];
    rackCount: number;
    packsPerRack: number;
    modulesPerPack: number;
    cellsPerModule: number;
  };
  live: {
    soc: number;
    soh: number;
    power: number; // kW, positive=discharge, negative=charge
    voltage: number;
    current: number;
    frequency: number;
    tempAvg: number;
    tempMin: number;
    tempMax: number;
    remainingEnergyKwh: number;
    availableEnergyKwh: number;
    dmccA: number; // max charge current limit
    dmdcA: number; // max discharge current limit
    contactorPositive: boolean;
    contactorNegative: boolean;
    isolationStatus: number; // kOhm
    cellVoltageMaxMv: number;
    cellVoltageMinMv: number;
    avgCellVoltageMv: number;
    cellVoltageMaxLoc: { bmuLoc: number; cellLoc: number };
    cellVoltageMinLoc: { bmuLoc: number; cellLoc: number };
    cellTempMaxC: number;
    cellTempMinC: number;
    cellTempMaxLoc: { bmuLoc: number; cellLoc: number };
    cellTempMinLoc: { bmuLoc: number; cellLoc: number };
    outVoltPosV: number;
    outVoltNegV: number;
    stateCode: number;
  };
  lifecycle: {
    // Aging & Degradation
    cycleCount: number;
    equivalentFullCycles: number;
    designedCycleLife: number;
    designedCalendarLifeYears: number;
    averageDoD: number; // %
    sohTrendMonthly: number[]; // last 12 months SoH readings

    // Energy Throughput
    lifetimeChargedMWh: number;
    lifetimeDischargedMWh: number;
    dailyChargedKWh: number;
    dailyDischargedKWh: number;
    roundTripEfficiency: number; // %
    initialRoundTripEfficiency: number; // %

    // Operational History
    totalOperatingHours: number;
    totalStandbyHours: number;
    lastMaintenanceDate: string;
    nextMaintenanceDue: string;
    warrantyExpiryDate: string;
    firmwareVersion: string;

    // Thermal Stress
    maxRecordedTemp: number; // °C
    thermalExcursionCount: number;
    avgOperatingTemp: number; // °C
  };
  connectedDevices: {
    inverter: string;
    meter: string;
  };
  activeAlarmCount: number;
  racks: BessRack[];
}

// Fault types (follows inverter fault pattern)
export type FaultSeverity = "critical" | "major" | "minor" | "warning";
export type FaultStatus = "active" | "recovered";

export interface BessFault {
  id: string;
  unitId: string;
  code: string;
  name: string;
  severity: FaultSeverity;
  status: FaultStatus;
  source: string;
  occurredAt: Date;
  recoveredAt: Date | null;
  relatedAlarmId: string;
}

// Curve data types (5-min intervals, 24h)
export interface BessPowerCurvePoint {
  time: string;
  power: number;
  soc: number;
}

export interface BessVoltageCurvePoint {
  time: string;
  voltage: number;
  current: number;
}

export interface BessTemperatureCurvePoint {
  time: string;
  tempAvg: number;
  tempMin: number;
  tempMax: number;
}

export interface BessCurveData {
  power: BessPowerCurvePoint[];
  voltage: BessVoltageCurvePoint[];
  temperature: BessTemperatureCurvePoint[];
}

// Hierarchy navigation
export type HierarchyLevel = "unit" | "rack" | "pack" | "module" | "cell";

export interface HierarchyPath {
  unitId: string;
  rackId?: string;
  packId?: string;
  moduleId?: string;
  cellId?: string;
}

// Sort fields for overview table
export type BessOverviewSortField =
  | "name"
  | "status"
  | "mode"
  | "soc"
  | "soh"
  | "power"
  | "capacity"
  | "alarms";

// Sort fields for hierarchy table
export type HierarchySortField =
  | "label"
  | "status"
  | "soc"
  | "soh"
  | "voltage"
  | "current"
  | "tempMin"
  | "tempMax"
  | "tempAvg"
  | "temperature";

export type SortDirection = "asc" | "desc";

// Battery Fault Flag types
export type BatteryFaultCategory = "pack" | "cell" | "module" | "communication" | "board";

export interface BatteryFaultFlag {
  key: string;
  label: string;
  category: BatteryFaultCategory;
  severity: "danger" | "warning";
  active: boolean;
}

// Chiller / TMS types
export type ChillerFaultCategory =
  | "sensor"
  | "power"
  | "system"
  | "pump"
  | "pressure"
  | "temperature"
  | "communication"
  | "compressor_vfd"
  | "lock";

export interface ChillerAlarmFlag {
  key: string;
  label: string;
  category: ChillerFaultCategory;
  active: boolean;
}

export interface BessChillerState {
  operatingMode: number;
  operatingModeStr: string;
  outletCoolantTempC: number;
  inletCoolantTempC: number;
  ambientTempC: number;
  inletCoolantPressure: number;
  outletCoolantPressure: number;
  compressorStatus: boolean;
  pumpStatus: boolean;
  fanRunStatus: boolean;
  electricHeaterStatus: boolean;
  compressorSpeed: number;
  pumpSpeed: number;
}

// HVCU types
export interface BessHvcuState {
  bcuStates: Array<{ id: number; controlState: number; stateStr: string }>;
  isolationControl: boolean;
}

// --- Device Tree types ---

export type BessDeviceType = "unit" | "rack" | "esc" | "hvac";

export interface BessSelectedDevice {
  id: string;
  type: BessDeviceType;
  unitId: string;
}

export interface BessDeviceTreeNode {
  id: string;
  name: string;
  type: BessDeviceType;
  status: BessUnitStatus | BessComponentStatus;
  parentUnitId: string;
  children?: BessDeviceTreeNode[];
  alarmCount: number;
}

// --- ESC (DC-DC Controller) ---

export interface BessEsc {
  id: string;
  unitId: string;
  deviceInfo: {
    model: string;
    serialNumber: string;
    firmware: string;
    protocol: string;
    modbusAddress: number;
  };
  live: {
    dcVoltageIn: number;
    dcVoltageOut: number;
    dcCurrent: number;
    power: number;
    efficiency: number;
    temperature: number;
    isolationResistance: number;
    operatingState: string;
  };
  status: BessComponentStatus;
  activeAlarmCount: number;
}

// --- HVAC (Thermal Management) ---

export interface BessHvac {
  id: string;
  unitId: string;
  deviceInfo: {
    model: string;
    serialNumber: string;
    firmware: string;
    protocol: string;
    modbusAddress: number;
  };
  chillerState: BessChillerState;
  setpoints: {
    coolantTempSetpoint: number;
    highAlarm: number;
    lowAlarm: number;
    operatingMode: string;
  };
  status: BessComponentStatus;
  activeAlarmCount: number;
}

// --- Settings types ---

export interface BessUnitSettings {
  deviceName: string;
  protocol: string;
  firmwareVersion: string;
  serialNumber: string;
  modbusAddress: number;
  maintenanceIntervalDays: number;
  tempAlarmThreshold: number;
  socLowAlarm: number;
  socHighAlarm: number;
}

export interface BessRackSettings {
  socCalibrationIntervalHours: number;
  chargeStopSoc: number;
  dischargeStopSoc: number;
  operatingMode: string;
  insulationResistanceThreshold: number;
  balancingEnabled: boolean;
  balancingThresholdMv: number;
}

export interface BessEscSettings {
  deviceName: string;
  model: string;
  serialNumber: string;
  firmware: string;
  protocol: string;
  modbusAddress: number;
  isolationResistanceThreshold: number;
  maxPowerLimit: number;
  autoRestart: boolean;
}

export interface BessHvacSettings {
  operatingMode: string;
  coolantTempSetpoint: number;
  highTempAlarm: number;
  lowTempAlarm: number;
  compressorMaxSpeed: number;
  pumpMaxSpeed: number;
  autoDefrost: boolean;
}

// --- Tab configurations ---

export interface TabConfig {
  id: string;
  label: string;
  disabled?: boolean;
}

export const UNIT_TABS: TabConfig[] = [
  { id: "summary", label: "Summary" },
  { id: "hierarchy", label: "Hierarchy" },
  { id: "monitoring", label: "Monitoring" },
  { id: "health", label: "Health" },
  { id: "fault", label: "Fault" },
  { id: "alarms", label: "Alarms" },
  { id: "settings", label: "Settings" },
];

export const RACK_TABS: TabConfig[] = [
  { id: "summary", label: "Summary" },
  { id: "hierarchy", label: "Hierarchy" },
  { id: "alarms", label: "Alarms" },
  { id: "settings", label: "Settings" },
];

export const ESC_TABS: TabConfig[] = [
  { id: "details", label: "Details" },
  { id: "alarms", label: "Alarms" },
  { id: "settings", label: "Settings" },
];

export const HVAC_TABS: TabConfig[] = [
  { id: "monitoring", label: "Monitoring" },
  { id: "alarms", label: "Alarms" },
  { id: "settings", label: "Settings" },
];
