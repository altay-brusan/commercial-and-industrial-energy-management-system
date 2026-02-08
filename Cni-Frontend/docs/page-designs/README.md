# EMS Dashboard - Design Documentation

Bu klasör, EMS (Energy Management System) Dashboard tasarım dökümanlarını içerir.

---

## Klasör Yapısı

```
docs/page-designs/
├── README.md           # Bu dosya
├── UI/                 # Görsel tasarım dökümanları
│   ├── visual-hierarchy.md
│   ├── layouts.md
│   ├── color-system.md
│   └── states.md
├── UX/                 # Kullanıcı deneyimi dökümanları
│   ├── design-principles.md
│   ├── interactions.md
│   ├── accessibility.md
│   ├── onboarding.md
│   ├── customization.md
│   └── performance.md
└── components/         # Widget dökümanları
    ├── system-health.md
    ├── active-alarms.md
    ├── power-flow.md
    ├── bess-status.md
    ├── solar-status.md
    ├── grid-status.md
    ├── weather-widget.md
    ├── financial-summary.md
    ├── energy-today.md
    ├── kpi-strip.md
    └── quick-actions.md
```

---

## UI Dökümanları

| Dosya | İçerik |
|-------|--------|
| `visual-hierarchy.md` | Widget tier sistemi, grid yapısı, spacing |
| `layouts.md` | Desktop, tablet, mobile layout'ları |
| `color-system.md` | Renk kodları, enerji renkleri, durum renkleri |
| `states.md` | Loading, error, empty, hover, focus states |

---

## UX Dökümanları

| Dosya | İçerik |
|-------|--------|
| `design-principles.md` | Glanceable, Progressive Disclosure, Action-Oriented |
| `interactions.md` | Hover, click, gesture, keyboard interactions |
| `accessibility.md` | WCAG 2.1 AA, screen reader, keyboard navigation |
| `onboarding.md` | Tour steps, contextual help, first-time experience |
| `customization.md` | Widget reordering, settings, layout presets |
| `performance.md` | Performance metrics, optimization, caching |

---

## Component Dökümanları

### Tier 1: Critical
| Component | Dosya |
|-----------|-------|
| System Health | `system-health.md` |
| Active Alarms | `active-alarms.md` |
| Power Flow | `power-flow.md` |

### Tier 2: Important
| Component | Dosya |
|-----------|-------|
| BESS Status | `bess-status.md` |
| Solar Status | `solar-status.md` |
| Grid Status | `grid-status.md` |
| Weather | `weather-widget.md` |

### Tier 3: Informational
| Component | Dosya |
|-----------|-------|
| Energy Today | `energy-today.md` |
| Financial Summary | `financial-summary.md` |
| KPI Strip | `kpi-strip.md` |
| Quick Actions | `quick-actions.md` |

---

## Tasarım Prensipleri Özeti

1. **Glanceable**: 3 saniyede genel durumu anlama
2. **Progressive Disclosure**: Özet → Detay → Sayfa
3. **Action-Oriented**: Her widget aksiyon önerebilir
4. **Consistency**: Renk, layout, interaction tutarlılığı
5. **Accessibility**: WCAG 2.1 AA uyumlu

---

## Renk Sistemi Özeti

| Kaynak | Renk | Kullanım |
|--------|------|----------|
| Solar | Yeşil | Üretim, pozitif |
| Grid | Kırmızı | Şebeke, maliyet |
| Battery | Mor | Depolama |
| Consumption | Turuncu | Tüketim, uyarı |

---

## Responsive Breakpoints

| Breakpoint | Min Width | Layout |
|------------|-----------|--------|
| Mobile | 0px | 1 column |
| Tablet | 768px | 2 columns |
| Desktop | 1024px | 4 columns |

---

## Implementation Status

| Component | Documented | Implemented |
|-----------|------------|-------------|
| System Health | Yes | Yes |
| Active Alarms | Yes | Yes |
| Power Flow | Yes | No (planned) |
| BESS Status | Yes | Yes |
| Solar Status | Yes | Yes |
| Grid Status | Yes | Yes |
| Weather | Yes | Yes |
| Financial Summary | Yes | Yes |
| Energy Today | Yes | Yes |
| KPI Strip | Yes | Yes |
| Quick Actions | Yes | Yes |

---

*Son güncelleme: 2024*
