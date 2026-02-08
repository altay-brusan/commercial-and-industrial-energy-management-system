# Interactions - EMS Dashboard

Widget'lar ve elementlerle kullanıcı etkileşimleri.

---

## Widget Etkileşimleri

### Hover Interactions

| Element | Hover Effect | Content |
|---------|--------------|---------|
| Widget | Subtle shadow + lift | - |
| Status badge | Tooltip | Detailed status |
| Metric value | Tooltip | Historical comparison |
| Subsystem icon | Tooltip | Subsystem details |
| Chart area | Crosshair + values | Point data |

### Click Interactions

| Element | Click Action | Target |
|---------|--------------|--------|
| Widget header | Navigate | Detail page |
| "Details" link | Navigate | Detail page |
| Action button | Execute | Action + feedback |
| Alarm row | Navigate | Alarm detail |
| Chart | Navigate | Full chart view |

### Long Press (Mobile)

| Element | Long Press Action |
|---------|-------------------|
| Widget | Context menu |
| Metric | Copy value |
| Alarm | Quick acknowledge |

---

## Power Flow Diagram Interactions

### Node Interactions

```
Hover on Solar node:
┌─────────────────────────────┐
│ Solar Production            │
│ ─────────────────           │
│ Current: 125 kW             │
│ Peak Today: 142 kW          │
│ Efficiency: 94%             │
│                             │
│ [View Details]              │
└─────────────────────────────┘
```

### Flow Line Interactions

- **Hover**: Highlight path + show power value
- **Click**: Show energy flow history
- **Animated arrows**: Direction + magnitude

### Zoom/Pan

- **Scroll**: Zoom in/out
- **Drag**: Pan view
- **Double-click**: Reset view
- **Pinch (mobile)**: Zoom

---

## Alarm Widget Interactions

### Alarm Row Actions

```
┌─────────────────────────────────────────────────────────────┐
│ [CRIT] Grid voltage exceeded - Main Connection    [ACK] [→] │
└─────────────────────────────────────────────────────────────┘
       ↓ Hover                   ↓ Click  ↓ Click
    Show timestamp             Acknowledge  Navigate
    and details                  alarm      to detail
```

### Bulk Actions

- **Select multiple**: Checkbox selection
- **ACK Selected**: Acknowledge selected alarms
- **Filter**: By priority, source, time

### Sound Control

- **Mute**: Silence new alarm sounds
- **Sound on**: Enable alarm sounds
- **Test**: Play test sound

---

## Chart Interactions

### Time Range Selection

```
[1H] [6H] [24H] [7D] [30D] [Custom]
```

### Zoom/Pan

- **Drag**: Select time range
- **Scroll**: Zoom in/out
- **Double-click**: Reset zoom

### Data Points

- **Hover**: Show value tooltip
- **Click**: Pin tooltip
- **Shift+Click**: Compare points

### Export

- **Right-click**: Export menu
- Options: PNG, CSV, PDF

---

## Form Interactions

### Quick Actions

```
┌──────────────────────────────────────────────────────────┐
│  CHARGE BESS                                              │
│  ─────────────                                            │
│                                                           │
│  Target SOC: [____80____] %                               │
│              └─ Slider: 20-100                            │
│                                                           │
│  Power Limit: [____50____] kW                             │
│              └─ Input with validation                     │
│                                                           │
│  [Cancel]                              [Start Charging]   │
│                                                           │
└──────────────────────────────────────────────────────────┘
```

### Validation Feedback

- **Real-time**: As user types
- **Error state**: Red border + message
- **Success state**: Green checkmark

---

## Gesture Support (Mobile)

| Gesture | Action |
|---------|--------|
| Swipe left | Next widget/page |
| Swipe right | Previous widget/page |
| Swipe down | Refresh data |
| Pull down | Pull-to-refresh |
| Pinch | Zoom charts |
| Long press | Context menu |

---

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `R` | Refresh dashboard |
| `A` | Go to alarms |
| `F` | Full screen mode |
| `S` | Open search |
| `1-9` | Navigate to widget |
| `?` | Show shortcuts help |
| `Esc` | Close modal/popup |
| `Tab` | Next focusable element |
| `Shift+Tab` | Previous element |
| `Enter` | Activate focused element |
| `Space` | Toggle/Select |

---

## Drag & Drop

### Widget Reordering

```
Hold widget header → Drag → Drop in new position

Visual feedback:
- Dragged widget: Elevated shadow, semi-transparent
- Drop zone: Highlighted border
- Invalid zone: Red border
```

### Dashboard Customization

```
┌─────────────────────────────────────┐
│  CUSTOMIZE MODE                     │
│                                     │
│  Drag widgets to reorder            │
│  Click × to remove                  │
│  Click + to add new                 │
│                                     │
│  [Save] [Cancel] [Reset Default]    │
└─────────────────────────────────────┘
```

---

## Real-time Updates

### Visual Feedback

- **New data**: Brief highlight/flash
- **Status change**: Smooth color transition
- **Value change**: Number animation (count up/down)

### Update Indicators

```
Last updated: 2 seconds ago    [Auto-refresh: ON]
             └─ Updates in real-time
```

### Stale Data Warning

```
┌─────────────────────────────────────────────────────────────┐
│  [!] Data is 5 minutes old. [Refresh Now]                   │
└─────────────────────────────────────────────────────────────┘
```
