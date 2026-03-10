import { apiClient } from "./api-client";
import { USER_ENDPOINTS } from "./endpoints";
import type { PaginatedResponse } from "./types";
import type { UserAccount, UserFormData } from "./user-types";

interface GetUsersParams {
  pageNumber?: number;
  pageSize?: number;
  search?: string;
}

export async function getUsers(
  params?: GetUsersParams
): Promise<PaginatedResponse<UserAccount>> {
  const searchParams = new URLSearchParams();
  if (params?.pageNumber) searchParams.set("pageNumber", String(params.pageNumber));
  if (params?.pageSize) searchParams.set("pageSize", String(params.pageSize));
  if (params?.search) searchParams.set("search", params.search);

  const query = searchParams.toString();
  const endpoint = `${USER_ENDPOINTS.LIST}${query ? `?${query}` : ""}`;
  return apiClient<PaginatedResponse<UserAccount>>(endpoint);
}

export async function getUserById(id: string): Promise<UserAccount> {
  return apiClient<UserAccount>(USER_ENDPOINTS.GET(id));
}

export async function createUser(data: UserFormData): Promise<UserAccount> {
  return apiClient<UserAccount>(USER_ENDPOINTS.LIST, {
    method: "POST",
    body: JSON.stringify(data),
  });
}

export async function updateUser(
  id: string,
  data: UserFormData
): Promise<UserAccount> {
  return apiClient<UserAccount>(USER_ENDPOINTS.UPDATE(id), {
    method: "PUT",
    body: JSON.stringify(data),
  });
}

export async function deleteUser(id: string): Promise<void> {
  return apiClient<void>(USER_ENDPOINTS.DELETE(id), {
    method: "DELETE",
  });
}

export async function addUserRole(
  userId: string,
  roleId: string
): Promise<void> {
  return apiClient<void>(USER_ENDPOINTS.ADD_ROLE(userId, roleId), {
    method: "POST",
  });
}

export async function removeUserRole(
  userId: string,
  roleId: string
): Promise<void> {
  return apiClient<void>(USER_ENDPOINTS.REMOVE_ROLE(userId, roleId), {
    method: "DELETE",
  });
}
