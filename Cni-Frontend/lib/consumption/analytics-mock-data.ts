import { consumers } from "./mock-data";
import type { ConsumerType, ConsumerStatus, DRStatus } from "./types";

// --- Aggregation helpers ---

export function getTotalDemand(): number {
  return consumers.reduce((sum, c) => sum + (c.activePower ?? 0), 0);
}

export function getTotalConsumptionToday(): number {
  return consumers.reduce((sum, c) => sum + (c.consumptionToday ?? 0), 0);
}

export function getPeakDemandToday(): number {
  return Math.max(...consumers.map((c) => c.peakDemandToday ?? 0));
}

export function getAvgLoadFactor(): number {
  const active = consumers.filter((c) => c.loadFactor !== null && c.loadFactor > 0);
  if (active.length === 0) return 0;
  return +(active.reduce((sum, c) => sum + (c.loadFactor ?? 0), 0) / active.length).toFixed(1);
}

export function getEstimatedCostToday(): number {
  return consumers.reduce((sum, c) => sum + (c.estimatedCostToday ?? 0), 0);
}

export function getConsumerStatusSummary(): Record<ConsumerStatus, number> {
  const summary: Record<ConsumerStatus, number> = { normal: 0, offline: 0, warning: 0, fault: 0, curtailed: 0 };
  for (const c of consumers) {
    summary[c.status]++;
  }
  return summary;
}

// --- DR helpers ---

export function getEnrolledCount(): number {
  return consumers.filter((c) => c.drStatus !== "not-enrolled").length;
}

export function getAvailableFlexibility(): number {
  return consumers.reduce((sum, c) => sum + (c.curtailableCapacity ?? 0), 0);
}

export function getCurrentCurtailment(): number {
  return consumers.reduce((sum, c) => sum + (c.currentCurtailment ?? 0), 0);
}

export function getDREventsYTD(): number {
  return Math.max(...consumers.map((c) => c.drEventsYTD));
}

// --- Demand Profile (24h aggregated) ---

export interface DemandProfilePoint {
  time: string;
  demand: number;
  contracted: number;
}

export function getDemandProfile(): DemandProfilePoint[] {
  const totalContracted = consumers.reduce((sum, c) => sum + c.contractedCapacity, 0);
  const totalBase = consumers.reduce((sum, c) => sum + (c.activePower ?? 0), 0);
  const data: DemandProfilePoint[] = [];

  for (let h = 0; h < 24; h++) {
    const time = `${String(h).padStart(2, "0")}:00`;
    const morningRamp = Math.exp(-0.5 * Math.pow((h - 10) / 2.5, 2));
    const afternoonPeak = Math.exp(-0.5 * Math.pow((h - 14) / 2, 2));
    const eveningDip = h >= 18 ? 0.4 : 1.0;
    const factor = (0.3 + 0.45 * morningRamp + 0.5 * afternoonPeak) * eveningDip;
    const demand = +(totalBase * factor * (1 + (Math.random() - 0.5) * 0.05)).toFixed(0);

    data.push({ time, demand, contracted: totalContracted });
  }
  return data;
}

// --- Consumption by Type ---

export interface ConsumptionByTypeItem {
  type: string;
  consumption: number; // kWh
}

const typeLabels: Record<ConsumerType, string> = {
  "commercial-building": "Commercial",
  "industrial-facility": "Industrial",
  "ev-charging-station": "EV Charging",
  "hvac-system": "HVAC",
  "lighting-system": "Lighting",
  "data-center": "Data Center",
  "water-pumping": "Water Pump",
  "cold-storage": "Cold Storage",
};

export function getConsumptionByType(): ConsumptionByTypeItem[] {
  const map = new Map<string, number>();
  for (const c of consumers) {
    const label = typeLabels[c.type];
    map.set(label, (map.get(label) ?? 0) + (c.consumptionToday ?? 0));
  }
  return Array.from(map.entries())
    .map(([type, consumption]) => ({ type, consumption }))
    .sort((a, b) => b.consumption - a.consumption);
}

