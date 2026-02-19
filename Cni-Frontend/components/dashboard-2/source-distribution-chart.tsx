"use client";

import { useState } from "react";
import {
  AreaChart,
  Area,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
} from "recharts";
import {
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
  type ChartConfig,
} from "@/components/ui/chart";
import { Layers } from "lucide-react";
import {
  hourlySourceData,
  weeklySourceData,
  monthlySourceData,
} from "@/lib/dashboard-2/mock-data";

type Period = "today" | "weekly" | "monthly";

const tabs: { key: Period; label: string }[] = [
  { key: "today", label: "Today" },
  { key: "weekly", label: "Weekly" },
  { key: "monthly", label: "Monthly" },
];

const chartConfig = {
  bess: { label: "BESS", color: "var(--color-energy-battery)" },
  pv: { label: "Solar", color: "var(--color-energy-solar)" },
  grid: { label: "Grid", color: "var(--color-energy-grid)" },
} satisfies ChartConfig;

export function SourceDistributionChart() {
  const [period, setPeriod] = useState<Period>("today");

  const getData = () => {
    if (period === "today") return hourlySourceData;
    if (period === "weekly") return weeklySourceData;
    return monthlySourceData;
  };

  const xKey = period === "today" ? "hour" : period === "weekly" ? "day" : "month";
  const unit = period === "today" ? " kW" : " kWh";

  return (
    <div className="rounded-lg bg-muted/30 p-5">
      {/* Header */}
      <div className="flex items-center justify-between mb-5">
        <div className="flex items-center gap-3">
          <div className="flex items-center justify-center w-9 h-9 rounded-lg bg-energy-battery/10">
            <Layers className="h-[18px] w-[18px] text-energy-battery" />
          </div>
          <div>
            <h2 className="text-sm font-medium">Source Distribution</h2>
            <p className="text-xs text-muted-foreground">By energy source</p>
          </div>
        </div>
        <div className="flex rounded-lg bg-background/60 p-0.5">
          {tabs.map((tab) => (
            <button
              key={tab.key}
              onClick={() => setPeriod(tab.key)}
              className={`px-3 py-1.5 text-xs font-medium rounded-md transition-colors ${
                period === tab.key
                  ? "bg-foreground text-background shadow-sm"
                  : "text-muted-foreground hover:text-foreground"
              }`}
            >
              {tab.label}
            </button>
          ))}
        </div>
      </div>

      {/* Chart */}
      <ChartContainer config={chartConfig} className="h-[260px] w-full">
        {period === "today" ? (
          <AreaChart data={getData()} accessibilityLayer>
            <defs>
              <linearGradient id="bessGrad" x1="0" y1="0" x2="0" y2="1">
                <stop offset="0%" stopColor="var(--color-energy-battery)" stopOpacity={0.4} />
                <stop offset="100%" stopColor="var(--color-energy-battery)" stopOpacity={0} />
              </linearGradient>
              <linearGradient id="pvGrad" x1="0" y1="0" x2="0" y2="1">
                <stop offset="0%" stopColor="var(--color-energy-solar)" stopOpacity={0.4} />
                <stop offset="100%" stopColor="var(--color-energy-solar)" stopOpacity={0} />
              </linearGradient>
              <linearGradient id="gridGrad" x1="0" y1="0" x2="0" y2="1">
                <stop offset="0%" stopColor="var(--color-energy-grid)" stopOpacity={0.4} />
                <stop offset="100%" stopColor="var(--color-energy-grid)" stopOpacity={0} />
              </linearGradient>
            </defs>
            <CartesianGrid strokeDasharray="3 3" className="stroke-border/50" vertical={false} />
            <XAxis dataKey={xKey} tickLine={false} axisLine={false} tickMargin={8} tick={{ fontSize: 11 }} />
            <YAxis tickLine={false} axisLine={false} tickMargin={8} tick={{ fontSize: 11 }} unit={unit} />
            <ChartTooltip content={<ChartTooltipContent />} />
            <Area type="monotone" dataKey="grid" stroke="var(--color-grid)" strokeWidth={2} fill="url(#gridGrad)" stackId="1" />
            <Area type="monotone" dataKey="pv" stroke="var(--color-pv)" strokeWidth={2} fill="url(#pvGrad)" stackId="1" />
            <Area type="monotone" dataKey="bess" stroke="var(--color-bess)" strokeWidth={2} fill="url(#bessGrad)" stackId="1" />
          </AreaChart>
        ) : (
          <BarChart data={getData()} accessibilityLayer>
            <CartesianGrid strokeDasharray="3 3" className="stroke-border/50" vertical={false} />
            <XAxis dataKey={xKey} tickLine={false} axisLine={false} tickMargin={8} tick={{ fontSize: 11 }} />
            <YAxis tickLine={false} axisLine={false} tickMargin={8} tick={{ fontSize: 11 }} unit={unit} />
            <ChartTooltip content={<ChartTooltipContent />} />
            <Bar dataKey="grid" stackId="src" fill="var(--color-grid)" />
            <Bar dataKey="pv" stackId="src" fill="var(--color-pv)" />
            <Bar dataKey="bess" stackId="src" fill="var(--color-bess)" radius={[4, 4, 0, 0]} />
          </BarChart>
        )}
      </ChartContainer>

      {/* Legend */}
      <div className="flex items-center gap-5 mt-3 px-1">
        <div className="flex items-center gap-1.5 text-xs text-muted-foreground">
          <div className="w-2.5 h-2.5 rounded-full bg-energy-grid" />
          Grid
        </div>
        <div className="flex items-center gap-1.5 text-xs text-muted-foreground">
          <div className="w-2.5 h-2.5 rounded-full bg-energy-solar" />
          Solar
        </div>
        <div className="flex items-center gap-1.5 text-xs text-muted-foreground">
          <div className="w-2.5 h-2.5 rounded-full bg-energy-battery" />
          BESS
        </div>
      </div>
    </div>
  );
}
