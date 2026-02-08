# Power Flow Diagram Widget

Enerji akışını görsel ve anlaşılır şekilde gösteren kritik widget.

---

## Görsel Tasarım

```
┌────────────────────────────────────────────────────────────────────────┐
│  POWER FLOW                                          [Full Screen] [R] │
├────────────────────────────────────────────────────────────────────────┤
│                                                                        │
│         SOLAR                                                          │
│         125 kW                                                         │
│         [████████░░] 85%                                               │
│              │                                                         │
│              │ ─────────────────────┐                                  │
│              ▼                      │                                  │
│                                     │                                  │
│    ┌─────────────────────────────────────────────────┐                │
│    │                                                 │                │
│    │                    BUS BAR                      │                │
│    │                    450 kW                       │                │
│    │                                                 │                │
│    └─────────────────────────────────────────────────┘                │
│         ▲              │              │                               │
│         │              │              ▼                               │
│         │              │                                              │
│    GRID               │         LOAD                                  │
│    Import: 275 kW     │         380 kW                                │
│    PF: 0.98           │         Peak: 420 kW                          │
│                       │                                               │
│                       ▼                                               │
│                  BESS                                                 │
│                  Charging: 50 kW                                      │
│                  SOC: 75%                                             │
│                  [███████░░░]                                         │
│                                                                        │
│  ─────────────────────────────────────────────────────────────────── │
│  Legend: --> Power Flow    === High Voltage    --- Medium Voltage     │
│                                                                        │
└────────────────────────────────────────────────────────────────────────┘
```

---

## Node Tipleri

| Node | İkon | Renk | Açıklama |
|------|------|------|----------|
| Solar | Güneş | Yeşil/Sarı | Solar üretim |
| BESS | Batarya | Mor | Enerji depolama |
| Grid | Şebeke | Mavi/Kırmızı | Şebeke bağlantısı |
| Load | Ev/Fabrika | Yeşil | Tüketim |
| Bus | Dikdörtgen | Gri | Toplama noktası |
| Generator | Motor | Gri | Jeneratör (opsiyonel) |

---

## Flow Line Özellikleri

### Animasyon
- Oklar akış yönünde hareket eder
- Hız, güç miktarına göre değişir

### Kalınlık
- Güç miktarına göre çizgi kalınlığı
- Min: 2px, Max: 8px

### Renk
- Kaynak rengine göre (Solar=yeşil, Grid=mavi, BESS=mor)

---

## Node Detayları (Hover)

### Solar Node Hover

```
┌─────────────────────────────┐
│ Solar Production            │
│ ─────────────────           │
│ Current: 125 kW             │
│ Peak Today: 142 kW          │
│ Efficiency: 94%             │
│ Irradiance: 850 W/m²        │
│                             │
│ [View Details]              │
└─────────────────────────────┘
```

### BESS Node Hover

```
┌─────────────────────────────┐
│ Battery Storage             │
│ ─────────────────           │
│ Mode: Charging              │
│ Power: +50 kW               │
│ SOC: 75%                    │
│ Temperature: 32°C           │
│ Est. Full: 2h 30m           │
│                             │
│ [View Details]              │
└─────────────────────────────┘
```

---

## İnteraktivite

### Hover on Node
- Detaylı bilgi tooltip
- Node highlight efekti

### Click on Node
- İlgili cihaz sayfasına navigate

### Animated Arrows
- Akış yönünü göster
- Power > 0: animasyon aktif
- Power = 0: statik çizgi

### Zoom/Pan
- Scroll: Zoom in/out
- Drag: Pan view
- Double-click: Reset view
- Pinch (mobile): Zoom

---

## Data Structure

```typescript
interface PowerFlowData {
  nodes: PowerFlowNode[];
  connections: PowerFlowConnection[];
  timestamp: Date;
}

interface PowerFlowNode {
  id: string;
  type: 'solar' | 'bess' | 'grid' | 'load' | 'bus' | 'generator';
  label: string;
  power: number; // kW
  status: 'active' | 'standby' | 'fault' | 'offline';
  details: Record<string, unknown>;
  position: { x: number; y: number };
}

interface PowerFlowConnection {
  id: string;
  from: string; // node id
  to: string; // node id
  power: number; // kW (positive = from->to)
  type: 'hv' | 'mv' | 'lv';
}
```

---

## Props

```typescript
interface PowerFlowProps {
  data: PowerFlowData;
  onNodeClick?: (nodeId: string) => void;
  onFullScreen?: () => void;
  onRefresh?: () => void;
  animated?: boolean;
  showLegend?: boolean;
  interactive?: boolean;
}
```

---

## Simplified Mode (Tablet/Mobile)

```
┌────────────────────────────────────┐
│  POWER SUMMARY                     │
│                                    │
│  Solar    -->  125 kW  -->         │
│  BESS     <--   50 kW  <--  LOAD   │
│  Grid     -->  275 kW  -->  380 kW │
│                                    │
│  Net: Export 70 kW                 │
│                                    │
└────────────────────────────────────┘
```

---

## Responsive Behavior

| Breakpoint | Görünüm |
|------------|---------|
| Desktop | Full animated diagram |
| Tablet | Simplified diagram |
| Mobile | Text-based power summary |

---

## Performance

- SVG tabanlı render
- requestAnimationFrame ile animasyon
- Throttled updates (100ms)
- Memoized node positions

---

## Accessibility

- `role="img"` with descriptive `aria-label`
- Keyboard navigation between nodes
- Screen reader: Announces power values
- High contrast mode: Thicker lines, patterns
