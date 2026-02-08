# Color System - EMS Dashboard

Dashboard'da kullanılan renk sistemi ve anlamları.

---

## Enerji Kaynağı Renkleri

| Kaynak | Renk | CSS Variable | Kullanım |
|--------|------|--------------|----------|
| Solar | Yeşil | `--energy-solar` | Solar üretim göstergeleri |
| Grid | Kırmızı | `--energy-grid` | Şebeke göstergeleri |
| BESS | Mor | `--energy-battery` | Batarya göstergeleri |
| Consumption | Turuncu | `--energy-consumption` | Tüketim göstergeleri |

```css
:root {
  --energy-solar: oklch(0.723 0.219 142.5);
  --energy-grid: oklch(0.577 0.245 27.325);
  --energy-battery: oklch(0.623 0.214 259.815);
  --energy-consumption: oklch(0.705 0.213 47.604);
}
```

---

## Durum Renkleri

| Durum | Renk | CSS Variable | Kullanım |
|-------|------|--------------|----------|
| Operational | Yeşil | `--status-operational` | Sistem çalışıyor |
| Warning | Turuncu | `--status-warning` | Dikkat gerektiren durum |
| Critical | Kırmızı | `--status-critical` | Kritik sorun |
| Offline | Gri | `--status-offline` | Sistem çevrimdışı |

```css
:root {
  --status-operational: var(--energy-solar);
  --status-warning: var(--energy-consumption);
  --status-critical: var(--energy-grid);
  --status-offline: var(--muted-foreground);
}
```

---

## System Health Bar Renkleri

| Durum | Renk | Bar Görünümü | Açıklama |
|-------|------|--------------|----------|
| Operational | Yeşil | ████████████ | Tüm sistemler çalışıyor |
| Warning | Sarı | ████████████ | Dikkat gerektiren durumlar var |
| Degraded | Turuncu | ████████████ | Bazı sistemler düşük performansta |
| Critical | Kırmızı | ████████████ | Kritik sorun, müdahale gerekli |
| Offline | Gri | ░░░░░░░░░░░░ | Sistem çevrimdışı |

---

## SOC (State of Charge) Renkleri

| SOC Aralığı | Renk | Durum |
|-------------|------|-------|
| 80-100% | Yeşil | Optimal |
| 50-79% | Sarı | Normal |
| 20-49% | Turuncu | Düşük |
| 0-19% | Kırmızı | Kritik |

---

## Grid Bağlantı Kalitesi Renkleri

| Kalite | Renk | Koşullar |
|--------|------|----------|
| Excellent | Yeşil | Freq 49.9-50.1, V ±5%, PF >0.95 |
| Good | Açık Yeşil | Freq 49.8-50.2, V ±8%, PF >0.90 |
| Fair | Sarı | Küçük sapmalar |
| Poor | Turuncu | Önemli sapmalar |
| Critical | Kırmızı | Limit dışı değerler |

---

## Alarm Öncelik Renkleri

| Öncelik | Renk | Badge Stili |
|---------|------|-------------|
| Critical | Kırmızı | `bg-energy-grid text-white` |
| High | Turuncu | `bg-energy-consumption text-white` |
| Medium | Mor | `bg-energy-battery text-white` |
| Low | Gri | `bg-muted text-muted-foreground` |

---

## Trend Gösterge Renkleri

| Trend | Renk | Sembol |
|-------|------|--------|
| Artış (Pozitif) | Yeşil | ↑ veya + |
| Azalış (Negatif) | Kırmızı | ↓ veya - |
| Sabit | Gri | → veya = |

**Not**: Trend'in pozitif mi negatif mi olduğu context'e bağlıdır:
- Üretim artışı = Yeşil (iyi)
- Maliyet artışı = Kırmızı (kötü)
- Tüketim azalışı = Yeşil (iyi)

---

## Power Flow Diagram Renkleri

| Kaynak | Renk | Hex Eşdeğeri |
|--------|------|--------------|
| Solar | Sarı/Yeşil | `--energy-solar` |
| BESS | Mor | `--energy-battery` |
| Grid | Mavi/Kırmızı | `--energy-grid` |
| Load | Yeşil | `--energy-consumption` |
| Generator | Gri | `--muted-foreground` |

---

## Dark Mode Renkleri

Dark mode'da renkler daha parlak ve doygun olacak şekilde ayarlanır:

```css
.dark {
  --energy-solar: oklch(0.65 0.25 145);
  --energy-grid: oklch(0.704 0.191 22.216);
  --energy-battery: oklch(0.55 0.25 265);
  --energy-consumption: oklch(0.75 0.18 55);
}
```
