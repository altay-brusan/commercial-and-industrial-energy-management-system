# Active Alarms Widget

Aktif alarmları öncelik sırasına göre gösteren kritik widget.

---

## Görsel Tasarım

```
┌─────────────────────────────────────────────────────────────┐
│  ACTIVE ALARMS                              [View All (12)] │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ CRITICAL (2)                                         │   │
│  ├─────────────────────────────────────────────────────┤   │
│  │ • BMU-03 Over Temperature - 58°C         [ACK] [->] │   │
│  │   10:45 AM - BESS Rack 1                            │   │
│  │ • Grid Frequency Deviation - 50.8 Hz     [ACK] [->] │   │
│  │   10:42 AM - Main Grid Connection                   │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ WARNING (5)                                          │   │
│  ├─────────────────────────────────────────────────────┤   │
│  │ • Inverter INV-02 Low Efficiency - 87%   [ACK] [->] │   │
│  │ • Solar Panel String 4 Underperforming   [ACK] [->] │   │
│  │ + 3 more warnings...                                │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│  Sound: ON    Auto-refresh: 5s    Escalation: ON           │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## Alarm Öncelik Seviyeleri

| Öncelik | Renk | Badge | Ses | Açıklama |
|---------|------|-------|-----|----------|
| Critical | Kırmızı | CRIT | Loud | Acil müdahale gerekli |
| High | Turuncu | HIGH | Medium | Hızlı aksiyon gerekli |
| Medium | Mor | MED | Soft | Dikkat gerekli |
| Low | Gri | LOW | None | Bilgilendirme |

---

## Alarm Row Yapısı

```
┌─────────────────────────────────────────────────────────────┐
│ [CRIT] Grid voltage exceeded upper limit        [ACK] [->] │
│        10:45:32 - Main Grid Connection - ALM-001           │
└─────────────────────────────────────────────────────────────┘
  │       │                    │             │      │
  │       │                    │             │      └─ Navigate to detail
  │       │                    │             └─ Acknowledge button
  │       │                    └─ Source info
  │       └─ Alarm message
  └─ Priority badge
```

---

## Aksiyon Butonları

| Buton | Aksiyon | Feedback |
|-------|---------|----------|
| ACK | Alarmı kabul et | Toast + satır soluklaşır |
| -> | Alarm detayına git | Navigate |
| View All | Alarm listesine git | Navigate |
| Mute | Sesi kapat | Icon change |

---

## Bulk Actions

- **Select All**: Tüm alarmları seç
- **ACK Selected**: Seçilenleri acknowledge et
- **ACK All**: Tümünü acknowledge et (confirmation required)

---

## İnteraktivite

### Hover
- Alarm satırı: Highlight
- ACK butonu: Tooltip "Acknowledge this alarm"

### Click
- ACK: Acknowledge alarm
- ->: Navigate to alarm detail
- Alarm row: Expand for more info

### Scroll
- ScrollArea ile maksimum 5 görünür
- Smooth scroll

---

## Data Structure

```typescript
interface Alarm {
  id: string;
  priority: 'critical' | 'high' | 'medium' | 'low';
  message: string;
  source: string;
  timestamp: Date;
  acknowledged: boolean;
  acknowledgedBy?: string;
  acknowledgedAt?: Date;
}

interface AlarmWidgetData {
  alarms: Alarm[];
  totalCount: number;
  criticalCount: number;
  highCount: number;
  mediumCount: number;
  lowCount: number;
  soundEnabled: boolean;
  autoRefreshInterval: number;
}
```

---

## Props

```typescript
interface ActiveAlarmsProps {
  data: AlarmWidgetData;
  maxVisible?: number;
  onAcknowledge?: (alarmId: string) => void;
  onAlarmClick?: (alarmId: string) => void;
  onViewAll?: () => void;
  onSoundToggle?: (enabled: boolean) => void;
}
```

---

## Empty State

```
┌─────────────────────────┐
│  ACTIVE ALARMS          │
│  ────────────────────── │
│                         │
│     [OK] All Clear!     │
│     No active alarms    │
│                         │
│     Last alarm was      │
│     3 days ago          │
│                         │
└─────────────────────────┘
```

---

## Responsive Behavior

| Breakpoint | Görünüm |
|------------|---------|
| Desktop | Full list with details |
| Tablet | Condensed rows |
| Mobile | Priority + message only, swipe to ACK |

---

## Sound Configuration

```typescript
interface AlarmSoundConfig {
  critical: { sound: 'alarm-critical.mp3', volume: 1.0, repeat: true };
  high: { sound: 'alarm-high.mp3', volume: 0.8, repeat: false };
  medium: { sound: 'alarm-medium.mp3', volume: 0.5, repeat: false };
  low: { sound: null, volume: 0, repeat: false };
}
```

---

## Accessibility

- `role="alert"` for critical alarms
- `aria-live="assertive"` for new alarms
- Focus management after ACK
- Keyboard shortcuts: `a` to ACK focused alarm
