# Maintenance Tasks API Documentation

**Base URL:** `/api/v1/MaintenanceTasks`
**Auth:** All endpoints require JWT Bearer token + role `Administrator` or `Tenant.Admin`

---

## Endpoints

| Method | Route | Permission | Description |
|--------|-------|------------|-------------|
| GET | `/` | `Maintenance.View` | Paginated task list with filters |
| GET | `/Calendar` | `Maintenance.View` | Tasks for a given month (calendar view) |
| GET | `/{id}` | `Maintenance.View` | Full task detail with sub-collections |
| POST | `/` | `Maintenance.Create` | Create a new task |
| POST | `/{id}/Status` | `Maintenance.Manage` | Change task status |
| POST | `/{id}/Complete` | `Maintenance.Manage` | Complete task with resolution details |
| POST | `/{id}/Close` | `Maintenance.Manage` | Close/cancel task with reason |
| POST | `/{id}/Notes` | `Maintenance.Manage` | Add a note |
| POST | `/{id}/Checklist` | `Maintenance.Manage` | Add a checklist item |
| PUT | `/{id}/Checklist/{itemId}` | `Maintenance.Manage` | Toggle checklist item completion |
| POST | `/{id}/WorkSessions/Start` | `Maintenance.Manage` | Start a work session timer |
| POST | `/{id}/WorkSessions/Stop` | `Maintenance.Manage` | Stop the active work session |

---

## Task Status Lifecycle

```
            ┌───────────┐
            │ Scheduled  │ (DueDate in the future at creation)
            └─────┬─────┘
                  │
                  ▼
            ┌───────────┐
   ┌───────►│  Pending   │ (DueDate in the past at creation)
   │        └─────┬─────┘
   │              │
   │              ▼
   │        ┌───────────┐
   │        │ InProgress │
   │        └─────┬─────┘
   │              │
   │         ┌────┴────┐
   │         ▼         ▼
   │   ┌──────────┐  ┌────────┐
   │   │ Completed│  │ Closed │◄── (from any non-terminal status)
   │   └──────────┘  └────────┘
   │
   │   ┌───────────┐
   └───│  Overdue   │ (set externally when past due)
       └───────────┘
```

### Valid Transitions

| From | To |
|------|----|
| `Scheduled` | `Pending`, `Closed` |
| `Pending` | `InProgress`, `Closed` |
| `InProgress` | `Completed` (via Complete endpoint), `Closed` |
| `Overdue` | `InProgress`, `Closed` |
| `Completed` | — (terminal) |
| `Closed` | — (terminal) |

---

## GET `/`

Paginated, filterable task list.

### Query Parameters

| Param | Type | Description |
|-------|------|-------------|
| `facilityId` | `Guid?` | Filter by facility |
| `status` | `string?` | Filter by status |
| `priority` | `string?` | Filter by priority (`Critical`, `High`, `Medium`, `Low`) |
| `assignedToId` | `string?` | Filter by assigned user ID |
| `dateFrom` | `DateTimeOffset?` | Tasks with DueDate >= this |
| `dateTo` | `DateTimeOffset?` | Tasks with DueDate <= this |
| `searchTerm` | `string?` | Search in title and description |
| `pageNumber` | `int` | Default: 1 |
| `pageSize` | `int` | Default: 20 |

### Response: `PaginatedList<MaintenanceTaskBriefDto>`

```json
{
  "items": [
    {
      "id": "guid",
      "title": "Replace faulty BMU sensor",
      "status": "InProgress",
      "priority": "High",
      "assignedToId": "user-id",
      "assignedToName": "John Doe",
      "assignedById": "admin-id",
      "assignedByName": "Admin User",
      "dueDate": "2026-03-10T00:00:00Z",
      "created": "2026-03-05T10:00:00Z",
      "deviceName": "BMU-01",
      "facilityName": "Demo Facility",
      "deviceId": "guid",
      "deviceAlarmId": "guid",
      "facilityId": "guid",
      "hasRelatedAlarm": true
    }
  ],
  "pageNumber": 1,
  "totalPages": 3,
  "totalCount": 42
}
```

---

## GET `/Calendar`

Returns tasks within a given month for calendar display.

### Query Parameters

| Param | Type | Required | Description |
|-------|------|----------|-------------|
| `month` | `int` | Yes | Month (1-12) |
| `year` | `int` | Yes | Year |
| `facilityId` | `Guid?` | No | Filter by facility |

### Response: `List<MaintenanceTaskBriefDto>`

---

## GET `/{id}`

Full task detail including all sub-collections.

### Response: `MaintenanceTaskDetailDto`

```json
{
  "...all MaintenanceTaskBriefDto fields...",
  "description": "Sensor readings are intermittent...",
  "resolution": "Replaced BMU temperature sensor",
  "rootCause": "Corroded connector",
  "recommendations": "Schedule quarterly connector inspections",
  "partsUsed": "BMU Temp Sensor v2 x1",
  "laborHours": 2.5,
  "closeReason": null,
  "closeExplanation": null,
  "completedAt": "2026-03-07T14:30:00Z",
  "closedAt": null,
  "completedByName": "John Doe",
  "closedByName": null,
  "relatedAlarm": {
    "id": "guid",
    "code": "BAT_TEMP_HIGH",
    "name": "High Temperature Warning",
    "severity": "High",
    "isActive": false,
    "occurredAt": "2026-03-05T09:15:00Z"
  },
  "notes": [
    {
      "id": "guid",
      "content": "Arrived on site, inspecting unit.",
      "authorName": "John Doe",
      "created": "2026-03-06T08:00:00Z"
    }
  ],
  "checklist": [
    {
      "id": "guid",
      "label": "Isolate BMU from power",
      "sortOrder": 0,
      "isCompleted": true,
      "completedAt": "2026-03-06T08:30:00Z",
      "completedByName": "John Doe"
    }
  ],
  "timeline": [
    {
      "id": "guid",
      "fromStatus": "InProgress",
      "toStatus": "Completed",
      "changedByName": "John Doe",
      "changedAt": "2026-03-07T14:30:00Z"
    }
  ],
  "workSessions": [
    {
      "id": "guid",
      "userName": "John Doe",
      "startedAt": "2026-03-06T08:00:00Z",
      "endedAt": "2026-03-06T12:00:00Z"
    }
  ]
}
```

