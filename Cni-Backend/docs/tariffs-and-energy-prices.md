# Tariffs & Energy Prices

## Core Concepts

### Tenant
A **Tenant** is a logical grouping of firms that are related to each other -- analogous to a holding company that owns multiple firms. This relationship is organizational; the system uses it for data isolation and access control.

### Firm
A **Firm** is a company that owns one or more facilities. The firm is also currently assumed to be responsible for maintenance operations. The system is **firm-centric**: access control, pricing strategy, monitoring, and visualization are all organized around firms.

### Facility
A **Facility** is a physical site where devices (inverters, batteries/BESS, smart meters, etc.) are installed to produce or consume energy. The energy produced may be used on-site or sold to distributors. Energy management at the facility level can be highly demand-oriented.

A facility may have its own energy contract with a distributor, broker, or energy aggregator -- potentially at a special discount rate. This is why pricing is split into two levels: **firm-level** (which market operator to use) and **facility-level** (which tariff and contract terms apply).

### Relationship Hierarchy

```
Tenant
  └── Firm
       ├── FirmMarketOperator ──► MarketOperator
       └── Facility
            └── FacilityTariff ──► Tariff

MarketOperator ──► DayAheadNetEnergyPriceHistory
```

---

## How Energy Prices Work

### Overview

The system focuses on **day-ahead** electricity market prices as a cost-effective approach. There are two levels of market pricing:

| Level | Description | Cost |
|-------|-------------|------|
| **Day-Ahead** | Estimated prices published one day before delivery. Used as the primary data source. | Free / cheap |
| **Intraday** | Real-time prices reflecting actual supply-demand conditions for each hour. | Expensive |

Since intraday data is expensive, the system currently uses only day-ahead prices and derives gross price estimates by applying distribution fees, transmission fees, and taxes.

### Market Operators (Price Sources)

| Operator | Region | Resolution | API Format |
|----------|--------|------------|------------|
| **ENTSO-E** (European Network of Transmission System Operators) | 40+ European countries | 15 min or 60 min | XML |
| **EPIAS** (Turkish Energy Exchange / EPİAŞ) | Turkey | 60 min | JSON |

- For European markets, day-ahead prices come from **ENTSO-E Transparency Platform**.
- For Turkey, day-ahead prices come from **EPIAS**.

Each market operator is stored in the `MarketOperators` table with its API credentials, base URL, supported time resolutions, and available area/bidding zone codes.

### Net Price vs. Gross Price

Prices fetched from ENTSO-E and EPIAS are **net prices** (EUR/MWh or TRY/MWh) -- the raw wholesale electricity cost. The system stores only net prices in the database and calculates gross prices **at query time** using the facility's tariff.

### Gross Price Formula

```
GrossPrice = (NetPrice + DistributionFee + TransmissionFee) × (1 + TaxRate)
```

If the facility has a contract discount:

```
DiscountedGrossPrice = GrossPrice × (1 - DiscountRate)
```

**Example (Netherlands, Liander distributor):**
- NetPrice: 50 EUR/MWh (from ENTSO-E)
- DistributionFee: 25 EUR/MWh
- TransmissionFee: 5 EUR/MWh
- TaxRate: 0.21 (21% VAT + electricity tax)

```
GrossPrice = (50 + 25 + 5) × (1 + 0.21) = 80 × 1.21 = 96.80 EUR/MWh
```

With a 10% facility contract discount:

```
DiscountedGrossPrice = 96.80 × (1 - 0.10) = 96.80 × 0.90 = 87.12 EUR/MWh
```

---

## Database Organization

### `Tariffs`
Stores distributor tariff configurations per country.

| Column | Type | Description |
|--------|------|-------------|
| CountryName | string(100) | Country name (e.g., "Netherlands", "Türkiye") |
| DistributorName | string(200) | Distributor company name (e.g., "Liander", "TEDAŞ") |
| DistributionFee | decimal(18,4) | Local grid distribution fee per MWh |
| TransmissionFee | decimal(18,4) | High-voltage transmission fee per MWh |
| TaxRate | decimal(5,4) | Tax rate as decimal (e.g., 0.21 = 21%) |
| Currency | string(10) | Currency code ("EUR", "TRY") |
| IsActive | bool | Soft-delete flag; only one active tariff per country |

