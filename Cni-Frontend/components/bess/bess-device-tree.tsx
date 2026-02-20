"use client";

import { useState } from "react";
import {
  ChevronRight,
  ChevronDown,
  Battery,
  Server,
  Cpu,
  Thermometer,
} from "lucide-react";
import { cn } from "@/lib/utils";
import { Badge } from "@/components/ui/badge";
import type {
  BessDeviceTreeNode,
  BessSelectedDevice,
  BessDeviceType,
} from "@/lib/bess/types";

interface BessDeviceTreeProps {
  tree: BessDeviceTreeNode[];
  selectedDevice: BessSelectedDevice | null;
  onSelectDevice: (device: BessSelectedDevice) => void;
}

const deviceIcon: Record<BessDeviceType, React.ElementType> = {
  unit: Battery,
  rack: Server,
  esc: Cpu,
  hvac: Thermometer,
};

const typeLabel: Record<BessDeviceType, string> = {
  unit: "Unit",
  rack: "Rack",
  esc: "Ctrl",
  hvac: "TMS",
};

function statusDotClass(status: string): string {
  switch (status) {
    case "normal":
      return "bg-status-operational";
    case "warning":
      return "bg-status-warning";
    case "fault":
      return "bg-status-critical";
    case "offline":
      return "bg-muted-foreground";
    default:
      return "bg-muted-foreground";
  }
}

function TreeNode({
  node,
  selectedDevice,
  onSelectDevice,
  depth = 0,
}: {
  node: BessDeviceTreeNode;
  selectedDevice: BessSelectedDevice | null;
  onSelectDevice: (device: BessSelectedDevice) => void;
  depth?: number;
}) {
  const [expanded, setExpanded] = useState(false);
  const hasChildren = node.children && node.children.length > 0;
  const isSelected = selectedDevice?.id === node.id;
  const Icon = deviceIcon[node.type];

  const handleChevronClick = (e: React.MouseEvent) => {
    e.stopPropagation();
    setExpanded(!expanded);
  };

  const handleSelect = () => {
    onSelectDevice({
      id: node.id,
      type: node.type,
      unitId: node.parentUnitId,
    });
  };

  return (
    <div>
      <div
        className={cn(
          "flex items-center gap-1.5 px-2 py-1.5 rounded-md text-sm cursor-pointer transition-colors group",
          isSelected
            ? "bg-primary/10 text-primary font-medium"
            : "text-muted-foreground hover:bg-secondary/50 hover:text-foreground"
        )}
        style={{ paddingLeft: `${8 + depth * 16}px` }}
        onClick={handleSelect}
      >
        {/* Expand/collapse chevron */}
        {hasChildren ? (
          <button
            onClick={handleChevronClick}
            className="shrink-0 p-0.5 rounded hover:bg-muted transition-colors"
          >
            {expanded ? (
              <ChevronDown className="h-3.5 w-3.5" />
            ) : (
              <ChevronRight className="h-3.5 w-3.5" />
            )}
          </button>
        ) : (
          <span className="w-[18px] shrink-0" />
        )}

        {/* Status dot */}
        <span
          className={cn(
            "h-2 w-2 rounded-full shrink-0",
            statusDotClass(node.status as string)
          )}
        />

        {/* Icon */}
        <Icon className="h-3.5 w-3.5 shrink-0" />

        {/* Name */}
        <span className="truncate flex-1 text-xs">{node.name}</span>

        {/* Type badge for non-unit types */}
        {node.type !== "unit" && node.type !== "rack" && (
          <span className="text-[10px] text-muted-foreground/70 shrink-0">
            {typeLabel[node.type]}
          </span>
        )}

        {/* Alarm count badge */}
        {node.alarmCount > 0 && (
          <Badge
            className="h-4 min-w-4 px-1 text-[10px] bg-severity-critical/15 text-severity-critical border-severity-critical/30 shrink-0"
          >
            {node.alarmCount}
          </Badge>
        )}
      </div>

      {/* Children */}
      {hasChildren && expanded && (
        <div>
          {node.children!.map((child) => (
            <TreeNode
              key={child.id}
              node={child}
              selectedDevice={selectedDevice}
              onSelectDevice={onSelectDevice}
              depth={depth + 1}
            />
          ))}
        </div>
      )}
    </div>
  );
}

export function BessDeviceTree({
  tree,
  selectedDevice,
  onSelectDevice,
}: BessDeviceTreeProps) {
  return (
    <aside className="w-[240px] shrink-0 border-r border-border bg-background overflow-y-auto">
      <div className="px-3 py-4">
        <p className="text-[11px] font-semibold uppercase tracking-wider text-muted-foreground mb-3 px-2">
          Device Tree
        </p>
        <nav className="space-y-0.5">
          {tree.map((node) => (
            <TreeNode
              key={node.id}
              node={node}
              selectedDevice={selectedDevice}
              onSelectDevice={onSelectDevice}
            />
          ))}
        </nav>
      </div>
    </aside>
  );
}
