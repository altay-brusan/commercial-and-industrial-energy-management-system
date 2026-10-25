import type { ApiErrorResponse, RefreshTokenResponse } from "./types";
import { AUTH_ENDPOINTS } from "./endpoints";

const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL ?? "";

export class ApiError extends Error {
  status: number;
  code: string;
  details?: string[];
  retryAfter?: number;

  constructor(
    status: number,
    message: string,
    code: string = "UNKNOWN_ERROR",
    details?: string[],
    retryAfter?: number
  ) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.code = code;
    this.details = details;
    this.retryAfter = retryAfter;
  }
}

// Token storage
export function getAccessToken(): string | null {
  if (typeof window === "undefined") return null;
  return localStorage.getItem("access_token");
}

export function setAccessToken(token: string | null): void {
  if (typeof window === "undefined") return;
  if (token) {
    localStorage.setItem("access_token", token);
  } else {
    localStorage.removeItem("access_token");
  }
}

// Refresh lock to prevent concurrent refresh calls
let refreshPromise: Promise<string> | null = null;

async function refreshAccessToken(): Promise<string> {
  if (refreshPromise) return refreshPromise;

  refreshPromise = (async () => {
    try {
      const expiredToken = getAccessToken();
      const headers: Record<string, string> = {
        "Content-Type": "application/json",
      };

      // Send expired token in Authorization header as required by backend
      if (expiredToken) {
        headers["Authorization"] = `Bearer ${expiredToken}`;
      }

      const res = await fetch(`${API_BASE_URL}${AUTH_ENDPOINTS.REFRESH_TOKEN}`, {
        method: "POST",
        headers,
        credentials: "include",
      });

      if (!res.ok) {
        throw new ApiError(res.status, "Session expired", "SESSION_EXPIRED");
      }

      const data: RefreshTokenResponse = await res.json();
      setAccessToken(data.accessToken);
      return data.accessToken;
    } finally {
      refreshPromise = null;
    }
  })();

  return refreshPromise;
}

export async function apiClient<T>(
  endpoint: string,
  options: RequestInit = {},
  baseUrl?: string
): Promise<T> {
  const base = baseUrl ?? API_BASE_URL;
  const token = getAccessToken();

  const headers: Record<string, string> = {
    "Content-Type": "application/json",
    ...(options.headers as Record<string, string>),
  };

  if (token) {
    headers["Authorization"] = `Bearer ${token}`;
  }

  let res = await fetch(`${base}${endpoint}`, {
    ...options,
    headers,
    credentials: "include",
  });

  // On 401, attempt token refresh and retry once
  if (res.status === 401 && token) {
    try {
      const newToken = await refreshAccessToken();
      headers["Authorization"] = `Bearer ${newToken}`;
      res = await fetch(`${base}${endpoint}`, {
        ...options,
        headers,
        credentials: "include",
      });
    } catch {
      setAccessToken(null);
      if (typeof window !== "undefined") {
        window.location.href = "/login";
      }
      throw new ApiError(401, "Session expired. Please sign in again.", "SESSION_EXPIRED");
    }
  }

  if (!res.ok) {
    let errorData: ApiErrorResponse | undefined;
    try {
      errorData = await res.json();
    } catch {
      // Response body may not be JSON
    }
    throw new ApiError(
      res.status,
      errorData?.error?.message ?? `Request failed (${res.status})`,
      errorData?.error?.code ?? "UNKNOWN_ERROR",
      errorData?.error?.details,
      errorData?.error?.retryAfter
    );
  }

  // Handle 204 No Content
  if (res.status === 204) {
    return undefined as T;
  }

  return res.json();
}
