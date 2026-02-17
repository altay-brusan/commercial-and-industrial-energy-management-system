# Enhanced Authentication & Authorization System

## Overview

The DERMS backend implements a **two-layer authorization model** that combines plan-based feature gating with granular permission-based access control. This system sits on top of the existing ASP.NET Identity role-based authentication.

### Authorization Flow

```
Request arrives
  │
  ├─ [Authorize] check (role-based, existing system)
  │   └─ User must have one of the specified Identity roles
  │
  └─ [RequirePermission] check (new system)
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

### Feature List (15 features)

| Key | Name | Category |
|-----|------|----------|
| `Portfolio` | Portfolio | Core |
| `Dashboard` | Dashboard | Core |
| `Monitoring` | Monitoring | Core |
| `FacilityMap` | Facility Map | Core |
| `Solar` | Solar | Energy |
| `Bess` | BESS | Energy |
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

### Permission List (33 permissions)

| Permission Key | Name | Feature |
|---------------|------|---------|
| `Portfolio.View` | View Portfolio | Portfolio |
| `Dashboard.View` | View Dashboard | Dashboard |
| `Monitoring.View` | View Monitoring | Monitoring |
| `Solar.View` | View Solar | Solar |
| `Bess.View` | View BESS | Bess |
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

## Database Migration

After pulling these changes, run:

```bash
dotnet ef migrations add AddAuthorizationEntities --project src/Infrastructure --startup-project src/Web
dotnet ef database update --project src/Infrastructure --startup-project src/Web
```

The seed data will automatically create features, permissions, and plans on first startup.

---

## Files Created

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
