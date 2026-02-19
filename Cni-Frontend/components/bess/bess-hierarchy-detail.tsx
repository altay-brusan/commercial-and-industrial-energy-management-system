"use client";

import { Activity } from "lucide-react";
import { AbbrTooltip } from "@/components/ui/abbr-tooltip";
import { Badge } from "@/components/ui/badge";
import { Progress } from "@/components/ui/progress";
import type { BessRack, BessPack, BessModule, BessComponentStatus } from "@/lib/bess/types";

type DetailItem = BessRack | BessPack | BessModule;

interface BessHierarchyDetailProps {
  item: DetailItem;
  label: string;
}

function ComponentStatusBadge({ status }: { status: BessComponentStatus }) {
  const styles: Record<BessComponentStatus, string> = {
    normal: "bg-energy-solar/15 text-energy-solar border-energy-solar/30",
    warning: "bg-energy-consumption/15 text-energy-consumption border-energy-consumption/30",
    fault: "bg-energy-grid/15 text-energy-grid border-energy-grid/30",
    offline: "bg-muted text-muted-foreground",
  };
  return <Badge className={styles[status]}>{status}</Badge>;
}

function getSOCColor(soc: number): string {
  if (soc >= 60) return "[&>[data-slot=progress-indicator]]:bg-energy-solar";
  if (soc >= 30) return "[&>[data-slot=progress-indicator]]:bg-energy-consumption";
  return "[&>[data-slot=progress-indicator]]:bg-energy-grid";
}

function Metric({ label, value }: { label: React.ReactNode; value: React.ReactNode }) {
  return (
    <div className="flex items-center justify-between py-1.5">
      <span className="text-xs text-muted-foreground">{label}</span>
      <div className="flex items-center gap-1.5">
        <span className="text-sm font-medium">{value}</span>
        <Activity className="h-3 w-3 text-muted-foreground/40" />
      </div>
    </div>
  );
}

export function BessHierarchyDetail({ item, label }: BessHierarchyDetailProps) {
  return (
    <div className="border rounded-md p-4 bg-muted/20 mb-4">
      <div className="flex items-center justify-between mb-3">
        <h4 className="text-sm font-semibold">{label}</h4>
        <ComponentStatusBadge status={item.status} />
      </div>

      <div className="mb-3 space-y-1.5">
        <div className="flex items-center justify-between">
          <span className="text-xs text-muted-foreground"><AbbrTooltip label="SoC" title="State of Charge" description="Current battery charge level as a percentage of total capacity" /></span>
          <span className="text-sm font-semibold">{item.soc}%</span>
        </div>
        <Progress value={item.soc} className={`h-2 ${getSOCColor(item.soc)}`} />
      </div>

      <div className="grid grid-cols-2 lg:grid-cols-3 gap-x-6">
        <Metric label={<AbbrTooltip label="SoH" title="State of Health" description="Battery's current capacity compared to its original rated capacity" />} value={`${item.soh}%`} />
        <Metric label="Voltage" value={`${item.voltage} V`} />
        <Metric label="Current" value={`${item.current} A`} />
        {"power" in item && <Metric label="Power" value={`${item.power} kW`} />}
        <Metric label="Temp Min" value={`${item.tempMin}°C`} />
        <Metric label="Temp Max" value={`${item.tempMax}°C`} />
        <Metric label="Temp Avg" value={`${item.tempAvg}°C`} />
      </div>
    </div>
  );
}
