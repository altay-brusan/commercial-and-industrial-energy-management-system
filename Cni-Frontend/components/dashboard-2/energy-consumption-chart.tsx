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
import { Activity } from "lucide-react";
import {
  hourlyConsumptionData,
  weeklyConsumptionData,
  monthlyConsumptionData,
} from "@/lib/dashboard-2/mock-data";

type Period = "today" | "weekly" | "monthly";

const tabs: { key: Period; label: string }[] = [
  { key: "today", label: "Today" },
  { key: "weekly", label: "Weekly" },
  { key: "monthly", label: "Monthly" },
];

const todayConfig = {
  consumption: { label: "Consumption (kW)", color: "var(--color-energy-consumption)" },
  production: { label: "Production (kW)", color: "var(--color-energy-solar)" },
} satisfies ChartConfig;

const periodConfig = {
  value: { label: "Usage (kWh)", color: "var(--color-energy-consumption)" },
} satisfies ChartConfig;

export function EnergyConsumptionChart() {
  const [period, setPeriod] = useState<Period>("today");

  return (
    <div className="rounded-lg bg-muted/30 p-5">
      {/* Header */}
      <div className="flex items-center justify-between mb-5">
        <div className="flex items-center gap-3">
          <div className="flex items-center justify-center w-9 h-9 rounded-lg bg-energy-consumption/10">
            <Activity className="h-[18px] w-[18px] text-energy-consumption" />
          </div>
          <div>
            <h2 className="text-sm font-medium">Energy Consumption</h2>
            <p className="text-xs text-muted-foreground">
              {period === "today" ? "Hourly consumption & production" : period === "weekly" ? "Last 7 days" : "Year overview"}
            </p>
          </div>
        </div>

        {/* Period tabs */}
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
      {period === "today" ? (
        <ChartContainer config={todayConfig} className="h-[280px] w-full">
          <AreaChart data={hourlyConsumptionData} accessibilityLayer>
            <defs>
              <linearGradient id="consumptionGrad" x1="0" y1="0" x2="0" y2="1">
                <stop offset="0%" stopColor="var(--color-energy-consumption)" stopOpacity={0.3} />
                <stop offset="100%" stopColor="var(--color-energy-consumption)" stopOpacity={0} />
              </linearGradient>
              <linearGradient id="productionGrad" x1="0" y1="0" x2="0" y2="1">
                <stop offset="0%" stopColor="var(--color-energy-solar)" stopOpacity={0.3} />
                <stop offset="100%" stopColor="var(--color-energy-solar)" stopOpacity={0} />
              </linearGradient>
            </defs>
            <CartesianGrid strokeDasharray="3 3" className="stroke-border/50" vertical={false} />
            <XAxis
              dataKey="hour"
              tickLine={false}
              axisLine={false}
              tickMargin={8}
              tick={{ fontSize: 11 }}
              interval={2}
            />
            <YAxis
              tickLine={false}
              axisLine={false}
              tickMargin={8}
              tick={{ fontSize: 11 }}
              unit=" kW"
            />
            <ChartTooltip content={<ChartTooltipContent />} />
            <Area
              type="monotone"
              dataKey="consumption"
              stroke="var(--color-consumption)"
              strokeWidth={2}
              fill="url(#consumptionGrad)"
            />
            <Area
              type="monotone"
              dataKey="production"
              stroke="var(--color-production)"
              strokeWidth={2}
              fill="url(#productionGrad)"
            />
          </AreaChart>
        </ChartContainer>
      ) : period === "weekly" ? (
        <ChartContainer config={periodConfig} className="h-[280px] w-full">
          <BarChart data={weeklyConsumptionData} accessibilityLayer>
            <CartesianGrid strokeDasharray="3 3" className="stroke-border/50" vertical={false} />
            <XAxis dataKey="day" tickLine={false} axisLine={false} tickMargin={8} tick={{ fontSize: 11 }} />
            <YAxis tickLine={false} axisLine={false} tickMargin={8} tick={{ fontSize: 11 }} unit=" kWh" />
            <ChartTooltip content={<ChartTooltipContent />} />
            <Bar dataKey="value" fill="var(--color-value)" radius={[4, 4, 0, 0]} />
          </BarChart>
        </ChartContainer>
      ) : (
        <ChartContainer config={periodConfig} className="h-[280px] w-full">
          <BarChart data={monthlyConsumptionData} accessibilityLayer>
            <CartesianGrid strokeDasharray="3 3" className="stroke-border/50" vertical={false} />
            <XAxis dataKey="month" tickLine={false} axisLine={false} tickMargin={8} tick={{ fontSize: 11 }} />
            <YAxis tickLine={false} axisLine={false} tickMargin={8} tick={{ fontSize: 11 }} unit=" kWh" />
            <ChartTooltip content={<ChartTooltipContent />} />
            <Bar dataKey="value" fill="var(--color-value)" radius={[4, 4, 0, 0]} />
          </BarChart>
        </ChartContainer>
      )}

      {/* Legend for today */}
      {period === "today" && (
        <div className="flex items-center gap-5 mt-3 px-1">
          <div className="flex items-center gap-1.5 text-xs text-muted-foreground">
            <div className="w-2.5 h-2.5 rounded-full bg-energy-consumption" />
            Consumption
          </div>
          <div className="flex items-center gap-1.5 text-xs text-muted-foreground">
            <div className="w-2.5 h-2.5 rounded-full bg-energy-solar" />
            Production
          </div>
        </div>
      )}
    </div>
  );
}
