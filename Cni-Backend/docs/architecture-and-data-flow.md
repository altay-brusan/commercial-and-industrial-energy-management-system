# DERMS Backend Architecture & Data Flow

## Table of Contents

1. [System Architecture Overview](#1-system-architecture-overview)
2. [Domain Model & Entity Hierarchy](#2-domain-model--entity-hierarchy)
3. [Data Ingestion Pipeline](#3-data-ingestion-pipeline)
4. [Register-to-Frontend Mapping (End-to-End Trace)](#4-register-to-frontend-mapping-end-to-end-trace)
5. [Dynamic UI System (Specifications-Driven)](#5-dynamic-ui-system-specifications-driven)
6. [Calculated Parameters Catalog](#6-calculated-parameters-catalog)
7. [Device Status Tracking Business Rules](#7-device-status-tracking-business-rules)
8. [Device Specification Templates & Flexibility](#8-device-specification-templates--flexibility)
9. [Adapter Patterns](#9-adapter-patterns)
10. [Device Hierarchy Architecture](#10-device-hierarchy-architecture)
11. [CQRS Query Architecture](#11-cqrs-query-architecture)

---

## 1. System Architecture Overview

### Layered Architecture

```
┌────────────────────────────────────────────────────────────────────┐
│                        External Systems                            │
│   EMQX Broker ◄──subscribe── Backend    Frontend ──JWT──► API       │
│   ENTSO-E / EPIAS ◄──── Hangfire Jobs                              │
└────────────────────────────────┬───────────────────────────────────┘
                                 │
┌────────────────────────────────▼───────────────────────────────────┐
│  Web Layer (src/Web/)                                              │
│  ┌──────────────┐  ┌──────────────────┐  ┌────────────────────┐   │
│  │ Minimal API  │  │ ApiKeyEndpoint   │  │ SignalR LiveHub    │   │
│  │ Endpoints    │  │ Filter (Ingest)  │  │ (Real-time push)   │   │
│  └──────┬───────┘  └────────┬─────────┘  └────────────────────┘   │
│         │                   │                                      │
│  ┌──────▼───────────────────▼─────────────────────────────────┐   │
│  │           MediatR Pipeline                                  │   │
│  │  UnhandledException → Authorization → Validation → Perf     │   │
│  └─────────────────────────┬───────────────────────────────────┘   │
└────────────────────────────┼───────────────────────────────────────┘
                             │
┌────────────────────────────▼───────────────────────────────────────┐
│  Application Layer (src/Application/)                              │
│  ┌─────────────┐ ┌────────────────┐ ┌──────────────────────────┐  │
│  │ Commands     │ │ Queries        │ │ Services                 │  │
│  │ IngestData   │ │ GetBessSummary │ │ DeviceDataCategorizer    │  │
│  │ RegisterDev  │ │ GetBessHealth  │ │ ParameterResolver        │  │
│  │ CreateDevice │ │ GetBessCurves  │ │ AlarmProcessingService   │  │
│  └─────────────┘ │ GetDeviceTree  │ │ PayloadDeserializer      │  │
│                   └────────────────┘ └──────────────────────────┘  │
└────────────────────────────┬───────────────────────────────────────┘
                             │
┌────────────────────────────▼───────────────────────────────────────┐
│  Infrastructure Layer (src/Infrastructure/)                        │
│  ┌──────────────────┐  ┌──────────────┐  ┌───────────────────┐   │
│  │ ApplicationDb    │  │ Hangfire      │  │ External APIs     │   │
│  │ Context (EF Core)│  │ Background    │  │ ENTSO-E, EPIAS    │   │
│  │ + Tenant Filters │  │ Jobs          │  │ SMTP              │   │
│  └──────────────────┘  └──────────────┘  └───────────────────┘   │
└────────────────────────────┬───────────────────────────────────────┘
                             │
┌────────────────────────────▼───────────────────────────────────────┐
│  Domain Layer (src/Domain/)                                        │
│  Entities, Enums, Events, Value Objects — No external dependencies │
└────────────────────────────────────────────────────────────────────┘
```

### Technology Stack

| Component | Technology |
|-----------|-----------|
| Runtime | .NET 9, ASP.NET Core Minimal APIs |
| Database | PostgreSQL (Npgsql) |
| ORM | Entity Framework Core 9.0 |
| CQRS | MediatR |
| Auth | JWT Bearer (HS256) + Refresh Token Rotation |
| Real-time | SignalR (`LiveDataHub`) |
| Background Jobs | Hangfire (PostgreSQL storage) |
| Caching | IMemoryCache (future: Redis) |
| Broker | EMQX (external) — backend subscribes directly via MQTTnet |

### Request Flow

```
[EMQX Broker]                    [Frontend SPA]
     │                                │
     │ MQTT Subscribe (QoS 1)         │ GET /api/v1/BatteryStorage/
     │ cni/+/+/telemetry              │     Units/{unitId}/summary (Bearer JWT)
     ▼                                ▼
┌─────────────────────┐   ┌─────────────────────────────────┐
│ MqttIngestionWorker │   │    ASP.NET Core Pipeline        │
│ (BackgroundService) │   │  Routing → Auth → MediatR       │
└────────┬────────────┘   └──────────────┬──────────────────┘
         │                               │
   IngestDeviceData              GetBessSummary
   Command (MediatR)             QueryHandler
         │                               │
  ┌──────▼─────────┐         ┌───────────▼────────────┐
  │ Resolve by     │         │ Load Device by         │
  │  deviceName    │         │  Device.Id (Guid)      │
  │ Categorize Kind│         │ Load latest telemetry  │
  │ Store in Bucket│         │  by DeviceId (Guid)    │
  │ Update LastSeen│         │ Extract values         │
  └──────┬─────────┘         └───────────┬────────────┘
         │                               │
         ▼                               ▼
    PostgreSQL                     JSON Response
    (JSONB)                        to Frontend
```

**Key:** Ingestion resolves devices by `deviceName` (string from RTU). All query endpoints and telemetry joins use `Device.Id` (Guid). The `deviceName` is never exposed to the frontend.

---

## 2. Domain Model & Entity Hierarchy

### Organizational Hierarchy

```
Tenant (multi-tenant root)
  └── Firm (company)
       └── Facility (physical site)
            └── Rtu (Remote Terminal Unit — gateway)
                 └── Device (physical device — BESS container, PCS, inverter, etc.)
                      ├── Device (child: Rack)
                      │    └── Device (child: Pack)
                      │         └── Device (child: Module)
                      ├── Device (child: ESC controller)
                      └── Device (child: HVAC/TMS)
```

### Device Entity (Key Fields)

| Field | Type | Purpose |
|-------|------|---------|
| `Id` | Guid | Primary key, used in all frontend communication |
| `RtuId` | Guid | Parent RTU (gateway) |
| `DeviceTypeId` | Guid | References DeviceType (e.g., "EnergyStorage", "EnergyStorage.Rack") |
| `ParentDeviceId` | Guid? | Self-referential FK for hierarchy (rack → container, pack → rack) |
| `DeviceSpecificationTemplateId` | Guid? | FK to `DeviceSpecificationTemplate` — links device to its UI/parameter template |
| `HierarchyIndex` | int? | 0-based position among siblings (used for edge address resolution, e.g., bmuLoc) |
| `Name` | string | Display name (e.g., "BESS Container 1", "Rack 1") |
| `SerialNumber` | string | Unique device serial for telemetry matching |
| `Model` | string? | Device model (e.g., "Megapack 2XL") |
| `Manufacturer` | string? | Manufacturer (e.g., "Tesla") |
| `Metadata` | JSON | Instance configuration (thresholds, site-specific values) |
| `Status` | string? | Current status (online/offline/standby/fault/maintenance) |
| `LastSeenUtc` | DateTimeOffset? | Timestamp of last received telemetry |

### Telemetry Bucket Tables

Time-series data is partitioned by device type into four bucket tables:

| Table | Entity | Routes From |
|-------|--------|-------------|
| `DeviceBatteryData` | `DeviceBatteryData` | Kind prefix: ContainerBattery*, Chiller*, HVCU*, Battery*, Bess*, Rack*, Pack*, Module* |
| `DevicePcsData` | `DevicePcsData` | Kind prefix: Pcs*, PCS* |
| `DeviceSolarData` | `DeviceSolarData` | Kind prefix: Solar*, Pv*, PV*, Irradiance* |
| `DeviceSmartMeterData` | `DeviceSmartMeterData` | Kind prefix: Meter*, Grid*, SmartMeter* |

Each telemetry record stores:

| Field | Type | Purpose |
|-------|------|---------|
| `Id` | long | Auto-increment PK (optimized for time-series) |
| `DeviceId` | Guid | Reference to Device |
| `Kind` | string | Message type discriminator (e.g., "ContainerBatteryState") |
| `Payload` | JSONB | **Raw payload as-is from edge device** |
| `DeviceLocalTime` | DateTimeOffset | Device-reported timestamp |
| `ServerReceivedTime` | DateTimeOffset | When backend received it |

---

## 3. Data Ingestion Pipeline

### Overview

The edge device (RTU) collects data from physical devices via Modbus/CAN, converts registers to JSON, and publishes to EMQX MQTT broker. The backend subscribes directly to EMQX as an MQTT client (no HTTP webhook hop).

```
Physical Device (BMS/PCS)
    │
    │  Modbus RTU / CAN Bus
    ▼
┌──────────┐
│   RTU    │  (Edge device — register → JSON conversion)
│ (Gateway)│
└────┬─────┘
     │  MQTT Publish → cni/{facilityId}/{rtuId}/telemetry
     ▼
┌──────────┐
│  EMQX    │  (MQTT Broker)
│  Broker  │
└────┬─────┘
     │  Direct MQTT Subscribe (QoS 1, persistent session)
     ▼
┌──────────────────────────────────────────┐
│  MqttIngestionWorker (BackgroundService) │
│                                          │
│  1. Parse topic → facilityId, rtuId      │
│  2. Resolve RTU GUID → serial number     │
│  3. Parse payload JSON                   │
│  4. Send IngestDeviceDataCommand (MediatR)│
└────┬─────────────────────────────────────┘
     │
     ▼
┌──────────────────────────────────────────┐
│  IngestDeviceDataHandler                 │
│                                          │
│  1. Resolve RTU by rtuSn (cached 5min)   │
│  2. Resolve Device by deviceName + RtuId │
│  3. Categorize Kind → bucket table       │
│  4. Store raw payload as JSONB           │
│  5. Update LastSeenUtc on RTU + Device   │
└──────────────────────────────────────────┘
```

> **Note:** HTTP ingestion endpoints (`POST /api/v1/DeviceData/Ingest` and `/Ingest/Batch`) remain available for testing, development, and third-party adapters, but production RTU traffic flows through MQTT.

### Device Identity Resolution

This is a critical concept for understanding how data flows from the edge to the frontend:

```
┌─────────────────────────────────────────────────────────────────────┐
│                    DEVICE IDENTITY FLOW                              │
│                                                                     │
│  INGESTION (edge → backend)                                        │
│  ─────────────────────────                                         │
│  MQTT topic:  cni/{facilityId}/{rtuId}/telemetry                   │
│  Payload:     { "deviceName": "Battery1", "kind": "...", ... }     │
│                      │                                              │
│                      ▼                                              │
│  Device lookup:  Device.Name == "Battery1" AND Device.RtuId == rtuId│
│                      │                                              │
│                      ▼                                              │
│  Telemetry stored:  DeviceBatteryData.DeviceId = Device.Id (Guid)  │
│                                                                     │
│  QUERY (backend → frontend)                                        │
│  ──────────────────────────                                        │
│  All query endpoints use Device.Id (Guid) exclusively:             │
│    GET /BatteryStorage/Units/{unitId}/summary  → Device.Id         │
│    GET /Pcs/Units/{unitId}/general             → Device.Id         │
│    GET /Dashboard/BessState/{facilityId}       → Rtu.FacilityId    │
│                                                                     │
│  Telemetry join:  DeviceBatteryData.DeviceId == Device.Id          │
│                                                                     │
│  SUMMARY                                                           │
│  ───────                                                           │
│  RTU sends:     deviceName (string)  → human-readable, configured  │
│  Backend stores: Device.Id (Guid)    → all telemetry FK            │
│  Frontend uses:  Device.Id (Guid)    → all API calls               │
│  deviceName is ONLY used at the ingestion boundary                 │
└─────────────────────────────────────────────────────────────────────┘
```

### Ingest Message Format (MQTT)

**Topic:** `cni/{facilityId}/{rtuId}/telemetry`

```json
{
  "deviceName": "Battery1",
  "kind": "ContainerBatteryState",
  "payload": "{\"socPercent\":9,\"sohPercent\":100,\"totalVoltageV\":793.6,...}",
  "timestampUtc": "2026-02-11T14:35:00.000Z"
}
```

| Field | Type | Description |
|-------|------|-------------|
| `deviceName` | string | Device name (unique within the RTU). Matched to `Device.Name` in DB. |
| `kind` | string | Telemetry type identifier — determines which bucket table stores the data |
| `payload` | string (JSON) | Device-specific telemetry data as a JSON string (stored as JSONB) |
| `path` | string? | Optional sub-path for hierarchical addressing |
| `timestampUtc` | ISO 8601 | UTC timestamp from the device |

### HTTP Ingest Endpoint (dev/test)

```
POST /api/v1/DeviceData/Ingest
Header: X-API-Key: <configured-key>
Body: { "rtuSn": "RTU-001", "deviceName": "Battery1", "kind": "...", "payload": "...", "deviceLocalTime": "..." }
```

```
POST /api/v1/DeviceData/Ingest/Batch
Body: [ {rtuSn, deviceName, kind, payload}, ... ]
Response: { total: 10, succeeded: 9, failed: 1 }
```

### Caching Strategy

RTU and Device lookups are cached in `IMemoryCache` with 5-minute sliding expiration. Only scalar values are cached (not EF entities) to avoid cross-scope tracking issues.

```
MQTT path:   RTU GUID from topic → Cache Key: "rtu:id-to-sn:{rtuId}" → RtuSerialNumber
Ingest:      Cache Key: "rtu:sn:{serialNumber}" → Guid (RTU ID)
             Cache Key: "device:name:{deviceName}:rtu:{rtuId}" → Guid (Device ID)
TTL: 5 minutes sliding
```

### Kind-to-Category Routing

`DeviceDataCategorizer.Categorize(kind)` maps the `Kind` string prefix to determine which bucket table receives the data:

| Kind Prefix | Category | Bucket Table |
|-------------|----------|--------------|
| `ContainerBattery*` | Bess | DeviceBatteryData |
| `Chiller*` | Bess | DeviceBatteryData |
| `HVCU*` | Bess | DeviceBatteryData |
| `Battery*` | Bess | DeviceBatteryData |
| `Bess*` | Bess | DeviceBatteryData |
| `Rack*` | Bess | DeviceBatteryData |
| `Pack*` | Bess | DeviceBatteryData |
| `Module*` | Bess | DeviceBatteryData |
| `Pcs*`, `PCS*` | Pcs | DevicePcsData |
| `Solar*`, `Pv*`, `PV*`, `Irradiance*` | Solar | DeviceSolarData |
| `Meter*`, `Grid*`, `SmartMeter*` | SmartMeter | DeviceSmartMeterData |

---

## 4. Register-to-Frontend Mapping (End-to-End Trace)

This section traces exactly how a raw register value from a physical BMS device ends up displayed as a UI element on the frontend.

### Example: SOC (State of Charge) — Complete Journey

We trace the `socPercent` field from `sample.json` through every layer.

#### Step 1: Physical Device → RTU (Edge)

The BMS (Battery Management System) exposes SOC via Modbus holding register (e.g., register 40001). The RTU reads this register, applies scaling (raw value 900 ÷ 100 = 9.0%), and packages it into a JSON message:

```json
{
  "kind": "ContainerBatteryState",
  "timestampUtc": "2026-02-11T14:35:00.000Z",
  "DeviceId": "EC100-DEV-001",
  "socPercent": 9,
  "sohPercent": 100,
  "totalVoltageV": 793.6,
  "instantCurrentA": -30.7,
  "state": "Discharge",
  ...
}
```

#### Step 2: RTU → EMQX → Backend Ingest

The RTU publishes this JSON to an MQTT topic. EMQX webhook rule POSTs it to the backend:

```
POST /api/v1/DeviceData/Ingest
X-API-Key: ***REMOVED***

{
  "rtuSn": "RTU-001",
  "deviceSn": "EC100-DEV-001",
  "kind": "ContainerBatteryState",
  "payload": "{\"socPercent\":9,\"sohPercent\":100,\"totalVoltageV\":793.6,...}"
}
```

#### Step 3: Ingest Handler — Categorize & Store

```
IngestDeviceDataHandler:
  1. RTU lookup: "RTU-001" → Guid (from cache or DB)
  2. Device lookup: "EC100-DEV-001" → Guid (from cache or DB)
  3. DeviceDataCategorizer.Categorize("ContainerBatteryState")
     → prefix "ContainerBattery" matches → DeviceDataCategory.Bess
  4. Create DeviceBatteryData entity:
     { DeviceId: <guid>, Kind: "ContainerBatteryState", Payload: <raw JSON> }
  5. Update Device.LastSeenUtc = now
  6. SaveChanges → stored in DeviceBatteryData table as JSONB
```

#### Step 4: Frontend Requests Summary → Query Handler

```
GET /api/v1/BatteryStorage/Units/{deviceGuid}/summary
Authorization: Bearer <jwt>
```

`GetBessSummaryQueryHandler` executes:

```
1. Load Device + SpecificationTemplate (via FK Include)
2. Load latest DeviceBatteryData for this device
3. Parse device.SpecificationTemplate.SpecificationsTemplate → find "tabs.summary.sections"
4. Resolve parameter values via ParameterResolver (uses ParameterMapTemplate formulas)
     → Falls back to PayloadDeserializer.ExtractValue(payload, "socPercent") if no parameter map
5. Return DynamicSectionDto[] to frontend
```

#### Step 5: PayloadDeserializer — The Bridge

`PayloadDeserializer.ExtractValue(JsonElement root, string dotPath)` is the critical adapter:

```csharp
// Simple path: "socPercent"
root.GetProperty("socPercent") → JsonElement(9)

// Nested path: "dcPower.voltage"
root.GetProperty("dcPower").GetProperty("voltage") → JsonElement(793.6)
```

#### Step 6: Response JSON → Frontend Rendering

The backend returns:

```json
[
  {
    "key": "liveStatus",
    "title": "Live Status",
    "order": 1,
    "fields": [
      {
        "key": "soc",
        "label": "State of Charge",
        "value": 9,
        "unit": "%",
        "format": "number",
        "sourcePath": "socPercent",
        "order": 1
      },
      {
        "key": "soh",
        "label": "State of Health",
        "value": 100,
        "unit": "%",
        "format": "number",
        "sourcePath": "sohPercent",
        "order": 2
      },
      ...
    ]
  }
]
```

The frontend renders each `SectionFieldDto` as a labeled value card.

### Visual Summary of the Complete Trace

```
┌─────────────────────────────────────────────────────────────────────┐
│  BMS Register 40001 = 900 (raw)                                     │
│         │                                                           │
│    RTU scaling: 900 ÷ 100 = 9%                                     │
│         │                                                           │
│    JSON: { "socPercent": 9 }                                        │
│         │                                                           │
│    MQTT → EMQX → Webhook POST                                      │
│         │                                                           │
│    IngestHandler:                                                   │
│      Categorize("ContainerBatteryState") → Bess bucket              │
│      Store payload as JSONB in DeviceBatteryData                    │
│         │                                                           │
│    GetBessSummaryHandler:                                           │
│      device.SpecificationTemplate.SpecificationsTemplate            │
│        .tabs.summary.sections[0].fields[0].sourcePath = "socPercent"│
│      ParameterResolver → PayloadDeserializer fallback → value: 9   │
│         │                                                           │
│    Response:                                                        │
│      { key: "soc", label: "State of Charge", value: 9, unit: "%" } │
│         │                                                           │
│    Frontend: renders "State of Charge: 9%"                          │
└─────────────────────────────────────────────────────────────────────┘
```

### Mapping Multiple Kinds from sample.json

The sample.json contains 12 different message kinds from a single BESS container. Here is how each maps through the system:

| Kind | Category | Bucket | Frontend Page | Key Registers |
|------|----------|--------|---------------|---------------|
| `ContainerBatteryState` | Bess | DeviceBatteryData | Summary, Curves | socPercent, sohPercent, totalVoltageV, instantCurrentA, state, cycleCount |
| `ContainerBatteryFaultState` | Bess | DeviceBatteryData | Fault Flags | faultCellOverVoltageWarning, faultPackOC, hasWarning, hasFault, activeFaults[] |
| `ContainerBatteryCellMapState` | Bess | DeviceBatteryData | Hierarchy (cell level) | cells[].voltageMv, cells[].temperatureC, totalVoltageV |
| `ChillerState` | Bess | DeviceBatteryData | Summary (TMS section) | outletCoolantTempC, inletCoolantTempC, compressorSpeed, operatingModeStr |
| `ChillerAlarmState` | Bess | DeviceBatteryData | Fault Flags (TMS) | systemCoolantShortage, compressorVfdOcAlarm, hasAlarm, activeAlarms[] |
| `HVCUControlState` | Bess | DeviceBatteryData | Summary (HVCU section) | bcu01ControlState, bcu01StateStr |
| `PcsPowerState` | Pcs | DevicePcsData | PCS Summary | pcs_outActPwrKw, pcs_outAppPwrKva, pcs_outReaPwrKvar |
| `PcsAcMeasState` | Pcs | DevicePcsData | PCS Summary | pcs_abLineV, pcs_aPhaseV, pcs_aPhaseCurrA |
| `PcsDcMeasState` | Pcs | DevicePcsData | PCS Summary | pcs_batV, pcs_batI, pcs_dcPwrKw |
| `PcsStatusState` | Pcs | DevicePcsData | PCS Summary | pcs_gridFreqHz, pcs_workingCondition, pcs_sourceMode |
| `PcsFaultGrp1State` | Pcs | DevicePcsData | Fault Flags (PCS) | pcs_hwEpoFault, pcs_gridAOvFault, pcs_hasFaultGrp1 |
| `PcsFaultGrp2State` | Pcs | DevicePcsData | Fault Flags (PCS) | pcs_preRelCloseFail, pcs_hasFaultGrp2 |
| `PcsTempsState` | Pcs | DevicePcsData | PCS Health | pcs_igbtTempC, pcs_ambientTempC, pcs_inductanceTempC |
| `PcsAlgoParamsState` | Pcs | DevicePcsData | PCS Config | pcs_modOvertProtC, pcs_ccCvRechgV |
| `PcsTimeoutsState` | Pcs | DevicePcsData | Fault Flags (PCS) | pcs_dcOcProtTout, pcs_hasTimeout |

---

## 5. Dynamic UI System (Specifications-Driven)

### Core Concept

The frontend UI is **not hardcoded** per device model. Instead, each `DeviceSpecificationTemplate.SpecificationsTemplate` JSON defines what tabs, sections, fields, charts, and fault flags to display. Devices link to their template via the `DeviceSpecificationTemplateId` FK. This allows different manufacturers and models to have completely different UIs without any code changes.

### Specifications JSON Structure

```json
{
  "tabs": {
    "summary": {
      "label": "Summary",
      "order": 1,
      "sections": [
        {
          "key": "liveStatus",
          "title": "Live Status",
          "order": 1,
          "fields": [
            {
              "key": "soc",
              "label": "State of Charge",
              "unit": "%",
              "format": "number",
              "sourcePath": "socPercent",
              "order": 1
            }
          ]
        }
      ]
    },
    "health": {
      "label": "Health",
      "order": 2,
      "sections": [...]
    }
  },
  "curveCharts": [
    {
      "key": "power",
      "title": "Power Over Time",
      "chartType": "line",
      "series": [
        {
          "key": "dcPower",
          "label": "DC Power",
          "unit": "kW",
          "sourcePath": "instantCurrentA",
          "color": "#4CAF50"
        }
      ]
    }
  ],
  "faultFlags": {
    "sources": [
      { "key": "bms", "label": "BMS", "kind": "ContainerBatteryFaultState" },
      { "key": "tms", "label": "TMS", "kind": "ChillerAlarmState" }
    ],
    "flags": [
      {
        "key": "faultCellOverVoltageWarning",
        "label": "Cell Over Voltage Warning",
        "source": "bms",
        "category": "voltage",
        "severity": "warning",
        "sourcePath": "faultCellOverVoltageWarning"
      }
    ]
  },
  "hierarchy": {
    "levels": [
      { "key": "rack", "label": "Rack", "depth": 0, "hasChildren": true },
      { "key": "pack", "label": "Pack", "depth": 1, "hasChildren": true },
      { "key": "module", "label": "Module", "depth": 2, "hasChildren": true },
      { "key": "cell", "label": "Cell", "depth": 3, "hasChildren": false }
    ]
  }
}
```

### sourcePath — The Key Abstraction

The `sourcePath` field is a dot-notation string that tells the query handler where to find the value in the raw JSONB payload:

| sourcePath | Payload Location | Example Value |
|------------|-----------------|---------------|
| `socPercent` | `payload.socPercent` | 9 |
| `sohPercent` | `payload.sohPercent` | 100 |
| `totalVoltageV` | `payload.totalVoltageV` | 793.6 |
| `instantCurrentA` | `payload.instantCurrentA` | -30.7 |
| `dcPower.voltage` | `payload.dcPower.voltage` | (nested object) |
| `state` | `payload.state` | "Discharge" |
| `maxCellVoltageMv` | `payload.maxCellVoltageMv` | 3315 |

This is how different manufacturers map their different register names to the same UI fields. A Tesla BESS might use `socPercent` while a BYD might use `battery_soc` — the `sourcePath` adapts.

### Query Handler Flow (All Pages Follow This Pattern)

```
1. Load Device + SpecificationTemplate (via .Include(d => d.SpecificationTemplate))
2. Load latest telemetry from bucket table (or multiple for curves)
3. Read device.SpecificationTemplate?.SpecificationsTemplate
4. If ParameterMapTemplate exists:
     → Use ParameterResolver (formulas + source extraction + caching)
5. If specs define the tab:
     → Build response from specs-defined sections/fields
     → Use ParameterResolver ?? PayloadDeserializer for values
6. If specs don't define the tab (or no specs):
     → Use hardcoded default sections with generic sourcePaths
7. Return DTO[] to frontend
```

### Response DTOs

| DTO | Used By | Structure |
|-----|---------|-----------|
| `DynamicSectionDto` | Summary, Health | `{ key, title, order, fields[], subsections[] }` |
| `SectionFieldDto` | Summary, Health | `{ key, label, value, unit, format, sourcePath, order }` |
| `ChartResponseDto` | Curves | `{ charts[], period, interval }` |
| `ChartDefinitionDto` | Curves | `{ key, title, chartType, xAxis, series[] }` |
| `FaultFlagsResponseDto` | Fault Flags | `{ flags[], sources[], categories[], summary }` |
| `HierarchyResponseDto` | Hierarchy | `{ levels[], rootItems[] }` |
| `DeviceTreeNodeDto` | Device Tree | `{ id, name, deviceTypeName, status, activeAlarmCount, children[] }` |

---

## 6. Calculated Parameters Catalog

### Parameters Extracted Directly from Payload

These values come directly from the edge device payload without calculation:

| Parameter | Source Field | Unit | Kind | Description |
|-----------|-------------|------|------|-------------|
| State of Charge | `socPercent` | % | ContainerBatteryState | Current battery charge level (0-100) |
| State of Health | `sohPercent` | % | ContainerBatteryState | Battery health degradation indicator (100 = new) |
| Total Voltage | `totalVoltageV` | V | ContainerBatteryState | Total DC bus voltage across all cells in series |
| Instant Current | `instantCurrentA` | A | ContainerBatteryState | Current flow (negative = discharge, positive = charge) |
| Cycle Count | `cycleCount` | cycles | ContainerBatteryState | Number of full charge/discharge cycles completed |
| Operating State | `state` | — | ContainerBatteryState | Current mode: Charge, Discharge, Idle, Standby |
| State Code | `stateCode` | — | ContainerBatteryState | Numeric state (104=Discharge, 102=Charge, etc.) |
| Remaining Energy | `remainingEnergyKwh` | kWh | ContainerBatteryState | Energy remaining in the battery |
| Available Energy | `availableEnergyKwh` | kWh | ContainerBatteryState | Maximum energy available at current SoH |
| Max Cell Voltage | `maxCellVoltageMv` | mV | ContainerBatteryState | Highest individual cell voltage |
| Min Cell Voltage | `minCellVoltageMv` | mV | ContainerBatteryState | Lowest individual cell voltage |
| Avg Cell Voltage | `avgCellVoltageMv` | mV | ContainerBatteryState | Average across all cells |
| Max Cell Temperature | `maxCellTempC` | C | ContainerBatteryState | Hottest cell |
| Min Cell Temperature | `minCellTempC` | C | ContainerBatteryState | Coldest cell |
| Ambient Temperature | `ambientTempC` | C | ContainerBatteryState | Container ambient temperature |
| DMCC (Max Charge Current) | `dmccA` | A | ContainerBatteryState | Dynamic max charge current allowed by BMS |
| DMDC (Max Discharge Current) | `dmdcA` | A | ContainerBatteryState | Dynamic max discharge current allowed by BMS |
| PCS Active Power | `pcs_outActPwrKw` | kW | PcsPowerState | Total AC active power output |
| PCS Apparent Power | `pcs_outAppPwrKva` | kVA | PcsPowerState | Total AC apparent power |
| PCS Reactive Power | `pcs_outReaPwrKvar` | kvar | PcsPowerState | Total AC reactive power |
| PCS DC Power | `pcs_dcPwrKw` | kW | PcsDcMeasState | DC side power (battery → PCS) |
| Grid Frequency | `pcs_gridFreqHz` | Hz | PcsStatusState | AC grid frequency |
| IGBT Temperature | `pcs_igbtTempC` | C | PcsTempsState | PCS power module temperature |
| Coolant Outlet Temp | `outletCoolantTempC` | C | ChillerState | TMS coolant output temperature |

### Parameters Calculated by Backend

These are computed at query time from raw payload values:

| Parameter | Formula | Why Calculated | Used In |
|-----------|---------|---------------|---------|
| **DC Power** | `totalVoltageV × instantCurrentA / 1000` | BMS reports V and A separately; power in kW is what users need for energy management decisions | Summary page, Curves |
| **Cell Voltage Delta** | `maxCellVoltageMv - minCellVoltageMv` | Imbalance indicator — high delta suggests cell degradation or balancing issues. Critical for predictive maintenance | Health page |
| **Cell Temperature Delta** | `maxCellTempC - minCellTempC` | Thermal imbalance — indicates cooling system issues or uneven load distribution | Health page |
| **Charge Status** | Derived from `state` field or `instantCurrentA` sign | "Charging", "Discharging", "Idle" label — more user-friendly than raw state codes | Unit list |
| **Capacity Fade** | `100 - sohPercent` | Percentage of original capacity lost — easier to understand degradation progress than SOH alone | Health page |
| **Collection Completeness** | `collectedCells / totalExpected × 100` | Cell map data may be incomplete due to communication timeouts; shows data quality | Cell Map page |
| **Power Factor** | `pcs_outActPwrKw / pcs_outAppPwrKva` | Ratio of real to apparent power — important for grid compliance and energy efficiency | PCS Summary |
| **AC/DC Conversion Efficiency** | `pcs_outActPwrKw / pcs_dcPwrKw × 100` | PCS conversion losses — tracks inverter health over time | PCS Health |
| **Bus Voltage Imbalance** | `abs(pcs_posBusV - pcs_negBusV)` | DC bus symmetry check — imbalance indicates capacitor or circuit issues | PCS Health |

### Why We Calculate Instead of Getting from Edge

1. **Edge simplicity**: The RTU's job is to read and transmit registers faithfully. Keeping calculations in the backend allows centralized formula updates without reflashing edge devices.
2. **Cross-kind calculations**: DC Power requires data from `ContainerBatteryState` while AC power comes from `PcsPowerState`. Cross-kind computations must happen server-side.
3. **Historical recalculation**: If a formula changes (e.g., better efficiency calculation), we can recalculate from stored raw data. If the edge calculated it, historical data would be stale.
4. **Manufacturer variation**: Different manufacturers report different fields. Tesla might report power directly; another might only report V and A. The backend normalizes.

---

## 7. Device Status Tracking Business Rules

### Status Definitions

| Status | Color | Meaning |
|--------|-------|---------|
| `online` | Green | Device is actively sending telemetry |
| `standby` | Yellow | Device was recently active but no fresh data |
| `offline` | Red | Device has not sent data for extended period |
| `fault` | Red | Device is reporting active fault conditions |
| `maintenance` | Orange | Device is in scheduled maintenance mode |
| `charging` | Blue | Battery is in charging state |
| `discharging` | Cyan | Battery is in discharging state |
| `idle` | Gray | Device is connected but not active |

### Status Resolution Rules

Status is determined from two inputs: **telemetry freshness** (time since `LastSeenUtc`) and **operating mode** (from payload).

#### Freshness-Based Rules

```
Time since LastSeenUtc:
  ≤ 15 seconds  → "online"
  ≤ 60 seconds  → "standby"
  > 60 seconds  → "offline"
```

These thresholds are designed for typical BESS telemetry intervals (5-10 second reporting).

#### Operating Mode Override

If the device is online (fresh telemetry), the operating mode from the payload can override the base status:

| Payload Field | Value | Override Status |
|---------------|-------|-----------------|
| `state` | "Charge" | `charging` |
| `state` | "Discharge" | `discharging` |
| `state` | "Idle" / "Standby" | `idle` |
| `stateCode` | 102 | `charging` |
| `stateCode` | 104 | `discharging` |
| Any active fault flag | `true` | `fault` (highest priority) |

#### Priority Order

```
1. fault (if any active fault flag) — highest priority
2. maintenance (if manually set)
3. offline (if LastSeenUtc > 60s)
4. standby (if LastSeenUtc > 15s)
5. charging/discharging/idle (from operating mode)
6. online (default when fresh)
```

### Status Update Timing

- **On ingest**: When telemetry arrives, `IngestDeviceDataHandler` updates `Device.LastSeenUtc = DateTimeOffset.UtcNow`
- **Background job** (planned): `DeviceStatusCheckWorker` runs every 30 seconds to check all devices and transition stale ones to standby/offline
- **Real-time push** (planned): Status changes are broadcast via `SignalR LiveDataHub`

### StatusBadgeDto Mapping

The `StatusBadgeDto.FromStatus(string?)` utility maps status strings to frontend badge colors:

```csharp
"online"      → { value: "online",      label: "Online",      color: "green"  }
"offline"     → { value: "offline",     label: "Offline",     color: "red"    }
"standby"     → { value: "standby",     label: "Standby",     color: "yellow" }
"fault"       → { value: "fault",       label: "Fault",       color: "red"    }
"maintenance" → { value: "maintenance", label: "Maintenance", color: "orange" }
"charging"    → { value: "charging",    label: "Charging",    color: "blue"   }
"discharging" → { value: "discharging", label: "Discharging", color: "cyan"   }
"idle"        → { value: "idle",        label: "Idle",        color: "gray"   }
```

---

## 8. Device Specification Templates & Flexibility

### The Flexibility Problem

BESS systems come from many manufacturers (Tesla, BYD, CATL, Samsung SDI, etc.), each with:
- Different register names for the same metric (e.g., `socPercent` vs `battery_soc` vs `soc_pct`)
- Different hierarchy structures (some have 4 levels, others have 3)
- Different fault flag sets (BMS-specific alarm registers)
- Different number of racks, packs, and modules per container

**Our solution**: `DeviceSpecificationTemplate` — a template entity that stores the complete UI definition and hierarchy blueprint per DeviceType + Model + Manufacturer.

### DeviceSpecificationTemplate Entity

```
DeviceSpecificationTemplate
├── DeviceTypeId      (Guid — e.g., "EnergyStorage" type)
├── Model             (string — e.g., "Megapack 2XL")
├── Manufacturer      (string? — e.g., "Tesla")
├── SpecificationsTemplate  (JSONB — tabs, sections, fields, charts, fault flags)
├── ParameterMapTemplate    (JSONB — standalone parameter map with formulas and source definitions)
├── MetadataTemplate        (JSONB — default instance config: thresholds, ratings)
├── HierarchyTemplate       (JSONB — child device tree blueprint)
├── IsDefault               (bool — default template for this DeviceType)
├── Devices                 (navigation — collection of Device entities using this template)
└── Unique Index: (DeviceTypeId, Model, Manufacturer)
```

Devices link to their template via `Device.DeviceSpecificationTemplateId` (nullable FK, SetNull on delete). Query handlers use `.Include(d => d.SpecificationTemplate)` to eagerly load the template.

### HierarchyTemplate Example (Tesla Megapack 2XL)

```json
{
  "children": [
    {
      "key": "rack",
      "deviceTypeName": "EnergyStorage.Rack",
      "count": 4,
      "namePattern": "Rack {index}",
      "children": [
        {
          "key": "pack",
          "deviceTypeName": "EnergyStorage.Pack",
          "count": 4,
          "namePattern": "Pack {index}",
          "children": [
            {
              "key": "module",
              "deviceTypeName": "EnergyStorage.Module",
              "count": 8,
              "namePattern": "Module {index}"
            }
          ]
        }
      ]
    },
    {
      "key": "esc",
      "deviceTypeName": "EnergyStorage.Controller",
      "count": 1,
      "namePattern": "ESC-{index}"
    },
    {
      "key": "hvac",
      "deviceTypeName": "EnergyStorage.TMS",
      "count": 2,
      "namePattern": "HVAC-{index}"
    }
  ]
}
```

This template produces: **4 racks × 4 packs × 8 modules = 128 modules + 1 ESC + 2 HVAC = 131 child devices**.

### How Templates Drive Registration

When `RegisterDeviceCompositeCommand` is executed:

```
1. Load template by TemplateId
2. Create root Device:
     - DeviceType from template.DeviceTypeId
     - DeviceSpecificationTemplateId = template.Id (FK link, not copy)
     - Metadata = template.MetadataTemplate
     - Model = template.Model
     - Manufacturer = template.Manufacturer
3. Parse HierarchyTemplate JSON
4. For each child definition:
     - Resolve DeviceType by name (e.g., "EnergyStorage.Rack")
     - Create {count} child Device entities
     - Name = namePattern with {index} replaced (1-based)
     - SerialNumber = "{parent.SerialNumber}-{childName}"
     - ParentDeviceId = parent.Id
     - HierarchyIndex = 0-based position among siblings
     - Recurse for nested children
5. All devices created in single transaction
```

### Flexibility Matrix

| What Varies | How We Handle It | Where Configured |
|-------------|-----------------|------------------|
| Register names per manufacturer | `sourcePath` in Specifications JSON | DeviceSpecificationTemplate.SpecificationsTemplate |
| Number of racks/packs/modules | `count` in HierarchyTemplate | DeviceSpecificationTemplate.HierarchyTemplate |
| Hierarchy depth | Nested `children[]` in HierarchyTemplate | DeviceSpecificationTemplate.HierarchyTemplate |
| Available tabs per device level | `tabs` object in Specifications | DeviceSpecificationTemplate.SpecificationsTemplate |
| Fault flag definitions | `faultFlags` in Specifications | DeviceSpecificationTemplate.SpecificationsTemplate |
| Chart definitions | `curveCharts` in Specifications | DeviceSpecificationTemplate.SpecificationsTemplate |
| Default thresholds | MetadataTemplate JSON | DeviceSpecificationTemplate.MetadataTemplate |
| Operating mode mapping | `state` field values in payload | RTU adapter configuration |
| Telemetry format | JSON (current) or binary (future adapter) | RTU → EMQX pipeline |

### Adding a New Manufacturer

To add support for a BYD BESS container:

1. **Create a DeviceSpecificationTemplate** via `POST /api/v1/DeviceSpecificationTemplates`:
   - Set `Model: "Cube Pro"`, `Manufacturer: "BYD"`, `DeviceTypeId: <EnergyStorage type GUID>`
   - Define `SpecificationsTemplate` with BYD's register names as sourcePaths:
     ```json
     { "tabs": { "summary": { "sections": [{
       "fields": [{ "key": "soc", "sourcePath": "battery_soc", ... }]
     }]}}}
     ```
   - Define `HierarchyTemplate` with BYD's rack/module structure
   - Define `MetadataTemplate` with BYD-specific thresholds

2. **Register a device** via `POST /api/Devices/RegisterComposite`:
   - The entire child tree is auto-created from the template

3. **No code changes needed** — the dynamic UI system reads Specifications at runtime.

---

## 9. Adapter Patterns

### Adapter 1: DeviceDataCategorizer (Kind → Bucket Routing)

**Purpose**: Routes incoming telemetry to the correct time-series bucket table based on the `Kind` string.

**Pattern**: Prefix-matching adapter. The `Kind` string uses prefixes that indicate the device subsystem.

```
Input:  "ContainerBatteryState"  → Output: DeviceDataCategory.Bess
Input:  "PcsPowerState"          → Output: DeviceDataCategory.Pcs
Input:  "SolarIrradianceState"   → Output: DeviceDataCategory.Solar
```

**Extension point**: To add a new device category (e.g., Wind Turbine), add a new enum value and prefix mapping. No changes to the ingestion handler.

**File**: `src/Application/Common/Services/DeviceDataCategorizer.cs`

### Adapter 2: PayloadDeserializer (sourcePath → Value Extraction)

**Purpose**: Bridges the gap between raw JSONB payload field names and the UI field definitions in Specifications.

**Pattern**: Dot-notation path resolver. Navigates nested JSON structures to extract scalar values.

```
Input:  (payload, "socPercent")           → Output: 9
Input:  (payload, "dcPower.voltage")      → Output: 793.6
Input:  (payload, "cells[0].voltageMv")   → Output: 3364
```

**Why this matters**: Different manufacturers use different field names for the same metric. The `sourcePath` in `Specifications` abstracts this — the query handler doesn't know or care about the actual field name.

**File**: `src/Application/Common/Services/PayloadDeserializer.cs`

### Adapter 3: DeviceSpecificationTemplate (Template → Device Instance)

**Purpose**: Converts a manufacturer-specific template into concrete Device entities with proper Specifications, Metadata, and hierarchy structure.

**Pattern**: Factory adapter. The template is a blueprint; the RegisterDeviceComposite handler is the factory that instantiates it.

```
Input:  Template (Tesla Megapack 2XL)
Output: 1 root + 4 racks + 16 packs + 128 modules + 1 ESC + 2 HVAC = 152 Device entities
        Each with appropriate Specifications and DeviceType
```

**File**: `src/Application/Devices/Commands/RegisterDeviceComposite/RegisterDeviceComposite.cs`

### Adapter 4: StatusBadgeDto (Status String → UI Badge)

**Purpose**: Converts raw device status strings into structured badge objects with color and display label.

**Pattern**: Mapping adapter. Centralizes the status-to-color mapping so the frontend doesn't need to know the color scheme.

**File**: `src/Application/Common/Models/Monitoring/StatusBadgeDto.cs`

### Adapter 5: Multi-Tenant Query Filters (Tenant Isolation)

**Purpose**: Transparently filters all database queries by the current tenant without any explicit filtering in query handlers.

**Pattern**: EF Core global query filters applied at `ApplicationDbContext` construction. Every query automatically includes `WHERE TenantId = @currentTenant`.

**File**: `src/Infrastructure/Data/ApplicationDbContext.cs`

### Future Adapter: Binary Protocol → JSON

When edge devices use binary protocols (e.g., Modbus TCP raw frames), a thin adapter at the EMQX level or in a sidecar service will convert binary frames to the expected JSON format before POSTing to the ingest endpoint. The backend remains unchanged.

---

## 10. Device Hierarchy Architecture

### Decision: Hybrid Approach (DB Entities + JSON)

| Level | Storage | Why |
|-------|---------|-----|
| Container (root) | Device entity (GUID) | Has tabs, alarms, status, telemetry |
| Rack | Device entity (GUID) | Has individual status, alarms, sub-hierarchy |
| Pack | Device entity (GUID) | Has individual status, monitoring |
| Module | Device entity (GUID) | Has individual telemetry, cell data |
| ESC (Controller) | Device entity (GUID) | Has configuration, status |
| HVAC/TMS | Device entity (GUID) | Has temperature monitoring, alarms |
| Cell | JSON array in Module payload | Too granular for DB — 1792+ cells per container |

### DeviceType Hierarchy

```
EnergyStorage          ← Root BESS container
EnergyStorage.Rack     ← Rack within container
EnergyStorage.Pack     ← Pack within rack
EnergyStorage.Module   ← Module within pack
EnergyStorage.Controller ← Energy Storage Controller (ESC)
EnergyStorage.TMS      ← Thermal Management System (HVAC/Chiller)
Pcs                    ← Power Conversion System (inverter)
```

These names are defined as constants in `DeviceTypeNames` (`src/Domain/Constants/DeviceTypeNames.cs`).

### Tree Query

`GET /api/Devices/{id}/Tree` returns:

```json
{
  "id": "abc-123",
  "name": "BESS Container 1",
  "deviceTypeName": "Battery",
  "status": "online",
  "activeAlarmCount": 2,
  "availableTabs": ["summary", "health", "curves", "faultFlags", "hierarchy"],
  "children": [
    {
      "id": "def-456",
      "name": "Rack 1",
      "deviceTypeName": "EnergyStorage.Rack",
      "status": "online",
      "activeAlarmCount": 1,
      "availableTabs": ["summary", "faultFlags"],
      "children": [
        {
          "id": "ghi-789",
          "name": "Pack 1",
          "deviceTypeName": "EnergyStorage.Pack",
          "status": "fault",
          "activeAlarmCount": 1,
          "availableTabs": ["summary"],
          "children": [...]
        }
      ]
    }
  ]
}
```

### Alarm Count Aggregation

The tree query counts active alarms including all descendants:

```sql
-- Single grouped query for all alarm counts
SELECT DeviceId, COUNT(*) as Count
FROM DeviceAlarms
WHERE DeviceId IN (@allDeviceIds) AND IsActive = true
GROUP BY DeviceId
```

Then recursively sums child counts into parent `activeAlarmCount`.

---

## 11. CQRS Query Architecture

### Device Identity at Query Time

**All query endpoints use `Device.Id` (Guid) to find devices and join telemetry.** The `deviceName` used during ingestion is never referenced by query handlers.

| Query Input | Lookup Pattern | Telemetry Join |
|------------|----------------|----------------|
| `unitId` (Guid) | `Device.Id == unitId` | `DeviceXxxData.DeviceId == unitId` |
| `facilityId` (Guid) | `Device.Rtu.FacilityId == facilityId` + DeviceType filter | `DeviceXxxData.DeviceId IN (device IDs)` |
| Search by name | `Device.Name.Contains(search)` or `Device.SerialNumber.Contains(search)` | N/A (used for filtering list, not telemetry join) |

### Query Pattern (All Monitoring Pages)

Every monitoring query follows the same pattern:

```
┌──────────────────────────────────────────────────────────────────┐
│  1. Load Device by Device.Id (Guid)                              │
│     Include: DeviceType, LogicalDevice, SpecificationTemplate    │
│                                                                  │
│  2. Load latest telemetry from bucket table                      │
│     DeviceBatteryData                                            │
│       .Where(dd => dd.DeviceId == device.Id)                     │
│       .GroupBy(dd => dd.Kind)                                    │
│       .Select(g => g.OrderByDescending(dd.DeviceLocalTime)       │
│                      .First())                                   │
│                                                                  │
│  3. Load SpecificationTemplate                                   │
│     specs = device.SpecificationTemplate?.SpecificationsTemplate  │
│                                                                  │
│  4. Build response from specs OR defaults                        │
│     if specs has "tabs.{pageName}.sections":                     │
│       → Build DynamicSectionDto[] from specs definition          │
│     else:                                                        │
│       → Build from hardcoded default sections                    │
│                                                                  │
│  5. For each field:                                              │
│     value = PayloadDeserializer.ExtractValue(payload, sourcePath)│
│                                                                  │
│  6. Return typed DTO to endpoint → JSON response                 │
└──────────────────────────────────────────────────────────────────┘
```

### Available Queries

**BESS** (all via tab dispatch: `GET /api/v1/BatteryStorage/Units/{unitId}/{tab}`)

| Tab Key / Route | Query | Returns | Data Source |
|----------|-------|---------|-------------|
| `GET /Units/{facilityId}` | GetBessUnitsQuery | BessUnitsResultDto | All EnergyStorage devices + latest telemetry |
| `GET /Units/{unitId}/Header` | GetBessUnitHeaderQuery | BessUnitHeaderDto | Single device metadata |
| `GET /Tree/{facilityId}` | GetBessDeviceTreeQuery | BessDeviceTreeResponseDto | Device parent-child tree with alarm counts |
| Tab: `summary` | GetBessSummaryQuery | DynamicSectionDto[] | specs.tabs.summary + payload |
| Tab: `health` | GetBessHealthQuery | DynamicSectionDto[] | specs.tabs.health + payload |
| Tab: `curves` | GetBessCurvesQuery | ChartResponseDto | specs.curveCharts + historical data |
| Tab: `faultflags` | GetBessFaultFlagsQuery | FaultFlagsResponseDto | DeviceAlarm system |
| Tab: `faults` | GetBessFaultsQuery | FaultsPaginatedResponseDto | DeviceAlarm table (paginated) |
| Tab: `settings` | GetBessSettingsQuery | DynamicSectionDto[] | specs.tabs.settings + metadata |

**PCS** (all via tab dispatch: `GET /api/v1/Pcs/Units/{unitId}/{tab}`)

| Tab Key / Route | Query | Returns | Data Source |
|----------|-------|---------|-------------|
| `GET /Units/{facilityId}` | GetPcsUnitsQuery | PcsUnitsResultDto | All PCS devices + linked BESS parent |
| `GET /Units/{unitId}/Header` | GetPcsHeaderQuery | PcsHeaderDto | Single device metadata + available tabs |
| `GET /Schema` | GetPcsSchemaQuery | SchemaResponseDto | All PCS Specifications |
| Tab: `general` | GetPcsGeneralQuery | DynamicSectionDto[] | specs.tabs.general + payload |
| Tab: `fault` | GetPcsFaultFlagsQuery | FaultFlagsResponseDto | DeviceAlarm system |
| Tab: `curve` | GetPcsCurvesQuery | ChartResponseDto | specs.curveCharts + historical data |

### Default Sections (When No Specifications Defined)

If a device has no linked `DeviceSpecificationTemplate` (or the template's `SpecificationsTemplate` JSON doesn't define a particular tab), hardcoded defaults are used:

#### Summary Defaults

| Section | Fields | sourcePaths |
|---------|--------|-------------|
| liveStatus | SOC, SOH, State, Cycle Count | socPercent, sohPercent, state, cycleCount |
| dcPower | Voltage, Current, Power | totalVoltageV, instantCurrentA, (calculated) |
| energyLimits | Remaining Energy, Available Energy, DMCC, DMDC | remainingEnergyKwh, availableEnergyKwh, dmccA, dmdcA |

#### Health Defaults

| Section | Fields | sourcePaths |
|---------|--------|-------------|
| sohOverview | SOH, Cycle Count | sohPercent, cycleCount |
| degradation | Capacity Fade, Resistance Increase | (calculated), resistanceIncrease |
| throughput | Lifetime Throughput, Energy In, Energy Out | lifetimeThroughputKwh, totalEnergyInKwh, totalEnergyOutKwh |

#### Curves Defaults

| Chart | Series | sourcePaths |
|-------|--------|-------------|
| power | DC Power | (calculated from V×I) |
| soc | SOC | socPercent |
| temperature | Battery Temperature | ambientTempC |
| voltage | DC Voltage | totalVoltageV |

---

## Appendix A: API Endpoint Summary

### Device Data Ingestion (API Key Auth)

| Method | Path | Description |
|--------|------|-------------|
| POST | `/api/v1/DeviceData/Ingest` | Single telemetry message |
| POST | `/api/v1/DeviceData/Ingest/Batch` | Batch of telemetry messages |

### Battery Storage Monitoring (JWT Auth)

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/v1/BatteryStorage/Tree/{facilityId}` | Device tree with alarm counts for facility |
| GET | `/api/v1/BatteryStorage/Units/{facilityId}` | List BESS units with aggregates for facility |
| GET | `/api/v1/BatteryStorage/Units/{unitId}/Header` | Unit header (name, model, available tabs) |
| GET | `/api/v1/BatteryStorage/Units/{unitId}/{tab}` | Tab content dispatch (see tab keys below) |

**BESS Tab Keys** (case-insensitive):
| Tab | Description | Extra Query Params |
|-----|-------------|--------------------|
| `summary` | Dynamic summary sections | — |
| `health` | Dynamic health sections | — |
| `curves` | Time-series chart data | `period`, `interval` |
| `faultflags` | Active fault flags from alarm system | — |
| `faults` | Fault history (paginated) | `page`, `pageSize`, `severity`, `isActive` |
| `settings` | Device settings/configuration | — |

### PCS Monitoring (JWT Auth)

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/v1/Pcs/Units/{facilityId}` | List PCS units for facility (with linked BESS parent) |
| GET | `/api/v1/Pcs/Units/{unitId}/Header` | Unit header (name, model, available tabs) |
| GET | `/api/v1/Pcs/Units/{unitId}/{tab}` | Tab content dispatch (see tab keys below) |
| GET | `/api/v1/Pcs/Schema` | PCS specifications schema |

**PCS Tab Keys** (case-insensitive):
| Tab | Description | Extra Query Params |
|-----|-------------|--------------------|
| `general` | General PCS status and measurements | — |
| `fault` | Fault flags from alarm system | — |
| `curve` | Time-series chart data | `period`, `interval` |

### Device Management (JWT Auth)

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/Devices` | List devices (paginated) |
| GET | `/api/Devices/{id}` | Get device by ID |
| GET | `/api/Devices/{id}/Tree` | Device tree with alarm counts |
| POST | `/api/Devices` | Create single device |
| POST | `/api/Devices/RegisterComposite` | Register device with full hierarchy |

### Device Specification Templates (JWT Auth)

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/v1/DeviceSpecificationTemplates` | List templates |
| GET | `/api/v1/DeviceSpecificationTemplates/{id}` | Get template |
| POST | `/api/v1/DeviceSpecificationTemplates` | Create template |
| PUT | `/api/v1/DeviceSpecificationTemplates/{id}` | Update template |

---

## Appendix B: Configuration Reference

### appsettings.json

```json
{
  "IngestSettings": {
    "ApiKey": "change-me-in-production"
  }
}
```

### appsettings.Development.json

```json
{
  "IngestSettings": {
    "ApiKey": "***REMOVED***"
  }
}
```

### EMQX Webhook Configuration

Configure EMQX to POST to:
- **URL**: `https://<backend-host>/api/v1/DeviceData/Ingest`
- **Headers**: `X-API-Key: <configured-api-key>`, `Content-Type: application/json`
- **Body template**: `{"rtuSn":"${rtu_sn}","deviceSn":"${device_sn}","kind":"${kind}","payload":"${payload}","deviceLocalTime":"${timestamp}"}`

For batch mode:
- **URL**: `https://<backend-host>/api/v1/DeviceData/Ingest/Batch`
- **Body**: Array of the above objects
