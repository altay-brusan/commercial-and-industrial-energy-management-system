# Dashboard Endpoints

> **Version:** 1.0
> **Last Updated:** 2026-03-10

---

## Overview

All dashboard endpoints are scoped to a single facility and require authentication with `Dashboard.View` permission.

**Base path:** `/api/v1/Dashboard/{facilityId}`

| Endpoint | Method | Route | Parameters |
|---|---|---|---|
| Stats | GET | `/{facilityId}/Stats` | — |
| Energy Usage | GET | `/{facilityId}/EnergyUsage` | — |
| Energy Source | GET | `/{facilityId}/EnergySource` | — |
| BESS State | GET | `/{facilityId}/BessState` | — |
| PCS State | GET | `/{facilityId}/PcsState` | — |
| Solar Capacity | GET | `/{facilityId}/SolarCapacity` | — |
| Energy Consumption | GET | `/{facilityId}/EnergyConsumption` | `?period=today\|weekly\|monthly` |
| CO2 Emissions | GET | `/{facilityId}/Co2Emissions` | `?period=today\|weekly\|monthly` |
| Source Distribution | GET | `/{facilityId}/SourceDistribution` | `?period=today\|weekly\|monthly` |
| Cost Savings | GET | `/{facilityId}/CostSavings` | `?period=today\|weekly\|monthly` |

---

## Message Package Mapping

Each dashboard endpoint reads telemetry from one or more message packages. The table below shows exactly which `Kind` values and payload fields each endpoint uses. All field names match the [Message Package Specification](./message-packages.md).

### Stats

| Source Table | Kind | Field | Usage |
|---|---|---|---|
| `DeviceSmartMeterData` | `Grid.Energy` | `importToday` | Total energy imported today (kWh) |
| `DeviceSmartMeterData` | `Grid.Power` | `activePower` | Current grid power (kW) |
| `DeviceBatteryData` | `Battery.Energy` | `soc` | Average BESS state of charge (%) |
| `FacilityTariff` → `Tariff` | — | — | Daily cost = today's energy × tariff rate |

### Energy Usage

| Source Table | Kind | Field | Usage |
|---|---|---|---|
| `DeviceSmartMeterData` | `Grid.Energy` | `importToday` | Grid energy consumption (kWh) |
| `DeviceSmartMeterData` | `Grid.Power` | `activePower` | Active power (kW) |
| `DeviceSmartMeterData` | `Grid.Power` | `reactivePower` | Reactive power (kVAR) |
| `DeviceSolarData` | `Solar.Power` | `dailyYield` | Solar energy production (kWh) |
| `DeviceBatteryData` | `Battery.Energy` | `remainingEnergy` | BESS energy contribution (kWh) |

### Energy Source

| Source Table | Kind | Field | Usage |
|---|---|---|---|
| `DeviceSmartMeterData` | `Grid.Power` | `activePower` | Grid power (kW) |
| `DeviceSolarData` | `Solar.Power` | `activePower` | Solar power (kW) |
| `DeviceBatteryData` | `Battery.Energy` | `power` | BESS power (kW) |
| `DevicePcsData` | `Pcs.PowerDistribution` | `pcsActivePower` | PCS power fallback if no battery data (kW) |

### BESS State

| Source Table | Kind | Field | Usage |
|---|---|---|---|
| `DeviceBatteryData` | `Battery.Energy` | `soc` | State of charge per unit (%) |
| `DeviceBatteryData` | `Battery.Soh` | `soh` | State of health per unit (%) |
| `DeviceBatteryData` | `Battery.Nameplate` | `nominalCapacity` | Rated capacity per unit (kWh) |

### PCS State

