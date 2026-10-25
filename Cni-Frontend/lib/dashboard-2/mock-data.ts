import type {
  TotalEnergyUsage,
  EnergySourceData,
  DailyEnergyCost,
  BESSStateData,
  PCSStateData,
  SolarCapacityData,
  DailyChartDataPoint,
  MonthlyChartDataPoint,
  DailyUsagePerSourceDataPoint,
  HourlyDataPoint,
  HourlySourceDataPoint,
  WeeklySourceDataPoint,
  MonthlySourceDataPoint,
  HourlyCostDataPoint,
  WeeklyCostDataPoint,
  MonthlyCostDataPoint,
} from "./types";

export const totalEnergyUsage: TotalEnergyUsage = {
  totalKWh: 15.8,
  activePowerKW: 12.4,
  reactivePowerKVAR: 3.2,
  sources: [
    { label: "Grid", value: 8.5, percentage: 53.8, color: "bg-energy-grid" },
    { label: "BESS", value: 4.2, percentage: 26.6, color: "bg-energy-battery" },
    { label: "Solar", value: 2.6, percentage: 16.5, color: "bg-energy-solar" },
    { label: "Generator", value: 0.5, percentage: 3.1, color: "bg-muted-foreground" },
  ],
};

export const energySourceData: EnergySourceData = {
  totalKW: 72.6,
  sources: [
    { label: "Grid", value: 38.9, percentage: 53.6, color: "var(--color-energy-grid)" },
    { label: "BESS", value: 19.3, percentage: 26.6, color: "var(--color-energy-battery)" },
    { label: "Solar", value: 12.0, percentage: 16.5, color: "var(--color-energy-solar)" },
    { label: "Generator", value: 2.4, percentage: 3.3, color: "var(--color-muted-foreground)" },
  ],
};

export const dailyEnergyCost: DailyEnergyCost = {
  totalCostTL: 31.77,
  savedTL: 0,
};

export const bessStateData: BESSStateData = {
  name: "BESS-1",
  socPercent: 100.0,
  status: "idle",
};

export const pcsStateData: PCSStateData = {
  mode: "On Grid",
  temperatureC: 61.3,
  loadKW: 33.4,
  efficiencyPercent: 89,
  acVoltageV: 400.2,
  acCurrentA: 48.5,
  dcVoltageV: 720.0,
  dcCurrentA: 46.4,
};

export const solarCapacityData: SolarCapacityData = {
  currentPowerKW: 0.0,
  maxCapacityKW: 500.0,
  maintenanceRequired: true,
  temperatureC: 45.2,
  efficiencyPercent: 0,
  energyProducedKWh: 0.0,
};

export const co2EmissionData: DailyChartDataPoint[] = [
  { day: "Mon", value: 42 },
  { day: "Tue", value: 35 },
  { day: "Wed", value: 48 },
  { day: "Thu", value: 38 },
  { day: "Fri", value: 55 },
  { day: "Sat", value: 30 },
  { day: "Sun", value: 25 },
];

export const co2TodayKg = 18.4;

export const co2MonthlyData: MonthlyChartDataPoint[] = [
  { month: "Jan", value: 1280 },
  { month: "Feb", value: 1050 },
  { month: "Mar", value: 1420 },
  { month: "Apr", value: 1180 },
  { month: "May", value: 1350 },
  { month: "Jun", value: 1520 },
  { month: "Jul", value: 1600 },
  { month: "Aug", value: 1550 },
  { month: "Sep", value: 1380 },
  { month: "Oct", value: 1260 },
  { month: "Nov", value: 1150 },
  { month: "Dec", value: 1300 },
];

export const dailyUsageData: DailyChartDataPoint[] = [
  { day: "Mon", value: 120 },
  { day: "Tue", value: 98 },
  { day: "Wed", value: 135 },
  { day: "Thu", value: 110 },
  { day: "Fri", value: 145 },
  { day: "Sat", value: 85 },
  { day: "Sun", value: 70 },
];

