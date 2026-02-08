# Grid Status Widget

Şebeke bağlantı kalitesini ve etkileşimini gösteren önemli widget.

---

## Görsel Tasarım

```
┌─────────────────────────────────────────────────────────────┐
│  GRID STATUS                                      [Details] │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  CONNECTION: GOOD                                    │   │
│  │  ████████████████████████████████████████          │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│  ┌─────────────────┐      ┌─────────────────┐              │
│  │  IMPORT         │      │  EXPORT         │              │
│  │  275 kW         │      │  0 kW           │              │
│  │  v From Grid    │      │  ^ To Grid      │              │
│  └─────────────────┘      └─────────────────┘              │
│                                                             │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │ Frequency    │  │ Voltage      │  │ Power Factor │      │
│  │  50.02 Hz    │  │  398V L-L    │  │  0.98        │      │
│  │  OK Normal   │  │  OK Normal   │  │  OK Good     │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
│                                                             │
│  Today: Imported 1,250 kWh | Exported 450 kWh | Net: -800  │
│                                                             │
│  Tariff Now: Peak ($2.5/kWh)    Next: Off-Peak in 2h       │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## Bağlantı Durumu

| Durum | Renk | Bar | Açıklama |
|-------|------|-----|----------|
| Connected | Yeşil | ████████████ | Şebekeye bağlı |
| Islanded | Sarı | ████████████ | Ada modunda |
| Fault | Kırmızı | ████████████ | Bağlantı arızası |

---

## Bağlantı Kalitesi

| Kalite | Renk | Koşullar |
|--------|------|----------|
| Excellent | Yeşil | Freq 49.9-50.1, V ±5%, PF >0.95 |
| Good | Açık Yeşil | Freq 49.8-50.2, V ±8%, PF >0.90 |
| Fair | Sarı | Küçük sapmalar |
| Poor | Turuncu | Önemli sapmalar |
| Critical | Kırmızı | Limit dışı değerler |

---

## Import/Export Gösterimi

### Import Mode (Grid → Load)
- Kırmızı/mavi renk
- v ok işareti
- "From Grid" etiketi

### Export Mode (Solar → Grid)
- Yeşil renk
- ^ ok işareti
- "To Grid" etiketi

### Badge
```
Import: [v IMPORT]
Export: [^ EXPORT]
```

---

## Grid Metrikleri

### Frequency
- Normal: 49.9-50.1 Hz
- Warning: 49.8-50.2 Hz
- Critical: <49.8 or >50.2 Hz

### Voltage
- 3-fazlı L-L gösterimi
- Normal: ±5%
- Warning: ±8%

### Power Factor
- Excellent: >0.95
- Good: 0.90-0.95
- Fair: 0.85-0.90
- Poor: <0.85

---

## Tariff Information

```
┌─────────────────────────────────────────────────────────────┐
│  Tariff Now: Peak                     $2.50/kWh            │
│  Next: Off-Peak                       in 2h 15m            │
│  Tomorrow Peak: 08:00-22:00                                 │
└─────────────────────────────────────────────────────────────┘
```

---

## Data Structure

```typescript
interface GridData {
  connectionStatus: 'connected' | 'islanded' | 'fault';
  quality: 'excellent' | 'good' | 'fair' | 'poor' | 'critical';
  importPower: number; // kW
  exportPower: number; // kW
  frequency: number; // Hz
  voltage: number; // V
  powerFactor: number;
  todayImported: number; // kWh
  todayExported: number; // kWh
  tariff: TariffInfo;
}

interface TariffInfo {
  current: string; // "Peak"
  rate: number; // $/kWh
  nextPeriod: string; // "Off-Peak"
  nextChangeIn: string; // "2h 15m"
}
```

---

## Props

```typescript
interface GridStatusProps {
  data: GridData;
  onDetailsClick?: () => void;
  showTariff?: boolean;
  showDailyStats?: boolean;
  compact?: boolean;
}
```

---

## İnteraktivite

### Hover
- Quality bar: Detailed quality metrics
- Import/Export: Today's hourly breakdown
- Tariff: Full tariff schedule

### Click
- Details → Grid monitoring page
- Tariff → Tariff schedule page

---

## Islanded Mode

Ada modunda özel görünüm:

```
┌─────────────────────────────────────────────────────────────┐
│  GRID STATUS                              [!] ISLANDED      │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  [!] Operating in island mode                               │
│      Grid disconnected at 10:42                             │
│                                                             │
│  Local Generation: 125 kW (Solar + BESS)                   │
│  Local Load: 120 kW                                         │
│  Balance: +5 kW (OK)                                        │
│                                                             │
│  [Attempt Reconnection]                                     │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## Responsive Behavior

| Breakpoint | Görünüm |
|------------|---------|
| Desktop | Full layout |
| Tablet | Condensed metrics |
| Mobile | Status + import/export only |

---

## Accessibility

- Connection status: Color + text + icon
- Quality bar: ARIA with description
- Metrics: Screen reader values with units
- Tariff: Time-based announcements
