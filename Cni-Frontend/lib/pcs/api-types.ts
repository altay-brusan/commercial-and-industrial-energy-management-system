import type {
  StatusBadgeDto,
  TabInfoDto,
  ChartDefinitionDto,
  ChartResponseDto,
  FaultFlagDto,
  FaultSourceDto,
  FaultCategoryDto,
  FaultFlagSummaryDto,
  FaultFlagsResponseDto,
  DynamicSectionDto,
  SummaryResponseDto,
} from "@/lib/bess/api-types";

// Re-export shared types used by PCS components
export type {
  StatusBadgeDto,
  TabInfoDto,
  ChartDefinitionDto,
  ChartResponseDto,
  FaultFlagDto,
  FaultSourceDto,
  FaultCategoryDto,
  FaultFlagSummaryDto,
  FaultFlagsResponseDto,
  DynamicSectionDto,
  SummaryResponseDto,
};

// --- Units List ---

export interface LinkedBessUnitDto {
  id: string;
  name: string;
}

export interface PcsUnitListItemDto {
  id: string;
  name: string;
  serialNumber: string | null;
  modelName: string | null;
  manufacturer: string | null;
  facilityName: string | null;
  facilityId: string;
  status: StatusBadgeDto;
  linkedBessUnit: LinkedBessUnitDto | null;
  gridMode: string;
  activePowerKw: number;
  reactivePowerKvar: number;
  gridFreqHz: number;
  igbtTempC: number;
  efficiencyPct: number;
  powerFactor: number;
  activeFaultCount: number;
  lastSeenUtc: string | null;
}

export interface PcsAggregatesDto {
  totalUnits: number;
  onlineCount: number;
  faultCount: number;
  totalActivePowerKw: number;
  totalReactivePowerKvar: number;
  avgEfficiencyPct: number;
  totalActiveFaults: number;
}

export interface PcsUnitsResultDto {
  units: PcsUnitListItemDto[];
  aggregates: PcsAggregatesDto;
}

// --- Unit Header ---

export interface PcsHeaderDto {
  id: string;
  deviceId: string;
  name: string;
  status: StatusBadgeDto;
  linkedBessUnit: LinkedBessUnitDto | null;
  availableTabs: TabInfoDto[];
}

// --- Schema ---

export interface PcsProductTypeDto {
  key: string;
  label: string;
  supportedTabs: string[];
  faultSources: string[];
  curveCharts: string[];
}

export interface SchemaResponseDto {
  productTypes: PcsProductTypeDto[];
}