**Design note:** This is an append-only table. Updates create a new tariff record and deactivate the old one, preserving full history. Facilities referencing the old tariff are automatically migrated to the new version.

### `MarketOperators`
Stores the price data sources (ENTSO-E, EPIAS) including API credentials.

| Column | Type | Description |
|--------|------|-------------|
| Name | string(200) | Display name (e.g., "ENTSO-E Transparency Platform") |
| Code | string(50) | Unique identifier (e.g., "ENTSOE", "EPIAS") |
| BaseUrl | string(500) | API base URL |
| ApiKey | string(500) | API key or credentials (EPIAS stores `username:password`) |
| SupportedResolutions | List\<int\> (JSONB) | Supported time intervals in minutes (e.g., [15, 60]) |
| AreaCodes | List\<string\> (JSONB) | Available bidding zone codes |
| PriceUnit | string(20) | Unit of fetched prices: `MWh` (default), `kWh`, or `GWh`. Used for conversion when publishing to RTUs. |
| IsActive | bool | Whether this operator is active |

> **Note:** This table currently stores raw credentials. This should be revisited in the future to use encrypted storage or a secrets manager.

### `FirmMarketOperators`
Junction table linking each firm to its market operator with firm-specific configuration.

| Column | Type | Description |
|--------|------|-------------|
| FirmId | int | The firm |
| MarketOperatorId | int | The market operator providing prices |
| AreaCode | string(50) | The firm's bidding zone code (e.g., "10YNL----------L" for Netherlands) |
| SamplingPeriodMinutes | int | How often samples are collected (15 or 60 min). Must be in the MO's SupportedResolutions. Default: 60 |
| IsActive | bool | Whether this configuration is active |

Each firm is linked to exactly one market operator. The area code determines which country/bidding zone prices are fetched for.

### `FacilityTariffs`
Junction table linking facilities to tariffs, supporting contract details and discount rates.

| Column | Type | Description |
|--------|------|-------------|
| FacilityId | int | The facility |
| TariffId | int | The assigned tariff |
| IsActive | bool | Only one active assignment per facility |
| ContractNumber | string(100) | Optional contract reference number |
| DiscountRate | decimal(5,4) | Contract discount (e.g., 0.10 = 10%). Default: 0 |
| ContractStartDate | DateTimeOffset | Contract start date |
| ContractEndDate | DateTimeOffset | Contract end date |
| Notes | string(1000) | Additional contract notes |

This is where the firm-level vs. facility-level distinction matters: the **firm** determines *which market operator* provides net prices, but the **facility** determines *which tariff and contract terms* apply to calculate the gross price a specific facility actually pays or receives.

### `DayAheadNetEnergyPriceHistories`
Stores the historical net prices fetched from market operators.

| Column | Type | Description |
|--------|------|-------------|
| MarketOperatorId | int | Source market operator |
| Country | string(100) | Country name (derived from area code) |
| NetPrice | decimal(18,6) | Net price per MWh before taxes and fees |
| TimestampUtc | DateTimeOffset | UTC timestamp of the price data point |

A unique index on `(MarketOperatorId, Country, TimestampUtc)` prevents duplicate entries and enables safe re-runs of the fetch jobs.

---

## Hangfire Background Jobs

### Why Background Jobs?

Day-ahead prices are published once per day by market operators. Rather than fetching them on-demand (which would add latency and hit API rate limits), the system pre-fetches prices on a schedule and stores them in the database. When users query day-ahead prices, the data is served instantly from the local database.

### Job Schedule

Two recurring Hangfire jobs run daily to fetch day-ahead prices for all active firm configurations:

| Job ID | Cron (UTC) | Local Time (CET) | Purpose |
|--------|-----------|-------------------|---------|
| `energy-price-early-morning` | `1 0 * * *` (00:01 UTC) | 01:01 CET | Early fetch to have data ready for start of business |
| `energy-price-afternoon` | `1 13 * * *` (13:01 UTC) | 14:01 CET | Catch updates after ENTSO-E publishes (~13:00 CET) |

