"use client"

import { useState, useMemo, useRef, useEffect } from "react"
import {
  ComposedChart,
  Area,
  Line,
  ReferenceLine,
  XAxis,
  YAxis,
  CartesianGrid,
} from "recharts"
import {
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
  type ChartConfig,
} from "@/components/ui/chart"
import { Slider } from "@/components/ui/slider"
import { Switch } from "@/components/ui/switch"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Badge } from "@/components/ui/badge"
import { ControlSidebar } from "@/components/control/control-sidebar"
import { fmtNum } from "@/lib/utils"
import {
  defaultPeakShavingConfig,
  generateBaseFacilityLoad,
  computePeakShavingChart,
} from "@/lib/control/mock-data"
import type { PeakShavingConfig } from "@/lib/control/types"

interface PeakShavingPanelProps {
  loadLabel?: string
}

export function PeakShavingPanel({ loadLabel = "Facility Load" }: PeakShavingPanelProps) {
  const chartConfig = {
    facilityLoad: { label: `${loadLabel} (kW)`, color: "var(--color-energy-consumption)" },
    shaved: { label: "Shaved (kW)", color: "var(--color-energy-solar)" },
    netLoad: { label: "Net Load (kW)", color: "var(--color-energy-battery)" },
  } satisfies ChartConfig
  const [config, setConfig] = useState<PeakShavingConfig>(defaultPeakShavingConfig)
  const [currentHour, setCurrentHour] = useState(12)
  const baseLoadData = useRef(generateBaseFacilityLoad())

  useEffect(() => {
    setCurrentHour(new Date().getHours())
  }, [])

  const chartData = useMemo(
    () => computePeakShavingChart(baseLoadData.current, config.thresholdKW),
    [config.thresholdKW]
  )

  const updateConfig = <K extends keyof PeakShavingConfig>(key: K, value: PeakShavingConfig[K]) => {
    setConfig((prev) => ({ ...prev, [key]: value }))
  }

  // Derived metrics
  const peakWithout = Math.max(...baseLoadData.current.map((p) => p.facilityLoad))
  const peakNet = Math.max(...chartData.map((p) => p.netLoad))
  const peakAvoided = peakWithout - peakNet
  const savingsEstimate = Math.round(peakAvoided * 0.15)
  const hourIdx = Math.min(currentHour, 23)
  const currentDemand = chartData[hourIdx]?.facilityLoad ?? 0
  const headroom = config.thresholdKW - (chartData[hourIdx]?.netLoad ?? 0)

  // Peak events
  const peakEvents = chartData
    .filter((p) => p.shaved > 0)
    .map((p) => ({
      time: p.hour,
      peak: p.facilityLoad,
      shaved: p.shaved,
      duration: "1h",
    }))

  return (
    <>
      {/* LEFT: Sidebar */}
      <ControlSidebar>
        <div className="space-y-6">
          {/* Threshold Control */}
          <div>
            <h3 className="text-sm font-semibold mb-3">Threshold Control</h3>
            <div className="space-y-3">
              <div className="flex items-center justify-between">
                <Label className="text-sm">Demand Threshold</Label>
                <div className="flex items-center gap-1">
                  <Input
                    type="number"
                    value={config.thresholdKW}
                    onChange={(e) => updateConfig("thresholdKW", Math.min(8000, Math.max(2000, Number(e.target.value))))}
                    className="w-20 h-7 text-xs text-right"
                  />
                  <span className="text-xs text-muted-foreground">kW</span>
                </div>
              </div>
              <Slider
                value={[config.thresholdKW]}
                min={2000}
                max={8000}
                step={100}
                onValueChange={([v]) => updateConfig("thresholdKW", v)}
              />
              <div className="flex justify-between text-[10px] text-muted-foreground">
                <span>2,000 kW</span>
                <span>8,000 kW</span>
              </div>
            </div>
            <div className="mt-3 space-y-2">
              <div className="flex items-center justify-between">
                <Label className="text-sm">Contracted Demand</Label>
                <div className="flex items-center gap-1">
                  <Input
                    type="number"
                    value={config.contractedDemandKW}
                    onChange={(e) => updateConfig("contractedDemandKW", Number(e.target.value))}
                    className="w-20 h-7 text-xs text-right"
                  />
                  <span className="text-xs text-muted-foreground">kW</span>
                </div>
              </div>
            </div>
          </div>

          {/* BESS Response */}
          <div>
            <h3 className="text-sm font-semibold mb-3">BESS Response</h3>
            <div className="space-y-4">
              <div className="space-y-2">
                <div className="flex items-center justify-between">
                  <Label className="text-sm">Max Response</Label>
                  <span className="text-xs text-muted-foreground">{fmtNum(config.maxBessResponseKW)} kW</span>
                </div>
                <Slider value={[config.maxBessResponseKW]} min={500} max={5000} step={100} onValueChange={([v]) => updateConfig("maxBessResponseKW", v)} />
              </div>
              <div className="space-y-2">
                <div className="flex items-center justify-between">
                  <Label className="text-sm">Min SoC Reserve</Label>
                  <span className="text-xs text-muted-foreground">{config.minSoCReserve}%</span>
                </div>
                <Slider value={[config.minSoCReserve]} min={5} max={50} step={1} onValueChange={([v]) => updateConfig("minSoCReserve", v)} />
              </div>
            </div>
          </div>

          {/* Options */}
          <div>
            <h3 className="text-sm font-semibold mb-3">Options</h3>
            <div className="flex items-center justify-between">
              <div>
                <Label className="text-sm">Predictive Pre-Charging</Label>
                <p className="text-xs text-muted-foreground">Pre-charge before predicted peaks</p>
              </div>
              <Switch checked={config.enablePredictive} onCheckedChange={(v) => updateConfig("enablePredictive", v)} />
            </div>
          </div>
        </div>
      </ControlSidebar>

      {/* CENTER: Chart */}
      <main className="flex-1 overflow-y-auto p-6">
        {/* Threshold slider in center for live interaction */}
        <div className="mb-4 pb-4 border-b border-border">
          <div className="flex items-center justify-between mb-2">
            <h2 className="text-lg font-semibold">Set Demand Threshold</h2>
            <div className="flex items-center gap-2">
              <span className="text-sm text-muted-foreground">Threshold:</span>
              <Input
                type="number"
                value={config.thresholdKW}
                onChange={(e) => updateConfig("thresholdKW", Math.min(8000, Math.max(2000, Number(e.target.value))))}
                className="w-24 h-7 text-xs text-right"
              />
              <span className="text-xs text-muted-foreground">kW</span>
            </div>
          </div>
          <Slider
            value={[config.thresholdKW]}
            min={2000}
            max={8000}
            step={100}
            onValueChange={([v]) => updateConfig("thresholdKW", v)}
          />
          <div className="flex justify-between text-[10px] text-muted-foreground mt-1">
            <span>2,000 kW</span>
            <span>8,000 kW</span>
          </div>
        </div>

        {/* Chart */}
        <ChartContainer config={chartConfig} className="h-[360px] w-full">
          <ComposedChart data={chartData} margin={{ top: 10, right: 10, left: 0, bottom: 0 }}>
            <defs>
              <linearGradient id="psLoadGrad" x1="0" y1="0" x2="0" y2="1">
                <stop offset="0%" stopColor="var(--color-energy-consumption)" stopOpacity={0.3} />
                <stop offset="100%" stopColor="var(--color-energy-consumption)" stopOpacity={0.05} />
              </linearGradient>
              <linearGradient id="psShavedGrad" x1="0" y1="0" x2="0" y2="1">
                <stop offset="0%" stopColor="var(--color-energy-solar)" stopOpacity={0.4} />
                <stop offset="100%" stopColor="var(--color-energy-solar)" stopOpacity={0.1} />
              </linearGradient>
            </defs>
            <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
            <XAxis dataKey="hour" tickLine={false} axisLine={false} tickMargin={8} className="text-xs" interval={2} />
            <YAxis tickLine={false} axisLine={false} tickMargin={8} className="text-xs" unit=" kW" />
            <ChartTooltip content={<ChartTooltipContent />} />
            <Area
              type="monotone"
              dataKey="facilityLoad"
              stroke="var(--color-energy-consumption)"
              strokeWidth={2}
              fill="url(#psLoadGrad)"
            />
            <Area
              type="monotone"
              dataKey="shaved"
              stroke="var(--color-energy-solar)"
              strokeWidth={1.5}
              fill="url(#psShavedGrad)"
            />
            <Line
              type="monotone"
              dataKey="netLoad"
              stroke="var(--color-energy-battery)"
              strokeWidth={2.5}
              dot={false}
            />
            <ReferenceLine
              y={config.thresholdKW}
              stroke="var(--color-severity-critical)"
              strokeDasharray="6 4"
              label={{
                value: `Threshold: ${fmtNum(config.thresholdKW)} kW`,
                position: "right",
                className: "text-xs fill-severity-critical",
              }}
            />
          </ComposedChart>
        </ChartContainer>
        <div className="flex gap-5 mt-3 justify-center">
          <div className="flex items-center gap-1.5 text-xs">
            <span className="h-2.5 w-2.5 rounded-full bg-energy-consumption" />
            <span className="text-muted-foreground">{loadLabel}</span>
          </div>
          <div className="flex items-center gap-1.5 text-xs">
            <span className="h-2.5 w-2.5 rounded-full bg-energy-solar" />
            <span className="text-muted-foreground">Shaved</span>
          </div>
          <div className="flex items-center gap-1.5 text-xs">
            <span className="h-2.5 w-2.5 rounded-full bg-energy-battery" />
            <span className="text-muted-foreground">Net Load</span>
          </div>
          <div className="flex items-center gap-1.5 text-xs">
            <span className="h-0.5 w-4 bg-severity-critical" style={{ borderTop: "2px dashed" }} />
            <span className="text-muted-foreground">Threshold</span>
          </div>
        </div>

        {/* Mini KPIs */}
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mt-6">
          <div className="border border-border rounded-lg p-4">
            <p className="text-xs text-muted-foreground mb-1">Peak Avoided</p>
            <p className="text-xl font-semibold tracking-tight text-energy-solar">{fmtNum(peakAvoided)} kW</p>
          </div>
          <div className="border border-border rounded-lg p-4">
            <p className="text-xs text-muted-foreground mb-1">Peak Without Shaving</p>
            <p className="text-xl font-semibold tracking-tight text-energy-consumption">{fmtNum(peakWithout)} kW</p>
          </div>
          <div className="border border-border rounded-lg p-4">
            <p className="text-xs text-muted-foreground mb-1">Net Peak</p>
            <p className="text-xl font-semibold tracking-tight text-energy-battery">{fmtNum(peakNet)} kW</p>
          </div>
          <div className="border border-border rounded-lg p-4">
            <p className="text-xs text-muted-foreground mb-1">Demand Charge Savings</p>
            <p className="text-xl font-semibold tracking-tight text-status-operational">${fmtNum(savingsEstimate)}</p>
          </div>
        </div>
      </main>

      {/* RIGHT: Details */}
      <aside className="w-[340px] shrink-0 border-l border-border bg-background overflow-y-auto p-5">
        <div className="mb-4">
          <h2 className="text-sm font-semibold">Peak Shaving Status</h2>
          <p className="text-xs text-muted-foreground">Demand management & peak events</p>
        </div>
        <div className="space-y-3">
          <div className="border border-border rounded-lg p-4">
            <p className="text-xs text-muted-foreground mb-1">Current Demand</p>
            <p className="text-xl font-semibold tracking-tight">{fmtNum(currentDemand)} kW</p>
          </div>
          <div className="border border-border rounded-lg p-4">
            <p className="text-xs text-muted-foreground mb-1">Threshold Headroom</p>
            <p className="text-xl font-semibold tracking-tight text-status-operational">
              {fmtNum(headroom)} kW
            </p>
          </div>
          <div className="border border-border rounded-lg p-4">
            <p className="text-xs text-muted-foreground mb-1">BESS Status</p>
            <Badge className="bg-status-operational text-white text-xs">Ready</Badge>
          </div>
          <div className="border border-border rounded-lg p-4">
            <p className="text-xs text-muted-foreground mb-1">SoC</p>
            <p className="text-xl font-semibold tracking-tight text-energy-battery">72%</p>
          </div>
          <div className="border border-border rounded-lg p-4">
            <p className="text-xs text-muted-foreground mb-1">Peak Events Today</p>
            <p className="text-xl font-semibold tracking-tight">{peakEvents.length}</p>
          </div>
          <div className="border border-border rounded-lg p-4">
            <p className="text-xs text-muted-foreground mb-1">Monthly Peak</p>
            <p className="text-xl font-semibold tracking-tight text-energy-consumption">{fmtNum(peakNet)} kW</p>
          </div>
        </div>

        {/* Peak Events Table */}
        {peakEvents.length > 0 && (
          <div className="mt-5">
            <h3 className="text-sm font-semibold mb-3">Peak Events</h3>
            <div className="border border-border rounded-lg overflow-hidden">
              <table className="w-full text-xs">
                <thead>
                  <tr className="border-b border-border bg-muted/50">
                    <th className="text-left font-medium px-3 py-2">Time</th>
                    <th className="text-right font-medium px-3 py-2">Peak</th>
                    <th className="text-right font-medium px-3 py-2">Shaved</th>
                  </tr>
                </thead>
                <tbody>
                  {peakEvents.map((evt) => (
                    <tr key={evt.time} className="border-b border-border last:border-0">
                      <td className="px-3 py-2 font-medium">{evt.time}</td>
                      <td className="px-3 py-2 text-right font-mono text-energy-consumption">{fmtNum(evt.peak)}</td>
                      <td className="px-3 py-2 text-right font-mono text-energy-solar">{fmtNum(evt.shaved)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}
      </aside>
    </>
  )
}
