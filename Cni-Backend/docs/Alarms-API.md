# Alarms API Documentation

**Base URL:** `/api/v1/Alarms`
**Ingestion:** `POST /api/v1/DeviceData/Ingest/Alarm`
**Auth:** Query/mutation endpoints require JWT Bearer token + role `Administrator` or `Tenant.Admin`. Ingestion endpoint requires `X-API-Key` header.

---

## Alarm Flow

```
┌─────────────────────────────────────────────────────────────────────────┐
│                          ALARM INGESTION                                │
│                                                                         │
│  RTU / Edge Device                    Third-Party API Adapter           │
│       │                                      │                          │
│       │  MQTT → EMQX webhook                 │  HTTP poll               │
│       ▼                                      ▼                          │
│  POST /api/v1/DeviceData/Ingest/Alarm  (same endpoint)                 │
│       │                                                                 │
│       ├─ Resolve (rtuSn, deviceName) → Device                          │
│       ├─ For each alarm in payload:                                    │
│       │    ├─ Match code → DeviceAlarmType (auto-create if not found)  │
│       │    ├─ active=true  → Create DeviceAlarm if not exists          │
│       │    └─ active=false → Recover existing DeviceAlarm              │
│       ├─ Snapshot comparison: auto-recover DB alarms not in payload    │
│       └─ Store RtuSerialNumber + DeviceName on each DeviceAlarm        │
└─────────────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                        ALARM LIFECYCLE                                  │
│                                                                         │
│  ┌──────────┐    Acknowledge     ┌──────────────┐                      │
│  │  Active   │ ─────────────────▶│ Acknowledged  │                      │
│  └────┬─────┘   (user action)    └──────┬───────┘                      │
│       │                                  │                              │
│       ▼                                  ▼                              │
│  ┌──────────┐                    ┌────────────┐                        │
│  │Recovered │                    │  Resolved  │                        │
│  │(auto)    │                    │(user/maint)│                        │
│  └──────────┘                    └────────────┘                        │
│                                                                         │
│  Recovered: Ingest/Alarm reports active=false or snapshot auto-recover │
│  Resolved:  User calls PATCH /Resolve, or MaintenanceTask completes   │
└─────────────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                        FRONTEND QUERY                                   │
│                                                                         │
│  GET /api/v1/Alarms ──▶ AlarmDtoMapper.ToDto() ──▶ AlarmDto            │
│       │                                                                 │
│       ├─ code         ← DeviceAlarmType.Code                           │
│       ├─ message      ← Details.message → AlarmType.Description        │
│       ├─ severity     ← AlarmType.Severity (Critical→critical, etc.)  │
│       ├─ status       ← Derived (IsActive + AcknowledgedAt)            │
│       ├─ source       ← Device.Name → DeviceAlarm.DeviceName          │
│       ├─ sourceType   ← Device.DeviceType.Name                        │
│       ├─ value        ← Details.value                                  │
│       ├─ threshold    ← Details.threshold                              │
│       └─ location     ← Device.Rtu.Facility.Name                      │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## Endpoints

| Method | Route | Auth | Permission | Description |
|--------|-------|------|------------|-------------|
| POST | `/api/v1/DeviceData/Ingest/Alarm` | API Key | — | Ingest alarm data from RTU/adapter |
| GET | `/` | JWT | `Alarm.View` | Paginated alarm list with filters |
| GET | `/Summary` | JWT | `Alarm.View` | Active alarm counts by severity |
| GET | `/{id}` | JWT | `Alarm.View` | Single alarm detail |
| PATCH | `/{id}/Acknowledge` | JWT | `Alarm.Manage` | Acknowledge an active alarm |
| PATCH | `/{id}/Resolve` | JWT | `Alarm.Manage` | Manually resolve an active alarm |
| GET | `/Statistics` | JWT | `Alarm.View` | Total alarm counts by severity and source type |
| GET | `/History/{deviceName}` | JWT | `Alarm.View` | Alarm history for a specific device by name |
| POST | `/{id}/AssignToMaintenance` | JWT | `Alarm.Manage` | Create maintenance task from alarm |

---

## Alarm Ingestion

### POST `/api/v1/DeviceData/Ingest/Alarm`

Receives alarm data from RTUs (via EMQX webhook) or third-party API adapters. Uses snapshot comparison — any active alarm in the DB for this device that is NOT in the incoming payload is auto-recovered.

**Auth:** `X-API-Key` header (same key as telemetry ingestion)

**Request:**

```json
{
  "rtuSn": "RTU-BESS-001",
  "deviceName": "Battery1",
  "alarms": [
    {
      "code": "CellOverVoltage",
      "severity": "warning",
      "active": true,
      "message": "Cell voltage exceeded threshold on rack 2",
      "value": 3.72,
      "threshold": 3.65,
      "raisedAt": "2026-03-09T10:00:00Z"
    },
    {
      "code": "PcsOverTemp",
      "severity": "high",
      "active": false
    }
  ],
  "deviceLocalTime": "2026-03-09T10:05:00Z"
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `rtuSn` | string | Yes | RTU serial number that reported the alarm |
| `deviceName` | string | Yes | Device name (matched under the RTU) |
| `alarms` | array | Yes | List of alarm items |
| `deviceLocalTime` | ISO 8601 | No | Defaults to UTC now |

**Alarm item fields:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `code` | string | Yes | Alarm type code (e.g., `CellOverVoltage`). Maps to `DeviceAlarmType.Code`. Auto-created if not found. |
| `severity` | string | No | `critical`, `high`, `medium`, `low`. Used when auto-creating alarm type. |
| `active` | boolean | Yes | `true` = fault active, `false` = fault cleared |
| `message` | string | No | Human-readable description. Stored in `Details` JSON, used as alarm message in API responses. |
| `value` | number | No | Measured value that triggered the alarm |
| `threshold` | number | No | Threshold that was exceeded |
| `raisedAt` | ISO 8601 | No | When originally raised. Defaults to `deviceLocalTime` or UTC now. |

**Response:**

```json
{ "success": true, "created": 1, "recovered": 1 }
```

**Processing:**
1. Resolve `(rtuSn, deviceName)` → `Device` record (cached, 5-min TTL)
2. For each alarm, resolve `code` → `DeviceAlarmType` (cached, 30-min TTL). Auto-create if not found.
3. `active: true` — create `DeviceAlarm` if no active alarm exists for this `(DeviceId, DeviceAlarmTypeId)`
4. `active: false` — recover matching active alarm (`IsActive = false`, `RecoveredAt = now`)
5. **Snapshot comparison** — auto-recover any active DB alarm whose code is NOT in the payload
6. Store `RtuSerialNumber` and `DeviceName` on each `DeviceAlarm` for traceability

---

## Alarm Status Model

Alarm status is **derived** from stored fields (lowercase strings):

| Status | Condition |
|--------|-----------|
| `active` | `IsActive == true` AND `AcknowledgedAt == null` |
| `acknowledged` | `IsActive == true` AND `AcknowledgedAt != null` |
| `resolved` | `IsActive == false` (either auto-recovery or human resolution) |

Two separate resolution paths:
- **RecoveredAt** — set automatically by the alarm ingestion endpoint when the fault clears (RTU sends `active: false`, or snapshot comparison auto-recovery)
- **ResolvedAt / ResolvedBy** — set manually via Resolve endpoint, or auto when a linked MaintenanceTask completes/closes

---

## GET `/`

Paginated, filterable alarm list.

### Query Parameters

| Param | Type | Description |
|-------|------|-------------|
| `facilityId` | `Guid?` | Filter by facility |
| `severity` | `string?` | Filter by severity (case-insensitive: `critical`, `high`, `warning`, `low`, `info`) |
| `status` | `string?` | Filter by status (case-insensitive: `active`, `acknowledged`, `resolved`) |
| `deviceId` | `Guid?` | Filter by device |
| `isActive` | `bool?` | Filter by IsActive flag directly |
| `dateFrom` | `DateTimeOffset?` | Alarms triggered at or after this time |
| `dateTo` | `DateTimeOffset?` | Alarms triggered at or before this time |
| `searchTerm` | `string?` | Search in alarm type name, code, and device name |
| `page` | `int` | Page number (default: 1) |
| `pageSize` | `int` | Page size (default: 20) |

### Response

```json
{
  "alarms": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "code": "INV-DCBUS-ABN",
      "message": "DC bus voltage abnormal - inverter shut down",
      "severity": "critical",
      "status": "active",
      "source": "YEDEK4ESKi",
      "sourceType": "inverter",
      "triggeredAt": "2026-02-09T06:12:00Z",
      "value": null,
      "threshold": null,
      "acknowledgedAt": null,
      "acknowledgedBy": null,
      "resolvedAt": null,
      "resolvedBy": null,
      "relatedAsset": null,
      "relatedMaintenanceTaskId": null,
      "location": "TEST FIELD"
    },
    {
      "id": "...",
      "code": "BESS-TEMP-DRIFT",
      "message": "Temperature sensor showing inconsistent readings",
      "severity": "high",
      "status": "acknowledged",
      "source": "BMU-01",
      "sourceType": "bess",
      "triggeredAt": "2026-02-08T07:23:00Z",
      "value": null,
      "threshold": null,
      "acknowledgedAt": "2026-02-08T08:15:00Z",
      "acknowledgedBy": {
        "id": "user-id",
        "name": "John Doe",
        "email": "john.doe@example.com",
        "initials": "JD",
        "role": "Operations Manager"
      },
      "resolvedAt": null,
      "resolvedBy": null,
      "relatedAsset": {
        "id": "device-guid",
        "name": "BMU-01",
        "type": "bess",
        "location": "Demo Facility",
        "status": "online",
        "serialNumber": "BESS-2024-001",
        "model": "DEMO-BG"
      },
      "relatedMaintenanceTaskId": "task-guid",
      "location": "Demo Facility"
    }
  ],
  "pagination": {
    "page": 1,
    "pageSize": 50,
    "totalItems": 7,
    "totalPages": 1
  }
}
```

### Field Descriptions

| Field | Source | Notes |
|-------|--------|-------|
| `code` | `DeviceAlarmType.Code` | Alarm type code (e.g. `INV-DCBUS-ABN`) |
| `message` | `Details.message` → `DeviceAlarmType.Description` | Instance message from ingestion preferred, falls back to alarm type description |
| `severity` | `DeviceAlarmType.Severity` | Mapped: `Critical`→`critical`, `Major`→`high`, `Minor`→`medium`, `Warning`→`low` |
| `status` | Derived | `active`, `acknowledged`, `resolved` (see Status Model) |
| `source` | `Device.Name` → `DeviceAlarm.DeviceName` | Device name, falls back to denormalized column if device deleted |
| `sourceType` | `Device.DeviceType.Name` | Lowercased: `inverter`, `energystorage`, `solar`, etc. Falls back to `AlarmType.Category` |
| `triggeredAt` | `DeviceAlarm.OccurredAt` | When the alarm was first raised |
| `value` | `Details.value` | Measured value from ingestion payload; `null` if not provided |
| `threshold` | `Details.threshold` | Threshold from ingestion payload; `null` if not provided |
| `acknowledgedBy` | `ApplicationUser` | User object or `null` |
| `resolvedBy` | `ApplicationUser` | User object or `null` |
| `relatedAsset` | `Device` entity + `Device.Metadata` | Device info object with `capacity`, `lastMaintenance` from metadata JSON; `null` if device not found |
| `relatedMaintenanceTaskId` | `DeviceAlarm.MaintenanceTaskId` | Linked task GUID or `null` |
| `location` | `Device.Rtu.Facility.Name` | Facility name where the alarm occurred |

### User Object Shape

```json
{
  "id": "string",
  "name": "John Doe",
  "email": "john.doe@example.com",
  "initials": "JD",
  "role": "Operations Manager",
  "department": "Management",
  "phone": "+1 555-0105"
}
```

- `initials` is derived from `Name[0] + Surname[0]`
- `role` maps from `ApplicationUser.Title`
- `department` maps from `ApplicationUser.Department`
- `phone` maps from `IdentityUser.PhoneNumber`

### Related Asset Object Shape

```json
{
  "id": "device-guid",
  "name": "BMU-01",
  "type": "energystorage",
  "location": "Facility Name",
  "capacity": "2.5 MWh",
  "status": "online",
  "lastMaintenance": "2026-01-15T10:00:00Z",
  "serialNumber": "BESS-2024-001",
  "model": "DEMO-BG"
}
```

- `type` maps from `Device.DeviceType.Name` (lowercased), falls back to `AlarmType.Category`
- `capacity` and `lastMaintenance` are extracted from `Device.Metadata` JSON (optional fields)

---

## GET `/Statistics`

Total alarm counts across all statuses, broken down by severity and source type.

### Query Parameters

| Param | Type | Description |
|-------|------|-------------|
| `facilityId` | `Guid?` | Scope to a specific facility |

### Response

```json
{
    "totalAlarms": 7,
    "activeAlarms": 3,
    "criticalAlarms": 1,
    "highAlarms": 3,
    "mediumAlarms": 0,
    "lowAlarms": 0,
    "acknowledgedCount": 1,
    "resolvedCount": 3,
    "bySourceType": {
        "inverter": 5,
        "bess": 1,
        "solar": 1
    }
}
```

| Field | Description |
|-------|-------------|
| `totalAlarms` | Count of all alarms (active + acknowledged + resolved) |
| `activeAlarms` | Count of active, unacknowledged alarms |
| `criticalAlarms` | Count of alarms with severity `critical` |
| `highAlarms` | Count of alarms with severity `high` |
| `mediumAlarms` | Count of alarms with severity `warning` or `medium` |
| `lowAlarms` | Count of alarms with severity `low` |
| `acknowledgedCount` | Count of active, acknowledged alarms |
| `resolvedCount` | Count of resolved alarms |
| `bySourceType` | Alarm counts grouped by source type (lowercase category) |

---

## GET `/History/{deviceName}`

Returns all alarms for a specific device, identified by device name (not ID). Ordered by `triggeredAt` descending.

### Path Parameters

| Param | Type | Description |
|-------|------|-------------|
| `deviceName` | `string` | Device name (e.g., `YEDEK4ESKi`) |

### Response

```json
{
    "alarms": [
        {
            "id": "...",
            "code": "INV-DCBUS-ABN",
            "message": "DC bus voltage abnormal - inverter shut down",
            "severity": "critical",
            "status": "active",
            "source": "YEDEK4ESKi",
            "sourceType": "inverter",
            "triggeredAt": "2026-02-09T06:12:00Z",
            ...
        }
    ]
}
```

Same `AlarmDto` shape as the list endpoint items, wrapped in `{ alarms: [...] }`.

---

## GET `/Summary`

Counts of active alarms by severity.

### Query Parameters

| Param | Type | Description |
|-------|------|-------------|
| `facilityId` | `Guid?` | Scope to a specific facility |

### Response

```json
{
  "totalActive": 12,
  "critical": 2,
  "high": 4,
  "acknowledged": 3
}
```

---

## GET `/{id}`

Single alarm detail. Same shape as list items.

Returns `404` if alarm not found.

---

## PATCH `/{id}/Acknowledge`

Marks an active alarm as acknowledged by the current user.

**Request body:** None

**Validation:**
- Alarm must exist → `404`
- Alarm must be active → `400`
- Alarm must not already be acknowledged → `400`

**Response:** `200 OK` with the updated `AlarmDto` (same shape as list/detail items, with `acknowledgedAt` and `acknowledgedBy` populated, status changed to `acknowledged`)

---

## PATCH `/{id}/Resolve`

Manually resolves an active alarm.

**Request body:** None

**Validation:**
- Alarm must exist → `404`
- Alarm must be active → `400`

**Response:** `200 OK` with the updated `AlarmDto` (same shape as list/detail items, with `resolvedAt` and `resolvedBy` populated, status changed to `resolved`)

---

## POST `/{id}/AssignToMaintenance`

Creates a maintenance task from an active alarm and links them.

**Request body:**

```json
{
    "title": "Fix DC bus voltage abnormality",
    "description": "Investigate and resolve DC bus voltage issue on YEDEK4ESKi inverter.",
    "priority": "critical",
    "assignedToId": "user-2",
    "dueDate": "2026-03-08T17:00:00Z"
}
```

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `title` | `string` | Yes | Max 200 chars |
| `description` | `string?` | No | Max 2000 chars |
| `priority` | `string` | Yes | `critical`, `high`, `medium`, `low` |
| `assignedToId` | `string` | Yes | User ID to assign the task to |
| `dueDate` | `DateTimeOffset` | Yes | Task due date |

**Note:** `facilityId` is **not** in the request — it is derived from the alarm's device → RTU → facility chain.

**Validation:**
- Alarm must exist → `404`
- Alarm must be active → `400`
- Alarm must not already have a linked maintenance task → `400`
- Alarm's device must have a resolvable facility → `400`

**Response:** `200 OK`

```json
{
    "task": {
        "id": "task-guid",
        "title": "Fix DC bus voltage abnormality",
        "description": "Investigate and resolve...",
        "status": "pending",
        "priority": "critical",
        "assignedTo": { "id": "...", "name": "...", "email": "...", "initials": "...", "role": "...", "department": "...", "phone": "..." },
        "assignedBy": { "id": "...", "name": "...", "email": "...", "initials": "...", "role": "...", "department": "...", "phone": "..." },
        "assignedDate": "2026-03-05T13:51:38Z",
        "dueDate": "2026-03-08T17:00:00Z",
        "relatedAsset": null,
        "relatedAlarm": {
            "id": "alarm-guid",
            "code": "INV-DCBUS-ABN",
            "message": "DC bus voltage abnormal - inverter shut down",
            "severity": "critical",
            "source": "YEDEK4ESKi",
            "triggeredAt": "2026-02-09T06:12:00Z",
            "value": null,
            "threshold": null,
            "acknowledgedAt": null,
            "acknowledgedBy": null
        },
        "checklist": [],
        "workSessions": [],
        "completionDetails": null,
        "closeDetails": null,
        "deletedAt": null,
        "createdAt": "2026-03-05T13:51:38Z",
        "updatedAt": "2026-03-05T13:51:38Z"
    },
    "alarm": {
        "id": "alarm-guid",
        "code": "INV-DCBUS-ABN",
        "message": "DC bus voltage abnormal - inverter shut down",
        "severity": "critical",
        "status": "active",
        "source": "YEDEK4ESKi",
        "sourceType": "inverter",
        "triggeredAt": "2026-02-09T06:12:00Z",
        "value": null,
        "threshold": null,
        "acknowledgedAt": null,
        "acknowledgedBy": null,
        "resolvedAt": null,
        "resolvedBy": null,
        "relatedAsset": null,
        "relatedMaintenanceTaskId": "task-guid",
        "location": "TEST FIELD"
    }
}
```

---

## Alarm ↔ Maintenance Task Link

When a `MaintenanceTask` is created with a `deviceAlarmId`:
- The alarm's `MaintenanceTaskId` is set
- The alarm returns `relatedMaintenanceTaskId` with the task GUID

When a linked task is **completed** or **closed**:
- The alarm is automatically resolved (`IsActive = false`)

---

## Tenant Scoping

Alarms are tenant-scoped through: `DeviceAlarm → Device → Rtu → Facility → Firm → Tenant`. Global query filters auto-scope results. Administrators without a selected tenant see all alarms.
