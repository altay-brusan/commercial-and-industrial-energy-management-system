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
