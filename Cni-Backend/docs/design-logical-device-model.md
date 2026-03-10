# Design: LogicalDevice / LogicalNode Device Model

**Status:** Draft — Awaiting team feedback
**Date:** 2026-03-05
**Author:** Backend team

---

## 1. Motivation

The embedded team is developing RTUs with IEC 61850-inspired addressing (`CAB1/BatteryNode1.voltage.mag.f`). The current backend model — where `DeviceSpecificationTemplate` is keyed by Manufacturer + Model and drives parameter extraction, UI layout, and hierarchy — has these limitations:

1. **Tight coupling to manufacturer internals:** The backend must understand each manufacturer's payload structure via `ParameterMapTemplate` and `DeviceDataCategorizer`.
2. **Redundant data:** Device.Model, Device.Manufacturer, and DeviceSpecificationTemplate all carry overlapping identity information.
3. **Flat parameter maps:** The `ParameterMapTemplate` is a monolithic JSON blob with no reusability across device types.
4. **No standard addressing:** The `Kind` string-based categorization (`ContainerBatteryState`, `PcsPowerState`) is ad-hoc and not aligned with the RTU's addressing scheme.

With the new RTU architecture, the backend should be **agnostic to manufacturer/model** and instead work with abstract device types (LogicalDevices) composed of reusable parameter groups (LogicalNodes).

---

## 2. Key Concepts

### 2.1 LogicalNode

A **reusable group of related parameters**, inspired by IEC 61850 logical nodes but with custom naming.

Examples:
- `BatteryEnergyData` — Power, Voltage, Current, Temperature, RemainingEnergy, SoC, SoH
- `PcsAcMeasurementData` — Grid Frequency, Phase Voltages, Phase Currents, Power Factor
- `HvacStatusData` — Coolant Temps, Compressor Status, Pressures

Each parameter in a node has:
- `key` — unique identifier within the node (e.g., `"soc"`, `"voltage"`)
- `label` — display name (e.g., `"State of Charge"`)
- `unit` — measurement unit (e.g., `"%"`, `"V"`, `"kW"`)
- `dataType` — `number`, `string`, `boolean`
- `sourceType` — where the value comes from:
  - **Registration** — set when device is registered in RTU library (chemistry, capacity, voltage range)
  - **Installation** — set when device is installed at facility (name, commissioning date)
  - **Realtime** — from RTU telemetry via EMQX
  - **Calculated** — computed by backend (operating hours, efficiency degradation, cycle life %)

### 2.2 LogicalDevice

An **abstract device type** composed of LogicalNodes. Defines what a device IS in terms of its capabilities.

Examples:
- `BatteryContainer` — has nodes: BatteryNameplateData, BatteryStatusData, BatteryEnergyData, BatteryHealthData, HvacStatusData
- `SolarInverter` — has nodes: SolarStatusData, SolarInverterData
- `PcsUnit` — has nodes: PcsStatusData, PcsAcMeasurementData, PcsDcMeasurementData

A manufacturer-specific device (e.g., "Tesla Megapack") is simply a LogicalDevice that may include extra nodes beyond the base ones. The backend doesn't need to know it's a Tesla — it just knows it's a `BatteryContainer` with certain nodes.

### 2.3 Device (Instance)

A **physical device installed** at a facility, referencing a LogicalDevice from the catalog. Manufacturer, model, and brand are stored as informational metadata — they do NOT drive backend logic.

### 2.4 Topology

Port-to-port connections between top-level devices, provided by the facility engineering tool as JSON. The topology says: "Logical device X's port 1 is connected to logical device Y's port 4." Internal hierarchy (e.g., Cabinet → Rack → Pack) is defined inside the LogicalDevice's `HierarchyTemplate`.

### 2.5 Addressing

RTU sends telemetry with IEC 61850-like addresses: `CAB1/BatteryNode1.voltage.mag.f`

Parsed as:
- `CAB1` — device instance name
- `BatteryNode1` — logical node (maps to a LogicalNode via alias)
- `voltage.mag.f` — parameter path within the node

This makes parameter extraction self-describing — no need for manufacturer-specific `ParameterMapTemplate`.

---

## 3. Proposed Entity Model

