"use client";

import { useState, useCallback } from "react";
import { ChevronLeft } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Progress } from "@/components/ui/progress";
import { Activity } from "lucide-react";
import { AbbrTooltip } from "@/components/ui/abbr-tooltip";
import {
  Bar,
  BarChart,
  CartesianGrid,
  XAxis,
  YAxis,
  Cell as RechartsCell,
  ResponsiveContainer,
} from "recharts";
import {
  ChartConfig,
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
} from "@/components/ui/chart";
import { cn } from "@/lib/utils";
import { BessAlarmsTab } from "./bess-alarms-tab";
import { BessHierarchyTable } from "./bess-hierarchy-table";
import { BessCellDetail } from "./bess-cell-detail";
import { MODULE_TABS } from "@/lib/bess/types";
import type {
  BessModule,
  BessPack,
  BessRack,
  BessUnit,
  BessCell,
  BessComponentStatus,
} from "@/lib/bess/types";

interface BessModuleDetailProps {
  module: BessModule;
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

function getTempColor(temp: number): string {
  if (temp >= 40) return "bg-energy-grid";
  if (temp >= 35) return "bg-energy-consumption";
  if (temp >= 30) return "bg-energy-consumption/60";
  if (temp >= 25) return "bg-energy-solar";
  return "bg-energy-solar/60";
}

const voltageChartConfig = {
  voltage: {
    label: "Voltage (V)",
    color: "var(--color-energy-battery)",
  },
} satisfies ChartConfig;

function ModuleSummaryTab({ module: mod }: { module: BessModule }) {
  const totalCells = mod.cells.length;
  const warningCells = mod.cells.filter((c) => c.status === "warning").length;
  const faultCells = mod.cells.filter((c) => c.status === "fault").length;

  const voltageData = mod.cells.map((c) => ({
    name: `C${c.cellNumber}`,
    voltage: c.voltage,
  }));

  return (
    <div className="flex-1 overflow-y-auto px-6 py-4">
      {/* SoC */}
      <div className="mb-4 space-y-2">
        <div className="flex items-center justify-between">
          <span className="text-sm text-muted-foreground">State of Charge</span>
          <span className="text-sm font-semibold">{mod.soc}%</span>
        </div>
        <Progress value={mod.soc} className={`h-3 ${getSOCColor(mod.soc)}`} />
      </div>

      {/* Electrical */}
      <SectionTitle title="Electrical" />
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-x-8 gap-y-0">
        <Metric label={<AbbrTooltip label="SoH" title="State of Health" description="Battery's current capacity compared to its original rated capacity" />} value={`${mod.soh}%`} />
        <Metric label="Voltage" value={`${mod.voltage} V`} />
        <Metric label="Current" value={`${mod.current} A`} />
      </div>

      {/* Temperature */}
      <SectionTitle title="Temperature" />
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-x-8 gap-y-0">
        <Metric label="Temp Min" value={`${mod.tempMin}°C`} />
        <Metric label="Temp Max" value={`${mod.tempMax}°C`} />
        <Metric label="Temp Avg" value={`${mod.tempAvg}°C`} />
      </div>

      {/* Composition */}
      <SectionTitle title="Composition" />
      <div className="border rounded-md p-4 bg-muted/30">
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-x-8 gap-y-3">
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

      {/* Voltage Distribution */}
      <SectionTitle title="Voltage Distribution" />
      <ChartContainer config={voltageChartConfig} className="h-[200px] w-full">
        <ResponsiveContainer width="100%" height="100%">
          <BarChart data={voltageData} margin={{ top: 10, right: 10, left: -15, bottom: 0 }}>
            <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
            <XAxis dataKey="name" tickLine={false} axisLine={false} tickMargin={8} className="text-xs" />
            <YAxis tickLine={false} axisLine={false} tickMargin={8} className="text-xs" domain={["dataMin - 0.05", "dataMax + 0.05"]} />
            <ChartTooltip content={<ChartTooltipContent />} />
            <Bar dataKey="voltage" radius={[4, 4, 0, 0]}>
              {voltageData.map((_, index) => (
                <RechartsCell
                  key={index}
                  fill="var(--color-energy-battery)"
                  fillOpacity={0.7}
                />
              ))}
            </Bar>
          </BarChart>
        </ResponsiveContainer>
      </ChartContainer>

      {/* Temperature Heatmap */}
      <SectionTitle title="Temperature Heatmap" />
      <div className="grid grid-cols-7 gap-1.5">
        {mod.cells.map((c) => (
          <div
            key={c.id}
            className={cn(
              "relative flex flex-col items-center justify-center rounded-md p-2 text-xs",
              getTempColor(c.temperature)
            )}
          >
            <span className="font-medium text-white">C{c.cellNumber}</span>
            <span className="text-white/80 text-[10px]">{c.temperature}°C</span>
          </div>
        ))}
      </div>
      <div className="flex items-center justify-center gap-4 text-xs text-muted-foreground mt-2">
        <div className="flex items-center gap-1.5">
          <div className="w-3 h-3 rounded bg-energy-solar/60" />
          <span>&lt;25°C</span>
        </div>
        <div className="flex items-center gap-1.5">
          <div className="w-3 h-3 rounded bg-energy-solar" />
          <span>25-30°C</span>
        </div>
        <div className="flex items-center gap-1.5">
          <div className="w-3 h-3 rounded bg-energy-consumption/60" />
          <span>30-35°C</span>
        </div>
        <div className="flex items-center gap-1.5">
          <div className="w-3 h-3 rounded bg-energy-consumption" />
          <span>35-40°C</span>
        </div>
        <div className="flex items-center gap-1.5">
          <div className="w-3 h-3 rounded bg-energy-grid" />
          <span>&gt;40°C</span>
        </div>
      </div>

      <div className="h-6" />
    </div>
  );
}

function ModuleCellsTab({
  module: mod,
  selectedCell,
  onSelectCell,
}: {
  module: BessModule;
  selectedCell: BessCell | null;
  onSelectCell: (cell: BessCell | null) => void;
}) {
  const handleCellSelect = useCallback((item: unknown) => {
    onSelectCell(item as BessCell);
  }, [onSelectCell]);

  return (
    <div className="flex-1 overflow-y-auto px-6 py-4">
      {selectedCell ? (
        <>
          <button
            onClick={() => onSelectCell(null)}
            className="text-sm text-muted-foreground hover:text-foreground mb-4 flex items-center gap-1"
          >
            <ChevronLeft className="h-3.5 w-3.5" />
            Back to cells
          </button>
          <BessCellDetail cell={selectedCell} module={mod} />
        </>
      ) : (
        <BessHierarchyTable
          level="module"
          items={mod.cells}
          onItemSelect={handleCellSelect}
        />
      )}
    </div>
  );
}

export function BessModuleDetail({ module: mod, pack, rack, unit, onBack, onAlarmOpen }: BessModuleDetailProps) {
  const [activeTab, setActiveTab] = useState("summary");
  const [selectedCell, setSelectedCell] = useState<BessCell | null>(null);

  const handleTabChange = useCallback((tabId: string) => {
    setActiveTab(tabId);
    if (tabId !== "cells") {
      setSelectedCell(null);
    }
  }, []);

  const headerTitle = selectedCell
    ? `Cell ${selectedCell.cellNumber}`
    : `Module ${mod.moduleNumber}`;

  const headerStatus = selectedCell ? selectedCell.status : mod.status;

  const breadcrumb = selectedCell
    ? `${unit.name} > Rack ${rack.rackNumber} > Pack ${pack.packNumber} > Module ${mod.moduleNumber}`
    : `${unit.name} > Rack ${rack.rackNumber} > Pack ${pack.packNumber}`;

  return (
    <div className="flex flex-col h-full">
      {/* Header */}
      <div className="shrink-0 border-b bg-background">
        <div className="flex items-center gap-3 px-6 py-4">
          <Button variant="ghost" size="icon" className="h-8 w-8" onClick={onBack}>
            <ChevronLeft className="h-4 w-4" />
          </Button>
          <h2 className="text-lg font-semibold">{headerTitle}</h2>
          <ComponentStatusBadge status={headerStatus} />
          <span className="text-sm text-muted-foreground">
            {breadcrumb}
          </span>
        </div>

        {/* Tabs */}
        <div className="px-6 flex gap-0 border-b-0">
          {MODULE_TABS.map((tab) => (
            <button
              key={tab.id}
              onClick={() => !tab.disabled && handleTabChange(tab.id)}
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
      {activeTab === "summary" && <ModuleSummaryTab module={mod} />}
      {activeTab === "cells" && (
        <ModuleCellsTab
          module={mod}
          selectedCell={selectedCell}
          onSelectCell={setSelectedCell}
        />
      )}
      {activeTab === "alarms" && (
        <BessAlarmsTab
          unitId={unit.id}
          onAlarmOpen={onAlarmOpen}
          sourceFilter={`Rack ${rack.rackNumber} / Pack ${pack.packNumber} / Module ${mod.moduleNumber}`}
        />
      )}
    </div>
  );
}
