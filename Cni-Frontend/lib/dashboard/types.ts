// Solar Metrics
export interface SolarMetrics {
  currentPower: number;
  dailyGeneration: number;
  capacity: number;
  status: "generating" | "idle" | "fault";
}

// Battery Metrics
export interface BatteryMetrics {
  soc: number;
  power: number;
  mode: "charging" | "discharging" | "idle" | "standby";
  temperature: number;
  voltage: number;
  current: number;
  cycleCount: number;
  stateOfHealth: number;
  capacity: number;
  todayCharged: number;
  todayDischarged: number;
}

// Grid Metrics
export interface GridMetrics {
  connectionStatus: "connected" | "disconnected" | "island";
  quality: "excellent" | "good" | "fair" | "poor";
  importPower: number;
  exportPower: number;
  frequency: number;
  voltage: number;
  powerFactor: number;
  tariff: "peak" | "off-peak" | "shoulder";
}

// Power Flow Data
export interface PowerFlowData {
  solarPower: number;
  batteryPower: number;
  gridPower: number;
  loadPower: number;
  batterySoc: number;
}

// Dashboard Device
export interface DashboardDevice {
  id: string;
  name: string;
  type: "solar_inverter" | "battery" | "grid_meter" | "load_meter" | "weather_station";
  manufacturer: string;
  model: string;
  serialNumber: string;
  status: "online" | "offline" | "warning" | "error";
  lastSeen: Date;
  location: string;
  specifications: Record<string, string>;
}

// Detail Panel State (discriminated union)
export type DetailPanelState =
  | { type: "solar" }
  | { type: "battery" }
  | { type: "grid" }
  | { type: "device"; deviceId: string }
  | { type: "alert"; alertId: string }
  | null;