### 3.1 LogicalNode

```
Table: LogicalNodes (global, no tenant filter)

Id                  Guid        PK
Name                string(100) UNIQUE — e.g., "BatteryEnergyData"
DisplayName         string(200) — e.g., "Battery Energy"
Description         string?
Parameters          jsonb       — parameter definitions array (see §3.5)
Created             DateTimeOffset
CreatedBy           string?
LastModified        DateTimeOffset
LastModifiedBy      string?
```

### 3.2 LogicalDevice

```
Table: LogicalDevices (global, no tenant filter)

Id                  Guid        PK
Name                string(100) UNIQUE — e.g., "BatteryContainer"
DisplayName         string(200) — e.g., "Battery Container"
Description         string?
Category            string(50)  INDEXED — "BESS", "Solar", "Wind", "Grid", "PCS"
Ports               jsonb       — port definitions (see §3.6)
HierarchyTemplate   jsonb?      — child device structure (see §3.7)
UiTemplate          jsonb?      — optional UI layout overrides (see §3.8)
Created             DateTimeOffset
CreatedBy           string?
LastModified        DateTimeOffset
LastModifiedBy      string?
```

### 3.3 LogicalDeviceNode (Join Table)

```
Table: LogicalDeviceNodes

LogicalDeviceId     Guid        PK, FK → LogicalDevices (CASCADE)
LogicalNodeId       Guid        PK, FK → LogicalNodes (RESTRICT)
NodeAlias           string(100)? — custom name, e.g., "mppt" instead of default
InstanceCount       int         DEFAULT 1 — e.g., 2 MPPTs, 16 modules
SortOrder           int         — display ordering
```

### 3.4 Device (Modified)

```
Table: Devices (existing, tenant-scoped via Rtu → Facility → Firm)

+ LogicalDeviceId   Guid?       FK → LogicalDevices (RESTRICT)

  Existing columns unchanged:
  Id, RtuId, DeviceTypeId, DeviceSpecificationTemplateId,
  Name, Model, Manufacturer, SerialNumber, Ports, Metadata,
  ParentDeviceId, HierarchyIndex, TopologyId, Status, LastSeenUtc

  Note: DeviceTypeId and DeviceSpecificationTemplateId remain during transition.
  They will be dropped in a future cleanup phase once all devices use LogicalDeviceId.
```

### 3.5 Parameters JSON Schema

Each LogicalNode's `Parameters` column:

```json
[
  {
    "key": "soc",
    "label": "State of Charge",
    "unit": "%",
    "dataType": "number",
    "sourceType": "Realtime",
    "description": "Battery state of charge percentage"
  },
  {
    "key": "chemistry",
    "label": "Cell Chemistry",
    "unit": null,
    "dataType": "string",
    "sourceType": "Registration",
    "description": "Battery cell chemistry type (LFP, NMC, etc.)"
  },
  {
    "key": "cycleLifePct",
    "label": "Cycle Life Used",
    "unit": "%",
    "dataType": "number",
    "sourceType": "Calculated",
    "formula": {
      "expression": "round(multiply(divide(cycleCount, designedCycleLife), 100), 1)",
      "inputs": {
        "cycleCount": { "$ref": "cycleCount" },
        "designedCycleLife": { "source": { "origin": "metadata", "paths": ["designedCycleLife"], "default": 6000 } }
      }
    }
  }
]
```

### 3.6 Ports JSON Schema

LogicalDevice `Ports` — defines what ports a device type has:

```json
[
  {
    "id": "AC",
    "type": "AC",
    "modes": [
      {
        "direction": "INPUT",
        "power_w": { "min": 0, "nom": 5000, "max": 5500 },
        "voltage_v": [220, 230],
        "current_a": { "min": 0, "nom": 22.8, "max": 25 },
        "frequency_hz": [50, 60],
        "phases": 1
      },
      {
        "direction": "OUTPUT",
        "power_w": { "min": 0, "nom": 5000, "max": 5500 },
        "voltage_v": [220, 230],
        "current_a": { "min": 0, "nom": 21.8, "max": 24 },
        "frequency_hz": [50, 60],
        "phases": 1
      }
    ]
  },
  {
    "id": "MPPT11",
    "type": "DC",
    "modes": [
      {
        "direction": "INPUT",
        "voltage_v": { "min": 125, "max": 500 },
        "current_a": { "min": 0, "max": 18 }
      }
    ]
  }
]
```

