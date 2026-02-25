# Battery Storage API Reference

Base URL: `/api/v1/BatteryStorage`
Authorization: Bearer JWT token required on all endpoints.

---

## 1. GET /DeviceTree

Returns the full device tree for all BESS units.

**Response** `BessDeviceTreeResponseDto`
```json
{
  "tree": [
    {
      "id": "guid",
      "name": "Megapack Alpha",
      "type": "unit",
      "deviceTypeName": "Battery",
      "status": "online",
      "alarmCount": 3,
      "availableTabs": ["summary", "health", "curves"],
      "children": [
        {
          "id": "guid",
          "name": "Rack 1",
          "type": "rack",
          "deviceTypeName": "Battery.Rack",
          "status": "online",
          "alarmCount": 1,
          "availableTabs": [],
          "children": [...]
        }
      ]
    }
  ]
}
```

**Type mapping:** Battery → `unit`, Battery.Rack → `rack`, Battery.Pack → `pack`, Battery.Module → `module`, Battery.Controller → `esc`, Battery.TMS → `hvac`.

---

## 2. GET /Units

Returns all BESS units with live telemetry and aggregates.

**Query params:**
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
      "name": "Megapack Alpha",
      "status": { "value": "online", "label": "Online", "color": "green" },
      "operatingMode": { "value": "discharging", "label": "Discharging", "color": "purple" },
      "soc": 87.5,
      "soh": 95.8,
      "power": 245.3,
      "capacityKwh": 3916.0,
      "activeAlarmCount": 3,
      "brand": "Tesla",
      "model": "Megapack 2XL",
      "serialNumber": "SN-BESS-01-42",
      "facilityName": "Solar Farm Alpha",
      "facilityId": "guid",
      "temperatureC": 32.5,
      "lastSeenUtc": "2025-02-09T10:30:00Z"
    }
  ],
  "aggregates": {
    "totalUnits": 4,
    "totalCapacityKwh": 15664.0,
    "averageSoc": 82.3,
    "totalActiveAlarms": 7,
    "onlineCount": 3,
    "offlineCount": 0,
    "faultCount": 1,
    "standbyCount": 0
  }
}
```

**Power calculation:** `totalVoltageV * instantCurrentA / 1000` with fallback to `dcPower.power`.
**Alarm counts** are aggregated across all devices in the same RTU (root + descendants).

---

## 3. GET /Units/{unitId}

Returns detail for a single BESS unit including available tabs.

**Response** `BessUnitDetailDto`
```json
{
  "id": "guid",
  "name": "Megapack Alpha",
  "serialNumber": "SN-BESS-01-42",
  "modelName": "Megapack 2XL",
  "manufacturer": "Tesla",
  "facilityId": "guid",
  "facilityName": "Solar Farm Alpha",
  "status": { "value": "online", "label": "Online", "color": "green" },
  "availableTabs": [
    { "key": "summary", "label": "Summary", "isEnabled": true, "order": 1 },
    { "key": "health", "label": "Health", "isEnabled": true, "order": 2 }
  ],
  "lastSeenUtc": "2025-02-09T10:30:00Z"
}
```

Tabs are derived from `Device.Specifications.supportedTabs` JSON or defaults to: summary, health, curves, hierarchy, faultFlags, faults.

---

## 4. GET /Units/{unitId}/Tree

Returns the device tree for a single BESS unit (hierarchy of racks, packs, modules, ESC, HVAC).

**Response** `DeviceTreeNodeDto` — same node structure as `/DeviceTree` but rooted at the requested unit.

---

## 5. GET /Units/{unitId}/Hierarchy

Legacy endpoint. Returns hierarchy data from `Device.Specifications` JSON.

---

## 6. GET /Units/{unitId}/HierarchyLevel

Legacy endpoint. Returns a single hierarchy level.

**Query params:** `level` (string), `parentId` (string?)

---

## 7. GET /Units/{unitId}/Summary

Returns dynamic summary sections based on device type. Auto-discriminates between Battery containers, sub-devices (Rack/Pack/Module), TMS, and generic devices.

**Response** `SummaryResponseDto`
```json
{
  "sections": [
    {
      "key": "nameplate",
      "title": "Nameplate Information",
      "icon": "info",
      "variant": "highlighted",
      "columns": 3,
      "fields": [
        { "key": "brand", "label": "Brand", "value": "Tesla", "type": "string" },
        { "key": "model", "label": "Model", "value": "Megapack 2XL", "type": "string" }
      ]
    },
    {
      "key": "liveStatus",
      "title": "Live Status",
      "icon": "activity",
      "variant": "card",
      "columns": 3,
      "fields": [
        {
          "key": "soc", "label": "State of Charge", "value": 87.5,
          "type": "number", "unit": "%", "format": "progress",
          "meta": { "thresholds": [{ "min": 60, "color": "green" }, { "min": 30, "color": "orange" }, { "min": 0, "color": "red" }] }
        }
      ]
    }
  ]
}
```

**Battery container sections (9):** `nameplate`, `liveStatus`, `energyLimits`, `cellExtremes`, `contactorsSafety`, `thermalManagement` (optional, from ChillerState), `hvcuControl` (optional, table variant with BCU rows), `healthPreview`, `connectedDevices` (from DeviceLink).

**Multiple Kind loading:** Queries latest `ContainerBatteryState`, `ChillerState`, `HVCUControlState` in one grouped query.

**Specs-driven mode:** If `Device.Specifications.tabs.summary.sections` exists, builds sections from that schema instead of defaults.

### Section field types
| Type | Description |
|------|-------------|
| `string` | Text value |
| `number` | Numeric value, may have `unit` |
| `boolean` | True/false, format `badge` shows colored label |
| `select` | Dropdown (Settings only), has `options` list |

### Field properties
| Property | Type | Description |
|----------|------|-------------|
| `key` | string | Unique field identifier |
| `label` | string | Display label |
| `value` | any | Current value |
| `type` | string | Field type |
| `unit` | string? | Unit of measurement |
| `format` | string? | `progress`, `temperature`, `badge`, `date` |
| `status` | string? | Status indicator |
| `editable` | bool? | Whether user can edit (Settings only) |
| `options` | string[]? | Options for select type (Settings only) |
| `meta` | object? | Extra metadata (thresholds, location, badge info) |

### Section variants
| Variant | Description |
|---------|-------------|
| `card` | Standard card layout |
| `highlighted` | Emphasized section (e.g., nameplate) |
| `table` | Contains `tableData` with columns/rows |
| `chart` | Contains `chartConfig` + `chartData` |

---

## 8. GET /Units/{unitId}/Health

Returns health analytics sections with SoH degradation trend chart.

**Response** `SummaryResponseDto` — same wrapper as Summary.

**Battery sections (7):**

| Section | Key | Variant | Description |
|---------|-----|---------|-------------|
| Health Overview | `healthOverview` | card (2 cols) | SoH, cycle/calendar life used (progress bars with thresholds), RTE, estimated RUL |
| SoH Trend | `sohTrend` | chart | 12-month area chart with EOL reference line at 80% |
| Degradation Metrics | `degradationMetrics` | card (3 cols) | Equivalent full cycles, designed cycle life, average DoD, total cycle count |
| Energy Throughput | `energyThroughput` | card (3 cols) | Lifetime charged/discharged (MWh), RTE, daily charged/discharged |
| Thermal Stress | `thermalStress` | card (2 cols) | Max recorded temp, thermal excursions, avg operating temp |
| Operational History | `operationalHistory` | card (3 cols) | Total operating/standby hours, firmware version |
| Maintenance & Warranty | `maintenanceWarranty` | highlighted (3 cols) | Last/next maintenance dates, warranty expiry with Active/Expired badge |

**SoH trend chart config:**
```json
{
  "chartConfig": {
    "type": "area",
    "xAxis": { "key": "month", "label": "Month" },
    "yAxis": { "key": "soh", "label": "SoH", "unit": "%", "domain": [80, 100] },
    "referenceLines": [{ "y": 80, "label": "EOL Threshold", "color": "red", "strokeDasharray": "5 5" }],
    "color": "#8b5cf6"
  },
  "chartData": [
    { "month": "Mar 24", "soh": 97.5 },
    { "month": "Apr 24", "soh": 97.4 }
  ]
}
```

**Computed fields:** `cycleLifeUsed` = cycleCount / designedCycleLife * 100. `calendarLifeUsed` from commissioning date in metadata. `estimatedRUL` from remaining calendar life.

---

## 9. GET /Units/{unitId}/Curves

Returns time-series chart data for monitoring curves.

**Query params:**
| Param | Type | Default | Values |
|-------|------|---------|--------|
| `period` | string | `day` | `day`, `week`, `month`, `year` |
| `interval` | string | `15m` | `5m`, `15m`, `1h`, `1d` |

**Response** `ChartResponseDto`
```json
{
  "charts": [
    {
      "key": "powerSoc",
      "title": "Power & State of Charge",
      "type": "area",
      "xAxis": { "key": "time", "label": "Time", "format": "HH:mm" },
      "series": [
        { "key": "power", "label": "Power", "unit": "kW", "color": "#8b5cf6", "yAxis": "left", "type": "area" },
        { "key": "soc", "label": "SoC", "unit": "%", "color": "#22c55e", "yAxis": "right", "type": "line" }
      ],
      "data": [
        { "time": "00:00", "power": -150.2, "soc": 87.5 },
        { "time": "00:05", "power": -157.5, "soc": 90.1 }
      ]
    }
  ]
}
```

**Default BESS charts (3):**
| Chart | Key | Type | Series |
|-------|-----|------|--------|
| Power & SoC | `powerSoc` | area | power (kW, left), soc (%, right) |
| Voltage & Current | `voltageCurrent` | line | voltage (V, left), current (A, right) |
| Temperature | `temperature` | area | tempMax, tempAvg, tempMin (all C) |

**Time format** varies by period: day → `HH:mm`, week → `ddd HH:mm`, month → `dd MMM`, year → `MMM yy`.

**Data is unified** per chart (one data array with all series values per row), not per-series.

**Specs-driven:** If `Device.Specifications.curveCharts` exists, builds charts from that schema.

---

## 10. GET /Units/{unitId}/FaultFlags

Returns current fault flag states from the latest telemetry payload.

**Response** `FaultFlagsResponseDto`
```json
{
  "flags": [
    {
      "key": "faultStatus",
      "label": "Fault Status",
      "isActive": false,
      "source": "battery",
      "category": "general",
      "severity": "Critical"
    }
  ],
  "sources": [
    { "key": "battery", "label": "Battery", "activeCount": 0, "totalCount": 5 }
  ],
  "categories": [
    { "key": "general", "label": "General", "activeCount": 0, "totalCount": 3 }
  ],
  "summary": {
    "totalFlags": 10,
    "activeFlags": 0,
    "activeBySource": {},
    "activeByCategory": {}
  }
}
```

Flag definitions come from `Device.Specifications.faultFlags.flags` with `sourcePath` dot-notation into the payload. Falls back to extracting `status.faultStatus`, `status.warningStatus`, `status.faultMessages` from payload.

---

## 11. GET /Units/{unitId}/Notifications

Returns paginated fault/alarm records for the unit and all descendant devices in the same RTU.

**Query params:**
| Param | Type | Default | Description |
|-------|------|---------|-------------|
| `page` | int | 1 | Page number |
| `pageSize` | int | 20 | Items per page |
| `severity` | string? | | Filter: `critical`, `major`, `minor`, `warning`, `info` |
| `isActive` | bool? | | Filter by active/recovered status |

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
  "pagination": {
    "page": 1,
    "pageSize": 50,
    "totalItems": 6,
    "totalPages": 1
  }
}
```

