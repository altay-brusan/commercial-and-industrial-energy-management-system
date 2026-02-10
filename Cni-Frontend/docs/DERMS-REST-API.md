# DERMS REST API Specification

> Complete API specification for the DERMS (Distributed Energy Resource Management System) backend.
> All data contracts are derived from the frontend UI kit's TypeScript interfaces and mock data.

---

## Table of Contents

1. [Introduction & Conventions](#1-introduction--conventions)
2. [Authentication](#2-authentication)
3. [Facilities](#3-facilities)
4. [Dashboard](#4-dashboard)
5. [Alarms](#5-alarms)
6. [Notifications](#6-notifications)
7. [Maintenance](#7-maintenance)
8. [BESS Monitoring](#8-bess-monitoring)
9. [Solar Inverters](#9-solar-inverters)
10. [Grid Loads](#10-grid-loads)
11. [Wind Turbines](#11-wind-turbines)
12. [Consumption](#12-consumption)
13. [Financials](#13-financials)
14. [Shared Types Reference](#14-shared-types-reference)

---

## 1. Introduction & Conventions

### Base URL

```
https://api.derms.example.com/v1
```

### Authentication

All endpoints (except `/auth/*`) require a Bearer token:

```
Authorization: Bearer <jwt_token>
```

### Facility Scoping

All endpoints (except `/auth/*` and `/facilities`) require a facility header:

```
X-Facility-Id: <facility_uuid>
```

The server MUST filter all returned data by the specified facility.

### Pagination

List endpoints support pagination via query parameters:

| Param      | Type   | Default | Description                      |
|------------|--------|---------|----------------------------------|
| `page`     | number | 1       | Page number (1-based)            |
| `pageSize` | number | 20      | Items per page (max 100)         |

Paginated responses include:

```typescript
interface PaginatedResponse<T> {
  data: T[];
  pagination: {
    page: number;
    pageSize: number;
    totalItems: number;
    totalPages: number;
  };
}
```

### Sorting

| Param     | Type   | Description                                 |
|-----------|--------|---------------------------------------------|
| `sortBy`  | string | Field name to sort by (endpoint-specific)   |
| `sortDir` | string | `asc` or `desc` (default: `desc`)           |

### Filtering

- Array filters use comma-separated values: `?severity=critical,high`
- Date ranges use ISO 8601: `?from=2026-01-01T00:00:00Z&to=2026-02-01T00:00:00Z`
- String search uses `?search=` prefix matching

### Date Format

All dates use **ISO 8601**: `2026-02-10T14:30:00Z`

### Currency

Financial endpoints accept `?currency=USD` (default) or `?currency=TRY`.

### Error Envelope

```typescript
interface ApiError {
  error: {
    code: string;        // e.g. "VALIDATION_ERROR", "NOT_FOUND"
    message: string;     // Human-readable description
    details?: Record<string, string[]>; // Field-level errors
  };
}
```

HTTP Status Codes: `200` OK, `201` Created, `400` Bad Request, `401` Unauthorized, `403` Forbidden, `404` Not Found, `422` Unprocessable Entity, `500` Internal Server Error.

---

## 2. Authentication

### 2.1 POST `/auth/login`

Login with email and password.

**Request Body:**
```typescript
{
  email: string;
  password: string;
}
```

**Response `200`:**
```typescript
{
  token: string;       // JWT access token
  refreshToken: string;
  expiresIn: number;   // seconds
  user: User;          // See Shared Types
}
```

**Example:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "dGhpcyBpcyBhIHJl...",
  "expiresIn": 3600,
  "user": {
    "id": "usr-001",
    "name": "Ahmet Yilmaz",
    "email": "ahmet@derms.com",
    "initials": "AY",
    "role": "admin",
    "department": "Operations",
    "phone": "+90 532 123 4567"
  }
}
```

---

### 2.2 POST `/auth/logout`

Invalidate the current session.

**Headers:** `Authorization: Bearer <token>`

**Response `200`:**
```json
{ "message": "Logged out successfully" }
```

---

### 2.3 POST `/auth/forgot-password`

Send a password reset email.

**Request Body:**
```typescript
{
  email: string;
}
```

**Response `200`:**
```json
{ "message": "If an account exists, a reset link has been sent." }
```

---

### 2.4 POST `/auth/reset-password`

Reset password using the token from email.

**Request Body:**
```typescript
{
  token: string;
  newPassword: string;
  confirmPassword: string;
}
```

**Response `200`:**
```json
{ "message": "Password reset successfully" }
```

---

### 2.5 GET `/auth/me`

Get the currently authenticated user's profile.

**Headers:** `Authorization: Bearer <token>`

**Response `200`:**
```typescript
{
  id: string;
  name: string;
  email: string;
  avatar?: string;
  initials: string;
  role?: string;
  department?: string;
  phone?: string;
  facilities: Facility[]; // facilities user has access to
}
```

---

## 3. Facilities

> These endpoints do NOT require `X-Facility-Id`.

### 3.1 GET `/facilities`

List all facilities the user has access to.

**Response `200`:**
```typescript
{
  data: Facility[];
}
```

```typescript
interface Facility {
  id: string;
  name: string;
  type: "solar" | "wind" | "bess" | "hybrid";
  location: string;
  country: string;
  capacity: number; // MW
}
```

**Example:**
```json
{
  "data": [
    {
      "id": "fac-001",
      "name": "Ankara Hybrid Plant",
      "type": "hybrid",
      "location": "Ankara, Turkey",
      "country": "TR",
      "capacity": 25.0
    }
  ]
}
```

---

### 3.2 GET `/facilities/:id`

Get facility details.

**Response `200`:**
```typescript
Facility
```

---

## 4. Dashboard

### 4.1 GET `/dashboard/summary`

Main dashboard summary with solar, battery, and grid metrics.

**Response `200`:**
```typescript
{
  solar: SolarMetrics;
  battery: BatteryMetrics;
  grid: GridMetrics;
}
```

```typescript
interface SolarMetrics {
  currentPower: number;       // kW
  dailyGeneration: number;    // kWh
  capacity: number;           // kW
  status: "generating" | "idle" | "fault";
}

interface BatteryMetrics {
  soc: number;                // % (0-100)
  power: number;              // kW
  mode: "charging" | "discharging" | "idle" | "standby";
  temperature: number;        // C
  voltage: number;            // V
  current: number;            // A
  cycleCount: number;
  stateOfHealth: number;      // %
  capacity: number;           // kWh
  todayCharged: number;       // kWh
  todayDischarged: number;    // kWh
}

interface GridMetrics {
  connectionStatus: "connected" | "disconnected" | "island";
  quality: "excellent" | "good" | "fair" | "poor";
  importPower: number;        // kW
  exportPower: number;        // kW
  frequency: number;          // Hz
  voltage: number;            // V
  powerFactor: number;
  tariff: "peak" | "off-peak" | "shoulder";
}
```

---

### 4.2 GET `/dashboard/energy-usage`

Total energy usage breakdown by source.

**Response `200`:**
```typescript
{
  totalKWh: number;
  activePowerKW: number;
  reactivePowerKVAR: number;
  sources: EnergySourceBreakdown[];
}

interface EnergySourceBreakdown {
  label: string;        // "Grid", "BESS", "Solar", "Generator"
  value: number;        // kW or kWh
  percentage: number;   // 0-100
}
```

---

### 4.3 GET `/dashboard/bess-state`

BESS state for the dashboard card.

**Response `200`:**
```typescript
{
  name: string;
  socPercent: number;
  status: "charging" | "discharging" | "idle";
}
```

---

### 4.4 GET `/dashboard/pcs-state`

Power Conversion System state.

**Response `200`:**
```typescript
{
  mode: "On Grid" | "Off Grid";
  temperatureC: number;
  loadKW: number;
  efficiencyPercent: number;
  acVoltageV: number;
  acCurrentA: number;
  dcVoltageV: number;
  dcCurrentA: number;
}
```

---

### 4.5 GET `/dashboard/solar-capacity`

Solar capacity overview.

**Response `200`:**
```typescript
{
  currentPowerKW: number;
  maxCapacityKW: number;
  maintenanceRequired: boolean;
  temperatureC: number;
  efficiencyPercent: number;
  energyProducedKWh: number;
}
```

---

### 4.6 GET `/dashboard/energy-cost`

Daily energy cost and savings.

**Response `200`:**
```typescript
{
  totalCostTL: number;
  savedTL: number;
}
```

---

### 4.7 GET `/dashboard/power-flow`

Real-time power flow between solar, battery, grid, and load.

**Response `200`:**
```typescript
{
  solarPower: number;    // kW
  batteryPower: number;  // kW (positive=discharge, negative=charge)
  gridPower: number;     // kW (positive=import, negative=export)
  loadPower: number;     // kW
  batterySoc: number;    // %
}
```

---

### 4.8 GET `/dashboard/devices`

List all dashboard-level devices.

**Response `200`:**
```typescript
{
  data: DashboardDevice[];
}

interface DashboardDevice {
  id: string;
  name: string;
  type: "solar_inverter" | "battery" | "grid_meter" | "load_meter" | "weather_station";
  manufacturer: string;
  model: string;
  serialNumber: string;
  status: "online" | "offline" | "warning" | "error";
  lastSeen: string;          // ISO 8601
  location: string;
  specifications: Record<string, string>;
}
```

---

### 4.9 GET `/dashboard/charts/energy`

Energy chart data (solar, consumption, grid over 24 hours).

**Query:** `?period=today|week|month`

**Response `200` (period=today):**
```typescript
{
  data: {
    time: string;           // "HH:mm"
    solar: number;
    consumption: number;
    grid: number;
  }[];
}
```

**Response `200` (period=week):**
```typescript
{
  data: {
    day: string;      // "Mon", "Tue", ...
    value: number;    // kWh
  }[];
}
```

**Response `200` (period=month):**
```typescript
{
  data: {
    month: string;    // "Jan", "Feb", ...
    value: number;    // kWh
  }[];
}
```

---

### 4.10 GET `/dashboard/charts/consumption`

Hourly consumption vs production.

**Query:** `?period=today|week|month`

**Response `200` (period=today):**
```typescript
{
  data: {
    hour: string;
    consumption: number;
    production: number;
  }[];
}
```

---

### 4.11 GET `/dashboard/charts/source-distribution`

Energy source distribution (BESS, PV, Grid).

**Query:** `?period=today|week|month`

**Response `200` (period=today):**
```typescript
{
  data: {
    hour: string;
    bess: number;
    pv: number;
    grid: number;
  }[];
}
```

**Response `200` (period=week):**
```typescript
{
  data: {
    day: string;
    bess: number;
    pv: number;
    grid: number;
  }[];
}
```

**Response `200` (period=month):**
```typescript
{
  data: {
    month: string;
    bess: number;
    pv: number;
    grid: number;
  }[];
}
```

---

### 4.12 GET `/dashboard/charts/cost-savings`

Cost and savings over time.

**Query:** `?period=today|week|month`

**Response `200` (period=today):**
```typescript
{
  data: {
    hour: string;
    cost: number;
    savings: number;
  }[];
}
```

**Response `200` (period=week):**
```typescript
{
  data: {
    day: string;
    cost: number;
    savings: number;
  }[];
}
```

**Response `200` (period=month):**
```typescript
{
  data: {
    month: string;
    cost: number;
    savings: number;
  }[];
}
```

---

### 4.13 GET `/dashboard/charts/co2`

CO2 emission data.

**Query:** `?period=daily|monthly`

**Response `200` (period=daily):**
```typescript
{
  todayKg: number;
  data: {
    day: string;
    value: number;    // kg
  }[];
}
```

**Response `200` (period=monthly):**
```typescript
{
  data: {
    month: string;
    value: number;    // kg
  }[];
}
```

---

### 4.14 GET `/dashboard/charts/daily-usage-per-source`

Daily usage breakdown per energy source.

**Response `200`:**
```typescript
{
  data: {
    day: string;
    bess: number;
    pv: number;
    grid: number;
  }[];
}
```

---

## 5. Alarms

### 5.1 GET `/alarms`

List alarms with filtering, sorting, and pagination.

**Query Parameters:**

| Param                   | Type   | Description                                          |
|-------------------------|--------|------------------------------------------------------|
| `page`, `pageSize`      | number | Pagination                                           |
| `sortBy`                | string | `triggeredAt`, `severity`, `status`, `source`        |
| `sortDir`               | string | `asc` or `desc`                                      |
| `severity`              | string | Comma-separated: `critical,high,medium,low`          |
| `status`                | string | Comma-separated: `active,acknowledged,resolved`      |
| `sourceType`            | string | Comma-separated: `solar,bess,grid,inverter,system`   |
| `from`, `to`            | string | ISO 8601 date range for `triggeredAt`                |
| `alarmCode`             | string | Partial match on alarm code                          |
| `maintenanceTaskStatus` | string | `has_task`, `no_task`, `task_in_progress`, `task_completed` |
| `location`              | string | Location filter                                      |
| `search`                | string | Search alarm message and code                        |

**Response `200`:**
```typescript
PaginatedResponse<Alarm>
```

```typescript
interface Alarm {
  id: string;
  code: string;
  message: string;
  severity: AlarmSeverity;         // "critical" | "high" | "medium" | "low"
  status: AlarmStatus;             // "active" | "acknowledged" | "resolved"
  source: string;
  sourceType: AlarmSourceType;     // "solar" | "bess" | "grid" | "inverter" | "system"
  triggeredAt: string;             // ISO 8601
  value?: string;
  threshold?: string;
  acknowledgedAt?: string;
  acknowledgedBy?: User;
  resolvedAt?: string;
  resolvedBy?: User;
  relatedAsset?: Asset;
  relatedMaintenanceTaskId?: string;
  location?: string;
}
```

---

### 5.2 GET `/alarms/:id`

Get alarm details.

**Response `200`:**
```typescript
Alarm
```

---

### 5.3 POST `/alarms/:id/acknowledge`

Acknowledge an active alarm.

**Request Body:**
```typescript
{
  note?: string;
}
```

**Response `200`:**
```typescript
{
  id: string;
  status: "acknowledged";
  acknowledgedAt: string;
  acknowledgedBy: User;
}
```

---

### 5.4 POST `/alarms/:id/resolve`

Resolve an alarm.

**Request Body:**
```typescript
{
  resolution: string;
  note?: string;
}
```

**Response `200`:**
```typescript
{
  id: string;
  status: "resolved";
  resolvedAt: string;
  resolvedBy: User;
}
```

---

### 5.5 POST `/alarms/:id/create-task`

Create a maintenance task from an alarm.

**Request Body:**
```typescript
{
  title: string;
  description: string;
  priority: TaskPriority;     // "low" | "medium" | "high" | "critical"
  assignedToId: string;       // User ID
  dueDate: string;            // ISO 8601
}
```

**Response `201`:**
```typescript
{
  taskId: string;
  alarmId: string;
  relatedMaintenanceTaskId: string;
}
```

---

### 5.6 GET `/alarms/summary`

Alarm summary counts by severity and status.

**Response `200`:**
```typescript
{
  total: number;
  active: number;
  acknowledged: number;
  resolved: number;
  bySeverity: {
    critical: number;
    high: number;
    medium: number;
    low: number;
  };
  bySourceType: {
    solar: number;
    bess: number;
    grid: number;
    inverter: number;
    system: number;
  };
}
```

---

### 5.7 GET `/alarms/locations`

Get distinct alarm locations for filter dropdown.

**Response `200`:**
```typescript
{
  data: string[];   // ["Building A", "Storage Bay 1", ...]
}
```

---

## 6. Notifications

### 6.1 GET `/notifications`

List system notifications with filtering and pagination.

**Query Parameters:**

| Param       | Type   | Description                                                   |
|-------------|--------|---------------------------------------------------------------|
| `page`, `pageSize` | number | Pagination                                              |
| `type`      | string | `maintenance_completed`, `system_update`, `alarm_resolved`, `schedule_reminder`, `system_alert` |
| `readStatus`| string | `all`, `read`, `unread`                                       |

**Response `200`:**
```typescript
PaginatedResponse<SystemNotification>
```

```typescript
interface SystemNotification {
  id: string;
  type: NotificationType;
  title: string;
  message: string;
  timestamp: string;       // ISO 8601
  read: boolean;
  relatedAlarmId?: string;
  relatedTaskId?: string;
}

type NotificationType =
  | "maintenance_completed"
  | "system_update"
  | "alarm_resolved"
  | "schedule_reminder"
  | "system_alert";
```

---

### 6.2 GET `/notifications/:id`

Get notification details.

**Response `200`:**
```typescript
SystemNotification
```

---

### 6.3 PATCH `/notifications/:id/read`

Mark a notification as read.

**Response `200`:**
```typescript
{
  id: string;
  read: true;
}
```

---

### 6.4 POST `/notifications/mark-all-read`

Mark all notifications as read.

**Response `200`:**
```typescript
{
  updatedCount: number;
}
```

---

### 6.5 GET `/notifications/unread-count`

Get unread notification count.

**Response `200`:**
```typescript
{
  count: number;
}
```

---

## 7. Maintenance

### 7.1 GET `/maintenance/tasks`

List maintenance tasks with filtering, sorting, and pagination.

**Query Parameters:**

| Param        | Type   | Description                                     |
|--------------|--------|-------------------------------------------------|
| `page`, `pageSize` | number | Pagination                                |
| `sortBy`     | string | `assignedDate`, `dueDate`, `priority`, `status` |
| `sortDir`    | string | `asc` or `desc`                                 |
| `status`     | string | Comma-separated: `scheduled,pending,in_progress,completed,closed,overdue` |
| `priority`   | string | Comma-separated: `low,medium,high,critical`     |
| `assignedTo` | string | User ID                                         |
| `from`, `to` | string | ISO 8601 date range for `dueDate`               |
| `search`     | string | Search title and description                    |

**Response `200`:**
```typescript
PaginatedResponse<MaintenanceTask>
```

```typescript
interface MaintenanceTask {
  id: string;
  title: string;
  description: string;
  status: TaskStatus;
  priority: TaskPriority;
  assignedTo: User;
  assignedBy: User;
  assignedDate: string;        // ISO 8601
  dueDate: string;             // ISO 8601
  relatedAsset?: Asset;
  relatedAlarm?: RelatedAlarm;
  checklist?: ChecklistItem[];
  workSessions?: WorkSession[];
  completionDetails?: TaskCompletionDetails;
  closeDetails?: TaskCloseDetails;
}

type TaskStatus = "scheduled" | "pending" | "in_progress" | "completed" | "closed" | "overdue";
type TaskPriority = "low" | "medium" | "high" | "critical";
```

---

### 7.2 POST `/maintenance/tasks`

Create a new maintenance task.

**Request Body:**
```typescript
{
  title: string;
  description: string;
  priority: TaskPriority;
  assignedToId: string;
  dueDate: string;             // ISO 8601
  relatedAssetId?: string;
  relatedAlarmId?: string;
  checklist?: { label: string }[];
}
```

**Response `201`:**
```typescript
MaintenanceTask
```

---

### 7.3 GET `/maintenance/tasks/:id`

Get task details.

**Response `200`:**
```typescript
MaintenanceTask
```

---

### 7.4 PATCH `/maintenance/tasks/:id`

Update a task (title, description, priority, assignment, due date).

**Request Body:**
```typescript
{
  title?: string;
  description?: string;
  priority?: TaskPriority;
  assignedToId?: string;
  dueDate?: string;
}
```

**Response `200`:**
```typescript
MaintenanceTask
```

---

### 7.5 PATCH `/maintenance/tasks/:id/status`

Change task status.

**Request Body:**
```typescript
{
  status: TaskStatus;
}
```

**Response `200`:**
```typescript
{
  id: string;
  status: TaskStatus;
  statusChange: StatusChange;
}

interface StatusChange {
  id: string;
  taskId: string;
  fromStatus: TaskStatus | null;
  toStatus: TaskStatus;
  changedBy: User;
  changedAt: string;
}
```

---

### 7.6 POST `/maintenance/tasks/:id/complete`

Complete a task with resolution details.

**Request Body:**
```typescript
{
  resolution: string;
  rootCause?: string;
  laborHours: number;
  partsUsed?: string;
  recommendations?: string;
}
```

**Response `200`:**
```typescript
{
  id: string;
  status: "completed";
  completionDetails: TaskCompletionDetails;
}

interface TaskCompletionDetails {
  resolution: string;
  rootCause?: string;
  laborHours: number;
  partsUsed?: string;
  recommendations?: string;
  completedAt: string;
  completedBy: User;
}
```

---

### 7.7 POST `/maintenance/tasks/:id/close`

Close a task (cancel, mark as duplicate, etc.).

**Request Body:**
```typescript
{
  reason: CloseReason;       // "cancelled" | "duplicate" | "not_needed" | "transferred" | "other"
  explanation: string;
}
```

**Response `200`:**
```typescript
{
  id: string;
  status: "closed";
  closeDetails: TaskCloseDetails;
}

interface TaskCloseDetails {
  reason: CloseReason;
  explanation: string;
  closedAt: string;
  closedBy: User;
}
```

---

### 7.8 GET `/maintenance/tasks/:id/timeline`

Get the status change history for a task.

**Response `200`:**
```typescript
{
  data: StatusChange[];
}
```

---

### 7.9 GET `/maintenance/tasks/:id/notes`

Get notes for a task.

**Response `200`:**
```typescript
{
  data: TaskNote[];
}

interface TaskNote {
  id: string;
  taskId: string;
  content: string;
  author: User;
  createdAt: string;
}
```

---

### 7.10 POST `/maintenance/tasks/:id/notes`

Add a note to a task.

**Request Body:**
```typescript
{
  content: string;
}
```

**Response `201`:**
```typescript
TaskNote
```

---

### 7.11 GET `/maintenance/tasks/:id/checklist`

Get the checklist for a task.

**Response `200`:**
```typescript
{
  data: ChecklistItem[];
}

interface ChecklistItem {
  id: string;
  label: string;
  completed: boolean;
  completedAt?: string;
  completedBy?: User;
}
```

---

### 7.12 PATCH `/maintenance/tasks/:id/checklist/:itemId`

Toggle or update a checklist item.

**Request Body:**
```typescript
{
  completed: boolean;
}
```

**Response `200`:**
```typescript
ChecklistItem
```

---

### 7.13 POST `/maintenance/tasks/:id/checklist`

Add a checklist item.

**Request Body:**
```typescript
{
  label: string;
}
```

**Response `201`:**
```typescript
ChecklistItem
```

---

### 7.14 GET `/maintenance/tasks/:id/work-sessions`

Get work sessions for a task.

**Response `200`:**
```typescript
{
  data: WorkSession[];
}

interface WorkSession {
  id: string;
  taskId: string;
  startedAt: string;
  endedAt?: string;
  user: User;
}
```

---

### 7.15 POST `/maintenance/tasks/:id/work-sessions`

Start a work session.

**Response `201`:**
```typescript
WorkSession
```

---

### 7.16 PATCH `/maintenance/tasks/:id/work-sessions/:sessionId`

End a work session.

**Request Body:**
```typescript
{
  endedAt: string;
}
```

**Response `200`:**
```typescript
WorkSession
```

---

### 7.17 GET `/maintenance/scheduled`

Get scheduled maintenance entries (for calendar view).

**Query:** `?from=2026-02-01&to=2026-03-01`

**Response `200`:**
```typescript
{
  data: ScheduledMaintenance[];
}

interface ScheduledMaintenance {
  id: string;
  title: string;
  scheduledDate: string;
  assetName: string;
  recurrence?: "daily" | "weekly" | "monthly" | "quarterly" | "yearly";
}
```

---

### 7.18 GET `/maintenance/users`

List users available for task assignment.

**Response `200`:**
```typescript
{
  data: User[];
}
```

---

### 7.19 GET `/maintenance/assets`

List assets available for task association.

**Response `200`:**
```typescript
{
  data: Asset[];
}

interface Asset {
  id: string;
  name: string;
  type: "solar" | "bess" | "grid" | "inverter" | "other";
  location?: string;
  capacity?: string;
  status?: "operational" | "maintenance" | "offline" | "warning";
  lastMaintenance?: string;
  serialNumber?: string;
}
```

---

## 8. BESS Monitoring

### 8.1 GET `/monitoring/bess/units`

List all BESS units.

**Query Parameters:**

| Param     | Type   | Description                                   |
|-----------|--------|-----------------------------------------------|
| `sortBy`  | string | `name`, `status`, `mode`, `soc`, `soh`, `power`, `capacity`, `alarms` |
| `sortDir` | string | `asc` or `desc`                               |
| `status`  | string | Comma-separated: `online,offline,fault,standby`|
| `search`  | string | Search unit name                              |

**Response `200`:**
```typescript
{
  data: BessUnitSummary[];
}

interface BessUnitSummary {
  id: string;
  name: string;
  status: BessUnitStatus;          // "online" | "offline" | "fault" | "standby"
  operatingMode: BessOperatingMode; // "charging" | "discharging" | "idle" | "standby" | "balancing"
  nameplate: {
    brand: string;
    model: string;
    serialNumber: string;
    chemistry: string;
    commissioningDate: string;
    capacityKWh: number;
    maxPowerKW: number;
    nominalVoltage: number;
    voltageRange: [number, number];
    rackCount: number;
    packsPerRack: number;
    modulesPerPack: number;
    cellsPerModule: number;
  };
  live: {
    soc: number;
    soh: number;
    power: number;       // kW (positive=discharge, negative=charge)
    voltage: number;
    current: number;
    frequency: number;
    tempAvg: number;
    tempMin: number;
    tempMax: number;
  };
  lifecycle: {
    cycleCount: number;
    lifetimeThroughputMWh: number;
    dailyChargedKWh: number;
    dailyDischargedKWh: number;
  };
  connectedDevices: {
    inverter: string;
    meter: string;
  };
  activeAlarmCount: number;
}
```

---

### 8.2 GET `/monitoring/bess/units/:id`

Get full BESS unit detail including racks (without deep hierarchy).

**Response `200`:**
```typescript
BessUnit   // Full unit with racks array (packs/modules/cells omitted at this level)
```

---

### 8.3 GET `/monitoring/bess/units/:id/racks`

Get racks for a BESS unit.

**Response `200`:**
```typescript
{
  data: BessRack[];
}

interface BessRack {
  id: string;
  rackNumber: number;
  status: BessComponentStatus;     // "normal" | "warning" | "fault" | "offline"
  voltage: number;
  current: number;
  soc: number;
  soh: number;
  power: number;
  tempMin: number;
  tempMax: number;
  tempAvg: number;
}
```

---

### 8.4 GET `/monitoring/bess/racks/:rackId/packs`

Get packs for a rack.

**Response `200`:**
```typescript
{
  data: BessPack[];
}

interface BessPack {
  id: string;
  packNumber: number;
  status: BessComponentStatus;
  voltage: number;
  current: number;
  soc: number;
  soh: number;
  tempMin: number;
  tempMax: number;
  tempAvg: number;
}
```

---

### 8.5 GET `/monitoring/bess/packs/:packId/modules`

Get modules for a pack.

**Response `200`:**
```typescript
{
  data: BessModule[];
}

interface BessModule {
  id: string;
  moduleNumber: number;
  status: BessComponentStatus;
  voltage: number;
  current: number;
  soc: number;
  soh: number;
  tempMin: number;
  tempMax: number;
  tempAvg: number;
}
```

---

### 8.6 GET `/monitoring/bess/modules/:moduleId/cells`

Get cells for a module.

**Response `200`:**
```typescript
{
  data: BessCell[];
}

interface BessCell {
  id: string;
  cellNumber: number;
  status: BessComponentStatus;
  voltage: number;
  current: number;
  temperature: number;
  soc: number;
  soh: number;
  internalResistance: number;    // mOhm
  balancing: boolean;
  cycleCount: number;
}
```

---

### 8.7 GET `/monitoring/bess/units/:id/faults`

Get faults for a BESS unit.

**Query:** `?status=active,recovered` `?severity=critical,major,minor,warning`

**Response `200`:**
```typescript
{
  data: BessFault[];
}

interface BessFault {
  id: string;
  unitId: string;
  code: string;
  name: string;
  severity: FaultSeverity;       // "critical" | "major" | "minor" | "warning"
  status: FaultStatus;           // "active" | "recovered"
  source: string;
  occurredAt: string;
  recoveredAt: string | null;
  relatedAlarmId: string;
}
```

---

### 8.8 GET `/monitoring/bess/units/:id/curves`

Get 24-hour curve data for a BESS unit (5-min intervals).

**Response `200`:**
```typescript
{
  power: BessPowerCurvePoint[];
  voltage: BessVoltageCurvePoint[];
  temperature: BessTemperatureCurvePoint[];
}

interface BessPowerCurvePoint {
  time: string;         // "HH:mm"
  power: number;        // kW
  soc: number;          // %
}

interface BessVoltageCurvePoint {
  time: string;
  voltage: number;      // V
  current: number;      // A
}

interface BessTemperatureCurvePoint {
  time: string;
  tempAvg: number;
  tempMin: number;
  tempMax: number;
}
```

---

### 8.9 GET `/monitoring/bess/summary`

Aggregated BESS fleet summary.

**Response `200`:**
```typescript
{
  totalUnits: number;
  onlineCount: number;
  offlineCount: number;
  faultCount: number;
  standbyCount: number;
  totalCapacityKWh: number;
  totalPowerKW: number;
  avgSoc: number;
  avgSoh: number;
  totalActiveAlarms: number;
}
```

---

### 8.10 GET `/monitoring/bess/hierarchy`

Get hierarchy table data with sorting/filtering.

**Query Parameters:**

| Param     | Type   | Description                                                                 |
|-----------|--------|-----------------------------------------------------------------------------|
| `unitId`  | string | Required: unit to start from                                               |
| `level`   | string | `rack`, `pack`, `module`, `cell`                                           |
| `parentId`| string | Parent ID (rack, pack, or module ID) for drill-down                        |
| `sortBy`  | string | `label`, `status`, `soc`, `soh`, `voltage`, `current`, `tempMin`, `tempMax`, `tempAvg` |
| `sortDir` | string | `asc` or `desc`                                                            |

**Response `200`:**
```typescript
{
  level: HierarchyLevel;
  data: HierarchyItem[];
}

// HierarchyItem is a polymorphic type: BessRack | BessPack | BessModule | BessCell
```

---

## 9. Solar Inverters

### 9.1 GET `/monitoring/solar-inverters`

List all solar inverters.

**Query Parameters:**

| Param     | Type   | Description                                                          |
|-----------|--------|----------------------------------------------------------------------|
| `page`, `pageSize` | number | Pagination                                                 |
| `sortBy`  | string | `deviceName`, `deviceModel`, `serialNumber`, `status`, `yieldToday`, `totalActivePower`, `associatedPlant` |
| `sortDir` | string | `asc` or `desc`                                                     |
| `status`  | string | Comma-separated: `normal,offline,warning,fault`                      |
| `search`  | string | Search device name, model, or serial number                         |

**Response `200`:**
```typescript
PaginatedResponse<SolarInverter>
```

```typescript
interface SolarInverter {
  id: string;
  deviceName: string;
  deviceModel: string;
  serialNumber: string;
  status: InverterStatus;            // "normal" | "offline" | "warning" | "fault"
  yieldToday: number | null;         // kWh
  totalActivePower: number | null;   // kW
  associatedPlant: string;

  // Detail fields
  totalDcPower: number | null;
  operatingStatus: string;
  phaseACurrent: number | null;
  phaseBCurrent: number | null;
  phaseCCurrent: number | null;
  totalReactivePower: number | null;
  insulationImpedance: number | null;
  gridFrequency: number | null;
  abLineVoltage: number | null;
  bcLineVoltage: number | null;
  caLineVoltage: number | null;
  totalPowerFactor: number | null;
  totalYield: number | null;         // MWh

  mpptData: MpptData[];
  stringCurrents: (number | null)[];

  totalApparentPower: number | null;
  dailyOperatingTime: number | null;
  dailyYieldTheoretical: number | null;

  commissioningDate: string;
  manufacturer: string;
}

interface MpptData {
  id: number;
  voltage: number | null;
  current: number | null;
}
```

---

### 9.2 GET `/monitoring/solar-inverters/:id`

Get inverter details.

**Response `200`:**
```typescript
SolarInverter
```

---

### 9.3 GET `/monitoring/solar-inverters/:id/faults`

Get faults for an inverter.

**Query:** `?status=active,recovered` `?severity=critical,major,minor,warning`

**Response `200`:**
```typescript
{
  data: InverterFault[];
}

interface InverterFault {
  id: string;
  inverterId: string;
  code: string;
  name: string;
  severity: FaultSeverity;
  status: FaultStatus;
  occurredAt: string;
  recoveredAt: string | null;
  relatedAlarmId: string;
}
```

---

### 9.4 GET `/monitoring/solar-inverters/:id/curves`

Get 24-hour curve data for an inverter.

**Response `200`:**
```typescript
{
  power: PowerCurvePoint[];
  voltage: VoltageCurvePoint[];
  current: CurrentCurvePoint[];
}

interface PowerCurvePoint {
  time: string;
  activePower: number;
  dcPower: number;
  reactivePower: number;
}

interface VoltageCurvePoint {
  time: string;
  abVoltage: number;
  bcVoltage: number;
  caVoltage: number;
}

interface CurrentCurvePoint {
  time: string;
  phaseA: number;
  phaseB: number;
  phaseC: number;
}
```

---

### 9.5 GET `/monitoring/solar-inverters/analytics/summary`

Aggregated solar plant summary.

**Response `200`:**
```typescript
{
  plantActivePower: number;           // kW
  plantYieldToday: number;            // kWh
  plantTotalYield: number;            // MWh
  installedCapacity: number;          // kWp
  yieldThisMonth: number;            // MWh
  deviceStatusSummary: Record<InverterStatus, number>;
}
```

---

### 9.6 GET `/monitoring/solar-inverters/analytics/power-curve`

Aggregated plant power curve (sum of all inverters).

**Response `200`:**
```typescript
{
  data: {
    time: string;
    activePower: number;     // kW
  }[];
}
```

---

### 9.7 GET `/monitoring/solar-inverters/analytics/yield-comparison`

Yield comparison across all inverters.

**Response `200`:**
```typescript
{
  data: {
    name: string;
    yield: number;           // kWh
  }[];
}
```

---

### 9.8 GET `/monitoring/solar-inverters/analytics/monthly-production`

Monthly production for last 12 months.

**Response `200`:**
```typescript
{
  data: {
    month: string;
    yield: number;           // MWh
  }[];
}
```

---

### 9.9 GET `/monitoring/solar-inverters/analytics/daily-trend`

Daily energy trend for last 30 days.

**Response `200`:**
```typescript
{
  data: {
    date: string;
    yield: number;           // kWh
  }[];
}
```

---

### 9.10 GET `/monitoring/solar-inverters/analytics/pr-trend`

Performance Ratio trend for last 30 days.

**Response `200`:**
```typescript
{
  data: {
    date: string;
    pr: number;              // % (0-100)
  }[];
}
```

---

### 9.11 GET `/monitoring/solar-inverters/analytics/specific-yield`

Specific yield per inverter (kWh/kWp).

**Response `200`:**
```typescript
{
  data: {
    name: string;
    specificYield: number;   // kWh/kWp
  }[];
}
```

---

### 9.12 GET `/monitoring/solar-inverters/analytics/mppt-comparison`

Average power per MPPT tracker across all inverters.

**Response `200`:**
```typescript
{
  data: {
    tracker: string;         // "MPPT 1", "MPPT 2", ...
    avgPower: number;        // kW
  }[];
}
```

---

### 9.13 GET `/monitoring/solar-inverters/analytics/string-heatmap`

String current heatmap (per inverter, per string).

**Response `200`:**
```typescript
{
  data: {
    inverter: string;
    currents: (number | null)[];   // array of string currents
  }[];
}
```

---

## 10. Grid Loads

### 10.1 GET `/monitoring/grid-devices`

List all grid devices.

**Query Parameters:**

| Param      | Type   | Description                                                        |
|------------|--------|--------------------------------------------------------------------|
| `page`, `pageSize` | number | Pagination                                               |
| `sortBy`   | string | `deviceName`, `deviceType`, `deviceModel`, `status`, `activePower`, `powerFactor`, `location` |
| `sortDir`  | string | `asc` or `desc`                                                   |
| `status`   | string | Comma-separated: `normal,offline,warning,fault,tripped`            |
| `deviceType` | string | Comma-separated: `connection-point,transformer,power-meter,protection-relay,circuit-breaker,switchgear` |
| `search`   | string | Search device name                                                |

**Response `200`:**
```typescript
PaginatedResponse<GridDevice>
```

```typescript
interface GridDevice {
  id: string;
  deviceName: string;
  deviceType: GridDeviceType;
  deviceModel: string;
  serialNumber: string;
  status: GridDeviceStatus;           // "normal" | "offline" | "warning" | "fault" | "tripped"
  location: string;

  // Electrical
  activePower: number | null;         // kW
  reactivePower: number | null;       // kVar
  apparentPower: number | null;       // kVA
  powerFactor: number | null;
  frequency: number | null;           // Hz
  powerFlowDirection: "import" | "export" | null;

  // 3-phase
  voltagePhaseA: number | null;
  voltagePhaseB: number | null;
  voltagePhaseC: number | null;
  currentPhaseA: number | null;
  currentPhaseB: number | null;
  currentPhaseC: number | null;
  thd: number | null;                 // %

  // Transformer-specific
  oilTemperature: number | null;
  windingTemperature: number | null;
  tapPosition: number | null;
  loadingPercent: number | null;
  voltageRatio: string | null;

  // Breaker/Relay-specific
  breakerPosition: "closed" | "open" | "tripped" | null;
  relayTripStatus: boolean | null;
  lastTripTime: string | null;

  // Energy
  importedToday: number | null;       // kWh
  exportedToday: number | null;       // kWh
  totalImported: number | null;       // MWh
  totalExported: number | null;       // MWh

  commissioningDate: string;
  manufacturer: string;
}

type GridDeviceType =
  | "connection-point"
  | "transformer"
  | "power-meter"
  | "protection-relay"
  | "circuit-breaker"
  | "switchgear";
```

---

### 10.2 GET `/monitoring/grid-devices/:id`

Get grid device details.

**Response `200`:**
```typescript
GridDevice
```

---

### 10.3 GET `/monitoring/grid-devices/:id/faults`

Get faults for a grid device.

**Query:** `?status=active,recovered` `?severity=critical,major,minor,warning`

**Response `200`:**
```typescript
{
  data: GridFault[];
}

interface GridFault {
  id: string;
  deviceId: string;
  code: string;
  name: string;
  severity: FaultSeverity;
  status: FaultStatus;
  occurredAt: string;
  recoveredAt: string | null;
  relatedAlarmId: string;
}
```

---

### 10.4 GET `/monitoring/grid-devices/:id/curves`

Get 24-hour curve data for a grid device.

**Response `200`:**
```typescript
{
  power: GridPowerCurvePoint[];
  voltage: GridVoltageCurvePoint[];
  frequency: GridFrequencyCurvePoint[];
  powerFactor: GridPowerFactorCurvePoint[];
}

interface GridPowerCurvePoint {
  time: string;
  activePower: number;
  reactivePower: number;
  apparentPower: number;
}

interface GridVoltageCurvePoint {
  time: string;
  phaseA: number;
  phaseB: number;
  phaseC: number;
}

interface GridFrequencyCurvePoint {
  time: string;
  frequency: number;
}

interface GridPowerFactorCurvePoint {
  time: string;
  powerFactor: number;
}
```

---

### 10.5 GET `/monitoring/grid-devices/analytics/summary`

Aggregated grid analytics summary.

**Response `200`:**
```typescript
{
  netPower: number;                    // kW
  totalImportedToday: number;          // kWh
  totalExportedToday: number;          // kWh
  avgPowerFactor: number;
  peakDemandToday: number;             // kW
  gridAvailability: number;            // %
  voltageCompliance: number;           // %
  powerQualityIndex: number;           // 0-100
  deviceStatusSummary: Record<GridDeviceStatus, number>;
}
```

---

### 10.6 GET `/monitoring/grid-devices/analytics/power-curve`

Grid power flow over 24 hours (import/export).

**Response `200`:**
```typescript
{
  data: {
    time: string;
    imported: number;     // kW
    exported: number;     // kW (negative values)
  }[];
}
```

---

### 10.7 GET `/monitoring/grid-devices/analytics/energy-comparison`

Per-device energy comparison (import/export).

**Response `200`:**
```typescript
{
  data: {
    name: string;
    imported: number;     // kWh
    exported: number;     // kWh
  }[];
}
```

---

### 10.8 GET `/monitoring/grid-devices/analytics/monthly-balance`

Monthly energy balance for last 12 months.

**Response `200`:**
```typescript
{
  data: {
    month: string;
    imported: number;     // MWh
    exported: number;     // MWh
  }[];
}
```

---

### 10.9 GET `/monitoring/grid-devices/analytics/peak-demand`

Daily peak demand for last 30 days.

**Response `200`:**
```typescript
{
  data: {
    date: string;
    peakDemand: number;   // kW
  }[];
}
```

---

### 10.10 GET `/monitoring/grid-devices/analytics/performance`

Grid performance metrics.

**Response `200`:**
```typescript
{
  gridAvailability: number;       // %
  voltageCompliance: number;      // %
  powerQualityIndex: number;      // 0-100
}
```

---

### 10.11 GET `/monitoring/grid-devices/analytics/voltage-profile`

3-phase voltage profile over 24 hours.

**Response `200`:**
```typescript
{
  data: {
    time: string;
    phaseA: number;       // V
    phaseB: number;
    phaseC: number;
  }[];
}
```

---

### 10.12 GET `/monitoring/grid-devices/analytics/frequency-trend`

Grid frequency trend over 24 hours.

**Response `200`:**
```typescript
{
  data: {
    time: string;
    frequency: number;    // Hz
  }[];
}
```

---

### 10.13 GET `/monitoring/grid-devices/analytics/pf-trend`

Power factor trend for last 30 days.

**Response `200`:**
```typescript
{
  data: {
    date: string;
    powerFactor: number;
  }[];
}
```

---

### 10.14 GET `/monitoring/grid-devices/analytics/transformer-loading`

Transformer loading heatmap (24h x transformers).

**Response `200`:**
```typescript
{
  data: {
    device: string;
    hours: number[];      // 24 values, each 0-100%
  }[];
}
```

---

## 11. Wind Turbines

### 11.1 GET `/monitoring/wind-turbines`

List all wind turbines.

**Query Parameters:**

| Param     | Type   | Description                                                        |
|-----------|--------|--------------------------------------------------------------------|
| `page`, `pageSize` | number | Pagination                                               |
| `sortBy`  | string | `deviceName`, `deviceModel`, `serialNumber`, `status`, `yieldToday`, `activePower`, `windFarm` |
| `sortDir` | string | `asc` or `desc`                                                   |
| `status`  | string | Comma-separated: `normal,offline,warning,fault`                    |
| `search`  | string | Search device name, model, or serial number                       |

**Response `200`:**
```typescript
PaginatedResponse<WindTurbine>
```

```typescript
interface WindTurbine {
  id: string;
  deviceName: string;
  deviceModel: string;
  serialNumber: string;
  status: TurbineStatus;
  yieldToday: number | null;          // MWh
  activePower: number | null;         // kW
  ratedPower: number;                 // kW
  windFarm: string;

  // Wind conditions
  windSpeed: number | null;           // m/s
  windDirection: number | null;       // degrees
  ambientTemp: number | null;

  // Rotor & Generator
  rotorSpeed: number | null;          // rpm
  generatorSpeed: number | null;      // rpm
  generatorPower: number | null;      // kW

  // Electrical
  reactivePower: number | null;       // kVar
  powerFactor: number | null;
  gridFrequency: number | null;       // Hz
  gridVoltage: number | null;         // V

  // Pitch & Yaw
  pitchAngles: [number | null, number | null, number | null];
  nacellePosition: number | null;     // degrees
  yawError: number | null;            // degrees

  // Temperatures
  generatorTemp: number | null;
  gearboxOilTemp: number | null;
  mainBearingTemp: number | null;
  controlCabinetTemp: number | null;

  // Cumulative
  totalYield: number | null;          // MWh
  totalOperatingHours: number | null;
  dailyOperatingTime: number | null;  // hours

  hubHeight: number;                  // m
  rotorDiameter: number;              // m
  commissioningDate: string;
  manufacturer: string;
}
```

---

### 11.2 GET `/monitoring/wind-turbines/:id`

Get turbine details.

**Response `200`:**
```typescript
WindTurbine
```

---

### 11.3 GET `/monitoring/wind-turbines/:id/faults`

Get faults for a turbine.

**Query:** `?status=active,recovered` `?severity=critical,major,minor,warning`

**Response `200`:**
```typescript
{
  data: TurbineFault[];
}

interface TurbineFault {
  id: string;
  turbineId: string;
  code: string;
  name: string;
  severity: FaultSeverity;
  status: FaultStatus;
  occurredAt: string;
  recoveredAt: string | null;
  relatedAlarmId: string;
}
```

---

### 11.4 GET `/monitoring/wind-turbines/:id/curves`

Get 24-hour curve data for a turbine.

**Response `200`:**
```typescript
{
  power: PowerCurvePoint[];
  windSpeed: WindSpeedCurvePoint[];
  pitch: PitchCurvePoint[];
  temperature: TemperatureCurvePoint[];
}

interface PowerCurvePoint {
  time: string;
  activePower: number;
  windSpeed: number;
  reactivePower: number;
}

interface WindSpeedCurvePoint {
  time: string;
  windSpeed: number;
  rotorSpeed: number;
}

interface PitchCurvePoint {
  time: string;
  pitchAngle: number;
  activePower: number;
}

interface TemperatureCurvePoint {
  time: string;
  generator: number;
  gearboxOil: number;
  mainBearing: number;
}
```

---

### 11.5 GET `/monitoring/wind-turbines/analytics/summary`

Aggregated wind farm summary.

**Response `200`:**
```typescript
{
  farmActivePower: number;            // kW
  farmYieldToday: number;             // MWh
  farmTotalYield: number;             // MWh
  installedCapacity: number;          // kW
  yieldThisMonth: number;             // MWh
  capacityFactor: number;             // %
  availability: number;               // %
  meanWindSpeed: number;              // m/s
  specificYield: number;              // kWh/kW
  deviceStatusSummary: Record<TurbineStatus, number>;
}
```

---

### 11.6 GET `/monitoring/wind-turbines/analytics/farm-power-curve`

Aggregated farm power curve (sum of all turbines).

**Response `200`:**
```typescript
{
  data: {
    time: string;
    activePower: number;     // kW
  }[];
}
```

---

### 11.7 GET `/monitoring/wind-turbines/analytics/yield-comparison`

Yield comparison across all turbines.

**Response `200`:**
```typescript
{
  data: {
    name: string;
    yield: number;           // MWh
  }[];
}
```

---

### 11.8 GET `/monitoring/wind-turbines/analytics/monthly-production`

Monthly production for last 12 months.

**Response `200`:**
```typescript
{
  data: {
    month: string;
    yield: number;           // MWh
  }[];
}
```

---

### 11.9 GET `/monitoring/wind-turbines/analytics/daily-trend`

Daily energy trend for last 30 days.

**Response `200`:**
```typescript
{
  data: {
    date: string;
    yield: number;           // MWh
  }[];
}
```

---

### 11.10 GET `/monitoring/wind-turbines/analytics/cf-trend`

Capacity factor trend for last 30 days.

**Response `200`:**
```typescript
{
  data: {
    date: string;
    capacityFactor: number;  // % (0-100)
  }[];
}
```

---

### 11.11 GET `/monitoring/wind-turbines/analytics/specific-yield`

Specific yield per turbine (kWh/kW).

**Response `200`:**
```typescript
{
  data: {
    name: string;
    specificYield: number;   // kWh/kW
  }[];
}
```

---

### 11.12 GET `/monitoring/wind-turbines/analytics/power-curve-scatter`

Wind speed vs power scatter data across all turbines.

**Response `200`:**
```typescript
{
  data: {
    windSpeed: number;       // m/s
    power: number;           // kW
    turbine: string;
  }[];
}
```

---

### 11.13 GET `/monitoring/wind-turbines/analytics/availability-heatmap`

Availability heatmap (24h x turbines).

**Response `200`:**
```typescript
{
  data: {
    turbine: string;
    hours: number[];         // 24 values, each 0-100%
  }[];
}
```

---

## 12. Consumption

### 12.1 GET `/monitoring/consumers`

List all consumers.

**Query Parameters:**

| Param     | Type   | Description                                                                |
|-----------|--------|----------------------------------------------------------------------------|
| `page`, `pageSize` | number | Pagination                                                       |
| `sortBy`  | string | `name`, `type`, `status`, `activePower`, `peakDemandToday`, `consumptionToday`, `loadFactor`, `location` |
| `sortDir` | string | `asc` or `desc`                                                           |
| `status`  | string | Comma-separated: `normal,offline,warning,fault,curtailed`                  |
| `type`    | string | Comma-separated: `commercial-building,industrial-facility,ev-charging-station,hvac-system,lighting-system,data-center,water-pumping,cold-storage` |
| `search`  | string | Search consumer name                                                      |

**Response `200`:**
```typescript
PaginatedResponse<Consumer>
```

```typescript
interface Consumer {
  id: string;
  name: string;
  type: ConsumerType;
  meterModel: string;
  serialNumber: string;
  status: ConsumerStatus;             // "normal" | "offline" | "warning" | "fault" | "curtailed"
  location: string;

  // Electrical
  activePower: number | null;         // kW
  reactivePower: number | null;       // kVar
  powerFactor: number | null;
  peakDemandToday: number | null;     // kW
  contractedCapacity: number;         // kW
  loadFactor: number | null;          // %

  // Consumption
  consumptionToday: number | null;    // kWh
  consumptionMTD: number | null;      // MWh
  estimatedCostToday: number | null;  // $

  // Demand Response
  drStatus: DRStatus;                 // "enrolled" | "active" | "standby" | "not-enrolled"
  curtailableCapacity: number | null; // kW
  currentCurtailment: number | null;  // kW
  drEventsYTD: number;

  commissioningDate: string;
  manufacturer: string;
}

type ConsumerType =
  | "commercial-building"
  | "industrial-facility"
  | "ev-charging-station"
  | "hvac-system"
  | "lighting-system"
  | "data-center"
  | "water-pumping"
  | "cold-storage";
```

---

### 12.2 GET `/monitoring/consumers/:id`

Get consumer details.

**Response `200`:**
```typescript
Consumer
```

---

### 12.3 GET `/monitoring/consumers/:id/faults`

Get faults for a consumer.

**Query:** `?status=active,recovered` `?severity=critical,major,minor,warning`

**Response `200`:**
```typescript
{
  data: ConsumerFault[];
}

interface ConsumerFault {
  id: string;
  consumerId: string;
  code: string;
  name: string;
  severity: FaultSeverity;
  status: FaultStatus;
  occurredAt: string;
  recoveredAt: string | null;
  relatedAlarmId: string;
}
```

---

### 12.4 GET `/monitoring/consumers/:id/curves`

Get 24-hour curve data for a consumer.

**Response `200`:**
```typescript
{
  consumption: ConsumerPowerCurvePoint[];
  demand: ConsumerDemandCurvePoint[];
}

interface ConsumerPowerCurvePoint {
  time: string;
  activePower: number;
  reactivePower: number;
}

interface ConsumerDemandCurvePoint {
  time: string;
  demand: number;
  contracted: number;
}
```

---

### 12.5 GET `/monitoring/consumers/analytics/summary`

Aggregated consumption analytics summary.

**Response `200`:**
```typescript
{
  totalDemand: number;                 // kW
  totalConsumptionToday: number;       // kWh
  peakDemandToday: number;             // kW
  avgLoadFactor: number;               // %
  estimatedCostToday: number;          // $
  enrolledCount: number;               // DR enrolled consumers
  availableFlexibility: number;        // kW
  currentCurtailment: number;          // kW
  drEventsYTD: number;
  consumerStatusSummary: Record<ConsumerStatus, number>;
}
```

---

### 12.6 GET `/monitoring/consumers/analytics/demand-profile`

Aggregated 24-hour demand profile.

**Response `200`:**
```typescript
{
  data: {
    time: string;
    demand: number;        // kW
    contracted: number;    // kW
  }[];
}
```

---

### 12.7 GET `/monitoring/consumers/analytics/consumption-by-type`

Consumption breakdown by consumer type.

**Response `200`:**
```typescript
{
  data: {
    type: string;            // "Commercial", "Industrial", "EV Charging", etc.
    consumption: number;     // kWh
  }[];
}
```

---

### 12.8 GET `/monitoring/consumers/analytics/monthly-consumption`

Monthly consumption for last 12 months.

**Response `200`:**
```typescript
{
  data: {
    month: string;
    consumption: number;     // MWh
  }[];
}
```

---

### 12.9 GET `/monitoring/consumers/analytics/peak-demand`

Daily peak demand for last 30 days.

**Response `200`:**
```typescript
{
  data: {
    date: string;
    peakDemand: number;      // kW
  }[];
}
```

---

### 12.10 GET `/monitoring/consumers/analytics/dr-summary`

Demand Response program summary.

**Response `200`:**
```typescript
{
  enrolledCount: number;
  availableFlexibility: number;      // kW
  currentCurtailment: number;        // kW
  drEventsYTD: number;
}
```

---

### 12.11 GET `/monitoring/consumers/analytics/dr-enrollment`

DR enrollment data per consumer.

**Response `200`:**
```typescript
{
  data: {
    id: string;
    name: string;
    type: string;
    drStatus: DRStatus;
    contractedCapacity: number;
    curtailableCapacity: number | null;
    currentCurtailment: number | null;
    drEventsYTD: number;
  }[];
}
```

---

### 12.12 GET `/monitoring/consumers/analytics/flexibility-by-type`

Curtailable flexibility breakdown by consumer type.

**Response `200`:**
```typescript
{
  data: {
    type: string;
    flexibility: number;     // kW
  }[];
}
```

---

### 12.13 GET `/monitoring/consumers/analytics/dr-events`

DR event history for last 12 months.

**Response `200`:**
```typescript
{
  data: {
    month: string;
    events: number;
  }[];
}
```

---

### 12.14 GET `/monitoring/consumers/analytics/load-heatmap`

Consumer load heatmap (24h x consumers).

**Response `200`:**
```typescript
{
  data: {
    consumer: string;
    hours: number[];         // 24 values, each 0-100% of contracted capacity
  }[];
}
```

---

## 13. Financials

All financial endpoints accept `?currency=USD` (default) or `?currency=TRY`. When `TRY` is specified, all monetary values are converted using the server-side exchange rate.

### 13.1 GET `/financials/kpi`

Financial KPIs.

**Response `200`:**
```typescript
{
  totalRevenue: number;
  totalCost: number;
  netIncome: number;
  grossMargin: number;        // %
  portfolioLCOE: number;      // $/kWh
  roi: number;                // %
  currency: Currency;
}
```

---

### 13.2 GET `/financials/revenue-streams`

Revenue streams with month-over-month comparison.

**Response `200`:**
```typescript
{
  data: RevenueStream[];
}

interface RevenueStream {
  type: RevenueStreamType;
  label: string;
  monthlyAmount: number;
  previousMonth: number;
  ytdAmount: number;
  description: string;
}

type RevenueStreamType =
  | "energy_arbitrage"
  | "demand_response"
  | "capacity_market"
  | "ancillary_services"
  | "feed_in_tariff"
  | "rec_carbon";
```

---

### 13.3 GET `/financials/revenue-breakdown`

Revenue breakdown for pie chart.

**Response `200`:**
```typescript
{
  data: {
    name: string;
    value: number;
  }[];
}
```

---

### 13.4 GET `/financials/cost-items`

Cost items with budget comparison.

**Response `200`:**
```typescript
{
  data: CostItem[];
}

interface CostItem {
  category: CostCategory;
  label: string;
  monthlyAmount: number;
  budgeted: number;
  variance: number;           // %
}

type CostCategory =
  | "operations"
  | "maintenance"
  | "grid_charges"
  | "depreciation"
  | "insurance"
  | "admin";
```

---

### 13.5 GET `/financials/tariff-periods`

Tariff period definitions.

**Response `200`:**
```typescript
{
  data: TariffPeriod[];
}

interface TariffPeriod {
  name: string;
  hours: string;
  rate: number;           // $/kWh
  rateTRY: number;        // TL/kWh
}
```

---

### 13.6 GET `/financials/hourly-price-profile`

24-hour price profile with period labels.

**Response `200`:**
```typescript
{
  data: {
    hour: string;
    rate: number;
    period: string;       // "Off-Peak", "Mid-Peak", "On-Peak", "Super-Peak"
  }[];
}
```

---

### 13.7 GET `/financials/real-time-pricing`

Real-time energy pricing for last 24 hours.

**Response `200`:**
```typescript
{
  data: {
    hour: string;
    price: number;        // $/MWh
  }[];
}
```

---

### 13.8 GET `/financials/peak-demand`

Peak demand with shaving profile (24 hours).

**Response `200`:**
```typescript
{
  data: {
    hour: string;
    demand: number;       // kW
    shaved: number;       // kW
  }[];
}
```

---

### 13.9 GET `/financials/arbitrage-profile`

BESS charge/discharge with price overlay (24 hours).

**Response `200`:**
```typescript
{
  data: {
    hour: string;
    chargeDischarge: number;  // kW (negative=charge, positive=discharge)
    price: number;            // $/MWh
  }[];
}
```

---

### 13.10 GET `/financials/asset-performance`

Financial performance per asset.

**Response `200`:**
```typescript
{
  data: AssetFinancial[];
}

interface AssetFinancial {
  type: AssetType;            // "solar" | "bess" | "wind" | "grid"
  name: string;
  capacity: string;
  capacityFactor: number;     // %
  monthlyProduction: number;  // MWh
  monthlyRevenue: number;
  monthlyCost: number;
  netIncome: number;
  lcoe: number;               // $/kWh
  roi: number;                // %
  investment: number;
  npv: number;
  irr: number;                // %
  paybackYears: number;
}
```

---

### 13.11 GET `/financials/transactions`

List transactions with filtering, sorting, and pagination.

**Query Parameters:**

| Param     | Type   | Description                                                            |
|-----------|--------|------------------------------------------------------------------------|
| `page`, `pageSize` | number | Pagination                                                   |
| `sortBy`  | string | `date`, `type`, `asset`, `amount`, `status`                           |
| `sortDir` | string | `asc` or `desc`                                                       |
| `type`    | string | Comma-separated: `energy_sale,energy_purchase,demand_response,capacity_payment,ancillary_service,rec_sale,carbon_credit,maintenance_cost,grid_charge` |
| `status`  | string | Comma-separated: `completed,pending,invoiced,overdue`                  |
| `from`, `to` | string | ISO 8601 date range                                                |
| `search`  | string | Search description, reference, or asset                               |

**Response `200`:**
```typescript
PaginatedResponse<Transaction>
```

```typescript
interface Transaction {
  id: string;
  date: string;                // ISO 8601
  type: TransactionType;
  asset: string;
  description: string;
  quantity: number;
  unitPrice: number;
  amount: number;              // positive=revenue, negative=expense
  status: TransactionStatus;   // "completed" | "pending" | "invoiced" | "overdue"
  reference: string;
  currency: Currency;
}
```

---

### 13.12 GET `/financials/monthly-pnl`

Monthly profit & loss for last 12 months.

**Response `200`:**
```typescript
{
  data: MonthlyPnL[];
}

interface MonthlyPnL {
  month: string;
  revenue: number;
  cost: number;
  netIncome: number;
  margin: number;             // %
}
```

---

### 13.13 GET `/financials/trends/revenue`

Monthly revenue trend broken down by stream.

**Response `200`:**
```typescript
{
  data: MonthlyRevenueTrend[];
}

interface MonthlyRevenueTrend {
  month: string;
  energy_arbitrage: number;
  demand_response: number;
  capacity_market: number;
  ancillary_services: number;
  feed_in_tariff: number;
  rec_carbon: number;
}
```

---

### 13.14 GET `/financials/trends/cost`

Monthly cost trend broken down by category.

**Response `200`:**
```typescript
{
  data: MonthlyCostTrend[];
}

interface MonthlyCostTrend {
  month: string;
  operations: number;
  maintenance: number;
  grid_charges: number;
  depreciation: number;
  insurance: number;
  admin: number;
}
```

---

### 13.15 GET `/financials/budget-vs-actual`

Monthly budget vs actual for last 12 months.

**Response `200`:**
```typescript
{
  data: BudgetVsActual[];
}

interface BudgetVsActual {
  month: string;
  budgeted: number;
  actual: number;
}
```

---

### 13.16 GET `/financials/projections`

Financial projections (3 scenarios: Base, Optimistic, Pessimistic).

**Response `200`:**
```typescript
{
  data: ProjectionScenario[];
}

interface ProjectionScenario {
  name: string;              // "Base", "Optimistic", "Pessimistic"
  months: ProjectionMonth[];
  npv: number;
  irr: number;               // %
  paybackYears: number;
}

interface ProjectionMonth {
  month: string;
  revenue: number;
  cost: number;
  netIncome: number;
  cumulative: number;
}
```

---

### 13.17 GET `/financials/transaction-summary`

Transaction summary aggregation.

**Response `200`:**
```typescript
{
  count: number;
  totalRevenue: number;
  totalExpense: number;
  net: number;
}
```

---

## 14. Shared Types Reference

### User

```typescript
interface User {
  id: string;
  name: string;
  email: string;
  avatar?: string;
  initials: string;
  role?: string;
  department?: string;
  phone?: string;
}
```

### Asset

```typescript
interface Asset {
  id: string;
  name: string;
  type: "solar" | "bess" | "grid" | "inverter" | "other";
  location?: string;
  capacity?: string;
  status?: "operational" | "maintenance" | "offline" | "warning";
  lastMaintenance?: string;    // ISO 8601
  serialNumber?: string;
}
```

### AlarmSeverity

```typescript
type AlarmSeverity = "critical" | "high" | "medium" | "low";
```

### FaultSeverity

```typescript
type FaultSeverity = "critical" | "major" | "minor" | "warning";
```

### Facility

```typescript
interface Facility {
  id: string;
  name: string;
  type: "solar" | "wind" | "bess" | "hybrid";
  location: string;
  country: string;
  capacity: number;   // MW
}
```

### Currency

```typescript
type Currency = "USD" | "TRY";
```

### SortDirection

```typescript
type SortDirection = "asc" | "desc";
```

---

## Endpoint Count Summary

| Domain            | Endpoints |
|-------------------|-----------|
| Auth              | 5         |
| Facilities        | 2         |
| Dashboard         | 14        |
| Alarms            | 7         |
| Notifications     | 5         |
| Maintenance       | 19        |
| BESS Monitoring   | 10        |
| Solar Inverters   | 13        |
| Grid Loads        | 14        |
| Wind Turbines     | 13        |
| Consumption       | 14        |
| Financials        | 17        |
| **Total**         | **133**   |
