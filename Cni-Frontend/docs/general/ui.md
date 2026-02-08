# UI Dokümantasyonu - Ana Sayfa (Dashboard)

Bu döküman, EMS Dashboard ana sayfasının UI yapısını, kullanılan componentleri ve tasarım kararlarını detaylı şekilde açıklar.

---

## Genel Bakış

Ana sayfa (`/`) bir Energy Management Dashboard'dur. Sayfa 5 ana bölümden oluşur:

```
┌─────────────────────────────────────────────────────────────┐
│  TopBar (Navigation + User Menu)                            │
├─────────────────────────────────────────────────────────────┤
│  StatsRow (4 KPI Kartı)                                     │
├─────────────────────────────────────────────────────────────┤
│  EnergyChart (Alan Grafiği)                                 │
├─────────────────────────────────────────────────────────────┤
│  PowerFlow (4 Güç Kaynağı Kartı)                            │
├─────────────────────────────────────────────────────────────┤
│  RecentActivity (Aktivite Tablosu)                          │
└─────────────────────────────────────────────────────────────┘
```

---

## Sayfa Yapısı

### Dosya: `app/page.tsx`

```tsx
<div className="min-h-screen bg-background">
  <TopBar activeTab="dashboard" />
  <main>
    <StatsRow />
    <EnergyChart />
    <PowerFlow />
    <RecentActivity />
  </main>
</div>
```

- **Layout**: Tam ekran yüksekliği, `bg-background` arka plan
- **Render Tipi**: Server Component (varsayılan)

---

## 1. TopBar Component

**Dosya**: `components/layout/top-bar.tsx`
**Render Tipi**: Client Component (`"use client"`)

### Yapı

```
┌─────────────────────────────────────────────────────────────┐
│ [Logo]  | Dashboard | Sites | Reports | Settings |   [User] │
└─────────────────────────────────────────────────────────────┘
```

### Özellikler

| Özellik | Değer |
|---------|-------|
| Yükseklik | `h-14` (56px) |
| Padding | `px-6` |
| Konum | `sticky top-0 z-50` |
| Border | `border-b border-border` |
| Background | `bg-background` |

### Alt Componentler

| Component | Kaynak | Kullanım |
|-----------|--------|----------|
| `Tabs` | `@/components/ui/tabs` | Navigasyon sekmeleri |
| `DropdownMenu` | `@/components/ui/dropdown-menu` | Kullanıcı menüsü |
| `Avatar` | `@/components/ui/avatar` | Kullanıcı avatarı |

### Props

```typescript
interface TopBarProps {
  activeTab?: string; // "dashboard" | "sites" | "reports" | "settings"
}
```

### Logo

- **Font**: Cocon (özel font)
- **Text**: "Helios Power"
- **Style**: `text-xl tracking-tight`

### Navigasyon Sekmeleri

| Sekme | URL | Değer |
|-------|-----|-------|
| Dashboard | `/` | `dashboard` |
| Sites | `/sites` | `sites` |
| Reports | `/reports` | `reports` |
| Settings | `/settings` | `settings` |

**Tab Stili**:
- Normal: Şeffaf arka plan
- Aktif: `bg-secondary` arka plan

### Kullanıcı Menüsü

**Dropdown İçeriği**:
- Profile
- Billing
- Notifications
- Sign out

---

## 2. StatsRow Component

**Dosya**: `components/dashboard/stats-row.tsx`
**Render Tipi**: Server Component

### Yapı

```
┌──────────────┬──────────────┬──────────────┬──────────────┐
│ Total        │ Current      │ Daily        │ Peak         │
│ Consumption  │ Power        │ Cost         │ Demand       │
│ 2,847 kWh    │ 4.2 kW       │ $127.50      │ 8.7 kW       │
│ +12%         │ -3%          │ +5%          │ same         │
└──────────────┴──────────────┴──────────────┴──────────────┘
```

### Özellikler

| Özellik | Değer |
|---------|-------|
| Layout | `grid grid-cols-2 md:grid-cols-4` |
| Border | `border-b border-border` |
| Kartlar Arası | `border-r border-border` (son hariç) |

### Kart Yapısı

| Element | Style |
|---------|-------|
| Label | `text-sm text-muted-foreground mb-1` |
| Value | `text-3xl font-semibold tracking-tight` |
| Unit | `text-sm text-muted-foreground` |
| Change Badge | `Badge variant="secondary"` |

### Veri Modeli

```typescript
interface StatItem {
  label: string;      // "Total Consumption"
  value: string;      // "2,847"
  unit?: string;      // "kWh"
  change?: {
    value: string;    // "+12%"
    trend: "up" | "down" | "neutral";
  };
}
```

### Trend Renkleri

