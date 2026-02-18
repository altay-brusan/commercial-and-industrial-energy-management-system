"use client";

import { useMemo } from "react";
import {
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
import { AbbrTooltip } from "@/components/ui/abbr-tooltip";
import { assetFinancials, formatCurrency, formatCurrencyCompact } from "@/lib/financials/mock-data";
import type { Currency } from "@/lib/financials/types";

interface AssetPerformanceViewProps {
  currency: Currency;
}

const assetTypeColors: Record<string, string> = {
  solar: "var(--color-energy-solar)",
  bess: "var(--color-energy-battery)",
  wind: "var(--color-primary)",
  grid: "var(--color-muted-foreground)",
};

const roiConfig = {
  roi: {
    label: "ROI (%)",
    color: "var(--color-energy-solar)",
  },
} satisfies ChartConfig;

const revenueExpenseConfig = {
  monthlyRevenue: { label: "Revenue", color: "var(--color-energy-solar)" },
  monthlyCost: { label: "Cost", color: "var(--color-energy-grid)" },
} satisfies ChartConfig;

export function AssetPerformanceView({ currency }: AssetPerformanceViewProps) {
  const roiData = useMemo(
    () =>
      assetFinancials
        .filter((a) => a.roi > 0)
        .sort((a, b) => b.roi - a.roi)
        .map((a) => ({ name: a.name, roi: a.roi, fill: assetTypeColors[a.type] })),
    []
  );

  const revenueExpenseData = useMemo(
    () =>
      assetFinancials.map((a) => ({
        name: a.name,
        monthlyRevenue: a.monthlyRevenue,
        monthlyCost: a.monthlyCost,
      })),
    []
  );

  return (
    <div className="flex flex-col">
      {/* Asset Comparison Table */}
      <div className="px-6 py-6 border-b border-border">
        <div className="mb-4">
          <h2 className="text-lg font-semibold">Asset Financial Comparison</h2>
          <p className="text-sm text-muted-foreground">Key financial metrics per asset</p>
        </div>
        <div className="border border-border rounded-lg overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-border bg-muted/50">
                <th className="text-left font-medium px-4 py-3">Asset</th>
                <th className="text-left font-medium px-4 py-3">Capacity</th>
                <th className="text-right font-medium px-4 py-3"><div className="flex items-center justify-end"><AbbrTooltip label="CF" title="Capacity Factor" description="Actual output as a percentage of maximum possible output" /></div></th>
                <th className="text-right font-medium px-4 py-3">Production</th>
                <th className="text-right font-medium px-4 py-3">Revenue</th>
                <th className="text-right font-medium px-4 py-3">Cost</th>
                <th className="text-right font-medium px-4 py-3">Net</th>
                <th className="text-right font-medium px-4 py-3"><div className="flex items-center justify-end"><AbbrTooltip label="LCOE" title="Levelized Cost of Energy" description="Average cost per unit of energy produced over the asset's lifetime" /></div></th>
                <th className="text-right font-medium px-4 py-3"><div className="flex items-center justify-end"><AbbrTooltip label="ROI" title="Return on Investment" description="Net profit as a percentage of the initial investment" /></div></th>
              </tr>
            </thead>
            <tbody>
              {assetFinancials.map((asset) => (
                <tr key={asset.name} className="border-b border-border last:border-0">
                  <td className="px-4 py-3">
                    <div className="flex items-center gap-2">
                      <span
                        className="h-2.5 w-2.5 rounded-full shrink-0"
                        style={{ backgroundColor: assetTypeColors[asset.type] }}
                      />
                      <span className="font-medium">{asset.name}</span>
                    </div>
                  </td>
                  <td className="px-4 py-3 text-muted-foreground">{asset.capacity}</td>
                  <td className="px-4 py-3 text-right">{asset.capacityFactor}%</td>
                  <td className="px-4 py-3 text-right">{asset.monthlyProduction.toLocaleString("en-US")} MWh</td>
                  <td className="px-4 py-3 text-right text-energy-solar">{formatCurrency(asset.monthlyRevenue, currency)}</td>
                  <td className="px-4 py-3 text-right text-energy-grid">{formatCurrency(asset.monthlyCost, currency)}</td>
                  <td className={`px-4 py-3 text-right font-medium ${asset.netIncome >= 0 ? "text-energy-solar" : "text-energy-grid"}`}>
                    {formatCurrency(asset.netIncome, currency)}
                  </td>
                  <td className="px-4 py-3 text-right font-mono text-xs">
                    {asset.lcoe > 0 ? `$${asset.lcoe.toFixed(3)}` : "—"}
                  </td>
                  <td className="px-4 py-3 text-right">
                    <Badge
                      variant="outline"
                      className={`text-xs ${asset.roi >= 10 ? "text-energy-solar border-energy-solar/30" : "text-muted-foreground"}`}
                    >
                      {asset.roi}%
                    </Badge>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* ROI Ranking + Revenue/Expense Chart */}
      <div className="px-6 py-6 border-b border-border">
        <div className="grid gap-8 lg:grid-cols-2">
          {/* ROI Ranking */}
          <div>
            <div className="mb-4">
              <h2 className="text-lg font-semibold">ROI Ranking</h2>
              <p className="text-sm text-muted-foreground">Return on investment by asset</p>
            </div>
            <ChartContainer config={roiConfig} className="h-[220px] w-full">
              <BarChart data={roiData} layout="vertical" margin={{ top: 10, right: 10, left: 0, bottom: 0 }}>
                <CartesianGrid strokeDasharray="3 3" className="stroke-border" horizontal={false} />
                <XAxis type="number" tickLine={false} axisLine={false} className="text-xs" unit="%" />
                <YAxis dataKey="name" type="category" tickLine={false} axisLine={false} width={100} className="text-xs" />
                <ChartTooltip content={<ChartTooltipContent formatter={(v) => `${v}%`} />} />
                <Bar dataKey="roi" radius={[0, 4, 4, 0]}>
                  {roiData.map((entry) => (
                    <Bar key={entry.name} dataKey="roi" fill={entry.fill} />
                  ))}
                </Bar>
              </BarChart>
            </ChartContainer>
          </div>

          {/* Revenue vs Cost by Asset */}
          <div>
            <div className="mb-4">
              <h2 className="text-lg font-semibold">Revenue vs Cost by Asset</h2>
              <p className="text-sm text-muted-foreground">Monthly revenue and cost comparison</p>
            </div>
            <ChartContainer config={revenueExpenseConfig} className="h-[220px] w-full">
              <BarChart data={revenueExpenseData} margin={{ top: 10, right: 10, left: 0, bottom: 0 }}>
                <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
                <XAxis dataKey="name" tickLine={false} axisLine={false} tickMargin={8} className="text-xs" />
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
                <Bar dataKey="monthlyRevenue" fill="var(--color-energy-solar)" radius={[4, 4, 0, 0]} />
                <Bar dataKey="monthlyCost" fill="var(--color-energy-grid)" radius={[4, 4, 0, 0]} />
              </BarChart>
            </ChartContainer>
            <div className="flex gap-6 mt-3 justify-center text-xs">
              <div className="flex items-center gap-1.5">
                <span className="h-2.5 w-2.5 rounded-full bg-energy-solar" />
                <span className="text-muted-foreground">Revenue</span>
              </div>
              <div className="flex items-center gap-1.5">
                <span className="h-2.5 w-2.5 rounded-full bg-energy-grid" />
                <span className="text-muted-foreground">Cost</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Investment Metrics */}
      <div className="px-6 py-6">
        <div className="mb-4">
          <h2 className="text-lg font-semibold">Investment Metrics</h2>
          <p className="text-sm text-muted-foreground">NPV, IRR, and payback period per asset</p>
        </div>
        <div className="border border-border rounded-lg overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-border bg-muted/50">
                <th className="text-left font-medium px-4 py-3">Asset</th>
                <th className="text-right font-medium px-4 py-3">Investment</th>
                <th className="text-right font-medium px-4 py-3"><div className="flex items-center justify-end"><AbbrTooltip label="NPV" title="Net Present Value" description="Total value of future cash flows discounted to today's value" /></div></th>
                <th className="text-right font-medium px-4 py-3"><div className="flex items-center justify-end"><AbbrTooltip label="IRR" title="Internal Rate of Return" description="Annualized rate of return that makes NPV equal to zero" /></div></th>
                <th className="text-right font-medium px-4 py-3">Payback</th>
              </tr>
            </thead>
            <tbody>
              {assetFinancials.map((asset) => (
                <tr key={asset.name} className="border-b border-border last:border-0">
                  <td className="px-4 py-3">
                    <div className="flex items-center gap-2">
                      <span
                        className="h-2.5 w-2.5 rounded-full shrink-0"
                        style={{ backgroundColor: assetTypeColors[asset.type] }}
                      />
                      <span className="font-medium">{asset.name}</span>
                    </div>
                  </td>
                  <td className="px-4 py-3 text-right">{formatCurrencyCompact(asset.investment, currency)}</td>
                  <td className={`px-4 py-3 text-right font-medium ${asset.npv >= 0 ? "text-energy-solar" : "text-energy-grid"}`}>
                    {formatCurrencyCompact(asset.npv, currency)}
                  </td>
                  <td className="px-4 py-3 text-right">{asset.irr}%</td>
                  <td className="px-4 py-3 text-right">{asset.paybackYears} yrs</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
