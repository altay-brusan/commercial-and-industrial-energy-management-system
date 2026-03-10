# Huawei FusionSolar Integration — Case Study

## 1. How the Huawei API Works

### Authentication

1. **Login** — `POST {baseUrl}/login` with `{ userName, systemCode }`
2. Response returns **multiple Set-Cookie headers**: `XSRF-TOKEN`, `HWWAFSESID`, `HWWAFSESTIME`, plus a session ID
3. All cookies are captured and packed into a single string: `"xsrfToken|||cookie1=v1; cookie2=v2; ..."`
4. Token is cached in `IMemoryCache` for 25 minutes (Huawei tokens expire at 30 min)

### Every API Call

Every subsequent call sends **two headers**:
- `XSRF-TOKEN: <token>` — anti-CSRF header
- `Cookie: XSRF-TOKEN=...; HWWAFSESID=...; HWWAFSESTIME=...; ...` — full session cookies

Without both, Huawei returns error code 305 (token expired).

### API Hierarchy

```
Account (userName + systemCode)
  └── Station (plantCode, e.g. "NE=55911180" = Pem Villa EMMA)
        ├── SmartGuard    (devTypeId: 23071) — skipped
        ├── EMMA           (devTypeId: 23070) — skipped
        ├── Inverter       (devTypeId: 1)     — grid-tied hybrid inverter
        └── Battery        (devTypeId: 39)    — lithium storage unit
```

### Endpoints Used

| Endpoint | Purpose | Payload |
|---|---|---|
| `POST /login` | Get session token | `{ userName, systemCode }` |
| `POST /getStationList` | List all stations | `{ pageNo, pageSize }` |
| `POST /getDevList` | List devices in a station | `{ stationCodes: "NE=55911180" }` |
| `POST /getDevRealKpi` | Real-time telemetry | `{ devIds: "id1,id2", devTypeId: 1 }` |

### Pem Villa EMMA Devices

```
Inverter-12  (id: 1000000055833015, SN: BT2460093355, SUN2000-10K)
Inverter-13  (id: 1000000060185746, SN: BT2511002783, SUN2000-10K)
Inverter-14  (id: 1000000060185749, SN: BT2511002884, SUN2000-10K)
Battery-1    (id: 1000000055833018, LUNA2000-10KW-C1)
Battery-1    (id: 1000000060185748, LUNA2000-5KW-C0)
Battery-1    (id: 1000000060185750, LUNA2000-5KW-C0)
```

### Sample API Responses

**`getDevRealKpi` for batteries (devTypeId=39):**
```json
{
  "battery_soc": 85.0,
  "battery_soh": 100.0,
  "ch_discharge_power": -2400.0,
  "battery_status": 2,
  "busbar_u": 380.0,
  "rated_capacity": 10.0,
  "charge_cap": 120.5,
  "discharge_cap": 98.3,
  "max_charge_power": 5000,
  "max_discharge_power": 5000
}
```

**`getDevRealKpi` for inverters (devTypeId=1):**
```json
{
  "active_power": 5.2,
  "reactive_power": 0.3,
  "power_factor": 0.99,
  "elec_freq": 50.01,
  "temperature": 42.0,
  "efficiency": 97.5,
  "a_u": 230.1, "b_u": 229.8, "c_u": 230.3,
  "a_i": 7.5, "b_i": 7.4, "c_i": 7.6,
  "pv1_u": 380.0, "pv1_i": 6.8,
  "pv2_u": 375.0, "pv2_i": 6.5,
  "mppt_power": 5.1,
  "day_cap": 32.4,
  "total_cap": 12540.0,
  "inverter_state": 512,
  "run_state": 1
}
```

### Virtual RTU Mapping

Since Huawei has no physical RTU — it's a cloud API — we create a **virtual RTU** so the data fits into our existing entity hierarchy:

```
Facility: Pem Villa EMMA (DataSource = Huawei)
  └── RTU: HUAWEI-NE55911180 (virtual)
        ├── Battery-12  (EnergyStorage) ← Huawei Battery 1000000055833018
        │     └── Inverter-12-PCS (PCS) ← Huawei Inverter 1000000055833015
        ├── Battery-13  (EnergyStorage) ← Huawei Battery 1000000060185748
        │     └── Inverter-13-PCS (PCS) ← Huawei Inverter 1000000060185746
        ├── Battery-14  (EnergyStorage) ← Huawei Battery 1000000060185750
        │     └── Inverter-14-PCS (PCS) ← Huawei Inverter 1000000060185749
        ├── Inverter-12-Solar (Solar) ← same Huawei Inverter 1000000055833015
        ├── Inverter-13-Solar (Solar) ← same Huawei Inverter 1000000060185746
        └── Inverter-14-Solar (Solar) ← same Huawei Inverter 1000000060185749
```

