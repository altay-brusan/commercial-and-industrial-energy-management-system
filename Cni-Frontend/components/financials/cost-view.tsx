"use client";

import { useMemo } from "react";
import { TrendingUp, TrendingDown } from "lucide-react";
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
import { Badge } from "@/components/ui/badge";
import {
  costItems,
  monthlyCostTrend,
  budgetVsActual,
  formatCurrency,
} from "@/lib/financials/mock-data";
import type { Currency } from "@/lib/financials/types";

interface CostViewProps {
  currency: Currency;
}

const costTrendConfig = {
  operations: { label: "Operations", color: "var(--color-energy-grid)" },
  maintenance: { label: "Maintenance", color: "var(--color-energy-consumption)" },
  grid_charges: { label: "Grid Charges", color: "var(--color-energy-battery)" },
  depreciation: { label: "Depreciation", color: "var(--color-primary)" },
  insurance: { label: "Insurance", color: "var(--color-chart-5)" },
  admin: { label: "Admin", color: "var(--color-muted-foreground)" },
} satisfies ChartConfig;

const budgetConfig = {
  budgeted: { label: "Budgeted", color: "var(--color-muted-foreground)" },
  actual: { label: "Actual", color: "var(--color-energy-grid)" },
} satisfies ChartConfig;

export function CostView({ currency }: CostViewProps) {
  const totalCost = useMemo(
    () => costItems.reduce((s, c) => s + c.monthlyAmount, 0),
    []
  );

  return (
    <div className="flex flex-col">
      {/* Cost Summary Cards */}
      <div className="px-6 py-6 border-b border-border">
        <div className="mb-4">
          <h2 className="text-lg font-semibold">Cost Summary</h2>
          <p className="text-sm text-muted-foreground">Current month cost breakdown</p>
        </div>
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {costItems.map((item) => {
            const pct = ((item.monthlyAmount / totalCost) * 100).toFixed(1);
            const isOver = item.variance > 0;
            return (
              <div key={item.category} className="border border-border rounded-lg p-4">
                <div className="flex items-center justify-between mb-2">
                  <span className="text-sm font-medium">{item.label}</span>
                  <Badge
                    variant="outline"
                    className={`text-xs ${isOver ? "text-energy-grid border-energy-grid/30" : "text-energy-solar border-energy-solar/30"}`}
                  >
                    {isOver ? <TrendingUp className="h-3 w-3 mr-1" /> : <TrendingDown className="h-3 w-3 mr-1" />}
                    {item.variance > 0 ? "+" : ""}{item.variance.toFixed(1)}%
                  </Badge>
                </div>
                <p className="text-2xl font-semibold tracking-tight">
                  {formatCurrency(item.monthlyAmount, currency)}
                </p>
                <div className="flex items-center justify-between mt-2 text-xs text-muted-foreground">
                  <span>Budget: {formatCurrency(item.budgeted, currency)}</span>
                  <span>{pct}% of total</span>
                </div>
              </div>
            );
          })}
        </div>
      </div>

      {/* Budget vs Actual */}
      <div className="px-6 py-6 border-b border-border">
        <div className="mb-4">
          <h2 className="text-lg font-semibold">Budget vs Actual</h2>
          <p className="text-sm text-muted-foreground">12-month budget vs actual cost comparison</p>
        </div>
        <ChartContainer config={budgetConfig} className="h-[280px] w-full">
          <BarChart data={budgetVsActual} margin={{ top: 10, right: 10, left: 0, bottom: 0 }}>
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
            <Bar dataKey="budgeted" fill="var(--color-muted-foreground)" radius={[4, 4, 0, 0]} opacity={0.4} />
            <Bar dataKey="actual" fill="var(--color-energy-grid)" radius={[4, 4, 0, 0]} />
          </BarChart>
        </ChartContainer>
        <div className="flex gap-6 mt-3 justify-center text-xs">
          <div className="flex items-center gap-1.5">
            <span className="h-2.5 w-2.5 rounded-full bg-muted-foreground opacity-40" />
            <span className="text-muted-foreground">Budgeted</span>
          </div>
          <div className="flex items-center gap-1.5">
            <span className="h-2.5 w-2.5 rounded-full bg-energy-grid" />
            <span className="text-muted-foreground">Actual</span>
          </div>
        </div>
      </div>

      {/* Cost Trend Stacked Area */}
      <div className="px-6 py-6 border-b border-border">
        <div className="mb-4">
          <h2 className="text-lg font-semibold">Cost Trend</h2>
          <p className="text-sm text-muted-foreground">12-month cost by category</p>
        </div>
        <ChartContainer config={costTrendConfig} className="h-[300px] w-full">
          <AreaChart data={monthlyCostTrend} margin={{ top: 10, right: 10, left: -20, bottom: 0 }}>
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
            <Area type="monotone" dataKey="depreciation" stackId="1" stroke="var(--color-primary)" fill="var(--color-primary)" fillOpacity={0.4} />
            <Area type="monotone" dataKey="operations" stackId="1" stroke="var(--color-energy-grid)" fill="var(--color-energy-grid)" fillOpacity={0.5} />
            <Area type="monotone" dataKey="grid_charges" stackId="1" stroke="var(--color-energy-battery)" fill="var(--color-energy-battery)" fillOpacity={0.5} />
            <Area type="monotone" dataKey="maintenance" stackId="1" stroke="var(--color-energy-consumption)" fill="var(--color-energy-consumption)" fillOpacity={0.5} />
            <Area type="monotone" dataKey="insurance" stackId="1" stroke="var(--color-chart-5)" fill="var(--color-chart-5)" fillOpacity={0.5} />
            <Area type="monotone" dataKey="admin" stackId="1" stroke="var(--color-muted-foreground)" fill="var(--color-muted-foreground)" fillOpacity={0.3} />
          </AreaChart>
        </ChartContainer>
        <div className="flex flex-wrap gap-4 mt-3 justify-center">
          {Object.entries(costTrendConfig).map(([key, cfg]) => (
            <div key={key} className="flex items-center gap-1.5 text-xs">
              <span className="h-2.5 w-2.5 rounded-full shrink-0" style={{ backgroundColor: cfg.color }} />
              <span className="text-muted-foreground">{cfg.label}</span>
            </div>
          ))}
        </div>
      </div>

      {/* Cost Detail Table */}
      <div className="px-6 py-6">
        <div className="mb-4">
          <h2 className="text-lg font-semibold">Cost Detail</h2>
          <p className="text-sm text-muted-foreground">Item-level cost breakdown</p>
        </div>
        <div className="border border-border rounded-lg overflow-hidden">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-border bg-muted/50">
                <th className="text-left font-medium px-4 py-3">Category</th>
                <th className="text-right font-medium px-4 py-3">Monthly</th>
                <th className="text-right font-medium px-4 py-3">Budgeted</th>
                <th className="text-right font-medium px-4 py-3">Variance</th>
              </tr>
            </thead>
            <tbody>
              {costItems.map((item) => (
                <tr key={item.category} className="border-b border-border last:border-0">
                  <td className="px-4 py-3 font-medium">{item.label}</td>
                  <td className="px-4 py-3 text-right">{formatCurrency(item.monthlyAmount, currency)}</td>
                  <td className="px-4 py-3 text-right text-muted-foreground">{formatCurrency(item.budgeted, currency)}</td>
                  <td className={`px-4 py-3 text-right font-medium ${item.variance > 0 ? "text-energy-grid" : item.variance < 0 ? "text-energy-solar" : "text-muted-foreground"}`}>
                    {item.variance > 0 ? "+" : ""}{item.variance.toFixed(1)}%
                  </td>
                </tr>
              ))}
              <tr className="bg-muted/50 font-medium">
                <td className="px-4 py-3">Total</td>
                <td className="px-4 py-3 text-right">{formatCurrency(totalCost, currency)}</td>
                <td className="px-4 py-3 text-right text-muted-foreground">
                  {formatCurrency(costItems.reduce((s, c) => s + c.budgeted, 0), currency)}
                </td>
                <td className="px-4 py-3 text-right">—</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
