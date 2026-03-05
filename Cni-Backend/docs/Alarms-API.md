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

Alarm status is **derived** from stored fields, not stored directly:

| Status | Condition |
|--------|-----------|
| `Active` | `IsActive == true` AND `AcknowledgedAt == null` |
| `Acknowledged` | `IsActive == true` AND `AcknowledgedAt != null` |
| `Resolved` | `IsActive == false` (either telemetry recovery or human resolution) |

Two separate resolution paths exist:
- **RecoveredAt** — set automatically by `AlarmProcessingService` when a telemetry payload clears the fault flag
- **ResolvedAt / ResolvedById** — set manually via the Resolve endpoint, or automatically when a linked MaintenanceTask is completed/closed

---

## GET `/`

Paginated, filterable alarm list. All parameters are optional.

### Query Parameters

| Param | Type | Description |
|-------|------|-------------|
| `facilityId` | `Guid?` | Filter by facility |
| `severity` | `string?` | Filter by alarm type severity (`Critical`, `High`, `Warning`, `Info`) |
| `status` | `string?` | Filter by derived status (`Active`, `Acknowledged`, `Resolved`) |
| `deviceId` | `Guid?` | Filter by device |
| `isActive` | `bool?` | Filter by IsActive flag directly |
| `dateFrom` | `DateTimeOffset?` | Alarms occurred at or after this time |
| `dateTo` | `DateTimeOffset?` | Alarms occurred at or before this time |
| `searchTerm` | `string?` | Search in alarm type name, code, and device name |
| `pageNumber` | `int` | Page number (default: 1) |
| `pageSize` | `int` | Page size (default: 20) |

### Response: `PaginatedList<AlarmDto>`

```json
{
  "items": [
    {
      "id": "guid",
      "deviceId": "guid",
      "deviceAlarmTypeId": "guid",
      "code": "BAT_CELL_OV_WARN",
      "name": "Cell Over Voltage Warning",
      "message": "Battery cell voltage exceeded warning threshold",
      "severity": "Warning",
      "status": "Active",
      "source": "BMS",
      "category": "Battery",
      "deviceName": "BMU-01",
      "deviceModel": "DEMO-BG",
      "facilityName": "Demo Facility",
      "facilityId": "guid",
      "occurredAt": "2026-03-05T10:30:00Z",
      "recoveredAt": null,
      "acknowledgedAt": null,
      "resolvedAt": null,
      "acknowledgedByName": null,
      "resolvedByName": null,
      "maintenanceTaskId": null,
      "hasMaintenanceTask": false
    }
  ],
  "pageNumber": 1,
  "totalPages": 5,
  "totalCount": 95,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

---

## GET `/Summary`

Counts of active alarms by severity.

### Query Parameters

| Param | Type | Description |
|-------|------|-------------|
| `facilityId` | `Guid?` | Scope to a specific facility |

### Response: `AlarmSummaryDto`

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

Full alarm detail including metadata.

### Response: `AlarmDetailDto`

Extends `AlarmDto` with:

```json
{
  "...all AlarmDto fields...",
  "details": "{\"raw\": \"json payload\"}",
  "isActive": true,
  "isRead": false,
  "acknowledgedById": null,
  "resolvedById": null,
  "created": "2026-03-05T10:30:00Z"
}
```

Returns `404` if alarm not found.

---

## POST `/{id}/Acknowledge`

Marks an active alarm as acknowledged by the current user.

**Request body:** None

**Validation:**
- Alarm must exist → `404`
- Alarm must be active → `400`
- Alarm must not already be acknowledged → `400`

**Side effects:**
- Sets `AcknowledgedAt = now`, `AcknowledgedById = current user`

**Response:** `204 No Content`

---

## POST `/{id}/Resolve`

Manually resolves an active alarm.

**Request body:** None

**Validation:**
- Alarm must exist → `404`
- Alarm must be active → `400`

**Side effects:**
- Sets `IsActive = false`, `ResolvedAt = now`, `ResolvedById = current user`

**Response:** `204 No Content`

---

## Alarm ↔ Maintenance Task Link

When a `MaintenanceTask` is created with a `DeviceAlarmId`:
- The alarm's `MaintenanceTaskId` is set, linking the two entities
- The alarm DTO returns `hasMaintenanceTask: true` and the `maintenanceTaskId`

When a linked task is **completed** or **closed**:
- The alarm is automatically resolved (`IsActive = false`, `ResolvedAt/By` set)

---

## Tenant Scoping

Alarms are tenant-scoped through the chain: `DeviceAlarm → Device → Rtu → Facility → Firm → Tenant`. The global query filter automatically restricts results to the current user's tenant. Administrators without a selected tenant see all alarms.
