# BESS & PCS Endpoint — Message Package Mapping

> **Version:** 1.0
> **Last Updated:** 2026-03-10

---

## Overview

This document maps every BESS and PCS endpoint tab to the message packages it consumes, the payload fields it extracts, and identifies gaps where unused message packages could enrich the data.

**Resolution chain:** Every numeric field follows this pattern:
```
ParameterResolver (from ParameterMapTemplate)  →  standard fallback extraction  →  null
```
If a device has a `ParameterMapTemplate`, the resolver uses the standard field names. The fallbacks also use the standard field names from the [Message Package Specification](./message-packages.md).

---

## Table of Contents

1. [BESS Endpoints](#1-bess-endpoints)
2. [PCS Endpoints](#2-pcs-endpoints)
3. [Hardcoded Fallback vs Standard Names](#3-hardcoded-fallback-vs-standard-names)
4. [Unused Message Packages — Improvement Opportunities](#4-unused-message-packages--improvement-opportunities)
5. [Resolution Architecture](#5-resolution-architecture)

---

## 1. BESS Endpoints

**Base path:** `/api/v1/BatteryStorage`

| Route | Handler | Description |
|---|---|---|
| `GET /Units/{facilityId}` | GetBessUnits | List all BESS units with live metrics |
| `GET /Units/{unitId}/Header` | GetBessUnitHeader | Unit info + connected sub-devices (ESC, HVAC, PCS) |
| `GET /Units/{unitId}/summary` | GetBessSummary | Detailed overview with sections |
| `GET /Units/{unitId}/health` | GetBessHealth | SoH trend, degradation, lifecycle |
| `GET /Units/{unitId}/curves` | GetBessCurves | Time-series charts (power, voltage, temp) |
| `GET /Units/{unitId}/faultflags` | GetBessFaultFlags | Fault/warning flag status |
| `GET /Units/{unitId}/faults` | GetBessFaults | Active/historical alarm records |
| `GET /Units/{unitId}/settings` | GetBessSettings | Device configuration & HVAC setpoints |
| `GET /Units/{unitId}/cells` | GetBessCells | Cell-level voltage/temp/SoC arrays |
| `GET /Tree/{facilityId}` | GetBessDeviceTree | Hierarchical device tree |

### 1.1 GetBessUnits — Unit List

| Message Package | Standard Field | ParameterKey | Hardcoded Fallback(s) | Dashboard Metric |
|---|---|---|---|---|
| `Battery.Energy` | `soc` | `ParameterKeys.Soc` | `"socPercent"`, `"stateOfCharge"`, `"soc"` | State of Charge (%) |
| `Battery.Soh` | `soh` | `ParameterKeys.Soh` | `"sohPercent"`, `"stateOfHealth"`, `"soh"` | State of Health (%) |
| `Battery.Energy` | `power` | `ParameterKeys.Power` | `"dcPower.power"`, `"power"` | Power (kW) |
| `Battery.Energy` | `voltage` × `current` | — | `"totalVoltageV"` × `"instantCurrentA"` / 1000 | Computed power fallback |
| `Battery.Energy` | `availableEnergy` | `ParameterKeys.AvailableEnergy` | `"availableEnergyKwh"`, `"capacityKwh"`, `"ratedEnergyKwh"` | Capacity (kWh) |
| `Battery.Energy` | `tempAvg` | `ParameterKeys.AmbientTemp` | `"ambientTempC"`, `"batteryTemperature"` | Temperature (°C) |
| `Battery.Status` | `operatingMode` | `ParameterKeys.OperatingMode` | `"state"` | Operating mode string |

**Multi-table:** Also loads PCS data via ContainerTelemetryLoader for merged payloads.

### 1.2 GetBessUnitHeader — Unit Header + Sub-devices

**Root unit (from Battery.Energy):**

| Standard Field | ParameterKey | Hardcoded Fallback(s) | Usage |
|---|---|---|---|
| `soc` | `ParameterKeys.Soc` | `"socPercent"` | SoC gauge |
| `soh` | `ParameterKeys.Soh` | `"sohPercent"` | SoH indicator |
| `voltage` | `ParameterKeys.Voltage` | `"totalVoltageV"` | Voltage display |
| `current` | `ParameterKeys.Current` | `"instantCurrentA"` | Current + computed power |
| `availableEnergy` | `ParameterKeys.AvailableEnergy` | `"availableEnergyKwh"` | Capacity |
| `operatingMode` | `ParameterKeys.OperatingMode` | `"state"` | Mode badge |

**ESC sub-device (from Pcs.Status — ESC shares PCS data table):**

| Hardcoded Field | Usage |
|---|---|
| `"operatingStateStr"`, `"state"` | Operating state badge |
| `"dcVoltageIn"`, `"dcVoltageInV"` | DC input voltage |
| `"dcVoltageOut"`, `"dcVoltageOutV"` | DC output voltage |
| `"dcCurrent"`, `"dcCurrentA"` | DC current |
| `"power"`, `"powerKw"` | Power output |
| `"efficiency"`, `"efficiencyPercent"` | Conversion efficiency |
| `"temperature"`, `"temperatureC"` | Component temperature |
| `"isolationResistance"`, `"isolationResistanceKohm"` | Isolation resistance |

**HVAC sub-device (from Battery.Thermal):**

| Standard Field | Hardcoded Fallback | Usage |
|---|---|---|
| `mode` | `"operatingModeStr"` | Cooling/Heating/Idle badge |
| `chillerCoolantOut` | `"outletCoolantTempC"` | Coolant outlet temp |
| `chillerCoolantIn` | `"inletCoolantTempC"` | Coolant inlet temp |
| `ambientTemp` | `"ambientTempC"` | Ambient temperature |
| `compressorRunning` | `"compressorStatus"` | Compressor badge |
| — | `"compressorSpeedPercent"` | Compressor speed |
| — | `"pumpStatus"` | Pump badge |
| — | `"pumpSpeedPercent"` | Pump speed |
| — | `"fanRunStatus"` | Fan badge |
| — | `"electricHeaterStatus"` | Heater badge |
| — | `"coolantTempSetpointC"` | Setpoint display |
| — | `"highAlarmTempC"` | High alarm threshold |
| — | `"lowAlarmTempC"` | Low alarm threshold |

### 1.3 GetBessSummary — Summary Tab

The richest endpoint — builds multiple sections from merged multi-table telemetry.

**Battery.Energy fields:**

| Standard Field | ParameterKey | Hardcoded Fallback(s) | Section |
|---|---|---|---|
| `soc` | `ParameterKeys.Soc` | `"socPercent"` | Overview |
| `soh` | `ParameterKeys.Soh` | `"sohPercent"` | Overview |
| `voltage` | `ParameterKeys.Voltage` | `"totalVoltageV"` | Electrical |
| `current` | `ParameterKeys.Current` | `"instantCurrentA"` | Electrical |
| `tempMin` | `ParameterKeys.TempMin` | `"minCellTempC"` | Temperature |
| `tempMax` | `ParameterKeys.TempMax` | `"maxCellTempC"` | Temperature |
| `remainingEnergy` | `ParameterKeys.RemainingEnergy` | `"remainingEnergyKwh"` | Electrical |
| `availableEnergy` | `ParameterKeys.AvailableEnergy` | `"availableEnergyKwh"` | Electrical |
| `cellVoltageMax` | `ParameterKeys.CellVoltageMax` | `"maxCellVoltageMv"` | Cell extremes |
| `cellVoltageMin` | `ParameterKeys.CellVoltageMin` | `"minCellVoltageMv"` | Cell extremes |
| `cellVoltageAvg` | `ParameterKeys.CellVoltageAvg` | `"avgCellVoltageMv"` | Cell extremes |
| `ambientTemp` | — | `"ambientTempC"` | Temperature |
| — | — | `"frequency"` | Electrical |
| — | — | `"dmccA"`, `"dmdcA"` | Max charge/discharge current |
| — | — | `"outVoltPosV"`, `"outVoltNegV"` | Positive/negative bus voltage |
| — | — | `"contactFbkPos"`, `"contactFbkNeg"` | Contactor feedback |
| — | — | `"isolationStatus"` | Isolation monitor |
| — | — | `"maxCellVoltageBmuLoc"`, `"maxCellVoltageCellLoc"` | Cell location info |
| — | — | `"minCellVoltageBmuLoc"`, `"minCellVoltageCellLoc"` | Cell location info |
| — | — | `"maxCellTempBmuLoc"`, `"maxCellTempCellLoc"` | Temp location info |

**Battery.Thermal fields (HVAC section):**

Same as Header HVAC section above, plus: `"inletCoolantPressure"`, `"outletCoolantPressure"`.

**Device.Metadata fields:**

| Field | Usage |
|---|---|
| `"chemistry"` | Cell chemistry type |
| `"commissioningDate"` | Commission date |
| `"capacityKwh"` | Rated capacity |
| `"maxPowerKw"` | Rated power |
| `"voltageMin"`, `"voltageMax"` | Voltage range |
| `"cellsPerModule"` | Module composition |

### 1.4 GetBessHealth — Health Tab

**Battery.Energy fields (latest + 12-month history):**

| Standard Field | ParameterKey | Hardcoded Fallback(s) | Section |
|---|---|---|---|
| `soh` | `ParameterKeys.Soh` | `"sohPercent"`, `"stateOfHealth"`, `"soh"` | Health Overview + SoH Trend |
| — | `ParameterKeys.CycleCount` | `"cycleCount"`, `"equivalentFullCycles"` | Degradation Metrics |
| — | `ParameterKeys.Rte` | `"roundTripEfficiency"`, `"rte"` | RTE |
| `tempMax` | `ParameterKeys.TempMax` | `"maxRecordedTempC"`, `"maxCellTempC"` | Thermal Stress |
| `tempAvg` | `ParameterKeys.TempAvg` | `"avgOperatingTempC"`, `"ambientTempC"`, `"batteryTemperature"` | Thermal Stress |
| — | `ParameterKeys.CalendarLifePct` | — | Calendar life |
| — | `ParameterKeys.EstimatedRul` | — | Remaining useful life |
| — | — | `"averageDod"`, `"avgDepthOfDischarge"` | Average DoD |
| — | — | `"initialRte"`, `"efficiencyDegradation"` | RTE degradation |
| — | — | `"lifetimeChargedMwh"`, `"totalCharged"` | Energy throughput |
| — | — | `"lifetimeDischargedMwh"`, `"totalDischarged"` | Energy throughput |
| — | — | `"dailyChargedKwh"`, `"dailyCharged"` | Daily energy |
| — | — | `"dailyDischargedKwh"`, `"dailyDischarged"` | Daily energy |
| — | — | `"totalOperatingHours"` | Operational hours |
| — | — | `"totalStandbyHours"` | Standby hours |
| — | — | `"thermalExcursions"` | Thermal stress count |
| — | — | `"firmwareVersion"` | Firmware |

**Device.Metadata fields:**

| Field | Usage |
|---|---|
| `"designedCycleLife"` | Cycle life denominator |
| `"designedCalendarLife"` | Calendar life denominator |
| `"commissioningDate"` | Age calculation |
| `"lastMaintenance"`, `"nextMaintenanceDue"` | Maintenance dates |
| `"warrantyExpiry"` | Warranty info |

**SoH Trend Chart:** Samples one data point per month for 12 months, extracting `soh` from historical Battery.Energy payloads.

### 1.5 GetBessCurves — Curves Tab

**No ParameterResolver — all hardcoded.**

| Source Table | Kind | Field | Chart | Series |
|---|---|---|---|---|
| `DeviceBatteryData` | `Battery.CellArray`* | `"socPercent"` | Power & SoC | SoC (%) |
| `DeviceBatteryData` | `Battery.CellArray`* | `"totalVoltageV"` × `"instantCurrentA"` / 1000 | Power & SoC | Power (kW) |
| `DeviceBatteryData` | `Battery.CellArray`* | `"totalVoltageV"` | Voltage & Current | Voltage (V) |
| `DeviceBatteryData` | `Battery.CellArray`* | `"instantCurrentA"` | Voltage & Current | Current (A) |
| `DeviceBatteryData` | `Battery.CellArray`* | `"maxCellTempC"` | Temperature | T.Max (°C) |
| `DeviceBatteryData` | `Battery.CellArray`* | `"ambientTempC"` | Temperature | T.Avg (°C) |
| `DeviceBatteryData` | `Battery.CellArray`* | `"minCellTempC"` | Temperature | T.Min (°C) |
| `DevicePcsData` | `Pcs.*` | `"acPower.totalActivePower"` | AC Power | Active Power (kW) |
| `DevicePcsData` | `Pcs.*` | `"efficiency"` | Efficiency | Efficiency (%) |

\* Kind is configurable via `SpecificationsTemplate["batteryDataKind"]`, default `"Battery.CellArray"`.

### 1.6 GetBessCells — Cells Tab

| Source Table | Kind | Array Field | Type | Usage |
|---|---|---|---|---|
| `DeviceBatteryData` | `Battery.Energy` | `"cellVoltages"` | double[] | Per-cell voltage |
| `DeviceBatteryData` | `Battery.Energy` | `"cellTemperatures"` | double[] | Per-cell temperature |
| `DeviceBatteryData` | `Battery.Energy` | `"cellCurrents"` | double[] | Per-cell current |
| `DeviceBatteryData` | `Battery.Energy` | `"cellSocs"` | double[] | Per-cell SoC |
| `DeviceBatteryData` | `Battery.Energy` | `"cellSohs"` | double[] | Per-cell SoH |
| `DeviceBatteryData` | `Battery.Energy` | `"cellResistances"` | double[] | Per-cell resistance |
| `DeviceBatteryData` | `Battery.Energy` | `"cellBalancing"` | bool[] | Balancing active |
| `DeviceBatteryData` | `Battery.Energy` | `"cellCycleCounts"` | int[] | Per-cell cycles |

> **Note:** Per the message spec, cell arrays should come from `Battery.CellArray` (cells[].voltage, cells[].temperature, cells[].balancing) — not flat arrays in `Battery.Energy`. The current extraction format may be a legacy convention.

### 1.7 GetBessSettings — Settings Tab

| Source | Field | Usage |
|---|---|---|
| `Device.Metadata` | `"protocol"` | Communication protocol |
| `Device.Metadata` | `"firmwareVersion"` | Firmware version |
| `Device.Metadata` | `"modbusAddress"` | Modbus address |
| `Device.Metadata` | `"tempAlarmThreshold"` | Temperature alarm threshold |
| `Device.Metadata` | `"socLowAlarm"`, `"socHighAlarm"` | SoC alarm limits |
| `Device.Metadata` | `"insulationResistanceThreshold"` | Insulation threshold |
| `Device.Metadata` | `"operatingMode"` | Mode setting |
| `Device.Metadata` | `"chargeStopSoc"`, `"dischargeStopSoc"` | SoC limits |
| `Device.Metadata` | `"balancingEnabled"`, `"balancingThresholdMv"` | Balancing config |
| `Device.Metadata` | `"socCalibrationIntervalHours"` | Calibration interval |
| `Device.Metadata` | `"maintenanceIntervalDays"` | Maintenance schedule |
| `Battery.Thermal` | `"operatingModeStr"` | HVAC mode |
| `Battery.Thermal` | `"coolantTempSetpointC"` | Coolant setpoint |
| `Battery.Thermal` | `"highAlarmTempC"`, `"lowAlarmTempC"` | Temp alarm limits |
| `Battery.Thermal` | `"compressorSpeedPercent"` | Compressor setting |
| `Battery.Thermal` | `"pumpSpeedPercent"` | Pump setting |
| `Battery.Thermal` | `"autoDefrost"` | Defrost toggle |

### 1.8 GetBessFaultFlags — Fault Flags Tab

Uses schema-driven extraction from `SpecificationsTemplate["faultFlags"]` or falls back to:

| Fallback Field | Usage |
|---|---|
| `"status.faultStatus"` | Fault active flag |
| `"status.warningStatus"` | Warning active flag |
| `"status.faultMessages"` | Fault description array |

### 1.9 GetBessFaults — Faults Tab

**No telemetry — queries `DeviceAlarm` table directly:**

| DB Field | Usage |
|---|---|
| `DeviceAlarmType.Code` | Fault type code |
| `DeviceAlarmType.Name` | Fault name |
| `DeviceAlarmType.Severity` | Severity level |
| `DeviceAlarmType.Source` | Source device type |
| `DeviceAlarm.OccurredAt` | Trigger timestamp |
| `DeviceAlarm.RecoveredAt` | Recovery timestamp |
| `DeviceAlarm.IsActive` | Active/recovered status |

---

## 2. PCS Endpoints

**Base path:** `/api/v1/Pcs`

| Route | Handler | Description |
|---|---|---|
| `GET /Units/{facilityId}` | GetPcsUnits | List all PCS units with live metrics |
| `GET /Units/{unitId}/Header` | GetPcsHeader | Unit info + available tabs |
| `GET /Units/{unitId}/general` | GetPcsGeneral | Full PCS detail (6 sections) |
| `GET /Units/{unitId}/fault` | GetPcsFaultFlags | Alarm type catalog + active alarms |
| `GET /Units/{unitId}/curve` | GetPcsCurves | Time-series charts |
| `GET /Schema` | GetPcsSchema | Template-driven schema metadata |

### 2.1 GetPcsUnits — Unit List

| Message Package | Standard Field | ParameterKey | Hardcoded Fallback | Metric |
|---|---|---|---|---|
| `Pcs.PowerDistribution` | `pcsActivePower` | `ParameterKeys.PcsActivePower` | `"pcs_outActPwrKw"` | Active power (kW) |
| `Pcs.PowerDistribution` | `pcsReactivePower` | `ParameterKeys.PcsReactivePower` | `"pcs_outReaPwrKvar"` | Reactive power (kVAR) |
| `Pcs.DcMeasurement` | `pcsDcPower` | `ParameterKeys.PcsDcPower` | `"pcs_dcPwrKw"` | DC power (kW) |
| `Pcs.AcMeasurement` | `pcsGridFreq` | `ParameterKeys.PcsGridFreq` | `"pcs_gridFreqHz"` | Grid frequency (Hz) |
| `Pcs.Status` | `pcsIgbtTemp` | `ParameterKeys.PcsIgbtTemp` | `"pcs_igbtTempC"` | IGBT temperature (°C) |
| `Pcs.Status` | `pcsGridMode` | `ParameterKeys.PcsGridMode` | `"pcs_gridMode"` | Grid mode |
| `Pcs.AcMeasurement` | `pcsPowerFactor` | `ParameterKeys.PcsPowerFactor` | `"pcs_acPwrFactor"` | Power factor |
| — | — | `ParameterKeys.PcsEfficiency` | — | Efficiency (computed from power) |

### 2.2 GetPcsGeneral — General Tab (6 Sections)

The most comprehensive PCS endpoint. Queries **5 message packages** simultaneously.

**Section 1: PCS Status (from `Pcs.Status`)**

| Standard Field | ParameterKey | Hardcoded Fallback | Usage |
|---|---|---|---|
| `pcsGridFreq` | `PcsGridFreq` | `"pcs_gridFreqHz"` | Grid frequency |
| `pcsPhaseSequence` | `PcsPhaseSequence` | `"pcs_phaseSeq"` | Phase sequence |
| `pcsDeratingCoefficient` | `PcsDeratingCoefficient` | `"pcs_deratingCoef"` | Derating factor |
| `pcsWorkingCondition` | `PcsWorkingCondition` | `"pcs_workCondition"` | Working condition |
| `pcsSourceMode` | `PcsSourceMode` | `"pcs_srcMode"` | Source mode |
| `pcsGridMode` | `PcsGridMode` | `"pcs_gridMode"` | Grid mode |

**Section 2: AC Measurements (from `Pcs.AcMeasurement`)**

| Standard Field | ParameterKey | Hardcoded Fallback | Usage |
|---|---|---|---|
| `pcsLineVoltageAB` | `PcsLineVoltageAB` | `"pcs_lineVoltAB"` | Line voltage A-B |
| `pcsLineVoltageBC` | `PcsLineVoltageBC` | `"pcs_lineVoltBC"` | Line voltage B-C |
| `pcsLineVoltageCA` | `PcsLineVoltageCA` | `"pcs_lineVoltCA"` | Line voltage C-A |
| `pcsPhaseVoltageA` | `PcsPhaseVoltageA` | `"pcs_phaseVoltA"` | Phase A voltage |
| `pcsPhaseVoltageB` | `PcsPhaseVoltageB` | `"pcs_phaseVoltB"` | Phase B voltage |
| `pcsPhaseVoltageC` | `PcsPhaseVoltageC` | `"pcs_phaseVoltC"` | Phase C voltage |
| `pcsPhaseCurrentA` | `PcsPhaseCurrentA` | `"pcs_phaseCurrA"` | Phase A current |
| `pcsPhaseCurrentB` | `PcsPhaseCurrentB` | `"pcs_phaseCurrB"` | Phase B current |
| `pcsPhaseCurrentC` | `PcsPhaseCurrentC` | `"pcs_phaseCurrC"` | Phase C current |
| `pcsInductorCurrentA` | `PcsInductorCurrentA` | `"pcs_inductCurrA"` | Inductor current A |
| `pcsInductorCurrentB` | `PcsInductorCurrentB` | `"pcs_inductCurrB"` | Inductor current B |
| `pcsInductorCurrentC` | `PcsInductorCurrentC` | `"pcs_inductCurrC"` | Inductor current C |

**Section 3: DC Measurements (from `Pcs.DcMeasurement`)**

| Standard Field | ParameterKey | Hardcoded Fallback | Usage |
|---|---|---|---|
| `pcsTotalDcBusVoltage` | `PcsTotalDcBusVoltage` | `"pcs_totalDcBusV"` | DC bus voltage |
| `pcsDcBusPosVoltage` | `PcsDcBusPosVoltage` | `"pcs_dcBusPosV"` | Positive bus |
| `pcsDcBusNegVoltage` | `PcsDcBusNegVoltage` | `"pcs_dcBusNegV"` | Negative bus |
| `pcsBatteryVoltage` | `PcsBatteryVoltage` | `"pcs_battVoltV"` | Battery voltage |
| `pcsBatteryCurrent` | `PcsBatteryCurrent` | `"pcs_battCurrA"` | Battery current |
| `pcsTotalDcCurrent` | `PcsTotalDcCurrent` | `"pcs_totalDcCurrA"` | Total DC current |
| `pcsDcPower` | `PcsDcPower` | `"pcs_dcPwrKw"` | DC power |
| `pcsPowerFactor` | `PcsPowerFactor` | `"pcs_acPwrFactor"` | Power factor |

**Section 4: Power Distribution (from `Pcs.PowerDistribution`)**

| Standard Field | ParameterKey | Hardcoded Fallback | Usage |
|---|---|---|---|
| `pcsActivePower` | `PcsActivePower` | `"pcs_outActPwrKw"` | Total active power |
| `pcsApparentPowerTotal` | `PcsApparentPowerTotal` | `"pcs_outAppPwrKva"` | Total apparent power |
| `pcsReactivePower` | `PcsReactivePower` | `"pcs_outReaPwrKvar"` | Total reactive power |
| `pcsActivePowerA/B/C` | `PcsActivePowerA/B/C` | `"pcs_actPwrKwA/B/C"` | Per-phase active |
| `pcsApparentPowerA/B/C` | `PcsApparentPowerA/B/C` | `"pcs_appPwrKvaA/B/C"` | Per-phase apparent |
| `pcsReactivePowerA/B/C` | `PcsReactivePowerA/B/C` | `"pcs_reaPwrKvarA/B/C"` | Per-phase reactive |

**Section 5: Temperatures (from `Pcs.Status`)**

| Standard Field | ParameterKey | Hardcoded Fallback | Usage |
|---|---|---|---|
| `pcsIgbtTemp` | `PcsIgbtTemp` | `"pcs_igbtTempC"` | IGBT temperature |
| `pcsAmbientTemp` | `PcsAmbientTemp` | `"pcs_ambTempC"` | Ambient temperature |
| `pcsInductanceTemp` | `PcsInductanceTemp` | `"pcs_inductTempC"` | Inductance temperature |

**Section 6: Charge Algorithm (from `Pcs.ChargeAlgorithm`)**

| Standard Field | ParameterKey | Hardcoded Fallback | Usage |
|---|---|---|---|
| `pcsCcCvRechargeV` | `PcsCcCvRechargeV` | `"pcs_ccCvRechargeV"` | Recharge voltage |
| `pcsCvChargeV` | `PcsCvChargeV` | `"pcs_cvChargeV"` | CV charge voltage |
| `pcsCvMaxTime` | `PcsCvMaxTime` | `"pcs_cvMaxTimeMin"` | Max CV time |
| `pcsCvFloatI` | `PcsCvFloatI` | `"pcs_cvFloatI"` | Float transition current |
| `pcsFloatChargeV` | `PcsFloatChargeV` | `"pcs_floatChargeV"` | Float charge voltage |
| `pcsFloatCcI` | `PcsFloatCcI` | `"pcs_floatCcI"` | Float CC current |

### 2.3 GetPcsCurves — Curves Tab

**No ParameterResolver — all hardcoded.**

| Chart | Hardcoded Field | Standard Field | Message Package |
|---|---|---|---|
| Power Profile | `"pcs_outActPwrKw"` | `pcsActivePower` | `Pcs.PowerDistribution` |
| Power Profile | `"pcs_outReaPwrKvar"` | `pcsReactivePower` | `Pcs.PowerDistribution` |
| DC Profile | `"pcs_totalDcBusV"` | `pcsTotalDcBusVoltage` | `Pcs.DcMeasurement` |
| DC Profile | `"pcs_totalDcCurrA"` | `pcsTotalDcCurrent` | `Pcs.DcMeasurement` |
| Phase Currents | `"pcs_phaseCurrA/B/C"` | `pcsPhaseCurrentA/B/C` | `Pcs.AcMeasurement` |
| Frequency | `"pcs_gridFreqHz"` | `pcsGridFreq` | `Pcs.AcMeasurement` |
| Temperature | `"pcs_igbtTempC"` | `pcsIgbtTemp` | `Pcs.Status` |
| Temperature | `"pcs_inductTempC"` | `pcsInductanceTemp` | `Pcs.Status` |
| Temperature | `"pcs_ambTempC"` | `pcsAmbientTemp` | `Pcs.Status` |
| Efficiency | `"pcs_efficiency"` | `pcsEfficiency` | `Pcs.Status` |

### 2.4 GetPcsFaultFlags — Fault Tab

**No telemetry — queries alarm catalog:**

| DB Table | Field | Usage |
|---|---|---|
| `DeviceAlarmType` | `Code`, `Name`, `Category`, `Severity` | Fault type definitions |
| `DeviceAlarm` | `DeviceAlarmTypeId`, `IsActive` | Active alarm status |

---

## 3. Hardcoded Fallback vs Standard Names

The handlers use two naming systems. The ParameterResolver uses standard names from the spec, but the hardcoded fallbacks use vendor-specific conventions:

### BESS Field Name Mismatches

| Standard (message-packages.md) | Hardcoded Fallback | Notes |
|---|---|---|
| `soc` | `"socPercent"`, `"stateOfCharge"` | Should just be `"soc"` |
| `soh` | `"sohPercent"`, `"stateOfHealth"` | Should just be `"soh"` |
| `voltage` | `"totalVoltageV"` | Should just be `"voltage"` |
| `current` | `"instantCurrentA"` | Should just be `"current"` |
| `power` | `"dcPower.power"` | Should just be `"power"` |
| `remainingEnergy` | `"remainingEnergyKwh"` | Should be `"remainingEnergy"` |
| `availableEnergy` | `"availableEnergyKwh"`, `"capacityKwh"`, `"ratedEnergyKwh"` | Should be `"availableEnergy"` |
| `tempMin` | `"minCellTempC"` | Should be `"tempMin"` |
| `tempMax` | `"maxCellTempC"` | Should be `"tempMax"` |
| `tempAvg` | `"ambientTempC"`, `"batteryTemperature"` | Should be `"tempAvg"` (different concept from `ambientTemp`) |
| `ambientTemp` | `"ambientTempC"` | Should be `"ambientTemp"` |
| `cellVoltageMax` | `"maxCellVoltageMv"` | Should be `"cellVoltageMax"` |
| `cellVoltageMin` | `"minCellVoltageMv"` | Should be `"cellVoltageMin"` |
| `cellVoltageAvg` | `"avgCellVoltageMv"` | Should be `"cellVoltageAvg"` |
| `operatingMode` | `"state"` | Should be `"operatingMode"` (from Battery.Status) |
| `cycleCount` | `"cycleCount"`, `"equivalentFullCycles"` | `cycleCount` correct; `equivalentFullCycles` is separate field |
| `rte` | `"roundTripEfficiency"` | Should be `"rte"` |
| `totalChargeEnergy` | `"lifetimeChargedMwh"`, `"totalCharged"` | Should be `"totalChargeEnergy"` |
| `totalDischargeEnergy` | `"lifetimeDischargedMwh"`, `"totalDischarged"` | Should be `"totalDischargeEnergy"` |

### PCS Field Name Mismatches

| Standard (message-packages.md) | Hardcoded Fallback | Notes |
|---|---|---|
| `pcsActivePower` | `"pcs_outActPwrKw"` | Should be `"pcsActivePower"` |
| `pcsReactivePower` | `"pcs_outReaPwrKvar"` | Should be `"pcsReactivePower"` |
| `pcsApparentPowerTotal` | `"pcs_outAppPwrKva"` | Should be `"pcsApparentPowerTotal"` |
| `pcsDcPower` | `"pcs_dcPwrKw"` | Should be `"pcsDcPower"` |
| `pcsGridFreq` | `"pcs_gridFreqHz"` | Should be `"pcsGridFreq"` |
| `pcsIgbtTemp` | `"pcs_igbtTempC"` | Should be `"pcsIgbtTemp"` |
| `pcsAmbientTemp` | `"pcs_ambTempC"` | Should be `"pcsAmbientTemp"` |
| `pcsInductanceTemp` | `"pcs_inductTempC"` | Should be `"pcsInductanceTemp"` |
| `pcsEfficiency` | `"pcs_efficiency"` | Should be `"pcsEfficiency"` |
| `pcsWorkingCondition` | `"pcs_workCondition"` | Should be `"pcsWorkingCondition"` |
| `pcsSourceMode` | `"pcs_srcMode"` | Should be `"pcsSourceMode"` |
| `pcsGridMode` | `"pcs_gridMode"` | Should be `"pcsGridMode"` |
| `pcsPhaseSequence` | `"pcs_phaseSeq"` | Should be `"pcsPhaseSequence"` |
| `pcsDeratingCoefficient` | `"pcs_deratingCoef"` | Should be `"pcsDeratingCoefficient"` |
| `pcsLineVoltageAB` | `"pcs_lineVoltAB"` | Should be `"pcsLineVoltageAB"` |
| `pcsTotalDcBusVoltage` | `"pcs_totalDcBusV"` | Should be `"pcsTotalDcBusVoltage"` |
| `pcsBatteryVoltage` | `"pcs_battVoltV"` | Should be `"pcsBatteryVoltage"` |
| `pcsBatteryCurrent` | `"pcs_battCurrA"` | Should be `"pcsBatteryCurrent"` |
| `pcsTotalDcCurrent` | `"pcs_totalDcCurrA"` | Should be `"pcsTotalDcCurrent"` |
| `pcsPowerFactor` | `"pcs_acPwrFactor"` | Should be `"pcsPowerFactor"` |
| All per-phase keys | `"pcs_actPwrKwA"`, etc. | Should be `"pcsActivePowerA"`, etc. |

---

## 4. Unused Message Packages — Improvement Opportunities

### Packages NOT Currently Queried

| Package | Fields Available | Could Enrich | Priority |
|---|---|---|---|
| **`Battery.Status`** | `operatingMode`, `isCharging`, `isConnected`, `alarmActive`, `faultCode`, `warningFlags`, `protectionFlags` | Units list (mode badge), Header (charging indicator), FaultFlags (flag extraction) | **High** — currently `operatingMode` is extracted from Battery.Energy fallback `"state"` field; dedicated package is more reliable |
| **`Battery.Health`** | `cycleCount`, `cycleLifePct`, `calendarLifePct`, `rte`, `totalChargeEnergy`, `totalDischargeEnergy`, `deepDischargeCount`, `lastFullChargeDate`, `operatingHours` | Health tab — all lifecycle metrics currently extracted from Battery.Energy as legacy fallback | **High** — this is the proper source for health data |
| **`Battery.RackEnergy`** | `soc`, `voltage`, `current`, `power`, `tempMin/Max/Avg`, `cellVoltageMax/Min`, `contactorStatus`, `balancingActive` | Rack-level summary (currently no per-rack telemetry in dashboard) | **Medium** — rack details are sub-device internal structure |
| **`Battery.ModuleEnergy`** | `voltage`, `current`, `tempMin/Max`, `cellVoltageMax/Min`, `cellCount` | Module-level summary | **Medium** — same as above |
| **`Battery.Thermal`** | `coolantFlowRate`, `coolantPressure`, `containerTemp`, `setpointTemp`, `dehumidifierActive` | HVAC section — 5 additional fields not currently extracted | **Medium** — enriches existing HVAC sections |
| **`Battery.CellArray`** | `cells[].id`, `cells[].voltage`, `cells[].temperature`, `cells[].balancing` | Cells tab — currently uses flat arrays from Battery.Energy instead of structured Battery.CellArray format | **Low** — works but format differs from spec |
| **`Battery.HealthCalc`** | `estimatedRul`, `projectedEol`, `avgDailyCycles`, `avgRte`, `capacityFadeRate`, `equivalentFullCycles` | Health tab — backend-computed metrics (requires computation engine) | **Low** — backend must compute these; not from RTU |
| **`Pcs.Energy`** | `totalYield`, `operatingHours` | PCS General (lifetime energy throughput, uptime) | **High** — easy to add, valuable KPI |
| **`Battery.Nameplate`** | `firmwareVersion`, `nominalCapacity`, `nominalVoltage`, `nominalPower`, `maxChargeCurrent`, `maxDischargeCurrent`, `cellChemistry`, `rackCount`, `modulesPerRack`, `cellsPerModule` | Summary/Settings — static device specs; currently pulled from Device.Metadata | **Medium** — useful if RTU reports it; currently comes from DB |

### Fields Available But Not Extracted From Already-Queried Packages

| Package (already queried) | Unused Field | Could Enrich |
|---|---|---|
| `Battery.Energy` | `powerDirection` | Units list — explicit charge/discharge indicator instead of inferring from current sign |
| `Battery.Energy` | `voltageSpread` | Summary — cell imbalance metric (Max - Min cell voltage) |
| `Battery.Energy` | `cellTempMax`, `cellTempMin` | Summary — cell-level temperature extremes (separate from pack/module temps) |
| `Battery.Energy` | `maxCellVoltageId`, `minCellVoltageId` | Summary — identify worst-performing cells |
| `Battery.Thermal` | `coolantFlowRate` | HVAC section — coolant flow monitoring |
| `Battery.Thermal` | `containerTemp` | HVAC section — internal container temp (separate from ambient) |
| `Battery.Thermal` | `setpointTemp` | HVAC section — current target temperature |
| `Battery.Thermal` | `dehumidifierActive` | HVAC section — dehumidifier status |
| `Pcs.Status` | `pcsEfficiency` | Already defined in ParameterKeys but only extracted in GetPcsGeneral, not in curves hardcoded list |

---

## 5. Resolution Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    Query Handler                         │
│                                                         │
│  1. Load devices for facility/unit                      │
│  2. ContainerTelemetryLoader.LoadLatest()                │
│     → queries 4 tables (Battery, PCS, Solar, SmartMeter)│
│     → groups by Kind → returns payloadsByKind            │
│  3. ParameterMapParser.ParseCached(template)             │
│     → returns parameterDefinitions                       │
│  4. new ParameterResolver(definitions, payloads, meta)   │
│                                                         │
│  For each metric:                                        │
│  ┌─────────────────────────────────────────────────────┐│
│  │ resolver?.ResolveNumber(ParameterKeys.X)            ││
│  │   ├─ Formula evaluation (references other params)   ││
│  │   ├─ Source extraction (Kind-specific payload)      ││
│  │   └─ Fallback source extraction                     ││
│  │ ?? ExtractNum(payload, "legacyField1", "legacy2")   ││
│  │   └─ Hardcoded vendor-specific field names          ││
│  └─────────────────────────────────────────────────────┘│
│                                                         │
│  5. Build response DTO                                   │
└─────────────────────────────────────────────────────────┘
```

**Key design points:**
- Devices **with** a ParameterMapTemplate get standard field names via the resolver
- Devices **without** a template fall back to hardcoded vendor-specific names
- The hardcoded names are the ones that need updating to match message-packages.md
- Curves handlers bypass the resolver entirely — always hardcoded
- FaultFlags and Faults use alarm tables, not telemetry payloads
