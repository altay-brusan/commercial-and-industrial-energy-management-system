# BatteryStorage API Documentation

**Base URL:** `/api/v1/BatteryStorage`
**Auth:** All endpoints require JWT Bearer token + role `Administrator` or `Tenant.Admin` + permission `Bess.View`

---

## 1. GET `/BatteryStorage/Units`

List all BESS (Battery Energy Storage System) units with real-time telemetry and fleet aggregates.

### Query Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `facilityId` | `Guid` | No | Filter units by facility. Omit to return all facilities. |
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

## 2. GET `/BatteryStorage/Units/Header/{unitId}`

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
    { "key": "notifications", "label": "Notifications", "isEnabled": true, "order": 4 },
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
| `availableTabs` | Ordered list of tabs the frontend should render. Derived from the device's `SpecificationTemplate`. |
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
        { "key": "power", "label": "Power", "value": -33.2, "type": "number", "unit": "kW" },
        { "key": "voltage", "label": "Voltage", "value": 793.6, "type": "number", "unit": "V" },
        { "key": "current", "label": "Current", "value": -30.7, "type": "number", "unit": "A" },
        { "key": "state", "label": "Operating State", "value": "Discharge", "type": "string" },
        { "key": "cycleCount", "label": "Cycle Count", "value": 145, "type": "number" },
        { "key": "remainingEnergy", "label": "Remaining Energy", "value": 186.24, "type": "number", "unit": "kWh" },
        { "key": "availableEnergy", "label": "Available Energy", "value": 258.67, "type": "number", "unit": "kWh" }
      ],
      "tableData": null,
      "chartConfig": null,
      "chartData": null
    },
    {
      "key": "cellExtremes",
      "title": "Cell Extremes",
      "icon": null,
      "variant": "card",
      "columns": 2,
      "optional": false,
      "fields": [
        { "key": "maxCellVoltage", "label": "Max Cell Voltage", "value": 3315, "type": "number", "unit": "mV" },
        { "key": "minCellVoltage", "label": "Min Cell Voltage", "value": 3299, "type": "number", "unit": "mV" },
        { "key": "avgCellVoltage", "label": "Avg Cell Voltage", "value": 3308, "type": "number", "unit": "mV" },
        { "key": "maxCellTemp", "label": "Max Cell Temp", "value": 25, "type": "number", "unit": "°C" },
        { "key": "minCellTemp", "label": "Min Cell Temp", "value": 21, "type": "number", "unit": "°C" },
        { "key": "ambientTemp", "label": "Ambient Temp", "value": 22, "type": "number", "unit": "°C" }
      ]
    },
    {
      "key": "pcs",
      "title": "PCS",
      "icon": null,
      "variant": "card",
      "columns": 3,
      "optional": true,
      "fields": [
        { "key": "activePower", "label": "Active Power", "value": -33.0, "type": "number", "unit": "kW" },
        { "key": "apparentPower", "label": "Apparent Power", "value": 33.0, "type": "number", "unit": "kVA" },
        { "key": "reactivePower", "label": "Reactive Power", "value": -0.3, "type": "number", "unit": "kVar" },
        { "key": "gridFrequency", "label": "Grid Frequency", "value": 50.05, "type": "number", "unit": "Hz" },
        { "key": "dcPower", "label": "DC Power", "value": -32.6, "type": "number", "unit": "kW" }
      ]
    },
    {
      "key": "chiller",
      "title": "Thermal Management",
      "icon": null,
      "variant": "card",
      "columns": 3,
      "optional": true,
      "fields": [
        { "key": "operatingMode", "label": "Operating Mode", "value": "Off", "type": "string" },
        { "key": "outletTemp", "label": "Outlet Coolant Temp", "value": -40, "type": "number", "unit": "°C" },
        { "key": "inletTemp", "label": "Inlet Coolant Temp", "value": -40, "type": "number", "unit": "°C" },
        { "key": "compressorStatus", "label": "Compressor", "value": false, "type": "boolean" },
        { "key": "pumpStatus", "label": "Pump", "value": false, "type": "boolean" }
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
- Sections with `optional: true` may be absent if the device has no child of that type (e.g. no PCS child → no PCS section).
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
        { "time": "2026-02-25T00:00:00Z", "voltage": 807.1, "current": -38.9 },
        { "time": "2026-02-25T00:15:00Z", "voltage": 805.3, "current": -37.2 },
        { "time": "2026-02-25T00:30:00Z", "voltage": 803.8, "current": -36.1 }
      ]
    },
    {
      "key": "power",
      "title": "Power",
      "type": "line",
      "xAxis": { "key": "time", "label": "Time", "format": "HH:mm" },
      "series": [
        { "key": "power", "label": "Power", "unit": "kW", "color": "#8b5cf6" }
      ],
      "data": [
        { "time": "2026-02-25T00:00:00Z", "power": -31.4 },
        { "time": "2026-02-25T00:15:00Z", "power": -30.0 },
        { "time": "2026-02-25T00:30:00Z", "power": -29.1 }
      ]
    },
    {
      "key": "temperature",
      "title": "Temperature",
      "type": "line",
      "xAxis": { "key": "time", "label": "Time", "format": "HH:mm" },
      "series": [
        { "key": "maxTemp", "label": "Max Cell Temp", "unit": "°C", "color": "#ef4444" },
        { "key": "minTemp", "label": "Min Cell Temp", "unit": "°C", "color": "#3b82f6" }
      ],
      "data": [
        { "time": "2026-02-25T00:00:00Z", "maxTemp": 25, "minTemp": 21 },
        { "time": "2026-02-25T00:15:00Z", "maxTemp": 25, "minTemp": 21 }
      ]
    },
    {
      "key": "pcs_power",
      "title": "PCS Power",
      "type": "line",
      "xAxis": { "key": "time", "label": "Time", "format": "HH:mm" },
      "series": [
        { "key": "activePower", "label": "Active Power", "unit": "kW", "color": "#10b981" },
        { "key": "reactivePower", "label": "Reactive Power", "unit": "kVar", "color": "#f59e0b" }
      ],
      "data": [
        { "time": "2026-02-25T00:00:00Z", "activePower": -33.0, "reactivePower": -0.3 }
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

Same `sections` structure as Summary. Typical sections:

```json
{
  "sections": [
    {
      "key": "healthOverview",
      "title": "Health Overview",
      "variant": "card",
      "columns": 3,
      "fields": [
        { "key": "soh", "label": "State of Health", "value": 98.0, "type": "number", "unit": "%" },
        { "key": "cycleCount", "label": "Cycle Count", "value": 145, "type": "number" },
        { "key": "remainingUsefulLife", "label": "Remaining Useful Life", "value": "~12 years", "type": "string" },
        { "key": "degradationRate", "label": "Annual Degradation", "value": 1.5, "type": "number", "unit": "%/year" }
      ]
    },
    {
      "key": "sohTrend",
      "title": "SoH Trend (12 Months)",
      "variant": "card",
      "columns": 1,
      "fields": [],
      "chartConfig": {
        "type": "line",
        "xAxis": { "key": "month", "label": "Month" },
        "yAxis": { "key": "soh", "label": "SoH", "unit": "%", "domain": [80, 100] },
        "referenceLines": [
          { "y": 80, "label": "End of Life", "color": "red", "strokeDasharray": "5 5" }
        ]
      },
      "chartData": [
        { "month": "2025-03", "soh": 99.8 },
        { "month": "2025-06", "soh": 99.5 },
        { "month": "2025-09", "soh": 99.1 },
        { "month": "2025-12", "soh": 98.5 },
        { "month": "2026-02", "soh": 98.0 }
      ]
    },
    {
      "key": "cycleLife",
      "title": "Cycle Life",
      "variant": "card",
      "columns": 2,
      "fields": [
        { "key": "totalCycles", "label": "Total Cycles", "value": 145, "type": "number" },
        { "key": "ratedCycles", "label": "Rated Cycles", "value": 6000, "type": "number" },
        { "key": "cycleLifeRemaining", "label": "Remaining", "value": 5855, "type": "number" },
        { "key": "avgDod", "label": "Avg Depth of Discharge", "value": 78.5, "type": "number", "unit": "%" }
      ]
    },
    {
      "key": "thermalStress",
      "title": "Thermal Stress",
      "variant": "card",
      "columns": 2,
      "fields": [
        { "key": "maxRecordedTemp", "label": "Max Recorded Temp", "value": 38, "type": "number", "unit": "°C" },
        { "key": "minRecordedTemp", "label": "Min Recorded Temp", "value": 12, "type": "number", "unit": "°C" },
        { "key": "avgOperatingTemp", "label": "Avg Operating Temp", "value": 24, "type": "number", "unit": "°C" },
        { "key": "thermalEvents", "label": "Thermal Events", "value": 0, "type": "number" }
      ]
    }
  ]
}
```

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
        { "key": "tempAlarmThreshold", "label": "Temperature Alarm", "value": 45, "type": "number", "unit": "°C", "editable": true },
        { "key": "socLowAlarm", "label": "SoC Low Alarm", "value": 10, "type": "number", "unit": "%", "editable": true },
        { "key": "socHighAlarm", "label": "SoC High Alarm", "value": 95, "type": "number", "unit": "%", "editable": true }
      ]
    },
    {
      "key": "operatingParams",
      "title": "Operating Parameters",
      "variant": "card",
      "columns": 2,
      "fields": [
        { "key": "chargeStopSoc", "label": "Charge Stop SoC", "value": 95, "type": "number", "unit": "%" },
        { "key": "dischargeStopSoc", "label": "Discharge Stop SoC", "value": 5, "type": "number", "unit": "%" },
        { "key": "operatingMode", "label": "Operating Mode", "value": "Auto", "type": "select", "options": ["Auto", "Manual", "Maintenance"] }
      ]
    },
    {
      "key": "deviceInfo",
      "title": "Device Information",
      "variant": "card",
      "columns": 2,
      "fields": [
        { "key": "firmwareVersion", "label": "Firmware Version", "value": "2.1.4", "type": "string" },
        { "key": "protocol", "label": "Communication Protocol", "value": "Modbus TCP", "type": "string" },
        { "key": "modbusAddress", "label": "Modbus Address", "value": 1, "type": "number" }
      ]
    }
  ]
}
```

### Notes
- Fields with `editable: true` indicate user-configurable values. **Update endpoint not yet implemented** — settings are currently read-only.
- Many values may be `null` if the device metadata doesn't contain the setting key.

---

## 7. GET `/BatteryStorage/Tree`

Hierarchical device tree for all BESS units. Returns a recursive tree structure: Unit → Battery/PCS/TMS → Rack → Pack → Module.

### Query Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `facilityId` | `Guid` | No | Filter tree to a single facility. Omit to return all facilities. |

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
          "children": [
            {
              "id": "c3d4e5f6-...",
              "name": "Rack 1",
              "type": "rack",
              "status": "normal",
              "parentDeviceId": "b2c3d4e5-...",
              "alarmCount": 0,
              "children": []
            }
          ]
        },
        {
          "id": "d4e5f6g7-...",
          "name": "PCS-01",
          "type": "pcs",
          "status": "normal",
          "parentDeviceId": "a1b2c3d4-...",
          "alarmCount": 0,
          "children": []
        },
        {
          "id": "e5f6g7h8-...",
          "name": "Chiller-01",
          "type": "hvac",
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
| `404` | `unitId` not found |
| `400` | Invalid `period` or `interval` value (Curves endpoint) |
