"use client";

import { useMemo } from "react";
import {
  Area,
  AreaChart,
  Bar,
  BarChart,
  CartesianGrid,
  Line,
  LineChart,
  ReferenceLine,
  XAxis,
  YAxis,
} from "recharts";
import {
  type ChartConfig,
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
} from "@/components/ui/chart";
import {
  getNetPower,
  getTotalImportedToday,
  getTotalExportedToday,
  getPeakDemandToday,
  getAvgPowerFactor,
  getDeviceStatusSummary,
  getGridPowerCurve,
  getDeviceEnergyComparison,
  getMonthlyEnergyBalance,
  getDailyPeakDemand,
} from "@/lib/grid-loads/analytics-mock-data";
import { gridDevices } from "@/lib/grid-loads/mock-data";

const powerFlowConfig = {
  imported: {
    label: "Imported (kW)",
    color: "var(--color-energy-consumption)",
  },
  exported: {
    label: "Exported (kW)",
    color: "var(--color-energy-solar)",
  },
} satisfies ChartConfig;

const deviceEnergyConfig = {
  imported: {
    label: "Imported (kWh)",
    color: "var(--color-energy-consumption)",
  },
  exported: {
    label: "Exported (kWh)",
    color: "var(--color-energy-solar)",
  },
} satisfies ChartConfig;

const monthlyBalanceConfig = {
  imported: {
    label: "Imported (MWh)",
    color: "var(--color-energy-consumption)",
  },
  exported: {
    label: "Exported (MWh)",
    color: "var(--color-energy-solar)",
  },
} satisfies ChartConfig;

const peakDemandConfig = {
  peakDemand: {
    label: "Peak Demand (kW)",
    color: "var(--color-energy-consumption)",
  },
} satisfies ChartConfig;

