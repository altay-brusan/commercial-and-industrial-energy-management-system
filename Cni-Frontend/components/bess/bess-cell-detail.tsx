"use client";

import { useMemo } from "react";
import { Activity, AlertTriangle } from "lucide-react";
import { AbbrTooltip } from "@/components/ui/abbr-tooltip";
import { Badge } from "@/components/ui/badge";
import { Progress } from "@/components/ui/progress";
import type { BessCell, BessModule, BessComponentStatus } from "@/lib/bess/types";

interface BessCellDetailProps {
  cell: BessCell;
  module: BessModule;
}

function ComponentStatusBadge({ status }: { status: BessComponentStatus }) {
  const styles: Record<BessComponentStatus, string> = {
    normal: "bg-energy-solar/15 text-energy-solar border-energy-solar/30",
    warning: "bg-energy-consumption/15 text-energy-consumption border-energy-consumption/30",
    fault: "bg-energy-grid/15 text-energy-grid border-energy-grid/30",
    offline: "bg-muted text-muted-foreground",
  };
  return <Badge className={`capitalize ${styles[status]}`}>{status}</Badge>;
}

function getSOCColor(soc: number): string {
  if (soc >= 60) return "[&>[data-slot=progress-indicator]]:bg-energy-solar";
  if (soc >= 30) return "[&>[data-slot=progress-indicator]]:bg-energy-consumption";
  return "[&>[data-slot=progress-indicator]]:bg-energy-grid";
}

function Metric({ label, value, icon }: { label: React.ReactNode; value: React.ReactNode; icon?: boolean }) {
  return (
    <div className="flex items-center justify-between py-1.5">
      <span className="text-sm text-muted-foreground">{label}</span>
      <div className="flex items-center gap-2">
        <span className="text-sm font-medium">{value}</span>
        {icon !== false && <Activity className="h-3.5 w-3.5 text-muted-foreground/50" />}
      </div>
    </div>
  );
}

export function BessCellDetail({ cell, module }: BessCellDetailProps) {
  const cells = module.cells;

  // Outlier detection
  const avgVoltage = cells.reduce((s, c) => s + c.voltage, 0) / cells.length;
  const avgTemp = cells.reduce((s, c) => s + c.temperature, 0) / cells.length;

  const outliers = useMemo(() => {
    const results: string[] = [];
    if (Math.abs(cell.voltage - avgVoltage) > 0.05) {
      results.push(`Voltage deviation: ${((cell.voltage - avgVoltage) * 1000).toFixed(0)} mV from module avg`);
    }
    if (Math.abs(cell.temperature - avgTemp) > 5) {
      results.push(`Temperature deviation: ${(cell.temperature - avgTemp).toFixed(1)}°C from module avg`);
    }
    return results;
  }, [cell, avgVoltage, avgTemp]);

  return (
    <div className="space-y-6">
      {/* Outlier warnings */}
      {outliers.length > 0 && (
        <div className="space-y-2">
          {outliers.map((msg, i) => (
            <div key={i} className="flex items-center gap-2 px-3 py-2 rounded-md bg-energy-consumption/10 border border-energy-consumption/30">
              <AlertTriangle className="h-4 w-4 text-energy-consumption shrink-0" />
              <span className="text-sm text-energy-consumption">{msg}</span>
            </div>
          ))}
        </div>
      )}

      {/* Cell metrics */}
      <div className="border rounded-md p-4 bg-muted/20">
        <div className="flex items-center justify-between mb-3">
          <h4 className="text-sm font-semibold">Cell {cell.cellNumber} Metrics</h4>
          <ComponentStatusBadge status={cell.status} />
        </div>

        <div className="mb-3 space-y-1.5">
          <div className="flex items-center justify-between">
            <span className="text-xs text-muted-foreground"><AbbrTooltip label="SoC" title="State of Charge" description="Current battery charge level as a percentage of total capacity" /></span>
            <span className="text-sm font-semibold">{cell.soc}%</span>
          </div>
          <Progress value={cell.soc} className={`h-2 ${getSOCColor(cell.soc)}`} />
        </div>

        <div className="grid grid-cols-2 lg:grid-cols-3 gap-x-6">
          <Metric label="Voltage" value={`${cell.voltage} V`} />
          <Metric label="Current" value={`${cell.current} A`} />
          <Metric label="Temperature" value={`${cell.temperature}°C`} />
          <Metric label={<AbbrTooltip label="SoH" title="State of Health" description="Battery's current capacity compared to its original rated capacity" />} value={`${cell.soh}%`} />
          <Metric label="Internal Res." value={`${cell.internalResistance} mΩ`} />
          <Metric label="Cycle Count" value={cell.cycleCount.toLocaleString("en-US")} />
          <Metric label="Balancing" value={cell.balancing ? "Active" : "Inactive"} icon={false} />
        </div>
      </div>
    </div>
  );
}
