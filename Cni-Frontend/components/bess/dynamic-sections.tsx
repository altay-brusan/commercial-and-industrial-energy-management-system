"use client";

import {
  Info, Activity, Gauge, BarChart3, Shield, Thermometer, Cpu,
  HeartPulse, TrendingDown, Zap, Clock, Wrench,
} from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Progress } from "@/components/ui/progress";
import {
  Area, AreaChart, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, ReferenceLine,
} from "recharts";
import { cn } from "@/lib/utils";
import type {
  DynamicSectionDto,
  SectionFieldDto,
  StatusBadgeDto,
  TableDataDto,
  ChartConfigDto,
} from "@/lib/bess/api-types";

// --- Icon map ---

const iconMap: Record<string, React.ElementType> = {
  info: Info,
  activity: Activity,
  gauge: Gauge,
  "bar-chart-3": BarChart3,
  shield: Shield,
  thermometer: Thermometer,
  cpu: Cpu,
  "heart-pulse": HeartPulse,
  "trending-down": TrendingDown,
  zap: Zap,
  clock: Clock,
  wrench: Wrench,
};

// --- Color helpers ---

function badgeColorToClass(color: string): string {
  const map: Record<string, string> = {
    green: "bg-status-operational/15 text-status-operational border-status-operational/30",
    red: "bg-severity-critical/15 text-severity-critical border-severity-critical/30",
    orange: "bg-severity-high/15 text-severity-high border-severity-high/30",
    yellow: "bg-severity-medium/15 text-severity-medium border-severity-medium/30",
    purple: "bg-energy-consumption/15 text-energy-consumption border-energy-consumption/30",
    blue: "bg-energy-grid/15 text-energy-grid border-energy-grid/30",
    sky: "bg-sky-500/15 text-sky-600 border-sky-500/30",
    grey: "bg-muted text-muted-foreground",
    gray: "bg-muted text-muted-foreground",
  };
  return map[color] ?? "bg-muted text-muted-foreground";
}

function thresholdColor(value: number, thresholds: Array<{ min: number; color: string }>): string {
  const sorted = [...thresholds].sort((a, b) => b.min - a.min);
  for (const t of sorted) {
    if (value >= t.min) {
      const colorMap: Record<string, string> = {
        green: "[&>[data-slot=progress-indicator]]:bg-status-operational",
        orange: "[&>[data-slot=progress-indicator]]:bg-severity-high",
        red: "[&>[data-slot=progress-indicator]]:bg-severity-critical",
      };
      return colorMap[t.color] ?? "[&>[data-slot=progress-indicator]]:bg-primary";
    }
  }
  return "[&>[data-slot=progress-indicator]]:bg-primary";
}

// --- Field Renderers ---

