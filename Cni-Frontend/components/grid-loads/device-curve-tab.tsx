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
  ResponsiveContainer,
} from "recharts";
import {
  ChartConfig,
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
} from "@/components/ui/chart";
import { getCurveDataForDevice } from "@/lib/grid-loads/mock-data";

interface DeviceCurveTabProps {
  deviceId: string;
}

const powerChartConfig = {
  activePower: {
    label: "Active Power (kW)",
    color: "var(--color-energy-solar)",
  },
  reactivePower: {
    label: "Reactive Power (kVar)",
    color: "var(--color-energy-battery)",
  },
  apparentPower: {
    label: "Apparent Power (kVA)",
    color: "var(--color-energy-consumption)",
  },
} satisfies ChartConfig;

const voltageChartConfig = {
  phaseA: {
    label: "Phase A (V)",
    color: "var(--color-energy-solar)",
  },
  phaseB: {
    label: "Phase B (V)",
    color: "var(--color-energy-consumption)",
  },
  phaseC: {
    label: "Phase C (V)",
    color: "var(--color-energy-battery)",
  },
} satisfies ChartConfig;

const frequencyChartConfig = {
  frequency: {
    label: "Frequency (Hz)",
    color: "var(--color-energy-solar)",
  },
} satisfies ChartConfig;

