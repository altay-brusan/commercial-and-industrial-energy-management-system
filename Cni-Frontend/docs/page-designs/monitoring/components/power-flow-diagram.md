# Power Flow Diagram Component

Monitoring Power Flow sayfasındaki interaktif güç akış diyagramı.

---

## Genel Bakış

Power Flow Diagram, sistemdeki enerji akışını görsel olarak gösterir. Kaynaklar, depolama, yükler ve şebeke arasındaki güç transferini animasyonlu çizgilerle temsil eder.

---

## Component Yapısı

```
┌────────────────────────────────────────────────────────────────────────────┐
│                         POWER FLOW DIAGRAM                                 │
│                                                              [⟲] [▢] [⚙]  │
├────────────────────────────────────────────────────────────────────────────┤
│                                                                            │
│     ┌───────────┐                                  ┌───────────┐          │
│     │   SOLAR   │                                  │   LOAD    │          │
│     │  373 kW   │────────────────┐                 │  450 kW   │          │
│     │    ↓      │                │                 │    ↓      │          │
│     └───────────┘                │                 └───────────┘          │
│                                  │                       ↑                │
│     ┌───────────┐                ▼                       │                │
│     │   BESS    │           ┌─────────┐                  │                │
│     │  +50 kW   │──────────→│   BUS   │──────────────────┘                │
│     │  SOC:75%  │           │  450 kW │                                   │
│     └───────────┘           └─────────┘                                   │
│                                  ↑                                        │
│     ┌───────────┐                │                                        │
│     │   GRID    │────────────────┘                                        │
│     │   27 kW   │                                                         │
│     │  import   │                                                         │
│     └───────────┘                                                         │
│                                                                            │
└────────────────────────────────────────────────────────────────────────────┘
```

---

## Props Interface

```typescript
interface PowerFlowDiagramProps {
  data: PowerFlowData;
  isLoading?: boolean;
  error?: Error | null;

  // Layout
  layout?: 'horizontal' | 'vertical' | 'radial';
  showLabels?: boolean;
  showValues?: boolean;
  showAnimation?: boolean;

  // Interactions
  onNodeClick?: (node: PowerNode) => void;
  onNodeHover?: (node: PowerNode | null) => void;
  onFlowClick?: (flow: PowerFlow) => void;

  // Controls
  zoomEnabled?: boolean;
  panEnabled?: boolean;
  onZoomChange?: (zoom: number) => void;

  // Theming
  colorScheme?: 'default' | 'colorblind' | 'monochrome';
}

interface PowerFlowData {
  nodes: PowerNode[];
  flows: PowerFlow[];
  balance: EnergyBalance;
  timestamp: Date;
}

interface PowerNode {
  id: string;
  type: 'solar' | 'bess' | 'grid' | 'load' | 'generator' | 'bus';
  name: string;
  power: number;          // kW, positive = generating/export
  status: 'active' | 'idle' | 'offline';
  metadata?: {
    soc?: number;         // For BESS
    efficiency?: number;   // For solar/inverters
    quality?: number;      // For grid
  };
}

interface PowerFlow {
  id: string;
  from: string;           // Node ID
  to: string;             // Node ID
  power: number;          // kW
  direction: 'forward' | 'reverse';
  utilization?: number;   // 0-100%
}

interface EnergyBalance {
  production: number;
  consumption: number;
  storage: number;
  grid: number;           // positive = import, negative = export
  losses: number;
}
```

---

## Node Components

### Solar Node

```
┌─────────────────────────┐
│         ☀️              │
│       SOLAR             │
│      373 kW             │
│    ──────────           │
│    Efficiency: 94%      │
│    Status: Generating   │
└─────────────────────────┘
```

**Visual Properties:**
| Property | Value |
|----------|-------|
| Background | `bg-energy-solar/10` |
| Border | `border-2 border-energy-solar` |
| Icon | Sun icon |
| Text Color | `text-energy-solar` |

### BESS Node

```
┌─────────────────────────┐
│         🔋              │
│        BESS             │
│       +50 kW            │
│    ──────────           │
│    SOC: 75%             │
│    ████████░░           │
│    Mode: Charging       │
└─────────────────────────┘
```

**Visual Properties:**
| Property | Value |
|----------|-------|
| Background | `bg-energy-battery/10` |
| Border | `border-2 border-energy-battery` |
| Icon | Battery icon (fill based on SOC) |
| Progress Bar | SOC visualization |

**Mode Colors:**
| Mode | Color | Arrow |
|------|-------|-------|
| Charging | Green | ↑ |
| Discharging | Red | ↓ |
| Idle | Gray | → |

