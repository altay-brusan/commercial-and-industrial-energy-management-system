"use client";

import { useState } from "react";
import {
  ChevronRight,
  ChevronDown,
  Battery,
  BatteryCharging,
  Server,
  Package,
  CircuitBoard,
  Cpu,
  Thermometer,
} from "lucide-react";
import { cn } from "@/lib/utils";
import { Badge } from "@/components/ui/badge";
import type { BessTreeNodeDto } from "@/lib/bess/api-types";

interface SelectedDevice {
  id: string;
  type: string;
  unitId: string;
}

interface BessDeviceTreeProps {
  tree: BessTreeNodeDto[];
  selectedDevice: SelectedDevice | null;
  onSelectDevice: (device: SelectedDevice) => void;
}

const deviceIcon: Record<string, React.ElementType> = {
  unit: Battery,
  battery: BatteryCharging,
  rack: Server,
  pack: Package,
  module: CircuitBoard,
  esc: Cpu,
  hvac: Thermometer,
  pcs: Cpu,
};

function statusDotClass(status: string): string {
  switch (status) {
    case "normal":
    case "online":
      return "bg-status-operational";
    case "warning":
      return "bg-status-warning";
    case "fault":
    case "critical":
      return "bg-status-critical";
    case "offline":
      return "bg-muted-foreground";
    default:
      return "bg-muted-foreground";
  }
}

function findUnitId(node: BessTreeNodeDto, tree: BessTreeNodeDto[]): string {
  // If the node itself is a unit, its own id is the unitId
  if (node.type === "unit") return node.id;
  // Otherwise, walk up the tree or find the root unit
  // For simplicity, we pass unitId through the recursive tree traversal
  return node.id;
}

function TreeNode({
  node,
  unitId,
  selectedDevice,
  onSelectDevice,
  depth = 0,
}: {
  node: BessTreeNodeDto;
  unitId: string;
  selectedDevice: SelectedDevice | null;
  onSelectDevice: (device: SelectedDevice) => void;
  depth?: number;
}) {
  const [expanded, setExpanded] = useState(false);
  const hasChildren = node.children && node.children.length > 0;
  const isSelected = selectedDevice?.id === node.id;
  const Icon = deviceIcon[node.type] ?? Battery;

  const currentUnitId = node.type === "unit" ? node.id : unitId;

  const handleChevronClick = (e: React.MouseEvent) => {
    e.stopPropagation();
    setExpanded(!expanded);
  };

  const handleSelect = () => {
    onSelectDevice({
      id: node.id,
      type: node.type,
      unitId: currentUnitId,
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
        style={{ paddingLeft: `${8 + depth * 12}px` }}
        onClick={handleSelect}
      >
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

        <span
          className={cn(
            "h-2 w-2 rounded-full shrink-0",
            statusDotClass(node.status)
          )}
        />

        <Icon className="h-3.5 w-3.5 shrink-0" />

        <span className="truncate flex-1 text-xs">{node.name}</span>

        {node.alarmCount > 0 && (
          <Badge
            className="h-4 min-w-4 px-1 text-[10px] bg-severity-critical/15 text-severity-critical border-severity-critical/30 shrink-0"
          >
            {node.alarmCount}
          </Badge>
        )}
      </div>

      {hasChildren && expanded && (
        <div>
          {node.children.map((child) => (
            <TreeNode
              key={child.id}
              node={child}
              unitId={currentUnitId}
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
              unitId={node.id}
              selectedDevice={selectedDevice}
              onSelectDevice={onSelectDevice}
            />
          ))}
        </nav>
      </div>
    </aside>
  );
}
