"use client";

import { useMemo } from "react";
import {
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
  getPlantYieldToday,
  getInstalledCapacity,
  getDeviceStatusSummary,
  getPrTrend,
  getSpecificYieldPerInverter,
  getMpptComparison,
  getStringCurrentHeatmap,
} from "@/lib/solar-inverters/analytics-mock-data";
import { solarInverters } from "@/lib/solar-inverters/mock-data";
import { cn } from "@/lib/utils";
import { AbbrTooltip } from "@/components/ui/abbr-tooltip";

const prTrendConfig = {
  pr: {
    label: "PR (%)",
    color: "var(--color-energy-solar)",
  },
} satisfies ChartConfig;

const specificYieldConfig = {
  specificYield: {
    label: "kWh/kWp",
    color: "var(--color-energy-solar)",
  },
} satisfies ChartConfig;

const mpptConfig = {
  avgPower: {
    label: "Avg Power (kW)",
    color: "var(--color-energy-battery)",
  },
} satisfies ChartConfig;

export function PerformanceView() {
  const yieldToday = useMemo(() => getPlantYieldToday(), []);
  const capacity = useMemo(() => getInstalledCapacity(), []);
  const statusSummary = useMemo(() => getDeviceStatusSummary(), []);
  const prTrend = useMemo(() => getPrTrend(), []);
  const specificYield = useMemo(() => getSpecificYieldPerInverter(), []);
  const mpptData = useMemo(() => getMpptComparison(), []);
  const heatmapData = useMemo(() => getStringCurrentHeatmap(), []);

  const totalInverters = solarInverters.length;
  const onlineInverters = statusSummary.online + statusSummary.warning;

  const plantSpecificYield = +(yieldToday / capacity).toFixed(2);
  const plantPr = 85.2;
  const cuf = +((yieldToday / (capacity * 24)) * 100).toFixed(1);
  const availability = +((onlineInverters / totalInverters) * 100).toFixed(1);

  const stats = [
    { label: "Performance Ratio", value: `${plantPr}`, unit: "%" },
    { label: "Capacity Utilization", value: `${cuf}`, unit: "%" },
    { label: "Specific Yield", value: `${plantSpecificYield}`, unit: "kWh/kWp" },
    { label: "Availability", value: `${availability}`, unit: `% (${onlineInverters}/${totalInverters})` },
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
        {/* PR Trend + Specific Yield */}
        <div className="px-6 py-6 border-b border-border">
          <div className="grid gap-8 lg:grid-cols-2">
            {/* PR Trend */}
            <div>
              <div className="mb-4">
                <h2 className="text-lg font-semibold"><AbbrTooltip label="PR Trend" title="Performance Ratio" description="Actual energy output versus theoretical maximum output" /></h2>
                <p className="text-sm text-muted-foreground">Last 30 days daily Performance Ratio (%)</p>
              </div>
              <ChartContainer config={prTrendConfig} className="h-[280px] w-full">
                <LineChart data={prTrend} margin={{ top: 10, right: 10, left: 0, bottom: 0 }}>
                  <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
                  <XAxis dataKey="date" tickLine={false} axisLine={false} tickMargin={8} className="text-xs" interval={4} />
                  <YAxis domain={[70, 100]} tickLine={false} axisLine={false} tickMargin={8} className="text-xs" />
                  <ReferenceLine y={80} stroke="var(--color-energy-grid)" strokeDasharray="4 4" label={{ value: "80%", position: "right", className: "text-xs fill-energy-grid" }} />
                  <ChartTooltip content={<ChartTooltipContent indicator="line" />} />
                  <Line type="monotone" dataKey="pr" stroke="var(--color-energy-solar)" strokeWidth={2} dot={false} />
                </LineChart>
              </ChartContainer>
            </div>

            {/* Specific Yield per Inverter */}
            <div>
              <div className="mb-4">
                <h2 className="text-lg font-semibold">Specific Yield per Inverter</h2>
                <p className="text-sm text-muted-foreground">kWh/kWp per inverter today</p>
              </div>
              <ChartContainer config={specificYieldConfig} className="h-[280px] w-full">
                <BarChart data={specificYield} layout="vertical" margin={{ top: 10, right: 10, left: 0, bottom: 0 }}>
                  <CartesianGrid strokeDasharray="3 3" className="stroke-border" horizontal={false} />
                  <XAxis type="number" tickLine={false} axisLine={false} className="text-xs" />
                  <YAxis dataKey="name" type="category" tickLine={false} axisLine={false} width={80} className="text-xs" />
                  <ChartTooltip content={<ChartTooltipContent />} />
                  <Bar dataKey="specificYield" fill="var(--color-energy-solar)" radius={[0, 4, 4, 0]} />
                </BarChart>
              </ChartContainer>
            </div>
          </div>
        </div>

        {/* MPPT Efficiency */}
        <div className="px-6 py-6 border-b border-border">
          <div className="mb-4">
            <h2 className="text-lg font-semibold"><AbbrTooltip label="MPPT Efficiency Comparison" title="Maximum Power Point Tracking" description="Algorithm that optimizes power extraction from solar panels" /></h2>
            <p className="text-sm text-muted-foreground">Average power per tracker across online inverters (kW)</p>
          </div>
          <ChartContainer config={mpptConfig} className="h-[260px] w-full">
            <BarChart data={mpptData} margin={{ top: 10, right: 10, left: 0, bottom: 0 }}>
              <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
              <XAxis dataKey="tracker" tickLine={false} axisLine={false} tickMargin={8} className="text-xs" />
              <YAxis tickLine={false} axisLine={false} tickMargin={8} className="text-xs" />
              <ChartTooltip content={<ChartTooltipContent />} />
              <Bar dataKey="avgPower" fill="var(--color-energy-battery)" radius={[4, 4, 0, 0]} />
            </BarChart>
          </ChartContainer>
        </div>

        {/* String Current Heatmap */}
        <div className="px-6 py-6">
          <div className="mb-4">
            <h2 className="text-lg font-semibold">String Current Heatmap</h2>
            <p className="text-sm text-muted-foreground">Current values by inverter &times; string (A)</p>
          </div>
          <div className="overflow-x-auto">
            <table className="w-full text-xs">
              <thead>
                <tr className="border-b border-border">
                  <th className="text-left py-2 px-2 font-medium text-muted-foreground sticky left-0 bg-background">Inverter</th>
                  {Array.from({ length: 12 }, (_, i) => (
                    <th key={i} className="text-center py-2 px-1 font-medium text-muted-foreground min-w-[40px]">
                      S{i + 1}
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {heatmapData.map((row) => (
                  <tr key={row.inverter} className="border-b border-border/50">
                    <td className="py-2 px-2 font-medium sticky left-0 bg-background">{row.inverter}</td>
                    {row.currents.map((val, i) => {
                      const isLow = val === null || val < 0.5;
                      const isNormal = val !== null && val >= 2.5;
                      return (
                        <td key={i} className="text-center py-1.5 px-1">
                          <div
                            className={cn(
                              "inline-flex items-center justify-center w-9 h-7 rounded text-[10px] font-mono",
                              isLow && "bg-severity-critical/20 text-severity-critical",
                              !isLow && isNormal && "bg-status-operational/20 text-status-operational",
                              !isLow && !isNormal && "bg-severity-high/20 text-severity-high"
                            )}
                          >
                            {val !== null ? val.toFixed(1) : "—"}
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
