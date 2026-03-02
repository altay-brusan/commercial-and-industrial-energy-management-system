"use client";

import { useRef, useEffect, useCallback, useState, useMemo } from "react";
import Image from "next/image";
import type {
  ScadaNodeData,
  FlowConnection,
  ScadaNodeId,
} from "@/lib/scada/types";

// ===== Isometric constants =====
const C30 = Math.cos(Math.PI / 6);
const S30 = 0.5;

// Node grid positions (isometric grid coords)
const NODE_GRID: Record<ScadaNodeId, { gx: number; gy: number }> = {
  load: { gx: -3, gy: -4 },
  battery: { gx: 5, gy: -4 },
  solar: { gx: -3, gy: 4 },
  grid: { gx: 5, gy: 4 },
};

// Reference design size
const REF_W = 1200;
const REF_H = 800;
const BASE_T = 46;

// Base sizes at scale=1
const BASE_ELLIPSE_RX = 120;
const BASE_ELLIPSE_RY = 40;
const BASE_ELLIPSE_OFFSET_Y = 15;
const BASE_IMG_SIZE = 200;

// Node images
const NODE_IMAGES: Record<ScadaNodeId, string> = {
  load: "/energy-flow/flow-load.png",
  battery: "/energy-flow/flow-battery.png",
  solar: "/energy-flow/flow-solar.png",
  grid: "/energy-flow/flow-grid.png",
};

// Node label colors
const LABEL_COLORS: Record<ScadaNodeId, string> = {
  load: "#8BC34A",
  battery: "#8BC34A",
  solar: "#FF9800",
  grid: "#2196F3",
};

// Value colors
const VALUE_COLORS: Record<ScadaNodeId, string> = {
  load: "#4CAF50",
  battery: "#4CAF50",
  solar: "#2196F3",
  grid: "#2196F3",
};

// Image offsets from node center (at scale=1)
const BASE_IMG_OFFSETS: Record<ScadaNodeId, { x: number; y: number }> = {
  load: { x: -100, y: -100 },
  battery: { x: -100, y: -100 },
  solar: { x: -100, y: -75 },
  grid: { x: -120, y: -95 },
};

// Stats card offsets from node center (at scale=1)
const BASE_CARD_OFFSETS: Record<ScadaNodeId, { x: number; y: number }> = {
  load: { x: -210, y: -120 },
  battery: { x: -55, y: -160 },
  solar: { x: -230, y: 5 },
  grid: { x: -240, y: 15 },
};

// Label x-offsets from node center (at scale=1)
const BASE_LABEL_X: Record<ScadaNodeId, number> = {
  load: -10,
  battery: 20,
  solar: -5,
  grid: 15,
};

// Theme color sets for canvas drawing
const THEME = {
  light: {
    bg: "#ffffff",
    gridColor: (a: number) => `rgba(170,180,200,${a})`,
    ellipseStroke: "rgba(180, 188, 200, 0.55)",
    ellipseFill: "rgba(235, 238, 245, 0.35)",
    baseLine: "rgba(190, 195, 210, 0.5)",
  },
  dark: {
    bg: "#1c1c1c",
    gridColor: (a: number) => `rgba(80,90,110,${a})`,
    ellipseStroke: "rgba(80, 90, 110, 0.55)",
    ellipseFill: "rgba(40, 45, 55, 0.4)",
    baseLine: "rgba(80, 90, 110, 0.5)",
  },
};

// ===== ISO projection =====
function iso(gx: number, gy: number, ox: number, oy: number, t: number): [number, number] {
  return [ox + (gx - gy) * C30 * t, oy + (gx + gy) * S30 * t];
}

function nPos(key: ScadaNodeId, ox: number, oy: number, t: number): [number, number] {
  const { gx, gy } = NODE_GRID[key];
  return iso(gx, gy, ox, oy, t);
}

// ===== Ellipse edge point =====
function ellipseEdgePoint(
  ecx: number,
  ecy: number,
  targetX: number,
  targetY: number,
  rx: number,
  ry: number,
  offsetY: number
): [number, number] {
  const ey = ecy + offsetY;
  const dx = targetX - ecx;
  const dy = targetY - ey;
  const angle = Math.atan2(dy * rx, dx * ry);
  return [ecx + rx * Math.cos(angle), ey + ry * Math.sin(angle)];
}

