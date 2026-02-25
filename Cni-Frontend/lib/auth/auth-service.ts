import { apiClient, setAccessToken } from "./api-client";
import { AUTH_ENDPOINTS } from "./endpoints";
import type {
  AuthUser,
  LoginRequest,
  LoginResponse,
  ForgotPasswordRequest,
  ResetPasswordRequest,
} from "./types";

export async function login(data: LoginRequest): Promise<{ user: AuthUser }> {
  const res = await apiClient<LoginResponse>(AUTH_ENDPOINTS.LOGIN, {
    method: "POST",
    body: JSON.stringify(data),
  });
  setAccessToken(res.accessToken);
  return { user: res.user };
}

export async function logout(): Promise<void> {
  try {
    await apiClient<void>(AUTH_ENDPOINTS.LOGOUT, { method: "POST" });
  } finally {
    setAccessToken(null);
  }
}

export async function forgotPassword(
  data: ForgotPasswordRequest
): Promise<void> {
  await apiClient<void>(AUTH_ENDPOINTS.FORGOT_PASSWORD, {
    method: "POST",
    body: JSON.stringify(data),
  });
}

export async function resetPassword(
  data: ResetPasswordRequest
): Promise<void> {
  await apiClient<void>(AUTH_ENDPOINTS.RESET_PASSWORD, {
    method: "POST",
    body: JSON.stringify(data),
  });
}

export async function getMe(): Promise<AuthUser> {
  return apiClient<AuthUser>(AUTH_ENDPOINTS.ME);
}
