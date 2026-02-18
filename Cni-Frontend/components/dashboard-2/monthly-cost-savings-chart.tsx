"use client";

import { BarChart, Bar, XAxis, YAxis, CartesianGrid } from "recharts";
import {
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
  type ChartConfig,
} from "@/components/ui/chart";
import { monthlyCostSavingsData } from "@/lib/dashboard-2/mock-data";

const chartConfig = {
  value: { label: "Savings (TL)", color: "var(--color-chart-2)" },
} satisfies ChartConfig;

export function MonthlyCostSavingsChart() {
  return (
    <div>
      <div className="mb-4">
        <h2 className="text-lg font-semibold">Monthly Cost Savings</h2>
        <p className="text-sm text-muted-foreground">Year overview</p>
      </div>
      <ChartContainer config={chartConfig} className="h-[260px] w-full">
        <BarChart data={monthlyCostSavingsData} accessibilityLayer>
          <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
          <XAxis dataKey="month" tickLine={false} axisLine={false} tickMargin={8} />
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
