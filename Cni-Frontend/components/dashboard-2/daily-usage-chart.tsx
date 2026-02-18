"use client";

import { BarChart, Bar, XAxis, YAxis, CartesianGrid } from "recharts";
import {
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
  type ChartConfig,
} from "@/components/ui/chart";
import { dailyUsageData } from "@/lib/dashboard-2/mock-data";

const chartConfig = {
  value: { label: "Usage (kWh)", color: "var(--color-chart-2)" },
} satisfies ChartConfig;

export function DailyUsageChart() {
  return (
    <div>
      <div className="mb-4">
        <h2 className="text-lg font-semibold">Daily Usage</h2>
        <p className="text-sm text-muted-foreground">Last 7 days</p>
      </div>
      <ChartContainer config={chartConfig} className="h-[260px] w-full">
        <BarChart data={dailyUsageData} accessibilityLayer>
          <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
          <XAxis dataKey="day" tickLine={false} axisLine={false} tickMargin={8} />
          <YAxis tickLine={false} axisLine={false} tickMargin={8} />
          <ChartTooltip content={<ChartTooltipContent />} />
          <Bar
            dataKey="value"
            fill="var(--color-value)"
            radius={[4, 4, 0, 0]}
          />
        </BarChart>
      </ChartContainer>
    </div>
  );
}