### Grid Node

```
┌─────────────────────────┐
│         ⚡              │
│        GRID             │
│       27 kW             │
│      import             │
│    ──────────           │
│    Quality: 98%         │
│    PF: 0.98             │
└─────────────────────────┘
```

**Visual Properties:**
| Import | Export |
|--------|--------|
| Border: `border-energy-grid` | Border: `border-energy-solar` |
| Arrow: ← (inward) | Arrow: → (outward) |
| Label: "import" | Label: "export" |

### Load Node

```
┌─────────────────────────┐
│         🏠              │
│        LOAD             │
│       450 kW            │
│    ──────────           │
│    Peak Today: 480 kW   │
│    Status: Normal       │
└─────────────────────────┘
```

### Bus Node (Central)

```
        ┌─────────┐
        │   BUS   │
        │ 450 kW  │
        │ Balance │
        └─────────┘
```

---

## Flow Lines

### Flow Line Types

```typescript
interface FlowLineStyle {
  strokeWidth: number;      // Based on power magnitude
  strokeColor: string;      // Based on source type
  animation: 'flow' | 'pulse' | 'none';
  dashArray?: string;
}
```

### Power-based Line Thickness

| Power Range | Stroke Width |
|-------------|--------------|
| 0-50 kW | 2px |
| 50-100 kW | 3px |
| 100-200 kW | 4px |
| 200-500 kW | 5px |
| 500+ kW | 6px |

### Flow Animation

```css
/* Forward flow animation */
.flow-line {
  stroke-dasharray: 10 5;
  animation: flow-forward 1s linear infinite;
}

@keyframes flow-forward {
  0% { stroke-dashoffset: 15; }
  100% { stroke-dashoffset: 0; }
}

/* Reverse flow animation */
.flow-line.reverse {
  animation: flow-reverse 1s linear infinite;
}

@keyframes flow-reverse {
  0% { stroke-dashoffset: 0; }
  100% { stroke-dashoffset: 15; }
}

/* No flow (idle) */
.flow-line.idle {
  stroke-dasharray: 3 3;
  animation: none;
  opacity: 0.3;
}
```

### Flow Colors

| Source | Color | CSS Class |
|--------|-------|-----------|
| Solar | Green | `stroke-energy-solar` |
| BESS (discharge) | Purple | `stroke-energy-battery` |
| BESS (charge) | Purple (reverse) | `stroke-energy-battery` |
| Grid (import) | Red | `stroke-energy-grid` |
| Grid (export) | Green | `stroke-energy-solar` |

---

## Energy Balance Panel

```
┌────────────────────────────────────────────────────────────────────────────┐
│                           ENERGY BALANCE                                   │
├──────────────────┬──────────────────┬──────────────────┬──────────────────┤
│    Production    │    Consumption   │     Storage      │       Grid       │
│                  │                  │                  │                  │
│     373 kW       │     450 kW       │     +50 kW       │     27 kW in     │
│        ↓         │        ↓         │        ↑         │        ←         │
│                  │                  │   (charging)     │    (import)      │
├──────────────────┴──────────────────┴──────────────────┴──────────────────┤
│ Balance: Production (373) + Grid (27) = Load (450) - Storage (50) ✓      │
└────────────────────────────────────────────────────────────────────────────┘
```

---

## Interactions

### Node Hover

```typescript
const handleNodeHover = (node: PowerNode | null) => {
  if (node) {
    // Show detailed tooltip
    setTooltip({
      node,
      position: getNodePosition(node.id),
    });

    // Highlight connected flows
    setHighlightedFlows(getConnectedFlows(node.id));
  } else {
    setTooltip(null);
    setHighlightedFlows([]);
  }
};
```

**Tooltip Content:**
```
┌─────────────────────────────┐
│ Solar Panel Array           │
│ ─────────────────           │
│ Current Power: 373 kW       │
│ Today's Production: 2.4 MWh │
│ Efficiency: 94.2%           │
│ Temperature: 45°C           │
│ ─────────────────           │
│ Status: Generating          │
│ Since: 06:15                │
│ ─────────────────           │
│ [View Details] [History]    │
└─────────────────────────────┘
```

### Node Click

```typescript
const handleNodeClick = (node: PowerNode) => {
  // Navigate to asset detail page
  router.push(`/assets/${node.id}`);
};
```

### Flow Hover

```typescript
const handleFlowHover = (flow: PowerFlow | null) => {
  if (flow) {
    setFlowTooltip({
      flow,
      from: getNode(flow.from),
      to: getNode(flow.to),
    });
  }
};
```

