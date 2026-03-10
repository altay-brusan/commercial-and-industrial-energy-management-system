import { apiClient } from "./api-client";
import { ROLE_ENDPOINTS } from "./endpoints";
import type { TenantRole, RoleFormData } from "./types";

export async function getRoles(): Promise<TenantRole[]> {
  return apiClient<TenantRole[]>(ROLE_ENDPOINTS.LIST);
}

export async function getRoleById(id: string): Promise<TenantRole> {
  return apiClient<TenantRole>(ROLE_ENDPOINTS.GET(id));
}

export async function createRole(data: RoleFormData): Promise<TenantRole> {
  return apiClient<TenantRole>(ROLE_ENDPOINTS.CREATE, {
    method: "POST",
    body: JSON.stringify(data),
  });
}

export async function updateRole(
  id: string,
  data: RoleFormData
): Promise<TenantRole> {
  return apiClient<TenantRole>(ROLE_ENDPOINTS.UPDATE(id), {
    method: "PUT",
    body: JSON.stringify(data),
  });
}

export async function deleteRole(id: string): Promise<void> {
  return apiClient<void>(ROLE_ENDPOINTS.DELETE(id), {
    method: "DELETE",
  });
}

export async function updateRolePermissions(
  roleId: string,
  permissionIds: string[]
): Promise<void> {
  return apiClient<void>(ROLE_ENDPOINTS.UPDATE_PERMISSIONS(roleId), {
    method: "PUT",
    body: JSON.stringify({ permissionIds }),
  });
}

export async function addUserToRole(
  roleId: string,
  userId: string
): Promise<void> {
  return apiClient<void>(ROLE_ENDPOINTS.ADD_USER(roleId), {
    method: "POST",
    body: JSON.stringify({ userId }),
  });
}

export async function removeUserFromRole(
  roleId: string,
  userId: string
): Promise<void> {
  return apiClient<void>(ROLE_ENDPOINTS.REMOVE_USER(roleId, userId), {
    method: "DELETE",
  });
}
