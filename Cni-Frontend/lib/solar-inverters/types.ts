export type InverterStatus = "normal" | "offline" | "warning" | "fault";

export interface MpptData {
  id: number;
  voltage: number | null;
  current: number | null;
}

export interface SolarInverter {
  id: string;
  deviceName: string;
  deviceModel: string;
  serialNumber: string;
  status: InverterStatus;
  yieldToday: number | null; // kWh
  totalActivePower: number | null; // kW
  associatedPlant: string;

  // Detail - Overview
  totalDcPower: number | null; // kW
  operatingStatus: string;
  phaseACurrent: number | null; // A
  phaseBCurrent: number | null; // A
  phaseCCurrent: number | null; // A
  totalReactivePower: number | null; // var
  insulationImpedance: number | null; // kOhm
  gridFrequency: number | null; // Hz
  abLineVoltage: number | null; // V
  bcLineVoltage: number | null; // V
  caLineVoltage: number | null; // V
  totalPowerFactor: number | null;
  totalYield: number | null; // MWh

  // MPPT data
  mpptData: MpptData[];

  // String currents (A)
  stringCurrents: (number | null)[];

  // Inverter unit
  totalApparentPower: number | null; // VA
  dailyOperatingTime: number | null; // min

  // Other
  dailyYieldTheoretical: number | null; // kWh

  // Device info
  commissioningDate: string;
  manufacturer: string;
}

export type InverterSortField =
  | "deviceName"
  | "deviceModel"
  | "serialNumber"
  | "status"
  | "yieldToday"
  | "totalActivePower"
  | "associatedPlant";

export type SortDirection = "asc" | "desc";

// Fault types
export type FaultSeverity = "critical" | "major" | "minor" | "warning";
export type FaultStatus = "active" | "recovered";

export interface InverterFault {
  id: string;
  inverterId: string;
  code: string;
  name: string;
  severity: FaultSeverity;
  status: FaultStatus;
  occurredAt: Date;
  recoveredAt: Date | null;
  relatedAlarmId: string;
}

// Curve data
export interface PowerCurvePoint {
  time: string;
  activePower: number;
  dcPower: number;
  reactivePower: number;
}

export interface VoltageCurvePoint {
  time: string;
  abVoltage: number;
  bcVoltage: number;
  caVoltage: number;
}

export interface CurrentCurvePoint {
  time: string;
  phaseA: number;
  phaseB: number;
  phaseC: number;
}

export interface InverterCurveData {
  power: PowerCurvePoint[];
  voltage: VoltageCurvePoint[];
  current: CurrentCurvePoint[];
}
