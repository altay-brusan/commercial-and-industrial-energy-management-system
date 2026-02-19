import { solarInverters, getCurveDataForInverter } from "./mock-data";
import type { InverterStatus } from "./types";

// --- Aggregation helpers ---

export function getPlantActivePower(): number {
  return solarInverters.reduce((sum, inv) => sum + (inv.totalActivePower ?? 0), 0);
}

export function getPlantYieldToday(): number {
  return solarInverters.reduce((sum, inv) => sum + (inv.yieldToday ?? 0), 0);
}

export function getPlantTotalYield(): number {
  return solarInverters.reduce((sum, inv) => sum + (inv.totalYield ?? 0), 0);
}

export function getInstalledCapacity(): number {
  return solarInverters.length * 125; // kWp per SG125HX
}

export function getDeviceStatusSummary(): Record<InverterStatus, number> {
  const summary: Record<InverterStatus, number> = { online: 0, offline: 0, warning: 0, fault: 0 };
  for (const inv of solarInverters) {
    summary[inv.status]++;
  }
  return summary;
}

// --- Aggregate daily power curve (sum of all inverter curves) ---

export interface PlantPowerPoint {
  time: string;
  activePower: number;
}

export function getPlantPowerCurve(): PlantPowerPoint[] {
  const curvesMap = new Map<string, number>();

  for (const inv of solarInverters) {
    if (inv.status === "offline") continue;
    const data = getCurveDataForInverter(inv.id);
    for (const pt of data.power) {
      curvesMap.set(pt.time, (curvesMap.get(pt.time) ?? 0) + pt.activePower);
    }
  }

  return Array.from(curvesMap.entries())
    .sort(([a], [b]) => a.localeCompare(b))
    .map(([time, activePower]) => ({ time, activePower: +activePower.toFixed(1) }));
}

// --- Inverter yield comparison ---

export interface InverterYieldItem {
  name: string;
  yield: number;
}

export function getInverterYieldComparison(): InverterYieldItem[] {
  return solarInverters
    .filter((i) => i.yieldToday !== null)
    .map((i) => ({ name: i.deviceName, yield: i.yieldToday! }))
    .sort((a, b) => b.yield - a.yield);
}

// --- Monthly energy production (last 12 months, mock) ---

export interface MonthlyYield {
  month: string;
  yield: number; // MWh
}

export function getMonthlyProduction(): MonthlyYield[] {
  const months = ["Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", "Jan", "Feb"];
  const yields = [28.5, 35.2, 42.1, 48.7, 52.3, 50.1, 44.8, 36.4, 25.9, 18.3, 15.1, 12.8];
  return months.map((month, i) => ({ month, yield: yields[i] }));
}

// --- Daily energy trend (last 30 days, mock) ---

export interface DailyYield {
  date: string;
  yield: number; // kWh
}

export function getDailyEnergyTrend(): DailyYield[] {
  const data: DailyYield[] = [];
  const today = new Date();
  for (let i = 29; i >= 0; i--) {
    const d = new Date(today);
    d.setDate(d.getDate() - i);
    const day = `${d.getDate()}/${d.getMonth() + 1}`;
    // Winter-ish pattern: base ~200-350 kWh/day with some variation
    const base = 250 + Math.sin((i / 30) * Math.PI) * 80;
    const noise = (Math.random() - 0.3) * 60;
    data.push({ date: day, yield: +Math.max(80, base + noise).toFixed(0) });
  }
  return data;
}

// --- PR Trend (last 30 days, mock) ---

export interface PrTrendPoint {
  date: string;
  pr: number; // %
}

export function getPrTrend(): PrTrendPoint[] {
  const data: PrTrendPoint[] = [];
  const today = new Date();
  for (let i = 29; i >= 0; i--) {
    const d = new Date(today);
    d.setDate(d.getDate() - i);
    const day = `${d.getDate()}/${d.getMonth() + 1}`;
    const pr = 82 + Math.random() * 8; // 82-90%
    data.push({ date: day, pr: +pr.toFixed(1) });
  }
  return data;
}

// --- Specific yield per inverter ---

export interface SpecificYieldItem {
  name: string;
  specificYield: number; // kWh/kWp
}

export function getSpecificYieldPerInverter(): SpecificYieldItem[] {
  return solarInverters
    .filter((i) => i.yieldToday !== null)
    .map((i) => ({
      name: i.deviceName,
      specificYield: +((i.yieldToday ?? 0) / 125).toFixed(2),
    }))
    .sort((a, b) => b.specificYield - a.specificYield);
}

// --- MPPT efficiency comparison ---

export interface MpptComparisonItem {
  tracker: string;
  avgPower: number; // kW
}

export function getMpptComparison(): MpptComparisonItem[] {
  const trackerSums = [0, 0, 0, 0, 0, 0];
  const trackerCounts = [0, 0, 0, 0, 0, 0];

  for (const inv of solarInverters) {
    if (inv.status === "offline") continue;
    for (const mppt of inv.mpptData) {
      const idx = mppt.id - 1;
      if (mppt.voltage !== null && mppt.current !== null) {
        trackerSums[idx] += (mppt.voltage * mppt.current) / 1000; // kW
        trackerCounts[idx]++;
      }
    }
  }

  return trackerSums.map((sum, i) => ({
    tracker: `MPPT ${i + 1}`,
    avgPower: trackerCounts[i] ? +(sum / trackerCounts[i]).toFixed(2) : 0,
  }));
}

// --- String current heatmap data ---

export interface StringHeatmapRow {
  inverter: string;
  currents: (number | null)[];
}

export function getStringCurrentHeatmap(): StringHeatmapRow[] {
  return solarInverters
    .filter((i) => i.status !== "offline")
    .map((i) => ({
      inverter: i.deviceName,
      currents: i.stringCurrents,
    }))
    .sort((a, b) => a.inverter.localeCompare(b.inverter));
}

// --- Monthly yield for this month (mock) ---

export function getYieldThisMonth(): number {
  return 12.8; // MWh
}
