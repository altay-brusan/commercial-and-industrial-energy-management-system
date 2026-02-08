# Color System - Monitoring Pages

Monitoring sayfalarında kullanılan renk sistemi ve anlamları.

---

## Genel Prensipler

Monitoring sayfalarında renkler, sistem durumunu ve veri akışını anlık olarak iletmek için kullanılır. Renkler tutarlı ve anlamlı olmalıdır.

---

## Bağlantı Durumu Renkleri

| Durum | Renk | CSS Class | Kullanım |
|-------|------|-----------|----------|
| Connected | Yeşil | `text-energy-solar` | Aktif bağlantı |
| Connecting | Sarı | `text-energy-consumption` | Bağlanıyor |
| Disconnected | Kırmızı | `text-energy-grid` | Bağlantı kesildi |
| Unknown | Gri | `text-muted-foreground` | Bilinmiyor |

```css
.connection-status {
  &.connected { color: var(--energy-solar); }
  &.connecting { color: var(--energy-consumption); }
  &.disconnected { color: var(--energy-grid); }
  &.unknown { color: var(--muted-foreground); }
}
```

---

## Asset Durum Renkleri

| Durum | Renk | Indicator | Açıklama |
|-------|------|-----------|----------|
| Online | Yeşil | ● | Cihaz çalışıyor |
| Warning | Turuncu | ● | Dikkat gerektiriyor |
| Offline | Kırmızı | ● | Cihaz çevrimdışı |
| Maintenance | Mor | ● | Bakımda |
| Standby | Gri | ○ | Bekleme modunda |

```tsx
const statusColors = {
  online: "bg-energy-solar",
  warning: "bg-energy-consumption",
  offline: "bg-energy-grid",
  maintenance: "bg-energy-battery",
  standby: "bg-muted-foreground",
};
```

---

## Veri Trend Renkleri

| Trend | Renk | Sembol | Kullanım |
|-------|------|--------|----------|
| Artış | Yeşil | ↑ | Değer artıyor |
| Azalış | Kırmızı | ↓ | Değer azalıyor |
| Sabit | Gri | → | Değer sabit |

**Context-Aware Coloring:**

Bazı metrikler için trend renkleri tersine çevrilir:

| Metrik | Artış = | Azalış = |
|--------|---------|----------|
| Production | Yeşil (iyi) | Kırmızı (kötü) |
| Efficiency | Yeşil (iyi) | Kırmızı (kötü) |
| Temperature | Kırmızı (kötü) | Yeşil (iyi) |
| Error Count | Kırmızı (kötü) | Yeşil (iyi) |
| Consumption | Kırmızı (maliyet ↑) | Yeşil (tasarruf) |

```typescript
function getTrendColor(metric: string, trend: 'up' | 'down'): string {
  const inverseMetrics = ['temperature', 'errors', 'consumption', 'cost'];
  const isInverse = inverseMetrics.includes(metric.toLowerCase());

  if (trend === 'up') {
    return isInverse ? 'text-energy-grid' : 'text-energy-solar';
  } else {
    return isInverse ? 'text-energy-solar' : 'text-energy-grid';
  }
}
```

---

## Power Flow Renkleri

### Kaynak Bazlı Renkler

| Kaynak | Ana Renk | Akış Rengi | CSS Variable |
|--------|----------|------------|--------------|
| Solar | Yeşil | Yeşil gradient | `--energy-solar` |
| BESS (Charge) | Mor | Mor gradient | `--energy-battery` |
| BESS (Discharge) | Mor (ters) | Mor gradient | `--energy-battery` |
| Grid (Import) | Kırmızı | Kırmızı gradient | `--energy-grid` |
| Grid (Export) | Yeşil | Yeşil gradient | `--energy-solar` |
| Load | Foreground | - | `--foreground` |

### Akış Animasyonu Renkleri

```css
.power-flow-line {
  stroke-dasharray: 10 5;
  animation: flow-animation 1s linear infinite;
}

@keyframes flow-animation {
  0% { stroke-dashoffset: 15; }
  100% { stroke-dashoffset: 0; }
}

/* Solar akışı */
.flow-solar { stroke: var(--energy-solar); }

/* Grid import */
.flow-grid-import { stroke: var(--energy-grid); }

/* Grid export */
.flow-grid-export { stroke: var(--energy-solar); }

/* Battery charge */
.flow-battery-charge { stroke: var(--energy-battery); }

/* Battery discharge */
.flow-battery-discharge { stroke: var(--energy-battery); }
```

