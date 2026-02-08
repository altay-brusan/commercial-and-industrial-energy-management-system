# Live Data Table Component

Monitoring Live Data sayfasındaki gerçek zamanlı veri tablosu.

---

## Genel Bakış

Live Data Table, sistemdeki tüm parametrelerin gerçek zamanlı değerlerini tablo formatında gösterir. Auto-refresh, filtreleme ve sıralama özellikleri sunar.

---

## Component Yapısı

```
┌────────────────────────────────────────────────────────────────────────────┐
│ LIVE DATA                                           ● Connected | 14:32:05 │
├────────────────────────────────────────────────────────────────────────────┤
│                                                                            │
│ Filters: [All Sources ▼] [All Types ▼]  Search: [____________]  [Export]  │
│                                                                            │
├────────────────────────────────────────────────────────────────────────────┤
│ Source    │ Parameter        │ Value      │ Unit  │ Trend │ Quality │ Time │
├───────────┼──────────────────┼────────────┼───────┼───────┼─────────┼──────┤
│ PV-01     │ Active Power     │ 125.4      │ kW    │  ↑    │  ●      │14:32 │
│ PV-01     │ DC Voltage       │ 620.5      │ V     │  →    │  ●      │14:32 │
│ PV-01     │ Efficiency       │ 94.2       │ %     │  ↓    │  ●      │14:32 │
│ BESS-01   │ SOC              │ 75.0       │ %     │  ↑    │  ●      │14:32 │
│ BESS-01   │ Power            │ +50.0      │ kW    │  →    │  ●      │14:32 │
│ ...       │ ...              │ ...        │ ...   │ ...   │ ...     │ ...  │
├────────────────────────────────────────────────────────────────────────────┤
│ Showing 1-50 of 248           [< Prev] [1] [2] [3] [4] [5] [Next >]       │
└────────────────────────────────────────────────────────────────────────────┘
```

---

## Props Interface

```typescript
interface LiveDataTableProps {
  data: DataPoint[];
  columns?: ColumnConfig[];
  isLoading?: boolean;
  error?: Error | null;

  // Filtering
  filters?: FilterState;
  onFilterChange?: (filters: FilterState) => void;

  // Sorting
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
  onSortChange?: (column: string, direction: 'asc' | 'desc') => void;

  // Pagination
  page?: number;
  pageSize?: number;
  totalCount?: number;
  onPageChange?: (page: number) => void;

  // Selection
  selectedIds?: string[];
  onSelectionChange?: (ids: string[]) => void;

  // Actions
  onRowClick?: (row: DataPoint) => void;
  onExport?: () => void;
  onRefresh?: () => void;

  // Real-time
  refreshInterval?: number;
  highlightChanges?: boolean;
}

interface DataPoint {
  id: string;
  source: string;
  parameter: string;
  value: number;
  unit: string;
  trend: 'up' | 'down' | 'stable';
  quality: 'good' | 'uncertain' | 'bad';
  timestamp: Date;
}

interface FilterState {
  sources: string[];
  types: string[];
  search: string;
}
```

---

## Kolon Yapılandırması

### Varsayılan Kolonlar

| Kolon | Width | Align | Sortable | Filterable |
|-------|-------|-------|----------|------------|
| Source | 120px | Left | Yes | Yes |
| Parameter | Auto | Left | Yes | Yes |
| Value | 100px | Right | Yes | No |
| Unit | 60px | Left | No | No |
| Trend | 50px | Center | Yes | Yes |
| Quality | 60px | Center | Yes | Yes |
| Time | 80px | Right | Yes | No |

### Kolon Tanımları

```typescript
const defaultColumns: ColumnConfig[] = [
  {
    key: 'source',
    header: 'Source',
    width: 120,
    sortable: true,
    filterable: true,
    render: (value) => <SourceBadge source={value} />,
  },
  {
    key: 'parameter',
    header: 'Parameter',
    sortable: true,
    filterable: true,
  },
  {
    key: 'value',
    header: 'Value',
    width: 100,
    align: 'right',
    sortable: true,
    render: (value, row) => (
      <ValueCell
        value={value}
        highlight={row.hasChanged}
        outOfRange={row.outOfRange}
      />
    ),
  },
  {
    key: 'unit',
    header: 'Unit',
    width: 60,
  },
  {
    key: 'trend',
    header: 'Trend',
    width: 50,
    align: 'center',
    sortable: true,
    render: (value) => <TrendIndicator trend={value} />,
  },
  {
    key: 'quality',
    header: 'Quality',
    width: 60,
    align: 'center',
    render: (value) => <QualityIndicator quality={value} />,
  },
  {
    key: 'timestamp',
    header: 'Time',
    width: 80,
    align: 'right',
    sortable: true,
    render: (value) => formatTime(value),
  },
];
```