### 3.7 HierarchyTemplate JSON Schema

LogicalDevice `HierarchyTemplate` — defines internal device hierarchy:

```json
{
  "children": [
    {
      "logicalDeviceName": "BatteryRack",
      "count": 10,
      "namePattern": "Rack-{index}",
      "children": [
        {
          "logicalDeviceName": "BatteryPack",
          "count": 4,
          "namePattern": "Pack-{index}",
          "children": [
            {
              "logicalDeviceName": "BatteryModule",
              "count": 8,
              "namePattern": "Module-{index}"
            }
          ]
        }
      ]
    },
    {
      "logicalDeviceName": "EnergyStorageController",
      "count": 1,
      "namePattern": "ESC-{index}"
    },
    {
      "logicalDeviceName": "HvacUnit",
      "count": 2,
      "namePattern": "HVAC-{index}"
    }
  ]
}
```

When a device is registered at a facility, this template is auto-expanded to create the full device tree with correct `ParentDeviceId` and `HierarchyIndex`.

### 3.8 UiTemplate JSON Schema

LogicalDevice `UiTemplate` — optional UI layout overrides:

```json
{
  "supportedTabs": [
    { "key": "summary", "label": "Summary", "isEnabled": true, "order": 0 },
    { "key": "health", "label": "Health", "isEnabled": true, "order": 1 },
    { "key": "hierarchy", "label": "Hierarchy", "isEnabled": true, "order": 2 },
    { "key": "curves", "label": "Curves", "isEnabled": true, "order": 3 },
    { "key": "faults", "label": "Faults", "isEnabled": true, "order": 4 }
  ],
  "curveCharts": [
    {
      "key": "power",
      "title": "Power",
      "chartType": "line",
      "series": [
        { "key": "dcPower", "nodeRef": "BatteryEnergyData", "paramKey": "power", "color": "#4CAF50" }
      ]
    }
  ]
}
```

---

## 4. Entity Relationship Diagram

```
┌─────────────────┐       ┌──────────────────────┐       ┌─────────────────┐
│  LogicalNode     │       │  LogicalDeviceNode    │       │  LogicalDevice   │
│─────────────────│       │──────────────────────│       │─────────────────│
│ Id          (PK) │◄──────│ LogicalNodeId    (PK) │──────►│ Id          (PK) │
│ Name     (unique)│       │ LogicalDeviceId  (PK) │       │ Name     (unique)│
│ DisplayName      │       │ NodeAlias             │       │ DisplayName      │
│ Description      │       │ InstanceCount         │       │ Category         │
│ Parameters (json)│       │ SortOrder             │       │ Ports      (json)│
└─────────────────┘       └──────────────────────┘       │ HierarchyTemplate│
                                                          │ UiTemplate (json)│
                                                          └────────┬────────┘
                                                                   │ 1
                                                                   │
                                                                   │ *
                                                          ┌────────┴────────┐
                                                          │     Device       │
                                                          │─────────────────│
                                                          │ Id          (PK) │
                                                          │ RtuId       (FK) │
                                                          │ LogicalDeviceId? │
                                                          │ ParentDeviceId?  │
                                                          │ Name             │
                                                          │ SerialNumber     │
                                                          │ Metadata   (json)│
                                                          │ Ports      (json)│
                                                          │ HierarchyIndex   │
                                                          │ Status           │
                                                          │ LastSeenUtc      │
                                                          └─────────────────┘
```

---

## 5. Parameter Source Types & Data Flow