const powerFactorChartConfig = {
  powerFactor: {
    label: "Power Factor",
    color: "var(--color-energy-solar)",
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

export function DeviceCurveTab({ deviceId }: DeviceCurveTabProps) {
  const data = useMemo(() => getCurveDataForDevice(deviceId), [deviceId]);

  if (data.power.length === 0) {
    return (
      <div className="flex-1 flex items-center justify-center text-muted-foreground py-20">
        No curve data available for this device.
      </div>
    );
  }

  const interval = Math.max(1, Math.floor(data.power.length / 12));

  // Calculate voltage domain
  const allVoltages = data.voltage.flatMap((v) => [v.phaseA, v.phaseB, v.phaseC]);
  const vMin = Math.min(...allVoltages) - 50;
  const vMax = Math.max(...allVoltages) + 50;

  return (
    <div className="flex-1 overflow-y-auto px-6 py-4 space-y-8">
      {/* Power Curve */}
      <ChartSection title="Power Curve">
        <ChartContainer config={powerChartConfig} className="h-[260px] w-full">
          <ResponsiveContainer width="100%" height="100%">
            <AreaChart data={data.power} margin={{ top: 10, right: 10, left: -15, bottom: 0 }}>
              <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
              <XAxis dataKey="time" tickLine={false} axisLine={false} tickMargin={8} className="text-xs" interval={interval} />
              <YAxis tickLine={false} axisLine={false} tickMargin={8} className="text-xs" />
              <ChartTooltip content={<ChartTooltipContent indicator="line" />} />
              <Area type="monotone" dataKey="activePower" stroke="var(--color-energy-solar)" fill="var(--color-energy-solar)" fillOpacity={0.25} strokeWidth={2} />
              <Area type="monotone" dataKey="reactivePower" stroke="var(--color-energy-battery)" fill="var(--color-energy-battery)" fillOpacity={0.1} strokeWidth={1.5} />
              <Area type="monotone" dataKey="apparentPower" stroke="var(--color-energy-consumption)" fill="var(--color-energy-consumption)" fillOpacity={0.05} strokeWidth={1.5} />
            </AreaChart>
          </ResponsiveContainer>
        </ChartContainer>
        <ChartLegend items={[
          { color: "var(--color-energy-solar)", label: "Active Power (kW)" },
          { color: "var(--color-energy-battery)", label: "Reactive Power (kVar)" },
          { color: "var(--color-energy-consumption)", label: "Apparent Power (kVA)" },
        ]} />
      </ChartSection>

      {/* Voltage Curve (3-phase) */}
      <ChartSection title="Voltage Curve">
        <ChartContainer config={voltageChartConfig} className="h-[260px] w-full">
          <ResponsiveContainer width="100%" height="100%">
            <LineChart data={data.voltage} margin={{ top: 10, right: 10, left: -15, bottom: 0 }}>
              <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
              <XAxis dataKey="time" tickLine={false} axisLine={false} tickMargin={8} className="text-xs" interval={interval} />
              <YAxis domain={[vMin, vMax]} tickLine={false} axisLine={false} tickMargin={8} className="text-xs" />
              <ChartTooltip content={<ChartTooltipContent indicator="line" />} />
              <Line type="monotone" dataKey="phaseA" stroke="var(--color-energy-solar)" strokeWidth={1.5} dot={false} />
              <Line type="monotone" dataKey="phaseB" stroke="var(--color-energy-consumption)" strokeWidth={1.5} dot={false} />
              <Line type="monotone" dataKey="phaseC" stroke="var(--color-energy-battery)" strokeWidth={1.5} dot={false} />
            </LineChart>
          </ResponsiveContainer>
        </ChartContainer>
        <ChartLegend items={[
          { color: "var(--color-energy-solar)", label: "Phase A (V)" },
          { color: "var(--color-energy-consumption)", label: "Phase B (V)" },
          { color: "var(--color-energy-battery)", label: "Phase C (V)" },
        ]} />
      </ChartSection>

      {/* Frequency Curve */}
      <ChartSection title="Frequency Curve">
        <ChartContainer config={frequencyChartConfig} className="h-[260px] w-full">
          <ResponsiveContainer width="100%" height="100%">
            <LineChart data={data.frequency} margin={{ top: 10, right: 10, left: -15, bottom: 0 }}>
              <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
              <XAxis dataKey="time" tickLine={false} axisLine={false} tickMargin={8} className="text-xs" interval={interval} />
              <YAxis domain={[49.8, 50.2]} tickLine={false} axisLine={false} tickMargin={8} className="text-xs" />
              <ReferenceLine y={50.0} stroke="var(--color-energy-grid)" strokeDasharray="4 4" label={{ value: "50.0 Hz", position: "right", className: "text-xs fill-energy-grid" }} />
              <ChartTooltip content={<ChartTooltipContent indicator="line" />} />
              <Line type="monotone" dataKey="frequency" stroke="var(--color-energy-solar)" strokeWidth={2} dot={false} />
            </LineChart>
          </ResponsiveContainer>
        </ChartContainer>
        <ChartLegend items={[
          { color: "var(--color-energy-solar)", label: "Frequency (Hz)" },
        ]} />
      </ChartSection>

      {/* Power Factor Curve */}
      <ChartSection title="Power Factor Curve">
        <ChartContainer config={powerFactorChartConfig} className="h-[260px] w-full">
          <ResponsiveContainer width="100%" height="100%">
            <LineChart data={data.powerFactor} margin={{ top: 10, right: 10, left: -15, bottom: 0 }}>
              <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
              <XAxis dataKey="time" tickLine={false} axisLine={false} tickMargin={8} className="text-xs" interval={interval} />
              <YAxis domain={[0.85, 1.01]} tickLine={false} axisLine={false} tickMargin={8} className="text-xs" />
              <ReferenceLine y={0.95} stroke="var(--color-energy-grid)" strokeDasharray="4 4" label={{ value: "0.95", position: "right", className: "text-xs fill-energy-grid" }} />
              <ChartTooltip content={<ChartTooltipContent indicator="line" />} />
              <Line type="monotone" dataKey="powerFactor" stroke="var(--color-energy-solar)" strokeWidth={2} dot={false} />
            </LineChart>
          </ResponsiveContainer>
        </ChartContainer>
        <ChartLegend items={[
          { color: "var(--color-energy-solar)", label: "Power Factor" },
        ]} />
      </ChartSection>

      <div className="h-6" />
    </div>
  );
}
