export interface AuthUser {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  avatarUrl: string | null;
  permissions: string[];
  tenantId: string;
  tenantName: string;
  lastLoginAt?: string;
  createdAt?: string;
}

// API request types
export interface LoginRequest {
  email: string;
  password: string;
  rememberMe?: boolean;
}

export interface ForgotPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  token: string;
  newPassword: string;
  confirmPassword: string;
}

// API response types
export interface LoginResponse {
  accessToken: string;
  tokenType: string;
  expiresIn: number;
  user: AuthUser;
}

export interface RefreshTokenResponse {
  accessToken: string;
  tokenType: string;
  expiresIn: number;
}

export interface ApiErrorResponse {
  error: {
    code: string;
    message: string;
    details?: string[];
    retryAfter?: number;
  };
}

// Pagination
export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

// Feature & Permission types

export type FeatureCategory = "Core" | "Energy" | "Intelligence" | "Management";

export interface Feature {
  id: string;
  key: string;
  name: string;
  description: string;
  category: FeatureCategory;
  isActive: boolean;
}

export interface Permission {
  id: string;
  key: string;
  name: string;
  description: string;
  featureId: string;
}

export interface PermissionGroup {
  feature: Feature;
  permissions: Permission[];
}

// Role types

export interface TenantRole {
  id: string;
  tenantId: string;
  name: string;
  description: string;
  isActive: boolean;
  isSystem: boolean;
  createdAt: Date;
  updatedAt: Date;
  permissionIds: string[];
  userIds: string[];
}

export interface RoleFormData {
  name: string;
  description: string;
  isActive: boolean;
}

// UserInfo response from /api/Users/userinfo
export interface UserInfoResponse {
  email: string;
  name: string;
  surname: string;
  roles: string[];
  permissions: string[];
  planFeatures: string[];
  tenants: unknown[];
}
