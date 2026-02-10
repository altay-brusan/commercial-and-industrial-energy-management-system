"use client";

import { useMemo } from "react";
import {
  Area,
  AreaChart,
  Bar,
  BarChart,
  CartesianGrid,
  Cell,
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
import { Badge } from "@/components/ui/badge";
import {
  tariffPeriods,
  getHourlyPriceProfile,
  getRealTimePricing,
  getPeakDemandData,
  formatCurrency,
  USD_TO_TRY,
} from "@/lib/financials/mock-data";
import type { Currency } from "@/lib/financials/types";

interface TariffViewProps {
  currency: Currency;
}

const periodColors: Record<string, string> = {
  "Off-Peak": "var(--color-energy-solar)",
  "Mid-Peak": "var(--color-energy-battery)",
  "On-Peak": "var(--color-energy-consumption)",
  "Super-Peak": "var(--color-energy-grid)",
};

const priceProfileConfig = {
  rate: {
    label: "Rate",
    color: "var(--color-primary)",
  },
} satisfies ChartConfig;

const rtpConfig = {
  price: {
    label: "Spot Price ($/MWh)",
    color: "var(--color-energy-consumption)",
  },
} satisfies ChartConfig;

const peakConfig = {
  demand: { label: "Demand (kW)", color: "var(--color-energy-consumption)" },
  shaved: { label: "BESS Shaved (kW)", color: "var(--color-energy-solar)" },
} satisfies ChartConfig;

export function TariffView({ currency }: TariffViewProps) {
  const priceProfile = useMemo(() => getHourlyPriceProfile(), []);
  const rtPricing = useMemo(() => getRealTimePricing(), []);
  const peakDemand = useMemo(() => getPeakDemandData(), []);

  const currentMonthlyCost = 42500;
  const optimizedCost = 36200;
  const savings = currentMonthlyCost - optimizedCost;

  return (
    <div className="flex flex-col">
      {/* TOU Tariff Table */}
      <div className="px-6 py-6 border-b border-border">
        <div className="mb-4">
          <h2 className="text-lg font-semibold">Time-of-Use Tariff Schedule</h2>
          <p className="text-sm text-muted-foreground">Current tariff periods and rates</p>
        </div>
        <div className="border border-border rounded-lg overflow-hidden">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-border bg-muted/50">
                <th className="text-left font-medium px-4 py-3">Period</th>
                <th className="text-left font-medium px-4 py-3">Hours</th>
                <th className="text-right font-medium px-4 py-3">Rate (USD/kWh)</th>
                <th className="text-right font-medium px-4 py-3">Rate (TRY/kWh)</th>
              </tr>
            </thead>
            <tbody>
              {tariffPeriods.map((period) => (
                <tr key={period.name} className="border-b border-border last:border-0">
                  <td className="px-4 py-3">
                    <div className="flex items-center gap-2">
                      <span
                        className="h-2.5 w-2.5 rounded-full shrink-0"
                        style={{ backgroundColor: periodColors[period.name] }}
                      />
                      <span className="font-medium">{period.name}</span>
                    </div>
                  </td>
                  <td className="px-4 py-3 text-muted-foreground">{period.hours}</td>
                  <td className="px-4 py-3 text-right font-mono">
                    ${period.rate.toFixed(3)}
                  </td>
                  <td className="px-4 py-3 text-right font-mono">
                    ₺{period.rateTRY.toFixed(2)}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* 24h Price Profile */}
      <div className="px-6 py-6 border-b border-border">
        <div className="mb-4">
          <h2 className="text-lg font-semibold">24-Hour Price Profile</h2>
          <p className="text-sm text-muted-foreground">Hourly TOU rate with period coloring</p>
        </div>
        <ChartContainer config={priceProfileConfig} className="h-[260px] w-full">
          <BarChart data={priceProfile} margin={{ top: 10, right: 10, left: 0, bottom: 0 }}>
            <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
            <XAxis dataKey="hour" tickLine={false} axisLine={false} tickMargin={8} className="text-xs" interval={2} />
            <YAxis
              tickLine={false}
              axisLine={false}
              tickMargin={8}
              className="text-xs"
              tickFormatter={(v) =>
                currency === "TRY" ? `₺${(v * USD_TO_TRY).toFixed(2)}` : `$${v.toFixed(3)}`
              }
            />
            <ChartTooltip
              content={
                <ChartTooltipContent
                  formatter={(value) =>
                    currency === "TRY"
                      ? `₺${(Number(value) * USD_TO_TRY).toFixed(2)}/kWh`
                      : `$${Number(value).toFixed(3)}/kWh`
                  }
                />
              }
            />
            <Bar dataKey="rate" radius={[2, 2, 0, 0]}>
              {priceProfile.map((entry) => (
                <Cell key={entry.hour} fill={periodColors[entry.period]} />
              ))}
            </Bar>
          </BarChart>
        </ChartContainer>
        <div className="flex gap-4 mt-3 justify-center">
          {Object.entries(periodColors).map(([name, color]) => (
            <div key={name} className="flex items-center gap-1.5 text-xs">
              <span className="h-2.5 w-2.5 rounded-full shrink-0" style={{ backgroundColor: color }} />
              <span className="text-muted-foreground">{name}</span>
            </div>
          ))}
        </div>
      </div>

      {/* Real-Time Pricing + Peak Demand */}
      <div className="px-6 py-6 border-b border-border">
        <div className="grid gap-8 lg:grid-cols-2">
          {/* Real-Time Pricing */}
          <div>
            <div className="mb-4">
              <h2 className="text-lg font-semibold">Real-Time Pricing</h2>
              <p className="text-sm text-muted-foreground">Last 24h spot market price ($/MWh)</p>
            </div>
            <ChartContainer config={rtpConfig} className="h-[260px] w-full">
              <LineChart data={rtPricing} margin={{ top: 10, right: 10, left: 0, bottom: 0 }}>
                <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
                <XAxis dataKey="hour" tickLine={false} axisLine={false} tickMargin={8} className="text-xs" interval={3} />
                <YAxis tickLine={false} axisLine={false} tickMargin={8} className="text-xs" />
                <ChartTooltip content={<ChartTooltipContent />} />
                <Line
                  type="monotone"
                  dataKey="price"
                  stroke="var(--color-energy-consumption)"
                  strokeWidth={2}
                  dot={false}
                />
              </LineChart>
            </ChartContainer>
          </div>

          {/* Peak Demand */}
          <div>
            <div className="mb-4">
              <h2 className="text-lg font-semibold">Peak Demand Shaving</h2>
              <p className="text-sm text-muted-foreground">Demand profile with BESS peak reduction</p>
            </div>
            <ChartContainer config={peakConfig} className="h-[260px] w-full">
              <AreaChart data={peakDemand} margin={{ top: 10, right: 10, left: 0, bottom: 0 }}>
                <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
                <XAxis dataKey="hour" tickLine={false} axisLine={false} tickMargin={8} className="text-xs" interval={3} />
                <YAxis tickLine={false} axisLine={false} tickMargin={8} className="text-xs" />
                <ChartTooltip content={<ChartTooltipContent />} />
                <Area
                  type="monotone"
                  dataKey="demand"
                  stroke="var(--color-energy-consumption)"
                  fill="var(--color-energy-consumption)"
                  fillOpacity={0.2}
                  strokeWidth={2}
                />
                <Area
                  type="monotone"
                  dataKey="shaved"
                  stroke="var(--color-energy-solar)"
                  fill="var(--color-energy-solar)"
                  fillOpacity={0.3}
                  strokeWidth={2}
                />
              </AreaChart>
            </ChartContainer>
            <div className="flex gap-6 mt-3 justify-center text-xs">
              <div className="flex items-center gap-1.5">
                <span className="h-2.5 w-2.5 rounded-full bg-energy-consumption" />
                <span className="text-muted-foreground">Total Demand</span>
              </div>
              <div className="flex items-center gap-1.5">
                <span className="h-2.5 w-2.5 rounded-full bg-energy-solar" />
                <span className="text-muted-foreground">BESS Shaved</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Tariff Optimization Summary */}
      <div className="px-6 py-6">
        <div className="mb-4">
          <h2 className="text-lg font-semibold">Tariff Optimization Summary</h2>
          <p className="text-sm text-muted-foreground">Potential savings through tariff optimization</p>
        </div>
        <div className="grid gap-4 sm:grid-cols-3">
          <div className="border border-border rounded-lg p-5">
            <p className="text-sm text-muted-foreground mb-1">Current Monthly Cost</p>
            <p className="text-2xl font-semibold tracking-tight text-energy-grid">
              {formatCurrency(currentMonthlyCost, currency)}
            </p>
          </div>
          <div className="border border-border rounded-lg p-5">
            <p className="text-sm text-muted-foreground mb-1">Optimized Cost</p>
            <p className="text-2xl font-semibold tracking-tight">
              {formatCurrency(optimizedCost, currency)}
            </p>
          </div>
          <div className="border border-border rounded-lg p-5">
            <p className="text-sm text-muted-foreground mb-1">Savings Potential</p>
            <div className="flex items-baseline gap-2">
              <p className="text-2xl font-semibold tracking-tight text-energy-solar">
                {formatCurrency(savings, currency)}
              </p>
              <Badge variant="outline" className="text-energy-solar border-energy-solar/30 text-xs">
                {((savings / currentMonthlyCost) * 100).toFixed(1)}%
              </Badge>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
