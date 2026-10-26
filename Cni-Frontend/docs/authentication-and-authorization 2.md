# Enhanced Authentication & Authorization System

## Overview

The DERMS backend implements **JWT-based authentication** with a **two-layer authorization model** that combines plan-based feature gating with granular permission-based access control.

- **Authentication** is handled via standard JWT access tokens and httpOnly refresh token cookies.
- **Authorization** uses ASP.NET Identity roles plus a custom permission/feature system.

---

## Part 1: Authentication (JWT)

### Architecture

The system uses **standard JWT access tokens** (signed with HMAC-SHA256) and **httpOnly cookie-based refresh tokens** with token rotation and revocation support.

```
Client                          Server
  │                                │
  ├── POST /api/v1/Auth/Login ────>│  (email + password)
  │<── 200 { accessToken, user } ──│  + Set-Cookie: refreshToken=<value>; HttpOnly; Secure
  │                                │
  ├── GET /api/Facilities ────────>│  Authorization: Bearer <accessToken>
  │<── 200 { data } ──────────────│
  │                                │
  │  ... access token expires ...  │
  │                                │
  ├── POST /api/v1/Auth/Refresh ──>│  Authorization: Bearer <expired-token>
  │   Cookie: refreshToken=<val>   │  + Cookie: refreshToken=<value>
  │<── 200 { accessToken } ────────│  + Set-Cookie: refreshToken=<new-value>
  │                                │
```

### JWT Token Structure

Access tokens contain the following claims:

| Claim | Type | Description |
|-------|------|-------------|
| `sub` / `NameIdentifier` | string | User ID (ASP.NET Identity) |
| `email` | string | User email address |
| `tenant_id` | string? | Tenant ID (omitted if user has no tenant) |
| `role` | string[] | One claim per role (e.g. `Administrator`, `User`) |
| `permission` | string[] | One claim per permission (e.g. `Facility.View`) |
| `jti` | string | Unique token identifier (GUID) |
| `exp` | int | Expiration timestamp (Unix) |
| `iss` | string | Issuer (`DERMS-Backend`) |
| `aud` | string | Audience (`DERMS-Frontend`) |

**File:** `src/Infrastructure/Services/JwtTokenService.cs`

### Configuration

JWT settings are configured in `appsettings.json`:

```json
"JwtSettings": {
  "Secret": "<256-bit-secret-key>",
  "Issuer": "DERMS-Backend",
  "Audience": "DERMS-Frontend",
  "AccessTokenExpirationMinutes": 15,
  "RefreshTokenExpirationDays": 7
}
```

| Setting | Default | Description |
|---------|---------|-------------|
| `Secret` | — | HMAC-SHA256 signing key (min 32 characters) |
| `Issuer` | `DERMS-Backend` | JWT `iss` claim |
| `Audience` | `DERMS-Frontend` | JWT `aud` claim |
| `AccessTokenExpirationMinutes` | `15` | Access token TTL |
| `RefreshTokenExpirationDays` | `7` | Refresh token TTL |

**File:** `src/Infrastructure/Configurations/JwtSettings.cs`

---

### Auth API Endpoints

#### POST `/api/v1/Auth/Login`

Authenticates a user and returns a JWT access token with a refresh token cookie.

**Rate Limited:** 5 requests per 15 minutes per client (configurable).

**Request:**
```json
{
  "email": "user@example.com",
  "password": "<your-password>",
  "rememberMe": false
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `email` | string | Yes | User email |
| `password` | string | Yes | User password |
| `rememberMe` | bool | No | If `true`, sets a persistent cookie with `Max-Age`. If `false` (default), sets a session cookie. |

**Success Response (200):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "tokenType": "Bearer",
  "expiresIn": 900,
  "user": {
    "id": "abc-123",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "role": "User",
    "avatarUrl": null,
    "permissions": ["Facility.View", "Facility.Create", "Device.View"]
  }
}
```

**Response Headers:**
```
Set-Cookie: refreshToken=<base64-value>; HttpOnly; Secure; SameSite=Strict; Path=/api/v1/Auth
```

**Error Responses:**