---

## Grid Quality Renkleri

| Kalite | Skor | Renk | Bar Rengi |
|--------|------|------|-----------|
| Excellent | 95-100% | Yeşil | `bg-energy-solar` |
| Good | 80-94% | Açık Yeşil | `bg-energy-solar/70` |
| Fair | 60-79% | Sarı | `bg-energy-consumption` |
| Poor | 40-59% | Turuncu | `bg-energy-consumption/80` |
| Critical | 0-39% | Kırmızı | `bg-energy-grid` |

```tsx
function getQualityColor(score: number): string {
  if (score >= 95) return 'bg-energy-solar';
  if (score >= 80) return 'bg-energy-solar/70';
  if (score >= 60) return 'bg-energy-consumption';
  if (score >= 40) return 'bg-energy-consumption/80';
  return 'bg-energy-grid';
}
```

---

## Parametre Limit Renkleri

| Durum | Renk | Kullanım |
|-------|------|----------|
| Normal | - (varsayılan) | Değer limitlerin içinde |
| Near Limit | Sarı background | Limite yaklaşıyor (±10%) |
| Out of Range | Kırmızı background | Limit aşıldı |

```tsx
const valueColors = {
  normal: "",
  nearLimit: "bg-energy-consumption/10 text-energy-consumption",
  outOfRange: "bg-energy-grid/10 text-energy-grid font-bold",
};
```

---

## Tablo Satır Renkleri

### Hover State

```css
.data-row:hover {
  background-color: var(--muted);
}
```

### Alternating Rows (Opsiyonel)

```css
.data-row:nth-child(even) {
  background-color: var(--muted/30);
}
```

### Highlighted Row (Yeni Veri)

```css
.data-row.updated {
  animation: row-highlight 1s ease;
}

@keyframes row-highlight {
  0% { background-color: var(--energy-solar/20); }
  100% { background-color: transparent; }
}
```

### Selected Row

```css
.data-row.selected {
  background-color: var(--accent);
  border-left: 3px solid var(--energy-battery);
}
```

---

## Grafik Renkleri

### Line Chart Renkleri

| Seri | Renk | Kullanım |
|------|------|----------|
| Series 1 | `--chart-1` | Primary metric |
| Series 2 | `--chart-2` | Secondary metric |
| Series 3 | `--chart-3` | Tertiary metric |
| Series 4 | `--chart-4` | Additional |
| Series 5 | `--chart-5` | Additional |

### Grid Parametreleri için Özel Renkler

| Parametre | Renk | Açıklama |
|-----------|------|----------|
| Voltage L1 | `--chart-1` | Mavi |
| Voltage L2 | `--chart-2` | Yeşil |
| Voltage L3 | `--chart-3` | Mor |
| Frequency | `--chart-4` | Turuncu |
| Power Factor | `--chart-5` | Pembe |

---

## Dark Mode Uyumluluğu

Dark mode'da monitoring renkleri daha parlak ve kontrast yüksek olacak şekilde ayarlanır:

```css
.dark {
  /* Bağlantı durumu - daha parlak */
  --connection-online: oklch(0.75 0.25 145);
  --connection-offline: oklch(0.75 0.20 25);

  /* Trend renkleri - daha parlak */
  --trend-up: oklch(0.75 0.25 145);
  --trend-down: oklch(0.75 0.20 25);

  /* Grafik renkleri - dark mode optimized */
  --chart-1: oklch(0.70 0.20 260);
  --chart-2: oklch(0.75 0.20 160);
  --chart-3: oklch(0.75 0.20 300);
}
```

---

## Erişilebilirlik Notları

1. **Renk kontrastı**: Tüm metin renkleri WCAG AA standardını karşılar
2. **Renk körlüğü**: Renkler yanında semboller kullanılır (●, ↑, ↓)
3. **Focus indicators**: Odaklanmış elementler `outline` ile belirtilir
4. **High contrast mode**: Sistem high contrast modunda border'lar kalınlaşır
