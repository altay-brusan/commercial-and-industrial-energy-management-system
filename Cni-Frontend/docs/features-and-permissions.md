# Features & Permissions

## Overview

The system uses a **two-layer authorization model**:
1. **Features**: Modules that can be enabled/disabled per tenant plan (16 total)
2. **Permissions**: Granular actions within features that can be assigned to roles (34 total)

## Features (16)

### Core

| Key | Name | Description |
|-----|------|-------------|
| `Portfolio` | Portfolio | Portfolio overview and management |
| `Dashboard` | Dashboard | Main dashboard and overview panels |
| `Monitoring` | Monitoring | Real-time system monitoring |
| `FacilityMap` | Facility Map | Interactive facility map view |

### Energy

| Key | Name | Description |
|-----|------|-------------|
| `Solar` | Solar | Solar energy monitoring and management |
| `Bess` | BESS | Battery energy storage system management |
| `Pcs` | PCS | Power conversion system management |
| `GridAndLoads` | Grid & Loads | Grid connection and load management |
| `Scada` | SCADA | Supervisory control and data acquisition |

### Intelligence

| Key | Name | Description |
|-----|------|-------------|
| `Analytics` | Analytics | Advanced analytics and data insights |
| `Financials` | Financials | Financial data and energy market prices |
| `Reporting` | Reporting | Report generation and export |

### Management

| Key | Name | Description |
|-----|------|-------------|
| `Operations` | Operations | Operational management and maintenance |
| `Settings` | Settings | System configuration and entity management |
| `UserManagement` | User Management | User accounts and access management |
| `Audit` | Audit | Audit logs and activity tracking |

---

## Permissions (34)

### Core Permissions

| Key | Name | Feature |
|-----|------|---------|
| `Portfolio.View` | View Portfolio | Portfolio |
| `Dashboard.View` | View Dashboard | Dashboard |
| `Monitoring.View` | View Monitoring | Monitoring |
| `FacilityMap.View` | View Facility Map | FacilityMap |

### Energy Permissions

| Key | Name | Feature |
|-----|------|---------|
| `Solar.View` | View Solar | Solar |
| `Bess.View` | View BESS | Bess |
| `Pcs.View` | View PCS | Pcs |
| `GridAndLoads.View` | View Grid & Loads | GridAndLoads |
| `Scada.View` | View SCADA | Scada |
| `Scada.Control` | Control SCADA | Scada |

### Intelligence Permissions

| Key | Name | Feature |
|-----|------|---------|
| `Analytics.View` | View Analytics | Analytics |
| `Financials.View` | View Financials | Financials |
| `Reporting.View` | View Reporting | Reporting |
| `Reporting.Export` | Export Reports | Reporting |

### Operations Permissions

| Key | Name | Feature |
|-----|------|---------|
| `Operations.View` | View Operations | Operations |
| `Operations.Manage` | Manage Operations | Operations |

### Settings Permissions (Firm, Facility, Device)

| Key | Name | Feature |
|-----|------|---------|
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

### User Management Permissions

| Key | Name | Feature |
|-----|------|---------|
| `UserManagement.View` | View Users | UserManagement |
| `UserManagement.Create` | Create Users | UserManagement |
| `UserManagement.Edit` | Edit Users | UserManagement |
| `UserManagement.Delete` | Delete Users | UserManagement |

### Audit Permissions

| Key | Name | Feature |
|-----|------|---------|
| `Audit.View` | View Audit Logs | Audit |

---

## System Roles

| Role | Type | Description |
|------|------|-------------|
| Administrator | System | Full access, bypasses all permission checks |
| Tenant Admin | System | All permissions within tenant's plan features |
| Operator | System | Monitoring, alarms, device control |
| User | System | Standard view permissions |
| Viewer | System | Read-only access |
| Field Technician | Custom | Operations, BESS, solar field access |
| Financial Analyst | Custom | Financials, analytics, reporting (inactive) |

## Authorization Flow

```
Request arrives
  │
  ├─ Is user Administrator? ──── YES → ALLOW (bypass all)
  │
  ├─ Does tenant's Plan include the feature? ──── NO → 403
  │
  ├─ Is user TenantAdmin? ──── YES → ALLOW (all permissions within plan)
  │
  └─ Does user's TenantRole include the permission? ──── NO → 403 → YES → ALLOW
```

## Mock Data

Mock features, permissions, and roles are defined in `lib/auth/mock-data.ts`. These are used as fallback when the backend is unavailable.

Frontend permission IDs use string format (`perm-1`, `perm-2`, ...). Backend uses numeric IDs (`1`, `2`, ...). The API response mapping handles this difference.
