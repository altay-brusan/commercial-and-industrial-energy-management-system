"use client";

import { useMemo } from "react";
import {
  Bar,
  BarChart,
  CartesianGrid,
  Line,
  LineChart,
  ReferenceLine,
  Scatter,
  ScatterChart,
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
  getCapacityFactor,
  getAvailability,
  getMeanWindSpeed,
  getSpecificYield,
  getCapacityFactorTrend,
  getSpecificYieldPerTurbine,
  getPowerCurveScatter,
  getAvailabilityHeatmap,
} from "@/lib/wind-turbines/analytics-mock-data";
import { windTurbines } from "@/lib/wind-turbines/mock-data";
import { cn } from "@/lib/utils";

const cfTrendConfig = {
  capacityFactor: {
    label: "CF (%)",
    color: "var(--color-energy-solar)",
  },
} satisfies ChartConfig;

const specificYieldConfig = {
  specificYield: {
    label: "kWh/kW",
    color: "var(--color-energy-solar)",
  },
} satisfies ChartConfig;

const powerCurveScatterConfig = {
  power: {
    label: "Power (kW)",
    color: "var(--color-energy-solar)",
  },
  windSpeed: {
    label: "Wind Speed (m/s)",
    color: "var(--color-energy-solar)",
  },
} satisfies ChartConfig;

export function PerformanceView() {
  const capacityFactor = useMemo(() => getCapacityFactor(), []);
  const availability = useMemo(() => getAvailability(), []);
  const meanWindSpeed = useMemo(() => getMeanWindSpeed(), []);
  const specificYield = useMemo(() => getSpecificYield(), []);
  const cfTrend = useMemo(() => getCapacityFactorTrend(), []);
  const specificYieldData = useMemo(() => getSpecificYieldPerTurbine(), []);
  const scatterData = useMemo(() => getPowerCurveScatter(), []);
  const heatmapData = useMemo(() => getAvailabilityHeatmap(), []);

  const totalTurbines = windTurbines.length;
  const onlineTurbines = windTurbines.filter((t) => t.status !== "offline").length;

  const stats = [
    { label: "Capacity Factor", value: `${capacityFactor}`, unit: "%" },
    { label: "Availability", value: `${availability}`, unit: `% (${onlineTurbines}/${totalTurbines})` },
    { label: "Mean Wind Speed", value: `${meanWindSpeed}`, unit: "m/s" },
    { label: "Specific Yield", value: `${specificYield}`, unit: "kWh/kW" },
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
        {/* CF Trend + Specific Yield */}
        <div className="px-6 py-6 border-b border-border">
          <div className="grid gap-8 lg:grid-cols-2">
            {/* Capacity Factor Trend */}
            <div>
              <div className="mb-4">
                <h2 className="text-lg font-semibold">Capacity Factor Trend</h2>
                <p className="text-sm text-muted-foreground">Last 30 days daily Capacity Factor (%)</p>
              </div>
              <ChartContainer config={cfTrendConfig} className="h-[280px] w-full">
                <LineChart data={cfTrend} margin={{ top: 10, right: 10, left: -15, bottom: 0 }}>
                  <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
                  <XAxis dataKey="date" tickLine={false} axisLine={false} tickMargin={8} className="text-xs" interval={4} />
                  <YAxis domain={[0, 100]} tickLine={false} axisLine={false} tickMargin={8} className="text-xs" />
                  <ReferenceLine y={30} stroke="var(--color-energy-grid)" strokeDasharray="4 4" label={{ value: "30%", position: "right", className: "text-xs fill-energy-grid" }} />
                  <ChartTooltip content={<ChartTooltipContent indicator="line" />} />
                  <Line type="monotone" dataKey="capacityFactor" stroke="var(--color-energy-solar)" strokeWidth={2} dot={false} />
                </LineChart>
              </ChartContainer>
            </div>

            {/* Specific Yield per Turbine */}
            <div>
              <div className="mb-4">
                <h2 className="text-lg font-semibold">Specific Yield per Turbine</h2>
                <p className="text-sm text-muted-foreground">kWh/kW per turbine today</p>
              </div>
              <ChartContainer config={specificYieldConfig} className="h-[280px] w-full">
                <BarChart data={specificYieldData} layout="vertical" margin={{ top: 10, right: 10, left: -15, bottom: 0 }}>
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

        {/* Power Curve Scatter */}
        <div className="px-6 py-6 border-b border-border">
          <div className="mb-4">
            <h2 className="text-lg font-semibold">Power Curve Scatter</h2>
            <p className="text-sm text-muted-foreground">Wind speed vs power output across all turbines</p>
          </div>
          <ChartContainer config={powerCurveScatterConfig} className="h-[300px] w-full">
            <ScatterChart margin={{ top: 10, right: 10, left: 0, bottom: 0 }}>
              <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
              <XAxis type="number" dataKey="windSpeed" tickLine={false} axisLine={false} tickMargin={8} className="text-xs" />
              <YAxis type="number" dataKey="power" tickLine={false} axisLine={false} tickMargin={8} className="text-xs" />
              <ChartTooltip content={<ChartTooltipContent />} />
              <Scatter data={scatterData} fill="var(--color-energy-solar)" fillOpacity={0.5} />
            </ScatterChart>
          </ChartContainer>
        </div>

        {/* Availability Heatmap */}
        <div className="px-6 py-6">
          <div className="mb-4">
            <h2 className="text-lg font-semibold">Availability Heatmap</h2>
            <p className="text-sm text-muted-foreground">Hourly availability per turbine (%)</p>
          </div>
          <div className="overflow-x-auto">
            <table className="w-full text-xs">
              <thead>
                <tr className="border-b border-border">
                  <th className="text-left py-2 px-2 font-medium text-muted-foreground sticky left-0 bg-background">Turbine</th>
                  {Array.from({ length: 24 }, (_, i) => (
                    <th key={i} className="text-center py-2 px-1 font-medium text-muted-foreground min-w-[36px]">
                      {String(i).padStart(2, "0")}
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {heatmapData.map((row) => (
                  <tr key={row.turbine} className="border-b border-border/50">
                    <td className="py-2 px-2 font-medium sticky left-0 bg-background">{row.turbine}</td>
                    {row.hours.map((val, i) => {
                      const isLow = val < 50;
                      const isMedium = val >= 50 && val < 90;
                      return (
                        <td key={i} className="text-center py-1.5 px-0.5">
                          <div
                            className={cn(
                              "inline-flex items-center justify-center w-8 h-6 rounded text-[9px] font-mono",
                              isLow && "bg-energy-grid/20 text-energy-grid",
                              isMedium && "bg-energy-consumption/20 text-energy-consumption",
                              !isLow && !isMedium && "bg-energy-solar/20 text-energy-solar"
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
