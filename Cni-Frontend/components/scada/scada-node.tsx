import Image from "next/image";
import type { ScadaNodeData } from "@/lib/scada/types";

// Ellipse (isometric ground platform) — wide & flat for isometric feel
export const ELLIPSE_W = 220;
export const ELLIPSE_H = 90;

// Factory image
const IMG_SIZE = 120;
// How far above ellipse bottom the factory image starts — tuned so building
// base visually rests at the ellipse centre (the "ground level")
const FACTORY_BOTTOM_OFFSET = 22;

// Label area
const LABEL_W = 155;
const GAP = 12;

// Total node container — labels on the left, ellipse + factory on the right
export const NODE_W = LABEL_W + GAP + ELLIPSE_W; // 387
export const NODE_H = FACTORY_BOTTOM_OFFSET + IMG_SIZE; // 142

// Ellipse center within the node container
export const ELLIPSE_CX = LABEL_W + GAP + ELLIPSE_W / 2; // 277
export const ELLIPSE_CY = NODE_H - ELLIPSE_H / 2; // 97

interface ScadaNodeProps {
  node: ScadaNodeData;
  selected: boolean;
  onClick: () => void;
}

export function ScadaNode({ node, selected, onClick }: ScadaNodeProps) {
  // Ellipse visual area starts at this X offset within the container
  const ellipseLeft = LABEL_W + GAP;

  return (
    <div
      onClick={onClick}
      className="relative cursor-pointer select-none"
      style={{ width: NODE_W, height: NODE_H }}
    >
      {/* ── Factory image — base aligned with ellipse center (ground level) ── */}
      <Image
        src="/factory.png"
        alt={node.name}
        width={IMG_SIZE}
        height={IMG_SIZE}
        className="absolute z-10 drop-shadow-md pointer-events-none"
        style={{
          left: ellipseLeft + (ELLIPSE_W - IMG_SIZE) / 2,
          bottom: FACTORY_BOTTOM_OFFSET,
        }}
        draggable={false}
        priority
      />

      {/* ── Isometric ground shadow (soft gradient inside ellipse) ── */}
      <div
        className="absolute rounded-[50%]"
        style={{
          left: ellipseLeft,
          bottom: 0,
          width: ELLIPSE_W,
          height: ELLIPSE_H,
          background: `radial-gradient(ellipse, rgba(0,0,0,0.05) 0%, transparent 70%)`,
        }}
      />

      {/* ── Dashed ellipse border — isometric ground platform ── */}
      <div
        className="absolute rounded-[50%] border border-dashed transition-colors"
        style={{
          left: ellipseLeft,
          bottom: 0,
          width: ELLIPSE_W,
          height: ELLIPSE_H,
          borderColor: selected
            ? `var(--${node.color})`
            : "var(--border)",
          borderWidth: selected ? 2 : 1,
        }}
      />

      {/* ── Node name — italic isometric text at bottom-right of ellipse ── */}
      <span
        className="absolute text-[11px] italic tracking-wider"
        style={{
          color: `var(--${node.color})`,
          opacity: 0.55,
          bottom: -2,
          right: 16,
          transform: "rotate(-15deg)",
          transformOrigin: "bottom right",
        }}
      >
        {node.name}
      </span>

      {/* ── Labels — left side, vertically centered with the building ── */}
      <div
        className="absolute flex flex-col justify-center space-y-1.5 overflow-hidden"
        style={{
          left: 0,
          top: 0,
          bottom: ELLIPSE_H / 2,
          width: LABEL_W,
        }}
      >
        {/* Primary: value + unit + description */}
        <div className="flex items-baseline gap-1.5 justify-end">
          <span
            className="text-[20px] font-bold tabular-nums leading-none"
            style={{ color: `var(--${node.color})` }}
          >
            {node.realPower}
          </span>
          <span className="text-[11px] font-medium text-muted-foreground">
            {node.unit}
          </span>
          <span className="text-[10px] text-muted-foreground ml-1">
            Real Power
          </span>
        </div>

        {/* Secondary */}
        {node.secondaryLabel && node.secondaryValue && (
          <div className="flex items-baseline gap-1.5 justify-end">
            <span className="text-[16px] font-semibold tabular-nums leading-none text-foreground">
              {node.secondaryValue}
            </span>
            <span className="text-[10px] text-muted-foreground ml-1">
              {node.secondaryLabel}
            </span>
          </div>
        )}

        {/* Tertiary */}
        {node.tertiaryLabel && node.tertiaryValue && (
          <div className="flex items-baseline gap-1.5 justify-end">
            <span className="text-[16px] font-semibold tabular-nums leading-none text-foreground">
              {node.tertiaryValue}
            </span>
            <span className="text-[10px] text-muted-foreground ml-1">
              {node.tertiaryLabel}
            </span>
          </div>
        )}
      </div>
    </div>
  );
}