// ===== Canvas drawing functions =====
function drawGrid(ctx: CanvasRenderingContext2D, ox: number, oy: number, t: number, colors: typeof THEME.light) {
  const N = 16;
  for (let i = -N; i <= N; i++) {
    const f = 1 - Math.abs(i) / (N + 1);
    const a = f * 0.25;
    if (a < 0.01) continue;
    ctx.strokeStyle = colors.gridColor(a);
    ctx.lineWidth = 0.5;

    const [x1, y1] = iso(i, -N, ox, oy, t);
    const [x2, y2] = iso(i, N, ox, oy, t);
    ctx.beginPath();
    ctx.moveTo(x1, y1);
    ctx.lineTo(x2, y2);
    ctx.stroke();

    const [x3, y3] = iso(-N, i, ox, oy, t);
    const [x4, y4] = iso(N, i, ox, oy, t);
    ctx.beginPath();
    ctx.moveTo(x3, y3);
    ctx.lineTo(x4, y4);
    ctx.stroke();
  }
}

function drawEllipse(ctx: CanvasRenderingContext2D, cx: number, cy: number, rx: number, ry: number, offsetY: number, colors: typeof THEME.light, selected: boolean, time: number) {
  const ey = cy + offsetY;

  ctx.fillStyle = colors.ellipseFill;
  ctx.beginPath();
  ctx.ellipse(cx, ey, rx, ry, 0, 0, Math.PI * 2);
  ctx.fill();

  // Default dashed border
  ctx.save();
  ctx.setLineDash([6, 5]);
  ctx.strokeStyle = colors.ellipseStroke;
  ctx.lineWidth = 1.2;
  ctx.beginPath();
  ctx.ellipse(cx, ey, rx, ry, 0, 0, Math.PI * 2);
  ctx.stroke();
  ctx.setLineDash([]);
  ctx.restore();

  // Selected: spinning blue arc
  if (selected) {
    const speed = 2.5;
    const arcLen = Math.PI * 0.8; // arc length (~144 degrees)
    const startAngle = time * speed;

    // Outer glow
    ctx.save();
    ctx.strokeStyle = "rgba(91, 141, 239, 0.15)";
    ctx.lineWidth = 6;
    ctx.beginPath();
    ctx.ellipse(cx, ey, rx, ry, 0, startAngle, startAngle + arcLen);
    ctx.stroke();
    ctx.restore();

    // Main arc
    ctx.save();
    ctx.strokeStyle = "rgba(91, 141, 239, 0.85)";
    ctx.lineWidth = 2.5;
    ctx.lineCap = "round";
    ctx.beginPath();
    ctx.ellipse(cx, ey, rx, ry, 0, startAngle, startAngle + arcLen);
    ctx.stroke();
    ctx.restore();

    // Second arc (opposite side, fainter)
    ctx.save();
    ctx.strokeStyle = "rgba(91, 141, 239, 0.4)";
    ctx.lineWidth = 2;
    ctx.lineCap = "round";
    ctx.beginPath();
    ctx.ellipse(cx, ey, rx, ry, 0, startAngle + Math.PI, startAngle + Math.PI + arcLen * 0.5);
    ctx.stroke();
    ctx.restore();
  }
}

function drawBlueDot(ctx: CanvasRenderingContext2D, x: number, y: number) {
  ctx.fillStyle = "rgba(91, 141, 239, 0.25)";
  ctx.beginPath();
  ctx.arc(x, y, 5, 0, Math.PI * 2);
  ctx.fill();
  ctx.fillStyle = "rgba(91, 141, 239, 0.75)";
  ctx.beginPath();
  ctx.arc(x, y, 2.5, 0, Math.PI * 2);
  ctx.fill();
}

