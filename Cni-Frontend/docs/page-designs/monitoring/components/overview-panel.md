# Overview Panel Component

Monitoring Overview sayfasındaki sistem durumu özet paneli.

---

## Genel Bakış

Overview Panel, tüm sistemin anlık durumunu tek bakışta gösterir. Operatörler bu panel ile sistemdeki tüm cihazların durumunu hızlıca değerlendirebilir.

---

## Component Yapısı

```
┌────────────────────────────────────────────────────────────────────────────┐
│                         SYSTEM STATUS OVERVIEW                             │
├────────────────────────────────────────────────────────────────────────────┤
│                                                                            │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐ ┌──────────────┐     │
│  │    TOTAL     │ │   ONLINE     │ │   WARNING    │ │   OFFLINE    │     │
│  │              │ │              │ │              │ │              │     │
│  │     48       │ │     45       │ │      2       │ │      1       │     │
│  │   assets     │ │      ●       │ │      ●       │ │      ●       │     │
│  │              │ │   93.75%     │ │    4.17%     │ │    2.08%     │     │
│  └──────────────┘ └──────────────┘ └──────────────┘ └──────────────┘     │
│                                                                            │
└────────────────────────────────────────────────────────────────────────────┘
```

---

## Props Interface

```typescript
interface SystemOverviewProps {
  data: {
    total: number;
    online: number;
    warning: number;
    offline: number;
    maintenance?: number;
  };
  isLoading?: boolean;
  lastUpdate?: Date;
  onStatusClick?: (status: AssetStatus) => void;
}

type AssetStatus = 'all' | 'online' | 'warning' | 'offline' | 'maintenance';
```

---

## Status Kartları

### Total Assets Card

```tsx
<StatusCard
  label="Total"
  value={48}
  sublabel="assets"
  variant="neutral"
  icon={<ServerIcon />}
/>
```

| Özellik | Değer |
|---------|-------|
| Background | `bg-muted/30` |
| Value Font | `text-4xl font-bold` |
| Label Font | `text-sm text-muted-foreground` |

### Online Card

```tsx
<StatusCard
  label="Online"
  value={45}
  percentage={93.75}
  variant="success"
  indicator="●"
  indicatorColor="bg-energy-solar"
/>
```

| Özellik | Değer |
|---------|-------|
| Indicator Color | `bg-energy-solar` (yeşil) |
| Text Color | `text-energy-solar` |
| Background | `bg-energy-solar/10` |

### Warning Card

```tsx
<StatusCard
  label="Warning"
  value={2}
  percentage={4.17}
  variant="warning"
  indicator="●"
  indicatorColor="bg-energy-consumption"
  pulse={true}
/>
```

| Özellik | Değer |
|---------|-------|
| Indicator Color | `bg-energy-consumption` (turuncu) |
| Animation | `pulse` (dikkat çekmek için) |
| Click Action | Filter to warning assets |

### Offline Card

```tsx
<StatusCard
  label="Offline"
  value={1}
  percentage={2.08}
  variant="error"
  indicator="●"
  indicatorColor="bg-energy-grid"
/>
```

| Özellik | Değer |
|---------|-------|
| Indicator Color | `bg-energy-grid` (kırmızı) |
| Text Color | `text-energy-grid` |
| Priority | High visibility |

---

## Asset Group Cards

### Solar Systems Card

```
┌─────────────────────────────────────────┐
│ SOLAR SYSTEMS                    [>]    │
├─────────────────────────────────────────┤
│                                         │
│  ┌───────────┐ ┌───────────┐ ┌───────┐ │
│  │   PV-01   │ │   PV-02   │ │ PV-03 │ │
│  │  125 kW   │ │  118 kW   │ │ 130kW │ │
│  │     ●     │ │     ●     │ │   ●   │ │
│  └───────────┘ └───────────┘ └───────┘ │
│                                         │
│  Total: 373 kW    Efficiency: 94.2%    │
│                                         │
└─────────────────────────────────────────┘
```

```typescript
interface AssetGroupCardProps {
  title: string;
  assets: AssetSummary[];
  totalValue?: string;
  metrics?: { label: string; value: string }[];
  onViewAll?: () => void;
}

interface AssetSummary {
  id: string;
  name: string;
  value: number;
  unit: string;
  status: 'online' | 'warning' | 'offline';
}
```

### BESS Systems Card

```
┌─────────────────────────────────────────┐
│ BESS SYSTEMS                     [>]    │
├─────────────────────────────────────────┤
│                                         │
│  ┌───────────┐ ┌───────────┐ ┌───────┐ │
│  │  BESS-01  │ │  BESS-02  │ │BESS-03│ │
│  │    75%    │ │    82%    │ │  68%  │ │
│  │     ↑     │ │     ↓     │ │   →   │ │
│  │  charging │ │discharge  │ │ idle  │ │
│  └───────────┘ └───────────┘ └───────┘ │
│                                         │
│  Avg SOC: 75%    Total Capacity: 3 MWh │
│                                         │
└─────────────────────────────────────────┘
```

**BESS Özel Göstergeler:**

| Mode | Icon | Color |
|------|------|-------|
| Charging | ↑ | `text-energy-solar` (yeşil) |
| Discharging | ↓ | `text-energy-grid` (kırmızı) |
| Idle | → | `text-muted-foreground` (gri) |

### Grid Status Card

