# MQTT Topic Structure — CNI DERMS

## Overview

All MQTT communication between RTUs and the CNI Backend flows through EMQX broker using a structured topic hierarchy.

## Topic Format

```
cni/{facilityId}/{rtuId}/{channel}
```

| Segment | Description | Example |
|---------|-------------|---------|
| `cni` | Fixed namespace prefix | `cni` |
| `{facilityId}` | UUID of the facility | `82285c08-0e78-4232-bb4a-5b4d846abc44` |
| `{rtuId}` | UUID of the RTU | `a1b2c3d4-e5f6-7890-abcd-ef1234567890` |
| `{channel}` | Message channel type | `telemetry`, `alarm`, `command` |

## Channels

### Inbound: RTU → Backend

#### `cni/{facilityId}/{rtuId}/telemetry`

Device telemetry data published by the RTU.

**Payload:**
```json
{
  "deviceName": "Battery1",
  "kind": "ContainerBatteryState",
  "path": null,
  "payload": "{\"socPercent\":85,\"sohPercent\":100,\"voltage\":51.2}",
  "deviceLocalTime": "2026-03-10T14:35:00.000+03:00"
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `deviceName` | string | Yes | Serial number or name of the device under this RTU |
| `kind` | string | Yes | Telemetry type identifier (see Kind Categories below) |
| `path` | string | No | Optional sub-path for hierarchical addressing |
| `payload` | string (JSON) | Yes | Device-specific telemetry data as a JSON string |
| `deviceLocalTime` | ISO 8601 | No | Local timestamp from the device |

**Kind Categories:**

| Category | Kind Prefixes | Storage |
|----------|---------------|---------|
| Battery/BESS | `ContainerBattery*`, `Battery*`, `Bess*`, `Rack*`, `Pack*`, `Module*`, `Chiller*`, `HVCU*` | `DeviceBatteryData` |
| PCS | `Pcs*`, `PCS*` | `DevicePcsData` |
| Solar | `Solar*`, `Pv*`, `PV*`, `Irradiance*` | `DeviceSolarData` |
| Grid/Meter | `Meter*`, `Grid*`, `SmartMeter*`, `Wind*` | `DeviceSmartMeterData` |

#### `cni/{facilityId}/{rtuId}/alarm`

Device alarm data published by the RTU.

**Payload:**
```json
{
  "rtuSn": "RTU-001",
  "deviceName": "Battery1",
  "alarms": [
    {
      "code": "BMU_OV",
      "severity": "Critical",
      "active": true,
      "message": "Battery over-voltage detected",
      "value": 58.5,
      "threshold": 57.0,
      "raisedAt": "2026-03-10T14:30:00+03:00"
    }
  ],
  "deviceLocalTime": "2026-03-10T14:35:00+03:00"
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `rtuSn` | string | Yes | RTU serial number |
| `deviceName` | string | Yes | Device serial number |
| `alarms` | array | Yes | List of alarm states |
| `alarms[].code` | string | Yes | Alarm type code |
| `alarms[].severity` | string | No | `Critical`, `Major`, `Minor`, `Warning` |
| `alarms[].active` | bool | Yes | `true` = fault active, `false` = recovered |
| `alarms[].message` | string | No | Human-readable description |
| `alarms[].value` | number | No | Measured value that triggered the alarm |
| `alarms[].threshold` | number | No | Threshold value |
| `alarms[].raisedAt` | ISO 8601 | No | When the alarm was first raised |

### Outbound: Backend → RTU

#### `cni/{facilityId}/{rtuId}/command`

Commands and data pushes from the backend to a specific RTU.

Each message has a `type` field that identifies the command kind.

##### `type: "day-ahead-prices"`

Day-ahead energy market prices, published automatically after the backend fetches from the market operator.

**Payload:**
```json
{
  "type": "day-ahead-prices",
  "date": "2026-03-10",
  "currency": "TRY",
  "unit": "TRY/kWh",
  "source": "EPIAS_DAF",
  "prices": [
    {"hour_start": 0, "hour_end": 1, "price": 1.99},
    {"hour_start": 1, "hour_end": 2, "price": 1.7232},
    {"hour_start": 2, "hour_end": 3, "price": 1.7232},
    {"hour_start": 3, "hour_end": 4, "price": 1.7232},
    {"hour_start": 4, "hour_end": 5, "price": 2.895},
    {"hour_start": 5, "hour_end": 6, "price": 3.2885},
    {"hour_start": 6, "hour_end": 7, "price": 2.676},
    {"hour_start": 7, "hour_end": 8, "price": 2.3477},
    {"hour_start": 8, "hour_end": 9, "price": 2.7897},
    {"hour_start": 9, "hour_end": 10, "price": 1.684},
    {"hour_start": 10, "hour_end": 11, "price": 1.2},
    {"hour_start": 11, "hour_end": 12, "price": 1.0},
    {"hour_start": 12, "hour_end": 13, "price": 0.25},
    {"hour_start": 13, "hour_end": 14, "price": 0.501},
    {"hour_start": 14, "hour_end": 15, "price": 1.0},
    {"hour_start": 15, "hour_end": 16, "price": 1.0},
    {"hour_start": 16, "hour_end": 17, "price": 1.5},
    {"hour_start": 17, "hour_end": 18, "price": 2.994},
    {"hour_start": 18, "hour_end": 19, "price": 3.399},
    {"hour_start": 19, "hour_end": 20, "price": 3.246},
    {"hour_start": 20, "hour_end": 21, "price": 3.4},
    {"hour_start": 21, "hour_end": 22, "price": 3.2},
    {"hour_start": 22, "hour_end": 23, "price": 3.0402},
    {"hour_start": 23, "hour_end": 24, "price": 2.7778}
  ]
}
```

| Field | Type | Description |
|-------|------|-------------|
| `type` | string | Always `"day-ahead-prices"` |
| `date` | string | Date in `YYYY-MM-DD` format |
| `currency` | string | Currency code (`TRY`, `EUR`) |
| `unit` | string | Price unit (`TRY/kWh`, `EUR/kWh`) — always per kWh |
| `source` | string | Market operator code + `_DAF` suffix (e.g., `EPIAS_DAF`, `ENTSOE_DAF`) |
| `prices` | array | 24 hourly price entries |
| `prices[].hour_start` | int | Hour start (0-23) |
| `prices[].hour_end` | int | Hour end (1-24) |
| `prices[].price` | decimal | Net price in the specified currency per kWh |

**Publishing Schedule:**
- Twice daily: **00:01 UTC** and **13:01 UTC**
- Automatically published to all RTUs linked to the market operator after prices are fetched

## Device Identity Resolution

Understanding how the backend resolves device identity from MQTT messages:

| Stage | Identifier | Source | Example |
|-------|-----------|--------|---------|
| **MQTT topic** | `facilityId`, `rtuId` | Topic path segments | `cni/82285c08-.../a1b2c3d4-.../telemetry` |
| **Payload** | `deviceName` | JSON field | `"Battery1"` |
| **DB lookup** | `Device.Name + RtuId` | Ingestion handler | `WHERE Name = 'Battery1' AND RtuId = 'a1b2c3d4-...'` |
| **Storage** | `Device.Id` (Guid) | Telemetry FK | `DeviceBatteryData.DeviceId = '...'` |
| **Frontend** | `Device.Id` (Guid) | All query APIs | `GET /BatteryStorage/Units/{unitId}/summary` |

**Key points:**
- `deviceName` must match the `Device.Name` registered in the database
- `deviceName` only needs to be unique **within the same RTU** (not globally)
- `deviceName` is only used at the ingestion boundary — all query endpoints and telemetry joins use `Device.Id` (Guid)
- The frontend never sees or uses `deviceName`

## QoS

All messages use **QoS 1 (At Least Once)** — messages are guaranteed to arrive but may be delivered more than once. RTUs should handle duplicate messages gracefully.

## Authentication

- RTUs authenticate with EMQX using username/password credentials
- Backend uses a dedicated publisher client (`cni-backend-publisher`) with broker credentials
- EMQX ACL rules should restrict:
  - RTUs: **publish** to `cni/{facilityId}/{rtuId}/telemetry` and `cni/{facilityId}/{rtuId}/alarm` only
  - RTUs: **subscribe** to `cni/{facilityId}/{rtuId}/command` only
  - Backend publisher: **publish** to `cni/+/+/command`
  - Backend subscriber: **subscribe** to `cni/+/+/telemetry` and `cni/+/+/alarm`