function drawConnection(
  ctx: CanvasRenderingContext2D,
  fromCenter: [number, number],
  toCenter: [number, number],
  time: number,
  rx: number,
  ry: number,
  offsetY: number,
  colors: typeof THEME.light
) {
  const f = ellipseEdgePoint(fromCenter[0], fromCenter[1], toCenter[0], toCenter[1], rx, ry, offsetY);
  const t = ellipseEdgePoint(toCenter[0], toCenter[1], fromCenter[0], fromCenter[1], rx, ry, offsetY);

  const dx = t[0] - f[0];
  const dy = t[1] - f[1];
  const len = Math.sqrt(dx * dx + dy * dy);

  // Base line
  ctx.strokeStyle = colors.baseLine;
  ctx.lineWidth = 1;
  ctx.beginPath();
  ctx.moveTo(f[0], f[1]);
  ctx.lineTo(t[0], t[1]);
  ctx.stroke();

  // Gradient flow wave animation
  const speed = 65;
  const gradLen = 80;
  const cycle = len + gradLen * 2;

  function drawGradWave(offset: number, alpha: number) {
    const gStartT = Math.max(0, offset / len);
    const gEndT = Math.min(1, (offset + gradLen) / len);
    if (gEndT <= 0 || gStartT >= 1) return;

    const gx1 = f[0] + dx * Math.max(0, gStartT);
    const gy1 = f[1] + dy * Math.max(0, gStartT);
    const gx2 = f[0] + dx * Math.min(1, gEndT);
    const gy2 = f[1] + dy * Math.min(1, gEndT);

    const grad = ctx.createLinearGradient(gx1, gy1, gx2, gy2);
    grad.addColorStop(0, "rgba(91,141,239,0)");
    grad.addColorStop(0.3, `rgba(91,141,239,${alpha * 0.45})`);
    grad.addColorStop(0.5, `rgba(120,170,255,${alpha * 0.6})`);
    grad.addColorStop(0.7, `rgba(91,141,239,${alpha * 0.45})`);
    grad.addColorStop(1, "rgba(91,141,239,0)");

    ctx.strokeStyle = grad;
    ctx.lineWidth = 2;
    ctx.beginPath();
    ctx.moveTo(gx1, gy1);
    ctx.lineTo(gx2, gy2);
    ctx.stroke();
  }

  const offset1 = ((time * speed) % cycle) - gradLen;
  drawGradWave(offset1, 1);

  const offset2 = (((time * speed) + cycle * 0.5) % cycle) - gradLen;
  drawGradWave(offset2, 0.8);

  // Blue junction dots
  drawBlueDot(ctx, f[0], f[1]);
  drawBlueDot(ctx, t[0], t[1]);
}

// ===== Dark mode hook =====
function useDarkMode() {
  const [dark, setDark] = useState(false);

  useEffect(() => {
    const html = document.documentElement;
    setDark(html.classList.contains("dark"));

    const observer = new MutationObserver(() => {
      setDark(html.classList.contains("dark"));
    });
    observer.observe(html, { attributes: true, attributeFilter: ["class"] });
    return () => observer.disconnect();
  }, []);

  return dark;
}

// ===== Component =====
interface EnergyFlowCanvasProps {
  nodes: ScadaNodeData[];
  connections: FlowConnection[];
  selectedNode: ScadaNodeId | null;
  onNodeClick: (id: ScadaNodeId) => void;
  showGrid?: boolean;
  compact?: boolean;
}

