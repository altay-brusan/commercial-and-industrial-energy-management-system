import { apiClient } from "./api-client";
import { PERMISSION_ENDPOINTS } from "./endpoints";
import type { Permission, PermissionGroup } from "./types";

export async function getPermissions(): Promise<PermissionGroup[]> {
  return apiClient<PermissionGroup[]>(PERMISSION_ENDPOINTS.LIST);
}

export async function getPermissionsByFeature(
  featureKey: string
): Promise<Permission[]> {
  return apiClient<Permission[]>(PERMISSION_ENDPOINTS.BY_FEATURE(featureKey));
}
