"use client";

import { useMemo } from "react";
import {
  Area,
  AreaChart,
  Line,
  LineChart,
  CartesianGrid,
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
import { getCurveDataForConsumer } from "@/lib/consumption/mock-data";

interface ConsumerCurveTabProps {
  consumerId: string;
}

const consumptionChartConfig = {
  activePower: {
    label: "Active Power (kW)",
    color: "var(--color-energy-consumption)",
  },
  reactivePower: {
    label: "Reactive Power (kVar)",
    color: "var(--color-energy-battery)",
  },
} satisfies ChartConfig;

const demandChartConfig = {
  demand: {
    label: "Demand (kW)",
    color: "var(--color-energy-consumption)",
  },
  contracted: {
    label: "Contracted (kW)",
    color: "var(--color-energy-grid)",
  },
} satisfies ChartConfig;

function ChartSection({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <div className="space-y-3">
      <h3 className="text-sm font-semibold text-foreground">{title}</h3>
      {children}
    </div>
  );
}

function ChartLegend({ items }: { items: { color: string; label: string }[] }) {
  return (
    <div className="flex items-center justify-center gap-6 mt-2 text-sm">
      {items.map((item) => (
        <div key={item.label} className="flex items-center gap-2">
          <div className="w-3 h-3 rounded-full" style={{ backgroundColor: item.color }} />
          <span className="text-muted-foreground text-xs">{item.label}</span>
        </div>
      ))}
    </div>
  );
}

export function ConsumerCurveTab({ consumerId }: ConsumerCurveTabProps) {
  const data = useMemo(() => getCurveDataForConsumer(consumerId), [consumerId]);

  if (data.consumption.length === 0) {
    return (
      <div className="flex-1 flex items-center justify-center text-muted-foreground py-20">
        No curve data available for this consumer.
      </div>
    );
  }

  const interval = Math.max(1, Math.floor(data.consumption.length / 12));

  // Get contracted capacity for reference line
  const contracted = data.demand.length > 0 ? data.demand[0].contracted : 0;

  return (
    <div className="flex-1 overflow-y-auto px-6 py-4 space-y-8">
      {/* Consumption Curve */}
      <ChartSection title="Consumption Curve">
        <ChartContainer config={consumptionChartConfig} className="h-[260px] w-full">
          <AreaChart data={data.consumption} margin={{ top: 10, right: 10, left: -15, bottom: 0 }}>
            <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
            <XAxis dataKey="time" tickLine={false} axisLine={false} tickMargin={8} className="text-xs" interval={interval} />
            <YAxis tickLine={false} axisLine={false} tickMargin={8} className="text-xs" />
            <ChartTooltip content={<ChartTooltipContent indicator="line" />} />
            <Area type="monotone" dataKey="activePower" stroke="var(--color-energy-consumption)" fill="var(--color-energy-consumption)" fillOpacity={0.25} strokeWidth={2} />
            <Area type="monotone" dataKey="reactivePower" stroke="var(--color-energy-battery)" fill="var(--color-energy-battery)" fillOpacity={0.1} strokeWidth={1.5} />
          </AreaChart>
        </ChartContainer>
        <ChartLegend items={[
          { color: "var(--color-energy-consumption)", label: "Active Power (kW)" },
          { color: "var(--color-energy-battery)", label: "Reactive Power (kVar)" },
        ]} />
      </ChartSection>

      {/* Demand Profile */}
      <ChartSection title="Demand Profile">
        <ChartContainer config={demandChartConfig} className="h-[260px] w-full">
          <LineChart data={data.demand} margin={{ top: 10, right: 10, left: -15, bottom: 0 }}>
            <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
            <XAxis dataKey="time" tickLine={false} axisLine={false} tickMargin={8} className="text-xs" interval={interval} />
            <YAxis tickLine={false} axisLine={false} tickMargin={8} className="text-xs" />
            <ReferenceLine y={contracted} stroke="var(--color-energy-grid)" strokeDasharray="4 4" label={{ value: "Contracted", position: "right", className: "text-xs fill-energy-grid" }} />
            <ChartTooltip content={<ChartTooltipContent indicator="line" />} />
            <Line type="monotone" dataKey="demand" stroke="var(--color-energy-consumption)" strokeWidth={2} dot={false} />
          </LineChart>
        </ChartContainer>
        <ChartLegend items={[
          { color: "var(--color-energy-consumption)", label: "Actual Demand (kW)" },
          { color: "var(--color-energy-grid)", label: "Contracted Capacity (kW)" },
        ]} />
      </ChartSection>

      <div className="h-6" />
    </div>
  );
}
