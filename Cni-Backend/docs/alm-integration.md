# ALM (Algorithm Module) Integration

## 1. What ALM Is

ALM is a separate Python/FastAPI microservice running in Kubernetes that performs time-series forecasting via TensorFlow. It predicts future energy consumption and generation so the DERMS platform can optimize battery dispatch, tariff arbitrage, and grid operations.

### Environments

| Environment | ALM Base URL |
|---|---|
| Development | `https://alm-cnidev.oklidis.com.tr` |
| Staging | `https://alm-cnistg.oklidis.com.tr` |

### Available Modules

| Module | Endpoint | Input | Output |
|---|---|---|---|
| **module-tsf-pc** | Power Consumption forecast | 7 days of hourly SmartMeter data (load) | 24h hourly power consumption forecast (single float per step) |
| **module-tsf-pg** | Power Generation forecast | 1 day of PV Solar data (per-inverter) | 24h hourly power generation forecast (list of floats per step, one per PV) |

New modules can be added — see [Adding a New Module](#9-adding-a-new-module).

---

## 2. Architecture Overview

The integration is fully **async callback-based**:

```
┌─────────────┐     1. POST CSV+schema      ┌─────────────┐
│   Backend    │ ───────────────────────────► │  ALM Service │
│  (this API)  │                              │  (FastAPI)   │
│              │ ◄─────────────────────────── │              │
└─────────────┘     4. POST callback          └──────┬───────┘
       │              with results                    │
       │                                              │
       ▼                                              ▼
┌─────────────┐                              ┌──────────────┐
│  PostgreSQL  │                              │  TensorFlow  │
│  alm schema  │                              │   Worker     │
└─────────────┘                              └──────────────┘
```

**Flow:**

1. **Backend prepares data** — queries device telemetry, aggregates into CSV, builds a JSON schema describing the data
2. **Backend submits job** — `POST /{module}` with `{ schema, data, callback_url }`
3. **ALM queues job** — returns `{ job_id, job_status: "queued" }` immediately
4. **ALM worker processes** — runs the TensorFlow model asynchronously
5. **ALM posts callback** — `POST /api/v1/Alm/Callback` with `{ job_id, job_status, result: { forecast: [...] } }`
6. **Backend stores forecast** — updates job status, stores forecast points in PostgreSQL

---

## 3. Opt-in Model

ALM predictions are **opt-in per facility**. The `Facility` entity has an `AlmEnabled` boolean property (default `false`). Only facilities with `AlmEnabled = true` are processed by the background worker.

This can be toggled via the existing facility update endpoints. The flag has no effect on other facility behavior — it only controls whether the daily ALM worker includes the facility.

---

## 4. Data Preparation

Before submitting a job, the backend must prepare the input data that the ALM model expects.

### Power Consumption (module-tsf-pc)

**Source:** `DeviceSmartMeterData` table (devices with `DeviceType.Name = "Smart Meter"` or `LogicalDevice.Category = "Grid"`)

**Process:**
1. Find all SmartMeter devices for the facility (`Device → Rtu → FacilityId`)
2. Query last 7 days (configurable) of telemetry, ordered by `DeviceLocalTime`
3. Extract `activePower` from each record's JSON `Payload`
4. Aggregate into **hourly buckets** — sum across all meters in the same hour
5. Require at least 24 data points (hours); return null if insufficient

**CSV format:**
```csv
UTCTime,LoadTotalPowerW
2026-03-05 00:00:00,15234.50
2026-03-05 01:00:00,14890.00
...
```

**Schema:**
```json
{
  "uid": "<facilityId>",
  "timestamp": "UTCTime",
  "timestamp_format": "%Y-%m-%d %H:%M:%S",
  "latitude": 39.925,
  "longitude": 32.866,
  "globalpower": "LoadTotalPowerW"
}
```

### Power Generation (module-tsf-pg)

**Source:** `DeviceSolarData` table (devices with `DeviceType.Name = "Solar"` or `LogicalDevice.Category = "Solar"`)

**Process:**
1. Find all Solar devices for the facility
2. Query last 1 day (configurable) of telemetry
3. Extract `activePower` per device, aggregate into hourly buckets per device
4. Build multi-column CSV: one column per PV device
5. Require at least 4 data points; return null if insufficient

**CSV format:**
```csv
UTCTime,PV1W,PV2W,PV3W
2026-03-11 06:00:00,0.00,0.00,0.00
2026-03-11 07:00:00,1250.30,1180.50,1310.20
...
```

**Schema:**
```json
{
  "uid": "<facilityId>",
  "timestamp": "UTCTime",
  "timestamp_format": "%Y-%m-%d %H:%M:%S",
  "latitude": 39.925,
  "longitude": 32.866,
  "pvpower": ["PV1W", "PV2W", "PV3W"]
}
```

---

## 5. ALM API Communication

### Submit Job

**Request:** `POST https://alm-cnidev.oklidis.com.tr/{module}`

```json
{
  "schema": { ... },
  "data": "UTCTime,LoadTotalPowerW\n2026-03-05 00:00:00,15234.50\n...",
  "callback_url": "https://our-backend/api/v1/Alm/Callback"
}
```

**Response:**
```json
{
  "job_id": "abc-123-def",
  "job_status": "queued"
}
```

### Poll Job Status (optional, for debugging)

**Request:** `GET https://alm-cnidev.oklidis.com.tr/{module}/jobs/{job_id}`

**Response:**
```json
{
  "job_id": "abc-123-def",
  "job_status": "processing",
  "result": null
}
```

### Callback (ALM → Backend)

**Request:** `POST /api/v1/Alm/Callback` (with `X-API-Key` header)

```json
{
  "job_id": "abc-123-def",
  "job_status": "completed",
  "result": {
    "status_code": 200,
    "message": "Forecast generated successfully",
    "forecast": [
      { "timestamp": "2026-03-12T00:00:00Z", "value": 14500.0, "step": 1 },
      { "timestamp": "2026-03-12T01:00:00Z", "value": 13800.0, "step": 2 },
      ...
    ]
  }
}
```

- For **PC** jobs, `value` is a single float (total power in watts)
- For **PG** jobs, `value` is a JSON array of floats (one per PV device, matching schema column order)
- The `value` field is stored as a raw JSON string in the database to handle both formats

---

## 6. Database Schema

All ALM tables live in the `alm` PostgreSQL schema.

### alm.PredictionJobs

Tracks every job submitted to ALM. Extends `BaseAuditableEntity` (has `Id: Guid`, audit timestamps).

| Column | Type | Description |
|---|---|---|
| `Id` | `uuid` (PK) | Job ID in our system |
| `FacilityId` | `uuid` (FK → Facilities) | Which facility this prediction is for |
| `JobType` | `int` | Enum: 0=PowerConsumption, 1=PowerGeneration |
| `Status` | `int` | Enum: 0=Queued, 1=Processing, 2=Completed, 3=Failed, 4=Timeout |
| `ExternalJobId` | `varchar(200)` | ALM's job_id (used to match callbacks) |
| `ResultStatusCode` | `int?` | HTTP-style status from ALM result |
| `ResultMessage` | `varchar(1000)` | Human-readable result/error message |
| `ErrorDetails` | `varchar(4000)` | Detailed error info on failure |
| `DataPointCount` | `int` | Number of data points sent to ALM |
| `SubmittedAtUtc` | `timestamptz?` | When we submitted to ALM |
| `CompletedAtUtc` | `timestamptz?` | When ALM called back |
| `Created` / `LastModified` | `timestamptz` | Audit fields (auto-set) |

**Indexes:**
- `(FacilityId, JobType, Status)` — for querying latest prediction per facility
- `ExternalJobId` — for callback lookup

### alm.ForecastPoints

Stores individual forecast data points. Lightweight entity with `long Id` (auto-increment, not Guid) for bulk insert performance.

| Column | Type | Description |
|---|---|---|
| `Id` | `bigint` (PK, identity) | Auto-generated |
| `AlmPredictionJobId` | `uuid` (FK → PredictionJobs) | Parent job |
| `Timestamp` | `timestamptz` | Forecasted point in time |
| `Step` | `int` | Sequence number (1, 2, 3, ...) |
| `Value` | `text` | Raw JSON — single float or array depending on module |

**Index:** `(AlmPredictionJobId, Timestamp)`

**Cascade delete:** Deleting a PredictionJob deletes all its ForecastPoints.

### Tenant Scoping

ALM entities are tenant-scoped through the facility chain:
- `AlmPredictionJob → Facility → Firm → TenantId`
- `AlmForecastPoint → AlmPredictionJob → Facility → Firm → TenantId`

EF Core global query filters enforce this automatically. The callback handler uses `IgnoreQueryFilters()` since callbacks arrive via API key (no tenant context).

---

## 7. Job Lifecycle

```
                SubmitJobAsync()
                     │
                     ▼
┌──────────┐    ┌────────────┐    ALM callback     ┌────────────┐
│  Queued   │───►│ Processing │──────────────────►  │ Completed  │
│  (0)      │    │  (1)       │                     │  (2)       │
└──────────┘    └────────────┘                     └────────────┘
                     │
                     │  callback with error
                     ▼
                ┌──────────┐
                │  Failed  │
                │  (3)     │
                └──────────┘

                ┌──────────┐
                │ Timeout  │  (for future use —
                │  (4)     │   not yet auto-set)
                └──────────┘
```

1. **Queued** — job record created in DB, before ALM submission
2. **Processing** — ALM accepted the job, `ExternalJobId` is set
3. **Completed** — callback received with `job_status: "completed"`, forecast points stored
4. **Failed** — callback received with non-completed status, `ErrorDetails` populated
5. **Timeout** — reserved for future timeout detection (not auto-set currently)

---

## 8. API Endpoints

### Callback (API Key Auth)

| Method | Path | Auth | Description |
|---|---|---|---|
| `POST` | `/api/v1/Alm/Callback` | `X-API-Key` header (`AlmSettings:ApiKey`) | Receives ALM results |

**Note:** Uses `AlmApiKeyEndpointFilter` — separate key from the device ingest API key (`IngestSettings:ApiKey`).

### Query & Trigger (JWT Auth)

All require `RequireAuthorization()`. Query handlers enforce `[Authorize(Roles = "Administrator,Tenant.Admin")]` + `[RequirePermission(Permissions.AnalyticsView)]`.

| Method | Path | Auth | Description |
|---|---|---|---|
| `GET` | `/api/v1/Alm/Predictions/{facilityId}` | JWT | List prediction jobs for a facility |
| `GET` | `/api/v1/Alm/Predictions/{facilityId}/Latest` | JWT | Get latest completed prediction with forecast points |
| `POST` | `/api/v1/Alm/Predictions/{facilityId}/Trigger` | JWT (Admin only) | Manually trigger a prediction |

#### GET /Predictions/{facilityId}

Returns a list of prediction job summaries (without forecast points for efficiency).

**Query parameters:**
- `jobType` (optional) — filter by `AlmJobType` enum value (0=PowerConsumption, 1=PowerGeneration)
- `status` (optional) — filter by `AlmJobStatus` enum value
- `limit` (optional, default 20) — max results

**Response:**
```json
[
  {
    "id": "guid",
    "facilityId": "guid",
    "jobType": 0,
    "status": 2,
    "externalJobId": "abc-123",
    "resultStatusCode": 200,
    "resultMessage": "Forecast generated successfully",
    "dataPointCount": 168,
    "forecastPointCount": 24,
    "submittedAtUtc": "2026-03-12T00:01:00Z",
    "completedAtUtc": "2026-03-12T00:03:45Z",
    "created": "2026-03-12T00:01:00Z"
  }
]
```

#### GET /Predictions/{facilityId}/Latest

Returns the most recent **completed** prediction for a facility, **including all forecast points**.

**Query parameters:**
- `jobType` (required) — which module's forecast to retrieve

**Response (200):**
```json
{
  "id": "guid",
  "facilityId": "guid",
  "jobType": 0,
  "status": 2,
  "externalJobId": "abc-123",
  "dataPointCount": 168,
  "submittedAtUtc": "2026-03-12T00:01:00Z",
  "completedAtUtc": "2026-03-12T00:03:45Z",
  "forecastPoints": [
    { "timestamp": "2026-03-13T00:00:00Z", "step": 1, "value": "14500.0" },
    { "timestamp": "2026-03-13T01:00:00Z", "step": 2, "value": "13800.0" },
    ...
  ]
}
```

Returns **404** if no completed prediction exists.

#### POST /Predictions/{facilityId}/Trigger

Manually triggers a prediction job. Administrator only — does not require `AlmEnabled` flag.

**Query parameters:**
- `jobType` (required) — which module to run

**Response:**
```json
{
  "success": true,
  "jobId": "guid",
  "externalJobId": "abc-123",
  "message": "Job submitted successfully"
}
```

---

## 9. Background Worker

`AlmPredictionWorker` is a Hangfire recurring job registered as `alm-daily-predictions`.

- **Schedule:** Configurable via `AlmSettings.CronSchedule` (default: `"1 0 * * *"` = 00:01 UTC daily)
- **Scope:** Only facilities with `AlmEnabled = true` (queries with `IgnoreQueryFilters` for cross-tenant)
- **Behavior:** For each enabled facility, submits a job for **every** `AlmJobType` enum value
- **Error handling:** Errors are collected per-job; all jobs are attempted even if some fail. If any failed, throws `AggregateException` which triggers Hangfire retry (3 attempts: 2min, 5min, 10min delays)
- **Gating:** Only registered when `AlmSettings.Enabled = true` in config

---

## 10. Configuration

```json
{
  "AlmSettings": {
    "Enabled": true,
    "BaseUrl": "https://alm-cnidev.oklidis.com.tr",
    "CallbackBaseUrl": "https://our-backend-url",
    "ApiKey": "shared-secret-for-callback-auth",
    "ModuleDataDays": {
      "PowerConsumption": 7,
      "PowerGeneration": 1
    },
    "JobTimeoutMinutes": 30,
    "CronSchedule": "1 0 * * *"
  }
}
```

| Setting | Description |
|---|---|
| `Enabled` | Master switch — disables API client registration, Hangfire job, and DI wiring |
| `BaseUrl` | ALM service base URL (environment-specific) |
| `CallbackBaseUrl` | Our backend's public URL that ALM will call back to |
| `ApiKey` | Shared secret for `X-API-Key` header on the callback endpoint |
| `ModuleDataDays` | Map of `AlmJobType` name → number of historical days to prepare. Falls back to enum defaults if not specified |
| `JobTimeoutMinutes` | Reserved for future timeout detection |
| `CronSchedule` | Hangfire cron expression for the daily worker |

---

## 11. Metrics

Two Prometheus histograms track ALM performance:

| Metric | Labels | Where Recorded |
|---|---|---|
| `cni_alm_request_duration_seconds` | `endpoint` (module name), `status` (success/error) | `AlmJobSubmitter` — measures data prep + API submission time |
| `cni_alm_callback_duration_seconds` | `job_type` (callback's job_status string) | `Alm` endpoint — measures callback processing time |

Both use exponential buckets from 10ms to ~40s.

---

## 12. Adding a New Module

To add a new ALM module (e.g. Wind Power forecast), you need changes in exactly **2 files** (+1 optional):

### Step 1: `src/Domain/Enums/AlmJobType.cs`

Add the enum value and its module/data-days mapping:

```csharp
public enum AlmJobType
{
    PowerConsumption = 0,
    PowerGeneration = 1,
    WindPower = 2          // ← add
}

// In AlmJobTypeExtensions:
public static string GetModuleName(...) => jobType switch
{
    ...
    AlmJobType.WindPower => "module-tsf-wp",  // ← add
};

public static int GetDefaultDataDays(...) => jobType switch
{
    ...
    AlmJobType.WindPower => 3,                // ← add
};
```

### Step 2: `src/Application/Common/Services/AlmDataPreparationService.cs`

Add a dispatch case and implement the data preparation method:

```csharp
// In PrepareDataAsync:
AlmJobType.WindPower => PrepareWindPowerDataAsync(facilityId, dataDays, ct),

// New method:
public async Task<AlmPreparedData?> PrepareWindPowerDataAsync(...)
{
    // Query wind device telemetry, build CSV, build schema
}
```

### Step 3 (optional): `appsettings.json`

Override the default data days if needed:

```json
"ModuleDataDays": {
  "PowerConsumption": 7,
  "PowerGeneration": 1,
  "WindPower": 3
}
```

**Everything else auto-adapts:** the worker iterates all enum values, the submitter reads module name from the enum, the API client posts to any module string, and metrics label with the module name.

---

## 13. Code Map

```
src/
├── Domain/
│   ├── Enums/
│   │   ├── AlmJobType.cs              # Job types + module name mapping
│   │   └── AlmJobStatus.cs            # Queued/Processing/Completed/Failed/Timeout
│   └── Entities/
│       ├── Facility.cs                # AlmEnabled property
│       └── Alm/
│           ├── AlmPredictionJob.cs     # Job tracking (BaseAuditableEntity)
│           └── AlmForecastPoint.cs     # Forecast data points (long Id)
│
├── Application/
│   ├── Common/
│   │   ├── Interfaces/
│   │   │   ├── IAlmApiClient.cs        # SubmitJobAsync(module, request)
│   │   │   └── IAlmJobSubmitter.cs     # SubmitJobAsync(facilityId, jobType)
│   │   ├── Models/Alm/
│   │   │   └── AlmApiModels.cs         # Request/response DTOs, schemas
│   │   └── Services/
│   │       └── AlmDataPreparationService.cs  # CSV+schema preparation
│   └── Alm/
│       ├── Commands/
│       │   ├── ProcessAlmCallback/     # Callback handler (API key auth, no [Authorize])
│       │   └── TriggerPrediction/      # Manual trigger (Admin only)
│       └── Queries/
│           ├── GetLatestPrediction/    # Latest completed + forecast points
│           └── GetPredictions/         # List with filters
│
├── Infrastructure/
│   ├── Alm/
│   │   ├── AlmApiClient.cs            # Typed HttpClient → ALM service
│   │   └── AlmJobSubmitter.cs         # Orchestrates: prep → create job → submit → update
│   ├── BackgroundJobs/
│   │   └── AlmPredictionWorker.cs     # Hangfire recurring job
│   ├── Configurations/
│   │   └── AlmSettings.cs             # Settings POCO with ModuleDataDays dictionary
│   └── Data/Configurations/Alm/
│       ├── AlmPredictionJobConfiguration.cs   # EF config, alm schema
│       └── AlmForecastPointConfiguration.cs   # EF config, identity column
│
└── Web/
    ├── Endpoints/
    │   └── Alm.cs                     # Callback + query + trigger endpoints
    └── Infrastructure/
        └── AlmApiKeyEndpointFilter.cs # API key auth for callback
```

---

## 14. Sequence Diagram — Daily Worker

```
┌──────────┐  ┌──────────────┐  ┌──────────────┐  ┌───────────┐  ┌─────────┐
│ Hangfire  │  │AlmPrediction │  │AlmJobSubmitter│  │  DataPrep  │  │ALM API  │
│ Scheduler │  │   Worker     │  │              │  │  Service   │  │         │
└─────┬─────┘  └──────┬───────┘  └──────┬───────┘  └─────┬──────┘  └────┬────┘
      │               │                │                 │               │
      │──RunPredictions()──►           │                 │               │
      │               │                │                 │               │
      │               │─ query AlmEnabled facilities ──► │               │
      │               │◄── [facility1, facility2] ──────│               │
      │               │                │                 │               │
      │               │  for each facility × jobType:   │               │
      │               │──SubmitJobAsync()──►             │               │
      │               │                │                 │               │
      │               │                │──PrepareDataAsync()──►          │
      │               │                │◄── (schema, csv, count) ──────│
      │               │                │                 │               │
      │               │                │── INSERT AlmPredictionJob ──►  │
      │               │                │                 │               │
      │               │                │── POST /{module} ──────────────►
      │               │                │◄── { job_id, "queued" } ──────│
      │               │                │                 │               │
      │               │                │── UPDATE ExternalJobId ──►     │
      │               │◄── AlmSubmissionResult ────────│                │
      │               │                │                 │               │
      │◄── done ──────│                │                 │               │
```

## 15. Sequence Diagram — Callback

```
┌─────────┐  ┌──────────┐  ┌──────────────────┐  ┌───────────┐
│ALM      │  │ Alm      │  │ProcessAlmCallback │  │PostgreSQL │
│Service  │  │ Endpoint │  │   Handler         │  │           │
└────┬────┘  └────┬─────┘  └────────┬──────────┘  └─────┬─────┘
     │            │                 │                    │
     │── POST /api/v1/Alm/Callback (X-API-Key) ──►     │
     │            │                 │                    │
     │            │── validate API key                  │
     │            │── map payload to command             │
     │            │── Send(ProcessAlmCallbackCommand) ──►│
     │            │                 │                    │
     │            │                 │── SELECT job       │
     │            │                 │   WHERE ExternalJobId = ? ──►
     │            │                 │◄── job ────────────│
     │            │                 │                    │
     │            │                 │── UPDATE status,   │
     │            │                 │   CompletedAtUtc   │
     │            │                 │                    │
     │            │                 │── INSERT forecast  │
     │            │                 │   points (bulk) ───►
     │            │                 │                    │
     │            │                 │── SaveChanges() ──►│
     │            │◄── { success: true } ───────────────│
     │◄── 200 OK ─│                 │                    │
```
