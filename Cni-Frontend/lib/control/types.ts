export type EmsAlgorithm = "self-consumption" | "tou" | "peak-shaving"
export type AlgorithmStatus = "active" | "standby" | "error"
export type TariffPeriodType = "off-peak" | "mid-peak" | "on-peak" | "super-peak"

// KPI strip
export interface ControlKPI {
  activeMode: EmsAlgorithm
  modeStatus: AlgorithmStatus
  bessSoc: number           // 0-100%
  bessPowerKW: number       // positive=discharge, negative=charge
  todaySavings: number
  monthSavings: number
}

// Self Consumption
export interface SelfConsumptionConfig {
  minSoC: number
  maxSoC: number
  reserveSoC: number
  gridExportEnabled: boolean
  gridExportLimitKW: number
  gridImportEnabled: boolean
  gridImportLimitKW: number
  prioritizeSolar: boolean
}

export interface SelfConsumptionHourlyPoint {
  hour: string
  solar: number
  battery: number
  grid: number
  load: number
}

// TOU
export interface TouTariffRow {
  id: string
  periodName: string
  type: TariffPeriodType
  startTime: string
  endTime: string
  buyPrice: number
  sellPrice: number
  targetSoC: number | null
  rateKW: number | null
  capacityKWh: number | null
}

export interface TouConfig {
  tariffSchedule: TouTariffRow[]
  chargeOnOffPeak: boolean
  dischargeOnPeak: boolean
  minSoCForDischarge: number
  maxSoCForCharge: number
  arbitrageMinSpread: number
}

export interface TouSchedulePoint {
  hour: string
  rate: number
  period: TariffPeriodType
  bessAction: "charge" | "discharge" | "idle"
  bessKW: number
}

// Peak Shaving
export interface PeakShavingConfig {
  thresholdKW: number
  contractedDemandKW: number
  maxBessResponseKW: number
  minSoCReserve: number
  enablePredictive: boolean
}

export interface PeakShavingBasePoint {
  hour: string
  facilityLoad: number
}

export interface PeakShavingChartPoint extends PeakShavingBasePoint {
  netLoad: number
  threshold: number
  shaved: number
}
