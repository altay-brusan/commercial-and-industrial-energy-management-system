# System Health Status Widget

Tek bakışta tüm sistemin durumunu göstermek için tasarlanmış kritik widget.

---

## Görsel Tasarım

```
┌─────────────────────────────────────────────────────────────┐
│  SYSTEM HEALTH                                    [Details] │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│     ┌─────────────────────────────────────────────┐        │
│     │  ████████████████████████████████████████   │        │
│     │            ALL SYSTEMS OPERATIONAL          │        │
│     └─────────────────────────────────────────────┘        │
│                                                             │
│     BESS: Online    Solar: Online    Grid: Connected        │
│     HVAC: Normal    Comms: Good      PCS: Active            │
│                                                             │
│     Last Update: 2 seconds ago         Uptime: 99.97%      │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## Durum ve Renkler

| Durum | Renk | Bar Görünümü | Açıklama |
|-------|------|--------------|----------|
| Operational | Yeşil | ████████████ | Tüm sistemler çalışıyor |
| Warning | Sarı | ████████████ | Dikkat gerektiren durumlar var |
| Degraded | Turuncu | ████████████ | Bazı sistemler düşük performansta |
| Critical | Kırmızı | ████████████ | Kritik sorun, müdahale gerekli |
| Offline | Gri | ░░░░░░░░░░░░ | Sistem çevrimdışı |

---

## Subsystem Göstergeleri

Her subsystem için:
- Durum ikonu (renkli nokta)
- İsim
- Kısa durum açıklaması (hover'da)

### Subsystem Listesi

| Subsystem | Olası Durumlar |
|-----------|----------------|
| BESS | Online, Charging, Discharging, Fault, Offline |
| Solar | Online, Producing, Curtailed, Night, Fault |
| Grid | Connected, Islanded, Fault |
| PCS | Active, Standby, Fault |
| Comms | Good, Degraded, Lost |
| HVAC | Normal, Warning, Fault |

---

## İnteraktivite

### Hover
- Her subsystem için detay tooltip
- Health bar için score breakdown

### Click
- Widget header → Monitoring sayfası
- Subsystem → İlgili cihaz sayfası
- Details link → Detaylı sistem durumu

### Animation
- Durum değişikliğinde pulse efekti
- Health bar smooth transition

---

## Data Structure

```typescript
interface SystemHealthData {
  overallStatus: 'operational' | 'warning' | 'degraded' | 'critical' | 'offline';
  healthScore: number; // 0-100
  uptime: string;
  lastUpdate: Date;
  subsystems: Subsystem[];
}

interface Subsystem {
  id: string;
  name: string;
  status: 'online' | 'warning' | 'fault' | 'offline';
  details: string;
  lastUpdate: Date;
}
```

---

## Props

```typescript
interface SystemHealthProps {
  data: SystemHealthData;
  onSubsystemClick?: (subsystemId: string) => void;
  onDetailsClick?: () => void;
  showSubsystems?: boolean;
  compact?: boolean;
}
```

---

## Variants

### Default (Full)
Tüm bilgiler görünür: health bar, subsystems, footer

### Compact
Sadece health bar ve overall status

### Minimal (Mobile)
Sadece status badge ve kritik uyarı sayısı

---

## Responsive Behavior

| Breakpoint | Görünüm |
|------------|---------|
| Desktop | Full layout |
| Tablet | Full layout |
| Mobile | Compact: status + alarm count |

---

## Accessibility

- `role="status"` for health indicator
- `aria-live="polite"` for status changes
- Color + icon + text for each status
- Keyboard navigable subsystems
