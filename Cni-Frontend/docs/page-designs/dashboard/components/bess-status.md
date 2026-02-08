# BESS Status Widget

Batarya sisteminin anlık durumunu gösteren önemli widget.

---

## Görsel Tasarım

```
┌─────────────────────────────────────────────────────────────┐
│  BESS STATUS                                      [Details] │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │                                                     │   │
│  │    ┌─────────────────────────────────────────┐     │   │
│  │    │ █████████████████████░░░░░░░░░░░░░░░░░░ │     │   │
│  │    │            75% SOC                       │     │   │
│  │    └─────────────────────────────────────────┘     │   │
│  │                                                     │   │
│  │    ^ CHARGING                    +50 kW            │   │
│  │    Estimated Full: 2h 30m                          │   │
│  │                                                     │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │ Temperature  │  │ Voltage      │  │ Current      │      │
│  │   32°C OK    │  │   748V       │  │   67A        │      │
│  │ Normal       │  │ Normal       │  │ Normal       │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
│                                                             │
│  Today: Charged 450 kWh | Discharged 320 kWh | Cycles: 0.8 │
│                                                             │
│  [Start Discharge]  [Pause]  [History]                     │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## SOC Renk Kodları

| SOC Aralığı | Renk | Progress Bar | Durum |
|-------------|------|--------------|-------|
| 80-100% | Yeşil | ██████████ | Optimal |
| 50-79% | Yeşil/Sarı | ███████░░░ | Normal |
| 20-49% | Turuncu | ████░░░░░░ | Düşük |
| 0-19% | Kırmızı | █░░░░░░░░░ | Kritik |

---

## Mode Göstergeleri

| Mode | İkon | Renk | Açıklama |
|------|------|------|----------|
| Charging | ^ | Yeşil | Şarj oluyor |
| Discharging | v | Mor | Deşarj oluyor |
| Idle | - | Gri | Beklemede |
| Standby | || | Sarı | Hazır beklemede |
| Fault | X | Kırmızı | Arıza |

---

## Metrik Kartları

### Temperature
- Değer: °C
- Normal: 20-40°C (Yeşil)
- Warning: 40-50°C (Sarı)
- Critical: >50°C (Kırmızı)

### Voltage
- Değer: V
- Pack voltage gösterimi
- Normal range highlight

### Current
- Değer: A
- Pozitif: Şarj
- Negatif: Deşarj

---

## Quick Actions

| Buton | Aksiyon | Koşul |
|-------|---------|-------|
| Start Discharge | Manuel deşarj başlat | Mode ≠ Discharging |
| Start Charge | Manuel şarj başlat | Mode ≠ Charging |
| Pause | Şarj/deşarjı duraklat | Mode = Charging/Discharging |
| History | Geçmiş performansa git | Her zaman |

---

## Data Structure

```typescript
interface BESSData {
  soc: number; // 0-100
  mode: 'charging' | 'discharging' | 'idle' | 'standby' | 'fault';
  power: number; // kW (positive=charging, negative=discharging)
  capacity: number; // kWh total
  temperature: number; // °C
  voltage: number; // V
  current: number; // A
  cycleCount: number;
  health: number; // 0-100 SOH
  estimatedTime?: string; // "2h 30m"
  todayCharged: number; // kWh
  todayDischarged: number; // kWh
  dailyCycles: number;
}
```

---

## Props

```typescript
interface BESSStatusProps {
  data: BESSData;
  onStartCharge?: () => void;
  onStartDischarge?: () => void;
  onPause?: () => void;
  onDetailsClick?: () => void;
  showActions?: boolean;
  compact?: boolean;
}
```

---

## İnteraktivite

### Hover
- SOC bar: Tooltip with exact percentage
- Metric cards: Historical trend mini-chart
- Mode badge: Mode explanation

### Click
- Details link → BESS monitoring page
- Metric card → Metric detail page
- Action buttons → Execute action

---

## Responsive Behavior

| Breakpoint | Görünüm |
|------------|---------|
| Desktop | Full layout with actions |
| Tablet | Full layout, condensed metrics |
| Mobile | SOC + mode only, tap to expand |

---

## Warnings/Alerts

Inline uyarı gösterimi:

```
┌─────────────────────────────────────────────────────────────┐
│  [!] High temperature detected. Cooling system active.      │
└─────────────────────────────────────────────────────────────┘
```

---

## Accessibility

- Progress bar: `aria-valuenow`, `aria-valuemin`, `aria-valuemax`
- Mode: `aria-label` with description
- Metric values: Screen reader friendly units
- Action buttons: Clear labels + keyboard accessible