| Source Table | Kind | Field | Usage |
|---|---|---|---|
| `DevicePcsData` | `Pcs.Status` | `pcsIgbtTemp` | IGBT temperature (°C), fallback: `pcsAmbientTemp` |
| `DevicePcsData` | `Pcs.Status` | `pcsEfficiency` | Conversion efficiency (%) |
| `DevicePcsData` | `Pcs.Status` | `pcsWorkingCondition` | Operating mode (Running/Standby/Fault/Off) |
| `DevicePcsData` | `Pcs.PowerDistribution` | `pcsActivePower` | Current load (kW) |
| `DevicePcsData` | `Pcs.AcMeasurement` | `pcsLineVoltageAB` | AC line voltage A-B (V) |
| `DevicePcsData` | `Pcs.DcMeasurement` | `pcsTotalDcBusVoltage` | DC bus voltage (V) |

### Solar Capacity

| Source Table | Kind | Field | Usage |
|---|---|---|---|
| `DeviceSolarData` | `Solar.Power` | `activePower` | Current output power (kW) |
| `DeviceSolarData` | `Solar.Power` | `ratedPower` | Max rated capacity (kW) |
| `DeviceSolarData` | `Solar.Power` | `cabinetTemp` | Inverter cabinet temperature (°C) |
| `DeviceSolarData` | `Solar.Power` | `dailyYield` | Energy produced today (kWh) |

### Energy Consumption (chart)

| Source Table | Kind | Field | Usage |
|---|---|---|---|
| `DeviceSmartMeterData` | `Grid.Power` | `activePower` | Grid consumption per hour (kW) |
| `DeviceSolarData` | `Solar.Power` | `activePower` | Solar production per hour (kW) |

### CO2 Emissions (chart)

| Source Table | Kind | Field | Usage |
|---|---|---|---|
| `DeviceSmartMeterData` | `Grid.Power` | `activePower` | Grid power → emissions (kW × 0.4 kg CO2/kWh) |
| `DeviceSolarData` | `Solar.Power` | `activePower` | Solar power → saved emissions (kW × 0.4 kg CO2/kWh) |

### Source Distribution (chart)

| Source Table | Kind | Field | Usage |
|---|---|---|---|
| `DeviceSmartMeterData` | `Grid.Power` | `activePower` | Grid power per hour (kW) |
| `DeviceSolarData` | `Solar.Power` | `activePower` | Solar power per hour (kW) |
| `DeviceBatteryData` | `Battery.Energy` | `power` | BESS power per hour (kW) |

### Cost Savings (chart)

| Source Table | Kind | Field | Usage |
|---|---|---|---|
| `DeviceSmartMeterData` | `Grid.Power` | `activePower` | Grid power → cost (kW × tariff rate) |
| `DeviceSolarData` | `Solar.Power` | `activePower` | Solar power → savings (kW × tariff rate) |
| `FacilityTariff` → `Tariff` | — | — | Rate = (DistributionFee + TransmissionFee) / 1000 × (1 + TaxRate) |

---

## Domain Rationale: Why Each Field Maps to Each Metric

This section explains the physical/domain reasoning behind each mapping — how a raw telemetry field from a device becomes a dashboard metric.

### Energy Flow Model

A facility has three energy sources that the dashboard tracks:

```
                    ┌─────────────────────────┐
   Grid ──────────►│                         │
   (import/export)  │       Facility          │──► Loads (consumption)
                    │       Bus Bar           │
   Solar ─────────►│                         │
   (generation)     │                         │
                    └────────▲────────────────┘
                             │
                    BESS ────┘
                    (charge/discharge)
```

- **Grid meter** (`SmartMeter` device type) sits at the point of connection to the external grid. It measures how much power/energy flows **in from** or **out to** the grid.
- **Solar inverter** (`Solar` device type) converts DC panel output to AC. It measures how much power the panels are **generating**.
- **BESS** (`EnergyStorage` device type) stores and releases energy. The battery BMS measures stored energy and power flow; the PCS converts between DC (battery) and AC (grid/load).

### Field-to-Metric Reasoning

#### Grid.Power → `activePower`

