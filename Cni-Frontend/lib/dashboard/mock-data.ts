import type {
  SolarMetrics,
  BatteryMetrics,
  GridMetrics,
  PowerFlowData,
  DashboardDevice,
} from "./types";

// Solar Metrics
export const solarMetrics: SolarMetrics = {
  currentPower: 68.5,
  dailyGeneration: 245.8,
  capacity: 100,
  status: "generating",
};

// Battery Metrics
export const batteryMetrics: BatteryMetrics = {
  soc: 72,
  power: 15.3,
  mode: "charging",
  temperature: 32,
  voltage: 768,
  current: 19.9,
  cycleCount: 847,
  stateOfHealth: 94.2,
  capacity: 200,
  todayCharged: 48.5,
  todayDischarged: 32.1,
};

// Grid Metrics
export const gridMetrics: GridMetrics = {
  connectionStatus: "connected",
  quality: "good",
  importPower: 12.4,
  exportPower: 0,
  frequency: 50.01,
  voltage: 400,
  powerFactor: 0.98,
  tariff: "peak",
};

// Power Flow Data Generator
export function generatePowerFlowData(): PowerFlowData {
  const hour = new Date().getHours();
  const isSunny = hour >= 6 && hour <= 18;

  const baseSolar = isSunny ? 45 + Math.random() * 25 : 0;
  const solarPower = Math.round(baseSolar * 10) / 10;

  const loadPower = Math.round((35 + Math.random() * 30) * 10) / 10;

  const solarExcess = solarPower - loadPower;
  let batteryPower: number;
  let gridPower: number;

  if (solarExcess > 0) {
    batteryPower = Math.round(Math.min(solarExcess * 0.7, 25) * 10) / 10;
    gridPower = Math.round((solarExcess - batteryPower) * -1 * 10) / 10;
  } else {
    batteryPower = Math.round(Math.min(Math.abs(solarExcess) * 0.3, 20) * -1 * 10) / 10;
    gridPower = Math.round((Math.abs(solarExcess) - Math.abs(batteryPower)) * 10) / 10;
  }

  return {
    solarPower,
    batteryPower,
    gridPower,
    loadPower,
    batterySoc: batteryMetrics.soc,
  };
}

// Energy Chart Data (24-hour)
export const energyChartData = [
  { time: "00:00", solar: 0, consumption: 18.2, grid: 18.2 },
  { time: "01:00", solar: 0, consumption: 15.5, grid: 15.5 },
  { time: "02:00", solar: 0, consumption: 14.1, grid: 14.1 },
  { time: "03:00", solar: 0, consumption: 13.8, grid: 13.8 },
  { time: "04:00", solar: 0, consumption: 14.5, grid: 14.5 },
  { time: "05:00", solar: 0, consumption: 16.2, grid: 16.2 },
  { time: "06:00", solar: 5.2, consumption: 22.4, grid: 17.2 },
  { time: "07:00", solar: 18.5, consumption: 35.1, grid: 16.6 },
  { time: "08:00", solar: 35.8, consumption: 42.3, grid: 6.5 },
  { time: "09:00", solar: 52.1, consumption: 48.7, grid: 0 },
  { time: "10:00", solar: 62.4, consumption: 51.2, grid: 0 },
  { time: "11:00", solar: 68.9, consumption: 49.5, grid: 0 },
  { time: "12:00", solar: 72.3, consumption: 45.8, grid: 0 },
  { time: "13:00", solar: 70.1, consumption: 47.2, grid: 0 },
  { time: "14:00", solar: 65.5, consumption: 50.8, grid: 0 },
  { time: "15:00", solar: 55.2, consumption: 52.1, grid: 0 },
  { time: "16:00", solar: 42.8, consumption: 55.4, grid: 12.6 },
  { time: "17:00", solar: 28.3, consumption: 58.2, grid: 29.9 },
  { time: "18:00", solar: 12.1, consumption: 62.5, grid: 50.4 },
  { time: "19:00", solar: 2.5, consumption: 58.8, grid: 56.3 },
  { time: "20:00", solar: 0, consumption: 52.1, grid: 52.1 },
  { time: "21:00", solar: 0, consumption: 42.5, grid: 42.5 },
  { time: "22:00", solar: 0, consumption: 32.8, grid: 32.8 },
  { time: "23:00", solar: 0, consumption: 24.2, grid: 24.2 },
];

