# Solar Inverters Monitoring API

Base URL: `/api/v1/SolarInverters`

All endpoints require JWT Bearer authentication and the `Solar.View` permission (except Commands which require `Solar.Control`).

---

## Overview

The Solar Inverters monitoring module provides real-time and historical data for solar inverter devices. It follows the same architecture as the BESS and PCS monitoring modules:

- **CQRS with MediatR** — each endpoint maps to a query/command
- **Dual device type filtering** — matches devices by `DeviceType.Name` (`Solar` or `Inverter`) OR `LogicalDevice.Category` (`Solar`)
- **ParameterResolver with fallback** — template-driven parameter resolution with hardcoded extraction as fallback
- **Tab-based routing** — the `/{unitId}/{tab}` endpoint dispatches to the appropriate query handler

---

## Endpoints

### 1. List Units

```
GET /api/v1/SolarInverters/Units/{facilityId}
```

Returns all solar inverter devices in a facility with aggregate statistics.

**Parameters:**
| Name | In | Type | Description |
|---|---|---|---|
| `facilityId` | path | Guid | Facility ID |
| `search` | query | string? | Filter by name or serial number |
| `sortBy` | query | string? | Sort field: `name`, `power`, `yield` |
| `sortDirection` | query | string? | `asc` or `desc` |

**Response:** `SolarUnitsResultDto`
```json
{
  "units": [
    {
      "id": "guid",
      "name": "Inverter #1",
      "serialNumber": "SN-001",
      "modelName": "SG125HX",
      "manufacturer": "Huawei",
      "facilityName": "Solar Plant A",
      "facilityId": "guid",
      "status": { "value": "online", "label": "Online", "color": "green" },
      "activePowerKw": 98.5,
      "dailyYieldKwh": 450.2,
      "totalYieldKwh": 125000.0,
      "efficiencyPct": 97.8,
      "temperatureC": 42.3,
      "activeFaultCount": 0,
      "lastSeenUtc": "2026-03-13T10:30:00Z"
    }
  ],
  "aggregates": {
    "totalUnits": 10,
    "onlineCount": 9,
    "offlineCount": 1,
    "faultCount": 0,
    "totalActivePowerKw": 985.0,
    "totalDailyYieldKwh": 4502.0,
    "averageEfficiencyPct": 97.5,
    "totalActiveFaults": 0
  }
}
```

**Telemetry source:** Latest `DeviceSolarData` per device, grouped by `(DeviceId, Kind)`, ordered by `DeviceLocalTime DESC`.

---

### 2. Unit Header

```
GET /api/v1/SolarInverters/Units/{unitId}/Header
```

Returns inverter metadata, status, alarm count, and available tabs.

**Response:** `SolarUnitHeaderDto`
```json
{
  "id": "guid",
  "name": "Inverter #1",
  "serialNumber": "SN-001",
  "modelName": "SG125HX",
  "manufacturer": "Huawei",
  "facilityId": "guid",
  "facilityName": "Solar Plant A",
  "status": { "value": "online", "label": "Online", "color": "green" },
  "activeAlarmCount": 0,
  "availableTabs": [
    { "key": "general", "label": "General", "enabled": true, "order": 1 },
    { "key": "faults", "label": "Faults", "enabled": true, "order": 2 },
    { "key": "curves", "label": "Curves", "enabled": true, "order": 3 },
    { "key": "history", "label": "History", "enabled": true, "order": 4 }
  ],
  "lastSeenUtc": "2026-03-13T10:30:00Z"
}
```

**Tabs:** Default tabs are `general`, `faults`, `curves`, `history`. Can be overridden by the device's `SpecificationTemplate.SpecificationsTemplate` JSON (`supportedTabs` array).

---

### 3. Tab Content (General / Faults / Curves / History)

```
GET /api/v1/SolarInverters/Units/{unitId}/{tab}
```

**Parameters:**
| Name | In | Type | Description |
|---|---|---|---|
| `unitId` | path | Guid | Device ID |
| `tab` | path | string | One of: `general`, `faults`, `curves`, `history` |
| `period` | query | string? | Time range for curves/history: `1h`, `6h`, `12h`, `24h`, `48h`, `7d`, `30d` |
| `interval` | query | string? | Sampling interval: `1m`, `5m`, `15m`, `30m`, `1h`, `1d` |
| `page` | query | int? | Page number for faults (default: 1) |
| `pageSize` | query | int? | Page size for faults (default: 20) |
| `severity` | query | string? | Filter faults by severity |
| `isActive` | query | bool? | Filter faults by active status |

#### Tab: `general`

Returns dynamic sections with live data. Response type: `SummaryResponseDto`.