The virtual RTU + device hierarchy means all existing BESS/PCS/Solar query handlers work without any changes.

### Data Flow (every 5 minutes via Hangfire)

```
Hangfire triggers SyncAllAccounts()
    │
    ├── Load all active HuaweiAccounts + StationMappings + DeviceMappings
    │
    ├── Pre-load all CNI Devices into memory (avoids N+1)
    │
    └── For each account:
        │
        ├── Get token (from cache or fresh login)
        │
        └── For each enabled station:
            │
            ├── Group device mappings by devTypeId (1=inverter, 39=battery)
            │
            ├── POST /getDevRealKpi { devIds: "id1,id2,id3", devTypeId: 39 }
            │   └── Returns: [{ devId: 123, dataItemMap: { battery_soc: 85, ... } }]
            │
            ├── POST /getDevRealKpi { devIds: "id1,id2,id3", devTypeId: 1 }
            │   └── Returns: [{ devId: 456, dataItemMap: { active_power: 5.2, ... } }]
            │
            └── For each KPI response:
                │
                ├── Battery (39) → HuaweiDataTranslator.TranslateBattery()
                │   └── Writes DeviceBatteryData { Kind: "Battery.Energy" }
                │
                └── Inverter (1) → looks up CNI device type:
                    ├── PCS device → TranslatePcs()
                    │   └── Writes DevicePcsData { Kind: "Pcs.AcMeasurement" }
                    └── Solar device → TranslateSolar()
                        └── Writes DeviceSolarData { Kind: "Solar.Power" }
```

### Error Handling

| Huawei Error | Code | Response |
|---|---|---|
| Token expired | 305 | Invalidate cache, re-login, retry the station |
| Rate limited | 407 | Log warning, stop syncing remaining stations |
| Other API error | varies | Log + throw, skip to next account |

### Rate Limits

- **Login**: 5 calls per 10 min
- **getDevRealKpi**: `ceil(devices/100)` calls per 5 min per device type
- Pem Villa (3 inverters + 3 batteries) = 2 API calls per sync cycle

---

