"use client";

import { useMemo } from "react";
import {
  Area,
  AreaChart,
  Bar,
  BarChart,
  CartesianGrid,
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
  getPlantActivePower,
  getPlantYieldToday,
  getPlantTotalYield,
  getInstalledCapacity,
  getYieldThisMonth,
  getDeviceStatusSummary,
  getPlantPowerCurve,
  getInverterYieldComparison,
  getMonthlyProduction,
  getDailyEnergyTrend,
} from "@/lib/solar-inverters/analytics-mock-data";
import { solarInverters } from "@/lib/solar-inverters/mock-data";

const powerCurveConfig = {
  activePower: {
    label: "Active Power",
    color: "var(--color-energy-solar)",
  },
} satisfies ChartConfig;

const inverterYieldConfig = {
  yield: {
    label: "Yield (kWh)",
    color: "var(--color-energy-solar)",
  },
} satisfies ChartConfig;

const monthlyConfig = {
  yield: {
    label: "Yield (MWh)",
    color: "var(--color-energy-solar)",
  },
} satisfies ChartConfig;

const dailyTrendConfig = {
  yield: {
    label: "Yield (kWh)",
    color: "var(--color-energy-solar)",
  },
} satisfies ChartConfig;

export function EnergyAnalyticsView() {
  const activePower = useMemo(() => getPlantActivePower(), []);
  const yieldToday = useMemo(() => getPlantYieldToday(), []);
  const totalYield = useMemo(() => getPlantTotalYield(), []);
  const installedCapacity = useMemo(() => getInstalledCapacity(), []);
  const yieldThisMonth = useMemo(() => getYieldThisMonth(), []);
  const statusSummary = useMemo(() => getDeviceStatusSummary(), []);
  const powerCurve = useMemo(() => getPlantPowerCurve(), []);
  const inverterYield = useMemo(() => getInverterYieldComparison(), []);
  const monthlyProd = useMemo(() => getMonthlyProduction(), []);
  const dailyTrend = useMemo(() => getDailyEnergyTrend(), []);

  const stats = [
    { label: "Installed Capacity", value: installedCapacity.toLocaleString("en-US"), unit: "kWp" },
    { label: "Active Power", value: activePower.toFixed(1), unit: "kW" },
    { label: "Yield Today", value: yieldToday.toFixed(1), unit: "kWh" },
    { label: "Yield This Month", value: yieldThisMonth.toFixed(1), unit: "MWh" },
    { label: "Total Yield", value: totalYield.toFixed(1), unit: "MWh" },
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
        {/* Device Status + Daily Power Curve */}
        <div className="px-6 py-6 border-b border-border">
          <div className="flex items-center justify-between mb-4">
            <div>
              <h2 className="text-lg font-semibold">Daily Power Curve</h2>
              <p className="text-sm text-muted-foreground">Aggregate plant active power (kW)</p>
            </div>
            <div className="flex items-center gap-5 text-sm">
              <div className="flex items-center gap-1.5">
                <span className="h-2 w-2 rounded-full bg-energy-solar" />
                <span className="text-muted-foreground">{statusSummary.normal} Normal</span>
              </div>
              <div className="flex items-center gap-1.5">
                <span className="h-2 w-2 rounded-full bg-muted-foreground" />
                <span className="text-muted-foreground">{statusSummary.offline} Offline</span>
              </div>
              <div className="flex items-center gap-1.5">
                <span className="h-2 w-2 rounded-full bg-energy-consumption" />
                <span className="text-muted-foreground">{statusSummary.warning} Warning</span>
              </div>
              <div className="flex items-center gap-1.5">
                <span className="h-2 w-2 rounded-full bg-energy-grid" />
                <span className="text-muted-foreground">{statusSummary.fault} Fault</span>
              </div>
              <span className="text-xs text-muted-foreground">{solarInverters.length} inverters</span>
            </div>
          </div>
          <ChartContainer config={powerCurveConfig} className="h-[280px] w-full">
            <AreaChart data={powerCurve} margin={{ top: 10, right: 10, left: 0, bottom: 0 }}>
              <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
              <XAxis
                dataKey="time"
                tickLine={false}
                axisLine={false}
                tickMargin={8}
                className="text-xs"
                interval={Math.max(1, Math.floor(powerCurve.length / 12))}
              />
              <YAxis tickLine={false} axisLine={false} tickMargin={8} className="text-xs" />
              <ChartTooltip content={<ChartTooltipContent indicator="line" />} />
              <Area
                type="monotone"
                dataKey="activePower"
                stroke="var(--color-energy-solar)"
                fill="var(--color-energy-solar)"
                fillOpacity={0.3}
              />
            </AreaChart>
          </ChartContainer>
        </div>

        {/* Inverter Yield + Monthly Production */}
        <div className="px-6 py-6 border-b border-border">
          <div className="grid gap-8 lg:grid-cols-2">
            {/* Inverter Yield Comparison */}
            <div>
              <div className="mb-4">
                <h2 className="text-lg font-semibold">Inverter Yield Comparison</h2>
                <p className="text-sm text-muted-foreground">Today&apos;s yield per inverter (kWh)</p>
              </div>
              <ChartContainer config={inverterYieldConfig} className="h-[280px] w-full">
                <BarChart data={inverterYield} layout="vertical" margin={{ top: 10, right: 10, left: 0, bottom: 0 }}>
                  <CartesianGrid strokeDasharray="3 3" className="stroke-border" horizontal={false} />
                  <XAxis type="number" tickLine={false} axisLine={false} className="text-xs" />
                  <YAxis dataKey="name" type="category" tickLine={false} axisLine={false} width={80} className="text-xs" />
                  <ChartTooltip content={<ChartTooltipContent />} />
                  <Bar dataKey="yield" fill="var(--color-energy-solar)" radius={[0, 4, 4, 0]} />
                </BarChart>
              </ChartContainer>
            </div>

            {/* Monthly Energy Production */}
            <div>
              <div className="mb-4">
                <h2 className="text-lg font-semibold">Monthly Energy Production</h2>
                <p className="text-sm text-muted-foreground">Last 12 months (MWh)</p>
              </div>
              <ChartContainer config={monthlyConfig} className="h-[280px] w-full">
                <BarChart data={monthlyProd} margin={{ top: 10, right: 10, left: 0, bottom: 0 }}>
                  <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
                  <XAxis dataKey="month" tickLine={false} axisLine={false} tickMargin={8} className="text-xs" />
                  <YAxis tickLine={false} axisLine={false} tickMargin={8} className="text-xs" />
                  <ChartTooltip content={<ChartTooltipContent />} />
                  <Bar dataKey="yield" fill="var(--color-energy-solar)" radius={[4, 4, 0, 0]} />
                </BarChart>
              </ChartContainer>
            </div>
          </div>
        </div>

        {/* Daily Energy Trend */}
        <div className="px-6 py-6">
          <div className="mb-4">
            <h2 className="text-lg font-semibold">Daily Energy Trend</h2>
            <p className="text-sm text-muted-foreground">Last 30 days yield (kWh)</p>
          </div>
          <ChartContainer config={dailyTrendConfig} className="h-[260px] w-full">
            <BarChart data={dailyTrend} margin={{ top: 10, right: 10, left: 0, bottom: 0 }}>
              <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
              <XAxis dataKey="date" tickLine={false} axisLine={false} tickMargin={8} className="text-xs" interval={4} />
              <YAxis tickLine={false} axisLine={false} tickMargin={8} className="text-xs" />
              <ChartTooltip content={<ChartTooltipContent />} />
              <Bar dataKey="yield" fill="var(--color-energy-solar)" radius={[4, 4, 0, 0]} />
            </BarChart>
          </ChartContainer>
        </div>
      </main>
    </div>
  );
}