---

## POST `/`

Create a new maintenance task.

### Request Body

```json
{
  "title": "Replace faulty BMU sensor",
  "description": "Sensor readings are intermittent...",
  "assignedToId": "user-id",
  "priority": "High",
  "dueDate": "2026-03-10T00:00:00Z",
  "facilityId": "facility-guid",
  "deviceId": "device-guid",
  "deviceAlarmId": "alarm-guid",
  "checklistItems": [
    "Isolate BMU from power",
    "Remove old sensor",
    "Install new sensor",
    "Verify readings"
  ]
}
```

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `title` | `string` | Yes | Max 200 chars |
| `description` | `string` | No | Max 2000 chars |
| `assignedToId` | `string` | Yes | Application user ID |
| `priority` | `string` | Yes | `Critical`, `High`, `Medium`, `Low` |
| `dueDate` | `DateTimeOffset` | Yes | |
| `facilityId` | `Guid` | Yes | Required for tenant scoping |
| `deviceId` | `Guid` | No | Related device |
| `deviceAlarmId` | `Guid` | No | Links task to alarm (sets alarm's `MaintenanceTaskId`) |
| `checklistItems` | `string[]` | No | Labels for initial checklist |

**Status logic:** `Scheduled` if DueDate > now, otherwise `Pending`.

**Response:** `201 Created` with task GUID

---

## POST `/{id}/Status`

Change task status. See valid transitions above.

### Request Body

```json
{
  "newStatus": "InProgress"
}
```

**Note:** Use `/Complete` to move to `Completed` status (requires resolution details).

**Response:** `204 No Content`

---

## POST `/{id}/Complete`

Complete a task with resolution details. Only from `InProgress` or `Overdue` status.

### Request Body

```json
{
  "resolution": "Replaced BMU temperature sensor",
  "rootCause": "Corroded connector",
  "laborHours": 2.5,
  "partsUsed": "BMU Temp Sensor v2 x1",
  "recommendations": "Schedule quarterly inspections"
}
```

| Field | Type | Required |
|-------|------|----------|
| `resolution` | `string` | Yes (max 2000) |
| `rootCause` | `string` | No (max 2000) |
| `laborHours` | `decimal` | No |
| `partsUsed` | `string` | No (max 1000) |
| `recommendations` | `string` | No (max 2000) |

**Side effect:** If linked to an alarm, the alarm is automatically resolved.

**Response:** `204 No Content`

---

## POST `/{id}/Close`

Close/cancel a task. Valid from any non-terminal status.

### Request Body

```json
{
  "reason": "Cancelled",
  "explanation": "Device was replaced entirely, making repair unnecessary."
}
```

| Field | Type | Required | Valid Values |
|-------|------|----------|-------------|
| `reason` | `string` | Yes | `Cancelled`, `Duplicate`, `NotNeeded`, `Transferred`, `Other` |
| `explanation` | `string` | No | Max 2000 chars |

**Side effect:** If linked to an alarm, the alarm is automatically resolved.

**Response:** `204 No Content`

---

## POST `/{id}/Notes`

Add a note to a task.

### Request Body

```json
{
  "content": "Arrived on site, beginning inspection."
}
```

**Response:** `201 Created` with note GUID

---

## POST `/{id}/Checklist`

Add a checklist item to a task.

### Request Body

```json
{
  "label": "Verify power isolation"
}
```

**Response:** `201 Created` with checklist item GUID

---

## PUT `/{id}/Checklist/{itemId}`

Toggle a checklist item's completion status.

### Request Body

```json
{
  "isCompleted": true
}
```

When `isCompleted: true`, sets `CompletedAt` and `CompletedById`. When `false`, clears them.

**Response:** `204 No Content`

---

## POST `/{id}/WorkSessions/Start`

Start a work session timer for the current user.

**Request body:** None

**Validation:** Only one open session per user per task.

**Response:** `201 Created` with session GUID

---

## POST `/{id}/WorkSessions/Stop`

Stop the current user's open work session.

**Request body:** None

**Response:** `204 No Content`

---

## Tenant Scoping

Tasks are tenant-scoped through: `MaintenanceTask → Facility → Firm → Tenant`. The `FacilityId` is required on creation and provides the tenant chain for query filters.

---

## Alarm Integration

- **Creating a task from an alarm:** Pass `deviceAlarmId` when creating. The alarm's `MaintenanceTaskId` is set to link the two.
- **Auto-resolution:** When a task with a linked alarm is completed or closed, the alarm is automatically resolved (`IsActive = false`).
- **One-to-one:** Each alarm can only be linked to one task (unique index on `DeviceAlarmId`).
