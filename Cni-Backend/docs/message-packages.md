# Message Package Specification

> **Version:** 1.0
> **Last Updated:** 2026-03-06
> **Audience:** Embedded team, backend team, integration developers

---

## Table of Contents

1. [Overview](#1-overview)
2. [Naming Convention](#2-naming-convention)
3. [Frequency Tiers](#3-frequency-tiers)
4. [Message Envelope Format](#4-message-envelope-format)
5. [Battery (BESS) Message Packages](#5-battery-bess-message-packages)
6. [PCS Message Packages](#6-pcs-message-packages)
7. [Solar Message Packages](#7-solar-message-packages)
8. [Wind Message Packages](#8-wind-message-packages)
9. [Grid Message Packages](#9-grid-message-packages)
10. [Common Message Packages](#10-common-message-packages)
11. [Alarm Message Packages](#11-alarm-message-packages)
12. [Third-Party API Adapters](#12-third-party-api-adapters)
13. [Appendix](#13-appendix)

---

## 1. Overview

### 1.1 Purpose

This document defines all telemetry message packages exchanged between RTUs and the CNI backend via EMQX. It also describes how third-party APIs (e.g., Huawei FusionSolar) map their data into the same standardized parameter structure.

All teams must use this document as the single source of truth for:

- **Embedded team**: what to publish and how to name it
- **Backend team**: what to expect in the `Payload` JSON blob and how to parse it
- **Integration developers**: how to map third-party API responses into our standard packages

### 1.2 Data Sources

| Source | Description | Flow |
|---|---|---|
| **RTU** | Direct telemetry from physical devices via Modbus/CAN | RTU -> EMQX webhook -> `POST /api/v1/DeviceData/Ingest` |
| **Backend (Calculated)** | Derived metrics computed by the backend from raw telemetry | Internal computation, stored on-demand |
| **Third-Party API** | External cloud APIs (FusionSolar, etc.) polled by adapter services | Adapter -> translated to standard packages -> same ingest path |

### 1.3 Message Flow

```
RTU First Boot:
  RTU --[bus scan]--> discovers devices with serial numbers
  RTU --[MQTT]--> Rtu.Handshake --> Backend binds serials + actual counts to topology

RTU Telemetry:
  RTU --[MQTT]--> EMQX --[Webhook POST]--> /api/v1/DeviceData/Ingest --> IngestDeviceDataHandler
                                                                            |-> Resolve (rtuSerialNumber, deviceName) → Device
                                                                            |-> DeviceDataCategorizer (kind → category)
                                                                            |-> Store typed entity (with path if sub-device)
                                                                            |-> AlarmProcessingService
                                                                            |-> Update LastSeenUtc

Third-Party API:
RTU Alarms:
  RTU --[MQTT]--> EMQX --[Webhook POST]--> /api/v1/DeviceData/Ingest/Alarm --> IngestDeviceAlarmHandler
                                                                                  |-> Resolve (rtuSerialNumber, deviceName) → Device
                                                                                  |-> Resolve or auto-create DeviceAlarmType by code
                                                                                  |-> Create/recover DeviceAlarm records
                                                                                  |-> Snapshot comparison (auto-recover missing)

Third-Party API (Telemetry):
  Adapter Service --[HTTP poll]--> FusionSolar API
                   --[translate]--> Standard envelope (deviceName, kind, payload)
                   --[POST]-------> /api/v1/DeviceData/Ingest --> (same pipeline)

Third-Party API (Alarms):
  Adapter Service --[HTTP poll]--> FusionSolar getAlarmList API
                   --[translate]--> Alarm envelope (rtuSn, deviceName, alarms[])
                   --[POST]-------> /api/v1/DeviceData/Ingest/Alarm --> (same alarm pipeline)
```

### 1.4 Data Ownership: RTU vs Backend

The backend and RTU have distinct responsibilities for data:

| Data Category | Owner | How It Gets to Backend |
|---|---|---|
| **Device topology** (device names, types, manufacturers, connections) | Database | Engineering tool exports a topology JSON at commissioning |
| **Internal structure schema** (max racks, packs, modules per device) | Database | `DeviceSpecificationTemplate.HierarchyTemplate` |
| **Device serial numbers** | RTU | RTU reports actual serial numbers during handshake |
| **Actual sub-device counts** (e.g., 10 racks out of 14 max) | RTU | Reported in handshake `actualCounts` |
| **Device specifications** (rated capacity, chemistry, etc.) | Database | `DeviceSpecificationTemplate` populated at commissioning |
| **Telemetry** (SoC, voltage, power, temperatures) | RTU | EMQX messages → Ingest endpoint |
| **Runtime info** (firmware version, boot time) | RTU | `Device.RuntimeInfo` / `Battery.Nameplate` packages |
| **Computed metrics** (RUL, efficiency trends, maintenance) | Backend | Calculated from stored telemetry on-demand |

**Key principle:** The backend is **vendor-agnostic and model-agnostic** at runtime — it works with device types (Battery, PCS, Solar, Grid), not brands or models. The RTU team handles all vendor-specific protocol mapping (Modbus registers, CAN frames, etc.) and delivers data in our standardized parameter format. The backend does not know serial numbers or actual sub-device counts until the RTU reports them via handshake. If a parameter is missing from a telemetry message, the backend simply does not display it.

#### RTU Handshake: Binding Serial Numbers to the Topology

The engineering tool defines **what** is in the field (device names, types, manufacturers, connections) but not the actual serial numbers of the physical devices. The RTU is the only entity that can discover real serial numbers by scanning devices on its bus (Modbus, CAN, etc.).

**The problem:** The backend has a topology entry like "Battery1 — Tesla, PCS1 — Sungrow" but no serial numbers. The RTU knows the serial numbers but not the backend's device names.

**The solution:** An RTU handshake at first boot (or reconnection) that binds serial numbers and runtime details to the known topology:

```
RTU boots
  │
  ├─ 1. RTU scans its bus → discovers all connected devices with serial numbers
  │
  ├─ 2. RTU sends Rtu.Handshake message to backend via EMQX
  │     (contains: RTU serial, list of discovered devices with name + serial + actual counts)
  │
  ├─ 3. Backend matches devices by name against the expected topology
  │
  ├─ 4. Backend stores serial numbers on the Device records
  │     Backend updates actual sub-device counts (e.g., rack count) if different from template max
  │
  └─ 5. Backend responds with acknowledgement
        All subsequent telemetry uses deviceName for identification.
```

**Example:** An RTU managing a BESS facility reports during handshake:

```json
{
  "rtuSerialNumber": "RTU-001",
  "kind": "Rtu.Handshake",
  "timestamp": "2026-03-06T08:00:00Z",
  "payload": {
    "devices": [
      {
        "deviceName": "Battery1",
        "serialNumber": "BAT-CATL-88712",
        "firmwareVersion": "2.1.0",
        "actualCounts": { "rack": 10, "packPerRack": 4, "modulePerPack": 8 }
      },
      {
        "deviceName": "PCS1",
        "serialNumber": "PCS-SUN-44201",
        "firmwareVersion": "3.0.1"
      },
      {
        "deviceName": "Inverter1",
        "serialNumber": "INV-TES-20250101",
        "firmwareVersion": "1.5.2"
      },
      {
        "deviceName": "MPPT1",
        "serialNumber": "MPPT-SUN-30042",
        "firmwareVersion": "1.0.0"
      }
    ]
  }
}
```

The backend matches each `deviceName` against the topology. Key behaviors:

- **Serial number binding** — stored on the `Device` record for reference, but `deviceName` (not serial) is the routing key for all subsequent telemetry.
- **Actual counts vs template max** — for battery, the template may say "up to 14 racks" but the handshake reports `"rack": 10`. The backend stores the actual count and uses it for telemetry validation.
- **Unexpected device** — if the RTU reports a `deviceName` not in the topology, the backend raises an alarm.
- **Missing device** — if an expected device is not reported, the backend marks it as offline/missing.

> **Note:** The exact handshake protocol (request/response vs fire-and-forget, MQTT topics, retry logic) is outside the scope of this document and will be defined in a separate RTU communication protocol spec.

#### RTU Replacement and Relocation

The RTU is a communication gateway — devices belong to the **facility**, not to the RTU. When an RTU is replaced or relocated:

**RTU replacement (same facility, same devices):**
1. Backend admin updates the `Rtu` record with the new RTU's serial number
2. New RTU boots and sends handshake with its serial → backend recognizes it
3. All device names and associations remain unchanged
4. Telemetry resumes normally

**RTU relocation (moved to a different facility):**
1. Backend admin reassigns the `Rtu` record to the new facility
2. Devices at the old facility are reassigned to a replacement RTU (or marked as unassigned)
3. The relocated RTU performs handshake at the new facility with whatever devices are there

In both cases, `deviceName` stability is preserved because names come from the engineering tool topology, not from the RTU hardware.

#### Device Hierarchy: Physical vs Internal

Not all levels in a device tree are physical devices. The backend distinguishes:

| Level | Example | Has Device Record | Has Serial Number | Identified By |
|---|---|---|---|---|
| **Physical device** | Battery container, PCS, Inverter, MPPT | Yes | Yes (from handshake) | `deviceName` |
| **Internal structure** | Rack, Pack, Module, Cell | No | No | `path` in envelope (e.g., `Rack/2/Pack/1`) |

**Physical devices** are defined by the engineering tool topology and registered as `Device` rows. Parent-child relationships between physical devices (e.g., Cabinet → Battery, Cabinet → PCS) use `Device.ParentDeviceId`.

**Internal structure** (racks, packs, modules) is defined by the `DeviceSpecificationTemplate.HierarchyTemplate` as a maximum capacity schema. The actual counts are reported by the RTU during handshake. Internal structure is addressed via the `path` field in telemetry messages — no `Device` rows are created for these levels.

**Telemetry-based validation:** At runtime, the backend can count distinct `path` values per package type (e.g., count unique `Rack/*` paths in `Battery.RackEnergy` messages). A mismatch between the handshake-reported count and observed telemetry paths triggers an alarm.

#### Vendor-Specific Extensions

Some devices support features beyond the standard packages (e.g., a vendor-specific `InverterPowerRegulator`). The backend handles this via:

- **Extended packages** — The RTU can send additional `Kind` values not in the standard catalog (e.g., `Solar.PowerRegulator`). The backend stores these as-is in the payload JSON blob.
- The backend does not need to understand the internal structure of vendor extensions. It stores and forwards the data. Frontend rendering for extended parameters is driven by the `DeviceSpecificationTemplate`.

### 1.5 Relationship to Code

| Concept | Code Reference |
|---|---|
| Package name | `Kind` field in `DeviceDataBase` entities |
| Parameter keys | `ParameterKeys` constants (`src/Domain/Constants/ParameterKeys.cs`) |
| Device types | `DeviceTypeNames` constants (`src/Domain/Constants/DeviceTypeNames.cs`) |
| Categorization | `DeviceDataCategorizer.Categorize(kind)` maps `Kind` prefix to `DeviceDataCategory` |
| Storage | `DeviceBatteryData`, `DevicePcsData`, `DeviceSolarData`, `DeviceSmartMeterData` |

---

## 2. Naming Convention

### 2.1 Package Names: `{DeviceType}.{Purpose}`

Package names appear as the `Kind` field in the EMQX message and in stored `DeviceDataBase` records.

**Format:** `{DeviceType}.{Purpose}`

**Rules:**
- PascalCase, no spaces, no dots within segments
- Maximum 2 segments separated by a single dot
- `DeviceType` prefix must match a known device type (see table below)
- `Purpose` suffix describes **what** the data is about

**Device type prefixes:**

| Prefix | Maps to `DeviceDataCategory` | EF Entity |
|---|---|---|
| `Battery` | Bess | `DeviceBatteryData` |
| `Pcs` | Pcs | `DevicePcsData` |
| `Solar` | Solar | `DeviceSolarData` |
| `Wind` | SmartMeter* | `DeviceSmartMeterData` |
| `Grid` | SmartMeter | `DeviceSmartMeterData` |
| `Device` | (varies) | (varies) |

> \* Wind does not yet have a dedicated `DeviceDataCategory`. Until one is added, Wind telemetry is stored as `SmartMeter` category. This is a known limitation to address in Phase 6.

**Examples:**
- `Battery.Status` — battery operating state and fault flags
- `Battery.Energy` — fast-changing power, voltage, current, SoC
- `Pcs.AcMeasurement` — PCS AC side voltages, currents, power factor
- `Solar.Power` — solar inverter real-time output
- `Grid.Power` — grid meter power readings

### 2.2 Parameter Keys: `camelCase`

All parameter keys within the `Payload` JSON use `camelCase`.

**Rules:**
- Same concept = same key name across all packages (e.g., `voltage` means the same thing everywhere)
- Add a unit suffix only when ambiguous: `tempC` vs `pressureBar`
- Prefer short, self-descriptive names: `soc`, `activePower`, `gridFrequency`
- Boolean flags use `is` prefix: `isCharging`, `isConnected`
- Array parameters use plural: `cellVoltages`, `rackStatuses`
- Prefix with device context when used outside the device's own package: `pcsActivePower` in a Battery package

### 2.3 Examples

```
Package Name         Parameter Key       Description
────────────────     ──────────────      ──────────────────────────
Battery.Energy       soc                 State of charge (%)
Battery.Energy       voltage             Total battery voltage (V)
Battery.Energy       current             Total battery current (A)
Battery.Energy       power               Total battery power (kW)
Pcs.AcMeasurement    pcsGridFreq         Grid frequency (Hz)
Pcs.DcMeasurement    pcsBatteryVoltage   Battery-side DC voltage (V)
Grid.Power           activePower         Active power (kW)
```

---

## 3. Frequency Tiers

Each message package has an assigned frequency tier that defines how often RTUs should publish it.

| Tier | Label | Interval | Typical Use |
|---|---|---|---|
| **Registration** | `registration` | Once (on device boot/provision) | Static nameplate data |
| **Realtime-Fast** | `realtime-fast` | 1-5 seconds | Operational state, power, voltage, current |
| **Realtime-Medium** | `realtime-medium` | 10s - 5 min | Cell arrays, MPPT, breaker status, thermal |
| **Realtime-Slow** | `realtime-slow` | Hourly / Daily / Weekly | SoH, cycle counts, lifecycle energy |
| **Config** | `config` | On change / On request | Charge algorithm parameters, setpoints |
| **Calculated** | `calculated` | On-demand (backend) | Computed metrics, not sent by RTU |

---

## 4. Message Envelope Format

Every EMQX message uses this standard JSON envelope:

```json
{
  "rtuSerialNumber": "RTU-001",
  "deviceName": "Battery1",
  "kind": "Battery.Energy",
  "path": null,
  "timestamp": "2026-03-06T12:00:00Z",
  "payload": {
    "soc": 85.2,
    "voltage": 748.5,
    "current": -120.3,
    "power": 89.9
  }
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `rtuSerialNumber` | string | Yes | Serial number of the RTU sending this data |
| `deviceName` | string | Yes | Name of the physical device (unique per RTU), as defined by the engineering tool topology |
| `kind` | string | Yes | Package name (e.g., `Battery.Energy`) — used by `DeviceDataCategorizer` |
| `path` | string | No | Sub-device address within the device for internal structure (e.g., `Rack/2`, `Rack/2/Pack/1/Module/3`). `null` for device-level data. |
| `timestamp` | string (ISO 8601) | Yes | UTC timestamp when the measurement was taken |
| `payload` | object | Yes | Key-value pairs defined by the package specification |

**Device resolution:** The backend resolves `(rtuSerialNumber, deviceName)` → `Device` record. The `path` field is used only to identify sub-device structure (racks, packs, modules) that do not have their own `Device` records.

**Path format:** Alternating `Level/Index`, slash-separated. Examples:

| Path | Meaning |
|---|---|
| `null` | Device-level (container, inverter, PCS, etc.) |
| `Rack/2` | Rack index 2 within the device |
| `Rack/2/Pack/1` | Pack index 1 within Rack 2 |
| `Rack/2/Pack/1/Module/3` | Module index 3 within Pack 1 of Rack 2 |

**Examples by device type:**

Container-level battery data:
```json
{ "rtuSerialNumber": "RTU-001", "deviceName": "Battery1", "kind": "Battery.Energy", "path": null, "timestamp": "...", "payload": { "soc": 85.2 } }
```

Rack-level data (internal structure, no separate Device record):
```json
{ "rtuSerialNumber": "RTU-001", "deviceName": "Battery1", "kind": "Battery.RackEnergy", "path": "Rack/2", "timestamp": "...", "payload": { "soc": 87.1 } }
```

PCS data (physical device, no path):
```json
{ "rtuSerialNumber": "RTU-001", "deviceName": "PCS1", "kind": "Pcs.Status", "path": null, "timestamp": "...", "payload": { "pcsWorkingCondition": "Running" } }
```

Solar MPPT data (physical sub-device with its own name):
```json
{ "rtuSerialNumber": "RTU-001", "deviceName": "MPPT1", "kind": "Solar.Mppt", "path": null, "timestamp": "...", "payload": { "mpptVoltages": [380.2] } }
```

**Batch endpoint** (`POST /api/v1/DeviceData/Ingest/Batch`) accepts an array of envelopes:

```json
{
  "messages": [
    { "rtuSerialNumber": "RTU-001", "deviceName": "Battery1", "kind": "Battery.Energy", "path": null, "timestamp": "...", "payload": { ... } },
    { "rtuSerialNumber": "RTU-001", "deviceName": "Battery1", "kind": "Battery.RackEnergy", "path": "Rack/1", "timestamp": "...", "payload": { ... } },
    { "rtuSerialNumber": "RTU-001", "deviceName": "PCS1", "kind": "Pcs.Status", "path": null, "timestamp": "...", "payload": { ... } }
  ]
}
```

**Authentication:** All ingest requests require the `X-API-Key` header matching `IngestSettings:ApiKey`.

---

## 5. Battery (BESS) Message Packages

### 5.1 Battery.Nameplate

> **Frequency:** Registration (once, on boot/firmware update) | **Source:** RTU

Runtime identity and specification data reported by the RTU. Static device metadata (manufacturer, model, serial number, install date, location) is **not** part of this package — it is stored in the database at commissioning time via the engineering tool.

| Parameter | Type | Unit | Description |
|---|---|---|---|
| `firmwareVersion` | string | — | Current firmware version |
| `nominalCapacity` | number | kWh | Rated energy capacity |
| `nominalVoltage` | number | V | Rated voltage |
| `nominalPower` | number | kW | Rated power |
| `maxChargeCurrent` | number | A | Maximum charge current |
| `maxDischargeCurrent` | number | A | Maximum discharge current |
| `cellChemistry` | string | — | Cell chemistry type (e.g., `LFP`, `NMC`) |
| `rackCount` | integer | — | Number of racks in the container |
| `modulesPerRack` | integer | — | Number of modules per rack |
| `cellsPerModule` | integer | — | Number of cells per module |

### 5.2 Battery.Status

> **Frequency:** Realtime-Fast (1-5s) | **Source:** RTU

Operating state, mode, and alarm flags.

| Parameter | Type | Unit | Description |
|---|---|---|---|
| `operatingMode` | string | — | Current mode: `Charging`, `Discharging`, `Idle`, `Standby`, `Fault` |
| `isCharging` | boolean | — | True if currently charging |
| `isConnected` | boolean | — | Contactor/relay connected |
| `alarmActive` | boolean | — | Any active alarm |
| `faultCode` | integer | — | Current fault code (0 = none) |
| `warningFlags` | integer | — | Bitmask of active warnings |
| `protectionFlags` | integer | — | Bitmask of active protections (over-temp, over-voltage, etc.) |

### 5.3 Battery.Energy

> **Frequency:** Realtime-Fast (1-5s) | **Source:** RTU

Fast-changing electrical measurements. This is the primary package for real-time battery monitoring.

| Parameter | Type | Unit | Description |
|---|---|---|---|
| `soc` | number | % | State of Charge |
| `voltage` | number | V | Total battery voltage |
| `current` | number | A | Total battery current (positive = charging) |
| `power` | number | kW | Total battery power |
| `powerDirection` | string | — | `Charging`, `Discharging`, or `Idle` |
| `remainingEnergy` | number | kWh | Remaining usable energy |
| `availableEnergy` | number | kWh | Available energy capacity |
| `tempMin` | number | C | Minimum cell temperature |
| `tempMax` | number | C | Maximum cell temperature |
| `tempAvg` | number | C | Average cell temperature |
| `cellVoltageMax` | number | mV | Highest cell voltage |
| `cellVoltageMin` | number | mV | Lowest cell voltage |
| `cellVoltageAvg` | number | mV | Average cell voltage |
| `voltageSpread` | number | mV | Max - Min cell voltage delta |
| `cellTempMax` | number | C | Highest individual cell temperature |
| `cellTempMin` | number | C | Lowest individual cell temperature |
| `maxCellVoltageId` | string | — | Cell ID with highest voltage |
| `minCellVoltageId` | string | — | Cell ID with lowest voltage |
| `ambientTemp` | number | C | Container ambient temperature |

### 5.4 Battery.Soh

> **Frequency:** Realtime-Slow (daily/weekly) | **Source:** RTU

State of Health, sampled infrequently.

| Parameter | Type | Unit | Description |
|---|---|---|---|
| `soh` | number | % | State of Health |

### 5.5 Battery.CellArray

> **Frequency:** Realtime-Medium (30s-5min) | **Source:** RTU

Full cell-level diagnostic array for detailed monitoring. Sent with the battery's `deviceName` and a `path` to identify the scope (e.g., `path: "Rack/2"` for all cells in rack 2, or `path: null` for the entire container).

| Parameter | Type | Unit | Description |
|---|---|---|---|
| `cells` | array | — | Array of cell objects (see below) |

Each cell object:

| Field | Type | Unit | Description |
|---|---|---|---|
| `id` | string | — | Cell identifier (e.g., `R1-M3-C12`) |
| `voltage` | number | mV | Cell voltage |
| `temperature` | number | C | Cell temperature |
| `balancing` | boolean | — | Cell balancing active |

### 5.6 Battery.Health

> **Frequency:** Realtime-Slow (hourly/daily) | **Source:** RTU

Lifecycle metrics reported by the BMS.

| Parameter | Type | Unit | Description |
|---|---|---|---|
| `cycleCount` | integer | — | Total charge/discharge cycles |
| `cycleLifePct` | number | % | Cycle life consumed |
| `calendarLifePct` | number | % | Calendar life consumed |
| `rte` | number | % | Round-trip efficiency |
| `totalChargeEnergy` | number | kWh | Lifetime energy charged |
| `totalDischargeEnergy` | number | kWh | Lifetime energy discharged |
| `deepDischargeCount` | integer | — | Number of deep discharge events |
| `lastFullChargeDate` | string | ISO date | Date of last full charge |
| `operatingHours` | number | hours | Total operating hours |

### 5.7 Battery.HealthCalc (Backend)

> **Frequency:** Calculated (on-demand) | **Source:** Backend

Computed lifecycle metrics derived from raw telemetry. Not sent by RTU.

| Parameter | Type | Unit | Description |
|---|---|---|---|
| `estimatedRul` | number | years | Estimated remaining useful life |
| `projectedEol` | string | ISO date | Projected end-of-life date |
| `avgDailyCycles` | number | — | Average daily cycle count |
| `avgRte` | number | % | Average round-trip efficiency (30-day rolling) |
| `capacityFadeRate` | number | %/year | Capacity degradation rate |
| `equivalentFullCycles` | number | — | Equivalent full cycles (normalized to 100% DoD) |

### 5.8 Battery.RackEnergy

> **Frequency:** Realtime-Fast (1-5s) | **Source:** RTU

Per-rack level measurements. Published once per rack using the parent battery's `deviceName` with a `path` to identify the rack (e.g., `path: "Rack/2"`). Racks are internal structure — they do not have their own `Device` records or serial numbers.

| Parameter | Type | Unit | Description |
|---|---|---|---|
| `soc` | number | % | Rack state of charge |
| `voltage` | number | V | Rack total voltage |
| `current` | number | A | Rack current |
| `power` | number | kW | Rack power |
| `tempMin` | number | C | Rack minimum temperature |
| `tempMax` | number | C | Rack maximum temperature |
| `tempAvg` | number | C | Rack average temperature |
| `cellVoltageMax` | number | mV | Highest cell voltage in rack |
| `cellVoltageMin` | number | mV | Lowest cell voltage in rack |
| `contactorStatus` | string | — | `Open`, `Closed`, `Fault` |
| `balancingActive` | boolean | — | Cell balancing in progress |

### 5.9 Battery.ModuleEnergy

> **Frequency:** Realtime-Fast (1-5s) | **Source:** RTU

Per-module level measurements. Published once per module using the parent battery's `deviceName` with a full `path` (e.g., `path: "Rack/2/Pack/1/Module/3"`). Modules are internal structure — they do not have their own `Device` records.

| Parameter | Type | Unit | Description |
|---|---|---|---|
| `voltage` | number | V | Module voltage |
| `current` | number | A | Module current |
| `tempMin` | number | C | Module minimum temperature |
| `tempMax` | number | C | Module maximum temperature |
| `cellVoltageMax` | number | mV | Highest cell voltage in module |
| `cellVoltageMin` | number | mV | Lowest cell voltage in module |
| `cellCount` | integer | — | Number of cells in module |

### 5.10 Battery.Thermal

> **Frequency:** Realtime-Medium (10-30s) | **Source:** RTU

HVAC / Thermal Management System (TMS) data for the container.

| Parameter | Type | Unit | Description |
|---|---|---|---|
| `chillerCoolantOut` | number | C | Coolant outlet temperature |
| `chillerCoolantIn` | number | C | Coolant inlet temperature |
| `compressorRunning` | boolean | — | Compressor running state |
| `compressorSpeed` | number | RPM | Compressor speed |
| `fanSpeed` | number | RPM | Fan speed |
| `heaterActive` | boolean | — | Heater engaged |
| `coolantFlowRate` | number | L/min | Coolant flow rate |
| `coolantPressure` | number | bar | Coolant system pressure |
| `ambientTemp` | number | C | External ambient temperature |
| `containerTemp` | number | C | Internal container temperature |
| `setpointTemp` | number | C | Target temperature setpoint |
| `mode` | string | — | `Cooling`, `Heating`, `Idle`, `Fault` |
| `dehumidifierActive` | boolean | — | Dehumidifier running |
| `coolantPressure` | number | bar | Coolant pressure |

---

## 6. PCS Message Packages

### 6.1 Pcs.Status

> **Frequency:** Realtime-Fast (1-5s) | **Source:** RTU

PCS operating state and key indicators.

| Parameter | Type | Unit | Description |
|---|---|---|---|
| `pcsWorkingCondition` | string | — | `Running`, `Standby`, `Fault`, `Off` |
| `pcsSourceMode` | string | — | `GridTied`, `Islanded`, `Transition` |
| `pcsGridMode` | string | — | `OnGrid`, `OffGrid` |
| `pcsPhaseSequence` | string | — | `Positive`, `Negative` |
| `pcsDeratingCoefficient` | number | % | Active derating factor |
| `pcsIgbtTemp` | number | C | IGBT module temperature |
| `pcsAmbientTemp` | number | C | PCS enclosure ambient temperature |
| `pcsInductanceTemp` | number | C | Inductance coil temperature |
| `pcsEfficiency` | number | % | Current conversion efficiency |

### 6.2 Pcs.AcMeasurement

> **Frequency:** Realtime-Fast (1-5s) | **Source:** RTU

AC side electrical measurements.

| Parameter | Type | Unit | Description |
|---|---|---|---|
| `pcsGridFreq` | number | Hz | Grid frequency |
| `pcsLineVoltageAB` | number | V | Line voltage A-B |
| `pcsLineVoltageBC` | number | V | Line voltage B-C |
| `pcsLineVoltageCA` | number | V | Line voltage C-A |
| `pcsPhaseVoltageA` | number | V | Phase A voltage |
| `pcsPhaseVoltageB` | number | V | Phase B voltage |
| `pcsPhaseVoltageC` | number | V | Phase C voltage |
| `pcsPhaseCurrentA` | number | A | Phase A current |
| `pcsPhaseCurrentB` | number | A | Phase B current |
| `pcsPhaseCurrentC` | number | A | Phase C current |
| `pcsInductorCurrentA` | number | A | Inductor current A |
| `pcsInductorCurrentB` | number | A | Inductor current B |
| `pcsInductorCurrentC` | number | A | Inductor current C |
| `pcsPowerFactor` | number | — | Power factor (-1.0 to 1.0) |

### 6.3 Pcs.DcMeasurement

> **Frequency:** Realtime-Fast (1-5s) | **Source:** RTU

DC side electrical measurements.

| Parameter | Type | Unit | Description |
|---|---|---|---|
| `pcsTotalDcBusVoltage` | number | V | Total DC bus voltage |
| `pcsDcBusPosVoltage` | number | V | Positive DC bus voltage |
| `pcsDcBusNegVoltage` | number | V | Negative DC bus voltage |
| `pcsBatteryVoltage` | number | V | Battery-side DC voltage |
| `pcsBatteryCurrent` | number | A | Battery-side DC current |
| `pcsTotalDcCurrent` | number | A | Total DC current |
| `pcsDcPower` | number | kW | Total DC power |

### 6.4 Pcs.PowerDistribution

> **Frequency:** Realtime-Fast (1-5s) | **Source:** RTU

Per-phase power breakdown.

| Parameter | Type | Unit | Description |
|---|---|---|---|
| `pcsActivePower` | number | kW | Total active power |
| `pcsActivePowerA` | number | kW | Phase A active power |
| `pcsActivePowerB` | number | kW | Phase B active power |
| `pcsActivePowerC` | number | kW | Phase C active power |
| `pcsReactivePower` | number | kVAR | Total reactive power |
| `pcsReactivePowerA` | number | kVAR | Phase A reactive power |
| `pcsReactivePowerB` | number | kVAR | Phase B reactive power |
| `pcsReactivePowerC` | number | kVAR | Phase C reactive power |
| `pcsApparentPowerTotal` | number | kVA | Total apparent power |
| `pcsApparentPowerA` | number | kVA | Phase A apparent power |
| `pcsApparentPowerB` | number | kVA | Phase B apparent power |
| `pcsApparentPowerC` | number | kVA | Phase C apparent power |

### 6.5 Pcs.ChargeAlgorithm

> **Frequency:** Config (on change) | **Source:** RTU

CC-CV charge algorithm parameters. Sent when configuration changes.

| Parameter | Type | Unit | Description |
|---|---|---|---|
| `pcsCcCvRechargeV` | number | V | CC-CV recharge voltage threshold |
| `pcsCvChargeV` | number | V | CV charge voltage setpoint |
| `pcsCvMaxTime` | number | min | Maximum CV charge duration |
| `pcsCvFloatI` | number | A | CV-to-float transition current |
| `pcsFloatChargeV` | number | V | Float charge voltage |
| `pcsFloatCcI` | number | A | Float CC charge current |

### 6.6 Pcs.Energy

> **Frequency:** Realtime-Medium (5min) | **Source:** RTU

Cumulative energy and uptime counters.

| Parameter | Type | Unit | Description |
|---|---|---|---|
| `totalYield` | number | kWh | Lifetime energy throughput |
| `operatingHours` | number | hours | Total PCS operating hours |

---

## 7. Solar Message Packages

### 7.1 Solar.Power

> **Frequency:** Realtime-Fast (1-5s) | **Source:** RTU

Solar inverter real-time output. Main monitoring package for solar assets.

| Parameter | Type | Unit | Description |
|---|---|---|---|
| `activePower` | number | kW | AC active output power |
| `reactivePower` | number | kVAR | AC reactive power |
| `apparentPower` | number | kVA | AC apparent power |
| `powerFactor` | number | — | Power factor |
| `gridFrequency` | number | Hz | Grid frequency |
| `phaseVoltageA` | number | V | Phase A voltage |
| `phaseVoltageB` | number | V | Phase B voltage |
| `phaseVoltageC` | number | V | Phase C voltage |
| `phaseCurrentA` | number | A | Phase A current |
| `phaseCurrentB` | number | A | Phase B current |
| `phaseCurrentC` | number | A | Phase C current |
| `dcPower` | number | kW | Total DC input power |
| `dcVoltage` | number | V | DC bus voltage |
| `dcCurrent` | number | A | Total DC input current |
| `efficiency` | number | % | DC-to-AC conversion efficiency |
| `cabinetTemp` | number | C | Inverter cabinet temperature |
| `dailyYield` | number | kWh | Energy produced today |
| `totalYield` | number | kWh | Lifetime energy produced |

### 7.2 Solar.Mppt

> **Frequency:** Realtime-Medium (10-30s) | **Source:** RTU

Per-MPPT tracker data. Published once per inverter; arrays contain all MPPT channels.

| Parameter | Type | Unit | Description |
|---|---|---|---|
| `mpptVoltages` | number[] | V | Voltage per MPPT tracker |
| `mpptCurrents` | number[] | A | Current per MPPT tracker |
| `mpptPowers` | number[] | kW | Power per MPPT tracker |
| `strings` | array | — | Per-string current measurements (see below) |

Each string object:

| Field | Type | Unit | Description |
|---|---|---|---|
| `mpptIndex` | integer | — | Which MPPT this string belongs to |
| `stringIndex` | integer | — | String index within the MPPT |
| `current` | number | A | String current |

---

## 8. Wind Message Packages

### 8.1 Wind.Power

> **Frequency:** Realtime-Fast (1-5s) | **Source:** RTU

Wind turbine power output.

| Parameter | Type | Unit | Description |
|---|---|---|---|
| `activePower` | number | kW | Active power output |
| `ratedPower` | number | kW | Turbine rated power |
| `generatorPower` | number | kW | Generator output power |
| `reactivePower` | number | kVAR | Reactive power |

### 8.2 Wind.Condition

> **Frequency:** Realtime-Fast (1-5s) | **Source:** RTU

Environmental and mechanical conditions.

| Parameter | Type | Unit | Description |
|---|---|---|---|
| `windSpeed` | number | m/s | Wind speed at hub height |
| `windDirection` | number | degrees | Wind direction (0-360) |
| `ambientTemp` | number | C | Ambient temperature |
| `rotorRpm` | number | RPM | Rotor speed |
| `generatorRpm` | number | RPM | Generator speed |

### 8.3 Wind.Electrical

> **Frequency:** Realtime-Fast (1-5s) | **Source:** RTU

Grid-side electrical measurements.

| Parameter | Type | Unit | Description |
|---|---|---|---|
| `gridFrequency` | number | Hz | Grid frequency |
| `gridVoltage` | number | V | Grid voltage (average line-to-line) |
| `powerFactor` | number | — | Power factor |

### 8.4 Wind.Mechanical

> **Frequency:** Realtime-Medium (10-30s) | **Source:** RTU

Mechanical component positions and angles.

| Parameter | Type | Unit | Description |
|---|---|---|---|
| `pitchAngleBlade1` | number | degrees | Blade 1 pitch angle |
| `pitchAngleBlade2` | number | degrees | Blade 2 pitch angle |
| `pitchAngleBlade3` | number | degrees | Blade 3 pitch angle |
| `nacelleDirection` | number | degrees | Nacelle orientation (0-360) |
| `yawError` | number | degrees | Yaw misalignment |

### 8.5 Wind.DriveTrainTemp

> **Frequency:** Realtime-Medium (30s-1min) | **Source:** RTU

Drive train temperature monitoring.

| Parameter | Type | Unit | Description |
|---|---|---|---|
| `generatorWindingTemp` | number | C | Generator winding temperature |
| `generatorBearingTemp` | number | C | Generator bearing temperature |
| `gearboxOilTemp` | number | C | Gearbox oil temperature |
| `gearboxBearingTemp` | number | C | Gearbox bearing temperature |

### 8.6 Wind.DailyEnergy

> **Frequency:** Realtime-Medium (5min) | **Source:** RTU

Daily operational counters, reset at midnight.

| Parameter | Type | Unit | Description |
|---|---|---|---|
| `dailyOperatingTime` | number | hours | Operating time today |
| `dailyYield` | number | kWh | Energy produced today |

### 8.7 Wind.Lifecycle

> **Frequency:** Realtime-Slow (hourly) | **Source:** RTU

Lifetime cumulative counters.

| Parameter | Type | Unit | Description |
|---|---|---|---|
| `totalYield` | number | kWh | Lifetime energy produced |
| `totalOperatingHours` | number | hours | Lifetime operating hours |

---

## 9. Grid Message Packages

### 9.1 Grid.Power

> **Frequency:** Realtime-Fast (1-5s) | **Source:** RTU

Grid meter real-time power and quality measurements.

| Parameter | Type | Unit | Description |
|---|---|---|---|
| `activePower` | number | kW | Total active power (positive = import) |
| `reactivePower` | number | kVAR | Total reactive power |
| `apparentPower` | number | kVA | Total apparent power |
| `powerFactor` | number | — | Power factor |
| `gridFrequency` | number | Hz | Grid frequency |
| `phaseVoltageA` | number | V | Phase A voltage |
| `phaseVoltageB` | number | V | Phase B voltage |
| `phaseVoltageC` | number | V | Phase C voltage |
| `phaseCurrentA` | number | A | Phase A current |
| `phaseCurrentB` | number | A | Phase B current |
| `phaseCurrentC` | number | A | Phase C current |
| `thdVoltage` | number | % | Total harmonic distortion (voltage) |
| `thdCurrent` | number | % | Total harmonic distortion (current) |

### 9.2 Grid.Energy

> **Frequency:** Realtime-Medium (5min) | **Source:** RTU

Cumulative energy counters.

| Parameter | Type | Unit | Description |
|---|---|---|---|
| `importToday` | number | kWh | Energy imported from grid today |
| `exportToday` | number | kWh | Energy exported to grid today |
| `importTotal` | number | kWh | Lifetime energy imported |
| `exportTotal` | number | kWh | Lifetime energy exported |

### 9.3 Grid.BreakerStatus

> **Frequency:** Realtime-Medium (10-30s) | **Source:** RTU

Circuit breaker and protection relay status.

| Parameter | Type | Unit | Description |
|---|---|---|---|
| `breakerPosition` | string | — | `Open`, `Closed`, `Trip`, `Invalid` |
| `relayStatus` | string | — | `Ready`, `Tripped`, `Blocked` |
| `tripCount` | integer | — | Total trip count |
| `lastTripTimestamp` | string | ISO datetime | Timestamp of last trip event |

### 9.4 Grid.BreakerLifecycle (Backend)

> **Frequency:** Calculated (on-demand) | **Source:** Backend

Computed breaker lifecycle metrics.

| Parameter | Type | Unit | Description |
|---|---|---|---|
| `totalOperations` | integer | — | Total open/close operations |
| `faultTrips` | integer | — | Number of fault-induced trips |
| `mechanicalLifeUsed` | number | % | Mechanical life consumed |
| `electricalLifeUsed` | number | % | Electrical life consumed |
| `estimatedRemainingOps` | integer | — | Estimated remaining operations |

---

## 10. Common Message Packages

### 10.1 Device.RuntimeInfo

> **Frequency:** Registration (once, on boot/firmware update) | **Source:** RTU

Runtime information reported by the RTU on device boot or firmware update. Can be sent by any device type. Uses `deviceName` in the envelope to identify the device.

Static identity fields (serial number, manufacturer, model, install date, location) are **not** part of telemetry — serial numbers are bound during the RTU handshake, and all other identity data is stored in the database at commissioning time via the engineering tool.

| Parameter | Type | Unit | Description |
|---|---|---|---|
| `firmwareVersion` | string | — | Current firmware version |
| `hardwareVersion` | string | — | Hardware revision |
| `bootTimestamp` | string | ISO datetime | Last boot/restart time |
| `configHash` | string | — | Hash of active RTU configuration (for change detection) |

### 10.2 Device.Maintenance (Backend)

> **Frequency:** Calculated (on-demand) | **Source:** Backend

Maintenance tracking computed by the backend.

| Parameter | Type | Unit | Description |
|---|---|---|---|
| `lastMaintenanceDate` | string | ISO date | Last maintenance performed |
| `nextMaintenanceDate` | string | ISO date | Next scheduled maintenance |
| `warrantyExpiry` | string | ISO date | Warranty expiration date |
| `deviceAge` | number | years | Device age since commissioning |
| `maintenanceScore` | number | % | Computed maintenance health score |

---

## 11. Alarm Message Packages

Alarm messages are **separate from telemetry**. They use a dedicated ingestion endpoint and are not routed through `DeviceDataCategorizer`. Instead, they are processed by `IngestDeviceAlarmHandler` which creates/recovers `DeviceAlarm` records directly.

### 11.1 Device.Alarm

> **Frequency:** On event (when fault state changes) | **Source:** RTU, Third-Party API Adapter

**Endpoint:** `POST /api/v1/DeviceData/Ingest/Alarm`

**Auth:** `X-API-Key` header

Unlike telemetry packages, alarm messages use a **different envelope format** — the `alarms` array replaces the `payload` object, and `deviceName` replaces `deviceSn`.

#### Envelope Format

```json
{
  "rtuSn": "RTU-BESS-001",
  "deviceName": "Battery1",
  "alarms": [
    {
      "code": "CellOverVoltage",
      "severity": "warning",
      "active": true,
      "message": "Cell voltage exceeded threshold on rack 2",
      "value": 3.72,
      "threshold": 3.65,
      "raisedAt": "2026-03-09T10:00:00Z"
    },
    {
      "code": "PcsOverTemp",
      "severity": "high",
      "active": false
    }
  ],
  "deviceLocalTime": "2026-03-09T10:05:00Z"
}
```

#### Envelope Fields

| Field | Type | Required | Description |
|---|---|---|---|
| `rtuSn` | string | Yes | RTU serial number that reported the alarm |
| `deviceName` | string | Yes | Name of the device the alarm belongs to (matched under the RTU) |
| `alarms` | array | Yes | List of alarm items (see below) |
| `deviceLocalTime` | string (ISO 8601) | No | Timestamp, defaults to UTC now |

#### Alarm Item Fields

| Field | Type | Required | Description |
|---|---|---|---|
| `code` | string | Yes | Alarm type code (e.g., `CellOverVoltage`, `INV-DCBUS-ABN`). Maps to `DeviceAlarmType.Code`. Auto-created if not found. |
| `severity` | string | No | `critical`, `high`, `medium`, or `low`. Used when auto-creating the alarm type. |
| `active` | boolean | Yes | `true` = fault active, `false` = fault cleared |
| `message` | string | No | Human-readable alarm description. Stored in `Details` JSON. |
| `value` | number | No | Current measured value that triggered the alarm (e.g., `3.72`). |
| `threshold` | number | No | Threshold that was exceeded (e.g., `3.65`). |
| `raisedAt` | string (ISO 8601) | No | When the alarm was originally raised. Defaults to `deviceLocalTime` or UTC now. |

#### Response

```json
{ "success": true, "created": 1, "recovered": 1 }
```

#### Processing Behavior

1. **Device resolution:** Backend resolves `(rtuSn, deviceName)` → `Device` record
2. **Alarm type resolution:** Each `code` is matched against `DeviceAlarmType.Code` for the device's model. If not found, a new `DeviceAlarmType` is auto-created with the provided severity and `DetectionStrategy = "Explicit"`
3. **Active alarms:** For each alarm with `active: true`, if no active `DeviceAlarm` exists for this `(DeviceId, DeviceAlarmTypeId)`, a new record is created with `message`, `value`, and `threshold` stored in the `Details` JSON column
4. **Cleared alarms:** For each alarm with `active: false`, the matching active `DeviceAlarm` is recovered (`IsActive = false`, `RecoveredAt = now`)
5. **Snapshot comparison:** Any active alarm in the DB for this device whose `code` is NOT present in the incoming `alarms` array is auto-recovered. This ensures stale alarms are cleared even if the RTU doesn't explicitly send `active: false`
6. **Traceability:** `RtuSerialNumber` and `DeviceName` are stored on each `DeviceAlarm` record

#### Alarm Lifecycle

```
  Ingest/Alarm (active=true)
         │
         ▼
      ┌──────┐
      │Active│ ── Acknowledge ──▶ ┌──────────────┐
      └──┬───┘    (user action)   │ Acknowledged │
         │                        └──────┬───────┘
         │                               │
         ▼                               ▼
  ┌────────────┐                  ┌────────────┐
  │ Recovered  │                  │  Resolved  │
  │(auto/alarm)│                  │(user/maint)│
  └────────────┘                  └────────────┘
```

| Status | Trigger | DB Fields Set |
|---|---|---|
| **Active** | Alarm ingested with `active: true` | `IsActive = true`, `OccurredAt` |
| **Acknowledged** | User calls `PATCH /api/v1/Alarms/{id}/Acknowledge` | `AcknowledgedAt`, `AcknowledgedById` |
| **Recovered** | Alarm ingested with `active: false`, or snapshot auto-recovery | `RecoveredAt`, `IsActive = false` |
| **Resolved** | User calls `PATCH /api/v1/Alarms/{id}/Resolve`, or linked maintenance task completed | `ResolvedAt`, `ResolvedById`, `IsActive = false` |

**Frontend status derivation:**
- `!IsActive` → `"resolved"`
- `IsActive && AcknowledgedAt != null` → `"acknowledged"`
- `IsActive && AcknowledgedAt == null` → `"active"`

#### Frontend API Response Mapping

The Alarm API (`GET /api/v1/Alarms`) maps stored alarm data to the frontend contract:

| API Field | DB Source | Notes |
|---|---|---|
| `code` | `DeviceAlarmType.Code` | Alarm type identifier |
| `message` | `Details.message` → `DeviceAlarmType.Description` | Instance message preferred, falls back to type description |
| `severity` | `DeviceAlarmType.Severity` | Mapped: Critical→critical, Major→high, Minor→medium, Warning→low |
| `status` | Derived from `IsActive` + `AcknowledgedAt` | See status derivation above |
| `source` | `Device.Name` → `DeviceAlarm.DeviceName` | Device name, falls back to denormalized column |
| `sourceType` | `Device.DeviceType.Name` | e.g., "inverter", "energystorage", "solar" |
| `triggeredAt` | `DeviceAlarm.OccurredAt` | When the alarm was first raised |
| `value` | `Details.value` | From ingestion payload, stored as string in API response |
| `threshold` | `Details.threshold` | From ingestion payload, stored as string in API response |
| `acknowledgedBy` | `ApplicationUser` (navigation) | `{ id, name, email, initials, role, department, phone }` |
| `resolvedBy` | `ApplicationUser` (navigation) | Same shape as acknowledgedBy |
| `relatedAsset` | `Device` + `Device.Metadata` | `{ id, name, type, location, capacity, status, lastMaintenance, serialNumber, model }` |
| `location` | `Device.Rtu.Facility.Name` | Facility name |

#### Legacy: ContainerBatteryFaultState (Deprecated)

The old `ContainerBatteryFaultState` kind embedded fault flags directly in telemetry payloads. This approach is **deprecated** — all new alarm data should use `POST /api/v1/DeviceData/Ingest/Alarm` instead.

The old `AlarmProcessingService` that extracted alarms from telemetry payloads has been removed from the ingestion pipeline.

---

## 12. Third-Party API Adapters

### 12.1 Adapter Architecture

Third-party API adapters are background services that:

1. Poll external cloud APIs on a scheduled interval
2. Transform the response into our standard message envelope format
3. Submit via the same `POST /api/v1/DeviceData/Ingest` endpoint

This ensures all data — whether from RTU or cloud API — flows through the same pipeline (`DeviceDataCategorizer` -> typed entity storage -> alarm processing).

```
┌─────────────────────┐
│  FusionSolar API     │
│  (Huawei iMaster)    │
└────────┬────────────┘
         │ HTTP GET (polled every 5min)
         v
┌─────────────────────┐
│  FusionSolar Adapter │  Translates FusionSolar keys → our standard keys
│  (Background Job)    │  Maps devTypeId → our DeviceType
└────────┬────────────┘
         │ POST /api/v1/DeviceData/Ingest/Batch
         v
┌─────────────────────┐
│  Ingest Pipeline     │  Same pipeline as RTU data
│  (IngestDeviceData)  │
└─────────────────────┘
```

### 12.2 FusionSolar (Huawei iMaster NetEco)

#### 12.2.1 API Overview

| Property | Value |
|---|---|
| Base URL | `https://intl.fusionsolar.huawei.com/thirdData` |
| Auth Method | XSRF-TOKEN (cookie-based session) |
| Session Lifetime | 30 minutes |
| Rate Limit | Varies by endpoint; ~1 req/5min recommended |

#### 12.2.2 Authentication (XSRF-TOKEN)

```
POST /thirdData/login
Body: { "userName": "...", "systemCode": "..." }
Response header: Set-Cookie: XSRF-TOKEN=...

All subsequent requests include:
  Cookie: XSRF-TOKEN=...
  XSRF-TOKEN: ... (as header)
```

Session refresh: re-login before 30-min expiry. The adapter should track session age and proactively refresh.

#### 12.2.3 Device Type Mapping

| FusionSolar `devTypeId` | FusionSolar Device | Our DeviceType | Our `DeviceDataCategory` |
|---|---|---|---|
| 1 | String Inverter | Solar | Solar |
| 17 | Grid Meter | Grid (SmartMeter) | SmartMeter |
| 39 | Battery | Battery (EnergyStorage) | Bess |
| 41 | ESS (Energy Storage System) | Battery (EnergyStorage) | Bess |
| 38 | PCS | PCS | Pcs |
| 47 | Power Sensor | Grid (SmartMeter) | SmartMeter |

#### 12.2.4 Parameter Mapping Tables

##### String Inverter (devTypeId = 1) -> Solar.Power

| FusionSolar Key | Our Key | Unit | Notes |
|---|---|---|---|
| `active_power` | `activePower` | kW | — |
| `reactive_power` | `reactivePower` | kVAR | — |
| `power_factor` | `powerFactor` | — | — |
| `efficiency` | `efficiency` | % | — |
| `temperature` | `cabinetTemp` | C | — |
| `day_cap` | `dailyYield` | kWh | — |
| `total_cap` | `totalYield` | kWh | — |
| `mppt_power` | `dcPower` | kW | Total MPPT power |
| `pv1_u` / `pv2_u` / ... | `mpptVoltages[]` | V | Map to Solar.Mppt |
| `pv1_i` / `pv2_i` / ... | `mpptCurrents[]` | A | Map to Solar.Mppt |
| `phase_A_voltage` / `ab_u` | `phaseVoltageA` | V | — |
| `phase_B_voltage` / `bc_u` | `phaseVoltageB` | V | — |
| `phase_C_voltage` / `ca_u` | `phaseVoltageC` | V | — |
| `phase_A_current` | `phaseCurrentA` | A | — |
| `phase_B_current` | `phaseCurrentB` | A | — |
| `phase_C_current` | `phaseCurrentC` | A | — |
| `grid_frequency` | `gridFrequency` | Hz | — |

##### Battery (devTypeId = 39) -> Battery.Energy + Battery.Status

| FusionSolar Key | Our Key | Our Package | Notes |
|---|---|---|---|
| `battery_soc` | `soc` | Battery.Energy | % |
| `battery_soh` | `soh` | Battery.Soh | % — separate package |
| `ch_discharge_power` | `power` | Battery.Energy | kW |
| `battery_status` | `operatingMode` | Battery.Status | Map: 0=Idle, 1=Charging, 2=Discharging |
| `bus_voltage` | `voltage` | Battery.Energy | V |
| `bus_current` | `current` | Battery.Energy | A |
| `max_temp` | `tempMax` | Battery.Energy | C |
| `min_temp` | `tempMin` | Battery.Energy | C |

##### ESS (devTypeId = 41) -> Battery.Energy

| FusionSolar Key | Our Key | Our Package | Notes |
|---|---|---|---|
| `soc` | `soc` | Battery.Energy | % |
| `charge_cap` | `totalChargeEnergy` | Battery.Health | kWh (lifetime) |
| `discharge_cap` | `totalDischargeEnergy` | Battery.Health | kWh (lifetime) |
| `active_power` | `power` | Battery.Energy | kW |

##### Grid Meter (devTypeId = 17) -> Grid.Power

| FusionSolar Key | Our Key | Our Package | Notes |
|---|---|---|---|
| `active_power` | `activePower` | Grid.Power | kW |
| `reactive_power` | `reactivePower` | Grid.Power | kVAR |
| `power_factor` | `powerFactor` | Grid.Power | — |
| `grid_frequency` | `gridFrequency` | Grid.Power | Hz |
| `phase_A_voltage` | `phaseVoltageA` | Grid.Power | V |
| `phase_B_voltage` | `phaseVoltageB` | Grid.Power | V |
| `phase_C_voltage` | `phaseVoltageC` | Grid.Power | V |
| `phase_A_current` | `phaseCurrentA` | Grid.Power | A |
| `phase_B_current` | `phaseCurrentB` | Grid.Power | A |
| `phase_C_current` | `phaseCurrentC` | Grid.Power | A |
| `active_cap` | `importTotal` | Grid.Energy | kWh |
| `reverse_active_cap` | `exportTotal` | Grid.Energy | kWh |

#### 12.2.5 Rate Limits and Polling Strategy

| Endpoint | Recommended Interval | Notes |
|---|---|---|
| `getDevRealKpi` | 5 minutes | Real-time KPIs per device |
| `getDevHistoryKpi` | 15 minutes | Historical data backfill |
| `getAlarmList` | 5 minutes | Active alarm polling |
| `getStationRealKpi` | 5 minutes | Plant-level summary |

**Polling strategy:**

1. Login and cache XSRF-TOKEN
2. Every 5 minutes: call `getDevRealKpi` for each device type
3. Transform each device's response into our envelope format
4. Batch-submit via `/api/v1/DeviceData/Ingest/Batch`
5. Every 15 minutes: call `getAlarmList` and feed into `AlarmProcessingService`
6. Re-login if session is older than 25 minutes

**Error handling:**

- HTTP 401/403: re-authenticate immediately
- HTTP 429: back off exponentially (5min -> 10min -> 20min)
- API returns `failCode != 0`: log and skip, do not retry immediately
- Network timeout: retry once after 30s, then skip cycle

### 12.3 Future Adapters (Placeholder)

The same adapter pattern applies to future integrations:

| Platform | Status | Expected Device Types |
|---|---|---|
| SMA Sunny Portal | Planned | Solar inverters, battery |
| SolarEdge Monitoring | Planned | Solar inverters, optimizers |
| Tesla Powerwall API | Planned | Battery (residential) |
| Growatt ShineServer | Planned | Solar inverters, battery |
| GoodWe SEMS | Planned | Solar inverters, battery, hybrid |

Each adapter will follow the same pattern: poll API -> translate to standard packages -> ingest via batch endpoint.

---

## 13. Appendix

### 13.1 Complete Parameter Key Reference (Alphabetical)

| Key | Type | Unit | Used In |
|---|---|---|---|
| `activePower` | number | kW | Solar.Power, Wind.Power, Grid.Power |
| `alarmActive` | boolean | — | Battery.Status |
| `ambientTemp` | number | C | Battery.Energy, Battery.Thermal, Wind.Condition |
| `apparentPower` | number | kVA | Solar.Power, Grid.Power |
| `availableEnergy` | number | kWh | Battery.Energy |
| `avgDailyCycles` | number | — | Battery.HealthCalc |
| `avgRte` | number | % | Battery.HealthCalc |
| `balancingActive` | boolean | — | Battery.RackEnergy |
| `breakerPosition` | string | — | Grid.BreakerStatus |
| `cabinetTemp` | number | C | Solar.Power |
| `calendarLifePct` | number | % | Battery.Health |
| `capacityFadeRate` | number | %/year | Battery.HealthCalc |
| `bootTimestamp` | string | ISO datetime | Device.RuntimeInfo |
| `cellChemistry` | string | — | Battery.Nameplate |
| `cellCount` | integer | — | Battery.ModuleEnergy |
| `cellTempMax` | number | C | Battery.Energy |
| `cellTempMin` | number | C | Battery.Energy |
| `cellVoltageAvg` | number | mV | Battery.Energy |
| `cellVoltageMax` | number | mV | Battery.Energy, Battery.RackEnergy, Battery.ModuleEnergy |
| `cellVoltageMin` | number | mV | Battery.Energy, Battery.RackEnergy, Battery.ModuleEnergy |
| `cells` | array | — | Battery.CellArray |
| `chillerCoolantIn` | number | C | Battery.Thermal |
| `chillerCoolantOut` | number | C | Battery.Thermal |
| `compressorRunning` | boolean | — | Battery.Thermal |
| `compressorSpeed` | number | RPM | Battery.Thermal |
| `contactorStatus` | string | — | Battery.RackEnergy |
| `containerTemp` | number | C | Battery.Thermal |
| `coolantFlowRate` | number | L/min | Battery.Thermal |
| `coolantPressure` | number | bar | Battery.Thermal |
| `current` | number | A | Battery.Energy, Battery.RackEnergy, Battery.ModuleEnergy |
| `cycleCount` | integer | — | Battery.Health |
| `cycleLifePct` | number | % | Battery.Health |
| `dailyOperatingTime` | number | hours | Wind.DailyEnergy |
| `dailyYield` | number | kWh | Solar.Power, Wind.DailyEnergy |
| `dcCurrent` | number | A | Solar.Power |
| `dcPower` | number | kW | Solar.Power |
| `dcVoltage` | number | V | Solar.Power |
| `deepDischargeCount` | integer | — | Battery.Health |
| `dehumidifierActive` | boolean | — | Battery.Thermal |
| `deviceAge` | number | years | Device.Maintenance |
| `efficiency` | number | % | Solar.Power |
| `electricalLifeUsed` | number | % | Grid.BreakerLifecycle |
| `equivalentFullCycles` | number | — | Battery.HealthCalc |
| `estimatedRemainingOps` | integer | — | Grid.BreakerLifecycle |
| `estimatedRul` | number | years | Battery.HealthCalc |
| `exportToday` | number | kWh | Grid.Energy |
| `exportTotal` | number | kWh | Grid.Energy |
| `fanSpeed` | number | RPM | Battery.Thermal |
| `faultCode` | integer | — | Battery.Status |
| `faultTrips` | integer | — | Grid.BreakerLifecycle |
| `configHash` | string | — | Device.RuntimeInfo |
| `firmwareVersion` | string | — | Battery.Nameplate, Device.RuntimeInfo |
| `gearboxBearingTemp` | number | C | Wind.DriveTrainTemp |
| `gearboxOilTemp` | number | C | Wind.DriveTrainTemp |
| `generatorBearingTemp` | number | C | Wind.DriveTrainTemp |
| `generatorPower` | number | kW | Wind.Power |
| `generatorRpm` | number | RPM | Wind.Condition |
| `generatorWindingTemp` | number | C | Wind.DriveTrainTemp |
| `gridFrequency` | number | Hz | Solar.Power, Wind.Electrical, Grid.Power |
| `gridVoltage` | number | V | Wind.Electrical |
| `hardwareVersion` | string | — | Device.RuntimeInfo |
| `heaterActive` | boolean | — | Battery.Thermal |
| `importToday` | number | kWh | Grid.Energy |
| `importTotal` | number | kWh | Grid.Energy |
| `isCharging` | boolean | — | Battery.Status |
| `isConnected` | boolean | — | Battery.Status |
| `lastFullChargeDate` | string | ISO date | Battery.Health |
| `lastMaintenanceDate` | string | ISO date | Device.Maintenance |
| `lastTripTimestamp` | string | ISO datetime | Grid.BreakerStatus |
| `maintenanceScore` | number | % | Device.Maintenance |
| `maxCellVoltageId` | string | — | Battery.Energy |
| `maxChargeCurrent` | number | A | Battery.Nameplate |
| `maxDischargeCurrent` | number | A | Battery.Nameplate |
| `mechanicalLifeUsed` | number | % | Grid.BreakerLifecycle |
| `minCellVoltageId` | string | — | Battery.Energy |
| `mode` | string | — | Battery.Thermal |
| `modulesPerRack` | integer | — | Battery.Nameplate |
| `mpptCurrents` | number[] | A | Solar.Mppt |
| `mpptPowers` | number[] | kW | Solar.Mppt |
| `mpptVoltages` | number[] | V | Solar.Mppt |
| `nacelleDirection` | number | degrees | Wind.Mechanical |
| `nextMaintenanceDate` | string | ISO date | Device.Maintenance |
| `nominalCapacity` | number | kWh | Battery.Nameplate |
| `nominalPower` | number | kW | Battery.Nameplate |
| `nominalVoltage` | number | V | Battery.Nameplate |
| `operatingHours` | number | hours | Battery.Health, Pcs.Energy |
| `operatingMode` | string | — | Battery.Status |
| `pcsActivePower` | number | kW | Pcs.PowerDistribution |
| `pcsActivePowerA` | number | kW | Pcs.PowerDistribution |
| `pcsActivePowerB` | number | kW | Pcs.PowerDistribution |
| `pcsActivePowerC` | number | kW | Pcs.PowerDistribution |
| `pcsAmbientTemp` | number | C | Pcs.Status |
| `pcsApparentPowerA` | number | kVA | Pcs.PowerDistribution |
| `pcsApparentPowerB` | number | kVA | Pcs.PowerDistribution |
| `pcsApparentPowerC` | number | kVA | Pcs.PowerDistribution |
| `pcsApparentPowerTotal` | number | kVA | Pcs.PowerDistribution |
| `pcsBatteryCurrent` | number | A | Pcs.DcMeasurement |
| `pcsBatteryVoltage` | number | V | Pcs.DcMeasurement |
| `pcsCcCvRechargeV` | number | V | Pcs.ChargeAlgorithm |
| `pcsCvChargeV` | number | V | Pcs.ChargeAlgorithm |
| `pcsCvFloatI` | number | A | Pcs.ChargeAlgorithm |
| `pcsCvMaxTime` | number | min | Pcs.ChargeAlgorithm |
| `pcsDcBusNegVoltage` | number | V | Pcs.DcMeasurement |
| `pcsDcBusPosVoltage` | number | V | Pcs.DcMeasurement |
| `pcsDcPower` | number | kW | Pcs.DcMeasurement |
| `pcsDeratingCoefficient` | number | % | Pcs.Status |
| `pcsEfficiency` | number | % | Pcs.Status |
| `pcsFloatCcI` | number | A | Pcs.ChargeAlgorithm |
| `pcsFloatChargeV` | number | V | Pcs.ChargeAlgorithm |
| `pcsGridFreq` | number | Hz | Pcs.AcMeasurement |
| `pcsGridMode` | string | — | Pcs.Status |
| `pcsIgbtTemp` | number | C | Pcs.Status |
| `pcsInductanceTemp` | number | C | Pcs.Status |
| `pcsInductorCurrentA` | number | A | Pcs.AcMeasurement |
| `pcsInductorCurrentB` | number | A | Pcs.AcMeasurement |
| `pcsInductorCurrentC` | number | A | Pcs.AcMeasurement |
| `pcsLineVoltageAB` | number | V | Pcs.AcMeasurement |
| `pcsLineVoltageBC` | number | V | Pcs.AcMeasurement |
| `pcsLineVoltageCA` | number | V | Pcs.AcMeasurement |
| `pcsPhaseCurrentA` | number | A | Pcs.AcMeasurement |
| `pcsPhaseCurrentB` | number | A | Pcs.AcMeasurement |
| `pcsPhaseCurrentC` | number | A | Pcs.AcMeasurement |
| `pcsPhaseSequence` | string | — | Pcs.Status |
| `pcsPhaseVoltageA` | number | V | Pcs.AcMeasurement |
| `pcsPhaseVoltageB` | number | V | Pcs.AcMeasurement |
| `pcsPhaseVoltageC` | number | V | Pcs.AcMeasurement |
| `pcsPowerFactor` | number | — | Pcs.AcMeasurement |
| `pcsReactivePower` | number | kVAR | Pcs.PowerDistribution |
| `pcsReactivePowerA` | number | kVAR | Pcs.PowerDistribution |
| `pcsReactivePowerB` | number | kVAR | Pcs.PowerDistribution |
| `pcsReactivePowerC` | number | kVAR | Pcs.PowerDistribution |
| `pcsSourceMode` | string | — | Pcs.Status |
| `pcsTotalDcBusVoltage` | number | V | Pcs.DcMeasurement |
| `pcsTotalDcCurrent` | number | A | Pcs.DcMeasurement |
| `pcsWorkingCondition` | string | — | Pcs.Status |
| `phaseCurrentA` | number | A | Solar.Power, Grid.Power |
| `phaseCurrentB` | number | A | Solar.Power, Grid.Power |
| `phaseCurrentC` | number | A | Solar.Power, Grid.Power |
| `phaseVoltageA` | number | V | Solar.Power, Grid.Power |
| `phaseVoltageB` | number | V | Solar.Power, Grid.Power |
| `phaseVoltageC` | number | V | Solar.Power, Grid.Power |
| `pitchAngleBlade1` | number | degrees | Wind.Mechanical |
| `pitchAngleBlade2` | number | degrees | Wind.Mechanical |
| `pitchAngleBlade3` | number | degrees | Wind.Mechanical |
| `power` | number | kW | Battery.Energy, Battery.RackEnergy |
| `powerDirection` | string | — | Battery.Energy |
| `powerFactor` | number | — | Solar.Power, Wind.Electrical, Grid.Power |
| `projectedEol` | string | ISO date | Battery.HealthCalc |
| `protectionFlags` | integer | — | Battery.Status |
| `rackCount` | integer | — | Battery.Nameplate |
| `ratedPower` | number | kW | Wind.Power |
| `reactivePower` | number | kVAR | Solar.Power, Wind.Power, Grid.Power |
| `relayStatus` | string | — | Grid.BreakerStatus |
| `remainingEnergy` | number | kWh | Battery.Energy |
| `rotorRpm` | number | RPM | Wind.Condition |
| `rte` | number | % | Battery.Health |
| `setpointTemp` | number | C | Battery.Thermal |
| `soc` | number | % | Battery.Energy, Battery.RackEnergy |
| `soh` | number | % | Battery.Soh |
| `strings` | array | — | Solar.Mppt |
| `tempAvg` | number | C | Battery.Energy, Battery.RackEnergy |
| `tempMax` | number | C | Battery.Energy, Battery.RackEnergy, Battery.ModuleEnergy |
| `tempMin` | number | C | Battery.Energy, Battery.RackEnergy, Battery.ModuleEnergy |
| `thdCurrent` | number | % | Grid.Power |
| `thdVoltage` | number | % | Grid.Power |
| `totalChargeEnergy` | number | kWh | Battery.Health |
| `totalDischargeEnergy` | number | kWh | Battery.Health |
| `totalOperatingHours` | number | hours | Wind.Lifecycle |
| `totalOperations` | integer | — | Grid.BreakerLifecycle |
| `totalYield` | number | kWh | Pcs.Energy, Solar.Power, Wind.Lifecycle |
| `tripCount` | integer | — | Grid.BreakerStatus |
| `voltage` | number | V | Battery.Energy, Battery.RackEnergy, Battery.ModuleEnergy |
| `voltageSpread` | number | mV | Battery.Energy |
| `warningFlags` | integer | — | Battery.Status |
| `warrantyExpiry` | string | ISO date | Device.Maintenance |
| `windDirection` | number | degrees | Wind.Condition |
| `windSpeed` | number | m/s | Wind.Condition |
| `yawError` | number | degrees | Wind.Mechanical |
---

### 13.2 FusionSolar Error Codes

| `failCode` | Meaning | Action |
|---|---|---|
| 0 | Success | Process normally |
| 305 | Not logged in / session expired | Re-authenticate |
| 306 | Invalid parameter | Log and skip |
| 307 | No data | Skip (device may be offline) |
| 401 | Access denied | Check credentials / permissions |
| 407 | Too many requests | Back off (double interval) |
| 20001 | Internal server error | Retry after 5 minutes |

### 13.3 Package Summary Table

| # | Package Name | Frequency | Source | Param Count | Category |
|---|---|---|---|---|---|
| 1 | Battery.Nameplate | Registration | RTU | 10 | Bess |
| 2 | Battery.Status | Realtime-Fast | RTU | 7 | Bess |
| 3 | Battery.Energy | Realtime-Fast | RTU | 19 | Bess |
| 4 | Battery.Soh | Realtime-Slow | RTU | 1 | Bess |
| 5 | Battery.CellArray | Realtime-Medium | RTU | 1 (array) | Bess |
| 6 | Battery.Health | Realtime-Slow | RTU | 9 | Bess |
| 7 | Battery.HealthCalc | Calculated | Backend | 6 | Bess |
| 8 | Battery.RackEnergy | Realtime-Fast | RTU | 11 | Bess |
| 9 | Battery.ModuleEnergy | Realtime-Fast | RTU | 7 | Bess |
| 10 | Battery.Thermal | Realtime-Medium | RTU | 13 | Bess |
| 11 | Pcs.Status | Realtime-Fast | RTU | 9 | Pcs |
| 12 | Pcs.AcMeasurement | Realtime-Fast | RTU | 14 | Pcs |
| 13 | Pcs.DcMeasurement | Realtime-Fast | RTU | 7 | Pcs |
| 14 | Pcs.PowerDistribution | Realtime-Fast | RTU | 12 | Pcs |
| 15 | Pcs.ChargeAlgorithm | Config | RTU | 6 | Pcs |
| 16 | Pcs.Energy | Realtime-Medium | RTU | 2 | Pcs |
| 17 | Solar.Power | Realtime-Fast | RTU | 18 | Solar |
| 18 | Solar.Mppt | Realtime-Medium | RTU | 4 | Solar |
| 19 | Wind.Power | Realtime-Fast | RTU | 4 | Wind |
| 20 | Wind.Condition | Realtime-Fast | RTU | 5 | Wind |
| 21 | Wind.Electrical | Realtime-Fast | RTU | 3 | Wind |
| 22 | Wind.Mechanical | Realtime-Medium | RTU | 5 | Wind |
| 23 | Wind.DriveTrainTemp | Realtime-Medium | RTU | 4 | Wind |
| 24 | Wind.DailyEnergy | Realtime-Medium | RTU | 2 | Wind |
| 25 | Wind.Lifecycle | Realtime-Slow | RTU | 2 | Wind |
| 26 | Grid.Power | Realtime-Fast | RTU | 13 | SmartMeter |
| 27 | Grid.Energy | Realtime-Medium | RTU | 4 | SmartMeter |
| 28 | Grid.BreakerStatus | Realtime-Medium | RTU | 4 | SmartMeter |
| 29 | Grid.BreakerLifecycle | Calculated | Backend | 5 | SmartMeter |
| 30 | Device.RuntimeInfo | Registration | RTU | 4 | (varies) |
| 31 | Device.Maintenance | Calculated | Backend | 5 | (varies) |
| 32 | Rtu.Handshake | Registration | RTU | 1 (array) | RTU |

**Total: 32 packages, ~210 unique parameters**