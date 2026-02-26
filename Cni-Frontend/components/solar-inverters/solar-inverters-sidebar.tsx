"use client";

import { useState } from "react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import {
  ChevronRight,
  ChevronDown,
  Router,
  Sun,
} from "lucide-react";
import { cn } from "@/lib/utils";
import { Badge } from "@/components/ui/badge";
import type { SolarDeviceTreeNode } from "@/lib/solar-inverters/types";

export type SolarInverterView = "device-list" | "energy-analytics" | "performance";

interface SolarInvertersSidebarProps {
  tree?: SolarDeviceTreeNode[];
  selectedInverterId?: string | null;
}

const basePath = "/monitoring/solar-inverters";

const menuItems: { id: SolarInverterView; label: string }[] = [
  { id: "device-list", label: "Device List" },
  { id: "energy-analytics", label: "Energy Analytics" },
  { id: "performance", label: "Performance" },
];

function statusDotClass(status: string): string {
  switch (status) {
    case "online":
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
  selectedInverterId,
  depth = 0,
}: {
  node: SolarDeviceTreeNode;
  selectedInverterId?: string | null;
  depth?: number;
}) {
  const [expanded, setExpanded] = useState(depth === 0);
  const hasChildren = node.children && node.children.length > 0;
  const isSelected = node.type === "inverter" && selectedInverterId === node.id;
  const Icon = node.type === "logger" ? Router : Sun;

  const handleChevronClick = (e: React.MouseEvent) => {
    e.stopPropagation();
    e.preventDefault();
    setExpanded(!expanded);
  };

  const content = (
    <div
      className={cn(
        "flex items-center gap-1.5 px-2 py-1.5 rounded-md text-sm cursor-pointer transition-colors",
        isSelected
          ? "bg-primary/10 text-primary font-medium"
          : "text-muted-foreground hover:bg-secondary/50 hover:text-foreground"
      )}
      style={{ paddingLeft: `${8 + depth * 12}px` }}
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

      {/* Alarm count badge */}
      {node.alarmCount > 0 && (
        <Badge className="h-4 min-w-4 px-1 text-[10px] bg-severity-critical/15 text-severity-critical border-severity-critical/30 shrink-0">
          {node.alarmCount}
        </Badge>
      )}
    </div>
  );

  return (
    <div>
      {node.type === "inverter" ? (
        <Link href={`${basePath}/${node.id}`}>{content}</Link>
      ) : (
        <div onClick={() => setExpanded(!expanded)}>{content}</div>
      )}

      {/* Children */}
      {hasChildren && expanded && (
        <div>
          {node.children!.map((child) => (
            <TreeNode
              key={child.id}
              node={child}
              selectedInverterId={selectedInverterId}
              depth={depth + 1}
            />
          ))}
        </div>
      )}
    </div>
  );
}

export function SolarInvertersSidebar({
  tree,
  selectedInverterId,
}: SolarInvertersSidebarProps) {
  const pathname = usePathname();
  const segments = pathname
    .replace(basePath, "")
    .replace(/^\//, "")
    .split("/")
    .filter(Boolean);

  // Determine active menu item: only "energy-analytics" and "performance" are explicit views
  const viewSegment = segments[0];
  const activeItem: SolarInverterView =
    viewSegment === "energy-analytics" || viewSegment === "performance"
      ? viewSegment
      : "device-list";
  const showTree = tree && tree.length > 0;

  return (
    <aside className="w-[240px] shrink-0 border-r border-border bg-background overflow-y-auto">
      <div className="px-3 py-4">
        <p className="text-[11px] font-semibold uppercase tracking-wider text-muted-foreground mb-3 px-2">
          Solar & Inverters
        </p>
        <nav className="space-y-0.5">
          {menuItems.map((item) => {
            const isActive = item.id === activeItem;
            const href = item.id === "device-list" ? basePath : `${basePath}/${item.id}`;
            return (
              <Link
                key={item.id}
                href={href}
                className={cn(
                  "block w-full text-left px-3 py-2 rounded-md text-sm transition-colors",
                  isActive
                    ? "bg-primary/10 text-primary font-medium"
                    : "text-muted-foreground hover:bg-secondary/50 hover:text-foreground"
                )}
              >
                {item.label}
              </Link>
            );
          })}
        </nav>
      </div>

      {/* Device Tree */}
      {showTree && (
        <div className="px-3 pb-4">
          <div className="border-t border-border pt-3">
            <p className="text-[11px] font-semibold uppercase tracking-wider text-muted-foreground mb-3 px-2">
              Device Tree
            </p>
            <nav className="space-y-0.5">
              {tree.map((node) => (
                <TreeNode
                  key={node.id}
                  node={node}
                  selectedInverterId={selectedInverterId}
                />
              ))}
            </nav>
          </div>
        </div>
      )}
    </aside>
  );
}
