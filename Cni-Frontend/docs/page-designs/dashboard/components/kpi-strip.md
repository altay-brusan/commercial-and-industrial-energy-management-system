# KPI Strip Widget

Önemli KPI'ları tek satırda gösteren bilgilendirici widget.

---

## Görsel Tasarım

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  KEY PERFORMANCE INDICATORS                                      [Customize]│
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐    │
│  │ AVAILABILITY │  │ EFFICIENCY   │  │ SELF-CONSUMP │  │ CARBON SAVED │    │
│  │              │  │              │  │              │  │              │    │
│  │    99.2%     │  │    94.5%     │  │    78%       │  │   1.2 tons   │    │
│  │    ^ 0.3%    │  │    v 0.5%    │  │    ^ 5%      │  │    ^ 15%     │    │
│  │              │  │              │  │              │  │              │    │
│  │  Target: 99% │  │ Target: 95%  │  │ Target: 75%  │  │ Target: 1t   │    │
│  │    OK Met    │  │   [!] Close  │  │    OK Met    │  │    OK Met    │    │
│  │              │  │              │  │              │  │              │    │
│  └──────────────┘  └──────────────┘  └──────────────┘  └──────────────┘    │
│                                                                             │
│  Period: Today  │  [Day] [Week] [Month] [Year]                              │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Varsayılan KPI'lar

| KPI | Formül | Birim | Hedef |
|-----|--------|-------|-------|
| Availability | Uptime / Total Time × 100 | % | >99% |
| Efficiency | Output Energy / Input Energy × 100 | % | >95% |
| Self-Consumption | Self-consumed / Total Production × 100 | % | >70% |
| Carbon Saved | kWh × Carbon Factor | tons | - |

---

## Period Selector

```
[Day] [Week] [Month] [Year]
  ↑
  Active
```

Her period için:
- KPI değerleri yeniden hesaplanır
- Trendler ilgili perioda göre karşılaştırılır
- Targets period-specific olabilir

---

## KPI Card Yapısı

```
┌──────────────────┐
│  AVAILABILITY    │  ← Label
│                  │
│    99.2%         │  ← Value (büyük font)
│    ^ 0.3%        │  ← Trend (vs previous period)
│                  │
│  Target: 99%     │  ← Target
│    OK Met        │  ← Status badge
│                  │
└──────────────────┘
```

---

## Target Status

| Status | Badge | Renk | Koşul |
|--------|-------|------|-------|
| Met | OK | Yeşil | value >= target |
| Close | [!] | Sarı | value within 5% of target |
| Behind | X | Kırmızı | value < target - 5% |
| N/A | - | Gri | No target set |

---

## Trend Göstergeleri

| Trend | İkon | Renk | Açıklama |
|-------|------|------|----------|
| Increase | ^ | Yeşil* | Artış |
| Decrease | v | Kırmızı* | Azalış |
| Stable | - | Gri | Değişim yok |

*Renk, KPI'ın doğasına göre değişir:
- Efficiency artışı = Yeşil (iyi)
- Downtime artışı = Kırmızı (kötü)

---

## Data Structure

```typescript
interface KPIData {
  kpis: KPI[];
  period: 'day' | 'week' | 'month' | 'year';
}

interface KPI {
  id: string;
  label: string;
  value: number;
  unit: string;
  trend: number; // % change
  target?: number;
  status: 'met' | 'close' | 'behind' | 'na';
  description?: string;
}
```

---

## Props

```typescript
interface KPIStripProps {
  data: KPIData;
  onPeriodChange?: (period: 'day' | 'week' | 'month' | 'year') => void;
  onCustomize?: () => void;
  onKPIClick?: (kpiId: string) => void;
  maxKPIs?: number; // Default: 4
  showTargets?: boolean;
  showTrends?: boolean;
}
```

---

## Customization Modal

```
┌─────────────────────────────────────────────────────────────┐
│  CUSTOMIZE KPIs                                        [×]  │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  DISPLAYED KPIs (max 4)           AVAILABLE KPIs           │
│  ─────────────────────           ───────────────           │
│  [≡] Availability      [×]       [ ] Peak Demand           │
│  [≡] Efficiency        [×]       [ ] Grid Dependency       │
│  [≡] Self-Consumption  [×]       [ ] Cost per kWh          │
│  [≡] Carbon Saved      [×]       [ ] Battery Cycles        │
│                                  [ ] Revenue               │
│                                  [ ] Uptime                │
│                                                             │
│  Drag to reorder                 Click to add              │
│                                                             │
│  [Cancel]                              [Save Changes]       │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## İnteraktivite

### Period Selector
- Click to switch period
- Instant data update

### KPI Card
- Hover: Detailed breakdown
- Click: KPI detail page

### Customize
- Drag to reorder
- Click to add/remove

---

## Responsive Behavior

| Breakpoint | Görünüm |
|------------|---------|
| Desktop | 4 KPIs in row |
| Tablet | 4 KPIs, smaller |
| Mobile | Horizontal scroll |

### Mobile Layout

```
┌─────────────────────────────────────────────────────────────┐
│  KPIs                                         [Day] [...]   │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ← [AVAIL] [EFFIC] [SELF-C] [CARBON] →                     │
│     99.2%   94.5%    78%     1.2t                          │
│                                                             │
│  Swipe for more                                             │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## Accessibility

- Period selector: Radio group with labels
- KPI values: Include units in aria-label
- Trends: Describe direction and percentage
- Status: Explicit status announcement
