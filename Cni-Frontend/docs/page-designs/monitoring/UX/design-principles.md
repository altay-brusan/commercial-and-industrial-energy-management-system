# Design Principles - Monitoring Pages

Monitoring sayfalarının tasarım prensipleri ve kullanıcı deneyimi ilkeleri.

---

## 1. Real-time Awareness

```
┌─────────────────────────────────────────────────────────────────┐
│  "Real-time Awareness"                                          │
│  ─────────────────────                                          │
│  Kullanıcı her an sistemin güncel durumunu bilmeli              │
└─────────────────────────────────────────────────────────────────┘
```

### Uygulama Kuralları

- Son güncelleme zamanı her zaman görünür
- Bağlantı durumu net şekilde belirtilir
- Veri yaşı (staleness) uyarısı gösterilir
- Otomatik yenileme geri sayımı görünür

### Zaman Gösterimleri

| Süre | Gösterim |
|------|----------|
| < 1 dakika | "Just now" veya "14:32:05" |
| 1-5 dakika | "2 minutes ago" + uyarı |
| 5-15 dakika | "10 minutes ago" + stale warning |
| > 15 dakika | Kırmızı banner + reconnect prompt |

### Örnek Implementation

```typescript
function getDataAgeLabel(lastUpdate: Date): { label: string; severity: 'ok' | 'warning' | 'critical' } {
  const ageMs = Date.now() - lastUpdate.getTime();
  const ageMin = ageMs / 60000;

  if (ageMin < 1) return { label: 'Just now', severity: 'ok' };
  if (ageMin < 5) return { label: `${Math.floor(ageMin)} min ago`, severity: 'warning' };
  if (ageMin < 15) return { label: `${Math.floor(ageMin)} min ago`, severity: 'critical' };
  return { label: 'Data stale', severity: 'critical' };
}
```

---

## 2. Minimal Cognitive Load

```
┌─────────────────────────────────────────────────────────────────┐
│  "Minimal Cognitive Load"                                       │
│  ────────────────────────                                       │
│  Operatör yüzlerce parametreyi tek bakışta anlayabilmeli       │
└─────────────────────────────────────────────────────────────────┘
```

### Stratejiler

1. **Visual Encoding**: Renkler ve semboller ile durum iletimi
2. **Aggregation**: Detay yerine özet gösterimi
3. **Exception Highlighting**: Sadece anormal değerleri vurgulama
4. **Progressive Disclosure**: İstendiğinde detay gösterimi

### Bilgi Katmanları

| Katman | Görünürlük | İçerik |
|--------|------------|--------|
| Özet | Her zaman | Status badge, ana metrik |
| Detay | Hover | Ek metrikler, trend |
| Tam | Click | Full page, history |

### Anti-patterns (Kaçınılacak)

- Tüm parametrelerin aynı anda gösterilmesi
- Renk kodlaması olmayan tablolar
- Anlamsız precision (125.847362 kW yerine 125.8 kW)
- Gereksiz animasyonlar

---

## 3. Scannable Data

```
┌─────────────────────────────────────────────────────────────────┐
│  "Scannable Data"                                               │
│  ───────────────                                                │
│  Tablo ve listeler hızlı taranabilir olmalı                    │
└─────────────────────────────────────────────────────────────────┘
```

### Tablo Tasarım Kuralları

1. **Sabit kolon genişlikleri**: Değerler hizalı kalır
2. **Monospace font**: Sayısal değerler için
3. **Sağa hizalama**: Sayısal kolonlar için
4. **Zebra striping**: Opsiyonel, uzun tablolarda
5. **Sticky headers**: Scroll'da başlıklar sabit

### Hızlı Tarama İçin

```
GOOD:
│ PV-01  │  125.4 kW │  ● │  ↑  │
│ PV-02  │   98.7 kW │  ● │  → │
│ PV-03  │    0.0 kW │  ○ │  -  │  ← Problem hemen görünür

BAD:
│ PV-01 Active Power Output is 125.4 kilowatts, Status: Online │
│ PV-02 Active Power Output is 98.7 kilowatts, Status: Online  │
```

### Visual Scanning Aids

- Status indicators solda (ilk görülen)
- Kritik değerler bold veya renkli
- Gruplar arası separator
- Anormal satırlar highlight

---

## 4. Fault Tolerance

```
┌─────────────────────────────────────────────────────────────────┐
│  "Fault Tolerance"                                              │
│  ─────────────────                                              │
│  Sistem hataları kullanıcıyı bloklamasın                       │
└─────────────────────────────────────────────────────────────────┘
```

### Graceful Degradation

| Hata Durumu | Davranış |
|-------------|----------|
| Kısmi veri kaybı | Mevcut veriyi göster, eksik alanları işaretle |
| Bağlantı kopması | Cache'den göster, uyar, retry et |
| Timeout | Partial update, failed items'ı listele |
| Server error | Son bilinen durumu göster |

