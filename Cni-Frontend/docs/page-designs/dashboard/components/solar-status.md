# Solar Status Widget

Solar üretim ve performansını gösteren önemli widget.

---

## Görsel Tasarım

```
┌─────────────────────────────────────────────────────────────┐
│  SOLAR STATUS                                     [Details] │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Current Production                                         │
│  ┌─────────────────────────────────────────────────────┐   │
│  │                                                     │   │
│  │    125 kW        of 150 kW capacity                │   │
│  │    ████████████████████░░░░░                       │   │
│  │                83% of capacity                     │   │
│  │                                                     │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  Production Today          vs Yesterday             │   │
│  │  ─────────────────────────────────────────────     │   │
│  │  480 kWh                   420 kWh (+14% ^)        │   │
│  │                                                     │   │
│  │  Forecast Remaining Today: 320 kWh                 │   │
│  │  Expected Total: 800 kWh                           │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│  Performance Ratio: 94%  |  Panel Temp: 45°C  |  PR ^ 2%   │
│                                                             │
│  [View Panels]  [Production Chart]  [Maintenance]          │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## Status Badge'leri

| Status | Renk | Açıklama |
|--------|------|----------|
| Producing | Yeşil | Normal üretim |
| Curtailed | Sarı | Kısıtlı üretim |
| Night | Gri | Gece modu |
| Fault | Kırmızı | Arıza |

---

## Metrikler

### Current Power
- Ana gösterge: kW
- Kapasite karşılaştırması: % of capacity
- Progress bar ile görselleştirme

### Production Today
- Toplam üretim: kWh
- vs Yesterday karşılaştırma: % change
- Trend ikonu: ^ veya v

### Forecast
- Remaining Today: Gün sonuna kadar tahmin
- Expected Total: Toplam günlük tahmin

### Performance Ratio
- PR yüzdesi
- Trend göstergesi
- Renk kodlu (>90% yeşil, 80-90% sarı, <80% kırmızı)

---

## Data Structure

```typescript
interface SolarData {
  currentPower: number; // kW
  capacity: number; // kW max
  status: 'producing' | 'curtailed' | 'night' | 'fault';
  todayProduction: number; // kWh
  yesterdayProduction: number; // kWh
  remainingForecast: number; // kWh
  expectedTotal: number; // kWh
  performanceRatio: number; // 0-100
  panelTemperature: number; // °C
  irradiance: number; // W/m²
  inverterStatus: InverterStatus[];
}

interface InverterStatus {
  id: string;
  power: number;
  status: 'online' | 'warning' | 'fault' | 'offline';
  efficiency: number;
}
```

---

## Props

```typescript
interface SolarStatusProps {
  data: SolarData;
  onViewPanels?: () => void;
  onViewChart?: () => void;
  onMaintenance?: () => void;
  onDetailsClick?: () => void;
  showForecast?: boolean;
  showActions?: boolean;
}
```

---

## Quick Actions

| Buton | Aksiyon | Açıklama |
|-------|---------|----------|
| View Panels | Panel haritası | String/panel bazlı görünüm |
| Production Chart | Üretim grafiği | 24h/7d/30d grafik |
| Maintenance | Bakım planı | Scheduled maintenance |

---

## İnteraktivite

### Hover
- Power bar: Exact values
- PR: Historical PR trend
- Production: Hourly breakdown

### Click
- Details → Solar monitoring page
- Actions → Execute action

---

## Weather Integration

Solar irradiance ve hava durumu bağlantısı:

```
┌─────────────────────────────────────────┐
│  Irradiance: 850 W/m²  |  Clear Sky     │
│  Expected production impact: 95%        │
└─────────────────────────────────────────┘
```

---

## Responsive Behavior

| Breakpoint | Görünüm |
|------------|---------|
| Desktop | Full layout with all metrics |
| Tablet | Condensed metrics |
| Mobile | Current power + today only |

---

## Night Mode

Gece modunda özel görünüm:

```
┌─────────────────────────────────────────────────────────────┐
│  SOLAR STATUS                               [Night Mode]    │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│              Production paused                              │
│              Sunrise: 06:42                                 │
│                                                             │
│  Today's Total: 480 kWh                                     │
│  Performance: 94%                                           │
│                                                             │
│  Tomorrow Forecast: 520 kWh (Sunny)                        │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## Accessibility

- Progress bar: Proper ARIA attributes
- Status: Color + text + icon
- Metrics: Screen reader friendly values
- Forecast: Clear time context
