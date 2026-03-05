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
  user: AuthUser;
}

export interface RefreshTokenResponse {
  accessToken: string;
}

export interface ApiErrorResponse {
  message: string;
  errors?: Record<string, string[]>;
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
