"use client";

import { useMemo } from "react";
import { Activity, AlertTriangle, Clock } from "lucide-react";
import {
  Area,
  AreaChart,
  CartesianGrid,
  XAxis,
  YAxis,
  ReferenceLine,
  ResponsiveContainer,
} from "recharts";
import {
  ChartConfig,
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
} from "@/components/ui/chart";
import { AbbrTooltip } from "@/components/ui/abbr-tooltip";
import { Badge } from "@/components/ui/badge";
import { Progress } from "@/components/ui/progress";
import type { BessUnit } from "@/lib/bess/types";

interface BessHealthTabProps {
  unit: BessUnit;
}

function MetricItem({ label, value, unit, review }: { label: React.ReactNode; value: number | null | string; unit?: string; review?: boolean }) {
  const displayValue =
    typeof value === "string"
      ? value
      : value === null
        ? "--"
        : `${value.toLocaleString("en-US")} ${unit ?? ""}`;
  return (
    <div className="flex items-center justify-between py-2">
      <span className="text-sm text-muted-foreground inline-flex items-center gap-1.5">
        {review && <span className="inline-block w-2 h-2 rounded-full bg-yellow-400 shrink-0" />}
        {label}
      </span>
      <div className="flex items-center gap-2">
        <span className="text-sm font-medium">{displayValue}</span>
        {typeof value === "number" && <Activity className="h-3.5 w-3.5 text-muted-foreground/50" />}
      </div>
    </div>
  );
}

function MetricGrid({ children }: { children: React.ReactNode }) {
  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-x-8 gap-y-0">
      {children}
    </div>
  );
}

