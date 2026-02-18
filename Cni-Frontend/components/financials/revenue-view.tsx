"use client";

import { useMemo } from "react";
import {
  TrendingUp,
  TrendingDown,
  Zap,
  Radio,
  Shield,
  Settings,
  Sun,
  Leaf,
} from "lucide-react";
import {
  Area,
  AreaChart,
  Bar,
  BarChart,
  CartesianGrid,
  Line,
  ComposedChart,
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
  revenueStreams,
  monthlyRevenueTrend,
  getArbitrageProfile,
  formatCurrency,
} from "@/lib/financials/mock-data";
import type { Currency, RevenueStreamType } from "@/lib/financials/types";

interface RevenueViewProps {
  currency: Currency;
}

const streamIcons: Record<RevenueStreamType, React.ReactNode> = {
  energy_arbitrage: <Zap className="h-4 w-4" />,
  demand_response: <Radio className="h-4 w-4" />,
  capacity_market: <Shield className="h-4 w-4" />,
  ancillary_services: <Settings className="h-4 w-4" />,
  feed_in_tariff: <Sun className="h-4 w-4" />,
  rec_carbon: <Leaf className="h-4 w-4" />,
};

const streamColors: Record<RevenueStreamType, string> = {
  energy_arbitrage: "var(--color-energy-battery)",
  demand_response: "var(--color-energy-consumption)",
  capacity_market: "var(--color-energy-grid)",
  ancillary_services: "var(--color-primary)",
  feed_in_tariff: "var(--color-energy-solar)",
  rec_carbon: "var(--color-chart-5)",
};

const trendConfig = {
  energy_arbitrage: { label: "Energy Arbitrage", color: "var(--color-energy-battery)" },
  demand_response: { label: "Demand Response", color: "var(--color-energy-consumption)" },
  capacity_market: { label: "Capacity Market", color: "var(--color-energy-grid)" },
  ancillary_services: { label: "Ancillary Services", color: "var(--color-primary)" },
  feed_in_tariff: { label: "Feed-in Tariff", color: "var(--color-energy-solar)" },
  rec_carbon: { label: "REC & Carbon", color: "var(--color-chart-5)" },
} satisfies ChartConfig;

const arbitrageConfig = {
  chargeDischarge: { label: "Charge/Discharge (MW)", color: "var(--color-energy-battery)" },
  price: { label: "Price ($/MWh)", color: "var(--color-energy-consumption)" },
} satisfies ChartConfig;

