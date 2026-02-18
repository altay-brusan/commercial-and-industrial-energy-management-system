"use client";

import { useRef, useState, useEffect, useCallback } from "react";
import type {
  ScadaNodeData,
  FlowConnection,
  ScadaNodeId,
} from "@/lib/scada/types";
import {
  ScadaNode,
  ELLIPSE_W,
  ELLIPSE_H,
  ELLIPSE_CX,
  ELLIPSE_CY,
} from "./scada-node";
import { ScadaFlowLine } from "./scada-flow-line";

interface ScadaDiagramProps {
  nodes: ScadaNodeData[];
  connections: FlowConnection[];
  selectedNode: ScadaNodeId | null;
  onNodeClick: (id: ScadaNodeId) => void;
}

const SEMI_A = ELLIPSE_W / 2;
const SEMI_B = ELLIPSE_H / 2;

// Ellipse center positions (fraction of container)
// Parallelogram — right column higher (isometric depth)
const CENTERS: Record<ScadaNodeId, { x: number; y: number }> = {
  load: { x: 0.38, y: 0.30 },
  battery: { x: 0.76, y: 0.22 },
  solar: { x: 0.37, y: 0.76 },
  grid: { x: 0.70, y: 0.66 },
};

// Point on ellipse perimeter in direction of angle θ
function ellipseEdge(
  cx: number,
  cy: number,
  angle: number
): { x: number; y: number } {
  const cos = Math.cos(angle);
  const sin = Math.sin(angle);
  const r =
    (SEMI_A * SEMI_B) /
    Math.sqrt((SEMI_B * cos) ** 2 + (SEMI_A * sin) ** 2);
  return { x: cx + r * cos, y: cy + r * sin };
}

function computePath(
  from: ScadaNodeId,
  to: ScadaNodeId,
  w: number,
  h: number
): string {
  const a = CENTERS[from];
  const b = CENTERS[to];
  const ax = a.x * w;
  const ay = a.y * h;
  const bx = b.x * w;
  const by = b.y * h;

  const angleAB = Math.atan2(by - ay, bx - ax);
  const angleBA = Math.atan2(ay - by, ax - bx);

  const start = ellipseEdge(ax, ay, angleAB);
  const end = ellipseEdge(bx, by, angleBA);

  // Gentle bezier — ground-level feel
  const dx = end.x - start.x;
  const dy = end.y - start.y;
  const cx1 = start.x + dx * 0.4;
  const cy1 = start.y + dy * 0.15;
  const cx2 = start.x + dx * 0.6;
  const cy2 = start.y + dy * 0.85;

  return `M ${start.x} ${start.y} C ${cx1} ${cy1}, ${cx2} ${cy2}, ${end.x} ${end.y}`;
}

export function ScadaDiagram({
  nodes,
  connections,
  selectedNode,
  onNodeClick,
}: ScadaDiagramProps) {
  const containerRef = useRef<HTMLDivElement>(null);
  const [size, setSize] = useState({ w: 0, h: 0 });

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

  return (
    <div ref={containerRef} className="relative w-full h-full min-h-[500px]">
      {/* SVG overlay — flow lines on isometric ground plane */}
      {size.w > 0 && (
        <svg
          className="absolute inset-0 w-full h-full pointer-events-none z-0"
          viewBox={`0 0 ${size.w} ${size.h}`}
        >
          {connections.map((conn) => {
            const id = `flow-${conn.from}-${conn.to}`;
            return (
              <ScadaFlowLine
                key={id}
                id={id}
                connection={conn}
                pathD={computePath(conn.from, conn.to, size.w, size.h)}
              />
            );
          })}
        </svg>
      )}

      {/* HTML nodes — offset so ellipse center is at CENTERS % */}
      {nodes.map((node) => {
        const c = CENTERS[node.id];
        return (
          <div
            key={node.id}
            className="absolute z-10"
            style={{
              left: `${c.x * 100}%`,
              top: `${c.y * 100}%`,
              marginLeft: -ELLIPSE_CX,
              marginTop: -ELLIPSE_CY,
            }}
          >
            <ScadaNode
              node={node}
              selected={selectedNode === node.id}
              onClick={() => onNodeClick(node.id)}
            />
          </div>
        );
      })}
    </div>
  );
}
