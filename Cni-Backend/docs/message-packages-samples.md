# Message Package Samples

> **Version:** 1.0
> **Last Updated:** 2026-03-13
> **Based on:** [message-packages.md](./message-packages.md) v1.0
> **Purpose:** Concrete JSON samples for every message package defined in the spec

---

## Table of Contents

1. [EMQX Webhook Envelope](#1-emqx-webhook-envelope)
2. [Battery (BESS) Samples](#2-battery-bess-samples)
3. [PCS Samples](#3-pcs-samples)
4. [Solar Samples](#4-solar-samples)
5. [Wind Samples](#5-wind-samples)
6. [Grid Samples](#6-grid-samples)
7. [Common Samples](#7-common-samples)
8. [Alarm Samples](#8-alarm-samples)
9. [RTU Handshake Sample](#9-rtu-handshake-sample)
10. [Batch Ingest Sample](#10-batch-ingest-sample)

---

## Reference Devices

| Device | Model | Key Specs |
|---|---|---|
| Battery Container | Ottomotive BESS | 800V nominal, LFP chemistry, 12 racks, 4 packs/rack, 8 modules/pack, 16 cells/module, ~3.4 MWh |
| PCS | Sungrow SC500HX | 500 kW, 3-phase, grid-tied |
| Solar Inverter | Huawei SG125HX | 125 kWp, 6 MPPT trackers, 12 strings (2 per MPPT) |
| Wind Turbine | Generic 2 MW | 3-blade, variable speed, geared drivetrain |
| Grid Meter | Generic 3-phase | Bidirectional smart meter |

---

## 1. EMQX Webhook Envelope

All samples below show the inner message payload. In production, EMQX wraps each message in a webhook POST body. The EMQX webhook envelope looks like this:

```json
{
  "topic": "devices/RTU-BESS-001/telemetry",
  "clientid": "RTU-BESS-001",
  "timestamp": 1772913600000,
  "payload": "<the JSON message as a string>"
}
```

The `payload` field is a JSON-encoded string containing the actual message envelope described in each sample below. The backend parses this string to extract the `rtuSerialNumber`, `deviceName`, `kind`, `path`, `timestamp`, and `payload` fields.

---

## 2. Battery (BESS) Samples

### 2.1 Battery.Nameplate

Registration message sent once on boot. Ottomotive BESS container specs.

```json
{
  "rtuSerialNumber": "RTU-BESS-001",
  "deviceName": "Battery1",
  "kind": "Battery.Nameplate",
  "path": null,
  "timestamp": "2026-03-13T06:00:00Z",
  "payload": {
    "firmwareVersion": "2.4.1",
    "nominalCapacity": 3440,
    "nominalVoltage": 800,
    "nominalPower": 1720,
    "maxChargeCurrent": 2150,
    "maxDischargeCurrent": 2150,
    "cellChemistry": "LFP",
    "rackCount": 12,
    "modulesPerRack": 32,
    "cellsPerModule": 16
  }
}
```

### 2.2 Battery.Status

#### Container-level (no path)

```json
{
  "rtuSerialNumber": "RTU-BESS-001",
  "deviceName": "Battery1",
  "kind": "Battery.Status",
  "path": null,
  "timestamp": "2026-03-13T12:30:05Z",
  "payload": {
    "operatingMode": "Discharging",
    "isCharging": false,
    "isConnected": true,
    "alarmActive": false,
    "faultCode": 0,
    "warningFlags": 0,
    "protectionFlags": 0
  }
}
```

### 2.3 Battery.Energy

#### Container-level (root device, no path)

```json
{
  "rtuSerialNumber": "RTU-BESS-001",
  "deviceName": "Battery1",
  "kind": "Battery.Energy",
  "path": null,
  "timestamp": "2026-03-13T12:30:05Z",
  "payload": {
    "soc": 72.4,
    "voltage": 787.2,
    "current": -185.6,
    "power": 146.1,
    "powerDirection": "Discharging",
    "remainingEnergy": 2490.6,
    "availableEnergy": 3268.0,
    "tempMin": 22.1,
    "tempMax": 28.7,
    "tempAvg": 25.3,
    "cellVoltageMax": 3312,
    "cellVoltageMin": 3289,
    "cellVoltageAvg": 3301,
    "voltageSpread": 23,
    "cellTempMax": 29.4,
    "cellTempMin": 21.8,
    "maxCellVoltageId": "R3-M12-C7",
    "minCellVoltageId": "R8-M5-C14",
    "ambientTemp": 24.6
  }
}
```

### 2.4 Battery.Soh

```json
{
  "rtuSerialNumber": "RTU-BESS-001",
  "deviceName": "Battery1",
  "kind": "Battery.Soh",
  "path": null,
  "timestamp": "2026-03-13T00:00:00Z",
  "payload": {
    "soh": 97.8
  }
}
```

### 2.5 Battery.CellArray

Cell-level diagnostics for Rack 2. Each rack has 32 modules x 16 cells = 512 cells. Sample shows a subset.

```json
{
  "rtuSerialNumber": "RTU-BESS-001",
  "deviceName": "Battery1",
  "kind": "Battery.CellArray",
  "path": "Rack/2",
  "timestamp": "2026-03-13T12:30:30Z",
  "payload": {
    "cells": [
      { "id": "R2-M1-C1", "voltage": 3298, "temperature": 24.2, "balancing": false },
      { "id": "R2-M1-C2", "voltage": 3301, "temperature": 24.3, "balancing": false },
      { "id": "R2-M1-C3", "voltage": 3295, "temperature": 24.1, "balancing": true },
      { "id": "R2-M1-C4", "voltage": 3310, "temperature": 24.5, "balancing": false },
      { "id": "R2-M1-C5", "voltage": 3303, "temperature": 24.4, "balancing": false },
      { "id": "R2-M1-C6", "voltage": 3297, "temperature": 24.0, "balancing": false },
      { "id": "R2-M1-C7", "voltage": 3306, "temperature": 24.6, "balancing": false },
      { "id": "R2-M1-C8", "voltage": 3299, "temperature": 24.3, "balancing": false },
      { "id": "R2-M1-C9", "voltage": 3302, "temperature": 24.2, "balancing": false },
      { "id": "R2-M1-C10", "voltage": 3308, "temperature": 24.7, "balancing": false },
      { "id": "R2-M1-C11", "voltage": 3294, "temperature": 24.1, "balancing": true },
      { "id": "R2-M1-C12", "voltage": 3300, "temperature": 24.4, "balancing": false },
      { "id": "R2-M1-C13", "voltage": 3305, "temperature": 24.5, "balancing": false },
      { "id": "R2-M1-C14", "voltage": 3296, "temperature": 24.0, "balancing": false },
      { "id": "R2-M1-C15", "voltage": 3307, "temperature": 24.6, "balancing": false },
      { "id": "R2-M1-C16", "voltage": 3301, "temperature": 24.3, "balancing": false }
    ]
  }
}
```

> **Note:** In production, this array contains all 512 cells for the rack. Truncated here for readability.

### 2.6 Battery.Health

```json
{
  "rtuSerialNumber": "RTU-BESS-001",
  "deviceName": "Battery1",
  "kind": "Battery.Health",
  "path": null,
  "timestamp": "2026-03-13T00:00:00Z",
  "payload": {
    "cycleCount": 847,
    "cycleLifePct": 14.1,
    "calendarLifePct": 8.3,
    "rte": 93.2,
    "totalChargeEnergy": 1458200,
    "totalDischargeEnergy": 1358940,
    "deepDischargeCount": 12,
    "lastFullChargeDate": "2026-03-12",
    "operatingHours": 14620
  }
}
```

### 2.7 Battery.HealthCalc (Backend-computed, not sent by RTU)

Included for reference. This is stored/returned by the backend, not ingested.

```json
{
  "rtuSerialNumber": "RTU-BESS-001",
  "deviceName": "Battery1",
  "kind": "Battery.HealthCalc",
  "path": null,
  "timestamp": "2026-03-13T01:00:00Z",
  "payload": {
    "estimatedRul": 18.4,
    "projectedEol": "2044-07-15",
    "avgDailyCycles": 1.4,
    "avgRte": 93.1,
    "capacityFadeRate": 0.12,
    "equivalentFullCycles": 792
  }
}
```

### 2.8 Battery.RackEnergy

#### With Rack path (Rack 2)

```json
{
  "rtuSerialNumber": "RTU-BESS-001",
  "deviceName": "Battery1",
  "kind": "Battery.RackEnergy",
  "path": "Rack/2",
  "timestamp": "2026-03-13T12:30:05Z",
  "payload": {
    "soc": 73.1,
    "voltage": 65.6,
    "current": -15.4,
    "power": 12.2,
    "tempMin": 23.0,
    "tempMax": 27.8,
    "tempAvg": 25.1,
    "cellVoltageMax": 3310,
    "cellVoltageMin": 3294,
    "contactorStatus": "Closed",
    "balancingActive": true
  }
}
```

#### With Rack+Pack path (Rack 2, Pack 1)

```json
{
  "rtuSerialNumber": "RTU-BESS-001",
  "deviceName": "Battery1",
  "kind": "Battery.RackEnergy",
  "path": "Rack/2/Pack/1",
  "timestamp": "2026-03-13T12:30:05Z",
  "payload": {
    "soc": 73.4,
    "voltage": 16.4,
    "current": -15.4,
    "power": 3.1,
    "tempMin": 23.2,
    "tempMax": 27.1,
    "tempAvg": 25.0,
    "cellVoltageMax": 3308,
    "cellVoltageMin": 3296,
    "contactorStatus": "Closed",
    "balancingActive": false
  }
}
```

### 2.9 Battery.ModuleEnergy

#### With Rack+Pack+Module path (Rack 2, Pack 1, Module 3)

```json
{
  "rtuSerialNumber": "RTU-BESS-001",
  "deviceName": "Battery1",
  "kind": "Battery.ModuleEnergy",
  "path": "Rack/2/Pack/1/Module/3",
  "timestamp": "2026-03-13T12:30:05Z",
  "payload": {
    "voltage": 52.8,
    "current": -15.4,
    "tempMin": 23.8,
    "tempMax": 26.2,
    "cellVoltageMax": 3306,
    "cellVoltageMin": 3298,
    "cellCount": 16
  }
}
```

#### Another module (Rack 7, Pack 2, Module 5)

```json
{
  "rtuSerialNumber": "RTU-BESS-001",
  "deviceName": "Battery1",
  "kind": "Battery.ModuleEnergy",
  "path": "Rack/7/Pack/2/Module/5",
  "timestamp": "2026-03-13T12:30:05Z",
  "payload": {
    "voltage": 52.9,
    "current": -15.2,
    "tempMin": 24.1,
    "tempMax": 26.8,
    "cellVoltageMax": 3309,
    "cellVoltageMin": 3295,
    "cellCount": 16
  }
}
```

### 2.10 Battery.Thermal

Container-level thermal management data.

```json
{
  "rtuSerialNumber": "RTU-BESS-001",
  "deviceName": "Battery1",
  "kind": "Battery.Thermal",
  "path": null,
  "timestamp": "2026-03-13T12:30:15Z",
  "payload": {
    "chillerCoolantOut": 18.4,
    "chillerCoolantIn": 24.7,
    "compressorRunning": true,
    "compressorSpeed": 2840,
    "fanSpeed": 1450,
    "heaterActive": false,
    "coolantFlowRate": 42.5,
    "coolantPressure": 2.8,
    "ambientTemp": 31.2,
    "containerTemp": 25.8,
    "setpointTemp": 25.0,
    "mode": "Cooling",
    "dehumidifierActive": false
  }
}
```

---

## 3. PCS Samples

All PCS samples use a standalone PCS device (Sungrow SC500HX, 500 kW).

### 3.1 Pcs.Status

```json
{
  "rtuSerialNumber": "RTU-BESS-001",
  "deviceName": "PCS1",
  "kind": "Pcs.Status",
  "path": null,
  "timestamp": "2026-03-13T12:30:05Z",
  "payload": {
    "pcsWorkingCondition": "Running",
    "pcsSourceMode": "GridTied",
    "pcsGridMode": "OnGrid",
    "pcsPhaseSequence": "Positive",
    "pcsDeratingCoefficient": 100.0,
    "pcsIgbtTemp": 52.3,
    "pcsAmbientTemp": 34.1,
    "pcsInductanceTemp": 48.7,
    "pcsEfficiency": 97.8
  }
}
```

### 3.2 Pcs.AcMeasurement

```json
{
  "rtuSerialNumber": "RTU-BESS-001",
  "deviceName": "PCS1",
  "kind": "Pcs.AcMeasurement",
  "path": null,
  "timestamp": "2026-03-13T12:30:05Z",
  "payload": {
    "pcsGridFreq": 50.02,
    "pcsLineVoltageAB": 398.4,
    "pcsLineVoltageBC": 399.1,
    "pcsLineVoltageCA": 397.8,
    "pcsPhaseVoltageA": 230.1,
    "pcsPhaseVoltageB": 230.4,
    "pcsPhaseVoltageC": 229.8,
    "pcsPhaseCurrentA": 724.6,
    "pcsPhaseCurrentB": 725.1,
    "pcsPhaseCurrentC": 723.9,
    "pcsInductorCurrentA": 726.2,
    "pcsInductorCurrentB": 726.8,
    "pcsInductorCurrentC": 725.5,
    "pcsPowerFactor": -0.998
  }
}
```

### 3.3 Pcs.DcMeasurement

```json
{
  "rtuSerialNumber": "RTU-BESS-001",
  "deviceName": "PCS1",
  "kind": "Pcs.DcMeasurement",
  "path": null,
  "timestamp": "2026-03-13T12:30:05Z",
  "payload": {
    "pcsTotalDcBusVoltage": 794.6,
    "pcsDcBusPosVoltage": 397.3,
    "pcsDcBusNegVoltage": -397.3,
    "pcsBatteryVoltage": 787.2,
    "pcsBatteryCurrent": -185.6,
    "pcsTotalDcCurrent": -188.2,
    "pcsDcPower": 149.4
  }
}
```

### 3.4 Pcs.PowerDistribution

```json
{
  "rtuSerialNumber": "RTU-BESS-001",
  "deviceName": "PCS1",
  "kind": "Pcs.PowerDistribution",
  "path": null,
  "timestamp": "2026-03-13T12:30:05Z",
  "payload": {
    "pcsActivePower": 146.1,
    "pcsActivePowerA": 48.8,
    "pcsActivePowerB": 48.7,
    "pcsActivePowerC": 48.6,
    "pcsReactivePower": 3.2,
    "pcsReactivePowerA": 1.1,
    "pcsReactivePowerB": 1.1,
    "pcsReactivePowerC": 1.0,
    "pcsApparentPowerTotal": 146.2,
    "pcsApparentPowerA": 48.8,
    "pcsApparentPowerB": 48.7,
    "pcsApparentPowerC": 48.7
  }
}
```

### 3.5 Pcs.ChargeAlgorithm

```json
{
  "rtuSerialNumber": "RTU-BESS-001",
  "deviceName": "PCS1",
  "kind": "Pcs.ChargeAlgorithm",
  "path": null,
  "timestamp": "2026-03-13T08:00:00Z",
  "payload": {
    "pcsCcCvRechargeV": 780.0,
    "pcsCvChargeV": 832.0,
    "pcsCvMaxTime": 120,
    "pcsCvFloatI": 20.0,
    "pcsFloatChargeV": 820.0,
    "pcsFloatCcI": 50.0
  }
}
```

### 3.6 Pcs.Energy

```json
{
  "rtuSerialNumber": "RTU-BESS-001",
  "deviceName": "PCS1",
  "kind": "Pcs.Energy",
  "path": null,
  "timestamp": "2026-03-13T12:35:00Z",
  "payload": {
    "totalYield": 1245800,
    "operatingHours": 14580
  }
}
```

---

## 4. Solar Samples

### 4.1 Solar.Power (Inverter-level)

Huawei SG125HX inverter at peak production.

```json
{
  "rtuSerialNumber": "RTU-SOLAR-001",
  "deviceName": "Inverter1",
  "kind": "Solar.Power",
  "path": null,
  "timestamp": "2026-03-13T12:15:00Z",
  "payload": {
    "activePower": 118.4,
    "reactivePower": 2.1,
    "apparentPower": 118.5,
    "powerFactor": 0.999,
    "gridFrequency": 50.01,
    "phaseVoltageA": 230.2,
    "phaseVoltageB": 230.5,
    "phaseVoltageC": 229.9,
    "phaseCurrentA": 171.5,
    "phaseCurrentB": 171.7,
    "phaseCurrentC": 171.3,
    "dcPower": 120.8,
    "dcVoltage": 620.0,
    "dcCurrent": 194.8,
    "efficiency": 98.0,
    "cabinetTemp": 42.7,
    "dailyYield": 624.3,
    "totalYield": 458920
  }
}
```

### 4.2 Solar.Mppt (MPPT tracker data with strings)

Huawei SG125HX with 6 MPPT trackers, 2 strings per MPPT (12 strings total).

```json
{
  "rtuSerialNumber": "RTU-SOLAR-001",
  "deviceName": "Inverter1",
  "kind": "Solar.Mppt",
  "path": null,
  "timestamp": "2026-03-13T12:15:10Z",
  "payload": {
    "mpptVoltages": [580.2, 585.7, 578.4, 582.1, 584.9, 579.6],
    "mpptCurrents": [34.2, 33.8, 34.5, 33.9, 34.1, 34.3],
    "mpptPowers": [19.8, 19.8, 20.0, 19.7, 19.9, 19.9],
    "strings": [
      { "mpptIndex": 1, "stringIndex": 1, "current": 17.2 },
      { "mpptIndex": 1, "stringIndex": 2, "current": 17.0 },
      { "mpptIndex": 2, "stringIndex": 1, "current": 16.9 },
      { "mpptIndex": 2, "stringIndex": 2, "current": 16.9 },
      { "mpptIndex": 3, "stringIndex": 1, "current": 17.3 },
      { "mpptIndex": 3, "stringIndex": 2, "current": 17.2 },
      { "mpptIndex": 4, "stringIndex": 1, "current": 17.0 },
      { "mpptIndex": 4, "stringIndex": 2, "current": 16.9 },
      { "mpptIndex": 5, "stringIndex": 1, "current": 17.1 },
      { "mpptIndex": 5, "stringIndex": 2, "current": 17.0 },
      { "mpptIndex": 6, "stringIndex": 1, "current": 17.2 },
      { "mpptIndex": 6, "stringIndex": 2, "current": 17.1 }
    ]
  }
}
```

---

## 5. Wind Samples

### 5.1 Wind.Power

```json
{
  "rtuSerialNumber": "RTU-WIND-001",
  "deviceName": "Turbine1",
  "kind": "Wind.Power",
  "path": null,
  "timestamp": "2026-03-13T14:00:05Z",
  "payload": {
    "activePower": 1680.0,
    "ratedPower": 2000.0,
    "generatorPower": 1720.0,
    "reactivePower": 85.0
  }
}
```

### 5.2 Wind.Condition

```json
{
  "rtuSerialNumber": "RTU-WIND-001",
  "deviceName": "Turbine1",
  "kind": "Wind.Condition",
  "path": null,
  "timestamp": "2026-03-13T14:00:05Z",
  "payload": {
    "windSpeed": 11.4,
    "windDirection": 225,
    "ambientTemp": 14.2,
    "rotorRpm": 14.8,
    "generatorRpm": 1480
  }
}
```

### 5.3 Wind.Electrical

```json
{
  "rtuSerialNumber": "RTU-WIND-001",
  "deviceName": "Turbine1",
  "kind": "Wind.Electrical",
  "path": null,
  "timestamp": "2026-03-13T14:00:05Z",
  "payload": {
    "gridFrequency": 50.01,
    "gridVoltage": 690,
    "powerFactor": 0.995
  }
}
```

### 5.4 Wind.Mechanical

```json
{
  "rtuSerialNumber": "RTU-WIND-001",
  "deviceName": "Turbine1",
  "kind": "Wind.Mechanical",
  "path": null,
  "timestamp": "2026-03-13T14:00:15Z",
  "payload": {
    "pitchAngleBlade1": 3.2,
    "pitchAngleBlade2": 3.1,
    "pitchAngleBlade3": 3.3,
    "nacelleDirection": 228,
    "yawError": -3.0
  }
}
```

### 5.5 Wind.DriveTrainTemp

```json
{
  "rtuSerialNumber": "RTU-WIND-001",
  "deviceName": "Turbine1",
  "kind": "Wind.DriveTrainTemp",
  "path": null,
  "timestamp": "2026-03-13T14:00:30Z",
  "payload": {
    "generatorWindingTemp": 92.4,
    "generatorBearingTemp": 68.1,
    "gearboxOilTemp": 58.3,
    "gearboxBearingTemp": 62.7
  }
}
```

### 5.6 Wind.DailyEnergy

```json
{
  "rtuSerialNumber": "RTU-WIND-001",
  "deviceName": "Turbine1",
  "kind": "Wind.DailyEnergy",
  "path": null,
  "timestamp": "2026-03-13T14:05:00Z",
  "payload": {
    "dailyOperatingTime": 13.2,
    "dailyYield": 18450
  }
}
```

### 5.7 Wind.Lifecycle

```json
{
  "rtuSerialNumber": "RTU-WIND-001",
  "deviceName": "Turbine1",
  "kind": "Wind.Lifecycle",
  "path": null,
  "timestamp": "2026-03-13T13:00:00Z",
  "payload": {
    "totalYield": 12480000,
    "totalOperatingHours": 42150
  }
}
```

---

## 6. Grid Samples

### 6.1 Grid.Power

```json
{
  "rtuSerialNumber": "RTU-BESS-001",
  "deviceName": "GridMeter1",
  "kind": "Grid.Power",
  "path": null,
  "timestamp": "2026-03-13T12:30:05Z",
  "payload": {
    "activePower": -142.8,
    "reactivePower": 5.4,
    "apparentPower": 143.0,
    "powerFactor": -0.999,
    "gridFrequency": 50.02,
    "phaseVoltageA": 230.3,
    "phaseVoltageB": 230.6,
    "phaseVoltageC": 230.0,
    "phaseCurrentA": 207.0,
    "phaseCurrentB": 207.2,
    "phaseCurrentC": 206.8,
    "thdVoltage": 1.8,
    "thdCurrent": 3.2
  }
}
```

> **Note:** Negative `activePower` indicates export to grid (the BESS is discharging and feeding power back).

### 6.2 Grid.Energy

```json
{
  "rtuSerialNumber": "RTU-BESS-001",
  "deviceName": "GridMeter1",
  "kind": "Grid.Energy",
  "path": null,
  "timestamp": "2026-03-13T12:35:00Z",
  "payload": {
    "importToday": 1240.5,
    "exportToday": 2180.3,
    "importTotal": 845200,
    "exportTotal": 1125800
  }
}
```

### 6.3 Grid.BreakerStatus

```json
{
  "rtuSerialNumber": "RTU-BESS-001",
  "deviceName": "GridMeter1",
  "kind": "Grid.BreakerStatus",
  "path": null,
  "timestamp": "2026-03-13T12:30:15Z",
  "payload": {
    "breakerPosition": "Closed",
    "relayStatus": "Ready",
    "tripCount": 3,
    "lastTripTimestamp": "2026-02-14T03:22:18Z"
  }
}
```

### 6.4 Grid.BreakerLifecycle (Backend-computed, not sent by RTU)

```json
{
  "rtuSerialNumber": "RTU-BESS-001",
  "deviceName": "GridMeter1",
  "kind": "Grid.BreakerLifecycle",
  "path": null,
  "timestamp": "2026-03-13T01:00:00Z",
  "payload": {
    "totalOperations": 4280,
    "faultTrips": 3,
    "mechanicalLifeUsed": 4.3,
    "electricalLifeUsed": 2.1,
    "estimatedRemainingOps": 95720
  }
}
```

---

## 7. Common Samples

### 7.1 Device.RuntimeInfo

Sent by any device on boot or firmware update.

```json
{
  "rtuSerialNumber": "RTU-BESS-001",
  "deviceName": "PCS1",
  "kind": "Device.RuntimeInfo",
  "path": null,
  "timestamp": "2026-03-13T06:00:02Z",
  "payload": {
    "firmwareVersion": "3.0.1",
    "hardwareVersion": "Rev-C",
    "bootTimestamp": "2026-03-13T05:59:48Z",
    "configHash": "a1b2c3d4e5f6"
  }
}
```

### 7.2 Device.Maintenance (Backend-computed, not sent by RTU)

```json
{
  "rtuSerialNumber": "RTU-BESS-001",
  "deviceName": "Battery1",
  "kind": "Device.Maintenance",
  "path": null,
  "timestamp": "2026-03-13T01:00:00Z",
  "payload": {
    "lastMaintenanceDate": "2026-01-15",
    "nextMaintenanceDate": "2026-07-15",
    "warrantyExpiry": "2031-06-01",
    "deviceAge": 1.7,
    "maintenanceScore": 94.5
  }
}
```

---

## 8. Alarm Samples

### 8.1 Device.Alarm

Alarm messages use a different endpoint (`POST /api/v1/DeviceData/Ingest/Alarm`) and a different envelope format than telemetry.

#### Active alarm with recovery of a previous alarm

```json
{
  "rtuSn": "RTU-BESS-001",
  "deviceName": "Battery1",
  "alarms": [
    {
      "code": "CellOverVoltage",
      "severity": "warning",
      "active": true,
      "message": "Cell voltage exceeded 3.65V threshold on Rack 2, Module 12, Cell 7",
      "value": 3.72,
      "threshold": 3.65,
      "raisedAt": "2026-03-13T12:28:45Z"
    },
    {
      "code": "CellUnderTemp",
      "severity": "medium",
      "active": false
    }
  ],
  "deviceLocalTime": "2026-03-13T12:30:00Z"
}
```

#### Single critical alarm (PCS fault)

```json
{
  "rtuSn": "RTU-BESS-001",
  "deviceName": "PCS1",
  "alarms": [
    {
      "code": "IgbtOverTemp",
      "severity": "critical",
      "active": true,
      "message": "IGBT module temperature exceeded safe operating range",
      "value": 105.2,
      "threshold": 95.0,
      "raisedAt": "2026-03-13T14:22:10Z"
    }
  ],
  "deviceLocalTime": "2026-03-13T14:22:15Z"
}
```

#### All-clear snapshot (no active alarms, triggers auto-recovery of any stale DB alarms)

```json
{
  "rtuSn": "RTU-BESS-001",
  "deviceName": "Battery1",
  "alarms": [],
  "deviceLocalTime": "2026-03-13T15:00:00Z"
}
```

---

## 9. RTU Handshake Sample

Sent once on RTU boot or reconnection. Binds serial numbers and actual sub-device counts to the topology.

```json
{
  "rtuSerialNumber": "RTU-BESS-001",
  "kind": "Rtu.Handshake",
  "timestamp": "2026-03-13T06:00:00Z",
  "payload": {
    "devices": [
      {
        "deviceName": "Battery1",
        "serialNumber": "OTT-BESS-2025-00142",
        "firmwareVersion": "2.4.1",
        "actualCounts": { "rack": 12, "packPerRack": 4, "modulePerPack": 8 }
      },
      {
        "deviceName": "PCS1",
        "serialNumber": "SUN-SC500-2025-A0087",
        "firmwareVersion": "3.0.1"
      },
      {
        "deviceName": "GridMeter1",
        "serialNumber": "ABB-M2M-44210",
        "firmwareVersion": "1.2.0"
      }
    ]
  }
}
```

---

## 10. Batch Ingest Sample

A typical batch submission combining multiple message types from one RTU scan cycle. Sent to `POST /api/v1/DeviceData/Ingest/Batch` with the `X-API-Key` header.

```json
{
  "messages": [
    {
      "rtuSerialNumber": "RTU-BESS-001",
      "deviceName": "Battery1",
      "kind": "Battery.Energy",
      "path": null,
      "timestamp": "2026-03-13T12:30:05Z",
      "payload": {
        "soc": 72.4,
        "voltage": 787.2,
        "current": -185.6,
        "power": 146.1,
        "powerDirection": "Discharging",
        "remainingEnergy": 2490.6,
        "availableEnergy": 3268.0,
        "tempMin": 22.1,
        "tempMax": 28.7,
        "tempAvg": 25.3,
        "cellVoltageMax": 3312,
        "cellVoltageMin": 3289,
        "cellVoltageAvg": 3301,
        "voltageSpread": 23,
        "cellTempMax": 29.4,
        "cellTempMin": 21.8,
        "maxCellVoltageId": "R3-M12-C7",
        "minCellVoltageId": "R8-M5-C14",
        "ambientTemp": 24.6
      }
    },
    {
      "rtuSerialNumber": "RTU-BESS-001",
      "deviceName": "Battery1",
      "kind": "Battery.Status",
      "path": null,
      "timestamp": "2026-03-13T12:30:05Z",
      "payload": {
        "operatingMode": "Discharging",
        "isCharging": false,
        "isConnected": true,
        "alarmActive": false,
        "faultCode": 0,
        "warningFlags": 0,
        "protectionFlags": 0
      }
    },
    {
      "rtuSerialNumber": "RTU-BESS-001",
      "deviceName": "Battery1",
      "kind": "Battery.RackEnergy",
      "path": "Rack/1",
      "timestamp": "2026-03-13T12:30:05Z",
      "payload": {
        "soc": 72.8,
        "voltage": 65.7,
        "current": -15.3,
        "power": 12.1,
        "tempMin": 22.8,
        "tempMax": 27.4,
        "tempAvg": 25.0,
        "cellVoltageMax": 3308,
        "cellVoltageMin": 3291,
        "contactorStatus": "Closed",
        "balancingActive": false
      }
    },
    {
      "rtuSerialNumber": "RTU-BESS-001",
      "deviceName": "Battery1",
      "kind": "Battery.RackEnergy",
      "path": "Rack/2",
      "timestamp": "2026-03-13T12:30:05Z",
      "payload": {
        "soc": 73.1,
        "voltage": 65.6,
        "current": -15.4,
        "power": 12.2,
        "tempMin": 23.0,
        "tempMax": 27.8,
        "tempAvg": 25.1,
        "cellVoltageMax": 3310,
        "cellVoltageMin": 3294,
        "contactorStatus": "Closed",
        "balancingActive": true
      }
    },
    {
      "rtuSerialNumber": "RTU-BESS-001",
      "deviceName": "PCS1",
      "kind": "Pcs.Status",
      "path": null,
      "timestamp": "2026-03-13T12:30:05Z",
      "payload": {
        "pcsWorkingCondition": "Running",
        "pcsSourceMode": "GridTied",
        "pcsGridMode": "OnGrid",
        "pcsPhaseSequence": "Positive",
        "pcsDeratingCoefficient": 100.0,
        "pcsIgbtTemp": 52.3,
        "pcsAmbientTemp": 34.1,
        "pcsInductanceTemp": 48.7,
        "pcsEfficiency": 97.8
      }
    },
    {
      "rtuSerialNumber": "RTU-BESS-001",
      "deviceName": "GridMeter1",
      "kind": "Grid.Power",
      "path": null,
      "timestamp": "2026-03-13T12:30:05Z",
      "payload": {
        "activePower": -142.8,
        "reactivePower": 5.4,
        "apparentPower": 143.0,
        "powerFactor": -0.999,
        "gridFrequency": 50.02,
        "phaseVoltageA": 230.3,
        "phaseVoltageB": 230.6,
        "phaseVoltageC": 230.0,
        "phaseCurrentA": 207.0,
        "phaseCurrentB": 207.2,
        "phaseCurrentC": 206.8,
        "thdVoltage": 1.8,
        "thdCurrent": 3.2
      }
    }
  ]
}
```