**Sections:**
| Section | Key | Fields |
|---|---|---|
| Overview | `overview` | activePower, reactivePower, dailyYield, totalYield, efficiency, powerFactor |
| AC Output | `acOutput` | voltageAB/BC/CA, currentA/B/C, frequency |
| MPPT Information | `mpptInfo` | Table of MPPT trackers with voltage, current, power per tracker |
| Device Information | `deviceInfo` | name, serial, model, manufacturer, firmware, protocol, installedCapacity |

**Telemetry source:** Latest `Solar.Power` and `Solar.Mppt` Kinds from `DeviceSolarData`.

**MPPT extraction:** Tries `mpptTrackers` JSON array first, then falls back to numbered pattern (`mppt1Voltage`, `mppt2Voltage`, etc.).

#### Tab: `faults`

Returns paginated fault/alarm records. Response type: `FaultsPaginatedResponseDto` (shared DTO with BESS).

#### Tab: `curves`

Returns time-series chart data. Response type: `ChartResponseDto` (shared DTO).

**Default charts** (when no `curveCharts` schema in template):
| Chart | Series |
|---|---|
| Power Output | activePower (kW), reactivePower (kVar) |
| Voltage | voltageAB, voltageBC, voltageCA (V) |
| Current | currentA, currentB, currentC (A) |
| DC Input (MPPT) | dcPower (kW), dcVoltage (V) |

**Schema-driven charts:** If the device's `SpecificationTemplate.SpecificationsTemplate` contains a `curveCharts` JSON array, those are used instead of defaults.

#### Tab: `history`

Returns dense history data with signal configuration metadata. Response type: `SolarHistoryResponseDto`.

```json
{
  "signals": [
    { "key": "activePower", "label": "Active Power", "unit": "kW", "group": "Power" },
    { "key": "voltageAB", "label": "Voltage AB", "unit": "V", "group": "Voltage" }
  ],
  "dataPointCount": 288,
  "interval": "5m",
  "period": "24h",
  "data": [
    { "time": "2026-03-13T00:00:00", "activePower": 0, "voltageAB": 398.2, ... }
  ]
}
```

**Signal groups:** Power (activePower, reactivePower, dcPower, dailyYield), Voltage (AB/BC/CA), Current (A/B/C), MPPT (mppt1-2 voltage/current), Other (frequency, efficiency, powerFactor, temperature).

---

### 4. Device Tree

```
GET /api/v1/SolarInverters/DeviceTree/{facilityId}
```

Returns the sidebar navigation tree for solar devices.

**Response:** `SolarDeviceTreeResponseDto`
```json
{
  "tree": [
    {
      "id": "guid",
      "name": "Inverter #1",
      "type": "inverter",
      "status": "online",
      "parentDeviceId": null,
      "alarmCount": 0,
      "children": [
        {
          "id": "guid",
          "name": "MPPT 1",
          "type": "mppt",
          "status": "normal",
          "alarmCount": 0,
          "children": []
        }
      ]
    }
  ]
}
```

**Display types:** `inverter`, `mppt`, `string`, `device` (fallback).

---

### 5. Energy Analytics

```
GET /api/v1/SolarInverters/EnergyAnalytics/{facilityId}?period=24h
```

Plant-level energy analytics aggregating across all solar devices in a facility.

**Response:** `SolarEnergyAnalyticsDto`
```json
{
  "stats": {
    "totalActivePowerKw": 985.0,
    "totalDailyYieldKwh": 4502.0,
    "totalYieldKwh": 1250000.0,
    "inverterCount": 10,
    "onlineCount": 9
  },
  "plantPowerCurve": {
    "key": "plantPower",
    "title": "Plant Power Curve",
    "type": "area",
    "xAxis": { "key": "time", "label": "Time" },
    "series": [{ "key": "power", "label": "Total Power", "unit": "kW" }],
    "data": [{ "time": "08:00", "power": 250.5 }, ...]
  },
  "inverterYieldComparison": [
    { "name": "Inverter #1", "dailyYield": 450.2, "totalYield": 125000 },
    { "name": "Inverter #2", "dailyYield": 448.1, "totalYield": 124800 }
  ]
}
```

**Data source:** Aggregates `Solar.Power` from `DeviceSolarData` across all solar devices in the facility. Plant power curve groups data by time buckets and sums power.

---

### 6. Performance

```
GET /api/v1/SolarInverters/Performance/{facilityId}?period=24h
```

Plant-level performance metrics.

**Response:** `SolarPerformanceDto`
```json
{
  "totalInstalledCapacityKw": 1250.0,
  "totalActivePowerKw": 985.0,
  "totalDailyYieldKwh": 4502.0,
  "capacityUtilizationPct": 78.8,
  "averageEfficiencyPct": 97.5,
  "peakPowerKw": 120.5,
  "inverterMetrics": [
    {
      "deviceId": "guid",
      "name": "Inverter #1",
      "activePowerKw": 98.5,
      "dailyYieldKwh": 450.2,
      "efficiencyPct": 97.8,
      "specificYield": 3.6,
      "capacityUtilizationPct": 78.8
    }
  ]
}
```