---

## Sub-components

### Source Badge

```typescript
interface SourceBadgeProps {
  source: string;
  type?: 'solar' | 'bess' | 'grid' | 'inverter';
}

const sourceColors = {
  solar: 'bg-energy-solar/10 text-energy-solar border-energy-solar/20',
  bess: 'bg-energy-battery/10 text-energy-battery border-energy-battery/20',
  grid: 'bg-energy-grid/10 text-energy-grid border-energy-grid/20',
  inverter: 'bg-chart-3/10 text-chart-3 border-chart-3/20',
};
```

```
┌──────────┐
│ ● PV-01  │  ← Solar type
└──────────┘
```

### Value Cell

```typescript
interface ValueCellProps {
  value: number;
  unit?: string;
  precision?: number;
  highlight?: boolean;      // Flash on change
  outOfRange?: boolean;     // Red if limit exceeded
  showChange?: boolean;     // Show delta value
  previousValue?: number;
}
```

```
Normal:        125.4
Highlight:     [127.5]  ← Flash animation
Out of Range:  [152.3]  ← Red background
With Change:   127.5 (+2.1)
```

### Trend Indicator

```typescript
interface TrendIndicatorProps {
  trend: 'up' | 'down' | 'stable';
  showLabel?: boolean;
}
```

| Trend | Symbol | Color | Label |
|-------|--------|-------|-------|
| up | ↑ | `text-energy-solar` | "Increasing" |
| down | ↓ | `text-energy-grid` | "Decreasing" |
| stable | → | `text-muted-foreground` | "Stable" |

### Quality Indicator

```typescript
interface QualityIndicatorProps {
  quality: 'good' | 'uncertain' | 'bad';
}
```

| Quality | Symbol | Color | Tooltip |
|---------|--------|-------|---------|
| good | ● | `text-energy-solar` | "Data quality: Good" |
| uncertain | ◐ | `text-energy-consumption` | "Data quality: Uncertain" |
| bad | ○ | `text-energy-grid` | "Data quality: Bad" |

---

## Filter Controls

### Source Filter

```
┌─────────────────────────────────────────────┐
│ Source: [Select Sources ▼]                  │
│         ┌───────────────────────────────┐   │
│         │ ☑ All Sources                 │   │
│         │ ─────────────────────────     │   │
│         │ ☑ Solar (PV-01, PV-02, PV-03) │   │
│         │ ☑ BESS (BESS-01, BESS-02)     │   │
│         │ ☐ Grid                        │   │
│         │ ☑ Inverters (6 devices)       │   │
│         └───────────────────────────────┘   │
└─────────────────────────────────────────────┘
```

### Type Filter

```
┌─────────────────────────────────────────────┐
│ Type: [Select Types ▼]                      │
│       ┌───────────────────────────────────┐ │
│       │ ☑ All Types                       │ │
│       │ ─────────────────────────         │ │
│       │ ☑ Power (Active, Reactive)        │ │
│       │ ☑ Voltage                         │ │
│       │ ☐ Current                         │ │
│       │ ☐ Temperature                     │ │
│       │ ☑ Efficiency                      │ │
│       │ ☐ SOC/SOH                         │ │
│       └───────────────────────────────────┘ │
└─────────────────────────────────────────────┘
```

### Search Box

```typescript
interface SearchBoxProps {
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
  debounceMs?: number;
}
```

```
┌─────────────────────────────────────────────┐
│ 🔍 [Search parameters...              ] [×] │
│                                             │
│ Recent: [Power] [Voltage] [Temperature]     │
└─────────────────────────────────────────────┘
```

---

## Table States

### Loading State

```
┌────────────────────────────────────────────────────────────────────────────┐
│ Source    │ Parameter        │ Value      │ Unit  │ Trend │ Quality │ Time │
├───────────┼──────────────────┼────────────┼───────┼───────┼─────────┼──────┤
│ ░░░░░░    │ ░░░░░░░░░░░░░░  │ ░░░░░░     │ ░░░   │ ░░    │ ░░      │░░░░░ │
│ ░░░░░░    │ ░░░░░░░░░░░░░░  │ ░░░░░░     │ ░░░   │ ░░    │ ░░      │░░░░░ │
│ ░░░░░░    │ ░░░░░░░░░░░░░░  │ ░░░░░░     │ ░░░   │ ░░    │ ░░      │░░░░░ │
└────────────────────────────────────────────────────────────────────────────┘
```