export const monthlyCostSavingsData: MonthlyChartDataPoint[] = [
  { month: "Jan", value: 1200 },
  { month: "Feb", value: 980 },
  { month: "Mar", value: 1450 },
  { month: "Apr", value: 1680 },
  { month: "May", value: 2100 },
  { month: "Jun", value: 2350 },
  { month: "Jul", value: 2500 },
  { month: "Aug", value: 2400 },
  { month: "Sep", value: 1900 },
  { month: "Oct", value: 1550 },
  { month: "Nov", value: 1100 },
  { month: "Dec", value: 850 },
];

export const dailyUsagePerSourceData: DailyUsagePerSourceDataPoint[] = [
  { day: "Mon", bess: 30, pv: 45, grid: 45 },
  { day: "Tue", bess: 25, pv: 38, grid: 35 },
  { day: "Wed", bess: 35, pv: 50, grid: 50 },
  { day: "Thu", bess: 28, pv: 42, grid: 40 },
  { day: "Fri", bess: 40, pv: 55, grid: 50 },
  { day: "Sat", bess: 20, pv: 35, grid: 30 },
  { day: "Sun", bess: 18, pv: 28, grid: 24 },
];

// --- Analytics chart data ---

// Energy Consumption: Today (hourly)
export const hourlyConsumptionData: HourlyDataPoint[] = [
  { hour: "00:00", consumption: 2.1, production: 0 },
  { hour: "01:00", consumption: 1.8, production: 0 },
  { hour: "02:00", consumption: 1.5, production: 0 },
  { hour: "03:00", consumption: 1.4, production: 0 },
  { hour: "04:00", consumption: 1.6, production: 0 },
  { hour: "05:00", consumption: 2.0, production: 0 },
  { hour: "06:00", consumption: 3.2, production: 0.3 },
  { hour: "07:00", consumption: 5.8, production: 1.2 },
  { hour: "08:00", consumption: 8.4, production: 3.5 },
  { hour: "09:00", consumption: 10.2, production: 5.8 },
  { hour: "10:00", consumption: 11.5, production: 7.2 },
  { hour: "11:00", consumption: 12.0, production: 8.1 },
  { hour: "12:00", consumption: 11.8, production: 8.5 },
  { hour: "13:00", consumption: 11.2, production: 8.0 },
  { hour: "14:00", consumption: 10.5, production: 6.8 },
  { hour: "15:00", consumption: 9.8, production: 5.2 },
  { hour: "16:00", consumption: 9.0, production: 3.4 },
  { hour: "17:00", consumption: 8.5, production: 1.5 },
  { hour: "18:00", consumption: 9.2, production: 0.2 },
  { hour: "19:00", consumption: 10.0, production: 0 },
  { hour: "20:00", consumption: 8.8, production: 0 },
  { hour: "21:00", consumption: 6.5, production: 0 },
  { hour: "22:00", consumption: 4.2, production: 0 },
  { hour: "23:00", consumption: 2.8, production: 0 },
];

// Energy Consumption: Weekly (daily totals)
export const weeklyConsumptionData: DailyChartDataPoint[] = [
  { day: "Mon", value: 120 },
  { day: "Tue", value: 98 },
  { day: "Wed", value: 135 },
  { day: "Thu", value: 110 },
  { day: "Fri", value: 145 },
  { day: "Sat", value: 85 },
  { day: "Sun", value: 70 },
];

// Energy Consumption: Monthly
export const monthlyConsumptionData: MonthlyChartDataPoint[] = [
  { month: "Jan", value: 3200 },
  { month: "Feb", value: 2900 },
  { month: "Mar", value: 3450 },
  { month: "Apr", value: 3100 },
  { month: "May", value: 3650 },
  { month: "Jun", value: 4200 },
  { month: "Jul", value: 4500 },
  { month: "Aug", value: 4350 },
  { month: "Sep", value: 3800 },
  { month: "Oct", value: 3300 },
  { month: "Nov", value: 2950 },
  { month: "Dec", value: 3100 },
];

