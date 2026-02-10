# DERMS Real-Time API Specification

> SignalR/WebSocket specification for real-time data streaming in the DERMS platform.
> All payload interfaces are derived from the frontend UI kit's TypeScript types and `live` fields.

---

## Table of Contents

1. [Connection & Authentication](#1-connection--authentication)
2. [MonitoringHub](#2-monitoringhub)
3. [AlarmHub](#3-alarmhub)
4. [NotificationHub](#4-notificationhub)
5. [Group/Channel Structure](#5-groupchannel-structure)
6. [Frequency Recommendations](#6-frequency-recommendations)
7. [Error Handling & Reconnection](#7-error-handling--reconnection)

---

## 1. Connection & Authentication

### Hub URLs

| Hub              | URL                              | Purpose                                     |
|------------------|----------------------------------|---------------------------------------------|
| MonitoringHub    | `wss://api.derms.example.com/hubs/monitoring`   | Device telemetry streams           |
| AlarmHub         | `wss://api.derms.example.com/hubs/alarms`       | Alarm lifecycle events             |
| NotificationHub  | `wss://api.derms.example.com/hubs/notifications` | System notification delivery      |

### Authentication

JWT token is passed as a query parameter during connection:

```
wss://api.derms.example.com/hubs/monitoring?access_token=<jwt_token>
```

For SignalR clients:

```typescript
const connection = new signalR.HubConnectionBuilder()
  .withUrl("/hubs/monitoring", {
    accessTokenFactory: () => getAccessToken(),
  })
  .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
  .build();
```

### Facility Subscription

After connecting, the client MUST join a facility group to receive data scoped to that facility. All messages are filtered by the joined facility.

#### Client → Server: `JoinFacility`

```typescript
// Join a facility's data stream
await connection.invoke("JoinFacility", facilityId: string);
```

#### Client → Server: `LeaveFacility`

```typescript
// Leave a facility's data stream (e.g. when switching facilities)
await connection.invoke("LeaveFacility", facilityId: string);
```

#### Server → Client: `FacilityJoined`

```typescript
// Confirmation
{
  facilityId: string;
  subscribedAt: string;    // ISO 8601
}
```

---

## 2. MonitoringHub

**URL:** `wss://api.derms.example.com/hubs/monitoring`

The MonitoringHub provides real-time telemetry for all monitored device types. Clients subscribe per device type and optionally per specific device ID.

### 2.1 Subscriptions (Client → Server)

#### `SubscribeBessUnits`

Subscribe to all BESS unit updates within the facility, or a specific unit.

```typescript
await connection.invoke("SubscribeBessUnits", deviceId?: string);
// deviceId is optional — omit to receive updates for all units
```

#### `UnsubscribeBessUnits`

```typescript
await connection.invoke("UnsubscribeBessUnits", deviceId?: string);
```

#### `SubscribeInverters`

Subscribe to all solar inverter updates within the facility, or a specific inverter.

```typescript
await connection.invoke("SubscribeInverters", deviceId?: string);
```

#### `UnsubscribeInverters`

```typescript
await connection.invoke("UnsubscribeInverters", deviceId?: string);
```

#### `SubscribeGridDevices`

Subscribe to all grid device updates within the facility, or a specific device.

```typescript
await connection.invoke("SubscribeGridDevices", deviceId?: string);
```

#### `UnsubscribeGridDevices`

```typescript
await connection.invoke("UnsubscribeGridDevices", deviceId?: string);
```

#### `SubscribeTurbines`

Subscribe to all wind turbine updates within the facility, or a specific turbine.

```typescript
await connection.invoke("SubscribeTurbines", deviceId?: string);
```

#### `UnsubscribeTurbines`

```typescript
await connection.invoke("UnsubscribeTurbines", deviceId?: string);
```

#### `SubscribeConsumers`

Subscribe to all consumer meter updates within the facility, or a specific consumer.

```typescript
await connection.invoke("SubscribeConsumers", deviceId?: string);
```

#### `UnsubscribeConsumers`

```typescript
await connection.invoke("UnsubscribeConsumers", deviceId?: string);
```

#### `SubscribePowerFlow`

Subscribe to facility-level power flow updates (dashboard).

```typescript
await connection.invoke("SubscribePowerFlow");
```

#### `UnsubscribePowerFlow`

```typescript
await connection.invoke("UnsubscribePowerFlow");
```

---

### 2.2 Server → Client Messages

#### `BessUnitUpdate`

**Frequency:** Every 5 seconds per unit

Provides live telemetry for a single BESS unit. Fields correspond to `BessUnit.live` and key operational data.

```typescript
interface BessUnitUpdate {
  unitId: string;
  timestamp: string;                // ISO 8601

  // Operating state
  status: BessUnitStatus;           // "online" | "offline" | "fault" | "standby"
  operatingMode: BessOperatingMode; // "charging" | "discharging" | "idle" | "standby" | "balancing"

  // Live telemetry (maps to BessUnit.live)
  soc: number;                      // % (0-100)
  soh: number;                      // % (0-100)
  power: number;                    // kW (positive=discharge, negative=charge)
  voltage: number;                  // V
  current: number;                  // A
  frequency: number;                // Hz
  tempAvg: number;                  // C
  tempMin: number;                  // C
  tempMax: number;                  // C

  // Lifecycle (daily counters)
  dailyChargedKWh: number;
  dailyDischargedKWh: number;

  // Alarm count
  activeAlarmCount: number;
}
```

**Example:**
```json
{
  "unitId": "bess-001",
  "timestamp": "2026-02-10T14:30:05Z",
  "status": "online",
  "operatingMode": "discharging",
  "soc": 68.5,
  "soh": 96.2,
  "power": 245.8,
  "voltage": 768.4,
  "current": 319.9,
  "frequency": 50.01,
  "tempAvg": 32.4,
  "tempMin": 28.1,
  "tempMax": 35.8,
  "dailyChargedKWh": 480.2,
  "dailyDischargedKWh": 312.5,
  "activeAlarmCount": 0
}
```

---

#### `InverterUpdate`

**Frequency:** Every 10 seconds per inverter

Provides live telemetry for a single solar inverter.

```typescript
interface InverterUpdate {
  inverterId: string;
  timestamp: string;

  // Status
  status: InverterStatus;           // "normal" | "offline" | "warning" | "fault"
  operatingStatus: string;

  // Power
  totalActivePower: number | null;  // kW
  totalDcPower: number | null;      // kW
  totalReactivePower: number | null;// var
  totalApparentPower: number | null;// VA
  totalPowerFactor: number | null;

  // Yield
  yieldToday: number | null;        // kWh
  totalYield: number | null;        // MWh

  // Grid
  gridFrequency: number | null;     // Hz
  abLineVoltage: number | null;     // V
  bcLineVoltage: number | null;     // V
  caLineVoltage: number | null;     // V

  // Phase currents
  phaseACurrent: number | null;     // A
  phaseBCurrent: number | null;
  phaseCCurrent: number | null;

  // MPPT
  mpptData: {
    id: number;
    voltage: number | null;
    current: number | null;
  }[];

  // String currents
  stringCurrents: (number | null)[];
}
```

**Example:**
```json
{
  "inverterId": "inv-001",
  "timestamp": "2026-02-10T14:30:10Z",
  "status": "normal",
  "operatingStatus": "On-grid Generating",
  "totalActivePower": 98.5,
  "totalDcPower": 102.3,
  "totalReactivePower": -2.1,
  "totalApparentPower": 98600,
  "totalPowerFactor": 0.998,
  "yieldToday": 542.8,
  "totalYield": 12450.2,
  "gridFrequency": 50.01,
  "abLineVoltage": 612,
  "bcLineVoltage": 610,
  "caLineVoltage": 614,
  "phaseACurrent": 93.2,
  "phaseBCurrent": 92.8,
  "phaseCCurrent": 93.5,
  "mpptData": [
    {"id": 1, "voltage": 720, "current": 24.5},
    {"id": 2, "voltage": 715, "current": 23.8}
  ],
  "stringCurrents": [11.2, 11.0, 10.8, 11.1, 10.9, 11.3]
}
```

---

#### `GridDeviceUpdate`

**Frequency:** Every 10 seconds per device

Provides live telemetry for a single grid device (transformer, breaker, meter, etc.).

```typescript
interface GridDeviceUpdate {
  deviceId: string;
  timestamp: string;

  // Status
  status: GridDeviceStatus;         // "normal" | "offline" | "warning" | "fault" | "tripped"

  // Power
  activePower: number | null;       // kW
  reactivePower: number | null;     // kVar
  apparentPower: number | null;     // kVA
  powerFactor: number | null;
  frequency: number | null;         // Hz
  powerFlowDirection: "import" | "export" | null;

  // 3-Phase
  voltagePhaseA: number | null;     // V
  voltagePhaseB: number | null;
  voltagePhaseC: number | null;
  currentPhaseA: number | null;     // A
  currentPhaseB: number | null;
  currentPhaseC: number | null;
  thd: number | null;               // %

  // Transformer-specific (null if not a transformer)
  oilTemperature: number | null;    // C
  windingTemperature: number | null;// C
  tapPosition: number | null;
  loadingPercent: number | null;    // %

  // Breaker-specific (null if not a breaker/relay)
  breakerPosition: "closed" | "open" | "tripped" | null;
  relayTripStatus: boolean | null;

  // Energy counters
  importedToday: number | null;     // kWh
  exportedToday: number | null;     // kWh
}
```

**Example:**
```json
{
  "deviceId": "grid-001",
  "timestamp": "2026-02-10T14:30:10Z",
  "status": "normal",
  "activePower": 2450.5,
  "reactivePower": 312.8,
  "apparentPower": 2470.4,
  "powerFactor": 0.992,
  "frequency": 50.01,
  "powerFlowDirection": "import",
  "voltagePhaseA": 5760,
  "voltagePhaseB": 5745,
  "voltagePhaseC": 5755,
  "currentPhaseA": 142.3,
  "currentPhaseB": 141.8,
  "currentPhaseC": 142.1,
  "thd": 2.8,
  "oilTemperature": 62.5,
  "windingTemperature": 78.3,
  "tapPosition": 5,
  "loadingPercent": 72.4,
  "breakerPosition": null,
  "relayTripStatus": null,
  "importedToday": 18520,
  "exportedToday": 0
}
```

---

#### `TurbineUpdate`

**Frequency:** Every 5 seconds per turbine

Provides live telemetry for a single wind turbine.

```typescript
interface TurbineUpdate {
  turbineId: string;
  timestamp: string;

  // Status
  status: TurbineStatus;            // "normal" | "offline" | "warning" | "fault"

  // Power
  activePower: number | null;       // kW
  reactivePower: number | null;     // kVar
  powerFactor: number | null;
  generatorPower: number | null;    // kW

  // Wind conditions
  windSpeed: number | null;         // m/s
  windDirection: number | null;     // degrees
  ambientTemp: number | null;       // C

  // Rotor & Generator
  rotorSpeed: number | null;        // rpm
  generatorSpeed: number | null;    // rpm

  // Pitch & Yaw
  pitchAngles: [number | null, number | null, number | null];
  nacellePosition: number | null;   // degrees
  yawError: number | null;          // degrees

  // Temperatures
  generatorTemp: number | null;     // C
  gearboxOilTemp: number | null;    // C
  mainBearingTemp: number | null;   // C
  controlCabinetTemp: number | null;// C

  // Grid
  gridFrequency: number | null;     // Hz
  gridVoltage: number | null;       // V

  // Yield
  yieldToday: number | null;        // MWh
  dailyOperatingTime: number | null;// hours
}
```

**Example:**
```json
{
  "turbineId": "wt-001",
  "timestamp": "2026-02-10T14:30:05Z",
  "status": "normal",
  "activePower": 2850,
  "reactivePower": -120,
  "powerFactor": 0.998,
  "generatorPower": 2880,
  "windSpeed": 12.4,
  "windDirection": 225,
  "ambientTemp": 8.5,
  "rotorSpeed": 14.2,
  "generatorSpeed": 1680,
  "pitchAngles": [3.2, 3.1, 3.3],
  "nacellePosition": 228,
  "yawError": 3.0,
  "generatorTemp": 85.2,
  "gearboxOilTemp": 62.4,
  "mainBearingTemp": 48.8,
  "controlCabinetTemp": 32.1,
  "gridFrequency": 50.01,
  "gridVoltage": 690,
  "yieldToday": 42.5,
  "dailyOperatingTime": 14.2
}
```

---

#### `ConsumerUpdate`

**Frequency:** Every 15 seconds per consumer

Provides live telemetry for a single consumer meter.

```typescript
interface ConsumerUpdate {
  consumerId: string;
  timestamp: string;

  // Status
  status: ConsumerStatus;           // "normal" | "offline" | "warning" | "fault" | "curtailed"

  // Power
  activePower: number | null;       // kW
  reactivePower: number | null;     // kVar
  powerFactor: number | null;

  // Demand
  peakDemandToday: number | null;   // kW
  loadFactor: number | null;        // %

  // Consumption
  consumptionToday: number | null;  // kWh
  estimatedCostToday: number | null;// $

  // Demand Response
  drStatus: DRStatus;               // "enrolled" | "active" | "standby" | "not-enrolled"
  currentCurtailment: number | null;// kW
}
```

**Example:**
```json
{
  "consumerId": "cons-001",
  "timestamp": "2026-02-10T14:30:15Z",
  "status": "normal",
  "activePower": 1250.5,
  "reactivePower": 185.2,
  "powerFactor": 0.989,
  "peakDemandToday": 1520,
  "loadFactor": 72.5,
  "consumptionToday": 12450,
  "estimatedCostToday": 1024.50,
  "drStatus": "enrolled",
  "currentCurtailment": 0
}
```

---

#### `PowerFlowUpdate`

**Frequency:** Every 5 seconds

Facility-level power flow snapshot for the dashboard power flow diagram.

```typescript
interface PowerFlowUpdate {
  timestamp: string;

  solarPower: number;        // kW (always >= 0)
  batteryPower: number;      // kW (positive=discharge, negative=charge)
  gridPower: number;         // kW (positive=import, negative=export)
  loadPower: number;         // kW (always >= 0)
  batterySoc: number;        // % (0-100)
}
```

**Example:**
```json
{
  "timestamp": "2026-02-10T14:30:05Z",
  "solarPower": 68.5,
  "batteryPower": 15.3,
  "gridPower": -12.4,
  "loadPower": 71.4,
  "batterySoc": 72
}
```

---

## 3. AlarmHub

**URL:** `wss://api.derms.example.com/hubs/alarms`

The AlarmHub provides event-driven alarm lifecycle messages. No explicit subscribe/unsubscribe is needed beyond joining a facility — all alarm events for the facility are pushed automatically.

### 3.1 Server → Client Messages

#### `AlarmCreated`

Fired when a new alarm is created. Delivers the full alarm object.

```typescript
interface AlarmCreatedMessage {
  alarm: {
    id: string;
    code: string;
    message: string;
    severity: AlarmSeverity;         // "critical" | "high" | "medium" | "low"
    status: "active";
    source: string;
    sourceType: AlarmSourceType;     // "solar" | "bess" | "grid" | "inverter" | "system"
    triggeredAt: string;             // ISO 8601
    value?: string;
    threshold?: string;
    relatedAsset?: {
      id: string;
      name: string;
      type: string;
    };
    location?: string;
  };
}
```

**Example:**
```json
{
  "alarm": {
    "id": "alm-2048",
    "code": "BESS-OT-001",
    "message": "Battery over-temperature detected on Rack 3",
    "severity": "high",
    "status": "active",
    "source": "BESS Unit 1 - Rack 3",
    "sourceType": "bess",
    "triggeredAt": "2026-02-10T14:32:15Z",
    "value": "42.8 C",
    "threshold": "40.0 C",
    "relatedAsset": {
      "id": "ast-003",
      "name": "BESS Unit 1",
      "type": "bess"
    },
    "location": "Storage Bay 1"
  }
}
```

---

#### `AlarmStatusChanged`

Fired when an alarm is acknowledged or resolved.

```typescript
interface AlarmStatusChangedMessage {
  alarmId: string;
  oldStatus: AlarmStatus;            // "active" | "acknowledged"
  newStatus: AlarmStatus;            // "acknowledged" | "resolved"
  changedBy: {
    id: string;
    name: string;
  };
  changedAt: string;                 // ISO 8601
  note?: string;
  resolution?: string;
}
```

**Example:**
```json
{
  "alarmId": "alm-2048",
  "oldStatus": "active",
  "newStatus": "acknowledged",
  "changedBy": {
    "id": "usr-001",
    "name": "Ahmet Yilmaz"
  },
  "changedAt": "2026-02-10T14:35:22Z",
  "note": "Investigating the issue"
}
```

---

#### `AlarmCountUpdated`

Fired whenever the alarm counts change (creation, acknowledgement, or resolution). Used by the TopBar alarm badge.

```typescript
interface AlarmCountUpdatedMessage {
  activeCount: number;
  criticalCount: number;
}
```

**Example:**
```json
{
  "activeCount": 12,
  "criticalCount": 2
}
```

---

## 4. NotificationHub

**URL:** `wss://api.derms.example.com/hubs/notifications`

The NotificationHub delivers system notifications in real-time. No explicit subscribe/unsubscribe is needed beyond joining a facility.

### 4.1 Server → Client Messages

#### `NotificationReceived`

Fired when a new notification is created for the user.

```typescript
interface NotificationReceivedMessage {
  notification: {
    id: string;
    type: NotificationType;
    title: string;
    message: string;
    timestamp: string;               // ISO 8601
    read: false;
    relatedAlarmId?: string;
    relatedTaskId?: string;
  };
}

type NotificationType =
  | "maintenance_completed"
  | "system_update"
  | "alarm_resolved"
  | "schedule_reminder"
  | "system_alert";
```

**Example:**
```json
{
  "notification": {
    "id": "notif-1024",
    "type": "alarm_resolved",
    "title": "Alarm Resolved",
    "message": "Battery over-temperature alarm ALM-2048 has been resolved",
    "timestamp": "2026-02-10T14:45:30Z",
    "read": false,
    "relatedAlarmId": "alm-2048"
  }
}
```

---

#### `UnreadCountUpdated`

Fired when the unread notification count changes. Used by the TopBar notification badge.

```typescript
interface UnreadCountUpdatedMessage {
  count: number;
}
```

**Example:**
```json
{
  "count": 5
}
```

---

## 5. Group/Channel Structure

Messages are scoped using a hierarchical group naming pattern. The server manages group membership internally based on client subscriptions.

### Group Naming Pattern

```
facility:{facilityId}:{domain}:{deviceId?}
```

### Group Examples

| Group Name                                | Description                          |
|-------------------------------------------|--------------------------------------|
| `facility:fac-001:bess`                   | All BESS units in facility fac-001   |
| `facility:fac-001:bess:bess-001`          | Specific BESS unit                   |
| `facility:fac-001:inverters`              | All solar inverters                  |
| `facility:fac-001:inverters:inv-003`      | Specific inverter                    |
| `facility:fac-001:grid`                   | All grid devices                     |
| `facility:fac-001:grid:grid-002`          | Specific grid device                 |
| `facility:fac-001:turbines`               | All wind turbines                    |
| `facility:fac-001:turbines:wt-005`        | Specific turbine                     |
| `facility:fac-001:consumers`              | All consumers                        |
| `facility:fac-001:consumers:cons-001`     | Specific consumer                    |
| `facility:fac-001:powerflow`              | Facility power flow                  |
| `facility:fac-001:alarms`                 | All alarms (auto-joined)             |
| `facility:fac-001:notifications`          | All notifications (auto-joined)      |

### Subscription Behavior

- When a client calls `SubscribeBessUnits()` without a device ID, they join `facility:{id}:bess` and receive all BESS updates.
- When a client calls `SubscribeBessUnits("bess-001")`, they join `facility:{id}:bess:bess-001` and only receive updates for that specific unit.
- Alarm and Notification groups are automatically joined when `JoinFacility` is called.

---

## 6. Frequency Recommendations

| Message               | Interval   | Rationale                                                  |
|-----------------------|------------|-------------------------------------------------------------|
| `BessUnitUpdate`      | 5 seconds  | Battery state changes rapidly during charge/discharge cycles |
| `InverterUpdate`      | 10 seconds | Solar output changes slowly; 10s is sufficient for monitoring |
| `GridDeviceUpdate`    | 10 seconds | Grid parameters are relatively stable; align with inverter rate |
| `TurbineUpdate`       | 5 seconds  | Wind conditions change rapidly; pitch/yaw need fast updates |
| `ConsumerUpdate`      | 15 seconds | Consumption patterns change slowly; lower frequency reduces load |
| `PowerFlowUpdate`     | 5 seconds  | Dashboard power flow diagram needs smooth real-time animation |
| `AlarmCreated`        | Event-driven | Immediate delivery on alarm trigger                      |
| `AlarmStatusChanged`  | Event-driven | Immediate delivery on status change                      |
| `AlarmCountUpdated`   | Event-driven | Immediate delivery; used for badge counts                |
| `NotificationReceived`| Event-driven | Immediate delivery                                       |
| `UnreadCountUpdated`  | Event-driven | Immediate delivery; used for badge counts                |

### Bandwidth Estimation (per facility)

Assuming a typical facility with 5 BESS units, 20 inverters, 10 grid devices, 8 turbines, and 10 consumers:

| Stream     | Devices | Interval | Msg Size (est.) | Messages/min | KB/min |
|------------|---------|----------|------------------|--------------|--------|
| BESS       | 5       | 5s       | ~350 bytes       | 60           | ~21    |
| Inverter   | 20      | 10s      | ~500 bytes       | 120          | ~60    |
| Grid       | 10      | 10s      | ~450 bytes       | 60           | ~27    |
| Turbine    | 8       | 5s       | ~400 bytes       | 96           | ~38    |
| Consumer   | 10      | 15s      | ~250 bytes       | 40           | ~10    |
| PowerFlow  | 1       | 5s       | ~100 bytes       | 12           | ~1.2   |
| **Total**  |         |          |                  | **388**      | **~157** |

---

## 7. Error Handling & Reconnection

### Connection States

```typescript
type ConnectionState = "connecting" | "connected" | "reconnecting" | "disconnected";
```

### Reconnection Strategy

The client should implement exponential backoff with jitter:

```typescript
const connection = new signalR.HubConnectionBuilder()
  .withUrl("/hubs/monitoring", {
    accessTokenFactory: () => getAccessToken(),
  })
  .withAutomaticReconnect({
    nextRetryDelayInMilliseconds: (retryContext) => {
      // Exponential backoff: 0, 2, 5, 10, 30 seconds
      const delays = [0, 2000, 5000, 10000, 30000];
      return delays[Math.min(retryContext.previousRetryCount, delays.length - 1)];
    },
  })
  .build();
```

### Re-subscription on Reconnect

After reconnection, the client MUST re-subscribe to all previously active subscriptions:

```typescript
connection.onreconnected(async () => {
  // Re-join facility
  await connection.invoke("JoinFacility", currentFacilityId);

  // Re-subscribe to active device streams
  for (const sub of activeSubscriptions) {
    await connection.invoke(sub.method, sub.deviceId);
  }
});
```

### Server-Side Errors

The server may send errors through the `HubError` message:

```typescript
interface HubError {
  code: string;              // "AUTH_EXPIRED", "FACILITY_NOT_FOUND", "RATE_LIMITED"
  message: string;
}
```

| Error Code            | Description                           | Client Action           |
|-----------------------|---------------------------------------|--------------------------|
| `AUTH_EXPIRED`        | JWT token has expired                | Refresh token and reconnect |
| `FACILITY_NOT_FOUND`  | Invalid facility ID                  | Prompt user to select facility |
| `RATE_LIMITED`        | Too many subscriptions               | Reduce subscription scope |
| `DEVICE_NOT_FOUND`    | Device ID does not exist             | Remove from subscription list |

### Heartbeat

SignalR handles keep-alive automatically with default settings:
- Server sends ping every 15 seconds
- Client timeout: 30 seconds (if no message from server)
- Server timeout: 30 seconds (if no message from client)
