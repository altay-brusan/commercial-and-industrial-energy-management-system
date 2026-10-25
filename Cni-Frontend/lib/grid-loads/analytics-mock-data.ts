import { gridDevices, getCurveDataForDevice } from "./mock-data";
import type { GridDeviceStatus } from "./types";

// --- Aggregation helpers ---

export function getNetPower(): number {
  return gridDevices.reduce((sum, d) => sum + (d.activePower ?? 0), 0);
}

export function getTotalImportedToday(): number {
  return gridDevices.reduce((sum, d) => sum + (d.importedToday ?? 0), 0);
}

export function getTotalExportedToday(): number {
  return gridDevices.reduce((sum, d) => sum + (d.exportedToday ?? 0), 0);
}

export function getAvgPowerFactor(): number {
  const devices = gridDevices.filter((d) => d.powerFactor !== null);
  if (devices.length === 0) return 0;
  const avg = devices.reduce((sum, d) => sum + (d.powerFactor ?? 0), 0) / devices.length;
  return +avg.toFixed(3);
}

export function getDeviceStatusSummary(): Record<GridDeviceStatus, number> {
  const summary: Record<GridDeviceStatus, number> = { normal: 0, offline: 0, warning: 0, fault: 0, tripped: 0 };
  for (const d of gridDevices) {
    summary[d.status]++;
  }
  return summary;
}

export function getPeakDemandToday(): number {
  // Mock: peak is typically 115-130% of current net power
  return +(getNetPower() * 1.22).toFixed(0);
}

// --- Power Flow Over Time (import/export separate) ---

export interface GridPowerFlowPoint {
  time: string;
  imported: number;
  exported: number;
}

export function getGridPowerCurve(): GridPowerFlowPoint[] {
  const importMap = new Map<string, number>();
  const exportMap = new Map<string, number>();

  for (const d of gridDevices) {
    if (d.status === "offline" || d.activePower === null) continue;
    const data = getCurveDataForDevice(d.id);
    for (const pt of data.power) {
      if (d.powerFlowDirection === "export") {
        exportMap.set(pt.time, (exportMap.get(pt.time) ?? 0) + Math.abs(pt.activePower));
      } else {
        importMap.set(pt.time, (importMap.get(pt.time) ?? 0) + pt.activePower);
      }
    }
  }

  const allTimes = new Set([...importMap.keys(), ...exportMap.keys()]);
  return Array.from(allTimes)
    .sort((a, b) => a.localeCompare(b))
    .map((time) => ({
      time,
      imported: +(importMap.get(time) ?? 0).toFixed(1),
      exported: +(-(exportMap.get(time) ?? 0)).toFixed(1),
    }));
}

// --- Device Energy Comparison (import/export per device) ---

export interface DeviceEnergyItem {
  name: string;
  imported: number;
  exported: number;
}

export function getDeviceEnergyComparison(): DeviceEnergyItem[] {
  return gridDevices
    .filter((d) => d.importedToday !== null || d.exportedToday !== null)
    .map((d) => ({
      name: d.deviceName,
      imported: d.importedToday ?? 0,
      exported: d.exportedToday ?? 0,
    }))
    .sort((a, b) => (b.imported + b.exported) - (a.imported + a.exported));
}

// --- Monthly Energy Balance ---

export interface MonthlyEnergyBalance {
  month: string;
  imported: number; // MWh
  exported: number; // MWh
}

export function getMonthlyEnergyBalance(): MonthlyEnergyBalance[] {
  const months = ["Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", "Jan", "Feb"];
  return months.map((month, i) => {
    // Import higher in winter, export higher in summer (solar export)
    const winterFactor = Math.cos((i / 12) * Math.PI * 2);
    const baseImport = 420 + winterFactor * 150;
    const baseExport = 180 - winterFactor * 80;
    return {
      month,
      imported: +(baseImport + (Math.random() - 0.5) * 40).toFixed(0),
      exported: +(Math.max(60, baseExport + (Math.random() - 0.5) * 30)).toFixed(0),
    };
  });
}

// --- Daily Peak Demand ---

export interface DailyPeakDemandPoint {
  date: string;
  peakDemand: number; // kW
}

