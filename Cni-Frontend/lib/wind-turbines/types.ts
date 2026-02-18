export type TurbineStatus = "normal" | "offline" | "warning" | "fault";

export interface WindTurbine {
  id: string;
  deviceName: string;
  deviceModel: string;
  serialNumber: string;
  status: TurbineStatus;
  yieldToday: number | null; // MWh
  activePower: number | null; // kW
  ratedPower: number; // kW
  windFarm: string;

  // Wind conditions
  windSpeed: number | null; // m/s
  windDirection: number | null; // degrees
  ambientTemp: number | null; // °C

  // Rotor & Generator
  rotorSpeed: number | null; // rpm
  generatorSpeed: number | null; // rpm
  generatorPower: number | null; // kW

  // Electrical
  reactivePower: number | null; // kVar
  powerFactor: number | null;
  gridFrequency: number | null; // Hz
  gridVoltage: number | null; // V

  // Pitch & Yaw
  pitchAngles: [number | null, number | null, number | null]; // 3 blades, degrees
  nacellePosition: number | null; // degrees
  yawError: number | null; // degrees

  // Temperatures
  generatorTemp: number | null; // °C
  gearboxOilTemp: number | null; // °C
  mainBearingTemp: number | null; // °C
  controlCabinetTemp: number | null; // °C

  // Cumulative
  totalYield: number | null; // MWh
  totalOperatingHours: number | null;
  dailyOperatingTime: number | null; // hours

  // Device info
  hubHeight: number; // m
  rotorDiameter: number; // m
  commissioningDate: string;
  manufacturer: string;
}

export type TurbineSortField =
  | "deviceName"
  | "deviceModel"
  | "serialNumber"
  | "status"
  | "yieldToday"
  | "activePower"
  | "windFarm";

export type SortDirection = "asc" | "desc";

// Fault types
export type FaultSeverity = "critical" | "major" | "minor" | "warning";
export type FaultStatus = "active" | "recovered";

export interface TurbineFault {
  id: string;
  turbineId: string;
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
  windSpeed: number;
  reactivePower: number;
}

export interface WindSpeedCurvePoint {
  time: string;
  windSpeed: number;
  rotorSpeed: number;
}

export interface PitchCurvePoint {
  time: string;
  pitchAngle: number;
  activePower: number;
}

export interface TemperatureCurvePoint {
  time: string;
  generator: number;
  gearboxOil: number;
  mainBearing: number;
}

export interface TurbineCurveData {
  power: PowerCurvePoint[];
  windSpeed: WindSpeedCurvePoint[];
  pitch: PitchCurvePoint[];
  temperature: TemperatureCurvePoint[];
}