## 2. Class Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              HANGFIRE (every 5 min)                        │
│                                    │                                       │
│                                    ▼                                       │
│                     ┌──────────────────────────────┐                       │
│                     │ HuaweiFusionSolarSyncWorker   │                      │
│                     │──────────────────────────────│                       │
│                     │ SyncAllAccounts()             │                      │
│                     │ SyncAccountAsync()            │                      │
│                     │ SyncStationAsync()            │                      │
│                     │ TranslateAndStore()           │                      │
│                     └──────┬───────┬───────┬───────┘                       │
│                            │       │       │                               │
│              ┌─────────────┘       │       └─────────────┐                 │
│              ▼                     ▼                     ▼                 │
│  ┌───────────────────┐  ┌──────────────────┐  ┌──────────────────────┐    │
│  │ HuaweiTokenManager│  │ IHuaweiApiClient │  │ HuaweiDataTranslator │    │
│  │───────────────────│  │──────────────────│  │──────────────────────│    │
│  │ GetTokenAsync()   │  │ LoginAsync()     │  │ TranslateBattery()   │    │
│  │ RefreshTokenAsync()│  │ GetDevRealKpi() │  │ TranslatePcs()       │    │
│  │ InvalidateToken() │  │ GetStationsAsync()│  │ TranslateSolar()     │    │
│  └──────┬────────────┘  │ GetDeviceList()  │  └──────────┬───────────┘    │
│         │               └────────┬─────────┘             │                │
│         │                        │                       │                │
│         ▼                        ▼                       ▼                │
│  ┌──────────────────┐  ┌──────────────────┐  ┌───────────────────────┐   │
│  │   IMemoryCache   │  │  HuaweiApiClient │  │  IApplicationDbContext │   │
│  │  (25-min token   │  │──────────────────│  │───────────────────────│   │
│  │   cache)         │  │ PostAsync<T>()   │  │ DeviceBatteryData     │   │
│  └──────────────────┘  │ ParseSessionToken│  │ DevicePcsData         │   │
│         ▲               │ LoginForSession()│  │ DeviceSolarData       │   │
│         │               └────────┬─────────┘  │ HuaweiAccounts       │   │
│  ┌──────┴───────────────┐        │            │ HuaweiStationMappings│   │
│  │HuaweiCredentialEncryptor│      ▼            │ HuaweiDeviceMappings │   │
│  │──────────────────────│  ┌──────────────┐   │ HuaweiRawDataLogs    │   │
│  │ Encrypt()            │  │  HttpClient  │   │ HuaweiSyncLogs       │   │
│  │ Decrypt()            │  │ (UseCookies  │   └───────────┬───────────┘   │
│  └──────────────────────┘  │  = false)    │               │               │
│         ▲                  └──────┬───────┘               │               │
│         │                         │                       ▼               │
│  ┌──────┴──────────┐              │              ┌────────────────┐       │
│  │ HuaweiSettings  │              ▼              │   PostgreSQL   │       │
│  │─────────────────│     ┌────────────────┐      │────────────────│       │
│  │ Enabled         │     │ Huawei Cloud   │      │ public schema: │       │
│  │ PollIntervalSec │     │ intl.fusion... │      │  DeviceBattery │       │
│  │ TokenCacheMin   │     │ /thirdData     │      │  DevicePcs     │       │
│  │ EncryptionKey   │     └────────────────┘      │  DeviceSolar   │       │
│  └─────────────────┘                             │ huawei schema: │       │
│                                                  │  Accounts      │       │
│                                                  │  StationMaps   │       │
│                                                  │  DeviceMaps    │       │
│                                                  │  RawDataLogs   │       │
│                                                  │  SyncLogs      │       │
│                                                  └────────────────┘       │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Domain Entities (huawei schema)

```
┌─────────────────┐ 1    * ┌──────────────────────┐ 1   * ┌────────────────┐
│  HuaweiAccount  │───────▶│ HuaweiStationMapping │──────▶│ HuaweiDevice   │
│─────────────────│        │──────────────────────│       │ Mapping        │
│ TenantId        │        │ StationCode          │       │────────────────│
│ UserName        │        │ FacilityId ──────┐   │       │ HuaweiDeviceId │
│ EncryptedSystem │        │ SyncEnabled      │   │       │ HuaweiDevTypeId│
│   Code          │        │ LastSyncUtc      │   │       │ DeviceId ──────┼─┐
│ BaseUrl         │        └──────────────────┘   │       │ DeviceName     │ │
│ IsActive        │              ▲                │       └────────────────┘ │
└─────────────────┘              │                │                          │
        │                 ┌──────┴──────┐         │       ┌──────────────┐   │
        │ 1    *          │  Facility   │◀────────┘       │   Device     │◀──┘
        ▼                 │ (DataSource │                  │ (public      │
┌───────────────┐         │  = Huawei)  │                  │  schema)     │
│ HuaweiRawData │         └─────────────┘                  └──────────────┘
│ Log           │
│ HuaweiSyncLog │
└───────────────┘
```

### DI Registration & Lifetimes

| Class | Lifetime | Why |
|---|---|---|
| `HuaweiCredentialEncryptor` | Singleton | Stateless, holds encryption key |
| `HuaweiDataTranslator` | Singleton | Stateless, pure mapping logic |
| `IHuaweiApiClient` → `HuaweiApiClient` | Transient | Typed HttpClient (managed by factory) |
| `HuaweiTokenManager` | Scoped | Uses scoped `IHuaweiApiClient`, but token cache is in singleton `IMemoryCache` |
| `HuaweiFusionSolarSyncWorker` | Scoped | Uses scoped DbContext |

---

## 3. The Parameter Resolution Problem

### The Problem

The existing query handlers were built for physical RTU devices that send telemetry with specific Kind names:

| Handler | Expected Kind | Huawei writes |
|---|---|---|
| GetBessUnits / GetBessSummary | `ContainerBatteryState` | `Battery.Energy` |
| GetPcsUnits / GetPcsGeneral | `PcsPowerState`, `PcsStatusState`, `PcsAcMeasState` | `Pcs.AcMeasurement` |
| GetSolarEnergyProduction | nested `{ DcOutput: { TotalEnergyDaily } }` | flat `{ dailyYield }` |

