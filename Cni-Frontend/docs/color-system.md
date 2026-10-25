# Color System

## Overview

The project uses two separate sets of CSS custom properties defined in `app/globals.css` with full light/dark mode support via `oklch()` color space. Colors are exposed as Tailwind utility classes.

---

## Energy Domain Colors

Used for **charts, power flow diagrams, and energy visualizations**. Not for severity or status.

| Variable | Color | Light Mode | Dark Mode | Tailwind Class |
|----------|-------|------------|-----------|----------------|
| `--energy-solar` | Amber | `oklch(0.82 0.17 86)` | `oklch(0.78 0.16 86)` | `bg-energy-solar`, `text-energy-solar` |
| `--energy-grid` | Blue | `oklch(0.55 0.22 260)` | `oklch(0.65 0.22 260)` | `bg-energy-grid`, `text-energy-grid` |
| `--energy-battery` | Teal | `oklch(0.65 0.17 150)` | `oklch(0.72 0.17 150)` | `bg-energy-battery`, `text-energy-battery` |
| `--energy-consumption` | Purple | `oklch(0.55 0.2 277)` | `oklch(0.65 0.2 277)` | `bg-energy-consumption`, `text-energy-consumption` |

### Usage Examples

```tsx
<div className="bg-energy-solar/10 text-energy-solar">Solar</div>
<div className="bg-energy-grid/10 text-energy-grid">Grid</div>
<div className="bg-energy-battery/10 text-energy-battery">Battery</div>
<div className="bg-energy-consumption/10 text-energy-consumption">Consumption</div>
```

---

## Severity Colors

Used for **alarm severity badges, alert indicators, and priority levels**.

| Variable | Color | Light Mode | Dark Mode | Tailwind Class |
|----------|-------|------------|-----------|----------------|
| `--severity-critical` | Red | `oklch(0.59 0.22 29)` | `oklch(0.65 0.22 29)` | `bg-severity-critical` |
| `--severity-high` | Orange | `oklch(0.75 0.18 55)` | `oklch(0.75 0.17 55)` | `bg-severity-high` |
| `--severity-medium` | Yellow | `oklch(0.82 0.17 85)` | `oklch(0.8 0.16 85)` | `bg-severity-medium` |
| `--severity-minor` | Purple | `oklch(0.65 0.18 295)` | `oklch(0.7 0.18 295)` | `bg-severity-minor` |

### Severity-to-Color Mapping

```
critical → severity-critical (red)
high     → severity-high (orange)
medium   → severity-medium (yellow)
low      → muted (gray)
```

---

## Status Colors

Used for **device status, system health, and operational state indicators**.

| Variable | Color | Alias | Tailwind Class |
|----------|-------|-------|----------------|
| `--status-operational` | Green | — | `bg-status-operational` |
| `--status-warning` | Orange | = severity-high | `bg-status-warning` |
| `--status-critical` | Red | = severity-critical | `bg-status-critical` |
| `--status-standby` | Yellow | — | `bg-status-standby` |
| `--status-offline` | Gray | = muted-foreground | `bg-status-offline` |

---

## Category Colors (Permission Matrix)

Used in the permission matrix and permission overview components to distinguish feature categories:

```typescript
const categoryColors: Record<FeatureCategory, string> = {
  Core: "bg-energy-grid/10 text-energy-grid border-energy-grid/30",
  Energy: "bg-energy-solar/10 text-energy-solar border-energy-solar/30",
  Intelligence: "bg-energy-consumption/10 text-energy-consumption border-energy-consumption/30",
  Management: "bg-energy-battery/10 text-energy-battery border-energy-battery/30",
};
```

---

## Fonts

| Font | Source | Usage | CSS Variable |
|------|--------|-------|-------------|
| Geist Sans | `next/font/google` | Body text, UI | `--font-geist-sans` |
| Geist Mono | `next/font/google` | Code, monospace | `--font-geist-mono` |
| Cocon | Local (`app/fonts/`) | Logo only | `--font-cocon` |