// Source Distribution: Today (hourly)
export const hourlySourceData: HourlySourceDataPoint[] = [
  { hour: "00:00", bess: 0.8, pv: 0, grid: 1.3 },
  { hour: "03:00", bess: 0.5, pv: 0, grid: 0.9 },
  { hour: "06:00", bess: 0.2, pv: 0.3, grid: 2.7 },
  { hour: "09:00", bess: 0.5, pv: 5.8, grid: 3.9 },
  { hour: "12:00", bess: 1.2, pv: 8.5, grid: 2.1 },
  { hour: "15:00", bess: 1.8, pv: 5.2, grid: 2.8 },
  { hour: "18:00", bess: 2.5, pv: 0.2, grid: 6.5 },
  { hour: "21:00", bess: 1.5, pv: 0, grid: 5.0 },
];

// Source Distribution: Weekly
export const weeklySourceData: WeeklySourceDataPoint[] = [
  { day: "Mon", bess: 30, pv: 45, grid: 45 },
  { day: "Tue", bess: 25, pv: 38, grid: 35 },
  { day: "Wed", bess: 35, pv: 50, grid: 50 },
  { day: "Thu", bess: 28, pv: 42, grid: 40 },
  { day: "Fri", bess: 40, pv: 55, grid: 50 },
  { day: "Sat", bess: 20, pv: 35, grid: 30 },
  { day: "Sun", bess: 18, pv: 28, grid: 24 },
];

// Source Distribution: Monthly
export const monthlySourceData: MonthlySourceDataPoint[] = [
  { month: "Jan", bess: 650, pv: 480, grid: 2070 },
  { month: "Feb", bess: 600, pv: 520, grid: 1780 },
  { month: "Mar", bess: 720, pv: 880, grid: 1850 },
  { month: "Apr", bess: 680, pv: 1100, grid: 1320 },
  { month: "May", bess: 750, pv: 1450, grid: 1450 },
  { month: "Jun", bess: 800, pv: 1800, grid: 1600 },
  { month: "Jul", bess: 850, pv: 1950, grid: 1700 },
  { month: "Aug", bess: 820, pv: 1850, grid: 1680 },
  { month: "Sep", bess: 780, pv: 1400, grid: 1620 },
  { month: "Oct", bess: 700, pv: 950, grid: 1650 },
  { month: "Nov", bess: 650, pv: 550, grid: 1750 },
  { month: "Dec", bess: 620, pv: 420, grid: 2060 },
];

// Cost & Savings: Today (hourly, sampled)
export const hourlyCostData: HourlyCostDataPoint[] = [
  { hour: "00:00", cost: 1.2, savings: 0.3 },
  { hour: "03:00", cost: 0.9, savings: 0.2 },
  { hour: "06:00", cost: 2.1, savings: 0.4 },
  { hour: "09:00", cost: 4.5, savings: 3.2 },
  { hour: "12:00", cost: 3.8, savings: 4.8 },
  { hour: "15:00", cost: 4.2, savings: 3.5 },
  { hour: "18:00", cost: 5.8, savings: 0.8 },
  { hour: "21:00", cost: 3.5, savings: 0.5 },
];

// Cost & Savings: Weekly
export const weeklyCostData: WeeklyCostDataPoint[] = [
  { day: "Mon", cost: 42, savings: 18 },
  { day: "Tue", cost: 35, savings: 22 },
  { day: "Wed", cost: 48, savings: 25 },
  { day: "Thu", cost: 38, savings: 20 },
  { day: "Fri", cost: 52, savings: 28 },
  { day: "Sat", cost: 28, savings: 15 },
  { day: "Sun", cost: 22, savings: 12 },
];

// Cost & Savings: Monthly
export const monthlyCostData: MonthlyCostDataPoint[] = [
  { month: "Jan", cost: 2800, savings: 1200 },
  { month: "Feb", cost: 2500, savings: 980 },
  { month: "Mar", cost: 2650, savings: 1450 },
  { month: "Apr", cost: 2200, savings: 1680 },
  { month: "May", cost: 2100, savings: 2100 },
  { month: "Jun", cost: 2300, savings: 2350 },
  { month: "Jul", cost: 2450, savings: 2500 },
  { month: "Aug", cost: 2400, savings: 2400 },
  { month: "Sep", cost: 2550, savings: 1900 },
  { month: "Oct", cost: 2700, savings: 1550 },
  { month: "Nov", cost: 2750, savings: 1100 },
  { month: "Dec", cost: 2900, savings: 850 },
];
