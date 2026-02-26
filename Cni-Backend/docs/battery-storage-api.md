# Battery Storage API Reference

Base URL: `/api/v1/BatteryStorage`
Authorization: Bearer JWT token required on all endpoints.

---

## Endpoints Overview

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/Tree/{facilityId}` | Device tree for a facility |
| GET | `/Units/{facilityId}` | All BESS units for a facility |
| GET | `/Units/{unitId}/Header` | Unit header with available tabs |
| GET | `/Units/{unitId}/{tab}` | Tab content dispatch |

### Available tabs (via `{tab}` parameter)

| Tab key | Query | Description |
|---------|-------|-------------|
| `summary` | `GetBessSummaryQuery` | Dynamic key-value summary sections |
| `health` | `GetBessHealthQuery` | Health analytics with SoH trend |
| `curves` | `GetBessCurvesQuery` | Time-series chart data |
| `faultflags` | `GetBessFaultFlagsQuery` | Current fault flag states |
| `faults` | `GetBessFaultsQuery` | Paginated fault/alarm records |
| `settings` | `GetBessSettingsQuery` | Device configuration sections |

Unknown tab keys return `404 Not Found`.

---

## 1. GET /Tree/{facilityId}

Returns the full device tree for all BESS units in a facility.

### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `facilityId` | `Guid` | Yes | The facility to load the tree for. |

**Response** `BessDeviceTreeResponseDto`
```json
{
  "tree": [
    {
      "id": "guid",
      "name": "Megapack Alpha",
      "type": "unit",
      "status": "normal",
      "parentDeviceId": null,
      "alarmCount": 2,
      "children": [
        {
          "id": "guid",
          "name": "Battery Pack 1",
          "type": "battery",
          "status": "normal",
          "parentDeviceId": "guid",
          "alarmCount": 0,
          "children": [...]
        },
        {
          "id": "guid",
          "name": "PCS-01",
          "type": "pcs",
          "status": "normal",
          "parentDeviceId": "guid",
          "alarmCount": 0,
          "children": []
        }
      ]
    }
  ]
}
```

### Device Types

| `type` | Description |
|--------|-------------|
| `unit` | Root BESS container (EnergyStorage) |
| `battery` | Battery sub-system |
| `rack` | Battery rack |
| `pack` | Battery pack |
| `module` | Battery module |
| `esc` | Energy Storage Controller |
| `hvac` | Thermal Management System (chiller) |
| `pcs` | Power Conversion System |
| `solar` | Solar panel/inverter |
| `meter` | Smart meter |

### Notes
- `alarmCount` is recursive — a parent's count includes all descendant alarms.
- `status` values: `normal`, `warning`, `fault`. Derived from active alarm severity.
- Children are ordered by `hierarchyIndex` then `name`.

---

## 2. GET /Units/{facilityId}

Returns all BESS units for a facility with live telemetry and aggregates.

### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `facilityId` | `Guid` | Yes | The facility to list units for. |

### Query Parameters

| Param | Type | Description |
|-------|------|-------------|
| `search` | string? | Filter by name or serial number |
| `sortBy` | string? | `name`, `soc`, `power`, `status` |
| `sortDirection` | string? | `asc` or `desc` |

**Response** `BessUnitsResultDto`
```json
{
  "units": [
    {
      "id": "guid",
      "name": "BESS Unit 01",
      "status": { "value": "online", "label": "Online", "color": "green" },
      "operatingMode": { "value": "discharging", "label": "Discharging", "color": "purple" },
      "soc": 85.5,
      "soh": 98.0,
      "power": -33.2,
      "capacityKwh": 258.67,
      "activeAlarmCount": 0,
      "brand": "CATL",
      "model": "DEMO-BG",
      "serialNumber": "DEMO-BG0RS3P12S0001",
      "facilityName": "Sofia Facility",
      "facilityId": "guid",
      "temperatureC": 22.0,
      "lastSeenUtc": "2026-02-25T12:35:00Z"
    }
  ],
  "aggregates": {
    "totalUnits": 4,
    "totalCapacityKwh": 1034.68,
    "averageSoc": 72.3,
    "totalActiveAlarms": 2,
    "onlineCount": 3,
    "offlineCount": 0,
    "faultCount": 1,
    "standbyCount": 0
  }
}
```

### Field Descriptions

| Field | Description |
|-------|-------------|
| `status` | Device health status badge — `online`, `offline`, `standby`, `fault`, `normal` |
| `operatingMode` | BMS operating state — `charging`, `discharging`, `idle`, `standby`, `fault` |
| `soc` | State of Charge (%). `null` if no telemetry. |
| `soh` | State of Health (%). `null` if no telemetry. |
| `power` | Instantaneous DC power (kW). Negative = discharging, positive = charging. |
| `capacityKwh` | Available energy capacity in kWh. |
| `activeAlarmCount` | Count of active (unrecovered) alarms across all devices in the unit's RTU. |
| `lastSeenUtc` | Timestamp of last received telemetry from the device. |

---

## 3. GET /Units/{unitId}/Header

Lightweight header info for a single BESS unit. Used to render the unit detail page header and tab navigation.

### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `unitId` | `Guid` | Yes | The BESS unit device ID. |

**Response** `BessUnitHeaderDto`
```json
{
  "id": "guid",
  "name": "BESS Unit 01",
  "serialNumber": "DEMO-BG0RS3P12S0001",
  "modelName": "DEMO-BG",
  "manufacturer": "CATL",
  "facilityId": "guid",
  "facilityName": "Sofia Facility",
  "status": { "value": "online", "label": "Online", "color": "green" },
  "operatingMode": { "value": "discharging", "label": "Discharging", "color": "purple" },
  "soc": 85.5,
  "soh": 98.0,
  "power": -33.2,
  "capacityKwh": 258.67,
  "activeAlarmCount": 0,
  "availableTabs": [
    { "key": "summary", "label": "Summary", "isEnabled": true, "order": 0 },
    { "key": "health", "label": "Health", "isEnabled": true, "order": 1 },
    { "key": "curves", "label": "Curves", "isEnabled": true, "order": 2 },
    { "key": "faultFlags", "label": "Fault Flags", "isEnabled": true, "order": 3 },
    { "key": "faults", "label": "Faults", "isEnabled": true, "order": 4 },
    { "key": "settings", "label": "Settings", "isEnabled": true, "order": 5 }
  ],
  "connectedDevices": {
    "esc": [{ "id": "guid", "name": "ESC-01" }],
    "hvac": [{ "id": "guid", "name": "Chiller-01" }],
    "pcs": [{ "id": "guid", "name": "PCS-01" }]
  },
  "lastSeenUtc": "2026-02-25T12:35:00Z"
}
```

### Field Descriptions

| Field | Description |
|-------|-------------|
| `availableTabs` | Ordered list of tabs the frontend should render. Each `key` maps to a `{tab}` value in the dispatch endpoint. |
| `connectedDevices` | Child devices grouped by display type (`esc`, `hvac`, `pcs`, `solar`, `meter`). Used for sub-device navigation. |

---

## 4. GET /Units/{unitId}/{tab}

Unified dispatch endpoint — returns content for the requested tab. The response DTO varies by tab.

### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `unitId` | `Guid` | Yes | The BESS unit device ID. |
| `tab` | `string` | Yes | Tab key (case-insensitive). See table above. |

### Query Parameters (tab-specific)

| Param | Type | Used by | Default | Description |
|-------|------|---------|---------|-------------|
| `period` | `string` | `curves` | `24h` | Time window: `1h`, `6h`, `12h`, `24h`, `48h`, `7d`, `30d` |
| `interval` | `string` | `curves` | auto | Sampling interval: `1m`, `5m`, `15m`, `30m`, `1h`, `1d` |
| `page` | `int` | `faults` | `1` | Page number |
| `pageSize` | `int` | `faults` | `20` | Items per page |
| `severity` | `string` | `faults` | | Filter: `critical`, `major`, `minor`, `warning`, `info` |
| `isActive` | `bool` | `faults` | | Filter by active/recovered status |

### Tab: `summary`

**Response** `SummaryResponseDto`
```json
{
  "sections": [
    {
      "key": "overview",
      "title": "Overview",
      "icon": "battery",
      "variant": "card",
      "columns": 3,
      "fields": [
        { "key": "soc", "label": "State of Charge", "value": 85.5, "type": "number", "unit": "%" },
        { "key": "soh", "label": "State of Health", "value": 98.0, "type": "number", "unit": "%" },
        { "key": "power", "label": "Power", "value": -33.2, "type": "number", "unit": "kW" }
      ]
    }
  ]
}
```

### Tab: `health`

**Response** `SummaryResponseDto` — same wrapper as summary. Typical sections: `healthOverview`, `sohTrend` (chart variant), `cycleLife`, `thermalStress`.

### Tab: `curves`

**Response** `ChartResponseDto`
```json
{
  "charts": [
    {
      "key": "voltage_current",
      "title": "Voltage & Current",
      "type": "line",
      "xAxis": { "key": "time", "label": "Time", "format": "HH:mm" },
      "series": [
        { "key": "voltage", "label": "Voltage", "unit": "V", "color": "#3b82f6", "yAxis": "left" },
        { "key": "current", "label": "Current", "unit": "A", "color": "#ef4444", "yAxis": "right" }
      ],
      "data": [
        { "time": "2026-02-25T00:00:00Z", "voltage": 807.1, "current": -38.9 }
      ]
    }
  ]
}
```

### Tab: `faultflags`

**Response** `FaultFlagsResponseDto`
```json
{
  "flags": [
    { "key": "faultStatus", "label": "Fault Status", "isActive": false, "source": "battery", "category": "general", "severity": "Critical" }
  ],
  "sources": [
    { "key": "battery", "label": "Battery", "activeCount": 0, "totalCount": 5 }
  ],
  "categories": [
    { "key": "general", "label": "General", "activeCount": 0, "totalCount": 3 }
  ],
  "summary": { "totalFlags": 10, "activeFlags": 0, "activeBySource": {}, "activeByCategory": {} }
}
```

### Tab: `faults`

**Response** `FaultsPaginatedResponseDto`
```json
{
  "faults": [
    {
      "id": "guid",
      "code": "BMS-0101",
      "name": "Cell Over Voltage",
      "severity": { "value": "major", "label": "Major", "color": "orange" },
      "status": { "value": "recovered", "label": "Recovered", "color": "grey" },
      "source": "Rack 1 / Pack 2 / Module 3",
      "occurredAt": "2025-02-08T14:32:00Z",
      "recoveredAt": "2025-02-08T14:45:00Z",
      "relatedAlarmId": "guid"
    }
  ],
  "pagination": { "page": 1, "pageSize": 20, "totalItems": 6, "totalPages": 1 }
}
```

### Tab: `settings`

**Response** `SummaryResponseDto`
```json
{
  "sections": [
    {
      "key": "alarmThresholds",
      "title": "Alarm Thresholds",
      "variant": "card",
      "columns": 2,
      "fields": [
        { "key": "tempAlarmThreshold", "label": "Temperature Alarm", "value": 45, "type": "number", "unit": "°C", "editable": true }
      ]
    }
  ]
}
```

---

## Section Field Types

| Type | Description |
|------|-------------|
| `string` | Text value |
| `number` | Numeric value, may have `unit` |
| `boolean` | True/false, format `badge` shows colored label |
| `select` | Dropdown (Settings only), has `options` list |

## Section Variants

| Variant | Description |
|---------|-------------|
| `card` | Standard card layout |
| `highlighted` | Emphasized section (e.g., nameplate) |
| `table` | Contains `tableData` with columns/rows |
| `chart` | Contains `chartConfig` + `chartData` |

---

## Common Types

### StatusBadgeDto
```json
{ "value": "online", "label": "Online", "color": "green" }
```

**Color mapping:**

| Value | Color |
|-------|-------|
| online / normal | green |
| charging | green / blue |
| discharging | purple / cyan |
| idle | gray |
| standby | yellow |
| warning / maintenance | orange |
| offline / fault | red |

### Authorization
All endpoints require:
- Role: `Administrator` or `Tenant.Admin`
- Permission: `Bess.View`

### Error Responses

| Status | Condition |
|--------|-----------|
| `401` | Missing or invalid JWT token |
| `403` | Insufficient role or missing `Bess.View` permission |
| `404` | Unknown `tab` key, or `unitId`/`facilityId` not found |
| `400` | Invalid query parameter value |