export function RevenueView({ currency }: RevenueViewProps) {
  const arbitrageData = useMemo(() => getArbitrageProfile(), []);

  return (
    <div className="flex flex-col">
      {/* Revenue Stream Cards */}
      <div className="px-6 py-6 border-b border-border">
        <div className="mb-4">
          <h2 className="text-lg font-semibold">Revenue Streams</h2>
          <p className="text-sm text-muted-foreground">Monthly revenue by source</p>
        </div>
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {revenueStreams.map((stream) => {
            const change = ((stream.monthlyAmount - stream.previousMonth) / stream.previousMonth) * 100;
            const isPositive = change >= 0;
            return (
              <div
                key={stream.type}
                className="border border-border rounded-lg p-4"
              >
                <div className="flex items-center justify-between mb-3">
                  <div className="flex items-center gap-2">
                    <div
                      className="p-1.5 rounded-md"
                      style={{ backgroundColor: `color-mix(in srgb, ${streamColors[stream.type]} 15%, transparent)` }}
                    >
                      {streamIcons[stream.type]}
                    </div>
                    <span className="text-sm font-medium">{stream.label}</span>
                  </div>
                  <Badge
                    variant="outline"
                    className={`text-xs ${isPositive ? "text-energy-solar border-energy-solar/30" : "text-energy-grid border-energy-grid/30"}`}
                  >
                    {isPositive ? <TrendingUp className="h-3 w-3 mr-1" /> : <TrendingDown className="h-3 w-3 mr-1" />}
                    {isPositive ? "+" : ""}{change.toFixed(1)}%
                  </Badge>
                </div>
                <p className="text-2xl font-semibold tracking-tight">
                  {formatCurrency(stream.monthlyAmount, currency)}
                </p>
                <p className="text-xs text-muted-foreground mt-1">
                  YTD: {formatCurrency(stream.ytdAmount, currency)}
                </p>
              </div>
            );
          })}
        </div>
      </div>

      {/* Revenue Trend Stacked Area */}
      <div className="px-6 py-6 border-b border-border">
        <div className="mb-4">
          <h2 className="text-lg font-semibold">Revenue Trend</h2>
          <p className="text-sm text-muted-foreground">12-month stacked revenue by stream</p>
        </div>
        <ChartContainer config={trendConfig} className="h-[320px] w-full">
          <AreaChart data={monthlyRevenueTrend} margin={{ top: 10, right: 10, left: 0, bottom: 0 }}>
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
            <Area type="monotone" dataKey="feed_in_tariff" stackId="1" stroke="var(--color-energy-solar)" fill="var(--color-energy-solar)" fillOpacity={0.6} />
            <Area type="monotone" dataKey="capacity_market" stackId="1" stroke="var(--color-energy-grid)" fill="var(--color-energy-grid)" fillOpacity={0.5} />
            <Area type="monotone" dataKey="demand_response" stackId="1" stroke="var(--color-energy-consumption)" fill="var(--color-energy-consumption)" fillOpacity={0.5} />
            <Area type="monotone" dataKey="rec_carbon" stackId="1" stroke="var(--color-chart-5)" fill="var(--color-chart-5)" fillOpacity={0.5} />
            <Area type="monotone" dataKey="ancillary_services" stackId="1" stroke="var(--color-primary)" fill="var(--color-primary)" fillOpacity={0.4} />
            <Area type="monotone" dataKey="energy_arbitrage" stackId="1" stroke="var(--color-energy-battery)" fill="var(--color-energy-battery)" fillOpacity={0.5} />
          </AreaChart>
        </ChartContainer>
        <div className="flex flex-wrap gap-4 mt-3 justify-center">
          {Object.entries(trendConfig).map(([key, cfg]) => (
            <div key={key} className="flex items-center gap-1.5 text-xs">
              <span className="h-2.5 w-2.5 rounded-full shrink-0" style={{ backgroundColor: cfg.color }} />
              <span className="text-muted-foreground">{cfg.label}</span>
            </div>
          ))}
        </div>
      </div>

      {/* Arbitrage Detail */}
      <div className="px-6 py-6">
        <div className="mb-4">
          <h2 className="text-lg font-semibold">BESS Arbitrage Profile</h2>
          <p className="text-sm text-muted-foreground">Charge/discharge pattern vs electricity price (24h)</p>
        </div>
        <ChartContainer config={arbitrageConfig} className="h-[280px] w-full">
          <ComposedChart data={arbitrageData} margin={{ top: 10, right: 10, left: 0, bottom: 0 }}>
            <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
            <XAxis dataKey="hour" tickLine={false} axisLine={false} tickMargin={8} className="text-xs" interval={2} />
            <YAxis
              yAxisId="left"
              tickLine={false}
              axisLine={false}
              tickMargin={8}
              className="text-xs"
              label={{ value: "MW", angle: -90, position: "insideLeft", style: { fontSize: 11 } }}
            />
            <YAxis
              yAxisId="right"
              orientation="right"
              tickLine={false}
              axisLine={false}
              tickMargin={8}
              className="text-xs"
              label={{ value: "$/MWh", angle: 90, position: "insideRight", style: { fontSize: 11 } }}
            />
            <ChartTooltip content={<ChartTooltipContent />} />
            <Bar yAxisId="left" dataKey="chargeDischarge" fill="var(--color-energy-battery)" radius={[2, 2, 0, 0]} opacity={0.7} />
            <Line yAxisId="right" type="monotone" dataKey="price" stroke="var(--color-energy-consumption)" strokeWidth={2} dot={false} />
          </ComposedChart>
        </ChartContainer>
        <div className="flex gap-6 mt-3 justify-center text-xs">
          <div className="flex items-center gap-1.5">
            <span className="h-2.5 w-2.5 rounded-full bg-energy-battery" />
            <span className="text-muted-foreground">Charge (-) / Discharge (+) MW</span>
          </div>
          <div className="flex items-center gap-1.5">
            <span className="h-2.5 w-6 rounded-full bg-energy-consumption" />
            <span className="text-muted-foreground">Electricity Price ($/MWh)</span>
          </div>
        </div>
      </div>
    </div>
  );
}