```
┌─────────────────────────────────────────────────────────────┐
│                      Data Sources                            │
├─────────────┬──────────────┬──────────────┬─────────────────┤
│ Registration│ Installation │   Realtime   │   Calculated    │
│             │              │              │                 │
│ Set when    │ Set when     │ From RTU     │ Computed by     │
│ device is   │ device is    │ telemetry    │ backend         │
│ added to    │ installed at │ via EMQX     │                 │
│ RTU library │ facility     │              │                 │
├─────────────┼──────────────┼──────────────┼─────────────────┤
│ Brand       │ Name         │ SoC          │ Cycle Life %    │
│ Model       │ Commissioning│ SoH          │ Operating Hours │
│ Chemistry   │ Date         │ Voltage      │ Efficiency      │
│ Capacity    │ DeviceId     │ Current      │ Degradation     │
│ Max Power   │ SerialNumber │ Power        │ Last Maint.     │
│ Voltage Rng │              │ Temperature  │ Next Maint.     │
│ Firmware    │              │ Grid Freq    │ Warranty Expiry │
│ Cycle Life  │              │ Wind Speed   │ Daily Yield     │
│ Calendar Lf │              │ etc.         │ Asset Age       │
└─────────────┴──────────────┴──────────────┴─────────────────┘
         │              │              │               │
         ▼              ▼              ▼               ▼
   Device.Metadata  Device.Metadata  Telemetry     Backend
   (informational)  (instance data)  Tables        Services
```

---

## 6. Addressing & Telemetry Flow

### Current Flow
```
RTU → EMQX webhook → POST /api/v1/DeviceData/Ingest
  Payload: { "Kind": "ContainerBatteryState", "SerialNumber": "...", "Payload": {...} }
  Backend: DeviceDataCategorizer routes by Kind prefix → typed DeviceData table
  Backend: ParameterMapTemplate + FormulaEvaluator extracts values per manufacturer
```

### New Flow (target)
```
RTU → EMQX → POST /api/v1/DeviceData/Ingest
  Payload: { "DeviceName": "CAB1", "NodeName": "BatteryEnergy", "Parameters": {...} }
  -- or equivalently --
  Address: "CAB1/BatteryEnergy.voltage.mag.f" = 1500.2

  Backend: Parse address → resolve Device by name → resolve LogicalNode by alias
  Backend: Route to telemetry table by LogicalDevice.Category (BESS/Solar/PCS/Grid)
  Backend: Store with structured address — no manufacturer-specific extraction needed
  Backend: Computed params (Calculated sourceType) still use FormulaEvaluator
```

---

## 7. LogicalNode Catalog (from Parameter Spreadsheet)

### BESS Nodes

| Node | Parameters | Source |
|------|-----------|--------|
| **BatteryNameplateData** | Brand, Model, Manufacturer, Chemistry, Capacity, MaxPower, VoltageRange, FirmwareVersion, DesignedCycleLife, DesignedCalendarLife | Registration |
| **BatteryStatusData** | OperatingMode, StateCode, HvacOperatingMode, TmsPumpStatus, FanHeaterStatus, CompressorStatus, PumpStatus | Realtime |
| **BatteryEnergyData** | PowerKW, TotalVoltage, TotalCurrent, Temperature, RemainingEnergy, AvailableEnergy, MaxAllowableChargeI, MaxAllowableDischargeI, SoC, SoH | Realtime |
| **BatteryHealthData** | CycleCount, RoundTripEfficiency, EquivFullCycles, LifetimeCharged, LifetimeDischarged, DailyCharged, DailyDischarged, InitialRTE, EfficiencyDegradation | Realtime |
| **BatteryHealthCalcData** | TotalOperatingHours, TotalStandbyHours, CycleLifePct | Calculated |
| **BatteryPackEnergyData** | SoH, Voltage, Current, TempMin, TempMax, TempAvg, PositiveContactor, NegativeContactor, IsolationResistance | Realtime |
| **BatteryModuleEnergyData** | SoH, Voltage, Current, TempMin, TempMax, TempAvg | Realtime |
| **HvacStatusData** | AmbientTemp, OutletCoolantTemp, InletCoolantTemp, InletPressure, OutletPressure, CompressorPressure, PumpPressure, MaxRecordedTemp, AvgOperatingTemp | Realtime |

### PCS Nodes

