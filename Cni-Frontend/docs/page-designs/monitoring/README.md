# Monitoring Pages - Design Documentation

Bu klasör, EMS (Energy Management System) Monitoring sayfalarının tasarım dokümantasyonunu içerir.

---

## Sayfa Yapısı

PAGE_HIERARCHY.md'ye göre Monitoring bölümü aşağıdaki sayfalardan oluşur:

```
/monitoring/
├── overview/          # Genel sistem durumu
├── live-data/         # Canlı veri akışı
├── power-flow/        # Güç akış diyagramı
└── grid-status/       # Şebeke durumu
```

---

## Dokümantasyon Yapısı

```
monitoring/
├── README.md                        # Bu dosya
├── UI/
│   ├── visual-hierarchy.md          # Görsel öncelik sıralaması
│   ├── layouts.md                   # Responsive layout tasarımları
│   ├── color-system.md              # Monitoring renk sistemi
│   └── states.md                    # UI state tanımları
├── UX/
│   ├── design-principles.md         # Tasarım prensipleri
│   ├── interactions.md              # Kullanıcı etkileşimleri
│   ├── accessibility.md             # Erişilebilirlik gereksinimleri
│   └── real-time-updates.md         # Gerçek zamanlı güncelleme stratejileri
└── components/
    ├── overview-panel.md            # Sistem durumu özet paneli
    ├── live-data-table.md           # Gerçek zamanlı veri tablosu
    ├── power-flow-diagram.md        # İnteraktif güç akış diyagramı
    └── grid-status-panel.md         # Şebeke durumu paneli
```

---

## Sayfa Açıklamaları

### 1. Overview (`/monitoring/overview`)

**Amaç**: Tüm sistemin anlık durumunu tek bakışta göstermek.

**Ana Özellikler**:
- Sistem durumu özeti (online/warning/offline sayıları)
- Asset grup kartları (Solar, BESS, Grid, Inverters)
- KPI metrikleri (Availability, MTBF, MTTR, Utilization)

**Hedef Kullanıcı**: Operatör, Mühendis, Yönetici

---

### 2. Live Data (`/monitoring/live-data`)

**Amaç**: Tüm sistem parametrelerinin gerçek zamanlı değerlerini görüntülemek.

**Ana Özellikler**:
- Otomatik yenilenen veri tablosu
- Kaynak ve tip bazlı filtreleme
- Değer değişimi animasyonları
- Dışa aktarma (CSV, Excel)

**Hedef Kullanıcı**: Operatör, Mühendis

---

### 3. Power Flow (`/monitoring/power-flow`)

**Amaç**: Enerji akışını görsel olarak temsil etmek.

**Ana Özellikler**:
- İnteraktif güç akış diyagramı
- Animasyonlu akış çizgileri
- Node hover/click detayları
- Enerji dengesi özeti

**Hedef Kullanıcı**: Operatör, Mühendis, Yönetici

---

### 4. Grid Status (`/monitoring/grid-status`)

**Amaç**: Şebeke bağlantı kalitesini ve parametrelerini izlemek.

**Ana Özellikler**:
- Bağlantı kalitesi göstergesi
- Voltaj, frekans, güç faktörü metrikleri
- 24 saat trend grafikleri
- Alarm eşik yapılandırması

**Hedef Kullanıcı**: Operatör, Mühendis

---

## Ortak Özellikler

Tüm monitoring sayfalarında bulunan ortak özellikler:

### Gerçek Zamanlı Güncelleme
- Varsayılan yenileme: 5 saniye
- Kullanıcı etkileşiminde otomatik duraklatma
- Bağlantı durumu göstergesi
- Stale data uyarısı

### Renk Kodlaması
| Renk | Anlamı |
|------|--------|
| Yeşil (`--energy-solar`) | Online, generating, positive trend |
| Kırmızı (`--energy-grid`) | Offline, consuming, negative trend |
| Mor (`--energy-battery`) | Battery, storage |
| Turuncu (`--energy-consumption`) | Warning, consumption |
| Gri | Idle, neutral, unknown |

### Erişilebilirlik
- Klavye navigasyonu desteği
- Screen reader uyumluluğu
- Renk bağımsız göstergeler
- Reduced motion desteği

---

## Responsive Breakpoints

| Breakpoint | Width | Davranış |
|------------|-------|----------|
| Mobile | < 768px | Tek kolon, kompakt kartlar |
| Tablet | 768-1023px | İki kolon, basitleştirilmiş |
| Desktop | 1024-1439px | Tam layout |
| Large | ≥ 1440px | Genişletilmiş spacing |

---

## Kullanılan Componentler

### shadcn/ui Componentleri
- Table (live-data-table)
- Card (status cards, parameter cards)
- Badge (status indicators)
- Tabs (page navigation)
- Select (filters)
- Tooltip (hover details)
- Dialog (details modal)

### Özel Componentler
- QualityBar
- TrendIndicator
- StatusDot
- ValueCell
- PowerFlowDiagram
- FrequencyGauge

---

## İlgili Kaynaklar

- [PAGE_HIERARCHY.md](/docs/PAGE_HIERARCHY.md) - Sayfa hiyerarşisi
- [Dashboard Documentation](/docs/page-designs/dashboard/) - Dashboard tasarımı
- [Color System](/docs/page-designs/dashboard/UI/color-system.md) - Renk sistemi

---

## Sonraki Adımlar

- [ ] Overview sayfası implementasyonu
- [ ] Live Data tablosu implementasyonu
- [ ] Power Flow diyagramı SVG tasarımı
- [ ] Grid Status trend grafikleri
- [ ] WebSocket entegrasyonu
- [ ] Offline mode desteği
