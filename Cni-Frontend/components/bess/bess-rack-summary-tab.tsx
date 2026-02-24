"use client";

import { Activity } from "lucide-react";
import { AbbrTooltip } from "@/components/ui/abbr-tooltip";
import { Badge } from "@/components/ui/badge";
import { Progress } from "@/components/ui/progress";
import type { BessRack, BessComponentStatus } from "@/lib/bess/types";

interface BessRackSummaryTabProps {
  rack: BessRack;
}

function ComponentStatusBadge({ status }: { status: BessComponentStatus }) {
  const styles: Record<BessComponentStatus, string> = {
    normal: "bg-status-operational/15 text-status-operational border-status-operational/30",
    warning: "bg-status-warning/15 text-status-warning border-status-warning/30",
    fault: "bg-status-critical/15 text-status-critical border-status-critical/30",
    offline: "bg-muted text-muted-foreground",
  };
  return <Badge className={styles[status]}>{status}</Badge>;
}

function getSOCColor(soc: number): string {
  if (soc >= 60) return "[&>[data-slot=progress-indicator]]:bg-status-operational";
  if (soc >= 30) return "[&>[data-slot=progress-indicator]]:bg-severity-high";
  return "[&>[data-slot=progress-indicator]]:bg-severity-critical";
}

function Metric({ label, value }: { label: React.ReactNode; value: React.ReactNode }) {
  return (
    <div className="flex items-center justify-between py-2">
      <span className="text-sm text-muted-foreground">{label}</span>
      <div className="flex items-center gap-1.5">
        <span className="text-sm font-medium">{value}</span>
        <Activity className="h-3.5 w-3.5 text-muted-foreground/40" />
      </div>
    </div>
  );
}

function SectionTitle({ title }: { title: string }) {
  return (
    <h3 className="text-sm font-semibold text-foreground mt-6 mb-2 first:mt-0">
      {title}
    </h3>
  );
}

export function BessRackSummaryTab({ rack }: BessRackSummaryTabProps) {
  const totalCells = rack.packs.reduce(
    (sum, p) => sum + p.modules.reduce((ms, m) => ms + m.cells.length, 0),
    0
  );
  const warningCells = rack.packs.reduce(
    (sum, p) =>
      sum +
      p.modules.reduce(
        (ms, m) => ms + m.cells.filter((c) => c.status === "warning").length,
        0
      ),
    0
  );
  const faultCells = rack.packs.reduce(
    (sum, p) =>
      sum +
      p.modules.reduce(
        (ms, m) => ms + m.cells.filter((c) => c.status === "fault").length,
        0
      ),
    0
  );

  return (
    <div className="flex-1 overflow-y-auto px-6 py-4">
      {/* Header info */}
      <div className="flex items-center gap-3 mb-4">
        <h4 className="text-sm font-semibold">Rack {rack.rackNumber}</h4>
        <ComponentStatusBadge status={rack.status} />
      </div>

      {/* SoC */}
      <div className="mb-4 space-y-2">
        <div className="flex items-center justify-between">
          <span className="text-sm text-muted-foreground">State of Charge</span>
          <span className="text-sm font-semibold">{rack.soc}%</span>
        </div>
        <Progress value={rack.soc} className={`h-3 ${getSOCColor(rack.soc)}`} />
      </div>

      {/* Key metrics */}
      <SectionTitle title="Electrical" />
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-x-8 gap-y-0">
        <Metric label={<AbbrTooltip label="SoH" title="State of Health" description="Battery's current capacity compared to its original rated capacity" />} value={`${rack.soh}%`} />
        <Metric label="Voltage" value={`${rack.voltage} V`} />
        <Metric label="Current" value={`${rack.current} A`} />
        <Metric label="Power" value={`${rack.power} kW`} />
      </div>

      {/* Temperature */}
      <SectionTitle title="Temperature" />
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-x-8 gap-y-0">
        <Metric label="Temp Min" value={`${rack.tempMin}°C`} />
        <Metric label="Temp Max" value={`${rack.tempMax}°C`} />
        <Metric label="Temp Avg" value={`${rack.tempAvg}°C`} />
      </div>

      {/* Composition */}
      <SectionTitle title="Composition" />
      <div className="border rounded-md p-4 bg-muted/30">
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-x-8 gap-y-3">
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">Packs</span>
            <span className="text-sm font-medium">{rack.packs.length}</span>
          </div>
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">Total Cells</span>
            <span className="text-sm font-medium">{totalCells}</span>
          </div>
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">Fault Cells</span>
            <span className="text-sm font-medium text-severity-critical">{faultCells}</span>
          </div>
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">Warning Cells</span>
            <span className="text-sm font-medium text-severity-high">{warningCells}</span>
          </div>
        </div>
      </div>

      <div className="h-6" />
    </div>
  );
}