| Status | Code | Condition |
|--------|------|-----------|
| 400 | `VALIDATION_ERROR` | Empty email or password |
| 401 | `INVALID_CREDENTIALS` | Wrong email or password |
| 423 | `ACCOUNT_LOCKED` | Too many failed login attempts (Identity lockout) |
| 429 | `RATE_LIMIT_EXCEEDED` | Rate limit exceeded |

---

#### POST `/api/v1/Auth/RefreshToken`

Exchanges an expired access token + valid refresh cookie for a new token pair. Implements **token rotation** — the old refresh token is revoked and a new one is issued.

**Request:**
- `Authorization: Bearer <expired-access-token>` header (required)
- `Cookie: refreshToken=<value>` cookie (required, set automatically by browser)
- No request body

**Success Response (200):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "tokenType": "Bearer",
  "expiresIn": 900
}
```

**Response Headers:**
```
Set-Cookie: refreshToken=<new-base64-value>; HttpOnly; Secure; SameSite=Strict; Path=/api/v1/Auth; Max-Age=604800
```

**Error Responses:**

| Status | Code | Condition |
|--------|------|-----------|
| 401 | `INVALID_REFRESH_TOKEN` | Missing, expired, or revoked refresh token |
| 401 | `INVALID_TOKEN` | Missing or invalid access token in Authorization header |

---

#### POST `/api/v1/Auth/Logout`

Terminates the current session. Invalidates the refresh token and clears the refresh token cookie.

**Requires:** Valid (non-expired) JWT access token via `Authorization: Bearer <token>` header.

**Request:**
- `Authorization: Bearer <access-token>` header (required)
- `Cookie: refreshToken=<value>` cookie (optional — if present, the token is revoked)
- No request body

**Success Response (200):**
```json
{
  "message": "Successfully logged out"
}
```

**Response Headers:**
```
Set-Cookie: refreshToken=; HttpOnly; Secure; SameSite=Strict; Path=/api/v1/Auth; Max-Age=0
```

**Error Responses:**

| Status | Code | Condition |
|--------|------|-----------|
| 401 | `UNAUTHORIZED` | Access token is missing or invalid |

---

#### GET `/api/v1/Auth/Me`

Returns the authenticated user's profile, permissions, and tenant information.

**Requires:** Valid (non-expired) JWT access token via `Authorization: Bearer <token>` header.

**Request:**
- `Authorization: Bearer <access-token>` header (required)
- No request body

**Success Response (200):**
```json
{
  "id": "user-001",
  "email": "john@company.com",
  "firstName": "John",
  "lastName": "Doe",
  "role": "Operator",
  "avatarUrl": null,
  "permissions": ["Facility.View", "Device.View", "Monitoring.View"],
  "tenantId": "1",
  "tenantName": "N2N Energy",
  "lastLoginAt": null,
  "createdAt": null
}
```

| Field | Type | Description |
|-------|------|-------------|
| `id` | string | User ID |
| `email` | string | User email |
| `firstName` | string | User's first name |
| `lastName` | string | User's last name |
| `role` | string | Primary role (`Administrator`, `TenantAdmin`, `User`, `Operator`, `Viewer`) |
| `avatarUrl` | string? | Profile picture URL (currently always `null`) |
| `permissions` | string[] | List of permission keys |
| `tenantId` | string? | Tenant ID (null if no tenant) |
| `tenantName` | string? | Tenant display name (null if no tenant) |
| `lastLoginAt` | datetime? | Last login timestamp (reserved for future use) |
| `createdAt` | datetime? | Account creation timestamp (reserved for future use) |

**Error Responses:**

| Status | Code | Condition |
|--------|------|-----------|
| 401 | `UNAUTHORIZED` | Access token is missing or invalid |

**File:** `src/Web/Endpoints/Auth.cs`

---

### Error Response Format

All auth endpoints use a consistent error format:

```json
{
  "error": {
    "code": "ERROR_CODE",
    "message": "Human-readable description.",
    "details": ["Optional additional context"],
    "retryAfter": 540
  }
}
```

| Field | Type | Description |
|-------|------|-------------|
| `code` | string | Machine-readable error code |
| `message` | string | Human-readable message |
| `details` | string[]? | Optional list of details (e.g. validation errors) |
| `retryAfter` | int? | Seconds to wait before retrying (rate limit / lockout) |

**File:** `src/Application/Common/Models/AuthModels.cs`

---

### Refresh Token Storage

Refresh tokens are **hashed with SHA-256** before being stored in the database. The raw token is only ever held in the httpOnly cookie.

#### RefreshToken Entity

| Field | Type | Description |
|-------|------|-------------|
| `Id` | int | Primary key |
| `UserId` | string | FK to `ApplicationUser` |
| `Token` | string | SHA-256 hash of the raw refresh token |
| `ExpiresAt` | DateTime | When the token expires |
| `IsRevoked` | bool | Whether the token has been revoked |
| `ReplacedByToken` | string? | Hash of the replacement token (rotation chain) |
| `CreatedByIp` | string? | IP address that created the token |
| `Created` | DateTime | Inherited from BaseAuditableEntity |
| `LastModified` | DateTime | Inherited from BaseAuditableEntity |

**Indexes:**
- Unique index on `Token`
- Index on `UserId`
- Cascade delete when user is deleted

**File:** `src/Domain/Entities/RefreshToken.cs`

#### Token Rotation

When a refresh token is used:
1. The old token is marked as `IsRevoked = true`
2. A new token is generated and stored
3. The old token's `ReplacedByToken` points to the new token's hash
4. If a revoked token is reused, the request is rejected with 401

This creates a linked chain: `Token A (revoked) → Token B (revoked) → Token C (active)`

---

### Rate Limiting

Login is protected with a **fixed window rate limiter**:

| Setting | Production | Test |
|---------|-----------|------|
| Permit Limit | 5 requests | 10,000 requests |
| Window | 15 minutes | 1 minute |
| Queue Limit | 0 (reject immediately) | 0 |

When rate limited, a `429 Too Many Requests` response is returned:
```json
{
  "error": {
    "code": "RATE_LIMIT_EXCEEDED",
    "message": "Too many login attempts. Please try again later.",
    "retryAfter": 900
  }
}
```

The `Retry-After` HTTP header is also set.

**File:** `src/Web/DependencyInjection.cs`

---

### Cookie Configuration

| Property | Value | Purpose |
|----------|-------|---------|
| `HttpOnly` | `true` | Prevents JavaScript access (XSS protection) |
| `Secure` | `true` | Only sent over HTTPS |
| `SameSite` | `Strict` | Only sent for same-origin requests (CSRF protection) |
| `Path` | `/api/v1/Auth` | Only sent to Auth endpoints (minimizes exposure) |
| `Max-Age` | 7 days (rememberMe) / session | Persistence controlled by `rememberMe` flag |

---

### How Existing Middleware Uses JWT Claims

The JWT claims are designed to be compatible with all existing middleware and services:

| Component | Claim Used | Purpose |
|-----------|------------|---------|
| `CurrentUser.cs` | `ClaimTypes.NameIdentifier` | Reads user ID for `IUser` interface |
| `MultiTenantMiddleware.cs` | `tenant_id` | Sets current tenant context |
| `AuthorizationBehaviour.cs` | `ClaimTypes.Role` | Checks `[Authorize(Roles = "...")]` attributes |
| `[RequirePermission]` pipeline | `permission` claims | Checks fine-grained permissions |

---

## Part 2: Authorization

### Authorization Flow

```
Request arrives
  │
  ├─ [Authorize] check (role-based)
  │   └─ User must have one of the specified Identity roles
  │
  └─ [RequirePermission] check (permission-based)
      │
      ├─ Is user an Administrator? ──── YES → ALLOW (bypass all)
      │
      ├─ Does tenant's Plan include the feature? ──── NO → 403
      │
      ├─ Is user a TenantAdmin? ──── YES → ALLOW (all permissions within plan)
      │
      └─ Does user's TenantRole include the permission? ──── NO → 403 → YES → ALLOW
