// Shared DTO types used across multiple endpoints

export interface StatusBadgeDto {
  value: string;
  label: string;
  color: string;
}

export interface TabInfoDto {
  key: string;
  label: string;
  isEnabled: boolean;
  order: number;
}

export interface ConnectedDeviceDto {
  id: string;
  name: string;
}

export interface PaginationDto {
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}

// --- Tree ---

export interface BessTreeNodeDto {
  id: string;
  name: string;
  type: string;
  status: string;
  parentDeviceId: string | null;
  alarmCount: number;
  children: BessTreeNodeDto[];
}

export interface BessDeviceTreeResponseDto {
  tree: BessTreeNodeDto[];
}

// --- Units List ---

export interface BessUnitListItemDto {
  id: string;
  name: string;
  status: StatusBadgeDto;
  operatingMode: StatusBadgeDto;
  soc: number | null;
  soh: number | null;
  power: number | null;
  capacityKwh: number | null;
  activeAlarmCount: number;
  brand: string | null;
  model: string | null;
  serialNumber: string | null;
  facilityName: string | null;
  facilityId: string;
  temperatureC: number | null;
  lastSeenUtc: string | null;
}

export interface BessAggregatesDto {
  totalUnits: number;
  totalCapacityKwh: number | null;
  averageSoc: number | null;
  totalActiveAlarms: number;
  onlineCount: number;
  offlineCount: number;
  faultCount: number;
  standbyCount: number;
}

export interface BessUnitsResultDto {
  units: BessUnitListItemDto[];
  aggregates: BessAggregatesDto;
}

// --- Unit Header ---

export interface BessUnitHeaderDto {
  id: string;
  name: string;
  serialNumber: string | null;
  modelName: string | null;
  manufacturer: string | null;
  facilityId: string;
  facilityName: string | null;
  unitId: string | null;
  unitName: string | null;
  rackId: string | null;
  rackName: string | null;
  packId: string | null;
  packName: string | null;
  hierarchyNumber: number | null;
  displayType: string;
  status: StatusBadgeDto;
  operatingMode: StatusBadgeDto | null;
  soc: number | null;
  soh: number | null;
  voltage: number | null;
  current: number | null;
  power: number | null;
  capacityKwh: number | null;
  activeAlarmCount: number;
  availableTabs: TabInfoDto[];
  connectedDevices: Record<string, ConnectedDeviceDto[]>;
  lastSeenUtc: string | null;
  sections: unknown[] | null;
}

// --- Dynamic Sections (Summary, Health, Settings) ---

export interface SectionFieldDto {
  key: string;
  label: string;
  value: unknown;
  type: string;
  unit: string | null;
  format: string | null;
  status: string | null;
  editable: boolean | null;
  options: string[] | null;
  meta: Record<string, unknown> | null;
}

export interface TableColumnDto {
  key: string;
  label: string;
  type: string;
}

export interface TableDataDto {
  columns: TableColumnDto[];
  rows: Record<string, unknown>[];
}

export interface ChartAxisDto {
  key: string;
  label: string;
  unit: string | null;
  domain: number[] | null;
}

export interface ChartReferenceLineDto {
  y: number;
  label: string;
  color: string | null;
  strokeDasharray: string | null;
}

export interface ChartConfigDto {
  type: string;
  xAxis: ChartAxisDto | null;
  yAxis: ChartAxisDto | null;
  referenceLines: ChartReferenceLineDto[] | null;
  color: string | null;
}

export interface DynamicSectionDto {
  key: string;
  title: string;
  icon: string | null;
  variant: string;
  columns: number;
  optional: boolean;
  fields: SectionFieldDto[];
  tableData: TableDataDto | null;
  chartConfig: ChartConfigDto | null;
  chartData: Record<string, unknown>[] | null;
}

export interface SummaryResponseDto {
  sections: DynamicSectionDto[];
}

// --- Curves ---

export interface CurveXAxisDto {
  key: string;
  label: string;
  format: string | null;
}

export interface CurveSeriesDto {
  key: string;
  label: string;
  unit: string | null;
  color: string | null;
  yAxis: string | null;
  type: string | null;
}

export interface ChartDefinitionDto {
  key: string;
  title: string;
  type: string;
  xAxis: CurveXAxisDto;
  series: CurveSeriesDto[];
  data: Record<string, unknown>[];
}

export interface ChartResponseDto {
  charts: ChartDefinitionDto[];
}

// --- Fault Flags ---

export interface FaultFlagDto {
  key: string;
  label: string;
  isActive: boolean;
  source: string | null;
  category: string | null;
  severity: string;
}

export interface FaultSourceDto {
  key: string;
  label: string;
  activeCount: number;
  totalCount: number;
}

export interface FaultCategoryDto {
  key: string;
  label: string;
  activeCount: number;
  totalCount: number;
}

export interface FaultFlagSummaryDto {
  totalFlags: number;
  activeFlags: number;
  activeBySource: Record<string, number> | null;
  activeByCategory: Record<string, number>;
}

export interface FaultFlagsResponseDto {
  flags: FaultFlagDto[];
  sources: FaultSourceDto[] | null;
  categories: FaultCategoryDto[];
  summary: FaultFlagSummaryDto;
}

// --- Notifications / Faults ---

export interface FaultRecordDto {
  id: string;
  code: string;
  name: string;
  severity: StatusBadgeDto;
  status: StatusBadgeDto;
  source: string | null;
  occurredAt: string;
  recoveredAt: string | null;
  relatedAlarmId: string | null;
}

export interface FaultsPaginatedResponseDto {
  faults: FaultRecordDto[];
  pagination: PaginationDto;
}
