import { apiClient } from "@/lib/auth/api-client";
import { FACILITY_ENDPOINTS } from "./endpoints";
import type { PaginatedFacilitiesDto } from "./types";

// Facilities API is not versioned (/api/Facilities, not /api/v1/Facilities).
// apiClient prepends NEXT_PUBLIC_API_BASE_URL (/api/v1), so we override with
// an absolute base to reach /api directly.
const FACILITY_BASE = "/api";

export async function getFacilities(params?: {
  pageNumber?: number;
  pageSize?: number;
}): Promise<PaginatedFacilitiesDto> {
  const searchParams = new URLSearchParams();
  if (params?.pageNumber) searchParams.set("PageNumber", String(params.pageNumber));
  if (params?.pageSize) searchParams.set("PageSize", String(params.pageSize));
  const qs = searchParams.toString();
  return apiClient<PaginatedFacilitiesDto>(
    `${FACILITY_ENDPOINTS.LIST}${qs ? `?${qs}` : ""}`,
    {},
    FACILITY_BASE
  );
}
