# UI States - Monitoring Pages

Monitoring sayfalarındaki elementlerin farklı durumlardaki görünümleri.

---

## Real-time Connection States

### Connected State

```
┌─────────────────────────────────────────────────────────────┐
│ ● Connected | Last update: 14:32:05 | Next refresh in 5s    │
└─────────────────────────────────────────────────────────────┘
```

**Style:**
```css
.connection-status.connected {
  color: var(--energy-solar);
}
.connection-indicator.connected {
  background-color: var(--energy-solar);
  animation: pulse 2s infinite;
}
```

### Connecting State

```
┌─────────────────────────────────────────────────────────────┐
│ ◌ Connecting... | Attempting to establish connection        │
└─────────────────────────────────────────────────────────────┘
```

**Style:**
```css
.connection-status.connecting {
  color: var(--energy-consumption);
}
.connection-indicator.connecting {
  animation: spin 1s linear infinite;
}
```

### Disconnected State

```
┌─────────────────────────────────────────────────────────────┐
│ ● Disconnected | Last data: 14:27:05 (5 min ago)           │
│ [Reconnect Now]                                             │
└─────────────────────────────────────────────────────────────┘
```

**Style:**
```css
.connection-status.disconnected {
  color: var(--energy-grid);
  font-weight: 600;
}
.connection-banner.disconnected {
  background-color: var(--energy-grid/10);
  border: 1px solid var(--energy-grid/30);
}
```

### Stale Data Warning

```
┌─────────────────────────────────────────────────────────────┐
│ [!] Data may be outdated | Last update: 2 minutes ago      │
│     Refresh failed. [Retry] [Use Cached]                   │
└─────────────────────────────────────────────────────────────┘
```

---

## Data Loading States

### Initial Loading (Skeleton)

```
┌─────────────────────────────────┐
│ LIVE DATA                       │
├─────────────────────────────────┤
│  ░░░░░░░░   │  ░░░░░   │  ░░░░ │
│  ░░░░░░░░   │  ░░░░░   │  ░░░░ │
│  ░░░░░░░░   │  ░░░░░   │  ░░░░ │
│  ░░░░░░░░   │  ░░░░░   │  ░░░░ │
│  ░░░░░░░░   │  ░░░░░   │  ░░░░ │
└─────────────────────────────────┘
```

### Refresh Loading (Spinner Overlay)

```
┌─────────────────────────────────┐
│ LIVE DATA            ◌ Loading │
├─────────────────────────────────┤
│  PV-01     │  125 kW  │  ↑     │
│  PV-02     │  118 kW  │  →     │
│  BESS-01   │  75%     │  ↑     │
│  (Opacity: 70% during refresh)  │
└─────────────────────────────────┘
```

### Partial Loading

```
┌─────────────────────────────────┐
│ LIVE DATA                       │
├─────────────────────────────────┤
│  PV-01     │  125 kW  │  ↑     │
│  PV-02     │  118 kW  │  →     │
│  BESS-01   │  ░░░░░░  │  ...   │  ← Loading
│  Grid      │  275 kW  │  ↓     │
└─────────────────────────────────┘
```

---

## Value Update States

### Value Changed (Highlight Flash)

```css
.value-cell.updated {
  animation: value-flash 0.5s ease;
}

@keyframes value-flash {
  0% {
    background-color: var(--energy-solar/30);
    transform: scale(1.02);
  }
  100% {
    background-color: transparent;
    transform: scale(1);
  }
}
```

### Value Increased

```
┌──────────────┐
│  127.5 kW ↑  │  ← Yeşil ok, değer yeşil flash
│  (+2.1)      │
└──────────────┘
```

### Value Decreased

```
┌──────────────┐
│  123.4 kW ↓  │  ← Kırmızı ok, değer kırmızı flash
│  (-1.5)      │
└──────────────┘
```

### Value Stable

```
┌──────────────┐
│  125.0 kW →  │  ← Gri ok, flash yok
└──────────────┘
```

---

## Asset Status States

### Online (Normal)

```
┌─────────────────────────┐
│ PV-01           ● Online│
│ ─────────────────────── │
│ Power: 125.4 kW         │
│ Efficiency: 94.2%       │
│ Status: Generating      │
└─────────────────────────┘
```

### Warning

```
┌─────────────────────────┐
│ PV-02          ● Warning│  ← Turuncu indicator
│ ─────────────────────── │
│ Power: 85.0 kW          │
│ Efficiency: 78.5%       │  ← Kırmızı text
│ [!] Low efficiency      │  ← Uyarı mesajı
│ [View Details]          │
└─────────────────────────┘
```

### Offline

```
┌─────────────────────────┐
│ PV-03          ● Offline│  ← Kırmızı indicator
│ ─────────────────────── │
│ Last seen: 14:15:30     │
│ Duration: 17 min        │
│                         │
│ [Diagnose] [Reconnect]  │
└─────────────────────────┘
```

### Maintenance Mode

```
┌─────────────────────────┐
│ BESS-02    🔧 Maintenance│  ← Mor indicator
│ ─────────────────────── │
│ Scheduled maintenance   │
│ Est. completion: 16:00  │
│                         │
│ [View Schedule]         │
└─────────────────────────┘
```

---

## Power Flow Diagram States

### Normal Flow

```
Solar ──────────→ Bus ──────────→ Load
      373 kW          450 kW
```
- Düz çizgiler, yeşil animasyon

