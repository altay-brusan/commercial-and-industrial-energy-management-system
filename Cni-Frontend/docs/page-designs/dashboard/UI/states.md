# UI States - EMS Dashboard

Widget'ların farklı durumlardaki görünümleri.

---

## Loading States

### Skeleton Loading

```
┌─────────────────────────┐
│  ░░░░░░░░░░░░           │
│  ────────────────────── │
│                         │
│  ░░░░░░░░░░░░░░░░░      │
│  ░░░░░░░░░░             │
│  ░░░░░░░░░░░░░          │
│                         │
└─────────────────────────┘
```

### Spinner Loading

```
┌─────────────────────────┐
│  BESS STATUS            │
│  ────────────────────── │
│                         │
│     ◌ Loading...        │
│                         │
└─────────────────────────┘
```

### CSS Implementation

```css
.skeleton {
  background: linear-gradient(
    90deg,
    var(--muted) 25%,
    var(--muted-foreground/10) 50%,
    var(--muted) 75%
  );
  background-size: 200% 100%;
  animation: skeleton-loading 1.5s infinite;
}

@keyframes skeleton-loading {
  0% { background-position: 200% 0; }
  100% { background-position: -200% 0; }
}
```

---

## Error States

### Widget Error

```
┌─────────────────────────┐
│  BESS STATUS            │
│  ────────────────────── │
│                         │
│     [!] Unable to load  │
│     data from server    │
│                         │
│     [Retry]  [Report]   │
│                         │
└─────────────────────────┘
```

### Connection Lost Banner

```
┌─────────────────────────────────────────────────────────────┐
│  [!] Connection Lost - Displaying cached data (5 min old)   │
│  [Reconnecting...] [Use Offline Mode]                       │
└─────────────────────────────────────────────────────────────┘
```

### Partial Data Error

```
┌─────────────────────────┐
│  BESS STATUS            │
│  ────────────────────── │
│                         │
│  SOC: 75%               │
│  ████████░░             │
│                         │
│  [!] Temperature data   │
│      unavailable        │
│                         │
└─────────────────────────┘
```

---

## Empty States

### No Alarms

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

### No Data Available

```
┌─────────────────────────┐
│  ENERGY TODAY           │
│  ────────────────────── │
│                         │
│     No production data  │
│     available yet       │
│                         │
│     Data collection     │
│     starts at sunrise   │
│                         │
└─────────────────────────┘
```

---

## Hover States

### Widget Hover

```css
.widget:hover {
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  transform: translateY(-2px);
  transition: all 0.2s ease;
}
```

### Interactive Element Hover

```css
.widget-action:hover {
  background-color: var(--accent);
  cursor: pointer;
}
```

---

## Focus States

### Keyboard Focus

```css
.widget:focus-visible {
  outline: 2px solid var(--ring);
  outline-offset: 2px;
}
```

### Interactive Element Focus

```css
.button:focus-visible {
  ring: 2px;
  ring-color: var(--ring);
  ring-offset: 2px;
}
```

---

## Disabled States

### Disabled Widget

```
┌─────────────────────────┐
│  GENERATOR STATUS       │  ← Opacity: 50%
│  ────────────────────── │
│                         │
│     Not Available       │
│     (No generator       │
│      configured)        │
│                         │
└─────────────────────────┘
```

### Disabled Action Button

```css
.button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
  pointer-events: none;
}
```

---

## Animation States

### Status Change Animation

```css
.status-indicator {
  transition: background-color 0.3s ease;
}

.status-indicator.pulse {
  animation: status-pulse 2s infinite;
}

@keyframes status-pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.5; }
}
```

### Data Update Animation

```css
.data-value.updated {
  animation: data-flash 0.5s ease;
}

@keyframes data-flash {
  0% { background-color: var(--energy-solar/20); }
  100% { background-color: transparent; }
}
```

---

## Responsive State Changes

### Collapsed Widget (Mobile)

```
┌─────────────────────────┐
│  BESS: 75% [>]          │  ← Tap to expand
└─────────────────────────┘
```

### Expanded Widget (Mobile)

```
┌─────────────────────────┐
│  BESS STATUS        [v] │
│  ────────────────────── │
│  SOC: 75%               │
│  Charging: +50 kW       │
│  Temp: 32°C             │
└─────────────────────────┘
```
