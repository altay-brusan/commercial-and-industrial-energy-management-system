"use client";

import { useState } from "react";
import { ChevronLeft } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Progress } from "@/components/ui/progress";
import { Activity } from "lucide-react";
import { AbbrTooltip } from "@/components/ui/abbr-tooltip";
import { BessAlarmsTab } from "./bess-alarms-tab";
import { PACK_TABS } from "@/lib/bess/types";
import type { BessPack, BessRack, BessUnit, BessComponentStatus } from "@/lib/bess/types";

interface BessPackDetailProps {
  pack: BessPack;
  rack: BessRack;
  unit: BessUnit;
  onBack: () => void;
  onAlarmOpen: (alarmId: string) => void;
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

function PackSummaryTab({ pack }: { pack: BessPack }) {
  const totalCells = pack.modules.reduce((sum, m) => sum + m.cells.length, 0);
  const warningCells = pack.modules.reduce(
    (sum, m) => sum + m.cells.filter((c) => c.status === "warning").length,
    0
  );
  const faultCells = pack.modules.reduce(
    (sum, m) => sum + m.cells.filter((c) => c.status === "fault").length,
    0
  );

  return (
    <div className="flex-1 overflow-y-auto px-6 py-4">
      {/* SoC */}
      <div className="mb-4 space-y-2">
        <div className="flex items-center justify-between">
          <span className="text-sm text-muted-foreground">State of Charge</span>
          <span className="text-sm font-semibold">{pack.soc}%</span>
        </div>
        <Progress value={pack.soc} className={`h-3 ${getSOCColor(pack.soc)}`} />
      </div>

      {/* Electrical */}
      <SectionTitle title="Electrical" />
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-x-8 gap-y-0">
        <Metric label={<AbbrTooltip label="SoH" title="State of Health" description="Battery's current capacity compared to its original rated capacity" />} value={`${pack.soh}%`} />
        <Metric label="Voltage" value={`${pack.voltage} V`} />
        <Metric label="Current" value={`${pack.current} A`} />
      </div>

      {/* Temperature */}
      <SectionTitle title="Temperature" />
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-x-8 gap-y-0">
        <Metric label="Temp Min" value={`${pack.tempMin}°C`} />
        <Metric label="Temp Max" value={`${pack.tempMax}°C`} />
        <Metric label="Temp Avg" value={`${pack.tempAvg}°C`} />
      </div>

      {/* Composition */}
      <SectionTitle title="Composition" />
      <div className="border rounded-md p-4 bg-muted/30">
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-x-8 gap-y-3">
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">Modules</span>
            <span className="text-sm font-medium">{pack.modules.length}</span>
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

export function BessPackDetail({ pack, rack, unit, onBack, onAlarmOpen }: BessPackDetailProps) {
  const [activeTab, setActiveTab] = useState("summary");

  return (
    <div className="flex flex-col h-full">
      {/* Header */}
      <div className="shrink-0 border-b bg-background">
        <div className="flex items-center gap-3 px-6 py-4">
          <Button variant="ghost" size="icon" className="h-8 w-8" onClick={onBack}>
            <ChevronLeft className="h-4 w-4" />
          </Button>
          <h2 className="text-lg font-semibold">Pack {pack.packNumber}</h2>
          <ComponentStatusBadge status={pack.status} />
          <span className="text-sm text-muted-foreground">
            {unit.name} &gt; Rack {rack.rackNumber}
          </span>
        </div>

        {/* Tabs */}
        <div className="px-6 flex gap-0 border-b-0">
          {PACK_TABS.map((tab) => (
            <button
              key={tab.id}
              onClick={() => !tab.disabled && setActiveTab(tab.id)}
              disabled={tab.disabled}
              className={`px-4 pb-2 text-sm transition-colors border-b-2 ${
                activeTab === tab.id
                  ? "border-foreground text-foreground font-medium"
                  : tab.disabled
                    ? "border-transparent text-muted-foreground/50 cursor-not-allowed"
                    : "border-transparent text-muted-foreground hover:text-foreground cursor-pointer"
              }`}
            >
              {tab.label}
            </button>
          ))}
        </div>
      </div>

      {/* Tab content */}
      {activeTab === "summary" && <PackSummaryTab pack={pack} />}
      {activeTab === "alarms" && (
        <BessAlarmsTab
          unitId={unit.id}
          onAlarmOpen={onAlarmOpen}
          sourceFilter={`Rack ${rack.rackNumber} / Pack ${pack.packNumber}`}
        />
      )}
    </div>
  );
}
