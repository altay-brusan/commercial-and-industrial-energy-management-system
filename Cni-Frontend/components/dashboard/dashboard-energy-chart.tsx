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
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { energyChartData } from "@/lib/dashboard/mock-data";

const chartConfig = {
  solar: {
    label: "Solar",
    color: "var(--color-energy-solar)",
  },
  consumption: {
    label: "Consumption",
    color: "var(--color-energy-consumption)",
  },
  grid: {
    label: "Grid",
    color: "var(--color-energy-grid)",
  },
} satisfies ChartConfig;

export function DashboardEnergyChart() {
  return (
    <Card>
      <CardHeader className="pb-2">
        <CardTitle className="text-base">Energy Overview</CardTitle>
        <p className="text-sm text-muted-foreground">24-hour power history (kW)</p>
      </CardHeader>
      <CardContent className="p-4 pt-0">
        <ChartContainer config={chartConfig} className="h-[280px] w-full">
          <ResponsiveContainer width="100%" height="100%">
            <AreaChart
              data={energyChartData}
              margin={{ top: 10, right: 10, left: 0, bottom: 0 }}
            >
              <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
              <XAxis
                dataKey="time"
                tickLine={false}
                axisLine={false}
                tickMargin={8}
                className="text-xs"
                interval={2}
              />
              <YAxis
                tickLine={false}
                axisLine={false}
                tickMargin={8}
                tickFormatter={(value) => `${value}`}
                className="text-xs"
              />
              <ChartTooltip
                content={<ChartTooltipContent indicator="line" />}
              />
              <Area
                type="monotone"
                dataKey="solar"
                stroke="var(--color-energy-solar)"
                fill="var(--color-energy-solar)"
                fillOpacity={0.3}
              />
              <Area
                type="monotone"
                dataKey="consumption"
                stroke="var(--color-energy-consumption)"
                fill="var(--color-energy-consumption)"
                fillOpacity={0.15}
              />
              <Area
                type="monotone"
                dataKey="grid"
                stroke="var(--color-energy-grid)"
                fill="var(--color-energy-grid)"
                fillOpacity={0.15}
              />
            </AreaChart>
          </ResponsiveContainer>
        </ChartContainer>

        <div className="flex items-center justify-center gap-6 mt-3 text-sm">
          <div className="flex items-center gap-2">
            <div className="w-3 h-3 rounded-full bg-energy-solar" />
            <span className="text-muted-foreground">Solar</span>
          </div>
          <div className="flex items-center gap-2">
            <div className="w-3 h-3 rounded-full bg-energy-consumption" />
            <span className="text-muted-foreground">Consumption</span>
          </div>
          <div className="flex items-center gap-2">
            <div className="w-3 h-3 rounded-full bg-energy-grid" />
            <span className="text-muted-foreground">Grid</span>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