The grid smart meter measures **instantaneous active power at the point of common coupling** (PCC). This is the single most important number for "how much power is the facility drawing from the grid right now."

- **Dashboard "consumption"**: Grid `activePower` is the real-time consumption from the grid. Positive = importing (consuming), negative = exporting (feeding back).
- **CO2 emissions**: Only grid-imported energy produces CO2 (fossil fuel mix). So `Grid.Power.activePower` (when positive) × emission factor = CO2 produced.
- **Grid cost**: You pay for grid imports. So `Grid.Power.activePower` (when positive) × tariff rate = cost incurred.

#### Grid.Energy → `importToday`

The grid meter also maintains a **cumulative energy counter** that resets daily. `importToday` is the total kWh imported from the grid since midnight — a direct hardware counter, more accurate than summing power samples.

- **Dashboard "total energy"**: Used for the Stats card showing today's total consumption.
- **Daily cost calculation**: `importToday` × tariff rate gives the actual cost so far today.

#### Solar.Power → `activePower`

The solar inverter reports its **AC output power**. This is how much the panels are actively generating right now.

- **Dashboard "production"**: Solar `activePower` is the real-time renewable generation.
- **CO2 saved**: Every kWh of solar displaces a kWh of grid import. So `Solar.Power.activePower` × emission factor = CO2 **avoided**.
- **Cost savings**: Solar generation offsets grid purchase. So `Solar.Power.activePower` × tariff rate = money saved.
- **Utilization**: `activePower / ratedPower × 100%` shows how much of the installed solar capacity is being used (affected by irradiance, temperature, shading).

#### Solar.Power → `dailyYield`

The inverter maintains a **daily energy counter** (kWh produced since midnight). More accurate than integrating power samples.

- **Energy Usage breakdown**: Shows total solar contribution as kWh alongside grid energy.
- **Solar Capacity "produced today"**: Direct readout of daily solar harvest.

#### Solar.Power → `cabinetTemp`

Inverter cabinet temperature — relevant for monitoring because high temperatures cause derating (the inverter reduces output to protect itself). Displayed in Solar Capacity widget.

#### Solar.Power → `ratedPower`

The inverter's maximum rated output capacity in kW. Used as the denominator for utilization percentage. Note: this may also come from `DeviceSpecificationTemplate` rather than telemetry if the inverter doesn't report it.

#### Battery.Energy → `soc` (State of Charge)

The BMS reports how full the battery is as a percentage (0-100%). This is the most important battery metric for operators.

- **Stats card**: Average SoC across all BESS units gives a facility-level battery status.
- **BESS State widget**: Per-unit SoC with charge level labels (Low/Medium/High/Full).

#### Battery.Energy → `power`

Instantaneous battery power in kW. Positive = charging, negative = discharging (convention follows `powerDirection` field).

- **Energy Source**: Shows BESS contribution to the facility power mix alongside grid and solar.
- **Source Distribution chart**: BESS power per hour bucket, showing when the battery is contributing vs absorbing.

#### Battery.Energy → `remainingEnergy`

Remaining usable energy in kWh — how much the battery can still deliver before hitting minimum SoC. Used in the Energy Usage breakdown to show BESS energy contribution.

#### Battery.Soh → `soh` (State of Health)

Battery degradation metric (100% = new, decreases over time). Reported infrequently (daily/weekly). Displayed per-unit in the BESS State widget to flag aging units.

#### Battery.Nameplate → `nominalCapacity`

Rated energy capacity in kWh — a static registration value reported once on boot. Displayed per-unit in BESS State to show the installed capacity alongside current SoC.

#### Pcs.PowerDistribution → `pcsActivePower`

The PCS (Power Conversion System) converts between DC battery and AC grid. `pcsActivePower` is the total AC-side active power — this is the "current load" on the PCS.

