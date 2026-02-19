"use client";

import { useMemo } from "react";
import {
  Area,
  AreaChart,
  Bar,
  BarChart,
  CartesianGrid,
  Cell,
  Pie,
  PieChart,
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
  monthlyPnL,
  getRevenueBreakdown,
  costItems,
  formatCurrency,
} from "@/lib/financials/mock-data";
import type { Currency } from "@/lib/financials/types";

interface OverviewViewProps {
  currency: Currency;
}

const revenueCostConfig = {
  revenue: {
    label: "Revenue",
    color: "var(--color-energy-solar)",
  },
  cost: {
    label: "Cost",
    color: "var(--color-energy-grid)",
  },
} satisfies ChartConfig;

const pnlConfig = {
  netIncome: {
    label: "Net Income",
    color: "var(--color-energy-solar)",
  },
} satisfies ChartConfig;

const pieConfig = {
  value: {
    label: "Revenue",
  },
} satisfies ChartConfig;

const costBreakdownConfig = {
  amount: {
    label: "Amount",
    color: "var(--color-energy-grid)",
  },
} satisfies ChartConfig;

export function OverviewView({ currency }: OverviewViewProps) {
  const revenueBreakdown = useMemo(() => getRevenueBreakdown(), []);

  const costBreakdown = useMemo(() => {
    return costItems.map((c) => ({
      name: c.label,
      amount: c.monthlyAmount,
    }));
  }, []);

  return (
    <div className="flex flex-col">
      {/* Revenue vs Cost Trend */}
      <div className="px-6 py-6 border-b border-border">
        <div className="mb-4">
          <h2 className="text-lg font-semibold">Revenue vs Cost Trend</h2>
          <p className="text-sm text-muted-foreground">12-month revenue and cost comparison</p>
        </div>
        <ChartContainer config={revenueCostConfig} className="h-[280px] w-full">
          <AreaChart data={monthlyPnL} margin={{ top: 10, right: 10, left: 0, bottom: 0 }}>
            <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
            <XAxis dataKey="month" tickLine={false} axisLine={false} tickMargin={8} className="text-xs" />
            <YAxis
              tickLine={false}
              axisLine={false}
              tickMargin={8}
              className="text-xs"
              tickFormatter={(v) => `$${(v / 1000).toFixed(0)}K`}
            />
            <ChartTooltip
              content={
                <ChartTooltipContent
                  formatter={(value) => formatCurrency(Number(value), currency)}
                />
              }
            />
            <Area
              type="monotone"
              dataKey="revenue"
              stroke="var(--color-energy-solar)"
              fill="var(--color-energy-solar)"
              fillOpacity={0.2}
              strokeWidth={2}
            />
            <Area
              type="monotone"
              dataKey="cost"
              stroke="var(--color-energy-grid)"
              fill="var(--color-energy-grid)"
              fillOpacity={0.15}
              strokeWidth={2}
            />
          </AreaChart>
        </ChartContainer>
      </div>

      {/* Revenue Breakdown + Monthly P&L */}
      <div className="px-6 py-6 border-b border-border">
        <div className="grid gap-8 lg:grid-cols-2">
          {/* Revenue Breakdown Pie */}
          <div>
            <div className="mb-4">
              <h2 className="text-lg font-semibold">Revenue Breakdown</h2>
              <p className="text-sm text-muted-foreground">Current month revenue by stream</p>
            </div>
            <ChartContainer config={pieConfig} className="h-[280px] w-full">
              <PieChart>
                <ChartTooltip
                  content={
                    <ChartTooltipContent
                      hideLabel
                      nameKey="name"
                      formatter={(value, name) => (
                        <div className="flex w-full items-center justify-between gap-4">
                          <span className="text-muted-foreground">{name}</span>
                          <span className="font-mono font-medium tabular-nums">{formatCurrency(Number(value), currency)}</span>
                        </div>
                      )}
                    />
                  }
                />
                <Pie
                  data={revenueBreakdown}
                  dataKey="value"
                  nameKey="name"
                  cx="50%"
                  cy="50%"
                  innerRadius={60}
                  outerRadius={110}
                  strokeWidth={2}
                  stroke="var(--color-background)"
                >
                  {revenueBreakdown.map((entry) => (
                    <Cell key={entry.name} fill={entry.fill} />
                  ))}
                </Pie>
              </PieChart>
            </ChartContainer>
            <div className="flex flex-wrap gap-3 mt-3 justify-center">
              {revenueBreakdown.map((entry) => (
                <div key={entry.name} className="flex items-center gap-1.5 text-xs">
                  <span
                    className="h-2.5 w-2.5 rounded-full shrink-0"
                    style={{ backgroundColor: entry.fill }}
                  />
                  <span className="text-muted-foreground">{entry.name}</span>
                </div>
              ))}
            </div>
          </div>

          {/* Monthly P&L */}
          <div>
            <div className="mb-4">
              <h2 className="text-lg font-semibold">Monthly P&L</h2>
              <p className="text-sm text-muted-foreground">Net income per month</p>
            </div>
            <ChartContainer config={pnlConfig} className="h-[280px] w-full">
              <BarChart data={monthlyPnL} margin={{ top: 10, right: 10, left: 0, bottom: 0 }}>
                <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
                <XAxis dataKey="month" tickLine={false} axisLine={false} tickMargin={8} className="text-xs" />
                <YAxis
                  tickLine={false}
                  axisLine={false}
                  tickMargin={8}
                  className="text-xs"
                  tickFormatter={(v) => `$${(v / 1000).toFixed(0)}K`}
                />
                <ChartTooltip
                  content={
                    <ChartTooltipContent
                      formatter={(value) => formatCurrency(Number(value), currency)}
                    />
                  }
                />
                <Bar dataKey="netIncome" radius={[4, 4, 0, 0]}>
                  {monthlyPnL.map((entry) => (
                    <Cell
                      key={entry.month}
                      fill={entry.netIncome >= 0 ? "var(--color-energy-solar)" : "var(--color-energy-grid)"}
                    />
                  ))}
                </Bar>
              </BarChart>
            </ChartContainer>
          </div>
        </div>
      </div>

      {/* Cost Breakdown */}
      <div className="px-6 py-6">
        <div className="mb-4">
          <h2 className="text-lg font-semibold">Cost Breakdown</h2>
          <p className="text-sm text-muted-foreground">Current month cost by category</p>
        </div>
        <ChartContainer config={costBreakdownConfig} className="h-[240px] w-full">
          <BarChart data={costBreakdown} layout="vertical" margin={{ top: 10, right: 10, left: 0, bottom: 0 }}>
            <CartesianGrid strokeDasharray="3 3" className="stroke-border" horizontal={false} />
            <XAxis
              type="number"
              tickLine={false}
              axisLine={false}
              className="text-xs"
              tickFormatter={(v) => `$${(v / 1000).toFixed(0)}K`}
            />
            <YAxis dataKey="name" type="category" tickLine={false} axisLine={false} width={100} className="text-xs" />
            <ChartTooltip
              content={
                <ChartTooltipContent
                  formatter={(value) => formatCurrency(Number(value), currency)}
                />
              }
            />
            <Bar dataKey="amount" fill="var(--color-energy-grid)" radius={[0, 4, 4, 0]} />
          </BarChart>
        </ChartContainer>
      </div>
    </div>
  );
}