function SectionTitle({ title }: { title: string }) {
  return (
    <h3 className="text-sm font-semibold text-foreground mt-6 mb-2 first:mt-0">
      {title}
    </h3>
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

// Health traffic-light colors: green / amber / red
const healthGood = { bar: "[&>[data-slot=progress-indicator]]:bg-emerald-500", text: "text-emerald-500" };
const healthWarning = { bar: "[&>[data-slot=progress-indicator]]:bg-amber-500", text: "text-amber-500" };
const healthCritical = { bar: "[&>[data-slot=progress-indicator]]:bg-red-500", text: "text-red-500" };

// "Used" metrics: low=green, mid=amber, high=red
function getUsedColor(pct: number) {
  if (pct >= 70) return healthCritical;
  if (pct >= 40) return healthWarning;
  return healthGood;
}

// "Quality" metrics: high=green, mid=amber, low=red
function getEfficiencyColor(rte: number) {
  if (rte >= 92) return healthGood;
  if (rte >= 88) return healthWarning;
  return healthCritical;
}

function getSoHColor(soh: number) {
  if (soh >= 90) return healthGood;
  if (soh >= 80) return healthWarning;
  return healthCritical;
}

const sohChartConfig = {
  soh: {
    label: "SoH (%)",
    color: "var(--color-energy-battery)",
  },
} satisfies ChartConfig;

export function BessHealthTab({ unit }: BessHealthTabProps) {
  const { lifecycle, nameplate, live } = unit;

  const cycleLifeUsedPct = useMemo(
    () => parseFloat(((lifecycle.equivalentFullCycles / lifecycle.designedCycleLife) * 100).toFixed(1)),
    [lifecycle.equivalentFullCycles, lifecycle.designedCycleLife]
  );

  const calendarLifeUsedPct = useMemo(() => {
    const parts = nameplate.commissioningDate.split("/");
    const monthMap: Record<string, number> = {
      Jan: 0, Feb: 1, Mar: 2, Apr: 3, May: 4, Jun: 5,
      Jul: 6, Aug: 7, Sep: 8, Oct: 9, Nov: 10, Dec: 11,
    };
    const commDate = new Date(Number(parts[2]), monthMap[parts[1]], Number(parts[0]));
    const yearsElapsed = (Date.now() - commDate.getTime()) / (365.25 * 24 * 3600 * 1000);
    return parseFloat(((yearsElapsed / lifecycle.designedCalendarLifeYears) * 100).toFixed(1));
  }, [nameplate.commissioningDate, lifecycle.designedCalendarLifeYears]);

  const estimatedRUL = useMemo(() => {
    const cycleRUL = (1 - lifecycle.equivalentFullCycles / lifecycle.designedCycleLife) * lifecycle.designedCalendarLifeYears;
    const calRUL = lifecycle.designedCalendarLifeYears * (1 - calendarLifeUsedPct / 100);
    return Math.max(0, Math.min(cycleRUL, calRUL)).toFixed(1);
  }, [lifecycle, calendarLifeUsedPct]);

  const efficiencyDegradation = useMemo(
    () => (lifecycle.initialRoundTripEfficiency - lifecycle.roundTripEfficiency).toFixed(1),
    [lifecycle.initialRoundTripEfficiency, lifecycle.roundTripEfficiency]
  );

  const sohChartData = useMemo(() => {
    const months = ["Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", "Jan", "Feb"];
    return lifecycle.sohTrendMonthly.map((soh, i) => ({
      month: months[i],
      soh,
    }));
  }, [lifecycle.sohTrendMonthly]);

  const warrantyExpired = useMemo(() => {
    const parts = lifecycle.warrantyExpiryDate.split("/");
    const monthMap: Record<string, number> = {
      Jan: 0, Feb: 1, Mar: 2, Apr: 3, May: 4, Jun: 5,
      Jul: 6, Aug: 7, Sep: 8, Oct: 9, Nov: 10, Dec: 11,
    };
    const expDate = new Date(Number(parts[2]), monthMap[parts[1]], Number(parts[0]));
    return Date.now() > expDate.getTime();
  }, [lifecycle.warrantyExpiryDate]);

  return (
    <div className="flex-1 overflow-y-auto px-6 py-4">
      {/* Health Overview */}
      <SectionTitle title="Health Overview" />

      {/* SoH progress */}
      {(() => { const c = getSoHColor(live.soh); return (
      <div className="mb-3 space-y-2">
        <div className="flex items-center justify-between">
          <span className="text-sm text-muted-foreground">State of Health</span>
          <span className={`text-sm font-semibold ${c.text}`}>{live.soh}%</span>
        </div>
        <Progress value={live.soh} className={`h-3 ${c.bar}`} />
      </div>
      ); })()}

      {/* Cycle Life progress */}
      {(() => { const c = getUsedColor(cycleLifeUsedPct); return (
      <div className="mb-3 space-y-2">
        <div className="flex items-center justify-between">
          <span className="text-sm text-muted-foreground">Cycle Life Used</span>
          <span className="text-sm font-semibold">
            <span className={c.text}>{cycleLifeUsedPct}%</span>
            <span className="text-xs text-muted-foreground font-normal ml-1.5">
              ({lifecycle.equivalentFullCycles.toLocaleString()} / {lifecycle.designedCycleLife.toLocaleString()} EFC)
            </span>
          </span>
        </div>
        <Progress value={cycleLifeUsedPct} className={`h-3 ${c.bar}`} />
      </div>
      ); })()}

      {/* Calendar Life progress */}
      {(() => { const c = getUsedColor(calendarLifeUsedPct); return (
      <div className="mb-3 space-y-2">
        <div className="flex items-center justify-between">
          <span className="text-sm text-muted-foreground">Calendar Life Used</span>
          <span className="text-sm font-semibold">
            <span className={c.text}>{calendarLifeUsedPct}%</span>
            <span className="text-xs text-muted-foreground font-normal ml-1.5">
              of {lifecycle.designedCalendarLifeYears} year design life
            </span>
          </span>
        </div>
        <Progress value={calendarLifeUsedPct} className={`h-3 ${c.bar}`} />
      </div>
      ); })()}

      {/* Round-Trip Efficiency progress */}
      {(() => { const c = getEfficiencyColor(lifecycle.roundTripEfficiency); return (
      <div className="mb-3 space-y-2">
        <div className="flex items-center justify-between">
          <span className="text-sm text-muted-foreground">
            <AbbrTooltip label="Round-Trip Efficiency" title="Round-Trip Efficiency (RTE)" description="Ratio of energy discharged to energy charged. Decreases over time as internal resistance increases. Formula: (Discharged / Charged) × 100%" />
          </span>
          <span className="text-sm font-semibold">
            <span className={c.text}>{lifecycle.roundTripEfficiency}%</span>
            <span className="text-xs text-muted-foreground font-normal ml-1.5">
              Initial: {lifecycle.initialRoundTripEfficiency}% (-{efficiencyDegradation}%)
            </span>
          </span>
        </div>
        <Progress value={lifecycle.roundTripEfficiency} className={`h-3 ${c.bar}`} />
      </div>
      ); })()}

      {/* Estimated RUL */}
      <div className="border rounded-md p-4 bg-muted/30 mt-4">
        <div className="flex items-center justify-between">
          <span className="text-sm text-muted-foreground">Estimated Remaining Useful Life</span>
          <span className="text-sm font-semibold">{estimatedRUL} years</span>
        </div>
      </div>

      {/* SoH Degradation Trend */}
      <SectionTitle title="SoH Degradation Trend (12 Months)" />
      <ChartContainer config={sohChartConfig} className="h-[240px] w-full">
        <ResponsiveContainer width="100%" height="100%">
          <AreaChart data={sohChartData} margin={{ top: 10, right: 10, left: -15, bottom: 0 }}>
            <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
            <XAxis dataKey="month" tickLine={false} axisLine={false} tickMargin={8} className="text-xs" />
            <YAxis domain={["dataMin - 1", "dataMax + 1"]} tickLine={false} axisLine={false} tickMargin={8} className="text-xs" />
            <ChartTooltip content={<ChartTooltipContent indicator="line" />} />
            <ReferenceLine y={80} stroke="var(--color-energy-grid)" strokeDasharray="5 5" label={{ value: "80% EOL", position: "insideBottomLeft", fontSize: 10, fill: "var(--color-energy-grid)" }} />
            <Area
              type="monotone"
              dataKey="soh"
              stroke="var(--color-energy-battery)"
              fill="var(--color-energy-battery)"
              fillOpacity={0.15}
              strokeWidth={2}
            />
          </AreaChart>
        </ResponsiveContainer>
      </ChartContainer>
      <ChartLegend items={[
        { color: "var(--color-energy-battery)", label: "SoH (%)" },
      ]} />

      {/* Degradation Metrics */}
      <SectionTitle title="Degradation Metrics" />
      <MetricGrid>
        <MetricItem label="Equivalent full cycles" value={lifecycle.equivalentFullCycles} unit="" />
        <MetricItem label="Designed cycle life" value={lifecycle.designedCycleLife} unit="cycles" />
        <MetricItem label="Average depth of discharge" value={lifecycle.averageDoD} unit="%" review />
      </MetricGrid>
      <MetricGrid>
        <MetricItem label="Total cycle count" value={lifecycle.cycleCount} unit="" review />
        <MetricItem label="Current SoH" value={live.soh} unit="%" />
        <MetricItem label="Designed calendar life" value={lifecycle.designedCalendarLifeYears} unit="years" review />
      </MetricGrid>

      {/* Energy Throughput */}
      <SectionTitle title="Energy Throughput" />
      <MetricGrid>
        <MetricItem label="Lifetime charged" value={lifecycle.lifetimeChargedMWh} unit="MWh" />
        <MetricItem label="Lifetime discharged" value={lifecycle.lifetimeDischargedMWh} unit="MWh" />
        <MetricItem label={<AbbrTooltip label="RTE" title="Round-Trip Efficiency" description="Ratio of energy discharged to energy charged. Formula: (Discharged / Charged) × 100%" />} value={lifecycle.roundTripEfficiency} unit="%" />
      </MetricGrid>
      <MetricGrid>
        <MetricItem label="Initial RTE" value={lifecycle.initialRoundTripEfficiency} unit="%" review />
        <MetricItem label="Efficiency degradation" value={`-${efficiencyDegradation}%`} review />
        <MetricItem label="Daily charged" value={lifecycle.dailyChargedKWh} unit="kWh" review />
      </MetricGrid>
      <MetricGrid>
        <MetricItem label="Daily discharged" value={lifecycle.dailyDischargedKWh} unit="kWh" review />
      </MetricGrid>

      {/* Thermal Stress */}
      <SectionTitle title="Thermal Stress" />
      <MetricGrid>
        <MetricItem label="Max recorded temperature" value={lifecycle.maxRecordedTemp} unit="°C" />
        <MetricItem label="Avg operating temperature" value={lifecycle.avgOperatingTemp} unit="°C" />
      </MetricGrid>

      {/* Operational History */}
      <SectionTitle title="Operational History" />
      <MetricGrid>
        <MetricItem label="Total operating hours" value={lifecycle.totalOperatingHours} unit="h" review />
        <MetricItem label="Total standby hours" value={lifecycle.totalStandbyHours} unit="h" review />
        <MetricItem label="Firmware version" value={lifecycle.firmwareVersion} review />
      </MetricGrid>

      {/* Maintenance & Warranty */}
      <SectionTitle title="Maintenance & Warranty" />
      <div className="border rounded-md p-4 bg-muted/30">
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-x-8 gap-y-3">
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">Last maintenance</span>
            <div className="flex items-center gap-1.5">
              <Clock className="h-3.5 w-3.5 text-muted-foreground" />
              <span className="text-sm font-medium">{lifecycle.lastMaintenanceDate}</span>
            </div>
          </div>
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">Next maintenance due</span>
            <div className="flex items-center gap-1.5">
              <AlertTriangle className="h-3.5 w-3.5 text-energy-consumption" />
              <span className="text-sm font-medium">{lifecycle.nextMaintenanceDue}</span>
            </div>
          </div>
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">Warranty expiry</span>
            <div className="flex items-center gap-1.5">
              <span className="text-sm font-medium">{lifecycle.warrantyExpiryDate}</span>
              <Badge className={warrantyExpired ? "bg-energy-grid/15 text-energy-grid border-energy-grid/30" : "bg-energy-solar/15 text-energy-solar border-energy-solar/30"}>
                {warrantyExpired ? "Expired" : "Active"}
              </Badge>
            </div>
          </div>
        </div>
      </div>

      <div className="h-6" />
    </div>
  );
}
