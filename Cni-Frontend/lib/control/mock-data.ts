import type {
  ControlKPI,
  SelfConsumptionConfig,
  SelfConsumptionHourlyPoint,
  TouConfig,
  TouTariffRow,
  TouSchedulePoint,
  TariffPeriodType,
  PeakShavingConfig,
  PeakShavingBasePoint,
  PeakShavingChartPoint,
} from "./types"

// --- KPI ---
export const controlKPI: ControlKPI = {
  activeMode: "peak-shaving",
  modeStatus: "active",
  bessSoc: 72,
  bessPowerKW: 850,
  todaySavings: 342,
  monthSavings: 8750,
}

// --- Self Consumption Defaults ---
export const defaultSelfConsumptionConfig: SelfConsumptionConfig = {
  minSoC: 10,
  maxSoC: 95,
  reserveSoC: 20,
  gridExportEnabled: true,
  gridExportLimitKW: 500,
  gridImportEnabled: true,
  gridImportLimitKW: 1000,
  prioritizeSolar: true,
}

export function getSelfConsumptionHourlyData(): SelfConsumptionHourlyPoint[] {
  const data: SelfConsumptionHourlyPoint[] = []
  for (let h = 0; h < 24; h++) {
    const hour = `${h.toString().padStart(2, "0")}:00`
    // Solar bell curve peaking at 12-13
    const solarAngle = Math.max(0, Math.sin(((h - 6) / 12) * Math.PI))
    const solar = Math.round(solarAngle * 4200)
    // Load: base ~1500 with morning and evening peaks
    const loadBase = 1500
    const morningPeak = h >= 8 && h <= 11 ? 1200 * Math.sin(((h - 8) / 3) * Math.PI) : 0
    const eveningPeak = h >= 17 && h <= 21 ? 1800 * Math.sin(((h - 17) / 4) * Math.PI) : 0
    const load = Math.round(loadBase + morningPeak + eveningPeak)
    // Battery: charges when solar > load, discharges when load > solar
    const surplus = solar - load
    const battery = surplus > 0 ? Math.round(Math.min(surplus * 0.8, 2000)) * -1 : Math.round(Math.min(Math.abs(surplus) * 0.6, 2000))
    // Grid: remainder
    const grid = load - solar - battery
    data.push({ hour, solar, battery, grid: Math.max(0, grid), load })
  }
  return data
}

// --- TOU Defaults ---
export const defaultTariffSchedule: TouTariffRow[] = [
  { id: "1", periodName: "Night Off-Peak", type: "off-peak", startTime: "22:00", endTime: "06:00", buyPrice: 0.045, sellPrice: 0.035, targetSoC: 95, rateKW: 500, capacityKWh: 2000 },
  { id: "2", periodName: "Morning Mid-Peak", type: "mid-peak", startTime: "06:00", endTime: "11:00", buyPrice: 0.112, sellPrice: 0.095, targetSoC: 50, rateKW: 0, capacityKWh: 2000 },
  { id: "3", periodName: "Midday On-Peak", type: "on-peak", startTime: "11:00", endTime: "14:00", buyPrice: 0.185, sellPrice: 0.165, targetSoC: 20, rateKW: 800, capacityKWh: 2000 },
  { id: "4", periodName: "Afternoon Super-Peak", type: "super-peak", startTime: "14:00", endTime: "15:00", buyPrice: 0.245, sellPrice: 0.220, targetSoC: 10, rateKW: 1000, capacityKWh: 2000 },
  { id: "5", periodName: "Afternoon On-Peak", type: "on-peak", startTime: "15:00", endTime: "17:00", buyPrice: 0.185, sellPrice: 0.165, targetSoC: 20, rateKW: 800, capacityKWh: 2000 },
  { id: "6", periodName: "Evening Mid-Peak", type: "mid-peak", startTime: "17:00", endTime: "22:00", buyPrice: 0.112, sellPrice: 0.095, targetSoC: 50, rateKW: 0, capacityKWh: 2000 },
]

