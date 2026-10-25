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
import { getCurveDataForInverter } from "@/lib/solar-inverters/mock-data";

interface InverterCurveTabProps {
  inverterId: string;
}

const powerChartConfig = {
  activePower: {
    label: "Active Power (kW)",
    color: "var(--color-energy-solar)",
  },
  dcPower: {
    label: "DC Power (kW)",
    color: "var(--color-energy-consumption)",
  },
  reactivePower: {
    label: "Reactive Power (var)",
    color: "var(--color-energy-battery)",
  },
} satisfies ChartConfig;

const voltageChartConfig = {
  abVoltage: {
    label: "A-B Voltage (V)",
    color: "var(--color-energy-solar)",
  },
  bcVoltage: {
    label: "B-C Voltage (V)",
    color: "var(--color-energy-consumption)",
  },
  caVoltage: {
    label: "C-A Voltage (V)",
    color: "var(--color-energy-battery)",
  },
} satisfies ChartConfig;

const currentChartConfig = {
  phaseA: {
    label: "Phase A (A)",
    color: "var(--color-energy-solar)",
  },
  phaseB: {
    label: "Phase B (A)",
    color: "var(--color-energy-consumption)",
  },
  phaseC: {
    label: "Phase C (A)",
    color: "var(--color-energy-battery)",
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

export function InverterCurveTab({ inverterId }: InverterCurveTabProps) {
  const data = useMemo(() => getCurveDataForInverter(inverterId), [inverterId]);

  if (data.power.length === 0) {
    return (
      <div className="flex-1 flex items-center justify-center text-muted-foreground py-20">
        No curve data available for this device.
      </div>
    );
  }

  return (
    <div className="flex-1 overflow-y-auto px-6 py-4 space-y-8">
      {/* Power Curve */}
      <ChartSection title="Power Curve">
        <ChartContainer config={powerChartConfig} className="h-[260px] w-full">
          <ResponsiveContainer width="100%" height="100%">
            <AreaChart data={data.power} margin={{ top: 10, right: 10, left: -15, bottom: 0 }}>
              <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
              <XAxis dataKey="time" tickLine={false} axisLine={false} tickMargin={8} className="text-xs" interval={Math.max(1, Math.floor(data.power.length / 12))} />
              <YAxis tickLine={false} axisLine={false} tickMargin={8} className="text-xs" />
              <ChartTooltip content={<ChartTooltipContent indicator="line" />} />
              <Area type="monotone" dataKey="activePower" stroke="var(--color-energy-solar)" fill="var(--color-energy-solar)" fillOpacity={0.25} strokeWidth={2} />
              <Area type="monotone" dataKey="dcPower" stroke="var(--color-energy-consumption)" fill="var(--color-energy-consumption)" fillOpacity={0.1} strokeWidth={1.5} />
              <Area type="monotone" dataKey="reactivePower" stroke="var(--color-energy-battery)" fill="var(--color-energy-battery)" fillOpacity={0.1} strokeWidth={1.5} />
            </AreaChart>
          </ResponsiveContainer>
        </ChartContainer>
        <ChartLegend items={[
          { color: "var(--color-energy-solar)", label: "Active Power (kW)" },
          { color: "var(--color-energy-consumption)", label: "DC Power (kW)" },
          { color: "var(--color-energy-battery)", label: "Reactive Power (var)" },
        ]} />
      </ChartSection>

      {/* Voltage Curve */}
      <ChartSection title="Voltage Curve (3-Phase Line)">
        <ChartContainer config={voltageChartConfig} className="h-[260px] w-full">
          <ResponsiveContainer width="100%" height="100%">
            <LineChart data={data.voltage} margin={{ top: 10, right: 10, left: -15, bottom: 0 }}>
              <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
              <XAxis dataKey="time" tickLine={false} axisLine={false} tickMargin={8} className="text-xs" interval={Math.max(1, Math.floor(data.voltage.length / 12))} />
              <YAxis tickLine={false} axisLine={false} tickMargin={8} className="text-xs" domain={["dataMin - 5", "dataMax + 5"]} />
              <ChartTooltip content={<ChartTooltipContent indicator="line" />} />
              <Line type="monotone" dataKey="abVoltage" stroke="var(--color-energy-solar)" strokeWidth={1.5} dot={false} />
              <Line type="monotone" dataKey="bcVoltage" stroke="var(--color-energy-consumption)" strokeWidth={1.5} dot={false} />
              <Line type="monotone" dataKey="caVoltage" stroke="var(--color-energy-battery)" strokeWidth={1.5} dot={false} />
            </LineChart>
          </ResponsiveContainer>
        </ChartContainer>
        <ChartLegend items={[
          { color: "var(--color-energy-solar)", label: "A-B Voltage (V)" },
          { color: "var(--color-energy-consumption)", label: "B-C Voltage (V)" },
          { color: "var(--color-energy-battery)", label: "C-A Voltage (V)" },
        ]} />
      </ChartSection>

      {/* Current Curve */}
      <ChartSection title="Current Curve (3-Phase)">
        <ChartContainer config={currentChartConfig} className="h-[260px] w-full">
          <ResponsiveContainer width="100%" height="100%">
            <LineChart data={data.current} margin={{ top: 10, right: 10, left: -15, bottom: 0 }}>
              <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
              <XAxis dataKey="time" tickLine={false} axisLine={false} tickMargin={8} className="text-xs" interval={Math.max(1, Math.floor(data.current.length / 12))} />
              <YAxis tickLine={false} axisLine={false} tickMargin={8} className="text-xs" />
              <ChartTooltip content={<ChartTooltipContent indicator="line" />} />
              <Line type="monotone" dataKey="phaseA" stroke="var(--color-energy-solar)" strokeWidth={1.5} dot={false} />
              <Line type="monotone" dataKey="phaseB" stroke="var(--color-energy-consumption)" strokeWidth={1.5} dot={false} />
              <Line type="monotone" dataKey="phaseC" stroke="var(--color-energy-battery)" strokeWidth={1.5} dot={false} />
            </LineChart>
          </ResponsiveContainer>
        </ChartContainer>
        <ChartLegend items={[
          { color: "var(--color-energy-solar)", label: "Phase A (A)" },
          { color: "var(--color-energy-consumption)", label: "Phase B (A)" },
          { color: "var(--color-energy-battery)", label: "Phase C (A)" },
        ]} />
      </ChartSection>

      <div className="h-6" />
    </div>
  );
}