export function getDailyPeakDemand(): DailyPeakDemandPoint[] {
  const data: DailyPeakDemandPoint[] = [];
  const today = new Date();
  for (let i = 29; i >= 0; i--) {
    const d = new Date(today);
    d.setDate(d.getDate() - i);
    const day = `${d.getDate()}/${d.getMonth() + 1}`;
    const base = 4200 + Math.sin((i / 30) * Math.PI * 2) * 800;
    const noise = (Math.random() - 0.5) * 600;
    data.push({ date: day, peakDemand: +Math.max(2800, base + noise).toFixed(0) });
  }
  return data;
}

// --- Performance metrics ---

export function getGridAvailability(): number {
  const online = gridDevices.filter((d) => d.status !== "offline").length;
  return +((online / gridDevices.length) * 100).toFixed(1);
}

export function getVoltageCompliance(): number {
  // % of time voltage stayed within ±10%
  return +(97.2 + Math.random() * 2).toFixed(1);
}

export function getPowerQualityIndex(): number {
  // PQI: composite score 0-100
  return +(92 + Math.random() * 6).toFixed(1);
}

// --- Voltage Profile (3-phase over 24h) ---

export interface VoltageProfilePoint {
  time: string;
  phaseA: number;
  phaseB: number;
  phaseC: number;
}

export function getVoltageProfile(): VoltageProfilePoint[] {
  const data: VoltageProfilePoint[] = [];
  for (let h = 0; h < 24; h++) {
    const time = `${String(h).padStart(2, "0")}:00`;
    const base = 5750;
    // Voltage sags during peak load
    const loadDip = h >= 10 && h <= 14 ? -30 : h >= 18 && h <= 21 ? -40 : 0;
    data.push({
      time,
      phaseA: +(base + loadDip + (Math.random() - 0.5) * 30).toFixed(0),
      phaseB: +(base - 15 + loadDip + (Math.random() - 0.5) * 30).toFixed(0),
      phaseC: +(base + 10 + loadDip + (Math.random() - 0.5) * 30).toFixed(0),
    });
  }
  return data;
}

// --- Frequency Trend ---

export interface FrequencyTrendPoint {
  time: string;
  frequency: number;
}

export function getFrequencyTrend(): FrequencyTrendPoint[] {
  const data: FrequencyTrendPoint[] = [];
  for (let h = 0; h < 24; h++) {
    const time = `${String(h).padStart(2, "0")}:00`;
    data.push({
      time,
      frequency: +(50 + (Math.random() - 0.5) * 0.15).toFixed(3),
    });
  }
  return data;
}

// --- Power Factor Trend ---

export interface PowerFactorTrendPoint {
  date: string;
  powerFactor: number;
}

export function getPowerFactorTrend(): PowerFactorTrendPoint[] {
  const data: PowerFactorTrendPoint[] = [];
  const today = new Date();
  for (let i = 29; i >= 0; i--) {
    const d = new Date(today);
    d.setDate(d.getDate() - i);
    const day = `${d.getDate()}/${d.getMonth() + 1}`;
    data.push({
      date: day,
      powerFactor: +(0.95 + Math.random() * 0.04).toFixed(3),
    });
  }
  return data;
}

// --- Transformer Loading Heatmap ---

export interface TransformerLoadingRow {
  device: string;
  hours: number[]; // 24 values, each 0-100%
}

export function getTransformerLoadingHeatmap(): TransformerLoadingRow[] {
  const transformers = gridDevices.filter((d) => d.deviceType === "transformer");
  return transformers.map((t) => {
    const hours: number[] = [];
    const baseLoading = t.loadingPercent ?? 60;
    for (let h = 0; h < 24; h++) {
      // Loading follows load profile
      const morningPeak = Math.exp(-0.5 * Math.pow((h - 11) / 2, 2));
      const eveningPeak = Math.exp(-0.5 * Math.pow((h - 19) / 2, 2));
      const factor = 0.4 + 0.35 * morningPeak + 0.45 * eveningPeak;
      const loading = baseLoading * factor * (1 + (Math.random() - 0.5) * 0.15);
      hours.push(+Math.min(100, Math.max(10, loading)).toFixed(0));
    }
    return { device: t.deviceName, hours };
  });
}
