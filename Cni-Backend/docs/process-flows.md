# CNI DERMS Backend — Process Flows & Data Model Documentation

## Table of Contents

1. [Data Model Overview](#1-data-model-overview)
2. [Entity Dependency Graph](#2-entity-dependency-graph)
3. [Process 1: Adding a New Site (Facility)](#3-process-1-adding-a-new-site-facility)
4. [Process 2: Adding a New Device](#4-process-2-adding-a-new-device)
5. [Process 3: Adding a New User](#5-process-3-adding-a-new-user)
6. [Process 4: Authorization Change](#6-process-4-authorization-change)
7. [Process 5: Authorization Removal](#7-process-5-authorization-removal)
8. [BESS Hierarchy — Deep Dive](#8-bess-hierarchy--deep-dive)
9. [Creation Order Summary](#9-creation-order-summary)
10. [Cascade Delete Behavior](#10-cascade-delete-behavior)

---

## 1. Data Model Overview

### Organizational Hierarchy

```
Tenant (top-level, multi-tenant root)
  ├── Firm (company under tenant)
  │     └── Facility (physical site)
  │           └── RTU (Remote Terminal Unit)
  │                 └── Device (IoT device)
  │                       └── Device (child, self-referential hierarchy)
  └── TenantRole (custom RBAC role)
        └── TenantRolePermission → Permission → Feature
```

### Entity Relationship Diagram

```mermaid
erDiagram
    Tenant ||--o{ Firm : "has many"
    Tenant ||--o{ TenantRole : "has many"
    Tenant }o--o| Plan : "subscribes to"

    Firm ||--o{ Facility : "has many"

    Facility ||--o{ Rtu : "has many"
    Facility ||--o{ DeviceLink : "has many"
    Facility ||--o{ FacilityTariff : "has many"

    Rtu ||--o{ Device : "has many"

    Device }o--|| DeviceType : "is of type"
    Device }o--o| DeviceSpecificationTemplate : "uses template"
    Device }o--o| LogicalDevice : "maps to"
    Device }o--o| Device : "parent-child"
    Device ||--o{ DeviceAlarm : "has many"

    DeviceType ||--o{ DeviceSpecificationTemplate : "has templates"

    LogicalDevice }o--o{ LogicalNode : "composed of"

    Plan ||--o{ PlanFeature : "includes"
    Feature ||--o{ PlanFeature : "included in"
    Feature ||--o{ Permission : "has many"

    TenantRole ||--o{ TenantRolePermission : "has many"
    Permission ||--o{ TenantRolePermission : "assigned to"

    ApplicationUser }o--o| Tenant : "belongs to"
    ApplicationUser ||--o{ UserTenantRole : "has roles"
    TenantRole ||--o{ UserTenantRole : "assigned to users"
    ApplicationUser ||--o{ UserFirmFacility : "has access"

    Tenant {
        Guid Id PK
        string Identifier UK
        string Name
        Guid PlanId FK
    }

    Firm {
        Guid Id PK
        Guid TenantId FK
        string Name
        string ContactEmail
        string TaxNumber
    }

    Facility {
        Guid Id PK
        Guid FirmId FK
        string Name
        string Country
        string Province
        string City
        string Address
        int EnergyCapacity
        double Longitude
        double Latitude
        enum DataSource
        bool AlmEnabled
    }

    Rtu {
        Guid Id PK
        Guid FacilityId FK
        string RtuSerialNumber UK
        string RtuModel
        string VpnIpAddress
        DateTimeOffset LastSeenUtc
    }

    Device {
        Guid Id PK
        Guid RtuId FK
        Guid DeviceTypeId FK
        Guid DeviceSpecificationTemplateId FK
        Guid LogicalDeviceId FK
        Guid ParentDeviceId FK
        string Name
        string Model
        string SerialNumber
        string Ports
        int HierarchyIndex
    }

    ApplicationUser {
        string Id PK
        Guid TenantId FK
        string UserName
        string Email
        string Name
        string Surname
    }

    TenantRole {
        Guid Id PK
        Guid TenantId FK
        string Name
        bool IsActive
    }

    Permission {
        Guid Id PK
        Guid FeatureId FK
        string Key UK
        string Name
    }
```

---

## 2. Entity Dependency Graph

Shows what must exist before each entity can be created.

```mermaid
graph TD
    subgraph "Global (No Dependencies)"
        Plan["Plan"]
        Feature["Feature"]
        DeviceType["DeviceType"]
        LogicalNode["LogicalNode"]
    end

    subgraph "Global (With Dependencies)"
        Permission["Permission"] --> Feature
        PlanFeature["PlanFeature"] --> Plan
        PlanFeature --> Feature
        LogicalDevice["LogicalDevice"] --> LogicalNode
        DeviceSpecTemplate["DeviceSpecificationTemplate"] --> DeviceType
    end

    subgraph "Tenant-Scoped"
        Tenant["Tenant"] -.->|optional| Plan
        Firm["Firm"] --> Tenant
        TenantRole["TenantRole"] --> Tenant
        TenantRolePerm["TenantRolePermission"] --> TenantRole
        TenantRolePerm --> Permission
        Facility["Facility"] --> Firm
        Rtu["RTU"] --> Facility
        Device["Device"] --> Rtu
        Device --> DeviceType
        Device -.->|optional| DeviceSpecTemplate
        Device -.->|optional| LogicalDevice
        Device -.->|optional, self-ref| Device
    end

    subgraph "User & Access"
        User["ApplicationUser"] -.->|optional| Tenant
        UserTenantRole["UserTenantRole"] --> User
        UserTenantRole --> TenantRole
        UserFirmFacility["UserFirmFacility"] --> User
        UserFirmFacility -.->|optional| Firm
        UserFirmFacility -.->|optional| Facility
    end

    style Tenant fill:#e1f5fe
    style Firm fill:#e1f5fe
    style Facility fill:#e1f5fe
    style Rtu fill:#e1f5fe
    style Device fill:#e1f5fe
    style User fill:#fff3e0
    style TenantRole fill:#fff3e0
```

### Required Creation Order

| Step | Entity | Required Dependencies |
|------|--------|----------------------|
| 1 | Plan, Feature, DeviceType, LogicalNode | None (global seed data) |
| 2 | Permission | Feature |
| 3 | PlanFeature | Plan + Feature |
| 4 | LogicalDevice | LogicalNode(s) |
| 5 | DeviceSpecificationTemplate | DeviceType |
| 6 | **Tenant** | None (optionally Plan) |
| 7 | Firm | Tenant |
| 8 | TenantRole | Tenant |
| 9 | TenantRolePermission | TenantRole + Permission |
| 10 | **Facility** | Firm |
| 11 | **RTU** | Facility |
| 12 | **Device** | RTU + DeviceType |
| 13 | **ApplicationUser** | None (optionally Tenant) |
| 14 | UserTenantRole | User + TenantRole |
| 15 | UserFirmFacility | User + Tenant (+ optional Firm/Facility) |

---

## 3. Process 1: Adding a New Site (Facility)

### Prerequisites

A **Facility** cannot be created in isolation. The following must exist first:

1. **Tenant** — the multi-tenant root
2. **Firm** — a company under the tenant (Facility belongs to a Firm)

### Data Constraints

| Field | Required | Notes |
|-------|----------|-------|
| FirmId | Yes | FK to existing Firm |
| Name | Yes | max 500 chars |
| Country | Yes | Full country name (e.g., "Türkiye") |
| Province | Yes | Province/state code |
| City | Yes | City name |
| Address | Yes | Physical address, max 1000 chars |
| EnergyCapacity | Yes | Integer capacity value |
| EnergyUnit | Yes | Enum: KWh (default), MWh, etc. |
| Longitude | Yes | GPS coordinate |
| Latitude | Yes | GPS coordinate |
| DataSource | No | Default: Rtu (0). Alternative: Huawei (1) |
| AlmEnabled | No | Default: false |

### Sequence Diagram

```mermaid
sequenceDiagram
    actor Admin as Administrator/TenantAdmin
    participant API as Web API
    participant Auth as AuthorizationBehaviour
    participant Val as ValidationBehaviour
    participant Handler as CreateFacilityHandler
    participant MT as MultiTenantService
    participant DB as ApplicationDbContext
    participant Events as DomainEvents

    Admin->>API: POST /api/Facilities
    API->>Auth: Check [Authorize] + [RequirePermission(Facility.Create)]

    alt User is Administrator
        Auth->>Auth: Bypass permission check
    else User is TenantAdmin
        Auth->>Auth: Check Plan has Facility feature
    else Regular User
        Auth->>Auth: Check TenantRole has Facility.Create permission
    end

    Auth->>Val: Pass to validation
    Val->>Val: Validate required fields (Name, FirmId, Country, etc.)

    alt Validation fails
        Val-->>Admin: 400 Bad Request + validation errors
    end

    Val->>Handler: Execute CreateFacilityCommand
    Handler->>MT: GetTenantIdForCreation(requestedTenantId?)

    alt Regular User
        MT-->>Handler: User's own TenantId
    else Administrator
        MT-->>Handler: X-Selected-Tenant-Id header or request body
    end

    Handler->>DB: Verify Firm exists and belongs to tenant
    Handler->>DB: Create Facility entity
    Handler->>Events: AddDomainEvent(FacilityCreatedEvent)
    Handler->>DB: SaveChangesAsync()
    Events->>Events: Dispatch FacilityCreatedEvent

    Handler-->>Admin: 201 Created + Facility Id
```

### What a Facility Can Exist Without

- RTUs and Devices (empty site is valid)
- Tariffs (can be assigned later)
- ALM predictions (opt-in)
- Topology JSON (optional layout)

### What Cannot Exist Without a Facility

- RTU (requires FacilityId)
- DeviceLink (requires FacilityId)
- FacilityTariff (requires FacilityId)

---

## 4. Process 2: Adding a New Device

### Prerequisites

A **Device** has the deepest dependency chain in the system:

```
Tenant → Firm → Facility → RTU → Device
```

ALL of these must exist before a device can be created. Additionally:
- **DeviceType** must exist (e.g., "EnergyStorage", "PCS", "Solar Panel")
- **DeviceSpecificationTemplate** (optional but recommended for composite devices)
- **LogicalDevice** (optional, IEC 61850 catalog reference)

### Three Device Creation Methods

#### Method 1: Direct Creation (`CreateDeviceCommand`)

Simple single-device creation.

| Field | Required | Notes |
|-------|----------|-------|
| RtuId | Yes | FK to existing RTU |
| DeviceTypeId | Yes | FK to existing DeviceType |
| Name | Yes | max 100 chars |
| Model | Yes | max 200 chars |
| SerialNumber | No | max 100 chars |
| Manufacturer | No | max 200 chars |
| Ports | No | JSON array, default "[]" |
| ParentDeviceId | No | FK to parent Device for hierarchy |
| DeviceSpecificationTemplateId | No | FK to template |
| LogicalDeviceId | No | FK to logical device catalog |
| Metadata | No | JSON blob |

#### Method 2: Composite Registration (`RegisterDeviceCompositeCommand`)

Creates a parent device with its entire child hierarchy from a template.

| Field | Required | Notes |
|-------|----------|-------|
| RtuId | Yes | FK to existing RTU |
| DeviceSpecificationTemplateId | Yes | Must have HierarchyTemplate |
| Name | Yes | Parent device name |
| SerialNumber | No | Parent serial number |

#### Method 3: Logical Device Registration (`RegisterDeviceFromLogicalDevice`)

Creates a device from the IEC 61850 LogicalDevice catalog.

| Field | Required | Notes |
|-------|----------|-------|
| RtuId | Yes | FK to existing RTU |
| LogicalDeviceId | Yes | FK to LogicalDevice with HierarchyTemplate |
| Name | Yes | Parent device name |

### Sequence Diagram — Composite Device Registration

```mermaid
sequenceDiagram
    actor Admin as Administrator/TenantAdmin
    participant API as Web API
    participant Auth as AuthorizationBehaviour
    participant Handler as RegisterDeviceCompositeHandler
    participant DB as ApplicationDbContext

    Admin->>API: POST /api/Devices/RegisterComposite
    API->>Auth: Check [Authorize] + [RequirePermission(Device.Create)]
    Auth->>Handler: Execute RegisterDeviceCompositeCommand

    Handler->>DB: Load DeviceSpecificationTemplate (includes HierarchyTemplate)
    Handler->>DB: Load RTU (verify exists)
    Handler->>DB: Load DeviceType from template

    Note over Handler: Create parent device
    Handler->>DB: Create Device (parent, HierarchyIndex=null)

    Note over Handler: Parse HierarchyTemplate JSON
    loop For each child in HierarchyTemplate
        Handler->>DB: Resolve child DeviceType
        Handler->>DB: Create Device (child)
        Note over Handler: Set ParentDeviceId = parent.Id
        Note over Handler: Set HierarchyIndex = 0-based position
        Note over Handler: Set DeviceSpecificationTemplateId from template
    end

    Handler->>DB: SaveChangesAsync()
    Handler-->>Admin: 201 Created + Parent Device Id

    Note over Admin: Child devices accessible via<br/>GET /api/Devices/{parentId}/tree
```

### Device Hierarchy Example (BESS Container)

```
Container (DeviceType: EnergyStorage)
  ├── Rack 1 (DeviceType: EnergyStorage, HierarchyIndex: 0)
  ├── Rack 2 (DeviceType: EnergyStorage, HierarchyIndex: 1)
  ├── PCS 1 (DeviceType: PCS, HierarchyIndex: 0)
  ├── Chiller 1 (DeviceType: Chiller, HierarchyIndex: 0)
  └── Smart Meter (DeviceType: SmartMeter, HierarchyIndex: 0)
```

### What a Device Can Exist Without

- Child devices (leaf device is valid)
- DeviceSpecificationTemplate (standalone device)
- LogicalDevice (legacy path)
- Telemetry data (device exists before data arrives)
- Alarms (created dynamically from telemetry)

### What Cannot Exist Without a Device

- DeviceAlarm (requires DeviceId)
- Child Device (requires ParentDeviceId)
- DeviceBatteryData/DevicePcsData/DeviceSolarData/DeviceSmartMeterData (require DeviceId + RtuId)

---

## 5. Process 3: Adding a New User

### Prerequisites

- **Tenant** must exist if the user will be scoped to a tenant
- System administrators can exist without a tenant (TenantId = null)

### User Creation Flow

```mermaid
sequenceDiagram
    actor SysAdmin as System Administrator
    participant API as Identity Service
    participant UM as UserManager
    participant DB as ApplicationDbContext

    SysAdmin->>API: CreateUserAsync(userName, password, tenantId)
    API->>API: Create ApplicationUser entity
    Note over API: UserName = email<br/>Email = email<br/>TenantId = tenantId

    API->>UM: CreateAsync(user, password)
    UM->>UM: Validate password policy
    UM->>UM: Hash password
    UM->>DB: Insert into AspNetUsers

    alt Success
        API-->>SysAdmin: (Success, UserId)
    else Password Policy Failure
        API-->>SysAdmin: (Failure, errors)
    end

    Note over SysAdmin: After user creation, assign roles and access

    SysAdmin->>API: AddToRoleAsync(user, "Tenant.Admin")
    API->>DB: Insert into AspNetUserRoles

    SysAdmin->>API: Assign TenantRole
    API->>DB: Insert UserTenantRole

    SysAdmin->>API: Assign Facility Access
    API->>DB: Insert UserFirmFacility
```

### Post-Creation Setup Required

After creating a user, the following must be configured for the user to be functional:

| Step | Entity | Purpose | Required? |
|------|--------|---------|-----------|
| 1 | Identity Role | Base role (Administrator, Tenant.Admin, User, etc.) | Yes |
| 2 | TenantRole → UserTenantRole | Feature-based permissions | Yes (for non-admins) |
| 3 | UserFirmFacility | Granular Firm/Facility access | Depends on role |

### User Types and Their Access Patterns

```mermaid
graph LR
    subgraph "System Administrator"
        A[TenantId = null]
        A --> A1[All tenants via header]
        A --> A2[Bypass all permissions]
    end

    subgraph "Tenant Admin"
        B[TenantId = X]
        B --> B1[Own tenant only]
        B --> B2[All permissions within Plan]
    end

    subgraph "Regular User"
        C[TenantId = X]
        C --> C1[Own tenant only]
        C --> C2[Only assigned permissions]
        C --> C3[Only assigned Firms/Facilities]
    end
```

---

## 6. Process 4: Authorization Change

Authorization changes involve modifying a user's access level. There are multiple layers:

### Layer 1: Identity Role Change

```mermaid
sequenceDiagram
    actor Admin as Administrator/TenantAdmin
    participant API as POST /api/Users/{id}/roles
    participant Auth as AuthorizationBehaviour
    participant Handler as AddUserRoleHandler
    participant UM as UserManager
    participant DB as Database

    Admin->>API: AddUserRoleCommand(UserId, RoleName)
    API->>Auth: Check [RequirePermission(UserManagement.Edit)]

    Handler->>DB: Find user by UserId
    Handler->>Handler: Validate role exists

    alt Assigning "Administrator" role
        Handler->>Handler: Check caller is super admin
        alt Caller is NOT super admin
            Handler-->>Admin: 403 Forbidden
        end
    end

    Handler->>UM: IsInRoleAsync(user, roleName)
    alt Already has role
        Handler-->>Admin: 400 "User already has this role"
    end

    Handler->>UM: AddToRoleAsync(user, roleName)
    Handler-->>Admin: 200 OK
```

### Layer 2: Permission Role Change (TenantRole Assignment)

```mermaid
sequenceDiagram
    actor Admin as TenantAdmin
    participant API as Web API
    participant Handler as Command Handler
    participant DB as Database

    Note over Admin: Step 1 — Create or select TenantRole
    Admin->>API: Create TenantRole (Name, TenantId)
    API->>DB: Insert TenantRole

    Note over Admin: Step 2 — Add permissions to role
    Admin->>API: Add TenantRolePermission(TenantRoleId, PermissionId)
    API->>DB: Insert TenantRolePermission

    Note over Admin: Step 3 — Assign user to role
    Admin->>API: Create UserTenantRole(UserId, TenantRoleId)
    API->>DB: Insert UserTenantRole

    Note over DB: User now has permissions:<br/>User → UserTenantRole → TenantRole →<br/>TenantRolePermission → Permission
```

### Layer 3: Facility Access Change

```mermaid
sequenceDiagram
    actor Admin as TenantAdmin
    participant API as Web API
    participant DB as Database

    Note over Admin: Grant access to specific Firm + Facility
    Admin->>API: Create UserFirmFacility
    Note over API: TenantId (required)<br/>UserId (required)<br/>FirmId (optional — all firms)<br/>FacilityId (optional — all facilities in firm)

    API->>DB: Insert UserFirmFacility

    Note over DB: Access patterns:<br/>FirmId=X, FacilityId=null → All facilities in Firm X<br/>FirmId=X, FacilityId=Y → Only Facility Y<br/>FirmId=null, FacilityId=null → Tenant-wide access
```

### Authorization Check Flow (Runtime)

```mermaid
flowchart TD
    A[Request arrives] --> B{Has JWT token?}
    B -->|No| C[401 Unauthorized]
    B -->|Yes| D[MultiTenantMiddleware extracts tenant_id]
    D --> E{Command has Authorize attr?}
    E -->|No| F[Execute handler]
    E -->|Yes| G{User has required Identity Role?}
    G -->|No| H[403 Forbidden]
    G -->|Yes| I{Command has RequirePermission?}
    I -->|No| F
    I -->|Yes| J{User is Administrator?}
    J -->|Yes| F
    J -->|No| K{Tenant Plan has Feature?}
    K -->|No| H
    K -->|Yes| L{User is TenantAdmin?}
    L -->|Yes| F
    L -->|No| M{User's TenantRole has Permission?}
    M -->|Yes| F
    M -->|No| H
```

---

## 7. Process 5: Authorization Removal

### Removing an Identity Role

```mermaid
sequenceDiagram
    actor Admin as Administrator/TenantAdmin
    participant API as DELETE /api/Users/{id}/roles/{roleName}
    participant Handler as RemoveUserRoleHandler
    participant UM as UserManager

    Admin->>API: RemoveUserRoleCommand(UserId, RoleName)
    Handler->>UM: IsInRoleAsync(user, roleName)

    alt User does not have role
        Handler-->>Admin: 400 "User does not have this role"
    end

    Handler->>UM: RemoveFromRoleAsync(user, roleName)
    Handler-->>Admin: 200 OK

    Note over Admin: User immediately loses role-based access<br/>Next JWT will not include this role
```

### Removing Permission-Based Access

```mermaid
sequenceDiagram
    actor Admin as TenantAdmin
    participant DB as Database

    Note over Admin: Option A — Remove user from TenantRole
    Admin->>DB: DELETE UserTenantRole WHERE UserId=X AND TenantRoleId=Y
    Note over DB: User loses all permissions from that role

    Note over Admin: Option B — Remove permission from role
    Admin->>DB: DELETE TenantRolePermission WHERE TenantRoleId=Y AND PermissionId=Z
    Note over DB: ALL users with this role lose this permission

    Note over Admin: Option C — Deactivate entire role
    Admin->>DB: UPDATE TenantRole SET IsActive=false WHERE Id=Y
    Note over DB: All users with this role lose all permissions

    Note over Admin: Option D — Remove facility access
    Admin->>DB: DELETE UserFirmFacility WHERE UserId=X AND FirmId=Y
    Note over DB: User loses access to Firm Y and its facilities
```

### Cascade Effects of Authorization Removal

```mermaid
flowchart TD
    subgraph "Delete Tenant"
        DT[Delete Tenant] --> DF[Cascade: Delete all Firms]
        DF --> DFac[Cascade: Delete all Facilities]
        DFac --> DR[Cascade: Delete all RTUs]
        DR --> DD[Cascade: Delete all Devices]
        DT --> DTR[Cascade: Delete all TenantRoles]
        DTR --> DTRP[Cascade: Delete TenantRolePermissions]
        DTR --> DUTR[Cascade: Delete UserTenantRoles]
    end

    subgraph "Delete User"
        DU[Delete User] --> DUTR2[Cascade: Delete UserTenantRoles]
        DU --> DUFF[Cascade: Delete UserFirmFacilities]
    end

    subgraph "Delete TenantRole"
        DTR2[Delete TenantRole] --> DTRP2[Cascade: Delete TenantRolePermissions]
        DTR2 --> DUTR3[Cascade: Delete UserTenantRoles]
        Note1[All users with this role lose permissions]
    end

    style DT fill:#ffcdd2
    style DU fill:#ffcdd2
    style DTR2 fill:#ffcdd2
```

---

## 8. BESS Hierarchy — Deep Dive

### Overview

When a user navigates to **Battery > Battery Package** and opens the **Hierarchy** tab, the frontend calls:

```
GET /api/v1/BatteryStorage/Tree/{facilityId}
```

This returns a recursive tree of all BESS devices in that facility. Clicking any node calls:

```
GET /api/v1/BatteryStorage/Units/{unitId}/Header
```

Which returns level-aware header data (breadcrumbs, telemetry, available tabs).

### Physical Hierarchy Structure (Ottomotive Reference)

The Ottomotive BESS Container is the reference device. Its hierarchy:

```
┌─────────────────────────────────────────────────────────────────┐
│  BESS Container (DeviceType: EnergyStorage)                     │
│  ├── Rack 1  (DeviceType: EnergyStorage.Rack, HierarchyIndex: 0)│
│  ├── Rack 2  (DeviceType: EnergyStorage.Rack, HierarchyIndex: 1)│
│  ├── ...                                                         │
│  ├── Rack 12 (DeviceType: EnergyStorage.Rack, HierarchyIndex: 11)│
│  ├── PCS_001 (DeviceType: PCS,                HierarchyIndex: 0)│
│  └── HVAC_001(DeviceType: EnergyStorage.TMS,  HierarchyIndex: 0)│
└─────────────────────────────────────────────────────────────────┘
```

The **LogicalDevice** model defines deeper nesting (IEC 61850-inspired):

```
BatteryContainer
  └── BatteryRack (×4 default)
        └── BatteryPack (×8 per rack)
              └── (leaf — no further children)
```

### Full DeviceType Hierarchy

```
DeviceTypeNames (Domain/Constants/DeviceTypeNames.cs)
├── EnergyStorage              → display: "unit"     (root container)
├── EnergyStorage.Rack         → display: "rack"
├── EnergyStorage.Pack         → display: "pack"
├── EnergyStorage.Module       → display: "module"
├── EnergyStorage.Battery      → display: "battery"  (cell-level)
├── EnergyStorage.Controller   → display: "esc"      (energy storage controller)
├── EnergyStorage.TMS          → display: "hvac"     (thermal management)
├── PCS                        → display: "pcs"
├── Solar                      → display: "solar"
└── Smart Meter                → display: "meter"
```

### How the Tree is Built (Backend Flow)

```mermaid
sequenceDiagram
    participant FE as Frontend
    participant API as GET /Tree/{facilityId}
    participant Handler as GetBessDeviceTreeHandler
    participant DB as Database
    participant Status as DeviceStatusService

    FE->>API: Request tree for facility
    API->>Handler: GetBessDeviceTreeQuery

    Handler->>DB: Find root devices WHERE<br/>DeviceType.Name == "EnergyStorage"<br/>OR LogicalDevice.Category == "BESS"
    Handler->>DB: Load ALL devices under same RTUs<br/>(includes Racks, Packs, PCS, HVAC, etc.)
    Handler->>Status: GetStatusBatchAsync(all device IDs)
    Handler->>DB: Load active alarm counts per device

    Note over Handler: Build parent→children map<br/>Key: ParentDeviceId<br/>Sort: HierarchyIndex ASC, Name ASC

    loop For each root device
        Handler->>Handler: BuildBessTreeNode(root, childrenMap)
        Note over Handler: Recursive: each node includes<br/>Id, Name, Type (display), Status,<br/>AlarmCount (own + descendants),<br/>Children[]
    end

    Handler-->>FE: BessDeviceTreeResponseDto { Tree: [...] }
```

### Display Type Mapping (`BessDisplayTypeMapper`)

The mapper at `GetBessDeviceTree.cs:120-136` converts DeviceType names to frontend display types:

| DeviceType Name | Display Type | Tree Icon | Notes |
|----------------|-------------|-----------|-------|
| `EnergyStorage` | `unit` | Container | Root level, has all tabs |
| `EnergyStorage.Rack` | `rack` | Rack | Sub-unit, summary + alarms tabs |
| `EnergyStorage.Pack` | `pack` | Pack | Sub-unit, summary + alarms tabs |
| `EnergyStorage.Module` | `module` | Module | Has **cells** tab |
| `EnergyStorage.Battery` | `battery` | Cell | Leaf level |
| `EnergyStorage.Controller` | `esc` | Controller | Settings + alarms tabs only |
| `EnergyStorage.TMS` | `hvac` | HVAC | Settings + alarms tabs only |
| `PCS` | `pcs` | Inverter | Shown in connectedDevices, not tree |
| Unknown type | `null` | — | **Filtered out of tree** |

### Level-Aware Header (`GetBessUnitHeader`)

When a user clicks any node in the hierarchy, the header endpoint returns level-specific data:

```mermaid
flowchart TD
    A[GET /Units/{unitId}/Header] --> B{What display type?}

    B -->|"unit"| C[Root Container Header]
    C --> C1[SoC, SoH, Power, Voltage, Current]
    C --> C2[Operating Mode badge]
    C --> C3[Connected Devices: PCS, HVAC, ESC]
    C --> C4[Tabs: Summary, Health, Curves,<br/>Hierarchy, FaultFlags, Faults]

    B -->|"rack" / "pack"| D[Sub-Unit Header]
    D --> D1[Walks UP parent chain to find:<br/>UnitId/Name, RackId/Name, PackId/Name]
    D --> D2[Inherits telemetry from root container]
    D --> D3[Tabs: Summary, Alarms]

    B -->|"module"| E[Module Header]
    E --> E1[Same as sub-unit]
    E --> E2[Tabs: Summary, **Cells**, Alarms]

    B -->|"hvac"| F[HVAC Sections Header]
    F --> F1[Device Info section]
    F --> F2[Chiller State: coolant temps, compressor,<br/>pump, fan, heater statuses]
    F --> F3[Setpoints section]
    F --> F4[Tabs: Settings, Alarms]

    B -->|"esc"| G[ESC Sections Header]
    G --> G1[Device Info section]
    G --> G2[Live Data: DC voltages, current,<br/>power, efficiency, temp]
    G --> G3[Tabs: Settings, Alarms]
```

### Breadcrumb Resolution (Parent Chain Walk)

For any non-root node, the handler walks up the `ParentDeviceId` chain to build breadcrumbs:

```
User clicks "Pack 3" inside "Rack 2" inside "Container A"

Header response includes:
  UnitId:   Container_A.Id    UnitName:   "Container A"
  RackId:   Rack_2.Id         RackName:   "Rack 2"
  PackId:   null               PackName:   null        (current level, not ancestor)
  HierarchyNumber: 3          (HierarchyIndex + 1)

Frontend renders: Container A > Rack 2 > Pack 3
```

Code path (`GetBessUnitHeader.cs:70-95`):
```csharp
// Walk UP parent chain
while (ancestor.ParentDeviceId.HasValue)
{
    var parentType = MapToDisplayType(parent.DeviceType?.Name);
    switch (parentType)
    {
        case "unit": unitId = parent.Id; unitName = parent.Name; break;
        case "rack": rackId = parent.Id; rackName = parent.Name; break;
        case "pack": packId = parent.Id; packName = parent.Name; break;
    }
    ancestor = parent;
}
```

### Telemetry Resolution for Hierarchy Levels

**Critical concept:** Telemetry data (SoC, voltage, current, etc.) is typically ingested at the **root container** level. Sub-devices (Rack, Pack, Module) don't receive their own telemetry from EMQX — they inherit from the root.

```mermaid
flowchart LR
    subgraph "Telemetry Source"
        EMQX[EMQX Broker] -->|"Kind: Battery.Energy<br/>DeviceId: Container"| Ingest[Ingest Endpoint]
        Ingest --> BattData[DeviceBatteryData table]
    end

    subgraph "Query Resolution"
        Root[Root Container] -->|"Direct lookup"| BattData
        Rack[Rack node] -->|"telemetryDeviceId = rootId"| BattData
        Pack[Pack node] -->|"telemetryDeviceId = rootId"| BattData
    end
```

In `GetBessUnitHeader.cs:140-142`:
```csharp
// For sub-devices, telemetry is stored under the root container
var telemetryDeviceId = !isRootUnit && unitId.HasValue
    ? unitId.Value   // ← uses ROOT device's telemetry
    : device.Id;
```

### Parameter Resolution & Derivation

The **ParameterResolver** provides computed values when telemetry doesn't directly supply them.

#### Ottomotive BESS — 23 Parameters (Seed Data)

| Parameter | Type | Source / Formula | Notes |
|-----------|------|-----------------|-------|
| `soc` | Source | `Battery.Energy → soc` | Direct from telemetry |
| `soh` | Source | `Battery.Soh → soh` | Direct from telemetry |
| `voltage` | Source | `Battery.Energy → voltage` | Direct (V) |
| `current` | Source | `Battery.Energy → current` | Direct (A) |
| **`power`** | **Formula** | `round(multiply(voltage, current, 0.001), 1)` | **Derived: V × A / 1000 = kW** |
| **`powerDirection`** | **Formula** | `cond(lt(current, 0), 'discharging', cond(gt(current, 0), 'charging', 'idle'))` | **Derived from current sign** |
| `operatingMode` | Source | `Battery.Status → state` | Direct |
| `cellVoltageMax` | Source | `Battery.Energy → cellMaxVoltage` | Direct (V) |
| `cellVoltageMin` | Source | `Battery.Energy → cellMinVoltage` | Direct (V) |
| `cellVoltageAvg` | Source | `Battery.Energy → cellAvgVoltage` | Direct (V) |
| **`voltageSpread`** | **Formula** | `subtract(cellVoltageMax, cellVoltageMin)` | **Derived: max - min** |
| `tempMax` | Source | `Battery.Energy → maxTemp` | Direct (°C) |
| `tempMin` | Source | `Battery.Energy → minTemp` | Direct (°C) |
| **`tempAvg`** | **Formula** | `round(avg(tempMin, tempMax), 1)` | **Derived: average** |
| `tempAmbient` | Source | `Battery.Energy → ambientTemp` | Direct (°C) |
| `cycles` | Source | `Battery.Health → cycleCount` | Direct |
| **`cycleLifePct`** | **Formula** | `round(multiply(divide(cycles, 6000), 100), 1)` | **Derived: cycles/designedLife × 100** |
| `availableEnergy` | Source | `Battery.Energy → availableEnergy` | Direct (kWh) |
| `pcsActivePower` | Source | `Pcs.PowerDistribution → totalActivePower` | From child PCS device |
| `pcsReactivePower` | Source | `Pcs.PowerDistribution → totalReactivePower` | From child PCS device |
| `pcsDcPower` | Source | `Pcs.DcMeasurement → dcPower` | From child PCS device |
| `pcsEfficiency` | Source | `Pcs.AcMeasurement → efficiency` | From child PCS device |
| `gridFrequency` | Source | `Pcs.AcMeasurement → gridFrequency` | From child PCS device |

#### Derivation Flow

```mermaid
flowchart TD
    subgraph "Raw Telemetry (DeviceBatteryData)"
        V[voltage: 798.5 V]
        I[current: -125.3 A]
        MaxT[maxTemp: 32.1 °C]
        MinT[minTemp: 28.4 °C]
        CellMax[cellMaxVoltage: 3.42 V]
        CellMin[cellMinVoltage: 3.38 V]
        Cycles[cycleCount: 1250]
    end

    subgraph "Formula Evaluation"
        V & I --> P["power = round(V × I × 0.001, 1)<br/>= round(798.5 × -125.3 × 0.001, 1)<br/>= **-100.1 kW**"]
        I --> PD["powerDirection = cond(lt(-125.3, 0),<br/>'discharging', ...)<br/>= **'discharging'**"]
        MaxT & MinT --> TA["tempAvg = round(avg(32.1, 28.4), 1)<br/>= **30.3 °C**"]
        CellMax & CellMin --> VS["voltageSpread = 3.42 - 3.38<br/>= **0.04 V**"]
        Cycles --> CLP["cycleLifePct = round(1250/6000 × 100, 1)<br/>= **20.8 %**"]
    end

    subgraph "Fallback (No Parameter Map)"
        V2[voltage] --> P2["power = V × I / 1000<br/>(hardcoded in handler)"]
        I2[current] --> P2
    end
```

### Devices That Don't Provide Hierarchy Info

For devices **without a `DeviceSpecificationTemplate`** or **without a `ParameterMapTemplate`**, the system uses a 3-tier fallback:

```mermaid
flowchart TD
    A[Query Handler needs SoC] --> B{ParameterResolver available?}
    B -->|Yes| C[resolver.ResolveNumber 'soc']
    C --> D{Parameter found?}
    D -->|Yes| E[Return resolved value]
    D -->|No, formula failed| F[Return null]

    B -->|No template| G[Hardcoded extraction]
    G --> H["ExtractDouble(payload, 'soc')"]
    H --> I{Found in JSON?}
    I -->|Yes| E
    I -->|No| J[Return null — shown as '--' in UI]
```

#### Specific Fallback Patterns in GetBessUnits/GetBessUnitHeader:

```csharp
// Pattern: resolver first ?? hardcoded fallback
Soc  = resolver?.ResolveNumber(ParameterKeys.Soc)
       ?? ExtractDouble(battPayload, "soc");

Power = resolver?.ResolveNumber(ParameterKeys.Power)
        ?? (voltage * current / 1000.0);  // manual derivation

Soh  = resolver?.ResolveNumber(ParameterKeys.Soh)
       ?? ExtractDouble(battPayload, "soh");
```

### Hierarchy Creation (How it Gets Built)

```mermaid
sequenceDiagram
    actor Admin as Administrator
    participant API as POST /api/Devices/RegisterComposite
    participant Handler as RegisterDeviceCompositeHandler
    participant DB as Database

    Admin->>API: RegisterDeviceCompositeCommand
    Note over API: RtuId, TemplateId (Ottomotive),<br/>Name: "Container A",<br/>SerialNumber: "OTT-001"

    Handler->>DB: Load DeviceSpecificationTemplate
    Note over Handler: HierarchyTemplate:<br/>12× EnergyStorage.Rack<br/>1× PCS<br/>1× EnergyStorage.TMS

    Handler->>DB: Load all DeviceTypes needed
    Note over Handler: EnergyStorage, EnergyStorage.Rack,<br/>PCS, EnergyStorage.TMS

    Handler->>Handler: Create root: Container A (EnergyStorage)
    Note over Handler: DeviceSpecificationTemplateId = template.Id<br/>Metadata = template.MetadataTemplate

    loop i = 1 to 12 (Racks)
        Handler->>Handler: Create "Rack {i}" (EnergyStorage.Rack)
        Note over Handler: ParentDeviceId = root.Id<br/>HierarchyIndex = i-1<br/>SerialNumber = "OTT-001-Rack-{i}"
    end

    Handler->>Handler: Create "PCS_001" (PCS)
    Note over Handler: ParentDeviceId = root.Id<br/>HierarchyIndex = 0

    Handler->>Handler: Create "HVAC_001" (EnergyStorage.TMS)
    Note over Handler: ParentDeviceId = root.Id<br/>HierarchyIndex = 0

    Handler->>DB: SaveChangesAsync() — bulk insert all 15 devices
    Handler-->>Admin: root.Id
```

### Available Tabs per Hierarchy Level

| Level | Tabs | Data Source |
|-------|------|-------------|
| **unit** (root) | Summary, Health, Curves, Hierarchy, FaultFlags, Faults | Own telemetry + child PCS |
| **rack** | Summary, Alarms | Inherited from root container |
| **pack** | Summary, Alarms | Inherited from root container |
| **module** | Summary, **Cells**, Alarms | Inherited from root container |
| **battery** | Summary, Alarms | Inherited from root container |
| **esc** | Settings, Alarms | Sections-based (device info + live data) |
| **hvac** | Settings, Alarms | Sections-based (chiller state + setpoints) |

### Connected Devices (Non-Hierarchy Siblings)

At the root level, the header also returns **connectedDevices** — child devices that are NOT part of the battery hierarchy (PCS, HVAC, ESC):

```json
{
  "connectedDevices": {
    "pcs": [{ "id": "...", "name": "PCS_001" }],
    "hvac": [{ "id": "...", "name": "HVAC_001" }]
  }
}
```

These are filtered OUT of the main tree (`rack`, `pack`, `module`, `battery` types stay in tree), but shown as sidebar links in the UI.

### JSON Example: Tree Response

```json
{
  "tree": [
    {
      "id": "container-guid",
      "name": "Container A",
      "type": "unit",
      "status": "normal",
      "parentDeviceId": null,
      "alarmCount": 3,
      "children": [
        {
          "id": "rack-1-guid",
          "name": "Rack 1",
          "type": "rack",
          "status": "normal",
          "parentDeviceId": "container-guid",
          "alarmCount": 1,
          "children": []
        },
        {
          "id": "rack-2-guid",
          "name": "Rack 2",
          "type": "rack",
          "status": "warning",
          "parentDeviceId": "container-guid",
          "alarmCount": 2,
          "children": []
        }
      ]
    }
  ]
}
```

> **Note:** In the Ottomotive template, Racks have no children defined in `HierarchyTemplate`. The LogicalDevice model (BatteryRack → BatteryPack nesting) supports deeper hierarchy but is not yet wired into the Ottomotive seed data. To get Pack/Module levels, either:
> 1. Add nested `children` to the Ottomotive `HierarchyTemplate`, or
> 2. Use `RegisterDeviceFromLogicalDevice` which follows `LogicalDevice.HierarchyTemplate` recursively.

---

## 9. Creation Order Summary

### Minimum Steps to Set Up a Fully Functional Site with Users

```mermaid
flowchart LR
    subgraph "Step 1: Organization"
        T[Create Tenant] --> F[Create Firm]
    end

    subgraph "Step 2: Site"
        F --> Fac[Create Facility]
        Fac --> R[Create RTU]
    end

    subgraph "Step 3: Devices"
        R --> D[Register Device/Composite]
    end

    subgraph "Step 4: Users"
        T --> TR[Create TenantRole]
        TR --> TRP[Assign Permissions to Role]
        T --> U[Create User]
        U --> UR[Assign Identity Role]
        U --> UTR[Assign TenantRole to User]
        U --> UFF[Assign Firm/Facility Access]
    end
```

### Cannot Create Without

| Entity | Requires |
|--------|----------|
| Firm | Tenant |
| Facility | Firm (→ Tenant) |
| RTU | Facility (→ Firm → Tenant) |
| Device | RTU + DeviceType (→ Facility → Firm → Tenant) |
| TenantRole | Tenant |
| TenantRolePermission | TenantRole + Permission (→ Feature) |
| UserTenantRole | User + TenantRole |
| UserFirmFacility | User + Tenant |

### Can Create Independently

| Entity | Notes |
|--------|-------|
| Tenant | Top-level, no dependencies |
| Plan | Global subscription tier |
| Feature | Global feature module |
| DeviceType | Global device catalog |
| LogicalDevice/Node | IEC 61850 catalog |
| ApplicationUser | Can exist without tenant (admin) |

---

## 10. Cascade Delete Behavior

| Deleting... | Cascades To | Blocked By |
|-------------|-------------|------------|
| Tenant | Firms, TenantRoles | — |
| Firm | Facilities | — |
| Facility | RTUs, DeviceLinks, FacilityTariffs | — |
| RTU | Devices | — |
| Device | Alarms | Child Devices (Restrict) |
| DeviceType | — | Devices using it (Restrict) |
| LogicalDevice | — | Devices using it (Restrict) |
| DeviceSpecTemplate | Devices set FK to null (SetNull) | — |
| Feature | Permissions, PlanFeatures | — |
| Permission | TenantRolePermissions | — |
| Plan | PlanFeatures | — |
| TenantRole | TenantRolePermissions, UserTenantRoles | — |
| User | UserTenantRoles, UserFirmFacilities | — |

**Key Restriction:** You cannot delete a parent Device if it still has child devices. You must delete children first (bottom-up).
