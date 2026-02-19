"use client";

import { BarChart, Bar, XAxis, YAxis, CartesianGrid } from "recharts";
import {
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
  ChartLegend,
  ChartLegendContent,
  type ChartConfig,
} from "@/components/ui/chart";
import { dailyUsagePerSourceData } from "@/lib/dashboard-2/mock-data";

const chartConfig = {
  bess: { label: "BESS", color: "var(--color-energy-battery)" },
  pv: { label: "PV", color: "var(--color-energy-solar)" },
  grid: { label: "Grid", color: "var(--color-energy-grid)" },
} satisfies ChartConfig;

export function DailyUsagePerSourceChart() {
  return (
    <div>
      <div className="mb-4">
        <h2 className="text-lg font-semibold">Usage per Source</h2>
        <p className="text-sm text-muted-foreground">Last 7 days breakdown</p>
      </div>
      <ChartContainer config={chartConfig} className="h-[260px] w-full">
        <BarChart data={dailyUsagePerSourceData} accessibilityLayer>
          <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
          <XAxis dataKey="day" tickLine={false} axisLine={false} tickMargin={8} />
          <YAxis tickLine={false} axisLine={false} tickMargin={8} />
          <ChartTooltip content={<ChartTooltipContent />} />
          <ChartLegend content={<ChartLegendContent />} />
          <Bar
            dataKey="bess"
            stackId="usage"
            fill="var(--color-bess)"
            radius={[0, 0, 0, 0]}
          />
          <Bar
            dataKey="pv"
            stackId="usage"
            fill="var(--color-pv)"
            radius={[0, 0, 0, 0]}
          />
          <Bar
            dataKey="grid"
            stackId="usage"
            fill="var(--color-grid)"
            radius={[4, 4, 0, 0]}
          />
        </BarChart>
      </ChartContainer>
    </div>
  );
}
