# Grid Status Panel Component

Monitoring Grid Status sayfasındaki şebeke durumu paneli.

---

## Genel Bakış

Grid Status Panel, şebeke bağlantısının kalitesini, elektrik parametrelerini ve trend verilerini gösterir. Operatörlerin şebeke sağlığını anlık olarak izlemesini sağlar.

---

## Component Yapısı

```
┌────────────────────────────────────────────────────────────────────────────┐
│ GRID STATUS                                           ● Connected | 14:32  │
├────────────────────────────────────────────────────────────────────────────┤
│                                                                            │
│  ┌─────────────────────────────────────────────────────────────────────┐  │
│  │                    CONNECTION QUALITY                               │  │
│  │                                                                     │  │
│  │  ████████████████████████████████████████████████░░░░░  98%        │  │
│  │                                                                     │  │
│  │        Excellent          Good          Fair          Poor          │  │
│  │            ▲                                                        │  │
│  └─────────────────────────────────────────────────────────────────────┘  │
│                                                                            │
│  ┌───────────────────────────┐  ┌───────────────────────────┐            │
│  │       VOLTAGE             │  │       FREQUENCY           │            │
│  │                           │  │                           │            │
│  │  L1: 398.5 V  ●          │  │  Current: 50.02 Hz        │            │
│  │  L2: 400.2 V  ●          │  │  Nominal: 50.00 Hz        │            │
│  │  L3: 399.1 V  ●          │  │  Range: ±0.5 Hz           │            │
│  │  ────────────             │  │  ────────────             │            │
│  │  Avg: 399.3 V             │  │  Status: ● Normal         │            │
│  │  Nominal: 400 V           │  │                           │            │
│  │  Deviation: -0.2%         │  │  [View 24h Trend]         │            │
│  └───────────────────────────┘  └───────────────────────────┘            │
│                                                                            │
│  ┌───────────────────────────┐  ┌───────────────────────────┐            │
│  │     POWER FACTOR          │  │       CURRENT             │            │
│  │                           │  │                           │            │
│  │  Current: 0.98            │  │  L1: 125.4 A              │            │
│  │  Target: > 0.95           │  │  L2: 127.2 A              │            │
│  │  ────────────             │  │  L3: 126.1 A              │            │
│  │  Status: ● Good           │  │  Neutral: 2.3 A           │            │
│  │                           │  │  ────────────             │            │
│  │  [Correction History]     │  │  Total: 378.7 A           │            │
│  └───────────────────────────┘  └───────────────────────────┘            │
│                                                                            │
└────────────────────────────────────────────────────────────────────────────┘
```

---

## Props Interface

```typescript
interface GridStatusPanelProps {
  data: GridStatusData;
  isLoading?: boolean;
  error?: Error | null;
  showTrends?: boolean;
  onParameterClick?: (parameter: GridParameter) => void;
}

interface GridStatusData {
  connection: {
    status: 'connected' | 'disconnected' | 'unstable';
    quality: number;          // 0-100
    qualityLabel: 'excellent' | 'good' | 'fair' | 'poor' | 'critical';
    lastConnected?: Date;
  };
  voltage: {
    l1: number;
    l2: number;
    l3: number;
    average: number;
    nominal: number;
    deviation: number;        // percentage
    status: ParameterStatus;
  };
  frequency: {
    current: number;
    nominal: number;
    range: { min: number; max: number };
    status: ParameterStatus;
  };
  powerFactor: {
    current: number;
    target: number;
    status: ParameterStatus;
  };
  current: {
    l1: number;
    l2: number;
    l3: number;
    neutral: number;
    total: number;
  };
  power: {
    active: number;           // kW
    reactive: number;         // kVAR
    apparent: number;         // kVA
    direction: 'import' | 'export';
  };
  timestamp: Date;
}

type ParameterStatus = 'normal' | 'warning' | 'critical';
```

---

## Connection Quality Meter

### Quality Levels

| Level | Score | Color | Description |
|-------|-------|-------|-------------|
| Excellent | 95-100% | `bg-energy-solar` | All parameters within optimal range |
| Good | 80-94% | `bg-energy-solar/70` | Minor deviations |
| Fair | 60-79% | `bg-energy-consumption` | Some parameters out of optimal |
| Poor | 40-59% | `bg-energy-consumption/80` | Significant issues |
| Critical | 0-39% | `bg-energy-grid` | Major problems |

### Quality Calculation

```typescript
function calculateGridQuality(data: GridStatusData): number {
  const weights = {
    voltage: 0.3,
    frequency: 0.3,
    powerFactor: 0.2,
    balance: 0.2,
  };

  const voltageScore = getVoltageScore(data.voltage);
  const frequencyScore = getFrequencyScore(data.frequency);
  const pfScore = getPowerFactorScore(data.powerFactor);
  const balanceScore = getBalanceScore(data.current);

  return (
    voltageScore * weights.voltage +
    frequencyScore * weights.frequency +
    pfScore * weights.powerFactor +
    balanceScore * weights.balance
  );
}

function getVoltageScore(voltage: VoltageData): number {
  const deviation = Math.abs(voltage.deviation);
  if (deviation <= 2) return 100;
  if (deviation <= 5) return 80;
  if (deviation <= 8) return 60;
  if (deviation <= 10) return 40;
  return 20;
}
```

