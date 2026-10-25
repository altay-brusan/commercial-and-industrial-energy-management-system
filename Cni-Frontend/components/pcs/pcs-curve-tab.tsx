"use client";

import { useState, useEffect } from "react";
import { Loader2 } from "lucide-react";
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
import * as pcsService from "@/lib/pcs/pcs-service";
import type { ChartDefinitionDto } from "@/lib/pcs/api-types";

interface PcsCurveTabProps {
  unitId: string;
}

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

function DynamicChart({ chart }: { chart: ChartDefinitionDto }) {
  if (!chart.data || chart.data.length === 0) {
    return (
      <ChartSection title={chart.title}>
        <div className="h-[200px] flex items-center justify-center text-sm text-muted-foreground">
          No data available
        </div>
      </ChartSection>
    );
  }

  const xKey = chart.xAxis.key;
  const interval = Math.max(1, Math.floor(chart.data.length / 12));

  const config: ChartConfig = {};
  chart.series.forEach((s) => {
    config[s.key] = {
      label: `${s.label}${s.unit ? ` (${s.unit})` : ""}`,
      color: s.color ?? "var(--color-primary)",
    };
  });

  const leftSeries = chart.series.filter((s) => !s.yAxis || s.yAxis === "left");
  const rightSeries = chart.series.filter((s) => s.yAxis === "right");
  const hasDualAxis = rightSeries.length > 0;

  const isLine = chart.type === "line";
  const ChartComponent = isLine ? LineChart : AreaChart;

  return (
    <ChartSection title={chart.title}>
      <ChartContainer config={config} className="h-[260px] w-full">
        <ResponsiveContainer width="100%" height="100%">
          <ChartComponent data={chart.data} margin={{ top: 10, right: 10, left: -15, bottom: 0 }}>
            <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
            <XAxis dataKey={xKey} tickLine={false} axisLine={false} tickMargin={8} className="text-xs" interval={interval} />
            <YAxis yAxisId="left" tickLine={false} axisLine={false} tickMargin={8} className="text-xs" />
            {hasDualAxis && (
              <YAxis yAxisId="right" orientation="right" tickLine={false} axisLine={false} tickMargin={8} className="text-xs" />
            )}
            <ChartTooltip content={<ChartTooltipContent indicator="line" />} />
            {chart.series.map((s) => {
              const yAxisId = s.yAxis === "right" ? "right" : "left";
              const color = s.color ?? "var(--color-primary)";

              if (isLine || s.type === "line") {
                return (
                  <Line
                    key={s.key}
                    yAxisId={yAxisId}
                    type="monotone"
                    dataKey={s.key}
                    stroke={color}
                    strokeWidth={1.5}
                    dot={false}
                  />
                );
              }
              return (
                <Area
                  key={s.key}
                  yAxisId={yAxisId}
                  type="monotone"
                  dataKey={s.key}
                  stroke={color}
                  fill={color}
                  fillOpacity={0.15}
                  strokeWidth={2}
                />
              );
            })}
          </ChartComponent>
        </ResponsiveContainer>
      </ChartContainer>
      <ChartLegend
        items={chart.series.map((s) => ({
          color: s.color ?? "var(--color-primary)",
          label: `${s.label}${s.unit ? ` (${s.unit})` : ""}`,
        }))}
      />
    </ChartSection>
  );
}

export function PcsCurveTab({ unitId }: PcsCurveTabProps) {
  const [charts, setCharts] = useState<ChartDefinitionDto[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    setLoading(true);
    pcsService.getUnitCurves(unitId).then((data) => {
      setCharts(data.charts);
    }).catch(() => {
      // handled by apiClient
    }).finally(() => setLoading(false));
  }, [unitId]);

  if (loading) {
    return (
      <div className="flex-1 flex items-center justify-center">
        <Loader2 className="h-5 w-5 animate-spin text-muted-foreground" />
      </div>
    );
  }

  if (charts.length === 0) {
    return (
      <div className="flex-1 flex items-center justify-center text-muted-foreground py-20">
        No curve data available for this unit.
      </div>
    );
  }

  return (
    <div className="flex-1 overflow-y-auto px-6 py-4 space-y-8">
      {charts.map((chart) => (
        <DynamicChart key={chart.key} chart={chart} />
      ))}
      <div className="h-6" />
    </div>
  );
}