| Node | Parameters | Source |
|------|-----------|--------|
| **PcsStatusData** | SourceMode, WorkingCondition, GridMode, PhaseSequence, DeratingCoefficient, Temperature, AmbientTemp, InductanceTemp | Realtime |
| **PcsAcMeasurementData** | GridFrequency, LineVoltages[], PhaseVoltages[], PhaseCurrents[], InductorCurrents[], AcPowerFactor | Realtime |
| **PcsDcMeasurementData** | TotalDcBus, DcBusPositive, DcBusNegative, BatteryVoltage, BatteryCurrent, TotalDcCurrent, DcPower, ActivePower[], ApparentPower[], ReactivePower[] | Realtime |
| **PcsChargeAlgorithmData** | CcCvRechargeV, CvChargeV, CvMaxTime, CvToFloatI, FloatChargeV, FloatToCcI | Realtime |
| **PcsEnergyData** | TotalYieldEnergy, TotalOperatingHours | Realtime |

### Solar Nodes

| Node | Parameters | Source |
|------|-----------|--------|
| **SolarNameplateData** | CommissioningDate, Manufacturer, DeviceName, DeviceModel, SerialNumber, Capacity | Registration/Installation |
| **SolarStatusData** | OperatingStatus, TodayActivePowerKW, TotalActivePower, TotalDcPower, PhaseCurrents[], TotalReactivePower, InsulationImpedance, GridFrequency, LineVoltages[], TotalPowerFactor, TotalYield, InverterApparentPower, DailyOperatingTime | Realtime |
| **SolarInverterData** | InverterName, SerialNumber, MpptData[]{MpptNumber, Current, Voltage, Strings[]{StringNumber, StringCurrent}} | Realtime |

### Wind Nodes

| Node | Parameters | Source |
|------|-----------|--------|
| **WindPowerData** | ActivePower, RatedPower, GeneratorPower, ReactivePower | Realtime |
| **WindConditionData** | WindSpeed, WindDirection, AmbientTemperature, GeneratorSpeed, RotorSpeed | Realtime |
| **WindElectricalData** | GridFrequency, GridVoltage, BladePitchAngles[], NacellePosition, YawError, GeneratorTemp, GearboxOilTemp, MainBearingTemp, ControlCabinetTemp | Realtime |
| **WindDailyEnergyData** | DailyOperatingTime, EnergyYieldToday | Realtime |
| **WindLifecycleData** | TotalOperatingHours, TotalYieldEnergy, LastMaintenance, NextMaintenanceDue, WarrantyExpiry, DesignedLife, AssetAge | Calculated |

### Grid Meter Nodes

| Node | Parameters | Source |
|------|-----------|--------|
| **GridMeterPowerData** | ActivePower, PowerFactor, ReactivePower, ApparentPower, PowerFlowDirection, PhaseVoltages[A,B,C], PhaseCurrents[A,B,C], TotalHarmonicDistortion | Realtime |
| **GridBreakerLifecycleData** | TotalOperations, FaultTrips, RatedOperations, OperationLifeUsed, DesignedLife | Calculated |

### Common Nodes

| Node | Parameters | Source |
|------|-----------|--------|
| **DeviceMaintenanceData** | LastMaintenance, NextMaintenanceDue, WarrantyExpiry | Calculated |
| **DeviceNameplateData** | DeviceName, DeviceType, DeviceModel, CommissioningDate, Manufacturer, SerialNumber | Registration/Installation |

---

## 8. LogicalDevice Catalog

| LogicalDevice | Category | LogicalNodes | Has Hierarchy |
|---|---|---|---|
| BatteryContainer | BESS | BatteryNameplateData, BatteryStatusData, BatteryEnergyData, BatteryHealthData, BatteryHealthCalcData, HvacStatusData, DeviceMaintenanceData | Yes: Battery → Rack → Pack → Module |
| BatteryRack | BESS | BatteryPackEnergyData | No |
| BatteryPack | BESS | BatteryModuleEnergyData | No |
| BatteryModule | BESS | (minimal — cell-level data) | No |
| PcsUnit | PCS | PcsStatusData, PcsAcMeasurementData, PcsDcMeasurementData, PcsChargeAlgorithmData, PcsEnergyData | No |
| SolarInverter | Solar | SolarNameplateData, SolarStatusData, SolarInverterData | Yes: MPPT → String |
| GridMeter | Grid | GridMeterPowerData, GridBreakerLifecycleData, DeviceNameplateData | No |
| WindTurbine | Wind | WindPowerData, WindConditionData, WindElectricalData, WindDailyEnergyData, WindLifecycleData, DeviceNameplateData | No |