All query handlers follow the same pattern:

```csharp
// Hardcoded Kind lookup — fails for Huawei
var batt = payloadsByKind.GetValueOrDefault("ContainerBatteryState"); // → null

// Then for each field:
var soc = resolver?.ResolveNumber("soc")          // ← tries ParameterMapTemplate first
    ?? ExtractNum(batt, "socPercent");              // ← hardcoded fallback (null for Huawei)
```

Without `ParameterMapTemplate`, the resolver is null, the hardcoded fallback finds nothing → **all fields show as null**.

### The Solution: DeviceSpecificationTemplate with ParameterMapTemplate

Each Huawei device gets a `DeviceSpecificationTemplate` that defines how to extract parameters from Huawei's payload format:

```json
{
  "version": 1,
  "parameters": {
    "soc": {
      "type": "number", "unit": "%",
      "source": { "kind": "Battery.Energy", "paths": ["soc"] }
    },
    "power": {
      "type": "number", "unit": "W",
      "source": { "kind": "Battery.Energy", "paths": ["chDischargePower"] }
    },
    "operatingMode": {
      "type": "string",
      "formula": {
        "expression": "cond(eq(s,0),'offline',cond(eq(s,1),'standby',cond(eq(s,2),'running',cond(eq(s,3),'fault','unknown'))))",
        "inputs": { "s": {"source": {"kind": "Battery.Energy", "paths": ["batteryStatus"]}} }
      }
    }
  }
}
```

The `ParameterResolver` reads this template, knows to look in `Battery.Energy` Kind payloads, and extracts the correct fields. The hardcoded fallback is never reached.

### Parameter Map Templates Created

**LUNA2000 Battery** (Kind: `Battery.Energy`):

| ParameterKey | Payload Field | Notes |
|---|---|---|
| `soc` | `soc` | State of charge % |
| `soh` | `soh` | State of health % |
| `power` | `chDischargePower` | Charge/discharge power in W |
| `powerDirection` | formula from `power` | charging/discharging/idle |
| `voltage` | `busbarVoltage` | DC bus voltage |
| `availableEnergy` | `ratedCapacity` | Rated capacity in kWh |
| `operatingMode` | formula from `batteryStatus` | offline/standby/running/fault |

**SUN2000 Inverter-PCS** (Kind: `Pcs.AcMeasurement`):

| ParameterKey | Payload Field |
|---|---|
| `pcsActivePower` | `activePower` |
| `pcsReactivePower` | `reactivePower` |
| `pcsPowerFactor` | `powerFactor` |
| `pcsGridFreq` | `gridFrequency` |
| `pcsIgbtTemp` | `temperature` |
| `pcsEfficiency` | `efficiency` |
| `pcsPhaseVoltageA/B/C` | `phaseAVoltage` / `phaseBVoltage` / `phaseCVoltage` |
| `pcsPhaseCurrentA/B/C` | `phaseACurrent` / `phaseBCurrent` / `phaseCCurrent` |
| `pcsLineVoltageAB/BC/CA` | `lineABVoltage` / `lineBCVoltage` / `lineCAVoltage` |

**SUN2000 Inverter-Solar** (Kind: `Solar.Power`):

| ParameterKey | Payload Field |
|---|---|
| `activePower` | `activePower` |
| `dailyYield` | `dailyYield` |
| `totalYield` | `totalYield` |
| `mpptPower` | `mpptPower` |

---

## 4. How the Summary Endpoint Works for Huawei

### Request

```
GET /api/v1/BatteryStorage/Units/{unitId}/summary
```

### Handler Flowchart