**Installed capacity:** Read from device `Metadata` JSON (`ratedPower` or `installedCapacityKw`). Capacity utilization = `activePower / ratedPower * 100`.

**Specific yield** = `dailyYield / ratedPower` (kWh/kWp).

---

### 7. Send Command

```
POST /api/v1/SolarInverters/Units/{unitId}/Commands/{action}
```

Send a remote control command to an inverter. Requires `Solar.Control` permission.

**Valid actions:** `TurnOn`, `TurnOff`, `Reset`, `Shutdown`, `Startup`, `ClearFaults`

**Response:** `SolarCommandResultDto`
```json
{
  "deviceId": "guid",
  "deviceName": "Inverter #1",
  "action": "TurnOn",
  "status": "accepted",
  "message": "Command 'TurnOn' accepted for device 'Inverter #1'"
}
```

> **Note:** Commands are currently accepted and acknowledged but not dispatched to physical devices. Actual SCADA/MQTT command dispatching will be implemented separately.

---

## Architecture

### File Structure

```
src/
├── Application/SolarInverters/
│   ├── Commands/
│   │   └── SendSolarCommand/
│   │       └── SendSolarCommand.cs          # Command + validator + handler + DTO
│   └── Queries/
│       ├── GetSolarUnits/
│       │   └── GetSolarUnits.cs             # Query + handler + DTOs
│       ├── GetSolarUnitHeader/
│       │   └── GetSolarUnitHeader.cs
│       ├── GetSolarGeneral/
│       │   └── GetSolarGeneral.cs
│       ├── GetSolarFaults/
│       │   └── GetSolarFaults.cs
│       ├── GetSolarCurves/
│       │   └── GetSolarCurves.cs            # Validator + handler
│       ├── GetSolarHistory/
│       │   └── GetSolarHistory.cs
│       ├── GetSolarDeviceTree/
│       │   └── GetSolarDeviceTree.cs
│       ├── GetSolarEnergyAnalytics/
│       │   └── GetSolarEnergyAnalytics.cs
│       └── GetSolarPerformance/
│           └── GetSolarPerformance.cs
├── Domain/Constants/
│   └── Permissions.cs                       # Added SolarControl
└── Web/Endpoints/
    └── SolarInverters.cs                    # Endpoint group
```

### Shared DTOs (reused from Common)

- `ChartResponseDto`, `ChartDefinitionDto`, `CurveSeriesDto`, `CurveXAxisDto` — chart data
- `FaultsPaginatedResponseDto`, `FaultRecordDto`, `PaginationDto` — fault records
- `DynamicSectionDto`, `SectionFieldDto`, `TableDataDto`, `SummaryResponseDto` — dynamic sections
- `StatusBadgeDto` — status display
- `TabInfoDto` — available tabs
- `MonitoringPeriods`, `MonitoringIntervals` — period/interval constants

### Telemetry Tables

| Table | Kinds Used | Endpoint |
|---|---|---|
| `DeviceSolarData` | `Solar.Power` | Units, Header, General, Curves, History, Analytics, Performance |
| `DeviceSolarData` | `Solar.Mppt` | General (MPPT table) |
| `DeviceAlarms` | — | Header (count), Faults (paginated), DeviceTree (count) |

### Authorization

| Endpoint | Permission |
|---|---|
| All GET endpoints | `Solar.View` |
| POST Commands | `Solar.Control` |

Both are gated by `[Authorize(Roles = "Administrator,Tenant.Admin")]` at the query/command level, plus `[RequirePermission]` for feature-based RBAC.

---

## Postman Mock API Mapping

| Postman Route | Backend Route | Notes |
|---|---|---|
| `GET /SolarInverters/Units` | `GET /Units/{facilityId}` | Backend scopes by facility |
| `GET /SolarInverters/Units/{id}` | `GET /Units/{unitId}/Header` | Explicit Header sub-route |
| `GET /SolarInverters/Units/{id}/General` | `GET /Units/{unitId}/general` | Tab routing |
| `GET /SolarInverters/Units/{id}/Faults` | `GET /Units/{unitId}/faults` | Tab routing |
| `GET /SolarInverters/Units/{id}/Curves` | `GET /Units/{unitId}/curves` | Tab routing |
| `GET /SolarInverters/Units/{id}/History` | `GET /Units/{unitId}/history` | Tab routing |
| `GET /SolarInverters/DeviceTree` | `GET /DeviceTree/{facilityId}` | Backend scopes by facility |
| `GET /SolarInverters/EnergyAnalytics` | `GET /EnergyAnalytics/{facilityId}` | Backend scopes by facility |
| `GET /SolarInverters/Performance` | `GET /Performance/{facilityId}` | Backend scopes by facility |
| `POST /SolarInverters/Units/{id}/Commands/{action}` | `POST /Units/{unitId}/Commands/{action}` | Command accepted, not dispatched |