**Severity badge colors:** critical → red, major → orange, minor → yellow, warning → yellow, info → blue.
**Status badges:** active → red, recovered → grey.
**Scope:** Queries `DeviceAlarm` across all devices sharing the same `RtuId`.

---

## 12. GET /Units/{unitId}/Settings

Returns device configuration sections with editable fields.

**Response** `SummaryResponseDto`
```json
{
  "sections": [
    {
      "key": "general",
      "title": "General Settings",
      "fields": [
        { "key": "deviceName", "label": "Device Name", "value": "Megapack Alpha", "type": "string", "editable": true },
        { "key": "serialNumber", "label": "Serial Number", "value": "SN-BESS-01-42", "type": "string", "editable": false }
      ]
    },
    {
      "key": "alarms",
      "title": "Alarm Thresholds",
      "fields": [
        { "key": "tempAlarmThreshold", "label": "Temperature Alarm Threshold", "value": 45, "type": "number", "unit": "C", "editable": true }
      ]
    },
    {
      "key": "operation",
      "title": "Operation Settings",
      "fields": [
        { "key": "operatingMode", "label": "Operating Mode", "value": "auto", "type": "select", "options": ["auto", "manual", "maintenance"], "editable": true },
        { "key": "balancingEnabled", "label": "Cell Balancing", "value": true, "type": "boolean", "editable": true }
      ]
    },
    {
      "key": "maintenance",
      "title": "Maintenance",
      "fields": [
        { "key": "maintenanceIntervalDays", "label": "Maintenance Interval", "value": 90, "type": "number", "unit": "days", "editable": true }
      ]
    }
  ]
}
```

