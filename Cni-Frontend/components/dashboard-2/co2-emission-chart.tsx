"use client";

import { BarChart, Bar, XAxis, YAxis, CartesianGrid } from "recharts";
import {
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
  type ChartConfig,
} from "@/components/ui/chart";
import { co2EmissionData } from "@/lib/dashboard-2/mock-data";

const chartConfig = {
  value: { label: "CO2 (kg)", color: "var(--color-chart-2)" },
} satisfies ChartConfig;

export function CO2EmissionChart() {
  return (
    <div>
      <div className="mb-4">
        <h2 className="text-lg font-semibold">CO2 Emission</h2>
        <p className="text-sm text-muted-foreground">Last 7 days</p>
      </div>
      <ChartContainer config={chartConfig} className="h-[260px] w-full">
        <BarChart data={co2EmissionData} accessibilityLayer>
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