export function EnergyFlowCanvas({
  nodes,
  connections,
  selectedNode,
  onNodeClick,
  showGrid = true,
  compact = false,
}: EnergyFlowCanvasProps) {
  const containerRef = useRef<HTMLDivElement>(null);
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const animRef = useRef<number>(0);
  const timeRef = useRef(0);
  const lastRef = useRef(performance.now());
  const [size, setSize] = useState({ w: 0, h: 0 });
  const isDark = useDarkMode();

  // Responsive resize
  const measure = useCallback(() => {
    if (containerRef.current) {
      const rect = containerRef.current.getBoundingClientRect();
      setSize({ w: rect.width, h: rect.height });
    }
  }, []);

  useEffect(() => {
    measure();
    const ro = new ResizeObserver(measure);
    if (containerRef.current) ro.observe(containerRef.current);
    return () => ro.disconnect();
  }, [measure]);

  // Compute scale factor
  const scale = useMemo(() => {
    if (size.w === 0 || size.h === 0) return 1;
    if (compact) {
      return Math.min(size.w / 700, size.h / 600, 1);
    }
    return Math.min(size.w / REF_W, size.h / REF_H, 1);
  }, [size, compact]);

  // Scaled values
  const t = BASE_T * scale;
  const ellipseRx = BASE_ELLIPSE_RX * scale;
  const ellipseRy = BASE_ELLIPSE_RY * scale;
  const ellipseOffsetY = BASE_ELLIPSE_OFFSET_Y * scale;
  const imgSize = Math.round(BASE_IMG_SIZE * scale);

  const colors = isDark ? THEME.dark : THEME.light;

  // Canvas animation loop
  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas || size.w === 0 || size.h === 0) return;

    const ctx = canvas.getContext("2d");
    if (!ctx) return;

    const dpr = window.devicePixelRatio || 1;
    canvas.width = size.w * dpr;
    canvas.height = size.h * dpr;
    ctx.scale(dpr, dpr);

    const ox = compact ? size.w * 0.45 : size.w * 0.5;
    const oy = compact ? size.h * 0.47 : size.h * 0.45;

    function frame(now: number) {
      const dt = Math.min((now - lastRef.current) / 1000, 0.05);
      lastRef.current = now;
      timeRef.current += dt;

      ctx!.clearRect(0, 0, size.w, size.h);

      // Background
      ctx!.fillStyle = colors.bg;
      ctx!.fillRect(0, 0, size.w, size.h);

      // Isometric grid
      if (showGrid) drawGrid(ctx!, ox, oy, t, colors);

      // Compute node screen positions
      const nsp: Record<string, [number, number]> = {};
      for (const k of Object.keys(NODE_GRID) as ScadaNodeId[]) {
        nsp[k] = nPos(k, ox, oy, t);
      }

      // Ellipses under each node
      for (const k of Object.keys(nsp) as ScadaNodeId[]) {
        drawEllipse(ctx!, nsp[k][0], nsp[k][1], ellipseRx, ellipseRy, ellipseOffsetY, colors, k === selectedNode, timeRef.current);
      }

      // Connections
      for (const conn of connections) {
        if (conn.direction !== "idle") {
          drawConnection(ctx!, nsp[conn.from], nsp[conn.to], timeRef.current, ellipseRx, ellipseRy, ellipseOffsetY, colors);
        }
      }

      animRef.current = requestAnimationFrame(frame);
    }

    animRef.current = requestAnimationFrame(frame);

    return () => {
      cancelAnimationFrame(animRef.current);
    };
  }, [size, connections, t, ellipseRx, ellipseRy, ellipseOffsetY, colors, selectedNode, showGrid, compact]);

  // Compute positions for HTML overlay
  const ox = compact ? size.w * 0.45 : size.w * 0.5;
  const oy = compact ? size.h * 0.47 : size.h * 0.45;
  const nodePositions: Record<ScadaNodeId, [number, number]> = {
    load: nPos("load", ox, oy, t),
    battery: nPos("battery", ox, oy, t),
    solar: nPos("solar", ox, oy, t),
    grid: nPos("grid", ox, oy, t),
  };

  // Get node data by id
  const nodeMap = new Map(nodes.map((n) => [n.id, n]));

  // Flow tag midpoints
  const flowTags = connections
    .filter((c) => c.direction !== "idle" && c.power > 0.01)
    .map((conn) => {
      const fc = nodePositions[conn.from];
      const tc = nodePositions[conn.to];
      const fe = ellipseEdgePoint(fc[0], fc[1], tc[0], tc[1], ellipseRx, ellipseRy, ellipseOffsetY);
      const te = ellipseEdgePoint(tc[0], tc[1], fc[0], fc[1], ellipseRx, ellipseRy, ellipseOffsetY);
      const mx = (fe[0] + te[0]) / 2;
      const my = (fe[1] + te[1]) / 2;
      return { key: `${conn.from}-${conn.to}`, x: mx + 8 * scale, y: my - 16 * scale, power: conn.power };
    });

  // Scaled font sizes
  const valueFontSize = Math.max(16, Math.round(26 * scale));
  const unitFontSize = Math.max(10, Math.round(13 * scale));
  const descFontSize = Math.max(9, Math.round(11 * scale));
  const secFontSize = Math.max(12, Math.round(16 * scale));
  const labelFontSize = Math.max(10, Math.round(13 * scale));
  const labelTracking = Math.max(1, Math.round(3 * scale));
  const tagFontSize = Math.max(9, Math.round(11 * scale));

  return (
    <div ref={containerRef} className={`relative w-full h-full ${compact ? "" : "min-h-[500px]"}`}>
      {/* Canvas layer */}
      <canvas
        ref={canvasRef}
        className="absolute inset-0 w-full h-full"
        style={{ zIndex: 1 }}
      />

      {/* HTML overlay */}
      {size.w > 0 && (
        <div
          className="absolute inset-0 pointer-events-none"
          style={{ zIndex: 2 }}
        >
          {/* Node images */}
          {(Object.keys(NODE_GRID) as ScadaNodeId[]).map((id) => {
            const [px, py] = nodePositions[id];
            const off = BASE_IMG_OFFSETS[id];
            return (
              <div
                key={`img-${id}`}
                className="absolute pointer-events-auto cursor-pointer"
                style={{
                  left: px + off.x * scale,
                  top: py + off.y * scale,
                  width: imgSize,
                  zIndex: 3,
                }}
                onClick={() => onNodeClick(id)}
              >
                <Image
                  src={NODE_IMAGES[id]}
                  alt={id}
                  width={imgSize}
                  height={Math.round(imgSize * 0.76)}
                  className="h-auto"
                  style={{ width: imgSize }}
                  draggable={false}
                  priority
                />
              </div>
            );
          })}

          {/* Stats cards (hidden in compact mode) */}
          {!compact && (Object.keys(NODE_GRID) as ScadaNodeId[]).map((id) => {
            const node = nodeMap.get(id);
            if (!node) return null;
            const [px, py] = nodePositions[id];
            const off = BASE_CARD_OFFSETS[id];
            return (
              <div
                key={`card-${id}`}
                className="absolute pointer-events-auto whitespace-nowrap"
                style={{
                  left: px + off.x * scale,
                  top: py + off.y * scale,
                  zIndex: 4,
                }}
              >
                <div className="flex items-baseline gap-[5px] mb-[1px]">
                  <span
                    className="font-bold leading-[1.2]"
                    style={{ color: VALUE_COLORS[id], fontSize: valueFontSize }}
                  >
                    {node.realPower.toFixed(2)}
                  </span>
                  <span className="text-muted-foreground font-normal" style={{ fontSize: unitFontSize }}>
                    {node.unit}
                  </span>
                  <span className="text-muted-foreground/70 font-normal" style={{ fontSize: descFontSize }}>
                    Real Power
                  </span>
                </div>

                {node.secondaryLabel && node.secondaryValue && (
                  <div className="flex items-baseline gap-[5px] mb-[1px]">
                    <span className="font-semibold text-foreground" style={{ fontSize: secFontSize }}>
                      {node.secondaryValue}
                    </span>
                    <span className="text-muted-foreground/70 font-normal" style={{ fontSize: descFontSize }}>
                      {node.secondaryLabel}
                    </span>
                  </div>
                )}

                {node.tertiaryLabel && node.tertiaryValue && (
                  <div className="flex items-baseline gap-[5px] mb-[1px]">
                    <span className="font-semibold text-foreground" style={{ fontSize: secFontSize }}>
                      {node.tertiaryValue}
                    </span>
                    <span className="text-muted-foreground/70 font-normal" style={{ fontSize: descFontSize }}>
                      {node.tertiaryLabel}
                    </span>
                  </div>
                )}
              </div>
            );
          })}

          {/* Node labels */}
          {(Object.keys(NODE_GRID) as ScadaNodeId[]).map((id) => {
            const node = nodeMap.get(id);
            if (!node) return null;
            const [px, py] = nodePositions[id];
            return (
              <div
                key={`lbl-${id}`}
                className="absolute font-normal whitespace-nowrap"
                style={{
                  left: px + BASE_LABEL_X[id] * scale,
                  top: py + (ellipseOffsetY + ellipseRy + 8 * scale),
                  color: LABEL_COLORS[id],
                  fontSize: labelFontSize,
                  letterSpacing: labelTracking,
                  zIndex: 4,
                }}
              >
                {node.name}
              </div>
            );
          })}

          {/* Flow tags */}
          {flowTags.map((ft) => (
            <div
              key={ft.key}
              className="absolute font-medium text-muted-foreground whitespace-nowrap rounded-[3px] bg-background/85"
              style={{
                left: ft.x,
                top: ft.y,
                padding: "1px 4px",
                fontSize: tagFontSize,
                letterSpacing: 0.3,
                zIndex: 4,
              }}
            >
              {ft.power.toFixed(2)} MW
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
