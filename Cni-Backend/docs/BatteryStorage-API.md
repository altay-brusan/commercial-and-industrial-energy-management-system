# BatteryStorage API Documentation

**Base URL:** `/api/v1/BatteryStorage`
**Auth:** All endpoints require JWT Bearer token + role `Administrator` or `Tenant.Admin` + permission `Bess.View`

---

## Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/Tree/{facilityId}` | Hierarchical device tree for a facility |
| GET | `/Units/{facilityId}` | All BESS units for a facility with telemetry |
| GET | `/Units/{unitId}/Header` | Unit header info and available tabs |
| GET | `/Units/{unitId}/{tab}` | Tab content dispatch (see tab keys below) |

### Tab Keys

| Key | Response DTO | Query Params |
|-----|-------------|--------------|
| `summary` | `SummaryResponseDto` | — |
| `health` | `SummaryResponseDto` | — |
| `curves` | `ChartResponseDto` | `period`, `interval` |
| `faultflags` | `FaultFlagsResponseDto` | — |
| `faults` | `FaultsPaginatedResponseDto` | `page`, `pageSize`, `severity`, `isActive` |
| `settings` | `SummaryResponseDto` | — |

Unknown tab keys return `404 Not Found`. Tab matching is case-insensitive.

---

## 1. GET `/BatteryStorage/Units/{facilityId}`

List all BESS (Battery Energy Storage System) units for a facility with real-time telemetry and fleet aggregates.

### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `facilityId` | `Guid` | Yes | The facility to list units for. |

### Query Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `search` | `string` | No | Free-text search on unit name or serial number (case-insensitive). |
| `sortBy` | `string` | No | Sort field: `name`, `soc`, `power`, `status`. Default: `name`. |
| `sortDirection` | `string` | No | `asc` or `desc`. Default: `asc`. |

### Response — `BessUnitsResultDto`

