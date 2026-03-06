import { apiClient } from "@/lib/auth/api-client";
import { PCS_ENDPOINTS } from "./endpoints";
import type {
  PcsUnitsResultDto,
  PcsHeaderDto,
  SummaryResponseDto,
  FaultFlagsResponseDto,
  ChartResponseDto,
  SchemaResponseDto,
} from "./api-types";

function withFacilityId(template: string, facilityId: string): string {
  return template.replace("{facilityId}", facilityId);
}

function withUnitId(template: string, unitId: string): string {
  return template.replace("{unitId}", unitId);
}

export async function getUnits(facilityId: string): Promise<PcsUnitsResultDto> {
  return apiClient<PcsUnitsResultDto>(withFacilityId(PCS_ENDPOINTS.UNITS, facilityId));
}

export async function getUnitHeader(unitId: string): Promise<PcsHeaderDto> {
  return apiClient<PcsHeaderDto>(withUnitId(PCS_ENDPOINTS.UNIT_HEADER, unitId));
}

export async function getUnitGeneral(unitId: string): Promise<SummaryResponseDto> {
  return apiClient<SummaryResponseDto>(
    withUnitId(PCS_ENDPOINTS.UNIT_TAB, unitId).replace("{tab}", "general")
  );
}

export async function getUnitFaultFlags(unitId: string): Promise<FaultFlagsResponseDto> {
  return apiClient<FaultFlagsResponseDto>(
    withUnitId(PCS_ENDPOINTS.UNIT_TAB, unitId).replace("{tab}", "fault")
  );
}

export async function getUnitCurves(
  unitId: string,
  params?: { period?: string; interval?: string }
): Promise<ChartResponseDto> {
  const searchParams = new URLSearchParams();
  if (params?.period) searchParams.set("period", params.period);
  if (params?.interval) searchParams.set("interval", params.interval);
  const qs = searchParams.toString();
  const base = withUnitId(PCS_ENDPOINTS.UNIT_TAB, unitId).replace("{tab}", "curve");
  return apiClient<ChartResponseDto>(`${base}${qs ? `?${qs}` : ""}`);
}

export async function getSchema(): Promise<SchemaResponseDto> {
  return apiClient<SchemaResponseDto>(PCS_ENDPOINTS.SCHEMA);
}
