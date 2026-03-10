# Authentication & Authorization

## Overview

The application uses **JWT-based authentication** with httpOnly cookie refresh tokens. The auth system is managed by `AuthProvider` context which wraps the entire app.

## Auth Flow

```
1. User submits login form
2. POST /api/v1/Auth/Login → { accessToken, tokenType, expiresIn, user }
3. accessToken stored in localStorage
4. refreshToken set as httpOnly cookie by backend
5. AuthProvider sets user state → redirect to dashboard
6. All subsequent API calls include Authorization: Bearer <token>
7. On 401 → automatic refresh attempt → retry original request
8. On refresh failure → clear token → redirect to /login
```

## Files

| File | Purpose |
|------|---------|
| `lib/auth/types.ts` | All auth-related TypeScript types |
| `lib/auth/endpoints.ts` | API endpoint path constants |
| `lib/auth/api-client.ts` | `apiClient()` fetch wrapper, `ApiError` class, token management |
| `lib/auth/auth-service.ts` | `login()`, `logout()`, `forgotPassword()`, `resetPassword()`, `getMe()` |
| `lib/auth/auth-context.tsx` | `AuthProvider` context, `useAuth()` hook, route protection |
| `lib/auth/schemas.ts` | Zod validation schemas for auth forms |

## API Endpoints

All auth endpoints are relative to `NEXT_PUBLIC_API_BASE_URL` (default: `/api/v1`).

| Action | Method | Endpoint | Notes |
|--------|--------|----------|-------|
| Login | POST | `/Auth/Login` | Returns accessToken + user object |
| Logout | POST | `/Auth/Logout` | Invalidates refresh token |
| Refresh Token | POST | `/Auth/RefreshToken` | Expired token in Auth header + cookie |
| Forgot Password | POST | `/Auth/ForgotPassword` | Sends reset email |
| Reset Password | POST | `/Auth/ResetPassword` | Token-based password reset |
| Get Current User | GET | `/Auth/Me` | Returns authenticated user profile |

## Token Management

### Access Token
- Stored in `localStorage` via `getAccessToken()` / `setAccessToken()`
- Included in all API requests as `Authorization: Bearer <token>`
- Short-lived (15 minutes, configured by backend)

### Refresh Token
- Stored as **httpOnly secure cookie** by the backend
- Sent automatically via `credentials: "include"` on fetch calls
- On refresh, the **expired access token** must be sent in the Authorization header
- Token rotation: old refresh token is revoked, new one issued

## ApiClient

`apiClient<T>(endpoint, options?, baseUrl?)` — generic fetch wrapper.

Features:
- Prepends `NEXT_PUBLIC_API_BASE_URL` to endpoint
- Adds `Authorization: Bearer` header if token exists
- Adds `credentials: "include"` for cookie transport
- Auto-retries on 401 with token refresh
- Parses error responses in `{ error: { code, message, details?, retryAfter? } }` format
- Handles 204 No Content

### ApiError Class

```typescript
class ApiError extends Error {
  status: number;      // HTTP status code
  code: string;        // Machine-readable error code (e.g. "INVALID_CREDENTIALS")
  details?: string[];  // Additional error details
  retryAfter?: number; // Seconds to wait (rate limit / lockout)
}
```

## AuthProvider

Located in `lib/auth/auth-context.tsx`:

- **Session Restoration**: On mount, checks for existing token → calls `getMe()` to restore user
- **Route Protection**: Redirects unauthenticated users from protected routes to `/login`
- **Auth Guard**: Returns `null` (blank screen) on protected routes while loading, preventing flash

### Public Routes
- `/login`
- `/forgot-password`
- `/reset-password`

### useAuth() Hook

```typescript
const { user, isAuthenticated, isLoading, login, logout } = useAuth();
```

## Login Response Shape

```json
{
  "accessToken": "eyJhbG...",
  "tokenType": "Bearer",
  "expiresIn": 900,
  "user": {
    "id": "abc-123",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "role": "Operator",
    "avatarUrl": null,
    "permissions": ["Facility.View", "Monitoring.View"],
    "tenantId": "1",
    "tenantName": "N2N Energy",
    "lastLoginAt": null,
    "createdAt": null
  }
}
```

## Error Response Format

```json
{
  "error": {
    "code": "INVALID_CREDENTIALS",
    "message": "Wrong email or password.",
    "details": ["Optional additional context"],
    "retryAfter": 900
  }
}
```

### Common Error Codes

| Code | Status | Description |
|------|--------|-------------|
| `VALIDATION_ERROR` | 400 | Invalid request data |
| `INVALID_CREDENTIALS` | 401 | Wrong email/password |
| `UNAUTHORIZED` | 401 | Missing or invalid token |
| `INVALID_REFRESH_TOKEN` | 401 | Expired/revoked refresh token |
| `ACCOUNT_LOCKED` | 423 | Too many failed login attempts |
| `RATE_LIMIT_EXCEEDED` | 429 | Rate limit hit (5 req / 15 min) |
