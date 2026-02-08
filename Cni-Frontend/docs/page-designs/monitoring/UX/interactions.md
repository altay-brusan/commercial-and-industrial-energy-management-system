# Interactions - Monitoring Pages

Monitoring sayfalarındaki kullanıcı etkileşimleri ve davranış kuralları.

---

## Real-time Data Interactions

### Auto-refresh Behavior

```
┌─────────────────────────────────────────────────────────────┐
│ Refresh Interval: [5s ▼]                                    │
│                                                             │
│ ● Auto-refresh active                                       │
│ ○ Paused (hover/interaction detected)                       │
│ ○ Disabled (manual mode)                                    │
└─────────────────────────────────────────────────────────────┘
```

**Refresh Interval Options:**
| Interval | Use Case |
|----------|----------|
| 1s | Critical monitoring, alarms |
| 5s | Standard monitoring (default) |
| 15s | Overview dashboards |
| 30s | Historical analysis |
| Manual | Deep analysis, comparisons |

**Auto-pause Rules:**
- Kullanıcı hover yaptığında 10s pause
- Modal/dropdown açıkken pause
- Focus input alanındayken pause
- Scroll sırasında pause

```typescript
const refreshBehavior = {
  defaultInterval: 5000,
  pauseOnHover: true,
  pauseOnInteraction: true,
  resumeAfter: 10000, // ms
  showCountdown: true,
};
```

---

## Data Table Interactions

### Row Selection

```
Single Click: Satırı seç (highlight)
┌─────────────────────────────────────────┐
│ ○ PV-01     │  125.4 kW │  ↑  │  14:32 │
│ ● PV-02     │   98.7 kW │  → │  14:32 │  ← Selected
│ ○ PV-03     │    0.0 kW │  -  │  14:32 │
└─────────────────────────────────────────┘

Ctrl + Click: Multi-select
┌─────────────────────────────────────────┐
│ ● PV-01     │  125.4 kW │  ↑  │  14:32 │  ← Selected
│ ○ PV-02     │   98.7 kW │  → │  14:32 │
│ ● PV-03     │    0.0 kW │  -  │  14:32 │  ← Selected
└─────────────────────────────────────────┘

Shift + Click: Range select
```

### Row Actions

```
Double Click: Detay sayfasına git
Hover (1s): Tooltip göster
Right Click: Context menu aç
```

### Column Sorting

```
Click header: Sort ascending
Click again: Sort descending
Click third time: Remove sort

┌───────────────────────────────────────────────────────┐
│ Source  │  Value ▲  │  Trend │  Time                 │
├───────────────────────────────────────────────────────┤
│ PV-03   │    0.0 kW │   -    │  14:32               │
│ PV-02   │   98.7 kW │   →   │  14:32               │
│ PV-01   │  125.4 kW │   ↑   │  14:32               │
└───────────────────────────────────────────────────────┘
```

### Column Resizing

```
Hover on column border → Resize cursor
Drag → Resize column
Double-click border → Auto-fit column
```

---

## Power Flow Diagram Interactions

### Node Interactions

```
Hover on node: Highlight + Show tooltip
┌─────────────────────────────────────────┐
│           ┌───────────┐                 │
│           │   SOLAR   │ ← Highlighted   │
│           │  373 kW   │                 │
│           └───────────┘                 │
│                 │                       │
│           ┌─────┴─────┐                 │
│           │ Power: 373 kW              │
│           │ Efficiency: 94.2%          │
│           │ Status: Generating         │
│           │ [View Details]             │
│           └───────────────┘            │
└─────────────────────────────────────────┘

Click on node: Navigate to asset detail
```

### Flow Line Interactions

```
Hover on flow line: Show flow details
┌─────────────────────────────────────────┐
│ Solar ════════════════> Bus             │
│          │                              │
│    ┌─────┴─────────────┐               │
│    │ Flow: 373 kW      │               │
│    │ Direction: →      │               │
│    │ Utilization: 78%  │               │
│    └───────────────────┘               │
└─────────────────────────────────────────┘
```

### Diagram Controls

```
┌─────────────────────────────────────────┐
│ [+] Zoom In                             │
│ [-] Zoom Out                            │
│ [⟲] Reset View                         │
│ [▢] Fullscreen                          │
│ [⚙] Settings                            │
└─────────────────────────────────────────┘
```

**Gesture Support:**
- Mouse wheel: Zoom
- Click + drag: Pan
- Pinch (touch): Zoom
- Double-click: Focus on node

---

## Filter Interactions

### Quick Filters

```
┌─────────────────────────────────────────────────────────┐
│ [All] [Solar] [BESS] [Grid] [Inverters]   [Clear All]  │
└─────────────────────────────────────────────────────────┘

Click: Toggle filter
Active filter has filled background
Multiple can be active simultaneously
```

### Advanced Filters

```
┌─────────────────────────────────────────────────────────┐
│ Source: [Select Sources ▼]                              │
│         ┌─────────────────────────────────────────────┐ │
│         │ ☑ Select All                                │ │
│         │ ───────────────────────────────────────     │ │
│         │ ☑ PV Systems (3)                            │ │
│         │ ☐ BESS (2)                                  │ │
│         │ ☑ Grid (1)                                  │ │
│         │ ☐ Inverters (6)                             │ │
│         │ ───────────────────────────────────────     │ │
│         │ [Apply]  [Cancel]                           │ │
│         └─────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────┘
```