**Retry policy:** 3 attempts with delays of 2 min, 5 min, and 10 min between retries.

### What the Jobs Do

Both jobs invoke `EnergyPriceFetchWorker.FetchDailyPrices()`, which:

1. Queries all active `FirmMarketOperator` records (with their `MarketOperator` data).
2. Groups by unique `(MarketOperatorId, AreaCode)` combinations to avoid redundant API calls (multiple firms in the same country share the same price data).
3. For each unique combination:
   - Converts the area code to a country name.
   - Calls the appropriate market operator API (ENTSO-E or EPIAS) via `IEnergyPriceService`.
   - Checks the database for already-stored timestamps (deduplication).
   - Inserts only new price data points into `DayAheadNetEnergyPriceHistories`.
4. If any source fails, throws an `AggregateException` so Hangfire retries the job.

Because of the deduplication logic, running the job multiple times for the same day is safe -- it will simply skip already-stored records.

### Hangfire Dashboard

The Hangfire dashboard is available at `/hangfire/dashboard` for monitoring job status, history, and retries. Hangfire is disabled in the test environment.

---

## API Endpoints

### Energy Prices

**Base route:** `/api/EnergyPrices`

#### `GET /api/EnergyPrices/Day-Ahead`
Returns day-ahead prices **from the database** (pre-fetched by Hangfire jobs).

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| facilityId | int | Yes | Facility to get prices for |
| startDate | DateTimeOffset | Yes | Start date (inclusive) |
| endDate | DateTimeOffset | No | End date (inclusive). If omitted, returns only startDate's data. |

**Authorization:** Administrator or Tenant.Admin role + `FinancialsView` permission.

**How it works:**
1. Looks up the facility's firm and its market operator configuration.
2. Finds the facility's active tariff (distribution fee, transmission fee, tax rate, discount rate).
3. Queries stored net prices from `DayAheadNetEnergyPriceHistories`.
4. Calculates gross price (and discounted gross price if applicable) for each data point.

**Returns:** Facility info, tariff details, and a list of price points with `TimestampUtc`, `NetPrice`, `GrossPrice`, and optionally `DiscountedGrossPrice`.

---

#### `GET /api/EnergyPrices/Day-Ahead/Live`
Fetches day-ahead prices **directly from the market operator API** in real time. Useful for getting next-day prices that may not yet be stored.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| firmId | int | Yes | Firm to get prices for |
| startDate | DateTimeOffset | No | Defaults to today or tomorrow based on MO publication schedule |
| endDate | DateTimeOffset | No | Defaults to startDate + 1 day |

**Authorization:** Administrator or Tenant.Admin role + `FinancialsView` permission.