**Data sources:** Device entity fields (name, serialNumber) + `Device.Metadata` JSON (thresholds, operation settings, firmware version, protocol).
**Specs-driven:** If `Device.Specifications.tabs.settings.sections` exists, builds from schema.

---

## 13. GET /Schema

Returns capability manifest for all Battery product types (from `DeviceSpecificationTemplate` entity).

**Response** `SchemaResponseDto`
```json
{
  "productTypes": [
    {
      "key": "megapack-2xl",
      "label": "Tesla Megapack 2XL",
      "hierarchy": ["rack", "pack", "module"],
      "supportedTabs": ["summary", "health", "hierarchy", "monitoring", "fault", "alarms", "settings"],
      "summarySections": ["nameplate", "liveStatus", "energyLimits", "cellExtremes", "contactorsSafety", "thermalManagement", "hvcuControl", "healthPreview", "connectedDevices"],
      "healthSections": ["healthOverview", "sohTrend", "degradationMetrics", "energyThroughput", "thermalStress", "operationalHistory", "maintenanceWarranty"],
      "faultSources": ["battery", "chiller"],
      "curveCharts": ["powerSoc", "voltageCurrent", "temperature"],
      "features": {
        "hasTms": true,
        "hasHvcu": true,
        "hasCellBalancing": true,
        "hasChillerFaults": true
      },
      "connectedDevices": {
        "escCount": 1,
        "hvacCount": 2
      }
    }
  ]
}
```

**Hierarchy levels** are derived by walking the `HierarchyTemplate` JSON tree (excluding `esc`/`hvac` auxiliary devices).
**Features** come from `SpecificationsTemplate.features` or are inferred from section presence.
**Connected devices** are counted from `HierarchyTemplate.children` entries with `key` of `esc`/`hvac`.

---

## Common Types

### StatusBadgeDto
```json
{ "value": "online", "label": "Online", "color": "green" }
```

Used for: device status, operating mode, fault severity, fault status.

### Authorization
All endpoints require:
- Role: `Administrator` or `Tenant.Admin`
- Permission: `Bess.View`

Enforced via `[Authorize]` + `[RequirePermission]` attributes on the MediatR command/query, not at the endpoint level.