function FieldValue({ field }: { field: SectionFieldDto }) {
  const { value, type, unit, format, meta } = field;

  // Null/undefined value
  if (value === null || value === undefined) {
    return <span className="text-sm text-muted-foreground">--</span>;
  }

  // Progress bar
  if (format === "progress" && typeof value === "number") {
    const thresholds = (meta?.thresholds as Array<{ min: number; color: string }>) ?? [];
    const colorClass = thresholds.length > 0
      ? thresholdColor(value, thresholds)
      : "[&>[data-slot=progress-indicator]]:bg-primary";
    const detail = meta?.detail as string | undefined;
    return (
      <div className="space-y-1.5 w-full">
        <div className="flex items-center justify-between">
          <span className="text-sm font-semibold">{value}{unit}</span>
          {detail && <span className="text-xs text-muted-foreground">{detail}</span>}
        </div>
        <Progress value={Math.min(value, 100)} className={cn("h-2.5", colorClass)} />
      </div>
    );
  }

  // Badge format (for operating mode, boolean meta badges, etc.)
  if (format === "badge") {
    if (type === "boolean") {
      const boolVal = value as boolean;
      const trueLabel = (meta?.trueLabel as string) ?? "On";
      const falseLabel = (meta?.falseLabel as string) ?? "Off";
      const trueColor = (meta?.trueColor as string) ?? "green";
      const falseColor = (meta?.falseColor as string) ?? "grey";
      return (
        <Badge className={cn("text-xs", badgeColorToClass(boolVal ? trueColor : falseColor))}>
          {boolVal ? trueLabel : falseLabel}
        </Badge>
      );
    }
    // String badge (e.g. operating mode)
    const badgeLabel = (meta?.label as string) ?? String(value);
    const badgeColor = (meta?.color as string) ?? (meta?.badgeColor as string) ?? "grey";
    return (
      <Badge className={cn("text-xs", badgeColorToClass(badgeColor))}>
        {badgeLabel}
      </Badge>
    );
  }

  // Temperature format
  if (format === "temperature" && typeof value === "number") {
    return <span className="text-sm font-medium">{value} {unit ?? "°C"}</span>;
  }

  // Number with unit
  if (type === "number" && typeof value === "number") {
    const location = meta?.location as { bmu: number; cell: number } | undefined;
    return (
      <div className="flex items-center gap-2">
        <span className="text-sm font-medium">
          {value.toLocaleString("en-US")} {unit ?? ""}
        </span>
        {location && (
          <span className="text-xs text-muted-foreground">
            BMU {location.bmu} / Cell {location.cell}
          </span>
        )}
      </div>
    );
  }

  // Boolean without badge format
  if (type === "boolean") {
    const boolVal = value as boolean;
    return (
      <Badge variant="outline" className={cn("text-xs", boolVal ? "text-status-operational" : "text-muted-foreground")}>
        {boolVal ? "Yes" : "No"}
      </Badge>
    );
  }

  // Default: string
  return <span className="text-sm font-medium">{String(value)}</span>;
}

function FieldRow({ field }: { field: SectionFieldDto }) {
  // Full-width progress fields
  if (field.format === "progress" && field.value !== null && field.value !== undefined) {
    return (
      <div className="py-2">
        <span className="text-sm text-muted-foreground mb-1 block">{field.label}</span>
        <FieldValue field={field} />
      </div>
    );
  }

  return (
    <div className="flex items-center justify-between py-2">
      <span className="text-sm text-muted-foreground">{field.label}</span>
      <FieldValue field={field} />
    </div>
  );
}

// --- Table Renderer ---

