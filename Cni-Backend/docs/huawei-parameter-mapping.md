# Huawei FusionSolar → CNI Parameter Mapping

This document maps Huawei FusionSolar API fields to CNI system parameters for the LUNA2000 battery and SUN2000 inverter integration.

---

## Battery (Huawei devTypeId=39 → `DeviceBatteryData`, Kind: `Battery.Energy`)

| CNI Parameter | Huawei API Field | Huawei Payload Key (translator) | Unit | Notes |
|---|---|---|---|---|
| `soc` | `battery_soc` | `soc` | % | Direct match |
| `soh` | `battery_soh` | `soh` | % | Direct match |
| `power` | `ch_discharge_power` | `chDischargePower` | W | **Huawei sends Watts, our system expects kW** — needs `/1000` |
| `voltage` | `busbar_u` | `busbarVoltage` | V | DC bus voltage |
| `current` | — | — | A | **Not available from Huawei battery API** |
| `powerDirection` | — | — | — | Can be derived: `chDischargePower > 0` = discharging, `< 0` = charging |
| `operatingMode` | `battery_status` | `batteryStatus` | — | Huawei: 0=offline, 1=standby, 2=running, 3=fault. **Needs mapping** to "idle"/"charging"/"discharging" |
| `availableEnergy` | `rated_capacity` | `ratedCapacity` | kWh | Rated, not remaining |
| `remainingEnergy` | — | — | kWh | **Not available** — could estimate as `ratedCapacity * soc / 100` |
| `tempMin` | — | — | °C | **Not available** (LUNA2000 doesn't expose cell temps) |
| `tempMax` | — | — | °C | **Not available** |
| `ambientTemp` | — | — | °C | **Not available** |
| `cellVoltageMax` | — | — | mV | **Not available** |
| `cellVoltageMin` | — | — | mV | **Not available** |
| `cycleCount` | — | — | — | **Not available** |
| — | `charge_cap` | `chargeEnergy` | kWh | Total charge energy — **no CNI equivalent yet** |
| — | `discharge_cap` | `dischargeEnergy` | kWh | Total discharge energy — **no CNI equivalent yet** |
| — | `max_charge_power` | `maxChargePower` | W | Max allowed charge — **no CNI equivalent yet** |
| — | `max_discharge_power` | `maxDischargePower` | W | Max allowed discharge — **no CNI equivalent yet** |
| — | `battery_unit_info` | `batteryUnitInfo` | — | Per-unit SOH array — **no CNI equivalent yet** |

---

## Inverter/PCS (Huawei devTypeId=1 → `DevicePcsData`, Kind: `Pcs.AcMeasurement`)

| CNI Parameter | Huawei API Field | Huawei Payload Key (translator) | Unit | Notes |
|---|---|---|---|---|
| `pcsActivePower` | `active_power` | `activePower` | kW | Direct match |
| `pcsReactivePower` | `reactive_power` | `reactivePower` | kVar | Direct match |
| `pcsPowerFactor` | `power_factor` | `powerFactor` | — | Direct match |
| `pcsGridFreq` | `elec_freq` | `gridFrequency` | Hz | Direct match |
| `pcsPhaseVoltageA` | `a_u` | `phaseAVoltage` | V | Direct match |
| `pcsPhaseVoltageB` | `b_u` | `phaseBVoltage` | V | Direct match |
| `pcsPhaseVoltageC` | `c_u` | `phaseCVoltage` | V | Direct match |
| `pcsPhaseCurrentA` | `a_i` | `phaseACurrent` | A | Direct match |
| `pcsPhaseCurrentB` | `b_i` | `phaseBCurrent` | A | Direct match |
| `pcsPhaseCurrentC` | `c_i` | `phaseCCurrent` | A | Direct match |
| `pcsLineVoltageAB` | `ab_u` | `lineABVoltage` | V | Direct match |
| `pcsLineVoltageBC` | `bc_u` | `lineBCVoltage` | V | Direct match |
| `pcsLineVoltageCA` | `ca_u` | `lineCAVoltage` | V | Direct match |
| `pcsIgbtTemp` | `temperature` | `temperature` | °C | Inverter temp, not IGBT specifically |
| `pcsEfficiency` | `efficiency` | `efficiency` | % | Direct match |
| `pcsGridMode` | `inverter_state` | `inverterState` | — | **Needs mapping** — Huawei uses different state codes |
| `pcsDcPower` | — | — | kW | **Not directly available** |
| `pcsBatteryVoltage` | — | — | V | **Not available** |
| `pcsBatteryCurrent` | — | — | A | **Not available** |
| — | `day_cap` | `dailyYield` | kWh | Daily energy — **no CNI equivalent yet** |
| — | `total_cap` | `totalYield` | kWh | Lifetime energy — **no CNI equivalent yet** |

---

## Inverter/Solar (Huawei devTypeId=1 → `DeviceSolarData`, Kind: `Solar.Power`)

| CNI Parameter | Huawei API Field | Huawei Payload Key (translator) | Unit | Notes |
|---|---|---|---|---|
| — | `active_power` | `activePower` | kW | Total solar output |
| — | `mppt_power` | `mpptPower` | kW | MPPT tracker power |
| — | `day_cap` | `dailyYield` | kWh | Today's generation |
| — | `total_cap` | `totalYield` | kWh | Lifetime generation |
| — | `pv1_u`..`pv28_u` | `pv1Voltage`..`pv28Voltage` | V | PV string voltages |
| — | `pv1_i`..`pv28_i` | `pv1Current`..`pv28Current` | A | PV string currents |

Solar parameters aren't mapped to CNI keys yet — solar queries have their own extraction logic.

---

## Key Gaps & Issues

1. **Unit mismatch**: Huawei `ch_discharge_power` is in **Watts**, CNI `power` expects **kW** — the translator or ParameterMapTemplate formula needs to divide by 1000
2. **No current**: Huawei battery API doesn't expose current separately — `power` formula (`voltage * current / 1000`) won't work, need direct source instead
3. **No cell-level data**: LUNA2000 residential doesn't expose cell temps/voltages — those fields will be null
4. **Status mapping**: `battery_status` (0/1/2/3) needs conversion to string ("offline"/"standby"/"running"/"fault")
5. **No chiller**: Residential batteries have no thermal management system — all chiller parameters will be null

---

## Huawei Battery Status Mapping

| Huawei `battery_status` | Meaning | CNI `operatingMode` |
|---|---|---|
| 0 | Offline | `"offline"` |
| 1 | Standby | `"idle"` |
| 2 | Running | Derive from `ch_discharge_power`: `> 0` → `"discharging"`, `< 0` → `"charging"` |
| 3 | Fault | `"fault"` |

---

## Pem Villa EMMA Device Structure

```
Station: NE=55911180 "Pem Villa EMMA"
├── SmartGuard     (devTypeId: 23071, id: 1000000055833014)
├── Inverter-12    (devTypeId: 1,     id: 1000000055833015, SN: BT2460093355)
│   └── Battery-1  (devTypeId: 39,    id: 1000000055833018, LUNA2000-10KW-C1)
├── Inverter-13    (devTypeId: 1,     id: 1000000060185746, SN: BT2511002783)
│   └── Battery-1  (devTypeId: 39,    id: 1000000060185748, LUNA2000-5KW-C0)
├── Inverter-14    (devTypeId: 1,     id: 1000000060185749, SN: BT2511002884)
│   └── Battery-1  (devTypeId: 39,    id: 1000000060185750, LUNA2000-5KW-C0)
└── EMMA           (devTypeId: 23070, id: 1000000055833012)
```

### CNI Hierarchy Mapping

Each Huawei inverter becomes **two** CNI Device records (PCS + Solar).
Each Huawei battery becomes **one** CNI Device (EnergyStorage).

```
Virtual RTU: "HUAWEI-NE=55911180"
├── Battery-12 (EnergyStorage, LUNA2000-10KW-C1)
│   └── Inverter-12-PCS (PCS, SUN2000-10K-MAP0)
├── Battery-13 (EnergyStorage, LUNA2000-5KW-C0)
│   └── Inverter-13-PCS (PCS, SUN2000-10K-MAP0)
├── Battery-14 (EnergyStorage, LUNA2000-5KW-C0)
│   └── Inverter-14-PCS (PCS, SUN2000-10K-MAP0)
├── Inverter-12-Solar (Solar)
├── Inverter-13-Solar (Solar)
└── Inverter-14-Solar (Solar)
```
