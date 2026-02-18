"use client";

import { useMemo } from "react";
import {
  Area,
  AreaChart,
  Line,
  LineChart,
  CartesianGrid,
  XAxis,
  YAxis,
  ResponsiveContainer,
} from "recharts";
import {
  ChartConfig,
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
} from "@/components/ui/chart";
import { getCurveDataForUnit } from "@/lib/bess/mock-data";

interface BessMonitoringTabProps {
  unitId: string;
}

const powerChartConfig = {
  power: {
    label: "Power (kW)",
    color: "var(--color-energy-battery)",
  },
  soc: {
    label: "SoC (%)",
    color: "var(--color-energy-solar)",
  },
} satisfies ChartConfig;

const voltageChartConfig = {
  voltage: {
    label: "Voltage (V)",
    color: "var(--color-energy-solar)",
  },
  current: {
    label: "Current (A)",
    color: "var(--color-energy-consumption)",
  },
} satisfies ChartConfig;

const temperatureChartConfig = {
  tempAvg: {
    label: "Avg Temp (°C)",
    color: "var(--color-energy-consumption)",
  },
  tempMin: {
    label: "Min Temp (°C)",
    color: "var(--color-energy-solar)",
  },
  tempMax: {
    label: "Max Temp (°C)",
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

export function BessMonitoringTab({ unitId }: BessMonitoringTabProps) {
  const data = useMemo(() => getCurveDataForUnit(unitId), [unitId]);

  if (data.power.length === 0) {
    return (
      <div className="flex-1 flex items-center justify-center text-muted-foreground py-20">
        No monitoring data available for this unit.
      </div>
    );
  }

  const interval = Math.max(1, Math.floor(data.power.length / 12));

  return (
    <div className="flex-1 overflow-y-auto px-6 py-4 space-y-8">
      {/* Power & SoC */}
      <ChartSection title="Power & State of Charge">
        <ChartContainer config={powerChartConfig} className="h-[260px] w-full">
          <ResponsiveContainer width="100%" height="100%">
            <AreaChart data={data.power} margin={{ top: 10, right: 10, left: -15, bottom: 0 }}>
              <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
              <XAxis dataKey="time" tickLine={false} axisLine={false} tickMargin={8} className="text-xs" interval={interval} />
              <YAxis yAxisId="power" tickLine={false} axisLine={false} tickMargin={8} className="text-xs" />
              <YAxis yAxisId="soc" orientation="right" tickLine={false} axisLine={false} tickMargin={8} className="text-xs" domain={[0, 100]} />
              <ChartTooltip content={<ChartTooltipContent indicator="line" />} />
              <Area yAxisId="power" type="monotone" dataKey="power" stroke="var(--color-energy-battery)" fill="var(--color-energy-battery)" fillOpacity={0.2} strokeWidth={2} />
              <Area yAxisId="soc" type="monotone" dataKey="soc" stroke="var(--color-energy-solar)" fill="var(--color-energy-solar)" fillOpacity={0.1} strokeWidth={1.5} />
            </AreaChart>
          </ResponsiveContainer>
        </ChartContainer>
        <ChartLegend items={[
          { color: "var(--color-energy-battery)", label: "Power (kW)" },
          { color: "var(--color-energy-solar)", label: "SoC (%)" },
        ]} />
      </ChartSection>

      {/* Voltage & Current */}
      <ChartSection title="Voltage & Current">
        <ChartContainer config={voltageChartConfig} className="h-[260px] w-full">
          <ResponsiveContainer width="100%" height="100%">
            <LineChart data={data.voltage} margin={{ top: 10, right: 10, left: -15, bottom: 0 }}>
              <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
              <XAxis dataKey="time" tickLine={false} axisLine={false} tickMargin={8} className="text-xs" interval={interval} />
              <YAxis yAxisId="voltage" tickLine={false} axisLine={false} tickMargin={8} className="text-xs" domain={["dataMin - 10", "dataMax + 10"]} />
              <YAxis yAxisId="current" orientation="right" tickLine={false} axisLine={false} tickMargin={8} className="text-xs" />
              <ChartTooltip content={<ChartTooltipContent indicator="line" />} />
              <Line yAxisId="voltage" type="monotone" dataKey="voltage" stroke="var(--color-energy-solar)" strokeWidth={1.5} dot={false} />
              <Line yAxisId="current" type="monotone" dataKey="current" stroke="var(--color-energy-consumption)" strokeWidth={1.5} dot={false} />
            </LineChart>
          </ResponsiveContainer>
        </ChartContainer>
        <ChartLegend items={[
          { color: "var(--color-energy-solar)", label: "Voltage (V)" },
          { color: "var(--color-energy-consumption)", label: "Current (A)" },
        ]} />
      </ChartSection>

      {/* Temperature */}
      <ChartSection title="Temperature">
        <ChartContainer config={temperatureChartConfig} className="h-[260px] w-full">
          <ResponsiveContainer width="100%" height="100%">
            <AreaChart data={data.temperature} margin={{ top: 10, right: 10, left: -15, bottom: 0 }}>
              <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
              <XAxis dataKey="time" tickLine={false} axisLine={false} tickMargin={8} className="text-xs" interval={interval} />
              <YAxis tickLine={false} axisLine={false} tickMargin={8} className="text-xs" />
              <ChartTooltip content={<ChartTooltipContent indicator="line" />} />
              <Area type="monotone" dataKey="tempMax" stroke="var(--color-energy-grid)" fill="var(--color-energy-grid)" fillOpacity={0.1} strokeWidth={1.5} />
              <Area type="monotone" dataKey="tempAvg" stroke="var(--color-energy-consumption)" fill="var(--color-energy-consumption)" fillOpacity={0.15} strokeWidth={2} />
              <Area type="monotone" dataKey="tempMin" stroke="var(--color-energy-solar)" fill="var(--color-energy-solar)" fillOpacity={0.1} strokeWidth={1.5} />
            </AreaChart>
          </ResponsiveContainer>
        </ChartContainer>
        <ChartLegend items={[
          { color: "var(--color-energy-grid)", label: "Max Temp (°C)" },
          { color: "var(--color-energy-consumption)", label: "Avg Temp (°C)" },
          { color: "var(--color-energy-solar)", label: "Min Temp (°C)" },
        ]} />
      </ChartSection>

      <div className="h-6" />
    </div>
  );
}
