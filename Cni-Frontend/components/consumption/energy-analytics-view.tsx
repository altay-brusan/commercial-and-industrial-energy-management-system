"use client";

import { useMemo } from "react";
import {
  Area,
  AreaChart,
  Bar,
  BarChart,
  CartesianGrid,
  Line,
  LineChart,
  ReferenceLine,
  XAxis,
  YAxis,
} from "recharts";
import {
  type ChartConfig,
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
} from "@/components/ui/chart";
import {
  getTotalDemand,
  getTotalConsumptionToday,
  getPeakDemandToday,
  getAvgLoadFactor,
  getEstimatedCostToday,
  getDemandProfile,
  getConsumptionByType,
  getMonthlyConsumption,
  getDailyPeakDemand,
} from "@/lib/consumption/analytics-mock-data";
import { consumers } from "@/lib/consumption/mock-data";

const demandProfileConfig = {
  demand: {
    label: "Demand (kW)",
    color: "var(--color-energy-consumption)",
  },
} satisfies ChartConfig;

const consumptionByTypeConfig = {
  consumption: {
    label: "Consumption (kWh)",
    color: "var(--color-energy-consumption)",
  },
} satisfies ChartConfig;

const monthlyConsumptionConfig = {
  consumption: {
    label: "Consumption (MWh)",
    color: "var(--color-energy-consumption)",
  },
} satisfies ChartConfig;

const peakDemandConfig = {
  peakDemand: {
    label: "Peak Demand (kW)",
    color: "var(--color-energy-consumption)",
  },
} satisfies ChartConfig;