**Flow Tooltip:**
```
┌─────────────────────────────┐
│ Solar → Bus                 │
│ ─────────────────           │
│ Power: 373 kW               │
│ Direction: →                │
│ Utilization: 78%            │
│ ─────────────────           │
│ Line capacity: 500 kW       │
└─────────────────────────────┘
```

---

## Zoom & Pan Controls

```
┌─────────────────────────┐
│ [+] Zoom In             │
│ [-] Zoom Out            │
│ [⟲] Reset View          │
│ [▢] Fullscreen          │
│ [⚙] Settings            │
└─────────────────────────┘
```

```typescript
interface DiagramControls {
  zoom: number;           // 0.5 - 2.0
  pan: { x: number; y: number };
  autoFit: boolean;
}

// Zoom limits
const ZOOM_MIN = 0.5;
const ZOOM_MAX = 2.0;
const ZOOM_STEP = 0.1;
```

### Gesture Support

| Gesture | Action |
|---------|--------|
| Mouse wheel | Zoom in/out |
| Click + drag | Pan |
| Double-click | Zoom to node |
| Pinch (touch) | Zoom |
| Two-finger drag | Pan |

---

## Layout Options

### Horizontal Layout (Default)

```
Solar ──┐
        │
BESS ───┼──→ Bus ──→ Load
        │
Grid ───┘
```

### Vertical Layout

```
      Solar
        │
        ↓
BESS → Bus → Load
        ↑
       Grid
```

### Radial Layout

```
         Solar
           │
     BESS──●──Load
           │
         Grid
```

---

## States

### Loading State

```
┌────────────────────────────────────────────────────────────────────────────┐
│                                                                            │
│                          ◌ Loading diagram...                              │
│                                                                            │
│     ░░░░░░░░░░░                           ░░░░░░░░░░░                     │
│          │                                      │                          │
│          └────────────░░░░░░░────────────────┘                           │
│                           │                                                │
│                      ░░░░░░░░░░░                                          │
│                                                                            │
└────────────────────────────────────────────────────────────────────────────┘
```

### No Data State

```
┌────────────────────────────────────────────────────────────────────────────┐
│                                                                            │
│                        No power flow data                                  │
│                                                                            │
│                 Power flow data is not available.                          │
│                 Please ensure devices are connected.                       │
│                                                                            │
│                        [Refresh] [Configure]                               │
│                                                                            │
└────────────────────────────────────────────────────────────────────────────┘
```

### Offline Node

```
┌─────────────────────────┐
│         ☀️ ⚠️           │  ← Warning icon overlay
│       SOLAR             │
│       -- kW             │  ← No value
│    ──────────           │
│    Status: Offline      │
│    Since: 14:15         │
└─────────────────────────┘
```
Dashed border, reduced opacity, warning indicator

---

## Responsive Behavior

### Desktop (≥1024px)
- Full diagram with all labels
- Side-by-side summary cards
- Hover tooltips

### Tablet (768px - 1023px)
- Simplified labels
- Tap for tooltips
- Stacked summary cards

### Mobile (<768px)
- List view alternative
- Compact diagram
- Bottom sheet for details

```tsx
// Mobile alternative view
<MobilePowerFlowList>
  <FlowItem icon="☀️" name="Solar" value="373 kW" direction="out" />
  <FlowItem icon="🔋" name="BESS" value="+50 kW" direction="in" />
  <FlowItem icon="⚡" name="Grid" value="27 kW" direction="in" />
  <FlowItem icon="🏠" name="Load" value="450 kW" direction="consuming" />
</MobilePowerFlowList>
```

---

## Usage Example

```tsx
import { PowerFlowDiagram } from '@/components/monitoring/power-flow-diagram';

export default function PowerFlowPage() {
  const { data, isLoading, error } = usePowerFlowData({
    refreshInterval: 5000,
  });

  return (
    <div className="space-y-6">
      <PowerFlowDiagram
        data={data}
        isLoading={isLoading}
        error={error}
        layout="horizontal"
        showAnimation={true}
        showValues={true}
        zoomEnabled={true}
        panEnabled={true}
        onNodeClick={(node) => router.push(`/assets/${node.id}`)}
      />

      <div className="grid grid-cols-4 gap-4">
        <SummaryCard
          title="Solar"
          value={data?.nodes.find(n => n.type === 'solar')?.power}
          unit="kW"
          color="solar"
        />
        {/* ... more summary cards */}
      </div>
    </div>
  );
}
```