| Trend | Renk Sınıfı |
|-------|-------------|
| up | `text-energy-grid` (kırmızı) |
| down | `text-energy-solar` (yeşil) |
| neutral | `text-muted-foreground` (gri) |

---

## 3. EnergyChart Component

**Dosya**: `components/dashboard/energy-chart.tsx`
**Render Tipi**: Client Component (`"use client"`)

### Yapı

```
┌─────────────────────────────────────────────────────────────┐
│ Energy Flow                                                 │
│ Today's consumption breakdown                               │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│           [ALAN GRAFİĞİ - 300px yükseklik]                  │
│                                                             │
├─────────────────────────────────────────────────────────────┤
│           [●] Solar    [●] Grid                             │
└─────────────────────────────────────────────────────────────┘
```

### Özellikler

| Özellik | Değer |
|---------|-------|
| Padding | `px-6 py-6` |
| Grafik Yüksekliği | `h-[300px]` |
| Kütüphane | Recharts |

### Grafik Konfigürasyonu

```typescript
const chartConfig = {
  consumption: {
    label: "Consumption",
    color: "var(--color-energy-consumption)",
  },
  solar: {
    label: "Solar",
    color: "var(--color-energy-solar)",
  },
  grid: {
    label: "Grid",
    color: "var(--color-energy-grid)",
  },
};
```

### Grafik Özellikleri

| Özellik | Değer |
|---------|-------|
| Grafik Tipi | `AreaChart` (stacked) |
| X Ekseni | Saat (00:00 - 22:00) |
| Y Ekseni | kW değerleri |
| Grid | `strokeDasharray="3 3"` |
| Fill Opacity | `0.4` |

### Veri Yapısı

```typescript
interface ChartDataPoint {
  time: string;       // "00:00"
  consumption: number; // 2.1
  solar: number;      // 0
  grid: number;       // 2.1
}
```

### Legend

- Solar: `bg-energy-solar` (yeşil nokta)
- Grid: `bg-energy-grid` (kırmızı nokta)

---

## 4. PowerFlow Component

**Dosya**: `components/dashboard/power-flow.tsx`
**Render Tipi**: Client Component (`"use client"`)

### Yapı

```
┌──────────────┬──────────────┬──────────────┬──────────────┐
│     [●]      │     [●]      │     [●]      │     [●]      │
│   4.2 kW     │   1.8 kW     │   0.5 kW     │   6.5 kW     │
│ Solar Panels │ Grid Import  │   Battery    │  Home Load   │
│  generating  │  consuming   │  generating  │  consuming   │
└──────────────┴──────────────┴──────────────┴──────────────┘
```

### Özellikler

| Özellik | Değer |
|---------|-------|
| Layout | `grid grid-cols-2 md:grid-cols-4 gap-4` |
| Padding | `px-6 py-6` |
| Border | `border-t border-border` |

### Kart Stili

| Element | Style |
|---------|-------|
| Container | `flex flex-col items-center p-4 rounded-lg bg-muted/30` |
| Color Dot | `w-3 h-3 rounded-full` |
| Value | `text-2xl font-semibold` |
| Unit | `text-sm font-normal text-muted-foreground` |
| Name | `text-sm text-muted-foreground` |
| Status | `text-xs` |

### Veri Modeli

```typescript
interface PowerSource {
  name: string;      // "Solar Panels"
  value: number;     // 4.2
  unit: string;      // "kW"
  status: "generating" | "consuming" | "idle";
  color: string;     // "bg-energy-solar"
}
```

### Kaynak Renkleri

| Kaynak | Renk Sınıfı |
|--------|-------------|
| Solar Panels | `bg-energy-solar` (yeşil) |
| Grid Import | `bg-energy-grid` (kırmızı) |
| Battery | `bg-energy-battery` (mor) |
| Home Load | `bg-foreground` (siyah/beyaz) |

### Durum Renkleri

| Durum | Renk |
|-------|------|
| generating | `text-energy-solar` (yeşil) |
| consuming | `text-energy-grid` (kırmızı) |
| idle | `text-muted-foreground` (gri) |

---

## 5. RecentActivity Component

**Dosya**: `components/dashboard/recent-activity.tsx`
**Render Tipi**: Server Component

### Yapı

```
┌─────────────────────────────────────────────────────────────┐
│ Recent Activity                                             │
│ Latest energy events and actions                            │
├───────┬─────────────────────────┬────────┬────────┬─────────┤
│ Time  │ Event                   │ Source │ Value  │ Status  │
├───────┼─────────────────────────┼────────┼────────┼─────────┤
│ 14:32 │ Peak demand detected    │ grid   │ 8.7 kW │completed│
│ 12:15 │ Solar export started    │ solar  │+2.1 kW │ active  │
│ 10:00 │ Battery charging        │battery │ 85%    │ active  │
│ 08:30 │ Morning peak consumption│ grid   │ 5.2 kW │completed│
│ 06:00 │ Off-peak charging...    │battery │ 100%   │scheduled│
└───────┴─────────────────────────┴────────┴────────┴─────────┘
```

