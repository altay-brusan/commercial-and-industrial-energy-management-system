"use client";

import { Zap, Thermometer } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { pcsStateData } from "@/lib/dashboard-2/mock-data";

export function PCSState() {
  const data = pcsStateData;
  const tempWarning = data.temperatureC > 55;

  return (
    <div className="rounded-lg bg-muted/30 p-5">
      {/* Header */}
      <div className="flex items-center justify-between mb-5">
        <div className="flex items-center gap-3">
          <div className="flex items-center justify-center w-9 h-9 rounded-lg bg-energy-solar/10">
            <Zap className="h-[18px] w-[18px] text-energy-solar" />
          </div>
          <div>
            <p className="text-sm font-medium">PCS</p>
            <p className="text-xs text-muted-foreground">Power Conversion</p>
          </div>
        </div>
        <Badge
          variant="outline"
          className="border-energy-solar/40 text-energy-solar text-[11px] px-2 py-0.5"
        >
          {data.mode}
        </Badge>
      </div>

      {/* Main Metric */}
      <div className="mb-4">
        <span className="text-3xl font-semibold tracking-tight">
          {data.loadKW}
        </span>
        <span className="text-sm font-normal text-muted-foreground ml-1">kW</span>
        <p className="text-xs text-muted-foreground mt-0.5">Current Load</p>
      </div>

      {/* Efficiency gauge */}
      {(() => {
        const r = 52;
        const cx = 70;
        const cy = 60;
        const arcLength = Math.PI * r;
        const offset = arcLength - (data.efficiencyPercent / 100) * arcLength;
        const arcPath = `M ${cx - r} ${cy} A ${r} ${r} 0 0 1 ${cx + r} ${cy}`;
        return (
          <div className="flex flex-col items-center mb-4">
            <svg viewBox="0 0 140 72" className="w-full max-w-[180px]">
              <path
                d={arcPath}
                fill="none"
                stroke="var(--color-muted)"
                strokeWidth="8"
                strokeLinecap="round"
              />
              <path
                d={arcPath}
                fill="none"
                stroke="var(--color-energy-solar)"
                strokeWidth="8"
                strokeLinecap="round"
                strokeDasharray={arcLength}
                strokeDashoffset={offset}
                className="transition-all duration-700"
              />
              <text x={cx} y={cy - 10} textAnchor="middle" className="fill-foreground text-xl font-semibold" style={{ fontSize: 20 }}>
                {data.efficiencyPercent}%
              </text>
              <text x={cx} y={cy + 4} textAnchor="middle" className="fill-muted-foreground" style={{ fontSize: 9 }}>
                Efficiency
              </text>
            </svg>
          </div>
        );
      })()}

      {/* Temperature + AC/DC grid */}
      <div className="grid grid-cols-3 gap-2">
        <div className="rounded-md bg-background/60 p-2.5 text-center">
          <Thermometer className={`h-3.5 w-3.5 mx-auto mb-1 ${tempWarning ? "text-energy-consumption" : "text-muted-foreground"}`} />
          <p className={`text-sm font-semibold ${tempWarning ? "text-energy-consumption" : ""}`}>
            {data.temperatureC}&deg;
          </p>
          <p className="text-[10px] text-muted-foreground mt-0.5">Temp</p>
        </div>
        <div className="rounded-md bg-background/60 p-2.5 text-center">
          <p className="text-[10px] text-muted-foreground font-medium uppercase tracking-wider mb-1">AC</p>
          <p className="text-sm font-semibold">{data.acVoltageV}V</p>
          <p className="text-[10px] text-muted-foreground mt-0.5">{data.acCurrentA}A</p>
        </div>
        <div className="rounded-md bg-background/60 p-2.5 text-center">
          <p className="text-[10px] text-muted-foreground font-medium uppercase tracking-wider mb-1">DC</p>
          <p className="text-sm font-semibold">{data.dcVoltageV}V</p>
          <p className="text-[10px] text-muted-foreground mt-0.5">{data.dcCurrentA}A</p>
        </div>
      </div>
    </div>
  );
}