### Quality Bar Component

```tsx
interface QualityBarProps {
  score: number;
  label: string;
  showMarkers?: boolean;
}

function QualityBar({ score, label, showMarkers = true }: QualityBarProps) {
  const color = getQualityColor(score);

  return (
    <div className="space-y-2">
      <div className="h-4 bg-muted rounded-full overflow-hidden">
        <div
          className={`h-full ${color} transition-all duration-500`}
          style={{ width: `${score}%` }}
        />
      </div>
      {showMarkers && (
        <div className="flex justify-between text-xs text-muted-foreground">
          <span>Critical</span>
          <span>Poor</span>
          <span>Fair</span>
          <span>Good</span>
          <span>Excellent</span>
        </div>
      )}
      <div className="text-center">
        <span className="text-2xl font-bold">{score}%</span>
        <span className="text-sm text-muted-foreground ml-2">{label}</span>
      </div>
    </div>
  );
}
```

---

## Parameter Cards

### Voltage Card

```
┌───────────────────────────────────┐
│ VOLTAGE                           │
├───────────────────────────────────┤
│                                   │
│  Phase    Value    Status         │
│  ─────────────────────            │
│  L1       398.5 V  ●             │
│  L2       400.2 V  ●             │
│  L3       399.1 V  ●             │
│  ─────────────────────            │
│  Average  399.3 V                 │
│  Nominal  400.0 V                 │
│  Deviation -0.2%                  │
│                                   │
│  [View Trend]                     │
└───────────────────────────────────┘
```

**Status Indicators:**
| Condition | Status | Color |
|-----------|--------|-------|
| Within ±5% | Normal | `text-energy-solar` |
| Within ±8% | Warning | `text-energy-consumption` |
| Beyond ±8% | Critical | `text-energy-grid` |

### Frequency Card

```
┌───────────────────────────────────┐
│ FREQUENCY                         │
├───────────────────────────────────┤
│                                   │
│  ┌─────────────────────────────┐ │
│  │                             │ │
│  │   49.5   50.0   50.5       │ │
│  │     │      │      │        │ │
│  │  ───┼──────●──────┼───     │ │
│  │     │      ▲      │        │ │
│  │   min    50.02   max       │ │
│  └─────────────────────────────┘ │
│                                   │
│  Current: 50.02 Hz  ● Normal     │
│  Range: 49.5 - 50.5 Hz           │
│                                   │
└───────────────────────────────────┘
```

**Frequency Gauge:**
```typescript
interface FrequencyGaugeProps {
  current: number;
  nominal: number;
  min: number;
  max: number;
}

// Visual representation of frequency position within range
```

### Power Factor Card

```
┌───────────────────────────────────┐
│ POWER FACTOR                      │
├───────────────────────────────────┤
│                                   │
│        Current: 0.98              │
│                                   │
│  0.80    0.90    0.95    1.00    │
│   │       │       │       │      │
│   ├───────┼───────┼───────┤      │
│                   ▲   ●          │
│               Target  Current     │
│                                   │
│  Target: > 0.95                   │
│  Status: ● Good                   │
│                                   │
│  [View History]                   │
└───────────────────────────────────┘
```

**Power Factor Status:**
| Value | Status | Action |
|-------|--------|--------|
| ≥ 0.95 | Good | None |
| 0.90-0.95 | Warning | Monitor |
| < 0.90 | Critical | Correction needed |

### Current Card

```
┌───────────────────────────────────┐
│ CURRENT                           │
├───────────────────────────────────┤
│                                   │
│  Phase    Value    % of Max       │
│  ─────────────────────            │
│  L1       125.4 A  ████████░░ 83% │
│  L2       127.2 A  █████████░ 85% │
│  L3       126.1 A  ████████░░ 84% │
│  ─────────────────────            │
│  Neutral  2.3 A    █░░░░░░░░░ 5%  │
│  ─────────────────────            │
│  Total    378.7 A                 │
│                                   │
│  Max Capacity: 150 A/phase        │
└───────────────────────────────────┘
```

**Current Balance Check:**
```typescript
function getCurrentBalanceStatus(current: CurrentData): ParameterStatus {
  const values = [current.l1, current.l2, current.l3];
  const avg = values.reduce((a, b) => a + b, 0) / 3;
  const maxDeviation = Math.max(...values.map(v => Math.abs(v - avg) / avg * 100));

  if (maxDeviation <= 5) return 'normal';
  if (maxDeviation <= 10) return 'warning';
  return 'critical';
}
```

---

## Power Summary Card

