# Alarms API Documentation

**Base URL:** `/api/v1/Alarms`
**Auth:** All endpoints require JWT Bearer token + role `Administrator` or `Tenant.Admin`

---

## Endpoints

| Method | Route | Permission | Description |
|--------|-------|------------|-------------|
| GET | `/` | `Alarm.View` | Paginated alarm list with filters |
| GET | `/Summary` | `Alarm.View` | Active alarm counts by severity |
| GET | `/{id}` | `Alarm.View` | Single alarm detail |
| POST | `/{id}/Acknowledge` | `Alarm.Manage` | Acknowledge an active alarm |
| POST | `/{id}/Resolve` | `Alarm.Manage` | Manually resolve an active alarm |

---

## Alarm Status Model

Alarm status is **derived** from stored fields (lowercase strings):

| Status | Condition |
|--------|-----------|
| `active` | `IsActive == true` AND `AcknowledgedAt == null` |
| `acknowledged` | `IsActive == true` AND `AcknowledgedAt != null` |
| `resolved` | `IsActive == false` (either telemetry recovery or human resolution) |

Two separate resolution paths:
- **RecoveredAt** — set automatically by `AlarmProcessingService` when telemetry clears the fault
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
| `message` | `DeviceAlarmType.Description` | Human-readable alarm description |
| `severity` | `DeviceAlarmType.Severity` | Lowercased: `critical`, `high`, `warning`, `low`, `info` |
| `status` | Derived | `active`, `acknowledged`, `resolved` |
| `source` | `Device.Name` (fallback: `AlarmType.Source`) | Name of the device that triggered the alarm |
| `sourceType` | `DeviceAlarmType.Category` | Lowercased: `bess`, `inverter`, `solar`, etc. |
| `triggeredAt` | `DeviceAlarm.OccurredAt` | When the alarm was triggered |
| `value` | From `Details` JSON (future) | Current measured value; `null` if not available |
| `threshold` | From `Details` JSON (future) | Threshold that was exceeded; `null` if not available |
| `acknowledgedBy` | `ApplicationUser` | User object or `null` |
| `resolvedBy` | `ApplicationUser` | User object or `null` |
| `relatedAsset` | `Device` entity | Device info object or `null` |
| `relatedMaintenanceTaskId` | `DeviceAlarm.MaintenanceTaskId` | Linked task GUID or `null` |
| `location` | `Facility.Name` | Facility where the alarm occurred |

### User Object Shape

```json
{
  "id": "string",
  "name": "John Doe",
  "email": "john.doe@example.com",
  "initials": "JD",
  "role": "Operations Manager"
}
```

- `initials` is derived from `Name[0] + Surname[0]`
- `role` maps from `ApplicationUser.Title`

### Related Asset Object Shape

```json
{
  "id": "device-guid",
  "name": "BMU-01",
  "type": "bess",
  "location": "Facility Name",
  "status": "online",
  "serialNumber": "BESS-2024-001",
  "model": "DEMO-BG"
}
```

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

## POST `/{id}/Acknowledge`

Marks an active alarm as acknowledged by the current user.

**Request body:** None

**Validation:**
- Alarm must exist → `404`
- Alarm must be active → `400`
- Alarm must not already be acknowledged → `400`

**Response:** `204 No Content`

---

## POST `/{id}/Resolve`

Manually resolves an active alarm.

**Request body:** None

**Validation:**
- Alarm must exist → `404`
- Alarm must be active → `400`

**Response:** `204 No Content`

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
