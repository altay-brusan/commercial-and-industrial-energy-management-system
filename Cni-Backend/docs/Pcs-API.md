# PCS (Power Conversion System) API Documentation

**Base URL:** `/api/v1/Pcs`
**Auth:** All endpoints require JWT Bearer token + role `Administrator` or `Tenant.Admin` + permission `Pcs.View`

---

## Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/Units/{facilityId}` | All PCS units for a facility with telemetry |
| GET | `/Units/{unitId}/Header` | Unit header info and available tabs |
| GET | `/Units/{unitId}/{tab}` | Tab content dispatch (see tab keys below) |
| GET | `/Schema` | Capability manifest for PCS product types |

### Tab Keys

| Key | Response DTO | Query Params |
|-----|-------------|--------------|
| `general` | `PcsGeneralDto` | — |
| `fault` | `FaultFlagsResponseDto` | — |
| `curve` | `ChartResponseDto` | `period`, `interval` |

Unknown tab keys return `404 Not Found`. Tab matching is case-insensitive.

---

## 1. GET `/Pcs/Units/{facilityId}`

List all PCS units for a facility with real-time telemetry and fleet aggregates.

### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `facilityId` | `Guid` | Yes | The facility to list PCS units for. |

### Response — `PcsUnitsResultDto`

```json
{
  "units": [
    {
      "id": "a1b2c3d4-...",
      "name": "PCS-01",
      "serialNumber": "PCS-SN-001",
      "modelName": "SungrowSC500HX",
      "manufacturer": "Sungrow",
      "facilityName": "Sofia Facility",
      "facilityId": "f5e6d7c8-...",
      "status": { "value": "online", "label": "Online", "color": "green" },
      "linkedBessUnit": { "id": "guid", "name": "BESS Unit 01" },
      "gridMode": "on-grid",
      "activePowerKw": -33.0,
      "reactivePowerKvar": -0.3,
      "gridFreqHz": 50.05,
      "igbtTempC": 42.5,
      "efficiencyPct": 97.8,
      "powerFactor": 0.99,
      "activeFaultCount": 0,
      "lastSeenUtc": "2026-02-25T12:35:00Z"
    }
  ],
  "aggregates": {
    "totalUnits": 2,
    "onlineCount": 2,
    "faultCount": 0,
    "totalActivePowerKw": -66.0,
    "totalReactivePowerKvar": -0.6,
    "avgEfficiencyPct": 97.8,
    "totalActiveFaults": 0
  }
}
```

### Field Descriptions

| Field | Description |
|-------|-------------|
| `status` | Device health status badge — `online`, `offline`, `standby`, `fault` |
| `linkedBessUnit` | The parent BESS unit this PCS is connected to. `null` if standalone. |
| `gridMode` | Grid connection mode — `on-grid`, `off-grid`, `island` |
| `activePowerKw` | AC active power (kW). Negative = exporting. |
| `reactivePowerKvar` | AC reactive power (kVar). |
| `gridFreqHz` | Grid frequency (Hz). |
| `igbtTempC` | IGBT module temperature (°C). |
| `efficiencyPct` | DC-to-AC conversion efficiency (%). |
| `powerFactor` | Power factor (0–1). |
| `activeFaultCount` | Count of active (unrecovered) fault alarms. |

---

## 2. GET `/Pcs/Units/{unitId}/Header`

Lightweight header info for a single PCS unit. Used to render the unit detail page header and tab navigation.

### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `unitId` | `Guid` | Yes | The PCS device ID. |

### Response — `PcsHeaderDto`

```json
{
  "id": "a1b2c3d4-...",
  "deviceId": "PCS-01",
  "name": "PCS-01",
  "status": { "value": "online", "label": "Online", "color": "green" },
  "linkedBessUnit": { "id": "guid", "name": "BESS Unit 01" },
  "availableTabs": [
    { "key": "general", "label": "General", "isEnabled": true, "order": 0 },
    { "key": "fault", "label": "Fault Flags", "isEnabled": true, "order": 1 },
    { "key": "curve", "label": "Curves", "isEnabled": true, "order": 2 }
  ]
}
```

### Field Descriptions

| Field | Description |
|-------|-------------|
| `availableTabs` | Ordered list of tabs. Each `key` maps to the `{tab}` value in the dispatch endpoint. |
| `linkedBessUnit` | The parent BESS unit. `null` if standalone PCS. |

---

## 3. GET `/Pcs/Units/{unitId}/{tab}`

Unified dispatch endpoint — returns content for the requested tab. The response DTO varies by tab.

### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `unitId` | `Guid` | Yes | The PCS device ID. |
| `tab` | `string` | Yes | Tab key (case-insensitive). See table above. |

### Query Parameters (tab-specific)

| Param | Type | Used by | Default | Description |
|-------|------|---------|---------|-------------|
| `period` | `string` | `curve` | `24h` | Time window: `1h`, `6h`, `12h`, `24h`, `48h`, `7d`, `30d` |
| `interval` | `string` | `curve` | auto | Sampling interval: `1m`, `5m`, `15m`, `30m`, `1h`, `1d` |

### Tab: `general`

**Response** `PcsGeneralDto` — dynamic key-value sections with real-time PCS telemetry (AC/DC power, grid, temperature, etc.)

### Tab: `fault`

**Response** `FaultFlagsResponseDto` — current fault flag states from DeviceAlarm system.

```json
{
  "flags": [
    { "key": "faultStatus", "label": "Fault Status", "isActive": false, "source": "pcs", "category": "general", "severity": "Critical" }
  ],
  "sources": [...],
  "categories": [...],
  "summary": { "totalFlags": 5, "activeFlags": 0 }
}
```

### Tab: `curve`

**Response** `ChartResponseDto` — time-series chart data for PCS monitoring curves (power, voltage, temperature, etc.)

```json
{
  "charts": [
    {
      "key": "ac_power",
      "title": "AC Power",
      "type": "line",
      "xAxis": { "key": "time", "label": "Time", "format": "HH:mm" },
      "series": [
        { "key": "activePower", "label": "Active Power", "unit": "kW", "color": "#8b5cf6" },
        { "key": "reactivePower", "label": "Reactive Power", "unit": "kVar", "color": "#f59e0b" }
      ],
      "data": [
        { "time": "2026-02-25T00:00:00Z", "activePower": -33.0, "reactivePower": -0.3 }
      ]
    }
  ]
}
```

---

## 4. GET `/Pcs/Schema`

Returns capability manifest for all PCS product types (from `DeviceSpecificationTemplate` entity).

### Response — `SchemaResponseDto`

```json
{
  "productTypes": [
    {
      "key": "sungrow-sc500hx",
      "label": "Sungrow SC500HX",
      "supportedTabs": ["general", "fault", "curve"],
      "faultSources": ["pcs"],
      "curveCharts": ["ac_power", "dc_power", "temperature"]
    }
  ]
}
```

---

## Common Types

### StatusBadgeDto

```json
{ "value": "online", "label": "Online", "color": "green" }
```

### Error Responses

| Status | Condition |
|--------|-----------|
| `401` | Missing or invalid JWT token |
| `403` | Insufficient role or missing `Pcs.View` permission |
| `404` | Unknown `tab` key, or `unitId`/`facilityId` not found |
| `400` | Invalid query parameter value |