```
┌───────────────────────────────────────────────────────────────────────────┐
│ POWER SUMMARY                                                             │
├───────────────────────────────────────────────────────────────────────────┤
│                                                                           │
│  Active Power      Reactive Power     Apparent Power                      │
│                                                                           │
│    275 kW            45 kVAR            278 kVA                          │
│   ← Import          ← Inductive                                           │
│                                                                           │
│  ─────────────────────────────────────────────────────────────────────    │
│                                                                           │
│  Direction: IMPORT from Grid                                              │
│  Today's Import: 2,450 kWh | Today's Export: 560 kWh                     │
│  Net: 1,890 kWh imported                                                  │
│                                                                           │
└───────────────────────────────────────────────────────────────────────────┘
```

---

## Trend Charts

### 24-Hour Trend

```
┌────────────────────────────────────────────────────────────────────────────┐
│ 24-HOUR TREND                                        [Voltage ▼] [Export] │
├────────────────────────────────────────────────────────────────────────────┤
│                                                                            │
│ 410V ┤                                                                     │
│      │        ╭─╮                                                          │
│ 405V ┤    ╭──╯  ╰─╮      ╭─╮                                              │
│      │ ──╯        ╰────╯  ╰──────────────────────────                    │
│ 400V ┼──────────────────────────────────────────────────────────────────  │
│      │                                                                     │
│ 395V ┤                                                                     │
│      │                                                                     │
│ 390V ┤                                                                     │
│      └────┼────┼────┼────┼────┼────┼────┼────┼────┼────┼────┼────┼────   │
│         00   02   04   06   08   10   12   14   16   18   20   22        │
│                                                                            │
│  Current: 399.3 V | Min: 396.2 V (03:15) | Max: 404.1 V (14:30)          │
└────────────────────────────────────────────────────────────────────────────┘
```

### Parameter Selection

```typescript
const trendParameters = [
  { key: 'voltage', label: 'Voltage (V)' },
  { key: 'frequency', label: 'Frequency (Hz)' },
  { key: 'powerFactor', label: 'Power Factor' },
  { key: 'activePower', label: 'Active Power (kW)' },
  { key: 'current', label: 'Current (A)' },
];
```

---

## Alarm Thresholds

```
┌───────────────────────────────────────────────────────────────────────────┐
│ ALARM THRESHOLDS                                              [Configure] │
├───────────────────────────────────────────────────────────────────────────┤
│                                                                           │
│ Parameter         Low Critical  Low Warning   High Warning  High Critical │
│ ─────────────────────────────────────────────────────────────────────     │
│ Voltage (V)       370           380           420           440           │
│ Frequency (Hz)    49.5          49.8          50.2          50.5          │
│ Power Factor      0.85          0.90          -             -             │
│ Current (A)       -             -             140           150           │
│                                                                           │
└───────────────────────────────────────────────────────────────────────────┘
```

---

## States

### Connected State

```
● Connected | Quality: Excellent (98%)
```
- Green indicator
- All metrics visible and updating

### Disconnected State

```
┌────────────────────────────────────────────────────────────────────────────┐
│ GRID STATUS                                                                │
├────────────────────────────────────────────────────────────────────────────┤
│                                                                            │
│  ┌─────────────────────────────────────────────────────────────────────┐  │
│  │                    ● GRID DISCONNECTED                              │  │
│  │                                                                     │  │
│  │        Last connected: 14:27:05 (5 minutes ago)                    │  │
│  │        Reason: Communication timeout                                │  │
│  │                                                                     │  │
│  │        [Reconnect]  [View Logs]  [Report Issue]                    │  │
│  └─────────────────────────────────────────────────────────────────────┘  │
│                                                                            │
│  Last known values (14:27:05):                                            │
│  Voltage: 399.3 V | Frequency: 50.02 Hz | PF: 0.98                       │
│                                                                            │
└────────────────────────────────────────────────────────────────────────────┘
```

### Unstable Connection State

```
● Unstable | Quality: Fair (65%) | Packet loss: 12%
```
- Yellow indicator
- Warning banner
- Retry recommendations

---

## Responsive Behavior

### Desktop (≥1024px)
- 2x2 grid for parameter cards
- Full trend chart
- All details visible

### Tablet (768px - 1023px)
- 2 column layout
- Condensed cards
- Collapsible trend section

### Mobile (<768px)
- Stacked cards
- Swipeable parameter tabs
- Mini trend chart

---

## Usage Example

```tsx
import { GridStatusPanel } from '@/components/monitoring/grid-status-panel';

export default function GridStatusPage() {
  const { data, isLoading, error } = useGridStatus({
    refreshInterval: 5000,
  });

  return (
    <div className="space-y-6">
      <GridStatusPanel
        data={data}
        isLoading={isLoading}
        error={error}
        showTrends={true}
        onParameterClick={(param) => {
          router.push(`/monitoring/grid-status/${param.key}`);
        }}
      />

      <TrendChart
        parameter="voltage"
        data={data?.history?.voltage}
        timeRange="24h"
      />
    </div>
  );
}
```