```
                            Handle(GetBessSummaryQuery)
                                       │
                                       ▼
                        ┌──────────────────────────┐
                        │ Load Device from DB       │
                        │ Include: DeviceType,      │
                        │ LogicalDevice,            │
                        │ SpecificationTemplate     │
                        └────────────┬─────────────┘
                                     │
                                     ▼
                             ┌───────────────┐
                             │ Device found? │
                             └───────┬───────┘
                                     │
                          no ┌───────┴───────┐ yes
                             ▼               ▼
                      ┌────────────┐  ┌─────────────────────────┐
                      │ throw 404  │  │ Read SpecificationsTemp │
                      └────────────┘  │ late from template      │
                                      └────────────┬────────────┘
                                                   │
                                                   ▼
                                      ┌─────────────────────────┐
                                      │ Has custom tabs?        │
                                      │ specs.tabs.summary      │
                                      │   .sections exists?     │
                                      └────────────┬────────────┘
                                                   │
                                     yes ┌─────────┴─────────┐ no
                                         ▼                   ▼
                              ┌────────────────────┐  ┌──────────────────┐
                              │ SCHEMA-DRIVEN PATH │  │ DEFAULT PATH     │
                              │                    │  │                  │
                              │ LoadLatest()       │  │ Switch on        │
                              │ GetPayloadsByKind()│  │ DeviceType.Name  │
                              │ BuildFromSchema()  │  └────────┬─────────┘
                              │                    │           │
                              │ return             │           ▼
                              └────────────────────┘  ┌────────────────────────┐
                                                      │                        │
                       ┌──────────────┬───────────────┼────────────┬───────────┘
                       ▼              ▼               ▼            ▼
               "EnergyStorage"   "Rack/Pack/    "Controller"    "TMS"
               OR                 Module"
               logicalCategory                                     │
               == "BESS"              │               │            ▼
                       │              ▼               ▼    BuildTmsSummary()
                       │      BuildSubDevice    BuildSubDevice
                       │        Summary()         Summary()
                       ▼
         ┌──────────────────────────────────────┐
         │  BuildBatteryContainerSummary()       │  ← Huawei LUNA2000 goes here
         │                                       │
         │  ┌────────────────────────────────┐   │
         │  │ ContainerTelemetryLoader       │   │
         │  │   .LoadLatest(deviceId)        │   │
         │  │                                │   │
         │  │ 1. Load root + descendants     │   │
         │  │    Battery-12                  │   │
         │  │      └── Inverter-12-PCS       │   │
         │  │                                │   │
         │  │ 2. Categorize each device      │   │
         │  │    Battery-12  → Bess table    │   │
         │  │    Inv-12-PCS  → Pcs table     │   │
         │  │                                │   │
         │  │ 3. Query each table            │   │
         │  │    DeviceBatteryData → latest   │   │
         │  │    DevicePcsData    → latest    │   │
         │  │                                │   │
         │  │ 4. Return payloadsByKind       │   │
         │  └────────────────────────────────┘   │
         │                                       │
         │  payloadsByKind = {                    │
         │    "Battery.Energy": {...},            │
         │    "Pcs.AcMeasurement": {...}          │
         │  }                                     │
         │                                       │
         │  ┌────────────────────────────────┐   │
         │  │ Hardcoded Kind lookups:        │   │
         │  │                                │   │
         │  │ batt = ["ContainerBatteryState"]│   │
         │  │      → null for Huawei         │   │
         │  │ chiller = ["ChillerState"]     │   │
         │  │      → null                    │   │
         │  └────────────────────────────────┘   │
         │                                       │
         │  ┌────────────────────────────────┐   │
         │  │ Has ParameterMapTemplate?      │   │
         │  └──────────┬─────────────────────┘   │
         │             │                         │
         │    yes ┌────┴────┐ no                 │
         │        ▼         ▼                    │
         │  ┌───────────┐ resolver = null        │
         │  │ Build     │ All fields use         │
         │  │ Parameter │ hardcoded fallbacks     │
         │  │ Resolver  │ (would all be null      │
         │  └─────┬─────┘  for Huawei)           │
         │        │                              │
         │        ▼                              │
         │  ┌────────────────────────────────┐   │
         │  │ ParameterResolver              │   │
         │  │                                │   │
         │  │ Has: parameterMap (from templ) │   │
         │  │ Has: payloadsByKind (from DB)  │   │
         │  │ Has: metadata (from device)    │   │
         │  │                                │   │
         │  │ resolver.Resolve("soc"):       │   │
         │  │   map["soc"].source.kind       │   │
         │  │     = "Battery.Energy"         │   │
         │  │   payloads["Battery.Energy"]   │   │
         │  │     .soc = 85                  │   │
         │  └────────────────────────────────┘   │
         │                                       │
         │  ┌────────────────────────────────┐   │
         │  │ Build sections (each field):   │   │
         │  │                                │   │
         │  │ resolver?.ResolveNumber(key)   │   │
         │  │     ↓ found?                   │   │
         │  │   yes → use it                 │   │
         │  │   no  → ?? fallback from batt  │   │
         │  │            (also null for HW)  │   │
         │  └────────────────────────────────┘   │
         │                                       │
         │  Sections built:                      │
         │  ┌────────────────────────────────┐   │
         │  │ 1. Nameplate Information       │   │
         │  │ 2. Live Status                 │   │
         │  │ 3. Energy & Limits             │   │
         │  │ 4. Cell Extremes               │   │
         │  │ 5. Contactors & Safety         │   │
         │  │ 6. Thermal Mgmt (if chiller)   │   │
         │  │ 7. Health Preview              │   │
         │  │ 8. Connected Devices           │   │
         │  └────────────────────────────────┘   │
         │                                       │
         └───────────────────┬───────────────────┘
                             │
                             ▼
                  ┌─────────────────────┐
                  │ return               │
                  │ SummaryResponseDto { │
                  │   Sections = [...]   │
                  │ }                    │
                  └─────────────────────┘
```