// Dashboard Devices
export const devices: DashboardDevice[] = [
  {
    id: "dev-001",
    name: "Solar Inverter #1",
    type: "solar_inverter",
    manufacturer: "SMA",
    model: "Sunny Tripower 25000TL",
    serialNumber: "SMA-2023-A1042",
    status: "online",
    lastSeen: new Date("2026-02-08T12:30:00"),
    location: "Building A - Rooftop",
    specifications: {
      "Rated Power": "25 kW",
      "Max DC Voltage": "1000 V",
      "MPPT Channels": "3",
      "Efficiency": "98.3%",
      "Firmware": "v3.2.1",
    },
  },
  {
    id: "dev-002",
    name: "Solar Inverter #2",
    type: "solar_inverter",
    manufacturer: "SMA",
    model: "Sunny Tripower 25000TL",
    serialNumber: "SMA-2023-A1043",
    status: "warning",
    lastSeen: new Date("2026-02-08T12:28:00"),
    location: "Building B - Rooftop",
    specifications: {
      "Rated Power": "25 kW",
      "Max DC Voltage": "1000 V",
      "MPPT Channels": "3",
      "Efficiency": "96.1%",
      "Firmware": "v3.1.8",
    },
  },
  {
    id: "dev-003",
    name: "BESS Controller",
    type: "battery",
    manufacturer: "Tesla",
    model: "Megapack Controller",
    serialNumber: "TSL-MP-2024-0892",
    status: "online",
    lastSeen: new Date("2026-02-08T12:30:00"),
    location: "Storage Facility",
    specifications: {
      "Capacity": "200 kWh",
      "Max Power": "100 kW",
      "Voltage Range": "600-900 V",
      "Chemistry": "LFP",
      "Firmware": "v5.0.3",
    },
  },
  {
    id: "dev-004",
    name: "Grid Power Meter",
    type: "grid_meter",
    manufacturer: "Schneider Electric",
    model: "ION8650",
    serialNumber: "SE-ION-2023-7821",
    status: "online",
    lastSeen: new Date("2026-02-08T12:30:00"),
    location: "Main Building - Switchgear",
    specifications: {
      "Measurement": "3-Phase",
      "Accuracy": "Class 0.2S",
      "CT Ratio": "2000/5",
      "Protocol": "Modbus TCP",
    },
  },
  {
    id: "dev-005",
    name: "Load Meter",
    type: "load_meter",
    manufacturer: "Schneider Electric",
    model: "PM5560",
    serialNumber: "SE-PM-2023-4512",
    status: "online",
    lastSeen: new Date("2026-02-08T12:29:00"),
    location: "Main Building - MDB",
    specifications: {
      "Measurement": "3-Phase",
      "Accuracy": "Class 0.5S",
      "Display": "Color LCD",
      "Protocol": "Modbus RTU",
    },
  },
  {
    id: "dev-006",
    name: "Weather Station",
    type: "weather_station",
    manufacturer: "Campbell Scientific",
    model: "CR1000X",
    serialNumber: "CS-CR-2023-1234",
    status: "offline",
    lastSeen: new Date("2026-02-08T10:15:00"),
    location: "Building A - Rooftop",
    specifications: {
      "Sensors": "Irradiance, Temp, Wind, Humidity",
      "Data Interval": "1 min",
      "Storage": "4 GB",
      "Communication": "Ethernet / 4G",
    },
  },
];

// Helper
export function getDeviceById(id: string): DashboardDevice | undefined {
  return devices.find((d) => d.id === id);
}
