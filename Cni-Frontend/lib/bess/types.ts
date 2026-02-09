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
  };
  lifecycle: {
    cycleCount: number;
    lifetimeThroughputMWh: number;
    dailyChargedKWh: number;
    dailyDischargedKWh: number;
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
