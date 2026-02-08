# Energy Today Widget

Günlük enerji üretim/tüketimini gösteren bilgilendirici widget.

---

## Görsel Tasarım

```
┌─────────────────────────────────────────────────────────────┐
│  ENERGY TODAY                                     [Details] │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │                                                     │   │
│  │   PRODUCTION              CONSUMPTION              │   │
│  │   ──────────────         ──────────────            │   │
│  │                                                     │   │
│  │      2,450 kWh               1,890 kWh             │   │
│  │      ^ 12% vs yesterday      v 5% vs yesterday     │   │
│  │                                                     │   │
│  │   ┌─────────────────────────────────────────────┐  │   │
│  │   │  Solar: 1,800 kWh (73%)                     │  │   │
│  │   │  BESS:    650 kWh (27%)                     │  │   │
│  │   └─────────────────────────────────────────────┘  │   │
│  │                                                     │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  24-Hour Production Profile                         │   │
│  │  ▁▂▃▅▆█████████████▆▅▃▂▁                           │   │
│  │  00  04  08  12  16  20  24                         │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│  Self-Consumption: 78%  │  Net Export: 560 kWh             │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## Ana Metrikler

### Production
- Total: kWh
- By source: Solar, BESS, Grid import
- Trend: vs yesterday %

### Consumption
- Total: kWh
- Trend: vs yesterday %

### Net Export/Import
- Pozitif: Export (yeşil)
- Negatif: Import (kırmızı)

### Self-Consumption Rate
- Formül: Self-consumed / Total Production × 100
- Target: >70%

---

## Stacked Bar Gösterimi

Production breakdown by source:

```
┌──────────────────────────────────────────────────────────┐
│ ████████████████████████████████████░░░░░░░░░░░░░░░░░░░░│
│ └─────── Solar 73% ──────────┘ └─ BESS 27% ─┘           │
└──────────────────────────────────────────────────────────┘
```

Renk kodları:
- Solar: `--energy-solar` (Yeşil)
- BESS: `--energy-battery` (Mor)
- Grid Import: `--energy-grid` (Kırmızı)

---

## Sparkline Chart

24-saatlik üretim profili:

```
▁▂▃▅▆█████████████▆▅▃▂▁
00  04  08  12  16  20  24
```

- Y ekseni: Power (kW)
- X ekseni: Time (hours)
- Area fill: Solar color

---

## Data Structure

```typescript
interface EnergyTodayData {
  production: {
    total: number; // kWh
    solar: number;
    bess: number;
    gridImport: number;
    vsYesterday: number; // %
  };
  consumption: {
    total: number; // kWh
    vsYesterday: number; // %
  };
  netExport: number; // kWh (positive=export, negative=import)
  selfConsumption: number; // %
  hourlyProfile: HourlyData[];
}

interface HourlyData {
  hour: number; // 0-23
  production: number; // kWh
  consumption: number; // kWh
}
```

---

## Props

```typescript
interface EnergyTodayProps {
  data: EnergyTodayData;
  onDetailsClick?: () => void;
  showChart?: boolean;
  showBreakdown?: boolean;
  compact?: boolean;
}
```

---

## Source Breakdown Legend

```
┌────────────────────────────────────────────────────────────┐
│  [■] Solar: 1,800 kWh (73%)                                │
│  [■] BESS: 650 kWh (27%)                                   │
│  [■] Grid Import: 0 kWh (0%)                               │
└────────────────────────────────────────────────────────────┘
```

---

## İnteraktivite

### Hover
- Stacked bar: Source tooltip
- Sparkline: Time + value tooltip
- Metrics: Hourly breakdown

### Click
- Details → Energy analytics page
- Chart → Full chart view

---

## Responsive Behavior

| Breakpoint | Görünüm |
|------------|---------|
| Desktop | Full layout with chart |
| Tablet | No sparkline |
| Mobile | Production/consumption only |

---

## Empty State

Gece veya sistem offline:

```
┌─────────────────────────────────────────────────────────────┐
│  ENERGY TODAY                                               │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│              No production data available yet               │
│              Data collection starts at sunrise              │
│                                                             │
│              Sunrise: 06:42 (in 2h 15m)                    │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## Comparison Mode

Yesterday karşılaştırması:

```
                    Today       Yesterday    Change
Production:        2,450 kWh    2,180 kWh    +12% ^
Consumption:       1,890 kWh    1,990 kWh    -5%  v
Net Export:          560 kWh      190 kWh    +194%
```

---

## Accessibility

- Stacked bar: aria-describedby for breakdown
- Sparkline: Hidden from screen readers, text alternative
- Values: Include units in aria-label
- Trends: Describe direction and meaning
