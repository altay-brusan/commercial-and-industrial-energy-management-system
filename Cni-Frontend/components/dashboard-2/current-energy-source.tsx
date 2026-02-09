"use client";

import { PieChart, Pie, Cell } from "recharts";
import {
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
  type ChartConfig,
} from "@/components/ui/chart";
import { energySourceData } from "@/lib/dashboard-2/mock-data";

const chartConfig = {
  Grid: { label: "Grid", color: "var(--color-energy-grid)" },
  BESS: { label: "BESS", color: "var(--color-energy-battery)" },
  Solar: { label: "Solar", color: "var(--color-energy-solar)" },
  Generator: { label: "Generator", color: "var(--color-muted-foreground)" },
} satisfies ChartConfig;

export function CurrentEnergySource() {
  const data = energySourceData;

  const pieData = data.sources.map((s) => ({
    name: s.label,
    value: s.value,
    fill: s.color,
  }));

  return (
    <div className="rounded-lg bg-muted/30 p-6">
      <div className="mb-4">
        <h2 className="text-lg font-semibold">Energy Sources</h2>
        <p className="text-sm text-muted-foreground">
          Current distribution breakdown
        </p>
      </div>

      <div className="flex items-center gap-8">
        <div className="relative shrink-0">
          <ChartContainer config={chartConfig} className="aspect-square h-[200px]">
            <PieChart>
              <ChartTooltip content={<ChartTooltipContent hideLabel />} />
              <Pie
                data={pieData}
                dataKey="value"
                nameKey="name"
                innerRadius={60}
                outerRadius={85}
                strokeWidth={2}
                stroke="var(--color-background)"
              >
                {pieData.map((entry) => (
                  <Cell key={entry.name} fill={entry.fill} />
                ))}
              </Pie>
            </PieChart>
          </ChartContainer>
          <div className="absolute inset-0 flex flex-col items-center justify-center pointer-events-none">
            <span className="text-3xl font-semibold tracking-tight">{data.totalKW}</span>
            <span className="text-sm text-muted-foreground">kW</span>
          </div>
        </div>

        <div className="flex-1 space-y-3">
          {data.sources.map((source) => (
            <div key={source.label} className="flex items-center justify-between">
              <div className="flex items-center gap-2">
                <div
                  className="w-3 h-3 rounded-full"
                  style={{ backgroundColor: source.color }}
                />
                <span className="text-sm">{source.label}</span>
              </div>
              <div className="text-right">
                <span className="text-sm font-semibold">{source.value} kW</span>
                <span className="text-xs text-muted-foreground ml-2">
                  {source.percentage}%
                </span>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
