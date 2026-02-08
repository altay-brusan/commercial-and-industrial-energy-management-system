# Visual Hierarchy - EMS Dashboard

Dashboard'daki widget'ların görsel öncelik sıralaması.

## Tier Sistemi

```
┌────────────────────────────────────────────────────────────────────────┐
│ TIER 1: Critical (En üst, en büyük)                                    │
│ • System Status                                                         │
│ • Active Alarms                                                         │
│ • Power Flow                                                            │
├────────────────────────────────────────────────────────────────────────┤
│ TIER 2: Important (Orta boy)                                           │
│ • BESS Status                                                           │
│ • Solar Status                                                          │
│ • Grid Status                                                           │
│ • Financial Summary                                                     │
├────────────────────────────────────────────────────────────────────────┤
│ TIER 3: Informational (Daha küçük)                                     │
│ • Charts & Trends                                                       │
│ • Weather                                                               │
│ • KPIs                                                                  │
└────────────────────────────────────────────────────────────────────────┘
```

## Tier Özellikleri

| Tier | Konum | Boyut | İçerik Tipi |
|------|-------|-------|-------------|
| Tier 1 | Üst | Büyük (2 kolon) | Kritik durum bilgisi |
| Tier 2 | Orta | Orta (1 kolon) | Detaylı sistem durumları |
| Tier 3 | Alt | Küçük/Geniş | Trend ve özet bilgiler |

## Grid Yapısı

- **Desktop**: 4 kolon grid
- **Tablet**: 2 kolon grid
- **Mobile**: 1 kolon grid

## Spacing

- Widget arası gap: `16px` (md: `24px`)
- İç padding: `16px` (md: `24px`)
- Section arası margin: `24px` (md: `32px`)
