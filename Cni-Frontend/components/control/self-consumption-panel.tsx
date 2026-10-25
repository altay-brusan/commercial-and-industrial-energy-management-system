"use client"

import { useState, useMemo } from "react"
import {
  AreaChart,
  Area,
  XAxis,
  YAxis,
  CartesianGrid,
  Line,
} from "recharts"
import {
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
  type ChartConfig,
} from "@/components/ui/chart"
import { cn } from "@/lib/utils"
import { fmtNum } from "@/lib/utils"
import { ControlSidebar } from "@/components/control/control-sidebar"
import { getSelfConsumptionHourlyData } from "@/lib/control/mock-data"
import { Battery, Zap } from "lucide-react"

type Priority = "ess-first" | "grid-first"

const chartConfig = {
  solar: { label: "Solar (kW)", color: "var(--color-energy-solar)" },
  load: { label: "Load (kW)", color: "var(--color-energy-consumption)" },
  battery: { label: "Battery (kW)", color: "var(--color-energy-battery)" },
  grid: { label: "Grid (kW)", color: "var(--color-energy-grid)" },
} satisfies ChartConfig

export function SelfConsumptionPanel() {
  const [priority, setPriority] = useState<Priority>("ess-first")
  const chartData = useMemo(() => getSelfConsumptionHourlyData(), [])

  const totalSolar = chartData.reduce((s, d) => s + d.solar, 0)
  const totalLoad = chartData.reduce((s, d) => s + d.load, 0)
  const totalGrid = chartData.reduce((s, d) => s + d.grid, 0)
  const selfConsumptionRatio = totalLoad > 0 ? Math.round(((totalLoad - totalGrid) / totalLoad) * 100) : 0
  const solarUtilization = totalSolar > 0 ? Math.round((Math.min(totalSolar, totalLoad) / totalSolar) * 100) : 0
  const gridDependency = totalLoad > 0 ? Math.round((totalGrid / totalLoad) * 100) : 0

  return (
    <>
      {/* LEFT: Sidebar */}
      <ControlSidebar>
        <div className="space-y-4">
          <div>
            <h3 className="text-sm font-semibold mb-2">PV Energy Supply & Load Power Consumption Priority</h3>
            <p className="text-xs text-muted-foreground mb-4">Define how surplus PV energy is routed when generation exceeds load demand.</p>
          </div>

          <button
            onClick={() => setPriority("ess-first")}
            className={cn(
              "w-full text-left border rounded-lg p-4 transition-colors",
              priority === "ess-first"
                ? "border-primary bg-primary/5 ring-1 ring-primary"
                : "border-border hover:border-muted-foreground/30"
            )}
          >
            <div className="flex items-center gap-3 mb-2">
              <div className={cn(
                "h-8 w-8 rounded-lg flex items-center justify-center",
                priority === "ess-first" ? "bg-primary/10 text-primary" : "bg-muted text-muted-foreground"
              )}>
                <Battery className="h-4 w-4" />
              </div>
              <div>
                <p className="text-sm font-medium">ESS &gt; Power Grid</p>
                <p className="text-[11px] text-muted-foreground">Maximum Self Consumption</p>
              </div>
            </div>
            <p className="text-xs text-muted-foreground leading-relaxed">
              Excess PV energy charges the battery first. Minimizes grid dependency and maximizes self consumption.
            </p>
          </button>

          <button
            onClick={() => setPriority("grid-first")}
            className={cn(
              "w-full text-left border rounded-lg p-4 transition-colors",
              priority === "grid-first"
                ? "border-primary bg-primary/5 ring-1 ring-primary"
                : "border-border hover:border-muted-foreground/30"
            )}
          >
            <div className="flex items-center gap-3 mb-2">
              <div className={cn(
                "h-8 w-8 rounded-lg flex items-center justify-center",
                priority === "grid-first" ? "bg-primary/10 text-primary" : "bg-muted text-muted-foreground"
              )}>
                <Zap className="h-4 w-4" />
              </div>
              <div>
                <p className="text-sm font-medium">Power Grid &gt; ESS</p>
                <p className="text-[11px] text-muted-foreground">Self Consumption</p>
              </div>
            </div>
            <p className="text-xs text-muted-foreground leading-relaxed">
              Excess PV energy is exported to the grid first. Battery charges from the remaining energy. Maximizes grid revenue.
            </p>
          </button>
        </div>
      </ControlSidebar>

      {/* CENTER: Chart */}
      <main className="flex-1 overflow-y-auto p-6">
        <div className="mb-4">
          <h2 className="text-lg font-semibold">24-Hour Energy Flow</h2>
          <p className="text-sm text-muted-foreground">Solar production, load demand, battery and grid interaction</p>
        </div>
        <ChartContainer config={chartConfig} className="h-[360px] w-full">
          <AreaChart data={chartData} margin={{ top: 10, right: 10, left: 0, bottom: 0 }}>
            <defs>
              <linearGradient id="scSolarGrad" x1="0" y1="0" x2="0" y2="1">
                <stop offset="0%" stopColor="var(--color-energy-solar)" stopOpacity={0.3} />
                <stop offset="100%" stopColor="var(--color-energy-solar)" stopOpacity={0} />
              </linearGradient>
              <linearGradient id="scBatteryGrad" x1="0" y1="0" x2="0" y2="1">
                <stop offset="0%" stopColor="var(--color-energy-battery)" stopOpacity={0.3} />
                <stop offset="100%" stopColor="var(--color-energy-battery)" stopOpacity={0} />
              </linearGradient>
              <linearGradient id="scGridGrad" x1="0" y1="0" x2="0" y2="1">
                <stop offset="0%" stopColor="var(--color-energy-grid)" stopOpacity={0.3} />
                <stop offset="100%" stopColor="var(--color-energy-grid)" stopOpacity={0} />
              </linearGradient>
            </defs>
            <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
            <XAxis dataKey="hour" tickLine={false} axisLine={false} tickMargin={8} className="text-xs" interval={2} />
            <YAxis tickLine={false} axisLine={false} tickMargin={8} className="text-xs" unit=" kW" />
            <ChartTooltip content={<ChartTooltipContent />} />
            <Area type="monotone" dataKey="solar" stroke="var(--color-energy-solar)" strokeWidth={2} fill="url(#scSolarGrad)" />
            <Area type="monotone" dataKey="battery" stroke="var(--color-energy-battery)" strokeWidth={2} fill="url(#scBatteryGrad)" />
            <Area type="monotone" dataKey="grid" stroke="var(--color-energy-grid)" strokeWidth={2} fill="url(#scGridGrad)" />
            <Line type="monotone" dataKey="load" stroke="var(--color-energy-consumption)" strokeWidth={2} dot={false} />
          </AreaChart>
        </ChartContainer>
        <div className="flex gap-5 mt-3 justify-center">
          {Object.entries(chartConfig).map(([key, cfg]) => (
            <div key={key} className="flex items-center gap-1.5 text-xs">
              <span className="h-2.5 w-2.5 rounded-full shrink-0" style={{ backgroundColor: cfg.color }} />
              <span className="text-muted-foreground">{cfg.label}</span>
            </div>
          ))}
        </div>
      </main>

      {/* RIGHT: Details */}
      <aside className="w-[340px] shrink-0 border-l border-border bg-background overflow-y-auto p-5">
        <div className="mb-4">
          <h2 className="text-sm font-semibold">Performance Metrics</h2>
          <p className="text-xs text-muted-foreground">Current self-consumption indicators</p>
        </div>
        <div className="space-y-3">
          <MetricCard label="Self-Consumption Ratio" value={`${selfConsumptionRatio}%`} color="text-energy-solar" />
          <MetricCard label="Solar Utilization" value={`${solarUtilization}%`} color="text-energy-solar" />
          <MetricCard label="Grid Dependency" value={`${gridDependency}%`} color="text-energy-grid" />
          <MetricCard label="Battery Cycles" value="1.8" color="text-energy-battery" />
          <MetricCard label="Grid Import" value={`${fmtNum(Math.round(totalGrid / 1000))} MWh`} color="text-energy-grid" />
          <MetricCard label="Solar Generation" value={`${fmtNum(Math.round(totalSolar / 1000))} MWh`} color="text-energy-solar" />
        </div>
      </aside>
    </>
  )
}

function MetricCard({ label, value, color }: { label: string; value: string; color: string }) {
  return (
    <div className="border border-border rounded-lg p-4">
      <p className="text-xs text-muted-foreground mb-1">{label}</p>
      <p className={`text-xl font-semibold tracking-tight ${color}`}>{value}</p>
    </div>
  )
}