---

## 9. Migration Strategy

### Incremental, not a clean break.

**Reason:** Frontend, seed data, alarm system, and queries all reference DeviceType/DeviceSpecificationTemplate. The embedded team is still iterating on IEC 61850 addressing.

### Phase A (Now): Add new tables alongside existing ones
- Create `LogicalNodes`, `LogicalDevices`, `LogicalDeviceNodes` tables
- Add nullable `LogicalDeviceId` FK to `Devices` table
- CRUD endpoints for catalog management
- Seed data for all nodes and devices from the parameter spreadsheet
- New `RegisterDeviceFromLogicalDevice` command (parallel to existing `RegisterDeviceComposite`)

### Phase B (Now): Compatibility layer
- Query handlers filter by EITHER `DeviceType.Name` OR `LogicalDevice.Category`
- Include both navigations in queries
- Both old-style and new-style devices work

### Phase C (After embedded team finalizes): Full migration
- Parse IEC 61850 addresses in ingestion
- Make `LogicalDeviceId` non-nullable
- Drop `DeviceTypeId`, `DeviceSpecificationTemplateId`, `Model`, `Manufacturer` from Device
- Migrate `DeviceAlarmType` to reference `LogicalDeviceId`
- Drop `DeviceType` and `DeviceSpecificationTemplate` tables
- Remove dual-filter logic and fallback extraction code

---

## 10. What Changes vs What Stays

| Component | Status |
|---|---|
| `DeviceType` entity | **Stays** during transition → dropped in Phase C |
| `DeviceSpecificationTemplate` entity | **Stays** during transition → dropped in Phase C |
| `Device.Model` / `Device.Manufacturer` | **Stays** as informational → moves to Metadata in Phase C |
| `ParameterMapTemplate` (monolithic JSON) | **Replaced** by LogicalNode.Parameters |
| `ParameterResolver` / `FormulaEvaluator` | **Stays** — still needed for Calculated params |
| `DeviceDataCategorizer` | **Stays** — extended with `LogicalDevice.Category` routing |
| `ContainerTelemetryLoader` | **Updated** to check LogicalDevice.Category first |
| `HierarchyTemplate` concept | **Migrated** from DeviceSpecificationTemplate to LogicalDevice |
| `SpecificationsTemplate` (UI schema) | **Migrated** to LogicalDevice.UiTemplate |
| `MetadataTemplate` | **Dropped** — metadata is instance-level, set via command |
| Port definitions | **Migrated** to LogicalDevice.Ports (catalog) + Device.Ports (instance) |

---

## 11. Open Questions for Team Discussion

1. **Node granularity:** Should `BatteryHealthData` be one node with mixed source types, or split into `BatteryHealthRealtimeData` (Realtime) and `BatteryHealthCalcData` (Calculated)?

2. **Sub-device flexibility:** Lower-level devices (Rack, Pack, Module) could be LogicalDevices OR just LogicalNodes depending on whether they're physically separate. The embedded team needs to confirm which levels are always physical devices vs. logical groupings.

3. **Addressing finalization:** The IEC 61850-like addressing (`CAB1/NodeName.param.path`) is still being developed. The exact format affects ingestion parsing. We should design the ingestion parser to be pluggable.

4. **Wind turbine scope:** Wind turbine nodes are defined in the spreadsheet but no backend code exists for wind yet. Should we seed the catalog definitions now and defer the query handlers?

5. **Formula evaluation location:** Should Calculated parameters be computed at ingestion time (store the result) or at query time (compute on the fly)? Current approach is query-time, but ingestion-time would reduce query complexity.

6. **Alarm system migration:** `DeviceAlarmType` currently references `DeviceTypeId`. When should this migrate to `LogicalDeviceId`? This affects alarm seeding and the alarm processing service.

7. **Engineering tool integration:** The engineering tool uploads topology JSON. What format? Should the backend validate it against the LogicalDevice catalog (e.g., "this port doesn't exist on this device type")?
