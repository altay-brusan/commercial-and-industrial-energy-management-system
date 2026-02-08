# EMS Sayfa Hiyerarşisi

Bu döküman, Energy Management System (EMS) arayüzünün sayfa yapısını tanımlar.

---

## Genel Bakış

```
/
├── dashboard/                    # Ana kontrol paneli
├── monitoring/                   # Gerçek zamanlı izleme
│   ├── overview/                 # Genel sistem durumu
│   ├── live-data/                # Canlı veri akışı
│   ├── power-flow/               # Güç akış diyagramı
│   └── grid-status/              # Şebeke durumu
├── assets/                       # Varlık yönetimi
│   ├── overview/                 # Tüm varlıklar listesi
│   ├── solar/                    # Güneş panelleri
│   ├── wind/                     # Rüzgar türbinleri
│   ├── battery/                  # Batarya sistemleri
│   ├── ev-chargers/              # EV şarj istasyonları
│   ├── inverters/                # İnverterler
│   └── [asset-id]/               # Tekil varlık detayı
├── analytics/                    # Analitik ve raporlar
│   ├── performance/              # Performans analizi
│   ├── energy-production/        # Üretim raporları
│   ├── consumption/              # Tüketim analizi
│   ├── forecasting/              # Tahminleme
│   └── comparison/               # Karşılaştırmalı analiz
├── alarms/                       # Alarm yönetimi
│   ├── active/                   # Aktif alarmlar
│   ├── history/                  # Alarm geçmişi
│   └── rules/                    # Alarm kuralları
├── events/                       # Olay günlüğü
│   ├── system/                   # Sistem olayları
│   └── user-activity/            # Kullanıcı aktiviteleri
├── control/                      # Kontrol merkezi
│   ├── dispatch/                 # Enerji dağıtımı
│   ├── schedules/                # Zamanlama
│   └── commands/                 # Manuel komutlar
├── settings/                     # Ayarlar
│   ├── general/                  # Genel ayarlar
│   ├── notifications/            # Bildirim ayarları
│   ├── integrations/             # Entegrasyonlar
│   └── system/                   # Sistem konfigürasyonu
├── admin/                        # Yönetim paneli
│   ├── users/                    # Kullanıcı yönetimi
│   ├── roles/                    # Rol ve izinler
│   ├── audit-log/                # Denetim günlüğü
│   └── licenses/                 # Lisans yönetimi
└── profile/                      # Kullanıcı profili
    ├── account/                  # Hesap bilgileri
    └── preferences/              # Tercihler
```

---

## Sayfa Detayları

### 1. Dashboard (`/dashboard`)

Ana kontrol paneli - sistemin genel durumunu tek bakışta gösterir.

| Widget | Açıklama |
|--------|----------|
| Toplam Üretim | Anlık ve günlük enerji üretimi |
| Toplam Tüketim | Anlık ve günlük enerji tüketimi |
| Batarya Durumu | SoC (State of Charge) özeti |
| Şebeke Durumu | Grid bağlantı durumu |
| Aktif Alarmlar | Kritik uyarılar sayısı |
| Hava Durumu | Tahmin ve etkiler |
| Mini Harita | Tesis lokasyonları |

---

### 2. Monitoring (`/monitoring`)

#### 2.1 Overview (`/monitoring/overview`)
- Tüm sistemlerin anlık durumu
- Durum kartları (online/offline/warning/error)
- KPI göstergeleri

#### 2.2 Live Data (`/monitoring/live-data`)
- Gerçek zamanlı veri tabloları
- Otomatik yenileme (5s/15s/30s/1m)
- Filtreleme ve sıralama

#### 2.3 Power Flow (`/monitoring/power-flow`)
- İnteraktif güç akış diyagramı (Sankey)
- Kaynak → Depolama → Tüketim → Şebeke
- Animasyonlu akış gösterimi

#### 2.4 Grid Status (`/monitoring/grid-status`)
- Şebeke bağlantı kalitesi
- Frekans ve voltaj izleme
- Import/Export dengeleri

---

### 3. Assets (`/assets`)

#### 3.1 Overview (`/assets/overview`)
- Tüm varlıkların listesi
- Tablo ve harita görünümü
- Toplu işlemler

#### 3.2 Solar (`/assets/solar`)
- PV sistem listesi
- Panel verimliliği
- String monitoring

#### 3.3 Wind (`/assets/wind`)
- Türbin listesi
- Rüzgar hızı / üretim korelasyonu
- Kapasite faktörü

#### 3.4 Battery (`/assets/battery`)
- BESS sistemleri
- SoC / SoH izleme
- Şarj/Deşarj döngüleri

#### 3.5 EV Chargers (`/assets/ev-chargers`)
- Şarj istasyonu durumları
- Oturum geçmişi
- Kullanım istatistikleri

#### 3.6 Inverters (`/assets/inverters`)
- İnverter listesi
- Verimlilik metrikleri
- Sıcaklık izleme

#### 3.7 Asset Detail (`/assets/[asset-id]`)
- Tekil varlık detay sayfası
- Gerçek zamanlı değerler
- Trend grafikleri
- Konfigürasyon
- Bakım geçmişi

---

### 4. Analytics (`/analytics`)

#### 4.1 Performance (`/analytics/performance`)
- PR (Performance Ratio) analizi
- Kayıp analizi (Loss analysis)
- Benchmark karşılaştırması

#### 4.2 Energy Production (`/analytics/energy-production`)
- Günlük/Haftalık/Aylık üretim
- Hedef vs Gerçekleşen
- Trend analizi

#### 4.3 Consumption (`/analytics/consumption`)
- Tüketim profilleri
- Pik saatler analizi
- Maliyet optimizasyonu