### Search Filter

```
┌─────────────────────────────────────────────────────────┐
│ Search: [__________|                                    │
│                                                         │
│ Type to filter parameters by name...                    │
│ Minimum 2 characters                                    │
│                                                         │
│ Recent: [Power] [Voltage] [Temperature]                 │
└─────────────────────────────────────────────────────────┘

Debounce: 300ms after typing stops
Clear on Escape key
```

### Filter Persistence

```typescript
const filterPersistence = {
  saveToURL: true,      // Shareable links
  saveToSession: true,  // Tab persistence
  saveToLocal: false,   // Don't persist across sessions
};

// URL example:
// /monitoring/live-data?source=pv,grid&type=power&refresh=5s
```

---

## Tooltip Interactions

### Hover Tooltip

```
Delay: 500ms before show
Duration: Show while hovering
Dismiss: On mouse leave
Position: Smart positioning (avoid edges)
```

### Rich Tooltip Content

```
┌───────────────────────────────┐
│ Active Power                  │
│ ─────────────────             │
│ Current: 125.4 kW             │
│ Min (24h): 0 kW               │
│ Max (24h): 148.2 kW           │
│ Average: 87.3 kW              │
│ ─────────────────             │
│ [📊 View History] [⚙ Configure]│
└───────────────────────────────┘

Clickable links inside tooltip
Tooltip stays on hover over tooltip itself
```

---

## Modal/Dialog Interactions

### Detail Modal

```
┌──────────────────────────────────────────────────────────┐
│ PV-01 Details                                     [×]    │
├──────────────────────────────────────────────────────────┤
│                                                          │
│ [Overview] [Live Data] [History] [Configuration]         │
│                                                          │
│ ┌────────────────────────────────────────────────────┐  │
│ │  Tab content here                                  │  │
│ └────────────────────────────────────────────────────┘  │
│                                                          │
│                              [Close]  [Open Full Page]   │
└──────────────────────────────────────────────────────────┘

Open: Double-click row or click "View Details"
Close: Click ×, press Escape, click outside
Navigate: Tab keys for internal navigation
```

### Confirmation Modal

```
┌──────────────────────────────────────────────────────────┐
│ Confirm Action                                     [×]   │
├──────────────────────────────────────────────────────────┤
│                                                          │
│ Are you sure you want to reset all filters?              │
│                                                          │
│ This will clear:                                         │
│ • Source filters (3 active)                              │
│ • Type filters (2 active)                                │
│ • Search query                                           │
│                                                          │
│                              [Cancel]  [Reset Filters]   │
└──────────────────────────────────────────────────────────┘
```

---

## Keyboard Shortcuts

### Global Shortcuts

| Shortcut | Action |
|----------|--------|
| `R` | Manual refresh |
| `P` | Pause/resume auto-refresh |
| `F` | Focus search/filter |
| `Escape` | Close modal/clear selection |
| `?` | Show keyboard shortcuts |

### Table Shortcuts

| Shortcut | Action |
|----------|--------|
| `↑` / `↓` | Navigate rows |
| `Enter` | Open selected row details |
| `Space` | Toggle row selection |
| `Ctrl + A` | Select all |
| `Ctrl + C` | Copy selected data |

### Navigation Shortcuts

| Shortcut | Action |
|----------|--------|
| `G` then `O` | Go to Overview |
| `G` then `L` | Go to Live Data |
| `G` then `P` | Go to Power Flow |
| `G` then `G` | Go to Grid Status |

---

## Touch Interactions (Tablet/Mobile)

### Gestures

| Gesture | Action |
|---------|--------|
| Tap | Select/Activate |
| Double-tap | Open details |
| Long-press | Context menu |
| Swipe left | Show actions |
| Swipe down | Refresh (pull-to-refresh) |
| Pinch | Zoom (power flow diagram) |

### Mobile-specific Adaptations

```
┌─────────────────────────────────┐
│ Swipe for actions               │
│                                 │
│ ┌───────────────────────┬─────┐│
│ │ PV-01      125.4 kW   │ [>] ││ ← Swipe reveals
│ └───────────────────────┴─────┘│
│                     ┌─────────┐│
│                     │ Details ││
│                     │ History ││
│                     │ Config  ││
│                     └─────────┘│
└─────────────────────────────────┘
```

---

## Animation Guidelines

### Data Update Animation

```css
/* Value change animation */
.value-changed {
  animation: pulse-highlight 0.3s ease;
}

@keyframes pulse-highlight {
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

### State Transition Animation

```css
/* Status change */
.status-indicator {
  transition: background-color 0.3s ease,
              transform 0.2s ease;
}

/* Expand/collapse */
.collapsible-content {
  transition: max-height 0.3s ease,
              opacity 0.2s ease;
}
```

### Performance Notes

- Animasyonlar `transform` ve `opacity` kullanmalı
- `will-change` sadece gerçekten animated elements için
- Reduced motion preference'a saygı göster

```css
@media (prefers-reduced-motion: reduce) {
  * {
    animation-duration: 0.01ms !important;
    transition-duration: 0.01ms !important;
  }
}
```
