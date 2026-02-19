"use client";

import { useState, useMemo } from "react";
import {
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
import { TrendingDown } from "lucide-react";
import {
  hourlyCostData,
  weeklyCostData,
  monthlyCostData,
} from "@/lib/dashboard-2/mock-data";

type Period = "today" | "weekly" | "monthly";

const tabs: { key: Period; label: string }[] = [
  { key: "today", label: "Today" },
  { key: "weekly", label: "Weekly" },
  { key: "monthly", label: "Monthly" },
];

const chartConfig = {
  cost: { label: "Cost (TL)", color: "var(--color-energy-grid)" },
  savings: { label: "Savings (TL)", color: "var(--color-energy-solar)" },
} satisfies ChartConfig;

export function CostSavingsChart() {
  const [period, setPeriod] = useState<Period>("today");

  const getData = () => {
    if (period === "today") return hourlyCostData;
    if (period === "weekly") return weeklyCostData;
    return monthlyCostData;
  };

  const xKey = period === "today" ? "hour" : period === "weekly" ? "day" : "month";

  const totals = useMemo(() => {
    const data = getData();
    const totalCost = data.reduce((sum, d) => sum + d.cost, 0);
    const totalSavings = data.reduce((sum, d) => sum + d.savings, 0);
    return { totalCost, totalSavings };
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [period]);

  const formatTL = (v: number) =>
    v.toLocaleString("tr-TR", { minimumFractionDigits: 0, maximumFractionDigits: 0 });

  return (
    <div className="rounded-lg bg-muted/30 p-5">
      {/* Header */}
      <div className="flex items-center justify-between mb-5">
        <div className="flex items-center gap-3">
          <div className="flex items-center justify-center w-9 h-9 rounded-lg bg-energy-solar/10">
            <TrendingDown className="h-[18px] w-[18px] text-energy-solar" />
          </div>
          <div>
            <h2 className="text-sm font-medium">Cost & Savings</h2>
            <p className="text-xs text-muted-foreground">Financial overview</p>
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

      {/* Summary stats */}
      <div className="grid grid-cols-2 gap-3 mb-4">
        <div className="rounded-md bg-background/60 p-3">
          <p className="text-[10px] text-muted-foreground uppercase tracking-wider mb-1">Total Cost</p>
          <p className="text-lg font-semibold">
            {formatTL(totals.totalCost)}
            <span className="text-xs font-normal text-muted-foreground ml-1">TL</span>
          </p>
        </div>
        <div className="rounded-md bg-background/60 p-3">
          <p className="text-[10px] text-muted-foreground uppercase tracking-wider mb-1">Total Savings</p>
          <p className="text-lg font-semibold text-energy-solar">
            {formatTL(totals.totalSavings)}
            <span className="text-xs font-normal text-muted-foreground ml-1">TL</span>
          </p>
        </div>
      </div>

      {/* Chart */}
      <ChartContainer config={chartConfig} className="h-[200px] w-full">
        <BarChart data={getData()} accessibilityLayer>
          <CartesianGrid strokeDasharray="3 3" className="stroke-border/50" vertical={false} />
          <XAxis dataKey={xKey} tickLine={false} axisLine={false} tickMargin={8} tick={{ fontSize: 11 }} />
          <YAxis tickLine={false} axisLine={false} tickMargin={8} tick={{ fontSize: 11 }} />
          <ChartTooltip content={<ChartTooltipContent />} />
          <Bar dataKey="cost" fill="var(--color-cost)" radius={[4, 4, 0, 0]} />
          <Bar dataKey="savings" fill="var(--color-savings)" radius={[4, 4, 0, 0]} />
        </BarChart>
      </ChartContainer>

      {/* Legend */}
      <div className="flex items-center gap-5 mt-3 px-1">
        <div className="flex items-center gap-1.5 text-xs text-muted-foreground">
          <div className="w-2.5 h-2.5 rounded-full bg-energy-grid" />
          Cost
        </div>
        <div className="flex items-center gap-1.5 text-xs text-muted-foreground">
          <div className="w-2.5 h-2.5 rounded-full bg-energy-solar" />
          Savings
        </div>
      </div>
    </div>
  );
}
