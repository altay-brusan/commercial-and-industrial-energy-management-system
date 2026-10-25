export interface EnergySourceBreakdown {
  label: string;
  value: number;
  percentage: number;
  color: string;
}

export interface TotalEnergyUsage {
  totalKWh: number;
  activePowerKW: number;
  reactivePowerKVAR: number;
  sources: EnergySourceBreakdown[];
}

export interface EnergySourceData {
  totalKW: number;
  sources: EnergySourceBreakdown[];
}

export interface DailyEnergyCost {
  totalCostTL: number;
  savedTL: number;
}

export interface BESSStateData {
  name: string;
  socPercent: number;
  status: "charging" | "discharging" | "idle";
}

export interface PCSStateData {
  mode: "On Grid" | "Off Grid";
  temperatureC: number;
  loadKW: number;
  efficiencyPercent: number;
  acVoltageV: number;
  acCurrentA: number;
  dcVoltageV: number;
  dcCurrentA: number;
}

export interface SolarCapacityData {
  currentPowerKW: number;
  maxCapacityKW: number;
  maintenanceRequired: boolean;
  temperatureC: number;
  efficiencyPercent: number;
  energyProducedKWh: number;
}

export interface DailyChartDataPoint {
  day: string;
  value: number;
}

export interface MonthlyChartDataPoint {
  month: string;
  value: number;
}

export interface DailyUsagePerSourceDataPoint {
  day: string;
  bess: number;
  pv: number;
  grid: number;
}

// Analytics chart types
export interface HourlyDataPoint {
  hour: string;
  consumption: number;
  production: number;
}

export interface HourlySourceDataPoint {
  hour: string;
  bess: number;
  pv: number;
  grid: number;
}

export interface WeeklySourceDataPoint {
  day: string;
  bess: number;
  pv: number;
  grid: number;
}

export interface MonthlySourceDataPoint {
  month: string;
  bess: number;
  pv: number;
  grid: number;
}

export interface HourlyCostDataPoint {
  hour: string;
  cost: number;
  savings: number;
}

export interface WeeklyCostDataPoint {
  day: string;
  cost: number;
  savings: number;
}

export interface MonthlyCostDataPoint {
  month: string;
  cost: number;
  savings: number;
}