- **PCS State widget**: Shows each PCS unit's load.
- **Energy Source fallback**: If no battery telemetry exists (e.g., battery BMS offline but PCS still reporting), PCS active power is used as a proxy for BESS power.

#### Pcs.Status → `pcsWorkingCondition`

Operating state of the PCS: `Running`, `Standby`, `Fault`, `Off`. Displayed as the PCS "mode" in the dashboard widget.

#### Pcs.Status → `pcsIgbtTemp` / `pcsAmbientTemp`

IGBT (power semiconductor) temperature is the most critical thermal indicator for a PCS — high IGBT temp causes derating or shutdown. `pcsAmbientTemp` is the fallback if IGBT sensor is not available.

#### Pcs.Status → `pcsEfficiency`

DC-to-AC conversion efficiency percentage. A healthy PCS runs at 95-98%. Lower values indicate a problem. Displayed per-unit in PCS State.

#### Pcs.AcMeasurement → `pcsLineVoltageAB`

AC line voltage between phases A and B. Represents the grid-side voltage level the PCS is operating at. Used as a single representative voltage reading in the PCS State widget.

#### Pcs.DcMeasurement → `pcsTotalDcBusVoltage`

Total DC bus voltage — the battery-side voltage level. Important because it indicates the battery bank voltage and whether the PCS can operate (DC bus must be within its operating range).

### Why Charts Use Power (kW) Instead of Energy Counters

The time-series chart endpoints (`EnergyConsumption`, `Co2Emissions`, `SourceDistribution`, `CostSavings`) use **power readings** (`activePower`) rather than energy counters (`importToday`, `dailyYield`) because:

1. **Temporal granularity**: Power readings arrive every 1-5 seconds, giving fine-grained hourly buckets. Energy counters update every 5 minutes and are cumulative — you'd need to diff consecutive readings, which is fragile if data is missing.
2. **Multi-source alignment**: Grid, solar, and BESS all report power on the same fast cadence, making it easy to align them in the same time bucket.
3. **Approximation**: Average power (kW) over a 1-hour bucket ≈ energy (kWh) for that hour. This is standard practice in energy management systems.

The **snapshot endpoints** (Stats, EnergyUsage) use energy counters (`importToday`, `dailyYield`) where available because they need a cumulative total, not a time-series.

---

## Computed Values

| Metric | Formula | Default |
|---|---|---|
| CO2 emissions (kg) | grid energy (kWh) × 0.4 | EU grid average |
| CO2 saved (kg) | solar energy (kWh) × 0.4 | EU grid average |
| Grid cost | grid energy (kWh) × tariff rate | 0.25 EUR/kWh |
| Solar savings | solar energy (kWh) × tariff rate | 0.25 EUR/kWh |
| Tariff rate (EUR/kWh) | (DistributionFee + TransmissionFee) / 1000 × (1 + TaxRate) | — |
| Charge level | SoC < 20% → Low, < 50% → Medium, < 80% → High, ≥ 80% → Full | Unknown |
| Utilization (%) | current solar power / rated capacity × 100 | — |

---

## Device Type Resolution

Handlers resolve devices by `DeviceType.Name` under the facility's RTU(s):

| Dashboard Category | Device Type Names |
|---|---|
| Grid / SmartMeter | `SmartMeter` |
| Solar | `Solar` |
| BESS (Battery) | `EnergyStorage`, `EnergyStorageBattery` |
| PCS | `Pcs`, `Inverter` |

---

## Time-Series Chart Bucketing

Chart endpoints (`EnergyConsumption`, `Co2Emissions`, `SourceDistribution`, `CostSavings`) aggregate telemetry by **hour buckets**. Within each hour, the average power reading is used as a proxy for energy (kWh) assuming steady state over the 1-hour window.

**Period ranges:**

| Period | Range |
|---|---|
| `today` (default) | Midnight UTC today → now |
| `weekly` | 7 days ago → now |
| `monthly` | 30 days ago → now |
