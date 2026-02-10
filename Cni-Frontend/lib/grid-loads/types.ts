export type GridDeviceStatus = "normal" | "offline" | "warning" | "fault" | "tripped";

export type GridDeviceType =
  | "connection-point"
  | "transformer"
  | "power-meter"
  | "protection-relay"
  | "circuit-breaker"
  | "switchgear";

export interface GridDevice {
  id: string;
  deviceName: string;
  deviceType: GridDeviceType;
  deviceModel: string;
  serialNumber: string;
  status: GridDeviceStatus;
  location: string;

  // Electrical
  activePower: number | null; // kW
  reactivePower: number | null; // kVar
  apparentPower: number | null; // kVA
  powerFactor: number | null;
  frequency: number | null; // Hz
  powerFlowDirection: "import" | "export" | null;

  // 3-phase
  voltagePhaseA: number | null; // V
  voltagePhaseB: number | null; // V
  voltagePhaseC: number | null; // V
  currentPhaseA: number | null; // A
  currentPhaseB: number | null; // A
  currentPhaseC: number | null; // A
  thd: number | null; // % total harmonic distortion

  // Transformer-specific
  oilTemperature: number | null; // °C
  windingTemperature: number | null; // °C
  tapPosition: number | null;
  loadingPercent: number | null; // %
  voltageRatio: string | null; // e.g. "34.5kV / 0.4kV"

  // Breaker/Relay-specific
  breakerPosition: "closed" | "open" | "tripped" | null;
  relayTripStatus: boolean | null;
  lastTripTime: Date | null;

  // Energy
  importedToday: number | null; // kWh
  exportedToday: number | null; // kWh
  totalImported: number | null; // MWh
  totalExported: number | null; // MWh

  // Device info
  commissioningDate: string;
  manufacturer: string;
}

export type GridDeviceSortField =
  | "deviceName"
  | "deviceType"
  | "deviceModel"
  | "status"
  | "activePower"
  | "powerFactor"
  | "location";

export type SortDirection = "asc" | "desc";

// Fault types
export type FaultSeverity = "critical" | "major" | "minor" | "warning";
export type FaultStatus = "active" | "recovered";

export interface GridFault {
  id: string;
  deviceId: string;
  code: string;
  name: string;
  severity: FaultSeverity;
  status: FaultStatus;
  occurredAt: Date;
  recoveredAt: Date | null;
  relatedAlarmId: string;
}

// Curve data
export interface GridPowerCurvePoint {
  time: string;
  activePower: number;
  reactivePower: number;
  apparentPower: number;
}

export interface GridVoltageCurvePoint {
  time: string;
  phaseA: number;
  phaseB: number;
  phaseC: number;
}

export interface GridFrequencyCurvePoint {
  time: string;
  frequency: number;
}

export interface GridPowerFactorCurvePoint {
  time: string;
  powerFactor: number;
}

export interface GridCurveData {
  power: GridPowerCurvePoint[];
  voltage: GridVoltageCurvePoint[];
  frequency: GridFrequencyCurvePoint[];
  powerFactor: GridPowerFactorCurvePoint[];
}