### Offline Capability

```typescript
const dataFetchStrategy = {
  primary: 'live-api',
  fallback: 'cached-data',
  offline: 'local-storage',
  staleThreshold: 5 * 60 * 1000, // 5 minutes
};
```

### Error Recovery UI

```
┌─────────────────────────────────────────────────────────────────┐
│ [!] 3 devices not responding                                    │
│                                                                 │
│ Showing cached data for: PV-03, BESS-02, INV-05                │
│ Last known values from 5 minutes ago                           │
│                                                                 │
│ [Retry All]  [Retry Failed Only]  [Continue]                   │
└─────────────────────────────────────────────────────────────────┘
```

---

## 5. Contextual Actions

```
┌─────────────────────────────────────────────────────────────────┐
│  "Contextual Actions"                                           │
│  ────────────────────                                           │
│  Her durum için uygun aksiyonlar sunulmalı                     │
└─────────────────────────────────────────────────────────────────┘
```

### Durum Bazlı Aksiyonlar

| Durum | Primary Action | Secondary Actions |
|-------|----------------|-------------------|
| Device Offline | Reconnect | Diagnose, View Logs |
| Value Out of Range | Acknowledge | Set Threshold, View History |
| Communication Error | Retry | Check Network, Report |
| Maintenance Mode | End Maintenance | Extend, Add Notes |

### Right-click Context Menu

```
┌─────────────────────────┐
│ PV-01                   │
├─────────────────────────┤
│ View Details            │
│ View Logs               │
│ ──────────────          │
│ Set Alert Threshold     │
│ Configure Parameters    │
│ ──────────────          │
│ Export Data             │
│ Add to Dashboard        │
└─────────────────────────┘
```

---

## 6. Predictability

```
┌─────────────────────────────────────────────────────────────────┐
│  "Predictability"                                               │
│  ───────────────                                                │
│  Aynı eylem her yerde aynı sonucu vermeli                      │
└─────────────────────────────────────────────────────────────────┘
```

### Tutarlı Davranışlar

| Eylem | Her Yerde Sonuç |
|-------|-----------------|
| Single Click | Select / Highlight |
| Double Click | Open Details |
| Hover | Show Tooltip |
| Right Click | Context Menu |
| Ctrl + Click | Multi-select |

### Navigation Patterns

- Breadcrumb her zaman mevcut
- Browser back/forward çalışır
- URL deep-linking desteklenir
- State URL'de yansıtılır (filters, sort)

---

## 7. Performance Perception

```
┌─────────────────────────────────────────────────────────────────┐
│  "Performance Perception"                                       │
│  ────────────────────────                                       │
│  Yavaşlık hissi minimuma indirilmeli                           │
└─────────────────────────────────────────────────────────────────┘
```

### Perceived Performance Techniques

1. **Skeleton Loading**: Boş alan yerine placeholder
2. **Optimistic Updates**: UI'ı hemen güncelle, sonra confirm
3. **Progressive Loading**: Önce kritik data, sonra detaylar
4. **Background Refresh**: UI'ı bloklamadan güncelle

### Loading Priority

```typescript
const loadingPriority = [
  'connection-status',  // 1. Bağlantı durumu
  'system-status',      // 2. Genel durum
  'critical-alarms',    // 3. Kritik alarmlar
  'asset-summary',      // 4. Asset özetleri
  'detailed-metrics',   // 5. Detay metrikler
  'historical-data',    // 6. Geçmiş veriler
];
```

### Refresh Without Flicker

```css
.data-container {
  /* Refresh sırasında layout shift önleme */
  min-height: var(--last-height);
}

.data-row {
  /* Smooth update animation */
  transition: background-color 0.2s ease;
}
```

---

## 8. Accessibility First

```
┌─────────────────────────────────────────────────────────────────┐
│  "Accessibility First"                                          │
│  ─────────────────────                                          │
│  Tüm kullanıcılar için erişilebilir olmalı                     │
└─────────────────────────────────────────────────────────────────┘
```

### Keyboard Navigation

| Key | Action |
|-----|--------|
| Tab | Navigate between elements |
| Enter | Activate / Open |
| Escape | Close / Cancel |
| Arrow keys | Navigate within lists/tables |
| Ctrl + R | Manual refresh |

### Screen Reader Support

- ARIA labels for status indicators
- Live regions for data updates
- Meaningful alt text
- Proper heading hierarchy

### Color Independence

```
Renk + Sembol her zaman birlikte:
● Online (yeşil + dolu daire)
○ Offline (kırmızı + boş daire)
↑ Increasing (yeşil + yukarı ok)
↓ Decreasing (kırmızı + aşağı ok)
```