function SectionTable({ tableData }: { tableData: TableDataDto }) {
  return (
    <div className="border rounded-md overflow-hidden bg-muted/30">
      <table className="w-full text-sm">
        <thead>
          <tr className="border-b bg-muted/50">
            {tableData.columns.map((col) => (
              <th key={col.key} className="text-left px-4 py-2 text-xs font-medium text-muted-foreground">
                {col.label}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {tableData.rows.map((row, i) => (
            <tr key={i} className="border-b last:border-b-0">
              {tableData.columns.map((col) => {
                const cellValue = row[col.key];
                if (col.type === "badge" && cellValue && typeof cellValue === "object") {
                  const badge = cellValue as StatusBadgeDto;
                  return (
                    <td key={col.key} className="px-4 py-2">
                      <Badge className={cn("text-xs", badgeColorToClass(badge.color))}>
                        {badge.label}
                      </Badge>
                    </td>
                  );
                }
                return (
                  <td key={col.key} className="px-4 py-2 font-medium">
                    {cellValue !== null && cellValue !== undefined ? String(cellValue) : "--"}
                  </td>
                );
              })}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

// --- Chart Renderer ---

function SectionChart({ config, data }: { config: ChartConfigDto; data: Record<string, unknown>[] }) {
  if (!data || data.length === 0) return null;

  const xKey = config.xAxis?.key ?? "x";
  const yKey = config.yAxis?.key ?? "y";
  const color = config.color ?? "#8b5cf6";

  return (
    <div className="h-[200px] w-full">
      <ResponsiveContainer width="100%" height="100%">
        <AreaChart data={data} margin={{ top: 5, right: 10, left: 10, bottom: 5 }}>
          <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
          <XAxis dataKey={xKey} tick={{ fontSize: 11 }} className="text-muted-foreground" />
          <YAxis
            domain={config.yAxis?.domain ? [config.yAxis.domain[0], config.yAxis.domain[1]] : ["auto", "auto"]}
            tick={{ fontSize: 11 }}
            className="text-muted-foreground"
            label={config.yAxis?.unit ? { value: config.yAxis.unit, angle: -90, position: "insideLeft", style: { fontSize: 11 } } : undefined}
          />
          <Tooltip
            contentStyle={{ fontSize: 12 }}
            labelStyle={{ fontWeight: 600 }}
          />
          <Area
            type="monotone"
            dataKey={yKey}
            stroke={color}
            fill={color}
            fillOpacity={0.15}
            strokeWidth={2}
          />
          {config.referenceLines?.map((line, i) => (
            <ReferenceLine
              key={i}
              y={line.y}
              label={{ value: line.label, position: "right", style: { fontSize: 10, fill: line.color ?? "#ef4444" } }}
              stroke={line.color ?? "#ef4444"}
              strokeDasharray={line.strokeDasharray ?? "5 5"}
            />
          ))}
        </AreaChart>
      </ResponsiveContainer>
    </div>
  );
}

// --- Section Renderer ---

function DynamicSection({ section }: { section: DynamicSectionDto }) {
  // Skip optional sections with no meaningful data
  if (section.optional && section.fields.every((f) => f.value === null || f.value === undefined) && !section.tableData && !section.chartData?.length) {
    return null;
  }

  const Icon = section.icon ? iconMap[section.icon] : null;
  const cols = section.columns || 3;
  const gridClass = cols === 2
    ? "grid-cols-1 md:grid-cols-2"
    : "grid-cols-1 md:grid-cols-2 lg:grid-cols-3";

  const isHighlighted = section.variant === "highlighted";
  const isChart = section.variant === "chart";
  const isTable = section.variant === "table";

  return (
    <div className="mt-6 first:mt-0">
      {/* Section title */}
      <h3 className="text-sm font-semibold text-foreground mb-2 flex items-center gap-2">
        {Icon && <Icon className="h-4 w-4 text-muted-foreground" />}
        {section.title}
      </h3>

      {/* Highlighted variant */}
      {isHighlighted && section.fields.length > 0 && (
        <div className="border rounded-md p-4 bg-muted/30">
          <div className={cn("grid gap-x-8 gap-y-3", gridClass)}>
            {section.fields.map((field) => (
              <FieldRow key={field.key} field={field} />
            ))}
          </div>
        </div>
      )}

      {/* Card variant */}
      {!isHighlighted && !isChart && section.fields.length > 0 && (
        <div className={cn("grid gap-x-8 gap-y-0", gridClass)}>
          {section.fields.map((field) => (
            <FieldRow key={field.key} field={field} />
          ))}
        </div>
      )}

      {/* Table variant */}
      {isTable && section.tableData && (
        <div className="mt-2">
          <SectionTable tableData={section.tableData} />
        </div>
      )}

      {/* Chart variant */}
      {isChart && section.chartConfig && section.chartData && (
        <SectionChart config={section.chartConfig} data={section.chartData} />
      )}
    </div>
  );
}

// --- Main Component ---

interface DynamicSectionsProps {
  sections: DynamicSectionDto[];
  className?: string;
}

export function DynamicSections({ sections, className }: DynamicSectionsProps) {
  return (
    <div className={cn("space-y-2", className)}>
      {sections.map((section) => (
        <DynamicSection key={section.key} section={section} />
      ))}
    </div>
  );
}
