# Customization - EMS Dashboard

Dashboard özelleştirme özellikleri.

---

## Widget Customization Panel

```
┌─────────────────────────────────────────────────────────────┐
│  CUSTOMIZE DASHBOARD                                   [×]  │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  AVAILABLE WIDGETS                 YOUR DASHBOARD           │
│  ──────────────────               ───────────────           │
│  [ ] Generator Status             [x] System Health         │
│  [ ] Maintenance Schedule         [x] Active Alarms         │
│  [ ] Energy Forecast              [x] Power Flow            │
│  [ ] Comparison Charts            [x] BESS Status           │
│  [ ] Historical Trends            [x] Solar Status          │
│  [ ] Load Breakdown               [x] Grid Status           │
│  [ ] Tariff Schedule              [x] Weather               │
│  [ ] Carbon Tracking              [x] Financial             │
│                                   [x] Energy Today          │
│                                   [x] KPI Strip             │
│                                   [x] Quick Actions         │
│                                                             │
│  LAYOUT                                                     │
│  ──────                                                     │
│  ( ) Compact     (•) Standard     ( ) Expanded              │
│                                                             │
│  THEME                                                      │
│  ─────                                                      │
│  (•) Light       ( ) Dark         ( ) System                │
│                                                             │
│  [Save Layout]  [Reset to Default]  [Share Layout]          │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## Drag & Drop Reordering

### Edit Mode

```
┌─────────────────────────────────────────────────────────────────────┐
│  EDIT MODE                                          [Done Editing]  │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  ┌─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ┐  ┌─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ┐              │
│  │ [≡] SYSTEM HEALTH [×]│  │ [≡] ACTIVE ALARMS [×] │              │
│  │                      │  │                       │              │
│  │  Drag to move        │  │  Drag to move         │              │
│  │                      │  │                       │              │
│  └ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─┘  └ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─┘              │
│                                                                     │
│  ┌ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ┐ │
│  │ [≡] POWER FLOW                                           [×] │ │
│  │                                                               │ │
│  │  Drag to move                                                 │ │
│  │                                                               │ │
│  └ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ┘ │
│                                                                     │
│  ┌ ─ ─ ─ ─ ─ ┐  ┌ ─ ─ ─ ─ ─ ┐  ┌ ─ ─ ─ ─ ─ ┐  ┌ ─ ─ ─ ─ ─ ┐     │
│  │ [+] Add   │  │           │  │           │  │           │     │
│  │  Widget   │  │           │  │           │  │           │     │
│  └ ─ ─ ─ ─ ─ ┘  └ ─ ─ ─ ─ ─ ┘  └ ─ ─ ─ ─ ─ ┘  └ ─ ─ ─ ─ ─ ┘     │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

### Drag Visual Feedback

```
Dragging:
┌─────────────────────────┐
│ [≡] BESS STATUS     [×] │  ← Semi-transparent
│                         │    Elevated shadow
│                         │    Cursor: grabbing
└─────────────────────────┘

Drop Zone:
┌ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ┐
│                          │  ← Dashed border
│   Drop here              │    Highlighted background
│                          │
└ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ┘
```

---

## Widget Settings

Her widget'ın kendi ayarları:

```
┌─────────────────────────────────────────────────────────────┐
│  BESS STATUS - Settings                                [×]  │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  DISPLAY OPTIONS                                            │
│  ───────────────                                            │
│  [x] Show temperature                                       │
│  [x] Show voltage                                           │
│  [x] Show current                                           │
│  [ ] Show cycle count                                       │
│  [x] Show daily statistics                                  │
│                                                             │
│  ALERTS                                                     │
│  ──────                                                     │
│  Low SOC warning at: [__20__] %                             │
│  High temp warning at: [__45__] °C                          │
│                                                             │
│  REFRESH RATE                                               │
│  ────────────                                               │
│  (•) Real-time   ( ) 5 sec   ( ) 30 sec   ( ) Manual        │
│                                                             │
│  [Cancel]                              [Save Settings]      │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## Layout Presets

### Preset Selection

```
┌─────────────────────────────────────────────────────────────┐
│  LAYOUT PRESETS                                             │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │              │  │              │  │              │      │
│  │   Default    │  │   Operator   │  │   Manager    │      │
│  │              │  │              │  │              │      │
│  │  ┌──┐ ┌──┐   │  │  ┌────────┐  │  │  ┌──┐ ┌──┐   │      │
│  │  └──┘ └──┘   │  │  │ Power  │  │  │  └──┘ └──┘   │      │
│  │  ┌────────┐  │  │  │  Flow  │  │  │  ┌──┐ ┌──┐   │      │
│  │  └────────┘  │  │  └────────┘  │  │  └──┘ └──┘   │      │
│  │              │  │              │  │              │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
│       [Apply]           [Apply]           [Apply]          │
│                                                             │
│  ┌──────────────┐  ┌──────────────┐                        │
│  │              │  │              │                        │
│  │   Minimal    │  │    Custom    │                        │
│  │              │  │              │                        │
│  │  ┌──┐ ┌──┐   │  │  ┌──────┐    │                        │
│  │  └──┘ └──┘   │  │  │ Your │    │                        │
│  │              │  │  │Layout│    │                        │
│  │              │  │  └──────┘    │                        │
│  │              │  │              │                        │
│  └──────────────┘  └──────────────┘                        │
│       [Apply]        [Edit] [Apply]                        │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### Preset Descriptions

| Preset | Focus | Widgets |
|--------|-------|---------|
| Default | Balanced | All widgets |
| Operator | Real-time monitoring | Power Flow, Alarms, Status |
| Manager | Financial overview | Financial, KPIs, Energy |
| Minimal | Essential only | Health, Alarms, Power |

---

## Sharing Layouts

### Export Layout

```
┌─────────────────────────────────────────────────────────────┐
│  SHARE LAYOUT                                          [×]  │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Share your dashboard layout with team members              │
│                                                             │
│  EXPORT OPTIONS                                             │
│  ──────────────                                             │
│  ( ) Copy link to clipboard                                 │
│  (•) Download as file                                       │
│  ( ) Send via email                                         │
│                                                             │
│  INCLUDE                                                    │
│  ───────                                                    │
│  [x] Widget positions                                       │
│  [x] Widget settings                                        │
│  [ ] Color theme                                            │
│  [ ] Personal preferences                                   │
│                                                             │
│  [Cancel]                              [Export]             │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### Import Layout

```
┌─────────────────────────────────────────────────────────────┐
│  IMPORT LAYOUT                                         [×]  │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Import a shared dashboard layout                           │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │                                                     │   │
│  │   Drag and drop layout file here                    │   │
│  │                                                     │   │
│  │   or                                                │   │
│  │                                                     │   │
│  │   [Browse Files]                                    │   │
│  │                                                     │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│  [Cancel]                              [Import]             │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## Persistence

### Storage Structure

```typescript
interface DashboardLayout {
  id: string;
  name: string;
  widgets: WidgetConfig[];
  theme: 'light' | 'dark' | 'system';
  density: 'compact' | 'standard' | 'expanded';
  createdAt: Date;
  updatedAt: Date;
}

interface WidgetConfig {
  id: string;
  type: string;
  position: { row: number; col: number };
  size: { width: number; height: number };
  settings: Record<string, unknown>;
  visible: boolean;
}
```

### Auto-save

- Layout değişiklikleri otomatik kaydedilir
- Debounce: 500ms
- Local storage + API sync
- Conflict resolution: Last write wins

### Reset Options

```
[Reset Widget] - Single widget to default
[Reset Layout] - All widgets to default positions
[Reset All]    - Layout + settings + preferences
```