```
┌─────────────────────────────────────────┐
│ GRID STATUS                             │
├─────────────────────────────────────────┤
│                                         │
│  Connection: ● Online                   │
│  Quality: ████████████░░░░ 85%         │
│                                         │
│  Import:  275 kW ↓                      │
│  Export:    0 kW                        │
│                                         │
│  Frequency: 50.02 Hz  ●                 │
│  Voltage:   399.3 V   ●                 │
│  PF:        0.98      ●                 │
│                                         │
└─────────────────────────────────────────┘
```

---

## KPI Metrics Bar

```
┌────────────────────────────────────────────────────────────────────────────┐
│                              KPI METRICS                                   │
├──────────────────┬──────────────────┬──────────────────┬──────────────────┤
│   AVAILABILITY   │      MTBF        │      MTTR        │   UTILIZATION    │
│                  │                  │                  │                  │
│     99.2%        │     720 hrs      │     2.5 hrs      │      78%         │
│     ↑ +0.3%      │     ↑ +12 hrs    │     ↓ -0.5 hrs   │     ↑ +5%        │
└──────────────────┴──────────────────┴──────────────────┴──────────────────┘
```

```typescript
interface KPIMetric {
  label: string;
  value: string | number;
  unit?: string;
  trend?: {
    direction: 'up' | 'down' | 'stable';
    value: string;
    isPositive: boolean;  // up = good veya down = good
  };
}
```

---

## States

### Loading State

```
┌────────────────────────────────────────────────────────────────────────────┐
│                         SYSTEM STATUS OVERVIEW                             │
├────────────────────────────────────────────────────────────────────────────┤
│                                                                            │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐ ┌──────────────┐     │
│  │ ░░░░░░░░░░░░ │ │ ░░░░░░░░░░░░ │ │ ░░░░░░░░░░░░ │ │ ░░░░░░░░░░░░ │     │
│  │ ░░░░░░░░     │ │ ░░░░░░░░     │ │ ░░░░░░░░     │ │ ░░░░░░░░     │     │
│  │ ░░░░░░░░░░   │ │ ░░░░░░░░░░   │ │ ░░░░░░░░░░   │ │ ░░░░░░░░░░   │     │
│  └──────────────┘ └──────────────┘ └──────────────┘ └──────────────┘     │
│                                                                            │
└────────────────────────────────────────────────────────────────────────────┘
```

### Error State

```
┌────────────────────────────────────────────────────────────────────────────┐
│                         SYSTEM STATUS OVERVIEW                             │
├────────────────────────────────────────────────────────────────────────────┤
│                                                                            │
│                    [!] Unable to load system status                        │
│                                                                            │
│                    Connection to server failed.                            │
│                    Last known status from 5 minutes ago.                   │
│                                                                            │
│                    [Retry]  [View Cached]                                  │
│                                                                            │
└────────────────────────────────────────────────────────────────────────────┘
```

### All Systems Operational

```
┌────────────────────────────────────────────────────────────────────────────┐
│                         SYSTEM STATUS OVERVIEW                             │
│                                                              ✓ All Clear  │
├────────────────────────────────────────────────────────────────────────────┤
│                                                                            │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐ ┌──────────────┐     │
│  │    TOTAL     │ │   ONLINE     │ │   WARNING    │ │   OFFLINE    │     │
│  │     48       │ │     48       │ │      0       │ │      0       │     │
│  │   assets     │ │    100%      │ │              │ │              │     │
│  └──────────────┘ └──────────────┘ └──────────────┘ └──────────────┘     │
│                                                                            │
└────────────────────────────────────────────────────────────────────────────┘
```

---

## Interactions

### Card Click

```typescript
const handleStatusClick = (status: AssetStatus) => {
  // Navigate to filtered view
  router.push(`/monitoring/overview?status=${status}`);
};
```

### Hover Tooltip

```
┌──────────────────────────────────────┐
│ Online Assets                        │
│ ─────────────────                    │
│ Solar Systems: 3 online              │
│ BESS Systems: 3 online               │
│ Inverters: 6 online                  │
│ Grid: 1 connected                    │
│ ─────────────────                    │
│ Click to view all online assets      │
└──────────────────────────────────────┘
```

---

## Responsive Behavior

### Desktop (≥1024px)
4 kolonlu grid, tüm kartlar yan yana

### Tablet (768px - 1023px)
2 kolonlu grid, 2 satır

### Mobile (<768px)
Horizontal scroll veya stacked

```tsx
<div className="grid grid-cols-2 md:grid-cols-4 gap-4">
  <StatusCard ... />
  <StatusCard ... />
  <StatusCard ... />
  <StatusCard ... />
</div>
```

---

## Usage Example

```tsx
import { SystemOverview } from '@/components/monitoring/system-overview';

export default function MonitoringOverviewPage() {
  const { data, isLoading, error, refresh } = useSystemStatus();

  return (
    <div className="space-y-6">
      <SystemOverview
        data={data}
        isLoading={isLoading}
        lastUpdate={data?.lastUpdate}
        onStatusClick={(status) => {
          router.push(`/assets?status=${status}`);
        }}
      />

      <div className="grid grid-cols-2 gap-6">
        <AssetGroupCard
          title="Solar Systems"
          assets={data?.solar}
        />
        <AssetGroupCard
          title="BESS Systems"
          assets={data?.bess}
        />
      </div>
    </div>
  );
}
```
