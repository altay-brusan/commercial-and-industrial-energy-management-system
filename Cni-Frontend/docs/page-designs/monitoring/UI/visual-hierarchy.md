# Visual Hierarchy - Monitoring Pages

Monitoring sayfalarındaki görsel öncelik sıralaması ve bilgi mimarisi.

---

## Genel Bakış

Monitoring sayfaları gerçek zamanlı veri görüntüleme ve sistem izleme için tasarlanmıştır. Görsel hiyerarşi, operatörlerin kritik bilgilere hızla ulaşabilmesini sağlar.

---

## Sayfa Bazlı Hiyerarşi

### Overview Page (`/monitoring/overview`)

```
┌────────────────────────────────────────────────────────────────────────┐
│ TIER 1: System Status Summary (En önemli)                              │
│ • Toplam sistem durumu badge'i                                         │
│ • Kritik alarm sayısı                                                  │
│ • Aktif cihaz sayısı                                                   │
├────────────────────────────────────────────────────────────────────────┤
│ TIER 2: Asset Status Grid (Orta önem)                                  │
│ • Solar sistemler durumu                                               │
│ • BESS sistemler durumu                                                │
│ • Grid bağlantı durumu                                                 │
│ • İnverter durumları                                                   │
├────────────────────────────────────────────────────────────────────────┤
│ TIER 3: KPI & Metrics (Detay bilgi)                                    │
│ • Availability yüzdeleri                                               │
│ • Performance metrikleri                                               │
│ • Trend göstergeleri                                                   │
└────────────────────────────────────────────────────────────────────────┘
```

### Live Data Page (`/monitoring/live-data`)

```
┌────────────────────────────────────────────────────────────────────────┐
│ TIER 1: Real-time Data Stream Header                                   │
│ • Son güncelleme zamanı                                                │
│ • Bağlantı durumu (connected/disconnected)                             │
│ • Yenileme hızı göstergesi                                             │
├────────────────────────────────────────────────────────────────────────┤
│ TIER 2: Data Table (Ana içerik)                                        │
│ • Parametre adı                                                        │
│ • Güncel değer (highlight ile)                                         │
│ • Birim                                                                │
│ • Trend (↑↓→)                                                         │
├────────────────────────────────────────────────────────────────────────┤
│ TIER 3: Filters & Controls                                             │
│ • Kaynak filtreleri                                                    │
│ • Yenileme aralığı seçimi                                              │
│ • Dışa aktarma butonu                                                  │
└────────────────────────────────────────────────────────────────────────┘
```

### Power Flow Page (`/monitoring/power-flow`)

```
┌────────────────────────────────────────────────────────────────────────┐
│ TIER 1: Interactive Power Flow Diagram                                 │
│ • Animasyonlu güç akış gösterimi                                       │
│ • Kaynak-hedef bağlantıları                                            │
│ • Anlık güç değerleri                                                  │
├────────────────────────────────────────────────────────────────────────┤
│ TIER 2: Source/Sink Summary Cards                                      │
│ • Solar üretim kartı                                                   │
│ • BESS kartı (charge/discharge)                                        │
│ • Grid kartı (import/export)                                           │
│ • Load kartı                                                           │
├────────────────────────────────────────────────────────────────────────┤
│ TIER 3: Balance & Statistics                                           │
│ • Enerji dengesi                                                       │
│ • Verimlilik oranları                                                  │
│ • Period toplamları                                                    │
└────────────────────────────────────────────────────────────────────────┘
```

### Grid Status Page (`/monitoring/grid-status`)

```
┌────────────────────────────────────────────────────────────────────────┐
│ TIER 1: Grid Connection Quality                                        │
│ • Bağlantı kalitesi göstergesi (Excellent/Good/Fair/Poor)              │
│ • Ana metrikler (Voltage, Frequency, Power Factor)                     │
├────────────────────────────────────────────────────────────────────────┤
│ TIER 2: Detailed Metrics                                               │
│ • Faz bazlı voltaj değerleri                                           │
│ • Akım değerleri                                                       │
│ • Harmonik analizi                                                     │
├────────────────────────────────────────────────────────────────────────┤
│ TIER 3: Historical & Trends                                            │
│ • Son 24 saat trend grafikleri                                         │
│ • Min/Max/Avg değerleri                                                │
│ • Anomali geçmişi                                                      │
└────────────────────────────────────────────────────────────────────────┘
```

---

## Görsel Öncelik Kuralları

### Font Boyutları

| Tier | Font Size | Font Weight | Kullanım |
|------|-----------|-------------|----------|
| Tier 1 | `text-3xl` - `text-4xl` | `font-bold` | Ana metrikler, durum |
| Tier 2 | `text-xl` - `text-2xl` | `font-semibold` | Detay metrikler |
| Tier 3 | `text-sm` - `text-base` | `font-normal` | Açıklamalar, labels |

### Spacing

| Element | Gap/Margin |
|---------|------------|
| Section arası | `gap-8` (32px) |
| Card arası | `gap-4` (16px) |
| İç element arası | `gap-2` (8px) |

### Visual Weight

```
Renk Doygunluğu: Tier 1 > Tier 2 > Tier 3
Gölge Derinliği: Tier 1 > Tier 2 > Tier 3
Border Kalınlığı: Tier 1 = 2px, Tier 2 = 1px, Tier 3 = 0 veya subtle
```

---

## Attention Grabbing Elements

### Kritik Durum Göstergeleri

```
┌─────────────────────────────────────────┐
│ [!] CRITICAL: Grid Disconnected        │  ← Pulse animation
│     Last seen: 14:32:05                │
│     [Reconnect] [Acknowledge]          │
└─────────────────────────────────────────┘
```

### Animasyonlu Değer Güncellemeleri

```css
.value-updated {
  animation: highlight-flash 0.5s ease;
}

@keyframes highlight-flash {
  0% { background-color: var(--energy-solar/30); }
  100% { background-color: transparent; }
}
```

### Badge Priority System

| Priority | Style | Kullanım |
|----------|-------|----------|
| Critical | `bg-energy-grid text-white pulse` | Sistem arızası |
| Warning | `bg-energy-consumption text-white` | Dikkat gerektiren |
| Info | `bg-energy-battery text-white` | Bilgilendirme |
| Success | `bg-energy-solar text-white` | Normal durum |

---

## Z-Index Katmanları

| Layer | Z-Index | İçerik |
|-------|---------|--------|
| Base | 0 | Normal içerik |
| Sticky Headers | 10 | Tablo başlıkları |
| Floating Controls | 20 | Filtre panelleri |
| Tooltips | 30 | Hover bilgileri |
| Modals | 40 | Dialog/Sheet |
| Alerts | 50 | Kritik bildirimler |