### Özellikler

| Özellik | Değer |
|---------|-------|
| Padding | `px-6 py-6` |
| Border | `border-t border-border` |
| Tablo | `Table` component |

### Tablo Kolonları

| Kolon | Genişlik | Hizalama | Style |
|-------|----------|----------|-------|
| Time | `w-[80px]` | Sol | `font-mono text-sm text-muted-foreground` |
| Event | Auto | Sol | `font-medium` |
| Source | Auto | Sol | Badge |
| Value | Auto | Sağ | `font-mono` |
| Status | Auto | Sağ | Badge |

### Veri Modeli

```typescript
interface Activity {
  id: string;
  time: string;       // "14:32"
  event: string;      // "Peak demand detected"
  source: "solar" | "grid" | "battery";
  value: string;      // "8.7 kW"
  status: "active" | "completed" | "scheduled";
}
```

### Source Badge Renkleri

| Kaynak | Style |
|--------|-------|
| solar | `bg-energy-solar/10 text-energy-solar border-energy-solar/20` |
| grid | `bg-energy-grid/10 text-energy-grid border-energy-grid/20` |
| battery | `bg-energy-battery/10 text-energy-battery border-energy-battery/20` |

### Status Badge Renkleri

| Durum | Style |
|-------|-------|
| active | `bg-energy-solar/10 text-energy-solar` |
| completed | `bg-secondary text-muted-foreground` |
| scheduled | `bg-energy-battery/10 text-energy-battery` |

---

## Renk Sistemi

### Enerji Renkleri (Light Mode)

| Renk | CSS Variable | OKLCH Değeri | Kullanım |
|------|--------------|--------------|----------|
| Solar | `--energy-solar` | `oklch(0.723 0.219 142.5)` | Yeşil - güneş üretimi |
| Grid | `--energy-grid` | `oklch(0.577 0.245 27.325)` | Kırmızı - şebeke |
| Battery | `--energy-battery` | `oklch(0.623 0.214 259.815)` | Mor - batarya |
| Consumption | `--energy-consumption` | `oklch(0.705 0.213 47.604)` | Turuncu - tüketim |

### Tailwind Kullanımı

```css
/* Background */
bg-energy-solar
bg-energy-grid
bg-energy-battery

/* Text */
text-energy-solar
text-energy-grid
text-energy-battery

/* Opacity ile */
bg-energy-solar/10
bg-energy-grid/20
```

---

## Responsive Tasarım

### Breakpoints

| Breakpoint | Genişlik | Etki |
|------------|----------|------|
| Default | < 768px | 2 kolonlu grid |
| `md:` | >= 768px | 4 kolonlu grid |
| `sm:` | >= 640px | Kullanıcı bilgisi görünür |

### Component Bazlı Responsive Davranış

| Component | Mobile | Desktop |
|-----------|--------|---------|
| TopBar | Logo + Avatar | Logo + Tabs + Avatar |
| StatsRow | 2x2 grid | 4x1 grid |
| PowerFlow | 2x2 grid | 4x1 grid |
| EnergyChart | Tam genişlik | Tam genişlik |
| RecentActivity | Horizontal scroll | Tam genişlik |

---

## Kullanılan UI Componentler

Tüm base componentler `@/components/ui/` altında bulunur (shadcn/ui).

| Component | Dosya | Kullanıldığı Yer |
|-----------|-------|------------------|
| Tabs | `tabs.tsx` | TopBar navigasyon |
| DropdownMenu | `dropdown-menu.tsx` | Kullanıcı menüsü |
| Avatar | `avatar.tsx` | Kullanıcı avatarı |
| Badge | `badge.tsx` | StatsRow, RecentActivity |
| Table | `table.tsx` | RecentActivity |
| Chart | `chart.tsx` | EnergyChart |

---

## Font Sistemi

| Font | CSS Variable | Kullanım |
|------|--------------|----------|
| Geist Sans | `--font-geist-sans` | Body text |
| Geist Mono | `--font-geist-mono` | Sayısal değerler |
| Cocon | `font-logo` class | Logo |

---

## Sayfa Özeti

| Metrik | Değer |
|--------|-------|
| Toplam Component | 5 ana component |
| Client Component | 3 (TopBar, EnergyChart, PowerFlow) |
| Server Component | 2 (StatsRow, RecentActivity) |
| Kullanılan UI Kit | 6 base component |
| Renk Paleti | 4 enerji rengi + sistem renkleri |
| Responsive | Mobile-first, md breakpoint |
