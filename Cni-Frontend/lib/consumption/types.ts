export type ConsumerType =
  | "commercial-building"
  | "industrial-facility"
  | "ev-charging-station"
  | "hvac-system"
  | "lighting-system"
  | "data-center"
  | "water-pumping"
  | "cold-storage";

export type ConsumerStatus = "normal" | "offline" | "warning" | "fault" | "curtailed";

export type DRStatus = "enrolled" | "active" | "standby" | "not-enrolled";

export interface Consumer {
  id: string;
  name: string;
  type: ConsumerType;
  meterModel: string;
  serialNumber: string;
  status: ConsumerStatus;
  location: string;

  // Electrical
  activePower: number | null; // kW
  reactivePower: number | null; // kVar
  powerFactor: number | null;
  peakDemandToday: number | null; // kW
  contractedCapacity: number; // kW
  loadFactor: number | null; // %

  // Consumption
  consumptionToday: number | null; // kWh
  consumptionMTD: number | null; // MWh
  estimatedCostToday: number | null; // $

  // Demand Response
  drStatus: DRStatus;
  curtailableCapacity: number | null; // kW
  currentCurtailment: number | null; // kW
  drEventsYTD: number;

  // Device info
  commissioningDate: string;
  manufacturer: string;
}

export type ConsumerSortField =
  | "name"
  | "type"
  | "status"
  | "activePower"
  | "peakDemandToday"
  | "consumptionToday"
  | "loadFactor"
  | "location";

export type SortDirection = "asc" | "desc";

// Fault types
export type FaultSeverity = "critical" | "major" | "minor" | "warning";
export type FaultStatus = "active" | "recovered";

export interface ConsumerFault {
  id: string;
  consumerId: string;
  code: string;
  name: string;
  severity: FaultSeverity;
  status: FaultStatus;
  occurredAt: Date;
  recoveredAt: Date | null;
  relatedAlarmId: string;
}

// Curve data
export interface ConsumerPowerCurvePoint {
  time: string;
  activePower: number;
  reactivePower: number;
}

export interface ConsumerDemandCurvePoint {
  time: string;
  demand: number;
  contracted: number;
}

export interface ConsumerCurveData {
  consumption: ConsumerPowerCurvePoint[];
  demand: ConsumerDemandCurvePoint[];
}
