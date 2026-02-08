# Financial Summary Widget

Finansal performansı özetleyen bilgilendirici widget.

---

## Görsel Tasarım

```
┌─────────────────────────────────────────────────────────────┐
│  FINANCIAL SUMMARY                                [Details] │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │                      TODAY                          │   │
│  │  ────────────────────────────────────────────────── │   │
│  │                                                     │   │
│  │   Grid Cost        Savings          Revenue        │   │
│  │   $4,250           $1,850           $2,100         │   │
│  │   v 5%             ^ 12%            ^ 8%           │   │
│  │                                                     │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │                   THIS MONTH                        │   │
│  │  ────────────────────────────────────────────────── │   │
│  │                                                     │   │
│  │   Total Cost       Total Savings    Total Revenue  │   │
│  │   $85,000          $42,000          $38,000        │   │
│  │   Budget: $90,000  Target: $40,000  Target: $35,000│   │
│  │   OK Under         OK Above         OK Above       │   │
│  │                                                     │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│  ROI Progress: 45% complete    Payback: 3.2 years left     │
│                                                             │
│  [Financial Report]  [Trends]  [Set Targets]               │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## Tab Yapısı

### Today Tab
- Grid Cost: Şebeke maliyeti
- Savings: Tasarruf (solar self-consumption)
- Revenue: Gelir (export revenue)
- Net Benefit: Toplam fayda

### This Month Tab
- Totals: Aylık toplamlar
- vs Target: Hedef karşılaştırması
- Budget tracking

---

## Metrik Kartları

### Grid Cost
- Değer: Currency
- Trend: vs yesterday/last month
- Status: Under/Over budget

### Savings
- Değer: Currency
- Source: Self-consumption value
- Trend: vs target

### Revenue
- Değer: Currency
- Source: Export income
- Trend: vs target

### Net Benefit
- Hesaplama: Savings + Revenue - Cost
- Pozitif = Yeşil
- Negatif = Kırmızı

---

## ROI Progress

```
ROI Progress: 45%
████████████████████░░░░░░░░░░░░░░░░░░░░░

Invested: $500,000
Recovered: $225,000
Remaining: $275,000
Payback: 3.2 years left
```

---

## Data Structure

```typescript
interface FinancialData {
  today: {
    cost: number;
    savings: number;
    revenue: number;
    netBenefit: number;
    costVsYesterday: number; // %
    savingsVsYesterday: number;
    revenueVsYesterday: number;
  };
  month: {
    totalCost: number;
    totalSavings: number;
    totalRevenue: number;
    costBudget: number;
    savingsTarget: number;
    revenueTarget: number;
  };
  roi: {
    invested: number;
    recovered: number;
    progressPercent: number;
    paybackYearsLeft: number;
  };
}
```

---

## Props

```typescript
interface FinancialSummaryProps {
  data: FinancialData;
  currency?: string; // "$", "€", "₺"
  onReportClick?: () => void;
  onTrendsClick?: () => void;
  onSetTargetsClick?: () => void;
  onDetailsClick?: () => void;
  defaultTab?: 'today' | 'month';
}
```

---

## Trend Göstergeleri

| Trend | İkon | Renk | Bağlam |
|-------|------|------|--------|
| Cost ^ | ^ | Kırmızı | Bad |
| Cost v | v | Yeşil | Good |
| Savings ^ | ^ | Yeşil | Good |
| Savings v | v | Kırmızı | Bad |
| Revenue ^ | ^ | Yeşil | Good |
| Revenue v | v | Kırmızı | Bad |

---

## Target Status Badges

```
Under Budget  → [OK] Yeşil
On Target     → [OK] Yeşil
At Risk       → [!]  Sarı
Over Budget   → [X]  Kırmızı
```

---

## Quick Actions

| Buton | Aksiyon |
|-------|---------|
| Financial Report | Generate/download report |
| Trends | Show historical charts |
| Set Targets | Configure targets/budgets |

---

## İnteraktivite

### Tab Switch
- Smooth transition
- Persist selection

### Hover
- Metric card: Breakdown tooltip
- ROI bar: Detailed progress

### Click
- Details → Full financial dashboard
- Actions → Execute action

---

## Responsive Behavior

| Breakpoint | Görünüm |
|------------|---------|
| Desktop | Full tabbed layout |
| Tablet | Condensed cards |
| Mobile | Today only, swipe for month |

---

## Currency Formatting

```typescript
function formatCurrency(value: number, currency: string): string {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: currency,
    minimumFractionDigits: 0,
    maximumFractionDigits: 0,
  }).format(value);
}
```

---

## Accessibility

- Currency values: Full pronunciation
- Trends: Describe direction and meaning
- Tabs: Proper ARIA attributes
- Progress bar: Percentage announcement
