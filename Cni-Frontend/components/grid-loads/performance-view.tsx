"use client";

import { useMemo } from "react";
import {
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
  getGridAvailability,
  getVoltageCompliance,
  getPowerQualityIndex,
  getAvgPowerFactor,
  getVoltageProfile,
  getFrequencyTrend,
  getPowerFactorTrend,
  getTransformerLoadingHeatmap,
} from "@/lib/grid-loads/analytics-mock-data";
import { gridDevices } from "@/lib/grid-loads/mock-data";
import { cn } from "@/lib/utils";

const voltageProfileConfig = {
  phaseA: {
    label: "Phase A (V)",
    color: "var(--color-energy-solar)",
  },
  phaseB: {
    label: "Phase B (V)",
    color: "var(--color-energy-consumption)",
  },
  phaseC: {
    label: "Phase C (V)",
    color: "var(--color-energy-battery)",
  },
} satisfies ChartConfig;

const frequencyConfig = {
  frequency: {
    label: "Frequency (Hz)",
    color: "var(--color-energy-solar)",
  },
} satisfies ChartConfig;

const pfTrendConfig = {
  powerFactor: {
    label: "Power Factor",
    color: "var(--color-energy-solar)",
  },
} satisfies ChartConfig;

export function PerformanceView() {
  const availability = useMemo(() => getGridAvailability(), []);
  const voltageCompliance = useMemo(() => getVoltageCompliance(), []);
  const pqi = useMemo(() => getPowerQualityIndex(), []);
  const avgPF = useMemo(() => getAvgPowerFactor(), []);
  const voltageProfile = useMemo(() => getVoltageProfile(), []);
  const frequencyTrend = useMemo(() => getFrequencyTrend(), []);
  const pfTrend = useMemo(() => getPowerFactorTrend(), []);
  const heatmapData = useMemo(() => getTransformerLoadingHeatmap(), []);

  const totalDevices = gridDevices.length;
  const onlineDevices = gridDevices.filter((d) => d.status !== "offline").length;

  const stats = [
    { label: "Grid Availability", value: `${availability}`, unit: `% (${onlineDevices}/${totalDevices})` },
    { label: "Voltage Compliance", value: `${voltageCompliance}`, unit: "%" },
    { label: "Power Quality Index", value: `${pqi}`, unit: "/100" },
    { label: "Avg Power Factor", value: `${avgPF}`, unit: "" },
  ];

  return (
    <div className="flex flex-col h-full overflow-hidden">
      {/* Stats Bar */}
      <div className="shrink-0 grid grid-cols-4 border-b border-border">
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
        {/* Voltage Profile + Frequency Trend */}
        <div className="px-6 py-6 border-b border-border">
          <div className="grid gap-8 lg:grid-cols-2">
            {/* Voltage Profile */}
            <div>
              <div className="mb-4">
                <h2 className="text-lg font-semibold">Voltage Profile</h2>
                <p className="text-sm text-muted-foreground">3-phase voltage over 24h (V)</p>
              </div>
              <ChartContainer config={voltageProfileConfig} className="h-[280px] w-full">
                <LineChart data={voltageProfile} margin={{ top: 10, right: 10, left: -15, bottom: 0 }}>
                  <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
                  <XAxis dataKey="time" tickLine={false} axisLine={false} tickMargin={8} className="text-xs" />
                  <YAxis domain={[5500, 6050]} tickLine={false} axisLine={false} tickMargin={8} className="text-xs" />
                  <ReferenceLine y={5175} stroke="var(--color-energy-grid)" strokeDasharray="4 4" strokeOpacity={0.5} />
                  <ReferenceLine y={6325} stroke="var(--color-energy-grid)" strokeDasharray="4 4" strokeOpacity={0.5} />
                  <ChartTooltip content={<ChartTooltipContent indicator="line" />} />
                  <Line type="monotone" dataKey="phaseA" stroke="var(--color-energy-solar)" strokeWidth={1.5} dot={false} />
                  <Line type="monotone" dataKey="phaseB" stroke="var(--color-energy-consumption)" strokeWidth={1.5} dot={false} />
                  <Line type="monotone" dataKey="phaseC" stroke="var(--color-energy-battery)" strokeWidth={1.5} dot={false} />
                </LineChart>
              </ChartContainer>
            </div>

            {/* Frequency Trend */}
            <div>
              <div className="mb-4">
                <h2 className="text-lg font-semibold">Frequency Trend</h2>
                <p className="text-sm text-muted-foreground">Grid frequency over 24h (Hz)</p>
              </div>
              <ChartContainer config={frequencyConfig} className="h-[280px] w-full">
                <LineChart data={frequencyTrend} margin={{ top: 10, right: 10, left: -15, bottom: 0 }}>
                  <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
                  <XAxis dataKey="time" tickLine={false} axisLine={false} tickMargin={8} className="text-xs" />
                  <YAxis domain={[49.8, 50.2]} tickLine={false} axisLine={false} tickMargin={8} className="text-xs" />
                  <ReferenceLine y={49.95} stroke="var(--color-energy-grid)" strokeDasharray="4 4" label={{ value: "49.95", position: "left", className: "text-[10px] fill-energy-grid" }} />
                  <ReferenceLine y={50.05} stroke="var(--color-energy-grid)" strokeDasharray="4 4" label={{ value: "50.05", position: "left", className: "text-[10px] fill-energy-grid" }} />
                  <ChartTooltip content={<ChartTooltipContent indicator="line" />} />
                  <Line type="monotone" dataKey="frequency" stroke="var(--color-energy-solar)" strokeWidth={2} dot={false} />
                </LineChart>
              </ChartContainer>
            </div>
          </div>
        </div>

        {/* Power Factor Trend (full width) */}
        <div className="px-6 py-6 border-b border-border">
          <div className="mb-4">
            <h2 className="text-lg font-semibold">Power Factor Trend</h2>
            <p className="text-sm text-muted-foreground">Daily average power factor (last 30 days)</p>
          </div>
          <ChartContainer config={pfTrendConfig} className="h-[260px] w-full">
            <LineChart data={pfTrend} margin={{ top: 10, right: 10, left: -15, bottom: 0 }}>
              <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
              <XAxis dataKey="date" tickLine={false} axisLine={false} tickMargin={8} className="text-xs" interval={4} />
              <YAxis domain={[0.9, 1.01]} tickLine={false} axisLine={false} tickMargin={8} className="text-xs" />
              <ReferenceLine y={0.95} stroke="var(--color-energy-grid)" strokeDasharray="4 4" label={{ value: "0.95", position: "right", className: "text-xs fill-energy-grid" }} />
              <ChartTooltip content={<ChartTooltipContent indicator="line" />} />
              <Line type="monotone" dataKey="powerFactor" stroke="var(--color-energy-solar)" strokeWidth={2} dot={false} />
            </LineChart>
          </ChartContainer>
        </div>

        {/* Transformer Loading Heatmap */}
        <div className="px-6 py-6">
          <div className="mb-4">
            <h2 className="text-lg font-semibold">Transformer Loading Heatmap</h2>
            <p className="text-sm text-muted-foreground">Hourly loading per transformer (%)</p>
          </div>
          <div className="overflow-x-auto">
            <table className="w-full text-xs">
              <thead>
                <tr className="border-b border-border">
                  <th className="text-left py-2 px-2 font-medium text-muted-foreground sticky left-0 bg-background">Device</th>
                  {Array.from({ length: 24 }, (_, i) => (
                    <th key={i} className="text-center py-2 px-1 font-medium text-muted-foreground min-w-[36px]">
                      {String(i).padStart(2, "0")}
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {heatmapData.map((row) => (
                  <tr key={row.device} className="border-b border-border/50">
                    <td className="py-2 px-2 font-medium sticky left-0 bg-background">{row.device}</td>
                    {row.hours.map((val, i) => {
                      const isHigh = val > 80;
                      const isMedium = val >= 50 && val <= 80;
                      return (
                        <td key={i} className="text-center py-1.5 px-0.5">
                          <div
                            className={cn(
                              "inline-flex items-center justify-center w-8 h-6 rounded text-[9px] font-mono",
                              isHigh && "bg-severity-critical/20 text-severity-critical",
                              isMedium && "bg-severity-high/20 text-severity-high",
                              !isHigh && !isMedium && "bg-status-operational/20 text-status-operational"
                            )}
                          >
                            {val}
                          </div>
                        </td>
                      );
                    })}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </main>
    </div>
  );
}
