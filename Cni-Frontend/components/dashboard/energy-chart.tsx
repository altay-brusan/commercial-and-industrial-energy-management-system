"use client";

import {
  Area,
  AreaChart,
  CartesianGrid,
  XAxis,
  YAxis,
  ResponsiveContainer,
} from "recharts";
import {
  ChartConfig,
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
} from "@/components/ui/chart";

const chartData = [
  { time: "00:00", consumption: 2.1, solar: 0, grid: 2.1 },
  { time: "02:00", consumption: 1.8, solar: 0, grid: 1.8 },
  { time: "04:00", consumption: 1.5, solar: 0, grid: 1.5 },
  { time: "06:00", consumption: 2.4, solar: 0.3, grid: 2.1 },
  { time: "08:00", consumption: 4.2, solar: 1.8, grid: 2.4 },
  { time: "10:00", consumption: 5.1, solar: 4.2, grid: 0.9 },
  { time: "12:00", consumption: 4.8, solar: 5.5, grid: 0 },
  { time: "14:00", consumption: 5.5, solar: 5.8, grid: 0 },
  { time: "16:00", consumption: 6.2, solar: 4.1, grid: 2.1 },
  { time: "18:00", consumption: 7.8, solar: 1.2, grid: 6.6 },
  { time: "20:00", consumption: 6.5, solar: 0, grid: 6.5 },
  { time: "22:00", consumption: 4.2, solar: 0, grid: 4.2 },
];

const chartConfig = {
  consumption: {
    label: "Consumption",
    color: "var(--color-energy-consumption)",
  },
  solar: {
    label: "Solar",
    color: "var(--color-energy-solar)",
  },
  grid: {
    label: "Grid",
    color: "var(--color-energy-grid)",
  },
} satisfies ChartConfig;

export function EnergyChart() {
  return (
    <div className="px-6 py-6">
      <div className="mb-4">
        <h2 className="text-lg font-semibold">Energy Flow</h2>
        <p className="text-sm text-muted-foreground">
          Today&apos;s consumption breakdown
        </p>
      </div>

      <ChartContainer config={chartConfig} className="h-[300px] w-full">
        <ResponsiveContainer width="100%" height="100%">
          <AreaChart
            data={chartData}
            margin={{ top: 10, right: 10, left: 0, bottom: 0 }}
          >
            <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
            <XAxis
              dataKey="time"
              tickLine={false}
              axisLine={false}
              tickMargin={8}
              className="text-xs"
            />
            <YAxis
              tickLine={false}
              axisLine={false}
              tickMargin={8}
              tickFormatter={(value) => `${value}kW`}
              className="text-xs"
            />
            <ChartTooltip
              content={<ChartTooltipContent indicator="line" />}
            />
            <Area
              type="monotone"
              dataKey="solar"
              stackId="1"
              stroke="var(--color-energy-solar)"
              fill="var(--color-energy-solar)"
              fillOpacity={0.4}
            />
            <Area
              type="monotone"
              dataKey="grid"
              stackId="1"
              stroke="var(--color-energy-grid)"
              fill="var(--color-energy-grid)"
              fillOpacity={0.4}
            />
          </AreaChart>
        </ResponsiveContainer>
      </ChartContainer>

      <div className="flex items-center justify-center gap-6 mt-4 text-sm">
        <div className="flex items-center gap-2">
          <div className="w-3 h-3 rounded-full bg-energy-solar" />
          <span className="text-muted-foreground">Solar</span>
        </div>
        <div className="flex items-center gap-2">
          <div className="w-3 h-3 rounded-full bg-energy-grid" />
          <span className="text-muted-foreground">Grid</span>
        </div>
      </div>
    </div>
  );
}
