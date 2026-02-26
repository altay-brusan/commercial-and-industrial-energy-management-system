import { apiClient } from "@/lib/auth/api-client";
import { BESS_ENDPOINTS } from "./endpoints";
import type {
  BessDeviceTreeResponseDto,
  BessUnitsResultDto,
  BessUnitHeaderDto,
  SummaryResponseDto,
  ChartResponseDto,
  FaultFlagsResponseDto,
  FaultsPaginatedResponseDto,
} from "./api-types";

function withDeviceId(template: string, deviceId: string): string {
  return template.replace("{deviceId}", deviceId);
}

export async function getTree(facilityId?: string): Promise<BessDeviceTreeResponseDto> {
  const params = facilityId ? `?facilityId=${facilityId}` : "";
  return apiClient<BessDeviceTreeResponseDto>(`${BESS_ENDPOINTS.TREE}${params}`);
}

export async function getUnits(params?: {
  facilityId?: string;
  search?: string;
  sortBy?: string;
  sortDirection?: string;
}): Promise<BessUnitsResultDto> {
  const searchParams = new URLSearchParams();
  if (params?.facilityId) searchParams.set("FacilityId", params.facilityId);
  if (params?.search) searchParams.set("Search", params.search);
  if (params?.sortBy) searchParams.set("SortBy", params.sortBy);
  if (params?.sortDirection) searchParams.set("SortDirection", params.sortDirection);
  const qs = searchParams.toString();
  return apiClient<BessUnitsResultDto>(`${BESS_ENDPOINTS.UNITS}${qs ? `?${qs}` : ""}`);
}

export async function getDeviceHeader(deviceId: string): Promise<BessUnitHeaderDto> {
  return apiClient<BessUnitHeaderDto>(`${BESS_ENDPOINTS.DEVICE_HEADER}/${deviceId}`);
}

export async function getDeviceSummary(deviceId: string): Promise<SummaryResponseDto> {
  return apiClient<SummaryResponseDto>(withDeviceId(BESS_ENDPOINTS.DEVICE_SUMMARY, deviceId));
}

export async function getDeviceHealth(deviceId: string): Promise<SummaryResponseDto> {
  return apiClient<SummaryResponseDto>(withDeviceId(BESS_ENDPOINTS.DEVICE_HEALTH, deviceId));
}

export async function getDeviceCurves(
  deviceId: string,
  params?: { period?: string; interval?: string }
): Promise<ChartResponseDto> {
  const searchParams = new URLSearchParams();
  if (params?.period) searchParams.set("period", params.period);
  if (params?.interval) searchParams.set("interval", params.interval);
  const qs = searchParams.toString();
  return apiClient<ChartResponseDto>(
    `${withDeviceId(BESS_ENDPOINTS.DEVICE_CURVES, deviceId)}${qs ? `?${qs}` : ""}`
  );
}

export async function getDeviceFaultFlags(deviceId: string): Promise<FaultFlagsResponseDto> {
  return apiClient<FaultFlagsResponseDto>(withDeviceId(BESS_ENDPOINTS.DEVICE_FAULT_FLAGS, deviceId));
}

export async function getDeviceNotifications(
  deviceId: string,
  params?: { page?: number; pageSize?: number; severity?: string; isActive?: boolean }
): Promise<FaultsPaginatedResponseDto> {
  const searchParams = new URLSearchParams();
  if (params?.page) searchParams.set("page", String(params.page));
  if (params?.pageSize) searchParams.set("pageSize", String(params.pageSize));
  if (params?.severity) searchParams.set("severity", params.severity);
  if (params?.isActive !== undefined) searchParams.set("isActive", String(params.isActive));
  const qs = searchParams.toString();
  return apiClient<FaultsPaginatedResponseDto>(
    `${withDeviceId(BESS_ENDPOINTS.DEVICE_NOTIFICATIONS, deviceId)}${qs ? `?${qs}` : ""}`
  );
}

export async function getDeviceSettings(deviceId: string): Promise<SummaryResponseDto> {
  return apiClient<SummaryResponseDto>(withDeviceId(BESS_ENDPOINTS.DEVICE_SETTINGS, deviceId));
}
