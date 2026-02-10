"use client";

import { Activity, ArrowUpRight, ArrowDownLeft, Minus } from "lucide-react";
import { AbbrTooltip } from "@/components/ui/abbr-tooltip";
import { Badge } from "@/components/ui/badge";
import { Progress } from "@/components/ui/progress";
import type { BessUnit } from "@/lib/bess/types";

interface BessSummaryTabProps {
  unit: BessUnit;
}

function formatValue(value: number | null, unit: string): string {
  if (value === null) return "--";
  return `${value.toLocaleString("en-US")} ${unit}`;
}

function MetricItem({ label, value, unit }: { label: React.ReactNode; value: number | null | string; unit?: string }) {
  const displayValue = typeof value === "string" ? value : formatValue(value, unit ?? "");
  return (
    <div className="flex items-center justify-between py-2">
      <span className="text-sm text-muted-foreground">{label}</span>
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

function getSOCColor(soc: number): string {
  if (soc >= 60) return "[&>[data-slot=progress-indicator]]:bg-energy-solar";
  if (soc >= 30) return "[&>[data-slot=progress-indicator]]:bg-energy-consumption";
  return "[&>[data-slot=progress-indicator]]:bg-energy-grid";
}

export function BessSummaryTab({ unit }: BessSummaryTabProps) {
  const { nameplate, live, lifecycle, connectedDevices } = unit;

  return (
    <div className="flex-1 overflow-y-auto px-6 py-4">
      {/* Nameplate */}
      <SectionTitle title="Nameplate Information" />
      <div className="border rounded-md p-4 bg-muted/30">
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-x-8 gap-y-3">
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">Brand</span>
            <span className="text-sm font-medium">{nameplate.brand}</span>
          </div>
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">Model</span>
            <span className="text-sm font-medium">{nameplate.model}</span>
          </div>
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">Serial Number</span>
            <span className="text-sm font-medium font-mono">{nameplate.serialNumber}</span>
          </div>
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">Chemistry</span>
            <span className="text-sm font-medium">{nameplate.chemistry}</span>
          </div>
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">Commissioning Date</span>
            <span className="text-sm font-medium">{nameplate.commissioningDate}</span>
          </div>
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">Capacity</span>
            <span className="text-sm font-medium">{nameplate.capacityKWh} kWh</span>
          </div>
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">Max Power</span>
            <span className="text-sm font-medium">{nameplate.maxPowerKW} kW</span>
          </div>
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">Voltage Range</span>
            <span className="text-sm font-medium">{nameplate.voltageRange[0]}–{nameplate.voltageRange[1]} V</span>
          </div>
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">Hierarchy</span>
            <span className="text-sm font-medium">
              {nameplate.rackCount}R / {nameplate.packsPerRack}P / {nameplate.modulesPerPack}M / {nameplate.cellsPerModule}C
            </span>
          </div>
        </div>
      </div>

      {/* Live Status */}
      <SectionTitle title="Live Status" />

      {/* SoC progress bar */}
      <div className="mb-4 space-y-2">
        <div className="flex items-center justify-between">
          <span className="text-sm text-muted-foreground">State of Charge</span>
          <span className="text-sm font-semibold">{live.soc}%</span>
        </div>
        <Progress value={live.soc} className={`h-3 ${getSOCColor(live.soc)}`} />
      </div>

      <MetricGrid>
        <MetricItem label={<AbbrTooltip label="SoH" title="State of Health" description="Battery's current capacity compared to its original rated capacity" />} value={live.soh} unit="%" />
        <div className="flex items-center justify-between py-2">
          <span className="text-sm text-muted-foreground">Power</span>
          <div className="flex items-center gap-2">
            {live.power === 0 ? (
              <div className="flex items-center gap-1 text-sm font-medium text-muted-foreground">
                <Minus className="h-3.5 w-3.5" />
                0 kW
              </div>
            ) : live.power < 0 ? (
              <div className="flex items-center gap-1 text-sm font-medium text-energy-solar">
                <ArrowDownLeft className="h-3.5 w-3.5" />
                {Math.abs(live.power)} kW
              </div>
            ) : (
              <div className="flex items-center gap-1 text-sm font-medium text-energy-battery">
                <ArrowUpRight className="h-3.5 w-3.5" />
                {live.power} kW
              </div>
            )}
            <Activity className="h-3.5 w-3.5 text-muted-foreground/50" />
          </div>
        </div>
        <MetricItem label="Voltage" value={live.voltage} unit="V" />
      </MetricGrid>

      <MetricGrid>
        <MetricItem label="Current" value={live.current} unit="A" />
        <MetricItem label="Frequency" value={live.frequency} unit="Hz" />
        <MetricItem label="Temp (Avg)" value={live.tempAvg} unit="°C" />
      </MetricGrid>

      <MetricGrid>
        <MetricItem label="Temp (Min)" value={live.tempMin} unit="°C" />
        <MetricItem label="Temp (Max)" value={live.tempMax} unit="°C" />
        <div className="flex items-center justify-between py-2">
          <span className="text-sm text-muted-foreground">Operating Mode</span>
          <Badge variant="secondary" className="capitalize">{unit.operatingMode}</Badge>
        </div>
      </MetricGrid>

      {/* Lifecycle */}
      <SectionTitle title="Lifecycle" />
      <MetricGrid>
        <MetricItem label="Cycle Count" value={lifecycle.cycleCount} unit="" />
        <MetricItem label="Lifetime Throughput" value={lifecycle.lifetimeThroughputMWh} unit="MWh" />
        <MetricItem label="Daily Charged" value={lifecycle.dailyChargedKWh} unit="kWh" />
      </MetricGrid>
      <MetricGrid>
        <MetricItem label="Daily Discharged" value={lifecycle.dailyDischargedKWh} unit="kWh" />
      </MetricGrid>

      {/* Connected Devices */}
      <SectionTitle title="Connected Devices" />
      <div className="border rounded-md p-4 bg-muted/30">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-x-8 gap-y-3">
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">Inverter / PCS</span>
            <span className="text-sm font-medium">{connectedDevices.inverter}</span>
          </div>
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">Meter</span>
            <span className="text-sm font-medium">{connectedDevices.meter}</span>
          </div>
        </div>
      </div>

      <div className="h-6" />
    </div>
  );
}