export function EnergyAnalyticsView() {
  const netPower = useMemo(() => getNetPower(), []);
  const importedToday = useMemo(() => getTotalImportedToday(), []);
  const exportedToday = useMemo(() => getTotalExportedToday(), []);
  const peakDemand = useMemo(() => getPeakDemandToday(), []);
  const avgPF = useMemo(() => getAvgPowerFactor(), []);
  const statusSummary = useMemo(() => getDeviceStatusSummary(), []);
  const powerCurve = useMemo(() => getGridPowerCurve(), []);
  const deviceEnergy = useMemo(() => getDeviceEnergyComparison(), []);
  const monthlyBalance = useMemo(() => getMonthlyEnergyBalance(), []);
  const dailyPeak = useMemo(() => getDailyPeakDemand(), []);

  const stats = [
    { label: "Net Power", value: (netPower / 1000).toFixed(1), unit: "MW" },
    { label: "Imported Today", value: (importedToday / 1000).toFixed(1), unit: "MWh" },
    { label: "Exported Today", value: (exportedToday / 1000).toFixed(1), unit: "MWh" },
    { label: "Peak Demand Today", value: peakDemand.toLocaleString("en-US"), unit: "kW" },
    { label: "Avg Power Factor", value: avgPF.toFixed(2), unit: "" },
  ];

  return (
    <div className="flex flex-col h-full overflow-hidden">
      {/* Stats Bar */}
      <div className="shrink-0 grid grid-cols-5 border-b border-border">
        {stats.map((stat, i) => (
          <div
            key={stat.label}
            className={`px-6 py-5 ${i < stats.length - 1 ? "border-r border-border" : ""}`}
          >
            <p className="text-sm text-muted-foreground mb-1">{stat.label}</p>
            <div className="flex items-baseline gap-2">
              <span className="text-3xl font-semibold tracking-tight">{stat.value}</span>
              <span className="text-sm text-muted-foreground">{stat.unit}</span>
            </div>
          </div>
        ))}
      </div>

      {/* Content */}
      <main className="flex-1 overflow-y-auto">
        {/* Power Flow Over Time */}
        <div className="px-6 py-6 border-b border-border">
          <div className="flex items-center justify-between mb-4">
            <div>
              <h2 className="text-lg font-semibold">Power Flow Over Time</h2>
              <p className="text-sm text-muted-foreground">Bidirectional power flow - import (orange) / export (green)</p>
            </div>
            <div className="flex items-center gap-5 text-sm">
              <div className="flex items-center gap-1.5">
                <span className="h-2 w-2 rounded-full bg-energy-solar" />
                <span className="text-muted-foreground">{statusSummary.normal} Normal</span>
              </div>
              <div className="flex items-center gap-1.5">
                <span className="h-2 w-2 rounded-full bg-muted-foreground" />
                <span className="text-muted-foreground">{statusSummary.offline} Offline</span>
              </div>
              <div className="flex items-center gap-1.5">
                <span className="h-2 w-2 rounded-full bg-energy-consumption" />
                <span className="text-muted-foreground">{statusSummary.warning} Warning</span>
              </div>
              <div className="flex items-center gap-1.5">
                <span className="h-2 w-2 rounded-full bg-energy-grid" />
                <span className="text-muted-foreground">{statusSummary.fault} Fault</span>
              </div>
              <span className="text-xs text-muted-foreground">{gridDevices.length} devices</span>
            </div>
          </div>
          <ChartContainer config={powerFlowConfig} className="h-[280px] w-full">
            <AreaChart data={powerCurve} margin={{ top: 10, right: 10, left: 0, bottom: 0 }}>
              <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
              <XAxis
                dataKey="time"
                tickLine={false}
                axisLine={false}
                tickMargin={8}
                className="text-xs"
                interval={Math.max(1, Math.floor(powerCurve.length / 12))}
              />
              <YAxis tickLine={false} axisLine={false} tickMargin={8} className="text-xs" />
              <ReferenceLine y={0} stroke="var(--color-border)" strokeWidth={1} />
              <ChartTooltip content={<ChartTooltipContent indicator="line" />} />
              <Area
                type="monotone"
                dataKey="imported"
                stroke="var(--color-energy-consumption)"
                fill="var(--color-energy-consumption)"
                fillOpacity={0.3}
              />
              <Area
                type="monotone"
                dataKey="exported"
                stroke="var(--color-energy-solar)"
                fill="var(--color-energy-solar)"
                fillOpacity={0.3}
              />
            </AreaChart>
          </ChartContainer>
        </div>

        {/* Device Energy Comparison + Monthly Balance */}
        <div className="px-6 py-6 border-b border-border">
          <div className="grid gap-8 lg:grid-cols-2">
            {/* Import/Export Comparison */}
            <div>
              <div className="mb-4">
                <h2 className="text-lg font-semibold">Import/Export Comparison</h2>
                <p className="text-sm text-muted-foreground">Energy per device today (kWh)</p>
              </div>
              <ChartContainer config={deviceEnergyConfig} className="h-[280px] w-full">
                <BarChart data={deviceEnergy} layout="vertical" margin={{ top: 10, right: 10, left: 0, bottom: 0 }}>
                  <CartesianGrid strokeDasharray="3 3" className="stroke-border" horizontal={false} />
                  <XAxis type="number" tickLine={false} axisLine={false} className="text-xs" />
                  <YAxis dataKey="name" type="category" tickLine={false} axisLine={false} width={60} className="text-xs" />
                  <ChartTooltip content={<ChartTooltipContent />} />
                  <Bar dataKey="imported" fill="var(--color-energy-consumption)" radius={[0, 4, 4, 0]} />
                  <Bar dataKey="exported" fill="var(--color-energy-solar)" radius={[0, 4, 4, 0]} />
                </BarChart>
              </ChartContainer>
            </div>

            {/* Monthly Energy Balance */}
            <div>
              <div className="mb-4">
                <h2 className="text-lg font-semibold">Monthly Energy Balance</h2>
                <p className="text-sm text-muted-foreground">Last 12 months (MWh)</p>
              </div>
              <ChartContainer config={monthlyBalanceConfig} className="h-[280px] w-full">
                <BarChart data={monthlyBalance} margin={{ top: 10, right: 10, left: 0, bottom: 0 }}>
                  <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
                  <XAxis dataKey="month" tickLine={false} axisLine={false} tickMargin={8} className="text-xs" />
                  <YAxis tickLine={false} axisLine={false} tickMargin={8} className="text-xs" />
                  <ChartTooltip content={<ChartTooltipContent />} />
                  <Bar dataKey="imported" stackId="a" fill="var(--color-energy-consumption)" radius={[0, 0, 0, 0]} />
                  <Bar dataKey="exported" stackId="a" fill="var(--color-energy-solar)" radius={[4, 4, 0, 0]} />
                </BarChart>
              </ChartContainer>
            </div>
          </div>
        </div>

        {/* Daily Peak Demand */}
        <div className="px-6 py-6">
          <div className="mb-4">
            <h2 className="text-lg font-semibold">Daily Peak Demand Trend</h2>
            <p className="text-sm text-muted-foreground">Last 30 days peak demand (kW)</p>
          </div>
          <ChartContainer config={peakDemandConfig} className="h-[260px] w-full">
            <LineChart data={dailyPeak} margin={{ top: 10, right: 10, left: 0, bottom: 0 }}>
              <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
              <XAxis dataKey="date" tickLine={false} axisLine={false} tickMargin={8} className="text-xs" interval={4} />
              <YAxis tickLine={false} axisLine={false} tickMargin={8} className="text-xs" />
              <ReferenceLine y={5000} stroke="var(--color-energy-grid)" strokeDasharray="4 4" label={{ value: "Contracted Limit", position: "right", className: "text-xs fill-energy-grid" }} />
              <ChartTooltip content={<ChartTooltipContent indicator="line" />} />
              <Line type="monotone" dataKey="peakDemand" stroke="var(--color-energy-consumption)" strokeWidth={2} dot={false} />
            </LineChart>
          </ChartContainer>
        </div>
      </main>
    </div>
  );
}