```

---

## Data Model

### Entity Relationship Diagram

```
Plan ──< PlanFeature >── Feature ──< Permission
 |                                       |
 v                                       v
Tenant ──< TenantRole ──< TenantRolePermission
                |
                v
           UserTenantRole ──> ApplicationUser
```

### System Roles (ASP.NET Identity)

| Role | Description |
|------|-------------|
| `Administrator` | Super admin with full system access across all tenants. Bypasses all permission and feature checks. |
| `TenantAdmin` | Tenant administrator. Gets all permissions within their tenant's plan features. |
| `User` | Regular user. Access determined by assigned TenantRole permissions. |
| `Operator` | Can view data and change settings within their tenant. |
| `Viewer` | Read-only access within their tenant. |

**Constants file:** `src/Domain/Constants/Roles.cs`

### Entities

#### Feature
Represents a module/capability of the system that can be enabled or disabled per plan.

| Field | Type | Description |
|-------|------|-------------|
| Id | int | Primary key |
| Key | string(100) | Unique identifier (e.g. `"Monitoring"`) |
| Name | string(200) | Display name |
| Description | string(500)? | Optional description |
| Category | string(100)? | Grouping category (Core, Energy, Intelligence, Management) |
| IsActive | bool | Whether the feature is globally active |

**File:** `src/Domain/Entities/Feature.cs`

#### Permission
Represents a specific action within a feature that can be granted to roles.

| Field | Type | Description |
|-------|------|-------------|
| Id | int | Primary key |
| Key | string(200) | Unique identifier in `Feature.Action` format (e.g. `"Facility.Create"`) |
| Name | string(200) | Display name |
| Description | string(500)? | Optional description |
| FeatureId | int | FK to parent Feature |

**File:** `src/Domain/Entities/Permission.cs`

#### PlanFeature
Junction table linking Plans to the Features they include.

| Field | Type | Description |
|-------|------|-------------|
| Id | int | Primary key |
| PlanId | int | FK to Plan |
| FeatureId | int | FK to Feature |

**Unique constraint:** `(PlanId, FeatureId)`

**File:** `src/Domain/Entities/PlanFeature.cs`

#### TenantRole
Custom roles created by tenant admins. Multi-tenant scoped.

| Field | Type | Description |
|-------|------|-------------|
| Id | int | Primary key |
| TenantId | int | FK to Tenant (implements IMultiTenantEntity) |
| Name | string(200) | Role name (unique per tenant) |
| Description | string(500)? | Optional description |
| IsActive | bool | Whether the role is active |

**Unique constraint:** `(TenantId, Name)`

**File:** `src/Domain/Entities/TenantRole.cs`

#### TenantRolePermission
Junction table linking TenantRoles to Permissions.

| Field | Type | Description |
|-------|------|-------------|
| Id | int | Primary key |
| TenantRoleId | int | FK to TenantRole |
| PermissionId | int | FK to Permission |

**Unique constraint:** `(TenantRoleId, PermissionId)`

**File:** `src/Domain/Entities/TenantRolePermission.cs`

#### UserTenantRole
Assigns a user to a TenantRole.

| Field | Type | Description |
|-------|------|-------------|
| Id | int | Primary key |
| UserId | string(450) | FK to ApplicationUser |
| TenantRoleId | int | FK to TenantRole |

**Unique constraint:** `(UserId, TenantRoleId)`

**File:** `src/Domain/Entities/UserTenantRole.cs`

### Modified Existing Entities

#### Tenant
Added fields:
- `PlanId` (int?) - FK to Plan, nullable. Determines which features are available to the tenant.
- `TenantRoles` (collection) - Navigation property.

#### Plan
Added fields:
- `PlanFeatures` (collection) - Navigation property to junction table.
- `Tenants` (collection) - Navigation property.

#### ApplicationUser
Added fields:
- `UserTenantRoles` (collection) - Navigation property.

---

## Features & Permissions

### Feature List (16 features)

| Key | Name | Category |
|-----|------|----------|
| `Portfolio` | Portfolio | Core |
| `Dashboard` | Dashboard | Core |
| `Monitoring` | Monitoring | Core |
| `FacilityMap` | Facility Map | Core |
| `Solar` | Solar | Energy |
| `Bess` | BESS | Energy |
| `Pcs` | PCS | Energy |
| `GridAndLoads` | Grid & Loads | Energy |
| `Scada` | SCADA | Energy |
| `Analytics` | Analytics | Intelligence |
| `Financials` | Financials | Intelligence |
| `Reporting` | Reporting | Intelligence |
| `Operations` | Operations | Management |
| `Settings` | Settings | Management |
| `UserManagement` | User Management | Management |
| `Audit` | Audit | Management |

**Constants file:** `src/Domain/Constants/Features.cs`

### Permission List (34 permissions)

| Permission Key | Name | Feature |
|---------------|------|---------|
| `Portfolio.View` | View Portfolio | Portfolio |
| `Dashboard.View` | View Dashboard | Dashboard |
| `Monitoring.View` | View Monitoring | Monitoring |
| `Solar.View` | View Solar | Solar |
| `Bess.View` | View BESS | Bess |
| `Pcs.View` | View PCS | Pcs |
| `GridAndLoads.View` | View Grid & Loads | GridAndLoads |
| `Scada.View` | View SCADA | Scada |
| `Scada.Control` | Control SCADA | Scada |
| `Analytics.View` | View Analytics | Analytics |
| `Financials.View` | View Financials | Financials |
| `Reporting.View` | View Reporting | Reporting |
| `Reporting.Export` | Export Reports | Reporting |
| `Operations.View` | View Operations | Operations |
| `Operations.Manage` | Manage Operations | Operations |
| `Firm.View` | View Firms | Settings |
| `Firm.Create` | Create Firms | Settings |
| `Firm.Edit` | Edit Firms | Settings |
| `Firm.Delete` | Delete Firms | Settings |
| `Facility.View` | View Facilities | Settings |
| `Facility.Create` | Create Facilities | Settings |
| `Facility.Edit` | Edit Facilities | Settings |
| `Facility.Delete` | Delete Facilities | Settings |
| `Device.View` | View Devices | Settings |
| `Device.Create` | Create Devices | Settings |
| `UserManagement.View` | View Users | UserManagement |
| `UserManagement.Create` | Create Users | UserManagement |
| `UserManagement.Edit` | Edit Users | UserManagement |
| `UserManagement.Delete` | Delete Users | UserManagement |
| `Audit.View` | View Audit Logs | Audit |
| `FacilityMap.View` | View Facility Map | FacilityMap |

**Constants file:** `src/Domain/Constants/Permissions.cs`

---

## Plans (Seed Data)

Three plans are seeded with different feature sets:

### Starter Plan
- **Price:** $0
- **Max Facilities:** 5
- **Max Users:** 10
- **API Access:** No
- **Features:** Portfolio, Dashboard, Monitoring, FacilityMap, Settings

### Professional Plan
- **Price:** $499
- **Max Facilities:** 50
- **Max Users:** 100
- **API Access:** Yes
- **Features:** All Starter + Solar, Bess, GridAndLoads, Analytics, Financials, Reporting

### Enterprise Plan
- **Price:** $1,999
- **Max Facilities:** 999
- **Max Users:** 999
- **API Access:** Yes
- **Features:** All 15 features

The demo tenant is assigned to the **Enterprise** plan.

---

## Authorization Pipeline

### How It Works

The `AuthorizationBehaviour<TRequest, TResponse>` is a MediatR pipeline behavior that runs before every command/query handler. It processes two types of attributes:

#### 1. `[Authorize]` (existing)
```csharp
[Authorize(Roles = "Administrator,TenantAdmin")]
public record CreateFirmCommand : IRequest<int> { ... }
```
- Checks the user has one of the specified ASP.NET Identity roles
- Uses `IIdentityService.IsInRoleAsync()`

#### 2. `[RequirePermission]` (new)
```csharp
[RequirePermission(Backend.Domain.Constants.Permissions.FirmCreate)]
public record CreateFirmCommand : IRequest<int> { ... }
```
- Checks the user has the specified permission through their tenant role
- Checks the tenant's plan includes the feature that owns the permission
- Administrators bypass all checks
- TenantAdmins bypass permission checks (but still need plan feature)

**Both attributes are additive** - if both are present, both must pass.

### Pipeline File
`src/Application/Common/Behaviours/AuthorizationBehaviour.cs`

### Permission Service
`src/Application/Common/Interfaces/IPermissionService.cs` (interface)
`src/Infrastructure/Services/PermissionService.cs` (implementation)

Methods:
- `TenantHasFeatureAsync(tenantId, featureKey)` - Does the tenant's plan include this feature?
- `UserHasPermissionAsync(userId, tenantId, permissionKey)` - Does the user have this permission via their tenant roles?
- `GetUserPermissionsAsync(userId, tenantId)` - Get all permission keys for a user
- `GetTenantFeaturesAsync(tenantId)` - Get all feature keys in the tenant's plan
- `GetFeatureForPermissionAsync(permissionKey)` - Resolve which feature owns a permission

---

## Annotated Endpoints

The following existing commands/queries have been annotated with `[RequirePermission]`:

| Command/Query | Permission | Feature |
|---|---|---|
| `CreateFacilityCommand` | `Facility.Create` | Settings |
| `UpdateFacilityCommand` | `Facility.Edit` | Settings |
| `DeleteFacilityCommand` | `Facility.Delete` | Settings |
| `GetFacilityQuery` | `Facility.View` | Settings |
| `GetFacilitiesWithPaginationQuery` | `Facility.View` | Settings |
| `CreateFirmCommand` | `Firm.Create` | Settings |
| `UpdateFirmCommand` | `Firm.Edit` | Settings |
| `DeleteFirmCommand` | `Firm.Delete` | Settings |
| `GetFirmQuery` | `Firm.View` | Settings |
| `GetFirmsWithPaginationQuery` | `Firm.View` | Settings |
| `CreateDeviceCommand` | `Device.Create` | Settings |
| `GetDeviceQuery` | `Device.View` | Settings |
| `GetDevicesWithPaginationQuery` | `Device.View` | Settings |
| `GetSolarEnergyProductionQuery` | `Solar.View` | Solar |
| `GetSolarPowerProductionQuery` | `Solar.View` | Solar |
| `GetInverterGraphQuery` | `Monitoring.View` | Monitoring |
| `GetPowerGeneralGraphQuery` | `Monitoring.View` | Monitoring |
| `GetDayAheadPricesQuery` | `Financials.View` | Financials |
| `GetLiveDayAheadEnergyPricesQuery` | `Financials.View` | Financials |
| `GetUsersWithPaginationQuery` | `UserManagement.View` | UserManagement |
| `GetUserQuery` | `UserManagement.View` | UserManagement |
| `UpdateUserCommand` | `UserManagement.Edit` | UserManagement |
| `DeleteUserCommand` | `UserManagement.Delete` | UserManagement |
| `AddUserRoleCommand` | `UserManagement.Edit` | UserManagement |
| `RemoveUserRoleCommand` | `UserManagement.Edit` | UserManagement |
| `GetBessUnitsQuery` | `Bess.View` | Bess |
| `GetBessUnitHeaderQuery` | `Bess.View` | Bess |
| `GetBessDeviceTreeQuery` | `Bess.View` | Bess |
| `GetBessSummaryQuery` | `Bess.View` | Bess |
| `GetBessHealthQuery` | `Bess.View` | Bess |
| `GetBessCurvesQuery` | `Bess.View` | Bess |
| `GetBessFaultFlagsQuery` | `Bess.View` | Bess |
| `GetBessFaultsQuery` | `Bess.View` | Bess |
| `GetBessSettingsQuery` | `Bess.View` | Bess |
| `GetPcsUnitsQuery` | `Pcs.View` | Pcs |
| `GetPcsHeaderQuery` | `Pcs.View` | Pcs |
| `GetPcsGeneralQuery` | `Pcs.View` | Pcs |
| `GetPcsFaultFlagsQuery` | `Pcs.View` | Pcs |
| `GetPcsCurvesQuery` | `Pcs.View` | Pcs |
| `GetPcsSchemaQuery` | `Pcs.View` | Pcs |

---

## API Endpoints

### Tenant Roles Management

| Method | Path | Description | Auth |
|--------|------|-------------|------|
| `GET` | `/api/TenantRoles` | List all roles for current tenant | Administrator, TenantAdmin |
| `GET` | `/api/TenantRoles/{id}` | Get single role with permissions | Administrator, TenantAdmin |
| `POST` | `/api/TenantRoles` | Create a new tenant role | Administrator, TenantAdmin |
| `PUT` | `/api/TenantRoles/{id}` | Update a tenant role | Administrator, TenantAdmin |
| `DELETE` | `/api/TenantRoles/{id}` | Delete a tenant role | Administrator, TenantAdmin |
| `PUT` | `/api/TenantRoles/{id}/permissions` | Bulk set permissions on a role | Administrator, TenantAdmin |
| `POST` | `/api/TenantRoles/{id}/users` | Assign role to a user | Administrator, TenantAdmin |
| `DELETE` | `/api/TenantRoles/{id}/users/{userId}` | Remove role from a user | Administrator, TenantAdmin |

**Endpoint file:** `src/Web/Endpoints/TenantRoles.cs`

### Permissions (Read-only)

| Method | Path | Description | Auth |
|--------|------|-------------|------|
| `GET` | `/api/Permissions` | Get all permissions grouped by feature | Authenticated |
| `GET` | `/api/Permissions/by-feature/{featureKey}` | Get permissions for a specific feature | Authenticated |

**Endpoint file:** `src/Web/Endpoints/Permissions.cs`

### UserInfo Enhancement

The `GET /api/Users/userinfo` endpoint now returns two additional fields:

```json
{
  "email": "user@example.com",
  "name": "John",
  "surname": "Doe",
  "roles": ["TenantAdmin"],
  "permissions": ["Facility.View", "Facility.Create", "Firm.View", ...],
  "planFeatures": ["Portfolio", "Dashboard", "Monitoring", "Solar", ...],
  "tenants": [...]
}
```

- **`permissions`**: List of permission keys the user has access to
  - Administrator: all permissions in the system
  - TenantAdmin: all permissions under features in their tenant's plan
  - User: only permissions from their assigned TenantRoles
- **`planFeatures`**: List of feature keys available in the tenant's plan

---

## Usage Examples

### Creating a Custom Role with Permissions

```http
POST /api/TenantRoles
Content-Type: application/json
Authorization: Bearer <token>

