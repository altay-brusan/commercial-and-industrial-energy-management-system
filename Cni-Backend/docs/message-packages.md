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
                                                                            |-> Update LastSeenUtc

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
| `coolantPressure` | number | bar | Coolant pressure |

---

## 6. PCS Message Packages

> TODO: Define PCS packages (Pcs.Status, Pcs.AcMeasurement, Pcs.DcMeasurement, Pcs.Config)

---

## 7. Solar Message Packages

> TODO: Define Solar packages (Solar.Power, Solar.Mppt, Solar.String, Solar.Nameplate)

---

## 8. Wind Message Packages

> TODO: Define Wind packages (Wind.Power, Wind.Status, Wind.Nameplate)

---

## 9. Grid Message Packages

> TODO: Define Grid packages (Grid.Power, Grid.Quality, Grid.Meter)

---

## 10. Common Message Packages

> TODO: Define common packages (Device.RuntimeInfo, Rtu.Handshake, Device.Status)

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

> TODO: Define adapter specifications (Huawei FusionSolar, etc.)

---

## 13. Appendix

> TODO: Full parameter key reference, severity mapping tables, error codes