### Reverse Flow (Grid Export)

```
Solar ──────────→ Bus ──────────→ Grid
      500 kW          +50 kW (export)
```
- Grid çizgisi ters yönde, yeşil renk

### No Flow (Idle)

```
Solar - - - - - - Bus - - - - - - Load
        0 kW            0 kW
```
- Kesik çizgi, gri renk, animasyon yok

### Critical Flow (Overload)

```
Solar ══════════→ Bus ══════════→ Load
      373 kW      ⚠️  520 kW (overload)
```
- Kalın çizgi, kırmızı yanıp sönme

---

## Table States

### Empty State

```
┌─────────────────────────────────────────────────────────────┐
│ LIVE DATA                                                   │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│              No data available                              │
│                                                             │
│        Data collection has not started yet.                 │
│        Please ensure devices are connected.                 │
│                                                             │
│              [Check Connections]                            │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### Filtered Empty State

```
┌─────────────────────────────────────────────────────────────┐
│ LIVE DATA                                                   │
│ Filter: Source = "PV-04"                                    │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│              No matching data                               │
│                                                             │
│        No parameters match your filter criteria.            │
│                                                             │
│              [Clear Filters]                                │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### Row Hover State

```css
.data-row:hover {
  background-color: var(--muted);
  cursor: pointer;
}
```

### Row Selected State

```css
.data-row.selected {
  background-color: var(--accent);
  border-left: 3px solid var(--energy-battery);
}
```

### Row Error State

```
│  BESS-01   │  Error   │  -     │  --:--  │  ← Kırmızı satır
│  [!] Communication timeout                │
```

---

## Filter States

### Default (All Selected)

```
┌────────────────────────────────────────────────────┐
│ Source: [All Sources ▼]  Type: [All Types ▼]      │
└────────────────────────────────────────────────────┘
```

### Filtered

```
┌────────────────────────────────────────────────────┐
│ Source: [PV Systems ▼]  Type: [Power ▼]           │
│ ┌──────────────────────────────────────────────┐  │
│ │ Active filters: PV Systems, Power  [Clear All]│  │
│ └──────────────────────────────────────────────┘  │
└────────────────────────────────────────────────────┘
```

### Filter Dropdown Open

```
┌────────────────────────────────────────────────────┐
│ Source: [PV Systems ▼]                             │
│         ┌─────────────────┐                        │
│         │ ☑ All Sources   │                        │
│         │ ☑ PV Systems    │                        │
│         │ ☐ BESS          │                        │
│         │ ☐ Grid          │                        │
│         │ ☐ Inverters     │                        │
│         └─────────────────┘                        │
└────────────────────────────────────────────────────┘
```

---

## Refresh Control States

### Auto Refresh Active

```
┌─────────────────────────┐
│ ⟳ Auto: 5s  [||]       │  ← Pause butonu görünür
│ Next refresh in 3s      │
└─────────────────────────┘
```

### Auto Refresh Paused

```
┌─────────────────────────┐
│ ⟳ Paused   [▶]         │  ← Resume butonu görünür
│ Click to resume         │
└─────────────────────────┘
```

### Manual Refresh

```
┌─────────────────────────┐
│ ⟳ Manual   [Refresh]   │
│ Last: 14:32:05          │
└─────────────────────────┘
```

### Refresh In Progress

```
┌─────────────────────────┐
│ ◌ Refreshing...         │  ← Spinner animasyonu
└─────────────────────────┘
```

---

## Error States

### Connection Error

```
┌─────────────────────────────────────────────────────────────┐
│ [!] Unable to connect to data server                        │
│                                                             │
│     Error: Connection timeout after 30 seconds              │
│     Last successful connection: 14:27:05                    │
│                                                             │
│     [Retry Connection]  [Use Cached Data]  [Report Issue]   │
└─────────────────────────────────────────────────────────────┘
```

### Partial Data Error

```
┌─────────────────────────────────────────────────────────────┐
│ [!] Some data unavailable                                   │
│                                                             │
│     3 of 48 devices are not responding:                     │
│     • PV-03 (timeout)                                       │
│     • BESS-02 (communication error)                         │
│     • INV-05 (offline)                                      │
│                                                             │
│     [Retry Failed]  [Continue Without]                      │
└─────────────────────────────────────────────────────────────┘
```

### Permission Error

```
┌─────────────────────────────────────────────────────────────┐
│ [!] Access Denied                                           │
│                                                             │
│     You don't have permission to view this data.            │
│     Required role: Operator or higher                       │
│                                                             │
│     [Request Access]  [Go Back]                             │
└─────────────────────────────────────────────────────────────┘
```

---

## Tooltip States

### Data Point Tooltip

```
┌─────────────────────────┐
│ Active Power            │
│ ───────────────         │
│ Current: 125.4 kW       │
│ Min: 0 kW               │
│ Max: 150 kW             │
│ Avg: 98.2 kW            │
│ ───────────────         │
│ Updated: 14:32:05       │
│ Quality: Good           │
└─────────────────────────┘
```

### Trend Tooltip

```
┌─────────────────────────┐
│ Trend: Increasing       │
│ ───────────────         │
│ Change: +2.1 kW         │
│ Rate: +1.7%/min         │
│ Since: 14:30:00         │
└─────────────────────────┘
```