### ParameterResolver: Step-by-Step Trace for Huawei

When `BuildLiveStatusSection` resolves each field:

```
resolver.ResolveNumber("soc")
  │
  ├── Look up parameterMap["soc"]
  │   → { source: { kind: "Battery.Energy", paths: ["soc"] } }
  │
  ├── Find payload for kind "Battery.Energy"
  │   → payloadsByKind["Battery.Energy"] = { "soc": 85, "soh": 100, ... }
  │
  ├── Extract value at path "soc"
  │   → 85.0
  │
  └── Return 85.0 ✅
```

```
resolver.ResolveString("operatingMode")
  │
  ├── Look up parameterMap["operatingMode"]
  │   → { formula: { expression: "cond(eq(s,2),'running',...)", inputs: { s: ... } } }
  │
  ├── Resolve input "s"
  │   → source: { kind: "Battery.Energy", paths: ["batteryStatus"] }
  │   → payloadsByKind["Battery.Energy"].batteryStatus = 2
  │
  ├── Evaluate formula: cond(eq(2, 2), 'running', ...)
  │   → "running"
  │
  └── Return "running" ✅
```

### What the Frontend Receives

```json
{
  "sections": [
    {
      "key": "nameplate",
      "title": "Nameplate Information",
      "icon": "info",
      "fields": [
        { "key": "brand", "label": "Brand", "value": "Huawei", "type": "string" },
        { "key": "model", "label": "Model", "value": "LUNA2000-10KW-C1", "type": "string" }
      ]
    },
    {
      "key": "liveStatus",
      "title": "Live Status",
      "icon": "activity",
      "fields": [
        { "key": "soc", "label": "State of Charge", "value": 85.0, "unit": "%", "format": "progress" },
        { "key": "soh", "label": "State of Health", "value": 100.0, "unit": "%" },
        { "key": "power", "label": "Power", "value": 2400, "unit": "kW",
          "meta": { "direction": "discharging" } },
        { "key": "voltage", "label": "Voltage", "value": 380.0, "unit": "V" },
        { "key": "operatingMode", "label": "Operating Mode", "value": "running", "format": "badge" }
      ]
    }
  ]
}
```

### Fields Available vs Not Available for Huawei

| Field | Value | Source |
|---|---|---|
| SOC | 85.0 % | ParameterResolver ✅ |
| SOH | 100.0 % | ParameterResolver ✅ |
| Power | -2400 W | ParameterResolver ✅ |
| Power Direction | "discharging" | ParameterResolver (formula) ✅ |
| Voltage | 380.0 V | ParameterResolver ✅ |
| Available Energy | 10.0 kWh | ParameterResolver ✅ |
| Operating Mode | "running" | ParameterResolver (formula) ✅ |
| Current | null | Not available from Huawei battery API |
| Temp Min/Max/Avg | null | Not available from Huawei battery API |
| Frequency | null | Not available (battery-level) |
| Cell Extremes | null | Not available from Huawei battery API |
| Contactor States | null | Not applicable to residential batteries |

### Key Takeaway

The `ParameterResolver` + `ParameterMapTemplate` system acts as a translation layer between vendor-specific telemetry formats and the generic query handlers. No query handler code needs to change when adding a new vendor — only a new template mapping is needed.
