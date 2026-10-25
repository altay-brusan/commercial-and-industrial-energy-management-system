# Mock Data

## Overview

All mock data lives in `lib/` subdirectories alongside their type definitions. Mock data is used as development fallback when the backend API is unavailable, and for UI prototyping.

## Mock Data Files

| File | Exports | Used By |
|------|---------|---------|
| `lib/auth/mock-data.ts` | `features`, `permissions`, `roles` + helper functions | Configuration pages, permission matrix |
| `lib/auth/user-mock-data.ts` | `userAccounts` + helper functions | Users page |
| `lib/maintenance/mock-data.ts` | `users`, `assets`, `maintenanceTasks` | Maintenance page, role user sections |
| `lib/alarms/mock-data.ts` | `alarms`, `notifications`, `getActiveAlarmCount()` | Alarms page, TopBar badges |
| `lib/bess/mock-data.ts` | Battery storage hierarchy, units, cells | BESS monitoring |
| `lib/pcs/mock-data.ts` | PCS units and data | PCS monitoring |
| `lib/solar-inverters/mock-data.ts` | Inverter list and details | Solar inverter pages |
| `lib/wind-turbines/mock-data.ts` | Wind turbine data | Wind turbine monitoring |
| `lib/consumption/mock-data.ts` | Consumption data | Consumption tracking |
| `lib/grid-loads/mock-data.ts` | Grid load data | Grid & loads monitoring |
| `lib/financials/mock-data.ts` | Financial/market data | Financials page |
| `lib/dashboard/mock-data.ts` | Dashboard stats/chart data | Dashboard |
| `lib/scada/mock-data.ts` | SCADA points and data | SCADA monitoring |

## Shared Data

### Maintenance Users (`lib/maintenance/mock-data.ts`)

The `users` array from maintenance mock data is **shared across multiple domains**:
- Role detail panel uses it to display assigned users
- `getUsersForRole(role)` in `lib/auth/mock-data.ts` filters from this array

> **Important:** The `User` type in `lib/maintenance/types.ts` uses a single `name` field and is a separate domain from `UserAccount` in `lib/auth/user-types.ts` which uses `firstName` + `lastName`. These two types are intentionally different to avoid cascade changes across the maintenance domain.

### AlarmSeverity Type

`AlarmSeverity` is defined in `lib/maintenance/types.ts` and shared by both alarms and maintenance modules.

## API Fallback Pattern

Pages fetch from the API first, falling back to mock data on failure:

```typescript
try {
  const data = await apiService.getData();
  setState(data);
} catch {
  setState(mockData); // fallback for development
}
```

This allows the UI to work independently of the backend during development while being ready for real API integration.