**Smart date defaulting:** If no date is specified:
- **ENTSO-E:** If it's after 13:00 CET, fetches tomorrow's prices (since they've been published). Otherwise, fetches today's.
- **EPIAS:** If it's after 14:00 Turkey time (UTC+3), fetches tomorrow's prices. Otherwise, fetches today's.

**Returns:** Price points with both UTC and local timestamps, statistics (min/max/average net and gross prices, peak hours), resolution in minutes, and the timezone used.

---

### Tariffs (Administrator only)

**Base route:** `/api/EnergyPrices/Tariffs`

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/Tariffs` | List tariffs (paginated, filterable by country and active status) |
| GET | `/Tariffs/{id}` | Get tariff by ID |
| GET | `/Tariffs/Country/{countryName}` | Get active tariff for a country (Admin or Tenant.Admin) |
| POST | `/Tariffs` | Create tariff (deactivates existing active tariff for the same country) |
| PUT | `/Tariffs/{id}` | Update tariff (creates new version, migrates facilities, deactivates old) |
| DELETE | `/Tariffs/{id}` | Soft-delete (deactivate) a tariff |

---

### Facility Tariffs (Administrator only)

**Base route:** `/api/EnergyPrices/FacilityTariffs`

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/FacilityTariffs` | List facility-tariff assignments (paginated, filterable) |
| GET | `/FacilityTariffs/{id}` | Get assignment by ID |
| GET | `/FacilityTariffs/Facility/{facilityId}` | Get active tariff for a facility |
| POST | `/FacilityTariffs` | Assign tariff to facility (with optional contract details and discount) |
| PUT | `/FacilityTariffs/{id}` | Change facility's tariff assignment |
| DELETE | `/FacilityTariffs/{id}` | Deactivate a facility-tariff assignment |

---

### Market Operators

**Base route:** `/api/MarketOperators`

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/MarketOperators` | List market operators (any authenticated user) |
| GET | `/MarketOperators/{id}` | Get market operator by ID |
| POST | `/MarketOperators` | Create market operator (Administrator only) |
| PUT | `/MarketOperators/{id}` | Update market operator (Administrator only) |
| DELETE | `/MarketOperators/{id}` | Delete market operator (Administrator only) |

---

### Firm Market Operators

**Base route:** `/api/FirmMarketOperators`

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/Firms/{firmId}/MarketOperator` | Get market operator config for a firm |
| POST | `/Firms/{firmId}/MarketOperator` | Assign market operator to firm (Admin or Tenant.Admin) |
| PUT | `/Firms/{firmId}/MarketOperator/{id}` | Update firm's market operator config (Admin or Tenant.Admin) |
| DELETE | `/Firms/{firmId}/MarketOperator/{id}` | Remove market operator from firm (Admin or Tenant.Admin) |

---

## Data Flow Summary

### Price Collection & RTU Dispatch (Automated)

```
Hangfire (00:01 & 13:01 UTC)
  → EnergyPriceFetchWorker.FetchDailyPrices()
    → For each unique (MarketOperator, AreaCode):
        → ENTSO-E API (XML) or EPIAS API (JSON)
        → Store net prices in DayAheadNetEnergyPriceHistories
          (deduplicated by MarketOperatorId + Country + TimestampUtc)
        → PublishPricesToRtusAsync():
            → Query: FirmMarketOperator → Firm → Facility → RTU
            → Build payload (MWh → kWh conversion via MarketOperator.PriceUnit)
            → MqttPublisherService.PublishToManyAsync()
            → Topic: cni/{facilityId}/{rtuId}/command (QoS 1)
```

**Error handling:** If the API fetch or DB save fails, Hangfire retries the entire job. If MQTT publishing fails, it is logged but does **not** trigger a retry (prices are already stored). Individual RTU publish failures do not affect other RTUs.

### Price Query from Database

```
GET /api/EnergyPrices/Day-Ahead?facilityId=1&startDate=2026-02-09
  → Facility → Firm → FirmMarketOperator → AreaCode → Country
  → Facility → FacilityTariff → Tariff (fees, tax, discount)
  → Query DayAheadNetEnergyPriceHistories for date range
  → Calculate: GrossPrice = (Net + DistFee + TransFee) × (1 + TaxRate)
  → If discount: DiscountedGross = Gross × (1 - DiscountRate)
```

### Live Price Query

```
GET /api/EnergyPrices/Day-Ahead/Live?firmId=1
  → Firm → FirmMarketOperator → MarketOperator
  → Determine target date (today/tomorrow based on publication schedule)
  → Call market operator API directly
  → Apply tariff formula to calculate gross prices
  → Compute statistics (min, max, avg, peak hours)
```

---

## Seeded Data

The system seeds the following default data for development:

**Market Operators:**
- ENTSO-E: Europe (NL, DE, FR, BE, AT area codes), 15/60 min resolution
- EPIAS: Turkey (TR), 60 min resolution

**Tariffs:**

| Country | Distributor | Dist. Fee | Trans. Fee | Tax Rate | Currency |
|---------|-------------|-----------|------------|----------|----------|
| Netherlands | Liander | 25 | 5 | 21% | EUR |
| Germany | E.ON | 30 | 6 | 19% | EUR |
| Türkiye | TEDAŞ | 150 | 30 | 20% | TRY |