{
  "name": "Facility Manager",
  "description": "Can view and manage facilities and devices"
}
```

```http
PUT /api/TenantRoles/1/permissions
Content-Type: application/json

{
  "permissionIds": [19, 20, 21, 22, 23, 24]
}
```
*(Permission IDs for Facility.View, Facility.Create, Facility.Edit, Facility.Delete, Device.View, Device.Create)*

### Assigning a Role to a User

```http
POST /api/TenantRoles/1/users
Content-Type: application/json

{
  "userId": "abc-123-def"
}
```

### Adding `[RequirePermission]` to a New Command

```csharp
using Backend.Application.Common.Security;

[Authorize(Roles = "Administrator,TenantAdmin")]
[RequirePermission(Backend.Domain.Constants.Permissions.FacilityCreate)]
public record CreateFacilityCommand : IRequest<int>
{
    // ...
}
```

> **Important:** In the `src/Application/` layer, always use the fully qualified `Backend.Domain.Constants.Permissions.XYZ` instead of just `Permissions.XYZ` because the `Backend.Application.Permissions` namespace (query folder) collides with the constants class.

---

## Database Migrations

After pulling these changes, apply the migrations:

```bash
dotnet ef database update --project src/Infrastructure --startup-project src/Web
```

Relevant migrations:
- `AddAuthorizationEntities` — Features, Permissions, Plans, TenantRoles, UserTenantRoles
- `AddRefreshToken` — RefreshTokens table with indexes

The seed data will automatically create features, permissions, plans, and new roles (Operator, Viewer) on first startup.

---

## Files Created

### Authentication (JWT)

| File | Purpose |
|------|---------|
| `src/Infrastructure/Configurations/JwtSettings.cs` | JWT configuration POCO |
| `src/Domain/Entities/RefreshToken.cs` | Refresh token entity with rotation chain |
| `src/Application/Common/Interfaces/IJwtTokenService.cs` | JWT service interface |
| `src/Application/Common/Models/AuthModels.cs` | Login/Refresh DTOs and error models |
| `src/Infrastructure/Services/JwtTokenService.cs` | JWT generation, validation, refresh token generation |
| `src/Web/Endpoints/Auth.cs` | Login and RefreshToken endpoints |

### Authorization (Permissions)

| File | Purpose |
|------|---------|
| `src/Domain/Constants/Features.cs` | Feature key constants |
| `src/Domain/Constants/Permissions.cs` | Permission key constants |
| `src/Domain/Entities/Feature.cs` | Feature entity |
| `src/Domain/Entities/Permission.cs` | Permission entity |
| `src/Domain/Entities/PlanFeature.cs` | Plan-Feature junction entity |
| `src/Domain/Entities/TenantRole.cs` | Tenant-scoped role entity |
| `src/Domain/Entities/TenantRolePermission.cs` | Role-Permission junction entity |
| `src/Domain/Entities/UserTenantRole.cs` | User-Role assignment entity |
| `src/Infrastructure/Data/Configurations/FeatureConfiguration.cs` | EF config |
| `src/Infrastructure/Data/Configurations/PermissionConfiguration.cs` | EF config |
| `src/Infrastructure/Data/Configurations/PlanFeatureConfiguration.cs` | EF config |
| `src/Infrastructure/Data/Configurations/TenantRoleConfiguration.cs` | EF config |
| `src/Infrastructure/Data/Configurations/TenantRolePermissionConfiguration.cs` | EF config |
| `src/Infrastructure/Data/Configurations/UserTenantRoleConfiguration.cs` | EF config |
| `src/Application/Common/Interfaces/IPermissionService.cs` | Permission service interface |
| `src/Infrastructure/Services/PermissionService.cs` | Permission service implementation |
| `src/Application/Common/Security/RequirePermissionAttribute.cs` | New attribute |
| `src/Application/TenantRoles/**` | Commands, queries, DTOs, validators (~12 files) |
| `src/Application/Permissions/**` | Queries and DTOs (~3 files) |
| `src/Web/Endpoints/TenantRoles.cs` | REST endpoints |
| `src/Web/Endpoints/Permissions.cs` | REST endpoints |

## Files Modified

### Authentication (JWT)

| File | Change |
|------|--------|
| `src/Infrastructure/Infrastructure.csproj` | Added `Microsoft.AspNetCore.Authentication.JwtBearer` package |
| `src/Infrastructure/DependencyInjection.cs` | Replaced opaque tokens with JWT Bearer authentication, registered `IJwtTokenService` |
| `src/Web/DependencyInjection.cs` | Added rate limiter for login endpoint |
| `src/Web/Program.cs` | Added `UseRateLimiter()` to pipeline |
| `src/Web/appsettings.json` | Added `JwtSettings` section |
| `src/Web/appsettings.Development.json` | Added `JwtSettings` section |
| `src/Domain/Constants/Roles.cs` | Added `Operator` and `Viewer` roles |
| `src/Infrastructure/Data/ApplicationDbContext.cs` | Added `DbSet<RefreshToken>` + entity configuration |
| `src/Infrastructure/Data/ApplicationDbContextInitializer.cs` | Added seeding for Operator and Viewer roles |
| `src/Web/Endpoints/Users.cs` | Removed `MapIdentityApiOklidis` call (old login/refresh) |

### Authorization (Permissions)

| File | Change |
|------|--------|
| `src/Domain/Entities/Tenant.cs` | Added PlanId, Plan, TenantRoles |
| `src/Domain/Entities/Plan.cs` | Added PlanFeatures, Tenants |
| `src/Domain/Entities/ApplicationUser.cs` | Added UserTenantRoles |
| `src/Infrastructure/Data/Configurations/TenantConfiguration.cs` | Added Plan + TenantRoles relationships |
| `src/Infrastructure/Data/ApplicationDbContext.cs` | Added 6 DbSets + TenantRole query filter |
| `src/Application/Common/Interfaces/IApplicationDbContext.cs` | Added 6 DbSet properties |
| `src/Infrastructure/DependencyInjection.cs` | Registered PermissionService |
| `src/Application/Common/Behaviours/AuthorizationBehaviour.cs` | Added [RequirePermission] processing |
| `src/Infrastructure/Data/ApplicationDbContextInitializer.cs` | Added feature/permission/plan seed data |
| `src/Application/Users/Queries/GetUserInfo/UserInfoDto.cs` | Added Permissions, PlanFeatures fields |
| `src/Application/Users/Queries/GetUserInfo/GetUserInfo.cs` | Populates permissions and plan features |
| 25 existing command/query files | Added `[RequirePermission]` attribute |
