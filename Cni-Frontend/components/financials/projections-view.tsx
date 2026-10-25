"use client";

import { useState, useMemo } from "react";
import {
  Area,
  AreaChart,
  CartesianGrid,
  Line,
  LineChart,
  XAxis,
  YAxis,
} from "recharts";
import {
  type ChartConfig,
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
} from "@/components/ui/chart";
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";
import {
  projectionScenarios,
  formatCurrency,
  formatCurrencyCompact,
} from "@/lib/financials/mock-data";
import type { Currency } from "@/lib/financials/types";

interface ProjectionsViewProps {
  currency: Currency;
}

const projectionLineConfig = {
  revenue: { label: "Revenue", color: "var(--color-status-operational)" },
  cost: { label: "Cost", color: "var(--color-severity-critical)" },
  netIncome: { label: "Net Income", color: "var(--color-primary)" },
} satisfies ChartConfig;

const cumulativeConfig = {
  cumulative: {
    label: "Cumulative Net Income",
    color: "var(--color-status-operational)",
  },
} satisfies ChartConfig;

export function ProjectionsView({ currency }: ProjectionsViewProps) {
  const [scenarioName, setScenarioName] = useState("Base");

  const scenario = useMemo(
    () => projectionScenarios.find((s) => s.name === scenarioName) ?? projectionScenarios[0],
    [scenarioName]
  );

  return (
    <div className="flex flex-col">
      {/* Scenario Tabs */}
      <div className="px-6 pt-6 pb-4 border-b border-border">
        <div className="flex items-center justify-between mb-4">
          <div>
            <h2 className="text-lg font-semibold">Financial Projections</h2>
            <p className="text-sm text-muted-foreground">12-month forward-looking estimates</p>
          </div>
          <Tabs value={scenarioName} onValueChange={setScenarioName}>
            <TabsList>
              <TabsTrigger value="Pessimistic">Pessimistic</TabsTrigger>
              <TabsTrigger value="Base">Base</TabsTrigger>
              <TabsTrigger value="Optimistic">Optimistic</TabsTrigger>
            </TabsList>
          </Tabs>
        </div>

        {/* Investment Metric Cards */}
        <div className="grid gap-4 sm:grid-cols-3">
          <div className="border border-border rounded-lg p-4">
            <p className="text-sm text-muted-foreground mb-1">Net Present Value</p>
            <p className={`text-2xl font-semibold tracking-tight ${scenario.npv >= 0 ? "text-status-operational" : "text-severity-critical"}`}>
              {formatCurrencyCompact(scenario.npv, currency)}
            </p>
          </div>
          <div className="border border-border rounded-lg p-4">
            <p className="text-sm text-muted-foreground mb-1">Internal Rate of Return</p>
            <p className="text-2xl font-semibold tracking-tight">
              {scenario.irr}%
            </p>
          </div>
          <div className="border border-border rounded-lg p-4">
            <p className="text-sm text-muted-foreground mb-1">Payback Period</p>
            <p className="text-2xl font-semibold tracking-tight">
              {scenario.paybackYears} years
            </p>
          </div>
        </div>
      </div>

      {/* 12-Month Projection Line Chart */}
      <div className="px-6 py-6 border-b border-border">
        <div className="mb-4">
          <h2 className="text-lg font-semibold">12-Month Projection — {scenario.name}</h2>
          <p className="text-sm text-muted-foreground">Projected revenue, cost, and net income</p>
        </div>
        <ChartContainer config={projectionLineConfig} className="h-[300px] w-full">
          <LineChart data={scenario.months} margin={{ top: 10, right: 10, left: 0, bottom: 0 }}>
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
            <Line type="monotone" dataKey="revenue" stroke="var(--color-status-operational)" strokeWidth={2} dot={false} />
            <Line type="monotone" dataKey="cost" stroke="var(--color-severity-critical)" strokeWidth={2} dot={false} />
            <Line type="monotone" dataKey="netIncome" stroke="var(--color-primary)" strokeWidth={2} dot={false} strokeDasharray="5 5" />
          </LineChart>
        </ChartContainer>
        <div className="flex gap-6 mt-3 justify-center text-xs">
          <div className="flex items-center gap-1.5">
            <span className="h-0.5 w-4 rounded-full bg-status-operational" />
            <span className="text-muted-foreground">Revenue</span>
          </div>
          <div className="flex items-center gap-1.5">
            <span className="h-0.5 w-4 rounded-full bg-severity-critical" />
            <span className="text-muted-foreground">Cost</span>
          </div>
          <div className="flex items-center gap-1.5">
            <span className="h-0.5 w-4 rounded-full bg-primary border-dashed" />
            <span className="text-muted-foreground">Net Income</span>
          </div>
        </div>
      </div>

      {/* Cumulative Cash Flow */}
      <div className="px-6 py-6">
        <div className="mb-4">
          <h2 className="text-lg font-semibold">Cumulative Cash Flow</h2>
          <p className="text-sm text-muted-foreground">Running total of projected net income</p>
        </div>
        <ChartContainer config={cumulativeConfig} className="h-[280px] w-full">
          <AreaChart data={scenario.months} margin={{ top: 10, right: 10, left: 0, bottom: 0 }}>
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
              dataKey="cumulative"
              stroke="var(--color-status-operational)"
              fill="var(--color-status-operational)"
              fillOpacity={0.2}
              strokeWidth={2}
            />
          </AreaChart>
        </ChartContainer>
      </div>

      {/* Scenario Comparison Table */}
      <div className="px-6 pb-6">
        <div className="mb-4">
          <h2 className="text-lg font-semibold">Scenario Comparison</h2>
          <p className="text-sm text-muted-foreground">Side-by-side scenario metrics</p>
        </div>
        <div className="border border-border rounded-lg overflow-hidden">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-border bg-muted/50">
                <th className="text-left font-medium px-4 py-3">Metric</th>
                {projectionScenarios.map((s) => (
                  <th key={s.name} className={`text-right font-medium px-4 py-3 ${s.name === scenarioName ? "text-primary" : ""}`}>
                    {s.name}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              <tr className="border-b border-border">
                <td className="px-4 py-3 font-medium">NPV</td>
                {projectionScenarios.map((s) => (
                  <td key={s.name} className={`px-4 py-3 text-right ${s.npv >= 0 ? "text-status-operational" : "text-severity-critical"}`}>
                    {formatCurrencyCompact(s.npv, currency)}
                  </td>
                ))}
              </tr>
              <tr className="border-b border-border">
                <td className="px-4 py-3 font-medium">IRR</td>
                {projectionScenarios.map((s) => (
                  <td key={s.name} className="px-4 py-3 text-right">{s.irr}%</td>
                ))}
              </tr>
              <tr className="border-b border-border">
                <td className="px-4 py-3 font-medium">Payback Period</td>
                {projectionScenarios.map((s) => (
                  <td key={s.name} className="px-4 py-3 text-right">{s.paybackYears} yrs</td>
                ))}
              </tr>
              <tr>
                <td className="px-4 py-3 font-medium">12mo Cumulative</td>
                {projectionScenarios.map((s) => {
                  const last = s.months[s.months.length - 1];
                  return (
                    <td key={s.name} className={`px-4 py-3 text-right font-medium ${last.cumulative >= 0 ? "text-status-operational" : "text-severity-critical"}`}>
                      {formatCurrencyCompact(last.cumulative, currency)}
                    </td>
                  );
                })}
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
