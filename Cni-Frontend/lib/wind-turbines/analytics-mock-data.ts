import { windTurbines, getCurveDataForTurbine } from "./mock-data";
import type { TurbineStatus } from "./types";

// --- Aggregation helpers ---

export function getFarmActivePower(): number {
  return windTurbines.reduce((sum, t) => sum + (t.activePower ?? 0), 0);
}

export function getFarmYieldToday(): number {
  return windTurbines.reduce((sum, t) => sum + (t.yieldToday ?? 0), 0);
}

export function getInstalledCapacity(): number {
  return windTurbines.reduce((sum, t) => sum + t.ratedPower, 0);
}

export function getDeviceStatusSummary(): Record<TurbineStatus, number> {
  const summary: Record<TurbineStatus, number> = { normal: 0, offline: 0, warning: 0, fault: 0 };
  for (const t of windTurbines) {
    summary[t.status]++;
  }
  return summary;
}

// --- Aggregate daily power curve (sum of all turbine curves) ---

export interface FarmPowerPoint {
  time: string;
  activePower: number;
}

export function getFarmPowerCurve(): FarmPowerPoint[] {
  const curvesMap = new Map<string, number>();

  for (const t of windTurbines) {
    if (t.status === "offline") continue;
    const data = getCurveDataForTurbine(t.id);
    for (const pt of data.power) {
      curvesMap.set(pt.time, (curvesMap.get(pt.time) ?? 0) + pt.activePower);
    }
  }

  return Array.from(curvesMap.entries())
    .sort(([a], [b]) => a.localeCompare(b))
    .map(([time, activePower]) => ({ time, activePower: +activePower.toFixed(1) }));
}

// --- Turbine yield comparison ---

export interface TurbineYieldItem {
  name: string;
  yield: number;
}

export function getTurbineYieldComparison(): TurbineYieldItem[] {
  return windTurbines
    .filter((t) => t.yieldToday !== null)
    .map((t) => ({ name: t.deviceName, yield: t.yieldToday! }))
    .sort((a, b) => b.yield - a.yield);
}

// --- Monthly energy production (last 12 months, mock) ---

export interface MonthlyYield {
  month: string;
  yield: number; // MWh
}

export function getMonthlyProduction(): MonthlyYield[] {
  const months = ["Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", "Jan", "Feb"];
  const yields = [1850, 1620, 1380, 980, 720, 650, 890, 1420, 1950, 2180, 2350, 2100];
  return months.map((month, i) => ({ month, yield: yields[i] }));
}

// --- Daily energy trend (last 30 days, mock) ---

export interface DailyYield {
  date: string;
  yield: number; // MWh
}

export function getDailyTrend(): DailyYield[] {
  const data: DailyYield[] = [];
  const today = new Date();
  for (let i = 29; i >= 0; i--) {
    const d = new Date(today);
    d.setDate(d.getDate() - i);
    const day = `${d.getDate()}/${d.getMonth() + 1}`;
    // Winter-ish wind: higher base with variation
    const base = 180 + Math.sin((i / 30) * Math.PI * 2) * 60;
    const noise = (Math.random() - 0.3) * 50;
    data.push({ date: day, yield: +Math.max(40, base + noise).toFixed(0) });
  }
  return data;
}

// --- Farm total yield ---

export function getFarmTotalYield(): number {
  return windTurbines.reduce((sum, t) => sum + (t.totalYield ?? 0), 0);
}

// --- Yield this month (mock) ---

export function getYieldThisMonth(): number {
  return 2100; // MWh
}

// --- Performance metrics ---

export function getCapacityFactor(): number {
  const totalPower = getFarmActivePower();
  const installed = getInstalledCapacity();
  return installed > 0 ? +((totalPower / installed) * 100).toFixed(1) : 0;
}

export function getAvailability(): number {
  const online = windTurbines.filter((t) => t.status !== "offline").length;
  return +((online / windTurbines.length) * 100).toFixed(1);
}

export function getMeanWindSpeed(): number {
  const operating = windTurbines.filter((t) => t.windSpeed !== null);
  if (operating.length === 0) return 0;
  const avg = operating.reduce((sum, t) => sum + (t.windSpeed ?? 0), 0) / operating.length;
  return +avg.toFixed(1);
}

export function getSpecificYield(): number {
  const yieldToday = getFarmYieldToday() * 1000; // kWh
  const installed = getInstalledCapacity();
  return installed > 0 ? +(yieldToday / installed).toFixed(2) : 0;
}

// --- Capacity factor trend (last 30 days) ---

export interface CapacityFactorPoint {
  date: string;
  capacityFactor: number;
}

export function getCapacityFactorTrend(): CapacityFactorPoint[] {
  const data: CapacityFactorPoint[] = [];
  const today = new Date();
  for (let i = 29; i >= 0; i--) {
    const d = new Date(today);
    d.setDate(d.getDate() - i);
    const day = `${d.getDate()}/${d.getMonth() + 1}`;
    const cf = 30 + Math.random() * 35; // 30-65%
    data.push({ date: day, capacityFactor: +cf.toFixed(1) });
  }
  return data;
}

// --- Specific yield per turbine ---

export interface SpecificYieldItem {
  name: string;
  specificYield: number; // kWh/kW
}

export function getSpecificYieldPerTurbine(): SpecificYieldItem[] {
  return windTurbines
    .filter((t) => t.yieldToday !== null)
    .map((t) => ({
      name: t.deviceName,
      specificYield: +((t.yieldToday! * 1000) / t.ratedPower).toFixed(2),
    }))
    .sort((a, b) => b.specificYield - a.specificYield);
}

// --- Power curve scatter data (wind speed vs power for all turbines) ---

export interface PowerCurveScatterPoint {
  windSpeed: number;
  power: number;
  turbine: string;
}

export function getPowerCurveScatter(): PowerCurveScatterPoint[] {
  const points: PowerCurveScatterPoint[] = [];
  for (const t of windTurbines) {
    if (t.status === "offline") continue;
    const data = getCurveDataForTurbine(t.id);
    // Sample every 6th point to avoid too many
    for (let i = 0; i < data.power.length; i += 6) {
      const pt = data.power[i];
      points.push({ windSpeed: pt.windSpeed, power: pt.activePower, turbine: t.deviceName });
    }
  }
  return points;
}

// --- Availability heatmap ---

export interface AvailabilityHeatmapRow {
  turbine: string;
  hours: number[]; // 24 values (0-23h), each 0-100%
}

export function getAvailabilityHeatmap(): AvailabilityHeatmapRow[] {
  return windTurbines.map((t) => {
    const hours: number[] = [];
    for (let h = 0; h < 24; h++) {
      if (t.status === "offline") {
        // Offline turbine: was available until hour 8
        hours.push(h < 8 ? 100 : 0);
      } else if (t.status === "warning") {
        // Warning: occasional dips
        hours.push(h >= 9 && h <= 12 ? 60 + Math.random() * 20 : 95 + Math.random() * 5);
      } else {
        hours.push(95 + Math.random() * 5);
      }
    }
    return { turbine: t.deviceName, hours: hours.map((v) => +v.toFixed(0)) };
  });
}
