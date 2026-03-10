export const AUTH_ENDPOINTS = {
  LOGIN: "/Auth/Login",
  LOGOUT: "/Auth/Logout",
  REFRESH_TOKEN: "/Auth/RefreshToken",
  FORGOT_PASSWORD: "/Auth/ForgotPassword",
  RESET_PASSWORD: "/Auth/ResetPassword",
  ME: "/Auth/Me",
} as const;

export const USER_ENDPOINTS = {
  LIST: "/Users",
  GET: (id: string) => `/Users/${id}`,
  UPDATE: (id: string) => `/Users/${id}`,
  DELETE: (id: string) => `/Users/${id}`,
  USER_INFO: "/Users/userinfo",
  ADD_ROLE: (userId: string, roleId: string) => `/Users/${userId}/roles/${roleId}`,
  REMOVE_ROLE: (userId: string, roleId: string) => `/Users/${userId}/roles/${roleId}`,
} as const;

export const ROLE_ENDPOINTS = {
  LIST: "/TenantRoles",
  GET: (id: string) => `/TenantRoles/${id}`,
  CREATE: "/TenantRoles",
  UPDATE: (id: string) => `/TenantRoles/${id}`,
  DELETE: (id: string) => `/TenantRoles/${id}`,
  UPDATE_PERMISSIONS: (roleId: string) => `/TenantRoles/${roleId}/permissions`,
  ADD_USER: (roleId: string) => `/TenantRoles/${roleId}/users`,
  REMOVE_USER: (roleId: string, userId: string) => `/TenantRoles/${roleId}/users/${userId}`,
} as const;

export const PERMISSION_ENDPOINTS = {
  LIST: "/Permissions",
  BY_FEATURE: (featureKey: string) => `/Permissions/by-feature/${featureKey}`,
} as const;