export function EnergyAnalyticsView() {
  const totalDemand = useMemo(() => getTotalDemand(), []);
  const consumptionToday = useMemo(() => getTotalConsumptionToday(), []);
  const peakDemand = useMemo(() => getPeakDemandToday(), []);
  const avgLF = useMemo(() => getAvgLoadFactor(), []);
  const estimatedCost = useMemo(() => getEstimatedCostToday(), []);
  const demandProfile = useMemo(() => getDemandProfile(), []);
  const consumptionByType = useMemo(() => getConsumptionByType(), []);
  const monthlyConsumption = useMemo(() => getMonthlyConsumption(), []);
  const dailyPeak = useMemo(() => getDailyPeakDemand(), []);

  const totalContracted = consumers.reduce((sum, c) => sum + c.contractedCapacity, 0);

  const stats = [
    { label: "Total Demand", value: (totalDemand / 1000).toFixed(1), unit: "MW" },
    { label: "Consumption Today", value: (consumptionToday / 1000).toFixed(1), unit: "MWh" },
    { label: "Peak Demand Today", value: peakDemand.toLocaleString("en-US"), unit: "kW" },
    { label: "Avg Load Factor", value: `${avgLF}`, unit: "%" },
    { label: "Estimated Cost", value: `$${estimatedCost.toLocaleString("en-US")}`, unit: "" },
  ];

  return (
    <div className="flex flex-col h-full overflow-hidden">
      {/* Stats Bar */}
      <div className="shrink-0 grid grid-cols-5 border-b border-border">
        {stats.map((stat, i) => (
          <div
            key={stat.label}
            className={`px-6 py-5 ${i < stats.length - 1 ? "border-r border-border" : ""}`}
          >
            <p className="text-sm text-muted-foreground mb-1">{stat.label}</p>
            <div className="flex items-baseline gap-2">
              <span className="text-3xl font-semibold tracking-tight">{stat.value}</span>
              <span className="text-sm text-muted-foreground">{stat.unit}</span>
            </div>
          </div>
        ))}
      </div>

      {/* Content */}
      <main className="flex-1 overflow-y-auto">
        {/* Demand Profile (full width) */}
        <div className="px-6 py-6 border-b border-border">
          <div className="mb-4">
            <h2 className="text-lg font-semibold">Demand Profile</h2>
            <p className="text-sm text-muted-foreground">24-hour aggregate demand with contracted limit</p>
          </div>
          <ChartContainer config={demandProfileConfig} className="h-[280px] w-full">
            <AreaChart data={demandProfile} margin={{ top: 10, right: 10, left: 0, bottom: 0 }}>
              <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
              <XAxis dataKey="time" tickLine={false} axisLine={false} tickMargin={8} className="text-xs" />
              <YAxis tickLine={false} axisLine={false} tickMargin={8} className="text-xs" />
              <ReferenceLine y={totalContracted} stroke="var(--color-energy-grid)" strokeDasharray="4 4" label={{ value: "Contracted Limit", position: "right", className: "text-xs fill-energy-grid" }} />
              <ChartTooltip content={<ChartTooltipContent indicator="line" />} />
              <Area
                type="monotone"
                dataKey="demand"
                stroke="var(--color-energy-consumption)"
                fill="var(--color-energy-consumption)"
                fillOpacity={0.3}
              />
            </AreaChart>
          </ChartContainer>
        </div>

        {/* Consumption by Type + Monthly Consumption */}
        <div className="px-6 py-6 border-b border-border">
          <div className="grid gap-8 lg:grid-cols-2">
            {/* Consumption by Type */}
            <div>
              <div className="mb-4">
                <h2 className="text-lg font-semibold">Consumption by Type</h2>
                <p className="text-sm text-muted-foreground">Today&apos;s consumption per consumer type (kWh)</p>
              </div>
              <ChartContainer config={consumptionByTypeConfig} className="h-[280px] w-full">
                <BarChart data={consumptionByType} layout="vertical" margin={{ top: 10, right: 10, left: 0, bottom: 0 }}>
                  <CartesianGrid strokeDasharray="3 3" className="stroke-border" horizontal={false} />
                  <XAxis type="number" tickLine={false} axisLine={false} className="text-xs" />
                  <YAxis dataKey="type" type="category" tickLine={false} axisLine={false} width={80} className="text-xs" />
                  <ChartTooltip content={<ChartTooltipContent />} />
                  <Bar dataKey="consumption" fill="var(--color-energy-consumption)" radius={[0, 4, 4, 0]} />
                </BarChart>
              </ChartContainer>
            </div>

            {/* Monthly Consumption */}
            <div>
              <div className="mb-4">
                <h2 className="text-lg font-semibold">Monthly Consumption</h2>
                <p className="text-sm text-muted-foreground">Last 12 months (MWh)</p>
              </div>
              <ChartContainer config={monthlyConsumptionConfig} className="h-[280px] w-full">
                <BarChart data={monthlyConsumption} margin={{ top: 10, right: 10, left: 0, bottom: 0 }}>
                  <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
                  <XAxis dataKey="month" tickLine={false} axisLine={false} tickMargin={8} className="text-xs" />
                  <YAxis tickLine={false} axisLine={false} tickMargin={8} className="text-xs" />
                  <ChartTooltip content={<ChartTooltipContent />} />
                  <Bar dataKey="consumption" fill="var(--color-energy-consumption)" radius={[4, 4, 0, 0]} />
                </BarChart>
              </ChartContainer>
            </div>
          </div>
        </div>

        {/* Daily Peak Demand (full width) */}
        <div className="px-6 py-6">
          <div className="mb-4">
            <h2 className="text-lg font-semibold">Daily Peak Demand Trend</h2>
            <p className="text-sm text-muted-foreground">Last 30 days peak demand (kW)</p>
          </div>
          <ChartContainer config={peakDemandConfig} className="h-[260px] w-full">
            <LineChart data={dailyPeak} margin={{ top: 10, right: 10, left: 0, bottom: 0 }}>
              <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
              <XAxis dataKey="date" tickLine={false} axisLine={false} tickMargin={8} className="text-xs" interval={4} />
              <YAxis tickLine={false} axisLine={false} tickMargin={8} className="text-xs" />
              <ReferenceLine y={totalContracted} stroke="var(--color-energy-grid)" strokeDasharray="4 4" label={{ value: "Contracted Limit", position: "right", className: "text-xs fill-energy-grid" }} />
              <ChartTooltip content={<ChartTooltipContent indicator="line" />} />
              <Line type="monotone" dataKey="peakDemand" stroke="var(--color-energy-consumption)" strokeWidth={2} dot={false} />
            </LineChart>
          </ChartContainer>
        </div>
      </main>
    </div>
  );
}