### Empty State (No Data)

```
┌────────────────────────────────────────────────────────────────────────────┐
│                                                                            │
│                         No data available                                  │
│                                                                            │
│                 Data collection has not started yet.                       │
│                 Please ensure devices are connected.                       │
│                                                                            │
│                        [Check Connections]                                 │
│                                                                            │
└────────────────────────────────────────────────────────────────────────────┘
```

### Empty State (No Results)

```
┌────────────────────────────────────────────────────────────────────────────┐
│                                                                            │
│                      No matching parameters                                │
│                                                                            │
│             No parameters match your filter criteria.                      │
│             Try adjusting your filters or search term.                     │
│                                                                            │
│                         [Clear Filters]                                    │
│                                                                            │
└────────────────────────────────────────────────────────────────────────────┘
```

### Error State

```
┌────────────────────────────────────────────────────────────────────────────┐
│                                                                            │
│                    [!] Unable to load data                                 │
│                                                                            │
│                  Error: Connection timeout                                 │
│                  Last successful update: 14:27:05                          │
│                                                                            │
│                   [Retry]  [View Cached Data]                              │
│                                                                            │
└────────────────────────────────────────────────────────────────────────────┘
```

---

## Row Interactions

### Row Hover

```css
.data-row:hover {
  background-color: var(--muted);
}
```

### Row Selection

```css
.data-row.selected {
  background-color: var(--accent);
  border-left: 3px solid var(--energy-battery);
}
```

### Row Context Menu

```
Right-click on row:
┌─────────────────────────────┐
│ View Details                │
│ View History                │
│ ──────────────              │
│ Set Alert Threshold         │
│ Add to Dashboard            │
│ ──────────────              │
│ Copy Value                  │
│ Export Row                  │
└─────────────────────────────┘
```

---

## Real-time Update Behavior

### Value Change Animation

```css
.value-cell.updated {
  animation: value-pulse 0.5s ease;
}

@keyframes value-pulse {
  0% {
    background-color: var(--energy-solar/30);
    transform: scale(1.02);
  }
  100% {
    background-color: transparent;
    transform: scale(1);
  }
}
```

### New Row Animation

```css
.data-row.new {
  animation: row-slide-in 0.3s ease;
}

@keyframes row-slide-in {
  0% {
    opacity: 0;
    transform: translateY(-10px);
  }
  100% {
    opacity: 1;
    transform: translateY(0);
  }
}
```

---

## Pagination

```typescript
interface PaginationProps {
  page: number;
  pageSize: number;
  totalCount: number;
  onPageChange: (page: number) => void;
  onPageSizeChange?: (size: number) => void;
}
```

```
┌────────────────────────────────────────────────────────────────────────────┐
│ Showing 1-50 of 248 parameters                                             │
│                                                                            │
│ Rows per page: [50 ▼]     [< Prev] [1] [2] [3] [4] [5] [Next >]          │
└────────────────────────────────────────────────────────────────────────────┘
```

---

## Export Functionality

### Export Options

```
┌───────────────────────────┐
│ Export Data               │
├───────────────────────────┤
│ ○ Current view (50 rows)  │
│ ● All data (248 rows)     │
│ ○ Selected only (5 rows)  │
├───────────────────────────┤
│ Format:                   │
│ [CSV ▼]                   │
├───────────────────────────┤
│ [Cancel]  [Export]        │
└───────────────────────────┘
```

### Export Formats

- CSV (default)
- Excel (.xlsx)
- JSON

---

## Usage Example

```tsx
import { LiveDataTable } from '@/components/monitoring/live-data-table';

export default function LiveDataPage() {
  const {
    data,
    isLoading,
    error,
    refresh,
  } = useLiveData({ refreshInterval: 5000 });

  const [filters, setFilters] = useState<FilterState>({
    sources: [],
    types: [],
    search: '',
  });

  const [sortConfig, setSortConfig] = useState({
    column: 'source',
    direction: 'asc' as const,
  });

  return (
    <LiveDataTable
      data={data}
      isLoading={isLoading}
      error={error}
      filters={filters}
      onFilterChange={setFilters}
      sortBy={sortConfig.column}
      sortDirection={sortConfig.direction}
      onSortChange={(col, dir) => setSortConfig({ column: col, direction: dir })}
      refreshInterval={5000}
      highlightChanges={true}
      onRowClick={(row) => router.push(`/assets/${row.source}`)}
      onExport={() => exportData(data, 'csv')}
    />
  );
}
```
