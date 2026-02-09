"use client";

import { useMemo } from "react";
import { Activity, AlertTriangle } from "lucide-react";
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
import { Badge } from "@/components/ui/badge";
import { Progress } from "@/components/ui/progress";
import { cn } from "@/lib/utils";
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
  return <Badge className={cn("capitalize", styles[status])}>{status}</Badge>;
}

function getSOCColor(soc: number): string {
  if (soc >= 60) return "[&>[data-slot=progress-indicator]]:bg-energy-solar";
  if (soc >= 30) return "[&>[data-slot=progress-indicator]]:bg-energy-consumption";
  return "[&>[data-slot=progress-indicator]]:bg-energy-grid";
}

function Metric({ label, value, icon }: { label: string; value: React.ReactNode; icon?: boolean }) {
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

  // Voltage bar chart data
  const voltageData = cells.map((c) => ({
    name: `C${c.cellNumber}`,
    voltage: c.voltage,
    isSelected: c.id === cell.id,
  }));

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
            <span className="text-xs text-muted-foreground">SoC</span>
            <span className="text-sm font-semibold">{cell.soc}%</span>
          </div>
          <Progress value={cell.soc} className={`h-2 ${getSOCColor(cell.soc)}`} />
        </div>

        <div className="grid grid-cols-2 lg:grid-cols-3 gap-x-6">
          <Metric label="Voltage" value={`${cell.voltage} V`} />
          <Metric label="Current" value={`${cell.current} A`} />
          <Metric label="Temperature" value={`${cell.temperature}°C`} />
          <Metric label="SoH" value={`${cell.soh}%`} />
          <Metric label="Internal Res." value={`${cell.internalResistance} mΩ`} />
          <Metric label="Cycle Count" value={cell.cycleCount.toLocaleString()} />
          <Metric label="Balancing" value={cell.balancing ? "Active" : "Inactive"} icon={false} />
        </div>
      </div>

      {/* Voltage bar chart */}
      <div className="space-y-3">
        <h4 className="text-sm font-semibold">Module Voltage Distribution</h4>
        <ChartContainer config={voltageChartConfig} className="h-[200px] w-full">
          <ResponsiveContainer width="100%" height="100%">
            <BarChart data={voltageData} margin={{ top: 10, right: 10, left: -15, bottom: 0 }}>
              <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
              <XAxis dataKey="name" tickLine={false} axisLine={false} tickMargin={8} className="text-xs" />
              <YAxis tickLine={false} axisLine={false} tickMargin={8} className="text-xs" domain={["dataMin - 0.05", "dataMax + 0.05"]} />
              <ChartTooltip content={<ChartTooltipContent />} />
              <Bar dataKey="voltage" radius={[4, 4, 0, 0]}>
                {voltageData.map((entry, index) => (
                  <RechartsCell
                    key={index}
                    fill={entry.isSelected ? "var(--color-energy-battery)" : "var(--color-energy-battery)"}
                    fillOpacity={entry.isSelected ? 1 : 0.3}
                    stroke={entry.isSelected ? "var(--color-energy-battery)" : "none"}
                    strokeWidth={entry.isSelected ? 2 : 0}
                  />
                ))}
              </Bar>
            </BarChart>
          </ResponsiveContainer>
        </ChartContainer>
      </div>

      {/* Temperature heatmap */}
      <div className="space-y-3">
        <h4 className="text-sm font-semibold">Module Temperature Heatmap</h4>
        <div className="grid grid-cols-7 gap-1.5">
          {cells.map((c) => (
            <div
              key={c.id}
              className={cn(
                "relative flex flex-col items-center justify-center rounded-md p-2 text-xs transition-all",
                getTempColor(c.temperature),
                c.id === cell.id
                  ? "ring-2 ring-foreground ring-offset-1 ring-offset-background"
                  : "opacity-80"
              )}
            >
              <span className="font-medium text-white">C{c.cellNumber}</span>
              <span className="text-white/80 text-[10px]">{c.temperature}°C</span>
            </div>
          ))}
        </div>
        <div className="flex items-center justify-center gap-4 text-xs text-muted-foreground">
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
      </div>
    </div>
  );
}
