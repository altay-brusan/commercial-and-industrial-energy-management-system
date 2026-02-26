"use client";

import { useState, useEffect, useMemo, useCallback } from "react";
import {
  ChevronRight,
  Loader2,
  Battery,
  BatteryCharging,
  Server,
  Package,
  CircuitBoard,
  Cpu,
  Thermometer,
} from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";
import * as bessService from "@/lib/bess/bess-service";
import type { BessTreeNodeDto } from "@/lib/bess/api-types";

interface BessHierarchyTabProps {
  deviceId: string;
}

interface BreadcrumbSegment {
  label: string;
  nodeId: string;
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

function statusBadge(status: string) {
  const map: Record<string, string> = {
    normal: "bg-status-operational/15 text-status-operational border-status-operational/30",
    online: "bg-status-operational/15 text-status-operational border-status-operational/30",
    warning: "bg-status-warning/15 text-status-warning border-status-warning/30",
    fault: "bg-status-critical/15 text-status-critical border-status-critical/30",
    critical: "bg-status-critical/15 text-status-critical border-status-critical/30",
    offline: "bg-muted text-muted-foreground",
  };
  const cls = map[status] ?? "bg-muted text-muted-foreground";
  return (
    <Badge className={cn("capitalize gap-1.5", cls)}>
      <span className={cn("h-2 w-2 rounded-full", {
        "bg-status-operational": status === "normal" || status === "online",
        "bg-status-warning": status === "warning",
        "bg-status-critical": status === "fault" || status === "critical",
        "bg-muted-foreground": status === "offline",
      })} />
      {status}
    </Badge>
  );
}

function findNodeById(nodes: BessTreeNodeDto[], id: string): BessTreeNodeDto | null {
  for (const node of nodes) {
    if (node.id === id) return node;
    const found = findNodeById(node.children, id);
    if (found) return found;
  }
  return null;
}

function buildPath(nodes: BessTreeNodeDto[], targetId: string, path: BessTreeNodeDto[] = []): BessTreeNodeDto[] | null {
  for (const node of nodes) {
    const newPath = [...path, node];
    if (node.id === targetId) return newPath;
    const found = buildPath(node.children, targetId, newPath);
    if (found) return found;
  }
  return null;
}

export function BessHierarchyTab({ deviceId }: BessHierarchyTabProps) {
  const [tree, setTree] = useState<BessTreeNodeDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [currentNodeId, setCurrentNodeId] = useState<string>(deviceId);

  useEffect(() => {
    setLoading(true);
    bessService.getTree().then((res) => {
      setTree(res.tree);
    }).catch(() => {}).finally(() => setLoading(false));
  }, [deviceId]);

  const currentNode = useMemo(() => findNodeById(tree, currentNodeId), [tree, currentNodeId]);
  const children = currentNode?.children ?? [];

  const breadcrumbs: BreadcrumbSegment[] = useMemo(() => {
    const path = buildPath(tree, currentNodeId);
    if (!path) return [{ label: "Unit", nodeId: deviceId }];
    return path.map((n) => ({ label: n.name, nodeId: n.id }));
  }, [tree, currentNodeId, deviceId]);

  const handleNodeClick = useCallback((nodeId: string) => {
    setCurrentNodeId(nodeId);
  }, []);

  if (loading) {
    return (
      <div className="flex-1 flex items-center justify-center">
        <Loader2 className="h-5 w-5 animate-spin text-muted-foreground" />
      </div>
    );
  }

  if (!currentNode) {
    return (
      <div className="flex-1 flex items-center justify-center text-muted-foreground py-20">
        No hierarchy data available for this unit.
      </div>
    );
  }

  return (
    <div className="flex-1 overflow-y-auto px-6 py-4">
      {/* Breadcrumb */}
      <nav className="flex items-center gap-1 text-sm mb-4 flex-wrap">
        {breadcrumbs.map((seg, i) => {
          const isLast = i === breadcrumbs.length - 1;
          return (
            <span key={seg.nodeId} className="flex items-center gap-1">
              {i > 0 && <ChevronRight className="h-3.5 w-3.5 text-muted-foreground/50" />}
              <button
                onClick={() => setCurrentNodeId(seg.nodeId)}
                className={cn(
                  "px-1.5 py-0.5 rounded transition-colors",
                  isLast
                    ? "text-foreground font-medium cursor-default"
                    : "text-muted-foreground hover:text-foreground hover:bg-muted cursor-pointer"
                )}
                disabled={isLast}
              >
                {seg.label}
              </button>
            </span>
          );
        })}
      </nav>

      {/* Current node summary */}
      <div className="border rounded-md p-4 bg-muted/20 mb-4">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-2">
            {(() => {
              const Icon = deviceIcon[currentNode.type] ?? Battery;
              return <Icon className="h-4 w-4 text-muted-foreground" />;
            })()}
            <h4 className="text-sm font-semibold">{currentNode.name}</h4>
            <Badge variant="outline" className="text-xs capitalize">{currentNode.type}</Badge>
          </div>
          <div className="flex items-center gap-2">
            {statusBadge(currentNode.status)}
            {currentNode.alarmCount > 0 && (
              <Badge className="bg-severity-critical/15 text-severity-critical border-severity-critical/30">
                {currentNode.alarmCount} alarm{currentNode.alarmCount !== 1 ? "s" : ""}
              </Badge>
            )}
          </div>
        </div>
      </div>

      {/* Children table */}
      {children.length > 0 ? (
        <div className="border rounded-md overflow-hidden">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b bg-muted/50">
                <th className="text-left px-6 py-2.5 text-xs font-medium text-muted-foreground">Name</th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-muted-foreground">Type</th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-muted-foreground">Status</th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-muted-foreground">Alarms</th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-muted-foreground">Children</th>
              </tr>
            </thead>
            <tbody>
              {children.map((child) => {
                const Icon = deviceIcon[child.type] ?? Battery;
                const hasChildren = child.children && child.children.length > 0;
                return (
                  <tr
                    key={child.id}
                    className={cn(
                      "border-b last:border-b-0 transition-colors",
                      hasChildren
                        ? "hover:bg-muted/30 cursor-pointer"
                        : "hover:bg-muted/30"
                    )}
                    onClick={() => hasChildren && handleNodeClick(child.id)}
                  >
                    <td className="px-6 py-2.5 font-medium">
                      <div className="flex items-center gap-2">
                        <Icon className="h-3.5 w-3.5 text-muted-foreground" />
                        {child.name}
                      </div>
                    </td>
                    <td className="px-4 py-2.5">
                      <Badge variant="outline" className="capitalize text-xs font-normal">{child.type}</Badge>
                    </td>
                    <td className="px-4 py-2.5">{statusBadge(child.status)}</td>
                    <td className="px-4 py-2.5">
                      {child.alarmCount > 0 ? (
                        <Badge className="bg-severity-critical/15 text-severity-critical border-severity-critical/30 text-xs">
                          {child.alarmCount}
                        </Badge>
                      ) : (
                        <span className="text-muted-foreground">0</span>
                      )}
                    </td>
                    <td className="px-4 py-2.5 text-muted-foreground">
                      {child.children?.length ?? 0}
                      {hasChildren && (
                        <ChevronRight className="inline h-3.5 w-3.5 ml-1" />
                      )}
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>

          <div className="px-6 py-2.5 border-t bg-muted/30 text-xs text-muted-foreground">
            {children.length} item{children.length !== 1 ? "s" : ""}
          </div>
        </div>
      ) : (
        <div className="text-center text-muted-foreground py-12 text-sm">
          No child devices at this level.
        </div>
      )}
    </div>
  );
}