#### 4.4 Forecasting (`/analytics/forecasting`)
- Üretim tahminleri
- Tüketim tahminleri
- Hava durumu entegrasyonu

#### 4.5 Comparison (`/analytics/comparison`)
- Dönemsel karşılaştırma
- Varlıklar arası karşılaştırma
- Benchmark analizi

---

### 5. Alarms (`/alarms`)

#### 5.1 Active (`/alarms/active`)
- Aktif alarm listesi
- Severity: Critical / Warning / Info
- Acknowledge işlemi
- Filtreleme (kaynak, tip, tarih)

#### 5.2 History (`/alarms/history`)
- Geçmiş alarmlar
- MTTR (Mean Time to Resolve)
- Tekrar eden alarm analizi

#### 5.3 Rules (`/alarms/rules`)
- Alarm kural tanımları
- Eşik değerleri
- Eskalasyon kuralları
- Bildirim yapılandırması

---

### 6. Events (`/events`)

#### 6.1 System Events (`/events/system`)
- Otomatik tetiklenen olaylar
- Firmware güncellemeleri
- Bağlantı kesintileri

#### 6.2 User Activity (`/events/user-activity`)
- Kullanıcı işlemleri
- Login/Logout kayıtları
- Yapılan değişiklikler

---

### 7. Control (`/control`)

#### 7.1 Dispatch (`/control/dispatch`)
- Enerji dağıtım kontrolü
- Setpoint yönetimi
- Otomatik/Manuel mod

#### 7.2 Schedules (`/control/schedules`)
- Zamanlı görevler
- Şarj/Deşarj programları
- Bakım planlaması

#### 7.3 Commands (`/control/commands`)
- Manuel komut gönderimi
- Start/Stop işlemleri
- Acil durum kontrolleri

---

### 8. Settings (`/settings`)

#### 8.1 General (`/settings/general`)
- Dil ve bölge
- Zaman dilimi
- Birim tercihleri (kW/MW, kWh/MWh)

#### 8.2 Notifications (`/settings/notifications`)
- E-posta bildirimleri
- SMS ayarları
- Push notification
- Bildirim kuralları

#### 8.3 Integrations (`/settings/integrations`)
- API bağlantıları
- SCADA entegrasyonu
- Webhook yapılandırması
- Third-party servisler

#### 8.4 System (`/settings/system`)
- Veri saklama politikaları
- Yedekleme ayarları
- Sistem bakımı

---

### 9. Admin (`/admin`)

#### 9.1 Users (`/admin/users`)
- Kullanıcı listesi
- Kullanıcı ekleme/düzenleme
- Aktif/Pasif durumu

#### 9.2 Roles (`/admin/roles`)
- Rol tanımları
- İzin matrisi
- Varsayılan roller

#### 9.3 Audit Log (`/admin/audit-log`)
- Tüm sistem değişiklikleri
- Güvenlik olayları
- Dışa aktarma

#### 9.4 Licenses (`/admin/licenses`)
- Lisans durumu
- Modül aktivasyonu
- Yenileme bilgileri

---

### 10. Profile (`/profile`)

#### 10.1 Account (`/profile/account`)
- Kullanıcı bilgileri
- Şifre değiştirme
- 2FA ayarları

#### 10.2 Preferences (`/profile/preferences`)
- Dashboard düzeni
- Varsayılan görünümler
- Kısayollar

---

## Navigasyon Yapısı

### Ana Menü (Sidebar)

```
┌─────────────────────────┐
│  [Logo] EMS             │
├─────────────────────────┤
│  📊 Dashboard           │
│  📡 Monitoring      ▶   │
│  ⚡ Assets          ▶   │
│  📈 Analytics       ▶   │
│  🔔 Alarms (3)          │
│  📋 Events              │
│  🎛️ Control         ▶   │
├─────────────────────────┤
│  ⚙️ Settings        ▶   │
│  👤 Admin           ▶   │
├─────────────────────────┤
│  [User Avatar]          │
│  Cemre Ceyhan           │
│  Profile | Logout       │
└─────────────────────────┘
```

### Üst Bar (Header)

```
┌────────────────────────────────────────────────────────────┐
│ [Breadcrumb]              🔍 Search    🔔 3    👤 Profile  │
└────────────────────────────────────────────────────────────┘
```

---

## Erişim Seviyeleri

| Rol | Dashboard | Monitoring | Assets | Analytics | Alarms | Control | Settings | Admin |
|-----|:---------:|:----------:|:------:|:---------:|:------:|:-------:|:--------:|:-----:|
| Viewer | ✅ | ✅ | 👁️ | ✅ | 👁️ | ❌ | ❌ | ❌ |
| Operator | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ |
| Engineer | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ⚙️ | ❌ |
| Admin | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |

**Açıklama:** ✅ Tam erişim | 👁️ Sadece görüntüleme | ⚙️ Kısıtlı | ❌ Erişim yok

---

## URL Yapısı Kuralları

1. **Küçük harf ve tire:** `/energy-production` ✅ `/EnergyProduction` ❌
2. **Tekil isimler için ID:** `/assets/[asset-id]`
3. **Çoğul isimler için listeler:** `/assets/solar`
4. **Nested maksimum 3 seviye:** `/analytics/performance/details` ✅
5. **Query parametreleri filtreleme için:** `/alarms/active?severity=critical&source=battery`

---

## Sonraki Adımlar

- [ ] Her sayfa için wireframe tasarımları
- [ ] Component listesi çıkarılması
- [ ] API endpoint mapping
- [ ] Responsive breakpoint tanımları