export const defaultTouConfig: TouConfig = {
  tariffSchedule: defaultTariffSchedule,
  chargeOnOffPeak: true,
  dischargeOnPeak: true,
  minSoCForDischarge: 20,
  maxSoCForCharge: 95,
  arbitrageMinSpread: 0.08,
}

export const periodColors: Record<TariffPeriodType, string> = {
  "off-peak": "var(--color-energy-solar)",
  "mid-peak": "var(--color-energy-battery)",
  "on-peak": "var(--color-energy-consumption)",
  "super-peak": "var(--color-energy-grid)",
}

export const periodLabels: Record<TariffPeriodType, string> = {
  "off-peak": "Off-Peak",
  "mid-peak": "Mid-Peak",
  "on-peak": "On-Peak",
  "super-peak": "Super-Peak",
}

function parseHour(time: string): number {
  return parseInt(time.split(":")[0], 10)
}

export function getTouScheduleData(tariff: TouTariffRow[]): TouSchedulePoint[] {
  const data: TouSchedulePoint[] = []
  for (let h = 0; h < 24; h++) {
    const hour = `${h.toString().padStart(2, "0")}:00`
    // Find which tariff period covers this hour
    let matchedRow: TouTariffRow | undefined
    for (const row of tariff) {
      const start = parseHour(row.startTime)
      const end = parseHour(row.endTime)
      if (start < end) {
        if (h >= start && h < end) { matchedRow = row; break }
      } else {
        // Wraps midnight (e.g., 22:00 - 06:00)
        if (h >= start || h < end) { matchedRow = row; break }
      }
    }
    const period: TariffPeriodType = matchedRow?.type ?? "off-peak"
    const rate = matchedRow?.buyPrice ?? 0.05
    // BESS action based on period
    let bessAction: "charge" | "discharge" | "idle" = "idle"
    let bessKW = 0
    if (period === "off-peak") {
      bessAction = "charge"
      bessKW = -1500
    } else if (period === "on-peak" || period === "super-peak") {
      bessAction = "discharge"
      bessKW = period === "super-peak" ? 2000 : 1200
    }
    data.push({ hour, rate, period, bessAction, bessKW })
  }
  return data
}

// --- Peak Shaving ---
export const defaultPeakShavingConfig: PeakShavingConfig = {
  thresholdKW: 5000,
  contractedDemandKW: 6000,
  maxBessResponseKW: 2500,
  minSoCReserve: 15,
  enablePredictive: true,
}

// Seeded pseudo-random for stable load profile
function seededRandom(seed: number): () => number {
  let s = seed
  return () => {
    s = (s * 16807 + 0) % 2147483647
    return s / 2147483647
  }
}

export function generateBaseFacilityLoad(): PeakShavingBasePoint[] {
  const rng = seededRandom(42)
  const data: PeakShavingBasePoint[] = []
  for (let h = 0; h < 24; h++) {
    const hour = `${h.toString().padStart(2, "0")}:00`
    // Base profile: low at night, ramp up during day, peak 10-15
    let base = 2000
    if (h >= 6 && h < 10) base = 2000 + (h - 6) * 800
    else if (h >= 10 && h < 15) base = 5200 + Math.sin(((h - 10) / 5) * Math.PI) * 1800
    else if (h >= 15 && h < 19) base = 5200 - (h - 15) * 600
    else if (h >= 19 && h < 22) base = 3200 - (h - 19) * 400
    else if (h >= 22) base = 2000
    // Add noise
    base += (rng() - 0.5) * 600
    data.push({ hour, facilityLoad: Math.round(base) })
  }
  return data
}

export function computePeakShavingChart(
  base: PeakShavingBasePoint[],
  threshold: number
): PeakShavingChartPoint[] {
  return base.map((p) => {
    const excess = Math.max(0, p.facilityLoad - threshold)
    return {
      ...p,
      threshold,
      shaved: excess,
      netLoad: p.facilityLoad - excess,
    }
  })
}