```json
{
  "units": [
    {
      "id": "a1b2c3d4-...",
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
      "facilityId": "f5e6d7c8-...",
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

## 2. GET `/BatteryStorage/Units/{unitId}/Header`

Lightweight header info for a single BESS unit. Used to render the unit detail page header and tab navigation.

### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `unitId` | `Guid` | Yes | The BESS unit device ID. |

### Response — `BessUnitHeaderDto`

```json
{
  "id": "a1b2c3d4-...",
  "name": "BESS Unit 01",
  "serialNumber": "DEMO-BG0RS3P12S0001",
  "modelName": "DEMO-BG",
  "manufacturer": "CATL",
  "facilityId": "f5e6d7c8-...",
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
    "esc": [
      { "id": "11111111-...", "name": "ESC-01" }
    ],
    "hvac": [
      { "id": "22222222-...", "name": "Chiller-01" }
    ],
    "pcs": [
      { "id": "33333333-...", "name": "PCS-01" }
    ]
  },
  "lastSeenUtc": "2026-02-25T12:35:00Z"
}
```

### Field Descriptions

| Field | Description |
|-------|-------------|
| `availableTabs` | Ordered list of tabs the frontend should render. Each `key` maps to a `{tab}` value in the dispatch endpoint. |
| `connectedDevices` | Child devices grouped by display type (`esc`, `hvac`, `pcs`, `solar`, `meter`). Used for sub-device navigation/selectors. |

---

## 3. GET `/BatteryStorage/Units/{unitId}/Summary`

Dynamic key-value sections with real-time telemetry for a BESS unit. Sections are schema-driven from the device's `SpecificationTemplate` when available, otherwise built from hardcoded defaults.

### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `unitId` | `Guid` | Yes | The BESS unit device ID. |

### Response — `SummaryResponseDto`

The response is an array of `sections`. Each section has a `key`, `title`, layout hints, and a `fields` array of key-value pairs.

```json
{
  "sections": [
    {
      "key": "overview",
      "title": "Overview",
      "icon": "battery",
      "variant": "card",
      "columns": 3,
      "optional": false,
      "fields": [
        { "key": "soc", "label": "State of Charge", "value": 85.5, "type": "number", "unit": "%" },
        { "key": "soh", "label": "State of Health", "value": 98.0, "type": "number", "unit": "%" },
        { "key": "power", "label": "Power", "value": -33.2, "type": "number", "unit": "kW" }
      ]
    }
  ]
}
```

### Section Field Types

| Type | Description |
|------|-------------|
| `string` | Plain text value |
| `number` | Numeric value, check `unit` for display formatting |
| `boolean` | true/false toggle or indicator |
| `select` | Dropdown with `options` array |

### Notes
- Sections with `optional: true` may be absent if the device has no child of that type (e.g. no PCS child -> no PCS section).
- Sub-device pages (rack, pack, module, ESC, TMS) also use this endpoint — they return sections relevant to that device level.

---

## 4. GET `/BatteryStorage/Units/{unitId}/Curves`

Time-series chart data for a BESS unit. Returns chart definitions with series metadata and data points.

### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `unitId` | `Guid` | Yes | The BESS unit device ID. |

### Query Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `period` | `string` | No | `24h` | Time window. Values: `1h`, `6h`, `12h`, `24h`, `48h`, `7d`, `30d` |
| `interval` | `string` | No | auto | Sampling interval. Values: `1m`, `5m`, `15m`, `30m`, `1h`, `1d`. Auto-derived from period if omitted. |

### Auto-derived Intervals (when `interval` is omitted)

| Period | Default Interval |
|--------|-----------------|
| `1h` | `1m` |
| `6h` | `5m` |
| `12h` | `5m` |
| `24h` | `15m` |
| `48h` | `30m` |
| `7d` | `1h` |
| `30d` | `1d` |

### Response — `ChartResponseDto`

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

### Notes
- `data` arrays are sampled at the requested interval. Empty arrays indicate no telemetry in the time window.
- `xAxis.format` hint for frontend date formatting: `HH:mm` for intra-day, `MM/dd` for multi-day periods.
- Power values: negative = discharging, positive = charging.

---

## 5. GET `/BatteryStorage/Units/{unitId}/Health`

Battery health assessment with SoH trends, cycle life, thermal stress, and maintenance info.

### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `unitId` | `Guid` | Yes | The BESS unit device ID. |

### Response — `SummaryResponseDto`

Same `sections` structure as Summary. Typical sections: `healthOverview`, `sohTrend` (chart variant), `cycleLife`, `thermalStress`.

---

## 6. GET `/BatteryStorage/Units/{unitId}/Settings`

Device configuration and settings. Currently read-only — values sourced from device metadata and specification template.

### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `unitId` | `Guid` | Yes | The BESS unit device ID. |

### Response — `SummaryResponseDto`

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

### Notes
- Fields with `editable: true` indicate user-configurable values. **Update endpoint not yet implemented** — settings are currently read-only.
- Many values may be `null` if the device metadata doesn't contain the setting key.

---

## 7. GET `/BatteryStorage/Tree/{facilityId}`

Hierarchical device tree for all BESS units in a facility. Returns a recursive tree structure: Unit -> Battery/PCS/TMS -> Rack -> Pack -> Module.

### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `facilityId` | `Guid` | Yes | The facility to load the tree for. |

### Response — `BessDeviceTreeResponseDto`

```json
{
  "tree": [
    {
      "id": "a1b2c3d4-...",
      "name": "BESS Unit 01",
      "type": "unit",
      "status": "normal",
      "parentDeviceId": null,
      "alarmCount": 2,
      "children": [
        {
          "id": "b2c3d4e5-...",
          "name": "Battery Pack 1",
          "type": "battery",
          "status": "normal",
          "parentDeviceId": "a1b2c3d4-...",
          "alarmCount": 0,
          "children": []
        }
      ]
    }
  ]
}
```

---

## Common Types

### StatusBadgeDto

All status fields share this shape:

```json
{ "value": "online", "label": "Online", "color": "green" }
```

**Status values:** `online`, `offline`, `standby`, `normal`, `warning`, `fault`, `maintenance`
**Operating mode values:** `charging`, `discharging`, `idle`, `standby`, `fault`

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

### Error Responses

| Status | Condition |
|--------|-----------|
| `401` | Missing or invalid JWT token |
| `403` | Insufficient role or missing `Bess.View` permission |
| `404` | Unknown `tab` key, or `unitId`/`facilityId` not found |
| `400` | Invalid query parameter value |
