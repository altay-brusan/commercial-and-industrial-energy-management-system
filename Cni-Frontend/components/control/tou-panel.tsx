"use client"

import { useState, useMemo, useEffect } from "react"
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Cell,
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
import { Button } from "@/components/ui/button"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import { Plus, Pencil, Trash2, Check, X, BatteryCharging, Zap, Minus } from "lucide-react"
import { ControlSidebar } from "@/components/control/control-sidebar"
import {
  defaultTouConfig,
  periodColors,
  periodLabels,
  getTouScheduleData,
} from "@/lib/control/mock-data"
import type { TouConfig, TouTariffRow, TariffPeriodType } from "@/lib/control/types"

const timeSlots: string[] = []
for (let h = 0; h < 24; h++) {
  for (let m = 0; m < 60; m += 30) {
    timeSlots.push(`${String(h).padStart(2, "0")}:${String(m).padStart(2, "0")}`)
  }
}

const scheduleChartConfig = {
  rate: { label: "Rate ($/kWh)", color: "var(--color-primary)" },
} satisfies ChartConfig

export function TouPanel() {
  const [config, setConfig] = useState<TouConfig>(defaultTouConfig)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [editRow, setEditRow] = useState<TouTariffRow | null>(null)

  const scheduleData = useMemo(
    () => getTouScheduleData(config.tariffSchedule),
    [config.tariffSchedule]
  )

  const updateConfig = <K extends keyof TouConfig>(key: K, value: TouConfig[K]) => {
    setConfig((prev) => ({ ...prev, [key]: value }))
  }

  const startEdit = (row: TouTariffRow) => {
    setEditingId(row.id)
    setEditRow({ ...row })
  }

  const cancelEdit = () => {
    setEditingId(null)
    setEditRow(null)
  }

  const saveEdit = () => {
    if (!editRow) return
    setConfig((prev) => ({
      ...prev,
      tariffSchedule: prev.tariffSchedule.map((r) => (r.id === editRow.id ? editRow : r)),
    }))
    cancelEdit()
  }

  const deleteRow = (id: string) => {
    setConfig((prev) => ({
      ...prev,
      tariffSchedule: prev.tariffSchedule.filter((r) => r.id !== id),
    }))
  }

  const addRow = () => {
    const newId = String(Date.now())
    const newRow: TouTariffRow = {
      id: newId,
      periodName: "New Period",
      type: "off-peak",
      startTime: "00:00",
      endTime: "00:00",
      buyPrice: 0.05,
      sellPrice: 0.04,
      targetSoC: null,
      rateKW: null,
      capacityKWh: null,
    }
    setConfig((prev) => ({
      ...prev,
      tariffSchedule: [...prev.tariffSchedule, newRow],
    }))
    startEdit(newRow)
  }

  // Status metrics
  const [currentHour, setCurrentHour] = useState(12)
  useEffect(() => { setCurrentHour(new Date().getHours()) }, [])
  const currentPoint = scheduleData.find((_, i) => i === currentHour)
  const nextChange = scheduleData.find((p, i) => i > currentHour && p.period !== currentPoint?.period)
  const avgBuyPrice = scheduleData.filter((p) => p.bessAction === "charge").reduce((s, p) => s + p.rate, 0) / Math.max(1, scheduleData.filter((p) => p.bessAction === "charge").length)
  const avgSellPrice = scheduleData.filter((p) => p.bessAction === "discharge").reduce((s, p) => s + p.rate, 0) / Math.max(1, scheduleData.filter((p) => p.bessAction === "discharge").length)

  return (
    <>
      {/* LEFT: Sidebar */}
      <ControlSidebar width="w-[620px]">
        <div className="space-y-6">
          {/* Tariff Table */}
          <div>
            <div className="flex items-center justify-between mb-3">
              <h3 className="text-sm font-semibold">Tariff Schedule</h3>
              <Button variant="outline" size="sm" className="h-7 text-xs" onClick={addRow}>
                <Plus className="h-3.5 w-3.5 mr-1" /> Add
              </Button>
            </div>
            <div className="space-y-1.5">
              {config.tariffSchedule.map((row) => {
                const isEditing = editingId === row.id
                const data = isEditing && editRow ? editRow : row
                return (
                  <div
                    key={row.id}
                    className="border border-border rounded-lg p-2.5 text-xs"
                  >
                    {isEditing ? (
                      <div className="space-y-2.5">
                        <div className="grid grid-cols-[2fr_1fr] gap-3">
                          <div>
                            <Label className="text-[11px] text-muted-foreground mb-1 block">Period Name</Label>
                            <Input
                              value={data.periodName}
                              onChange={(e) => setEditRow((prev) => prev ? { ...prev, periodName: e.target.value } : prev)}
                              className="h-8 text-xs"
                              placeholder="Period name"
                            />
                          </div>
                          <div>
                            <Label className="text-[11px] text-muted-foreground mb-1 block">Action</Label>
                            <Select
                              value={data.type}
                              onValueChange={(v) => setEditRow((prev) => prev ? { ...prev, type: v as TariffPeriodType } : prev)}
                            >
                              <SelectTrigger className="h-8 text-xs">
                                <SelectValue />
                              </SelectTrigger>
                              <SelectContent>
                                <SelectItem value="off-peak">Charge</SelectItem>
                                <SelectItem value="mid-peak">Idle</SelectItem>
                                <SelectItem value="on-peak">Discharge</SelectItem>
                              </SelectContent>
                            </Select>
                          </div>
                        </div>
                        <div className="grid grid-cols-2 gap-3">
                          <div>
                            <Label className="text-[11px] text-muted-foreground mb-1 block">Start Time</Label>
                            <Select
                              value={data.startTime}
                              onValueChange={(v) => setEditRow((prev) => prev ? { ...prev, startTime: v } : prev)}
                            >
                              <SelectTrigger className="h-8 text-xs w-full">
                                <SelectValue placeholder="Start" />
                              </SelectTrigger>
                              <SelectContent>
                                {timeSlots.map((t) => (
                                  <SelectItem key={`s-${t}`} value={t}>{t}</SelectItem>
                                ))}
                              </SelectContent>
                            </Select>
                          </div>
                          <div>
                            <Label className="text-[11px] text-muted-foreground mb-1 block">End Time</Label>
                            <Select
                              value={data.endTime}
                              onValueChange={(v) => setEditRow((prev) => prev ? { ...prev, endTime: v } : prev)}
                            >
                              <SelectTrigger className="h-8 text-xs w-full">
                                <SelectValue placeholder="End" />
                              </SelectTrigger>
                              <SelectContent>
                                {timeSlots.map((t) => (
                                  <SelectItem key={`e-${t}`} value={t}>{t}</SelectItem>
                                ))}
                              </SelectContent>
                            </Select>
                          </div>
                        </div>
                        <div className="grid grid-cols-3 gap-3">
                          <div>
                            <Label className="text-[11px] text-muted-foreground mb-1 block">Target SoC (%)</Label>
                            <Input
                              type="number"
                              value={data.targetSoC ?? ""}
                              onChange={(e) => setEditRow((prev) => prev ? { ...prev, targetSoC: e.target.value === "" ? null : Number(e.target.value) } : prev)}
                              className="h-8 text-xs"
                              placeholder="-"
                            />
                          </div>
                          <div>
                            <Label className="text-[11px] text-muted-foreground mb-1 block">Rate (kW)</Label>
                            <Input
                              type="number"
                              value={data.rateKW ?? ""}
                              onChange={(e) => setEditRow((prev) => prev ? { ...prev, rateKW: e.target.value === "" ? null : Number(e.target.value) } : prev)}
                              className="h-8 text-xs"
                              placeholder="-"
                            />
                          </div>
                          <div>
                            <Label className="text-[11px] text-muted-foreground mb-1 block">Capacity (kWh)</Label>
                            <Input
                              type="number"
                              value={data.capacityKWh ?? ""}
                              onChange={(e) => setEditRow((prev) => prev ? { ...prev, capacityKWh: e.target.value === "" ? null : Number(e.target.value) } : prev)}
                              className="h-8 text-xs"
                              placeholder="-"
                            />
                          </div>
                        </div>
                        <div className="grid grid-cols-2 gap-3">
                          <div>
                            <Label className="text-[11px] text-muted-foreground mb-1 block">Buy Price ($/kWh)</Label>
                            <Input
                              type="number"
                              step="0.001"
                              value={data.buyPrice}
                              onChange={(e) => setEditRow((prev) => prev ? { ...prev, buyPrice: Number(e.target.value) } : prev)}
                              className="h-8 text-xs"
                              placeholder="0.000"
                            />
                          </div>
                          <div>
                            <Label className="text-[11px] text-muted-foreground mb-1 block">Sell Price ($/kWh)</Label>
                            <Input
                              type="number"
                              step="0.001"
                              value={data.sellPrice}
                              onChange={(e) => setEditRow((prev) => prev ? { ...prev, sellPrice: Number(e.target.value) } : prev)}
                              className="h-8 text-xs"
                              placeholder="0.000"
                            />
                          </div>
                        </div>
                        <div className="flex justify-end gap-1 pt-1">
                          <Button variant="outline" size="sm" className="h-7 text-xs px-3" onClick={cancelEdit}>
                            <X className="h-3.5 w-3.5 mr-1" /> Cancel
                          </Button>
                          <Button size="sm" className="h-7 text-xs px-3" onClick={saveEdit}>
                            <Check className="h-3.5 w-3.5 mr-1" /> Save
                          </Button>
                        </div>
                      </div>
                    ) : (
                      <div className="flex items-center gap-2">
                        <span
                          className="h-2.5 w-2.5 rounded-full shrink-0"
                          style={{ backgroundColor: periodColors[data.type] }}
                        />
                        <div className="flex-1 min-w-0">
                          <div className="font-medium truncate">{data.periodName}</div>
                          <div className="text-muted-foreground">
                            {data.startTime} - {data.endTime} &middot; ${data.buyPrice.toFixed(3)} / ${data.sellPrice.toFixed(3)}
                          </div>
                        </div>
                        <div className="flex items-center gap-0.5 shrink-0">
                          <Button variant="ghost" size="icon" className="h-6 w-6" onClick={() => startEdit(row)}>
                            <Pencil className="h-3 w-3" />
                          </Button>
                          <Button variant="ghost" size="icon" className="h-6 w-6 text-severity-critical" onClick={() => deleteRow(row.id)}>
                            <Trash2 className="h-3 w-3" />
                          </Button>
                        </div>
                      </div>
                    )}
                  </div>
                )
              })}
            </div>
          </div>

        </div>
      </ControlSidebar>

      {/* CENTER: Schedule chart */}
      <main className="flex-1 overflow-y-auto p-6">
        <div className="mb-4">
          <h2 className="text-lg font-semibold">24-Hour Tariff Schedule</h2>
          <p className="text-sm text-muted-foreground">Hourly tariff rates with period coloring and BESS dispatch</p>
        </div>
        <ChartContainer config={scheduleChartConfig} className="h-[300px] w-full">
          <BarChart data={scheduleData} margin={{ top: 10, right: 10, left: 0, bottom: 0 }}>
            <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
            <XAxis dataKey="hour" tickLine={false} axisLine={false} tickMargin={8} className="text-xs" interval={2} />
            <YAxis tickLine={false} axisLine={false} tickMargin={8} className="text-xs" tickFormatter={(v) => `$${v.toFixed(2)}`} />
            <ChartTooltip
              content={
                <ChartTooltipContent
                  formatter={(value) => `$${Number(value).toFixed(3)}/kWh`}
                />
              }
            />
            <Bar dataKey="rate" radius={[2, 2, 0, 0]}>
              {scheduleData.map((entry) => (
                <Cell key={entry.hour} fill={periodColors[entry.period]} />
              ))}
            </Bar>
          </BarChart>
        </ChartContainer>
        <div className="flex gap-4 mt-3 justify-center">
          {(Object.entries(periodLabels) as [TariffPeriodType, string][]).map(([type, label]) => (
            <div key={type} className="flex items-center gap-1.5 text-xs">
              <span className="h-2.5 w-2.5 rounded-full shrink-0" style={{ backgroundColor: periodColors[type] }} />
              <span className="text-muted-foreground">{label}</span>
            </div>
          ))}
        </div>

        {/* BESS Action Timeline */}
        <div className="mt-6 border-t border-border pt-4">
          <h3 className="text-sm font-semibold mb-3">BESS Dispatch Schedule</h3>
          <div className="flex gap-0.5">
            {scheduleData.map((point) => {
              const bgColor = point.bessAction === "charge"
                ? "bg-energy-grid"
                : point.bessAction === "discharge"
                ? "bg-energy-solar"
                : "bg-muted"
              const Icon = point.bessAction === "charge"
                ? BatteryCharging
                : point.bessAction === "discharge"
                ? Zap
                : Minus
              return (
                <div
                  key={point.hour}
                  className={`flex-1 flex flex-col items-center gap-1 rounded py-2 ${bgColor}/10`}
                  title={`${point.hour}: ${point.bessAction} ${point.bessKW}kW`}
                >
                  <Icon className={`h-3 w-3 ${point.bessAction === "charge" ? "text-energy-grid" : point.bessAction === "discharge" ? "text-energy-solar" : "text-muted-foreground"}`} />
                  <span className="text-[9px] text-muted-foreground">{point.hour.slice(0, 2)}</span>
                </div>
              )
            })}
          </div>
          <div className="flex gap-4 mt-2 justify-center text-xs">
            <div className="flex items-center gap-1.5"><BatteryCharging className="h-3 w-3 text-energy-grid" /><span className="text-muted-foreground">Charge</span></div>
            <div className="flex items-center gap-1.5"><Zap className="h-3 w-3 text-energy-solar" /><span className="text-muted-foreground">Discharge</span></div>
            <div className="flex items-center gap-1.5"><Minus className="h-3 w-3 text-muted-foreground" /><span className="text-muted-foreground">Idle</span></div>
          </div>
        </div>
      </main>

      {/* RIGHT: Details */}
      <aside className="w-[340px] shrink-0 border-l border-border bg-background overflow-y-auto p-5">
        <div className="mb-4">
          <h2 className="text-sm font-semibold">TOU Status</h2>
          <p className="text-xs text-muted-foreground">Current tariff and arbitrage metrics</p>
        </div>
        <div className="space-y-3">
          <div className="border border-border rounded-lg p-4">
            <p className="text-xs text-muted-foreground mb-1">Current Period</p>
            {currentPoint && (
              <Badge
                className="text-xs font-medium text-white"
                style={{ backgroundColor: periodColors[currentPoint.period] }}
              >
                {periodLabels[currentPoint.period]}
              </Badge>
            )}
          </div>
          <div className="border border-border rounded-lg p-4">
            <p className="text-xs text-muted-foreground mb-1">Current Rate</p>
            <p className="text-xl font-semibold tracking-tight">
              ${currentPoint?.rate.toFixed(3) ?? "---"}<span className="text-xs text-muted-foreground">/kWh</span>
            </p>
          </div>
          <div className="border border-border rounded-lg p-4">
            <p className="text-xs text-muted-foreground mb-1">Next Period Change</p>
            <p className="text-xl font-semibold tracking-tight">
              {nextChange?.hour ?? "---"}
            </p>
          </div>
          <div className="border border-border rounded-lg p-4">
            <p className="text-xs text-muted-foreground mb-1">Today Arbitrage Revenue</p>
            <p className="text-xl font-semibold tracking-tight text-status-operational">$128</p>
          </div>
          <div className="border border-border rounded-lg p-4">
            <p className="text-xs text-muted-foreground mb-1">Avg Buy Price</p>
            <p className="text-xl font-semibold tracking-tight text-energy-grid">
              ${avgBuyPrice.toFixed(3)}<span className="text-xs text-muted-foreground">/kWh</span>
            </p>
          </div>
          <div className="border border-border rounded-lg p-4">
            <p className="text-xs text-muted-foreground mb-1">Avg Sell Price / Spread</p>
            <p className="text-xl font-semibold tracking-tight text-energy-solar">
              ${avgSellPrice.toFixed(3)}<span className="text-xs text-muted-foreground">/kWh</span>
            </p>
            <p className="text-xs text-muted-foreground mt-1">
              Spread: ${(avgSellPrice - avgBuyPrice).toFixed(3)}
            </p>
          </div>
        </div>
      </aside>
    </>
  )
}
