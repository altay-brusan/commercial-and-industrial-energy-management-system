# API Services

## Overview

API service modules live in `lib/auth/` and provide typed functions for communicating with the backend. All services use the shared `apiClient()` from `lib/auth/api-client.ts`, which handles auth headers, token refresh, and error parsing.

All pages implement a **mock fallback pattern**: if the API call fails (backend unavailable), the UI falls back to local mock data for development purposes.

---

## User Service

**File:** `lib/auth/user-service.ts`

| Function | Method | Endpoint | Returns |
|----------|--------|----------|---------|
| `getUsers(params?)` | GET | `/Users` | `PaginatedResponse<UserAccount>` |
| `getUserById(id)` | GET | `/Users/{id}` | `UserAccount` |
| `createUser(data)` | POST | `/Users` | `UserAccount` |
| `updateUser(id, data)` | PUT | `/Users/{id}` | `UserAccount` |
| `deleteUser(id)` | DELETE | `/Users/{id}` | `void` |
| `addUserRole(userId, roleId)` | POST | `/Users/{userId}/roles/{roleId}` | `void` |
| `removeUserRole(userId, roleId)` | DELETE | `/Users/{userId}/roles/{roleId}` | `void` |

### Query Parameters (getUsers)

| Param | Type | Description |
|-------|------|-------------|
| `pageNumber` | number | Page number (1-based) |
| `pageSize` | number | Items per page |
| `search` | string | Search by name/email |

### UserAccount Model

```typescript
interface UserAccount {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  initials: string;
  avatar?: string;
  department?: string;
  phone?: string;
  isActive: boolean;
  roleIds: string[];
  createdAt: Date;
  updatedAt: Date;
  lastLogin?: Date;
}
```

### UserFormData (Create/Update payload)

```typescript
interface UserFormData {
  firstName: string;
  lastName: string;
  email: string;
  department: string;
  phone: string;
  isActive: boolean;
  roleIds: string[];
}
```

---

## Role Service

**File:** `lib/auth/role-service.ts`

| Function | Method | Endpoint | Returns |
|----------|--------|----------|---------|
| `getRoles()` | GET | `/TenantRoles` | `TenantRole[]` |
| `getRoleById(id)` | GET | `/TenantRoles/{id}` | `TenantRole` |
| `createRole(data)` | POST | `/TenantRoles` | `TenantRole` |
| `updateRole(id, data)` | PUT | `/TenantRoles/{id}` | `TenantRole` |
| `deleteRole(id)` | DELETE | `/TenantRoles/{id}` | `void` |
| `updateRolePermissions(roleId, permissionIds)` | PUT | `/TenantRoles/{roleId}/permissions` | `void` |
| `addUserToRole(roleId, userId)` | POST | `/TenantRoles/{roleId}/users` | `void` |
| `removeUserFromRole(roleId, userId)` | DELETE | `/TenantRoles/{roleId}/users/{userId}` | `void` |

### TenantRole Model

```typescript
interface TenantRole {
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
```

### RoleFormData (Create/Update payload)

```typescript
interface RoleFormData {
  name: string;
  description: string;
  isActive: boolean;
}
```

---

## Permission Service

**File:** `lib/auth/permission-service.ts`

| Function | Method | Endpoint | Returns |
|----------|--------|----------|---------|
| `getPermissions()` | GET | `/Permissions` | `PermissionGroup[]` |
| `getPermissionsByFeature(featureKey)` | GET | `/Permissions/by-feature/{featureKey}` | `Permission[]` |

### Models

```typescript
interface PermissionGroup {
  feature: Feature;
  permissions: Permission[];
}

interface Feature {
  id: string;
  key: string;        // e.g. "Solar", "Bess", "UserManagement"
  name: string;
  description: string;
  category: "Core" | "Energy" | "Intelligence" | "Management";
  isActive: boolean;
}

interface Permission {
  id: string;
  key: string;        // e.g. "Facility.View", "Scada.Control"
  name: string;
  description: string;
  featureId: string;
}
```

---

## Pagination

All list endpoints that support pagination return:

```typescript
interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}
```

---

## Endpoint Constants

All endpoint paths are defined in `lib/auth/endpoints.ts`:

```typescript
AUTH_ENDPOINTS       // /Auth/Login, /Auth/Logout, etc.
USER_ENDPOINTS       // /Users, /Users/{id}, etc.
ROLE_ENDPOINTS       // /TenantRoles, /TenantRoles/{id}/permissions, etc.
PERMISSION_ENDPOINTS // /Permissions, /Permissions/by-feature/{key}
```

These are relative paths. The `apiClient` prepends `NEXT_PUBLIC_API_BASE_URL` (`/api/v1`) to form the final URL.