// --- Monthly Consumption ---

export interface MonthlyConsumptionItem {
  month: string;
  consumption: number; // MWh
}

export function getMonthlyConsumption(): MonthlyConsumptionItem[] {
  const months = ["Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", "Jan", "Feb"];
  return months.map((month, i) => {
    const winterFactor = Math.cos((i / 12) * Math.PI * 2);
    const base = 1200 + winterFactor * 300;
    return {
      month,
      consumption: +(base + (Math.random() - 0.5) * 100).toFixed(0),
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
  const totalContracted = consumers.reduce((sum, c) => sum + c.contractedCapacity, 0);
  for (let i = 29; i >= 0; i--) {
    const d = new Date(today);
    d.setDate(d.getDate() - i);
    const day = `${d.getDate()}/${d.getMonth() + 1}`;
    const base = 4800 + Math.sin((i / 30) * Math.PI * 2) * 600;
    const noise = (Math.random() - 0.5) * 400;
    data.push({ date: day, peakDemand: +Math.max(3500, base + noise).toFixed(0) });
  }
  return data;
}

// --- DR Enrollment Data ---

export interface DREnrollmentItem {
  id: string;
  name: string;
  type: string;
  drStatus: DRStatus;
  contractedCapacity: number;
  curtailableCapacity: number | null;
  currentCurtailment: number | null;
  drEventsYTD: number;
}

export function getDREnrollmentData(): DREnrollmentItem[] {
  return consumers.map((c) => ({
    id: c.id,
    name: c.name,
    type: typeLabels[c.type],
    drStatus: c.drStatus,
    contractedCapacity: c.contractedCapacity,
    curtailableCapacity: c.curtailableCapacity,
    currentCurtailment: c.currentCurtailment,
    drEventsYTD: c.drEventsYTD,
  }));
}

// --- Flexibility by Type (donut) ---

export interface FlexibilityByTypeItem {
  type: string;
  flexibility: number; // kW
}

export function getFlexibilityByType(): FlexibilityByTypeItem[] {
  const map = new Map<string, number>();
  for (const c of consumers) {
    if (c.curtailableCapacity) {
      const label = typeLabels[c.type];
      map.set(label, (map.get(label) ?? 0) + c.curtailableCapacity);
    }
  }
  return Array.from(map.entries())
    .map(([type, flexibility]) => ({ type, flexibility }))
    .sort((a, b) => b.flexibility - a.flexibility);
}

// --- DR Event History (12 months) ---

export interface DREventHistoryItem {
  month: string;
  events: number;
}

export function getDREventHistory(): DREventHistoryItem[] {
  const months = ["Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", "Jan", "Feb"];
  return months.map((month) => ({
    month,
    events: Math.floor(Math.random() * 6) + 1,
  }));
}

// --- Consumer Load Heatmap ---

export interface ConsumerLoadHeatmapRow {
  consumer: string;
  hours: number[]; // 24 values, each 0-100% of contracted capacity
}

export function getConsumerLoadHeatmap(): ConsumerLoadHeatmapRow[] {
  return consumers.map((c) => {
    const hours: number[] = [];
    const baseFactor = c.loadFactor ?? 50;
    for (let h = 0; h < 24; h++) {
      const morningRamp = Math.exp(-0.5 * Math.pow((h - 10) / 2.5, 2));
      const afternoonPeak = Math.exp(-0.5 * Math.pow((h - 14) / 2, 2));
      const eveningDip = h >= 18 ? 0.5 : 1.0;
      const factor = (0.3 + 0.45 * morningRamp + 0.5 * afternoonPeak) * eveningDip;
      const loading = baseFactor * factor * (1 + (Math.random() - 0.5) * 0.2);
      hours.push(+Math.min(100, Math.max(5, loading)).toFixed(0));
    }
    return { consumer: c.name, hours };
  });
}

export { typeLabels as consumerTypeLabels };
